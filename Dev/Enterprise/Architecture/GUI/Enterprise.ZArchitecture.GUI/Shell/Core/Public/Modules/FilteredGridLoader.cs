using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ZArchitecture.GUI.Shell.Core.Public.Modules
{
	public class FilteredGridLoader : Disposable
	{
		public bool ThrowExceptionOnMaximumRowsLoaded { get; set; } = true;

		public bool HasSearched { get; private set; }
		public bool IsPerformingSearch { get; private set; }
		public int MaxRowsToLoad { get; set; } = SystemDataRegistry.Instance.MaxNumberOfRecordsToShowInDisplayGrids.Value;
		internal Form ParentForm { get; set; }
		bool HasHookedCollection { get; set; }

		protected readonly ResultCountMessage resultCountMessage;
		protected readonly bool enablePreviousNextSupport;
		protected readonly ModuleIdentifier id;
		protected readonly FilterStripBusinessObject filterBusinessObject;
		protected readonly Type typeOfElements;

		public readonly Func<BusinessObjectFactory> GetNewFactory;

		protected virtual bool PermitActiveCollectionUpdates => true;

		public FilteredGridLoader(FilterStripBusinessObject filterBusinessObject, ResultCountMessage handler, IModuleDecisionProvider provider, ModuleIdentifier moduleId, Func<BusinessObjectFactory> createFactory, Type typeOfElements)
			: this(filterBusinessObject, handler, provider.EnablePreviousNextSupport, moduleId, createFactory, typeOfElements)
		{
		}

		public FilteredGridLoader(FilterStripBusinessObject filterBusinessObject, ResultCountMessage handler, bool enablePreviousNextSupport, ModuleIdentifier moduleId, Func<BusinessObjectFactory> createFactory, Type typeOfElements)
		{
			this.filterBusinessObject = filterBusinessObject;
			this.resultCountMessage = handler;
			this.enablePreviousNextSupport = enablePreviousNextSupport;
			this.id = moduleId;
			this.typeOfElements = typeOfElements;

			this.GetNewFactory = Argument.NotNull(createFactory, nameof(createFactory));
		}

		public PerformSearchResult PerformSearch(BusinessObjectFactory factory, Type type, ZQuery query)
		{
			try
			{
				IsPerformingSearch = true;
				try
				{
					var bizosAndFactory = RunWithNoQueryHintFallback(factory, type, query);
					return PerformSearchResult.Success(bizosAndFactory.factory, query, bizosAndFactory.bizos, PermitActiveCollectionUpdates);
				}
				catch (MaxRowsLoadedException)
				{
					return PerformSearchResult.MaxRowsExceeded(query);
				}
				catch (LimitedRunException limitedRunException)
				{
					return PerformSearchResult.LimitedRunError(limitedRunException.Message);
				}
				catch (System.Data.Common.DbException ex) when (new SqlExceptionWrapper(ex).Number == 8623)
				{
					return PerformSearchResult.QueryTooComplicated();
				}
				catch (System.Data.Common.DbException ex) when (new SqlExceptionWrapper(ex).Number == 258)
				{
					ExceptionReporter.Instance.ReportException("", ex);
					return PerformSearchResult.SqlException285();
				}
				catch (System.Data.Common.DbException ex) when (new SqlExceptionWrapper(ex).Number == 8003)
				{
					return PerformSearchResult.TooManyParameters();
				}
				catch (System.Data.Common.DbException ex) when (new SqlExceptionWrapper(ex).Number == 8618)
				{
					return PerformSearchResult.MinimumRowSizeExceeds();
				}
			}
			finally
			{
				HasSearched = true;
				IsPerformingSearch = false;
			}
		}

		public bool PushItemsIntoCollection(IBusinessObjectCollection gridCollection, PerformSearchResult result, SortInfo sort)
		{
			using (gridCollection.SuspendListChanged())
			{
				if (result.Type == PerformSearchResultType.Success)
				{
					try
					{
						IsPerformingSearch = true; // We need this to support legacy uses of "IsPerformingSearch" that assume that search also populates the collection. e.g. APEnquiryModule.GridCollection_CountChanged.
						LoadItemsIntoCollection(result.Factory, result.Query, gridCollection, result.LoadedRows, result.PermitActiveCollectionUpdates);
					}
					finally
					{
						IsPerformingSearch = false;
					}

					if (!HasHookedCollection)
					{
						HookCollection(gridCollection);
						HasHookedCollection = true;
						UpdateModuleResultsCache(gridCollection);
					}
				}
				else
				{
					SwapInNewFactory(gridCollection);
				}
			}

			var rowCount = result.Type == PerformSearchResultType.MaxRowsExceeded ? MaxRowsToLoad + 1 : gridCollection.Count;
			if (rowCount > MaxRowsToLoad)
			{
				RemoveResultsFromGridCollection(gridCollection);
				if (EnvProxy.Instance.Registry.ShowExactRowCountOnExcessResult)
				{
					rowCount = DoGetExactRowCountOnExcessResult(gridCollection, result.Query);
				}
				if (gridCollection is IActiveBusinessObjectCollection activeCollection)
				{
					activeCollection.Relationship = new CollectionRelationshipThatHasAlreadyLoaded(gridCollection.TypeOfElements, result.Query, result.LoadedRows);
				}
			}
			else if (!gridCollection.IsSorted && sort != null)
			{
				gridCollection.ApplySort(sort);
			}

			resultCountMessage.UpdateResultCountMessage(rowCount, result.ErrorMessage);
			return rowCount <= MaxRowsToLoad;
		}

		(BusinessObject[] bizos, BusinessObjectFactory factory) RunWithNoQueryHintFallback(BusinessObjectFactory factory, Type type, ZQuery query)
		{
			try
			{
				return RunLimitedLoader(factory, type, query);
			}
			catch (System.Data.Common.DbException ex) when (
				new DbErrorMatch(ex).ExceptionType == DbErrorType.QueryProcessorCouldNotProduceQueryPlanBecauseOfHints ||
				new DbErrorMatch(ex).ExceptionType == DbErrorType.QueryProcessorCouldNotProduceQueryPlanBecauseMinimumWorktableWasTooLong
			)
			{
				if (Env.Security.RunExtremelyLongFindQuery.IsAllowed)
				{
					var message = Res.GetString("41354EB8-CEF0-4B36-8B29-B935B1D0F95D",
						@"The search criteria you have provided has created a query that may take a long time to run.
It is better to provide more precise criteria that may result in a smaller number of results.
Alternately it may also be better to provide simpler search criteria to decrease the work being performed.
Consider using starts with or exact criteria, or restricting the amount of data by using smaller date ranges.

Technical information : SQL Server was unable to generate an execution plan for this query in conjunction with the recommended SQL query hints indicated by the search criteria. The query can be retried without the recommended SQL query hints, but this could result in unexpected performance implications.

Are you sure you want to run it?");

					var caption = Res.GetString("FilterStripControl|RunQuery", "Run Query?");
					if (DialogResult.Yes == Globals.Message.Show(message, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Question, DialogResult.No, ParentForm))
					{
						var queryWithoutHints = query.DeepClone();
						queryWithoutHints.ClearTableIndexHintsIncludingSubQueries();
						queryWithoutHints.DisableForceSeekIncludingSubQueries();
						return RunLimitedLoader(factory, type, queryWithoutHints);
					}
					else
					{
						throw new LimitedRunException(Res.GetString("E02EF528-50A5-417C-81A4-9D0CFAA0FEAF",
							@"Query execution has been canceled to prevent unexpected performance degradation."));
					}
				}
				else
				{
					throw new LimitedRunException(
						Res.GetString("7094748B-4BA0-41F6-94EA-F36E7C765AC3",
							@"The search criteria you have provided has created a query that may take a long time to run.
It is better to provide more precise criteria that may result in a smaller number of results.
Alternately it may also be better to provide simpler search criteria to decrease the work being performed.
Consider using starts with or exact criteria, or restricting the amount of data by using smaller date ranges. To run this query you are required to have '{0}' security right.

Technical information : SQL Server was unable to generate an execution plan for this query in conjunction with the recommended SQL query hints indicated by the search criteria.",
							Env.Security.RunExtremelyLongFindQuery.DisplayTextPathToSecurityRight)
					);
				}
			}
		}

		(BusinessObject[], BusinessObjectFactory) RunLimitedLoader(BusinessObjectFactory factory, Type type, ZQuery query)
		{
			var limit = SystemDataRegistry.Instance.FindWindowQueryGovernorCosts.Value;
			try
			{
				using (GetQueryGovernorCostLimitDisposer(factory, limit.AllowedCost))
				{
					return (LoadCollection(factory, type, query), factory);
				}
			}
			catch (System.Data.Common.DbException sqlException1) when (new SqlExceptionWrapper(sqlException1).Number == 8649)
			{
				var caption = Res.GetString("FilterStripControl|RunQuery", "Run Query?");
				var limitValue = GetLimitFromException(sqlException1);

				if (!limitValue.HasValue && !Env.Security.RunExtremelyLongFindQuery.IsAllowed)
				{
					throw new LimitedRunException(
						Res.GetString("690fca6c-808a-4167-aee9-bd54a29ca7c9", "The estimated cost of your query can't be retrieved from SQL Server. You may not run it.")
					);
				}

				if (limitValue < limit.MaximalCost || Env.Security.RunExtremelyLongFindQuery.IsAllowed)
				{
					var message = limitValue.HasValue
						? Res.GetString("a1c03fa5-6bd2-468e-a2db-ef0c6bdc3789", @"The search criteria you have provided has created a query that may take a long time to run.
It is better to provide more precise criteria that may result in a smaller number of results.
Alternately it may also be better to provide simpler search criteria to decrease the work being performed.
Consider using starts with or exact criteria, or restricting the amount of data by using smaller date ranges.

Technical information : This query generated an execution plan with an estimated cost of {0} which is greater than the maximum of {1} set in the registry at System > Database > User Options > Estimated query cost limit.

Are you sure you want to run it?", limitValue.Value, limit.AllowedCost)
						: Res.GetString("fab511e5-7fb8-4795-88e5-a2e5ce6259e3", "The estimated cost of your query can't be retrieved from SQL Server. \r\nPlease review your filters and see if you can specify more accurately what you are looking for.\r\nAre you sure you want to run it?");

					if (DialogResult.Yes == Globals.Message.Show(message, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Question, DialogResult.No, ParentForm))
					{
						var newFactory = GetNewFactory();
						newFactory.NameForDebugging += " (new factory for query with high estimated cost)";
						return (LoadCollection(newFactory, type, query), newFactory);
					}
					else
					{
						throw new LimitedRunException(Res.GetString("0BAF1A87-2245-4FEA-AB8B-137F65261A4B", @"Query execution has been canceled due to high estimated cost."));
					}
				}
				else
				{
					throw new LimitedRunException(
						Res.GetString("010f2a78-2458-4875-9e7b-12998a0adde2", @"The search criteria you have provided has created a query that may take a long time to run.
It is better to provide more precise criteria that may result in a smaller number of results.
Alternately it may also be better to provide simpler search criteria to decrease the work being performed.
Consider using starts with or exact criteria, or restricting the amount of data by using smaller date ranges. To run this query you are required to have '{0}' security right.

Technical information : This query generated an execution plan with an estimated cost of {1} which is greater than the maximum of {2} set in the registry at System > Database > User Options > Estimated query cost limit.", Env.Security.RunExtremelyLongFindQuery.DisplayTextPathToSecurityRight, limitValue, limit.MaximalCost)
					);
				}
			}
		}

		static int? GetLimitFromException(System.Data.Common.DbException sqlException)
		{
			var msg = new ZString(sqlException?.Message);
			var pos1 = msg.IndexOf("(", StringComparison.OrdinalIgnoreCase);
			var pos2 = msg.IndexOf(")", Math.Max(0, pos1), StringComparison.OrdinalIgnoreCase);
			if (msg.IsEmpty || pos2 < ++pos1)
			{
				return null;
			}

			int value;
			if (int.TryParse(msg.SubstringSafe(pos1, pos2 - pos1), NumberStyles.Integer, CultureInfo.InvariantCulture, out value))
			{
				return value;
			}
			return null;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "direct sql for modifying query governor")]
		IDisposable GetQueryGovernorCostLimitDisposer(BusinessObjectFactory factory, int limit)
		{
			var disableQueryGovernorCostLimit = IsTemplateRecordsFilter();
			// WI00736132 due to some old beahviour in TemplateRecordSupportedFilterGridModule
			if (limit > 0 && !disableQueryGovernorCostLimit)
			{
				var connection = ((IDbConnected)factory).Connection;
				connection.ExecuteNonQuery(string.Format(CultureInfo.InvariantCulture, "SET QUERY_GOVERNOR_COST_LIMIT {0}", limit));
				return new DisposableAction(() => connection.ExecuteNonQuery("SET QUERY_GOVERNOR_COST_LIMIT 0"));
			}
			return null;
		}

		bool IsTemplateRecordsFilter()
		{
			var module = filterBusinessObject?.ParentModule as IZFilterGridModule;

			if (module == null || !module.AllowTemplateRecords)
			{
				return false;
			}

			var templateFiltersToDisableWarning = new List<string>
			{
				FilterStripBusinessObject.TemplateRecordsFilterCodes.TemplatesOnly,
				FilterStripBusinessObject.TemplateRecordsFilterCodes.TemplatesIncluded
			};

			var templateOptionSelected = filterBusinessObject.ActiveModuleFilters
				.OfType<ModuleTextFilter>()
				.Any(textFilter => textFilter.IsActive &&
								   textFilter.Description == FilterStripBusinessObject.TemplateRecordsDescription &&
								   templateFiltersToDisableWarning.Contains(textFilter.Property));

			return templateOptionSelected;
		}

		protected internal virtual int DoGetExactRowCountOnExcessResult(IBusinessObjectCollection collection, ZQuery query)
		{
			query = query.DeepClone();
			query.ModificationsEnabled = true;
			query.MaximumRows = null;

			return GetEstimatedLoadCount(collection, query);
		}

		protected virtual void RemoveResultsFromGridCollection(IBusinessObjectCollection gridCollection)
		{
			if (gridCollection is BusinessObjectCollection legacyCollection)
			{
				legacyCollection.RemoveAll();
			}

			if (gridCollection is IActiveBusinessObjectCollection activeGridCollection)
			{
				activeGridCollection.AdditionalFilter = ZQuery.NoResultQuery;
			}
		}

		public virtual int GetEstimatedLoadCount(IBusinessObjectCollection gridCollection, ZQuery query)
		{
			var result = -1;
			var legacyGridCollection = gridCollection as BusinessObjectCollection;
			if (legacyGridCollection != null)
			{
				result = legacyGridCollection.GetEstimatedLoadCount(query);
			}

			var activeCollection = gridCollection as IActiveBusinessObjectCollection;
			if (activeCollection != null)
			{
				var completeFilter = new ZQuery();
				completeFilter.AddToFilter(query);
				completeFilter.AddToFilter(activeCollection.Relationship.RelationshipFilter);
				result = gridCollection.Factory.GetDatabaseCount(gridCollection.TypeOfElements, completeFilter);
			}
			return result;
		}

		protected internal void SwapInNewFactory(IBusinessObjectCollection gridCollection)
		{
			if (originalFactory == null)
			{
				originalFactory = gridCollection.Factory;
			}

			var factory = GetNewFactory();
			SwapFactory(gridCollection, factory);
			OnFactorySwapped(originalFactory, factory);
		}

		void LoadItemsIntoCollection(BusinessObjectFactory factory, ZQuery query, IBusinessObjectCollection collection, BusinessObject[] items, bool permitActiveCollectionUpdates)
		{
			if (originalFactory == null)
			{
				originalFactory = collection.Factory;
			}
			PropertyDescriptor sortProperty = null;
			var sortDirection = ListSortDirection.Ascending;
			if (collection.IsSorted && collection.SortProperty is ZCustomPropertyDescriptor)
			{
				sortProperty = collection.SortProperty;
				sortDirection = collection.SortDirection;
				collection.RemoveSort();
			}

			if (collection is BusinessObjectCollection legacyCollection)
			{
				SwapLegacyCollectionFactory(factory, legacyCollection);
				legacyCollection.SetLoadResult(query, items);
			}

			if (collection is IActiveBusinessObjectCollection activeCollection)
			{
				query.MaximumRows = null;
				using (activeCollection.SuspendListChanged())
				{
					// Some modules filter the results of the query in memory.
					// If this is this case then the ABOC filter should not be searching for additional items
					if (permitActiveCollectionUpdates)
					{
						if (activeCollection.Relationship is ManyToManyRelationship)
						{
							activeCollection.AdditionalFilter = ZQuery.NoResultQuery;
							activeCollection.SetFactory(factory);
							activeCollection.AdditionalFilter = query;
						}
						else
						{
							activeCollection.AdditionalFilter = ZQuery.NoResultQuery;
							activeCollection.SetFactory(factory);
							activeCollection.Relationship = new CollectionRelationshipThatHasAlreadyLoaded(activeCollection.TypeOfElements, query, items);
							activeCollection.AdditionalFilter = new ZQuery(); // Ensure not null
						}
					}
					else
					{
						activeCollection.Relationship = new AdhocCollectionRelationship(collection.TypeOfElements);
						activeCollection.AdditionalFilter = new ZQuery(); // Ensure that this isn't bound to a NoResultQuery
						activeCollection.AddRange(items);
						activeCollection.SetFactory(factory);
					}
				}
			}

			OnFactorySwapped(originalFactory, factory);

			if (sortProperty != null)
			{
				collection.ApplySort(sortProperty, sortDirection);
			}
		}

		class CollectionRelationshipThatHasAlreadyLoaded : CollectionRelationship
		{
			public CollectionRelationshipThatHasAlreadyLoaded(Type elementType, ZQuery filter, BusinessObject[] items)
				: base(elementType, filter)
			{
				this.items = items.ToList();
				matchingPKs = new HashSet<ZGuid>(items.Select(x => x.PK).Distinct());
			}
			readonly List<BusinessObject> items;
			readonly HashSet<ZGuid> matchingPKs;

			protected override BusinessObject[] LoadBusinessObjectsCore(BusinessObjectFactory factory, ZQuery filter)
			{
				return Array.Empty<BusinessObject>();
			}

			protected override void AddToRelationship(BusinessObject businessObject)
			{
				if (matchingPKs.Add(businessObject.PK))
				{
					items.Add(businessObject);
				}
			}

			protected override IEnumerator GetDataEnumeratorCore(ZQuery additionalFilter)
			{
				foreach (INeedRow item in items.Where(i => i.MatchesFilter(additionalFilter)))
				{
					yield return item.Row;
				}
			}
		}

		BusinessObjectFactory originalFactory;

		protected virtual void OnFactorySwapped(BusinessObjectFactory oldFactory, BusinessObjectFactory newFactory)
		{
		}

		void SwapFactory(IBusinessObjectCollection collection, BusinessObjectFactory factory)
		{
			if (collection is BusinessObjectCollection legacyCollection)
			{
				SwapLegacyCollectionFactory(factory, legacyCollection);
			}

			if (collection is IActiveBusinessObjectCollection activeCollection)
			{
				activeCollection.AdditionalFilter = ZQuery.NoResultQuery;
				activeCollection.SetFactory(factory);
			}
		}

		static void SwapLegacyCollectionFactory(BusinessObjectFactory factory, BusinessObjectCollection legacyCollection)
		{
			var filteredCollection = legacyCollection as IBusinessObjectCollectionView;
			var originalLegacyCollection = filteredCollection != null ? filteredCollection.CollectionToFilter : legacyCollection;

			if (!originalLegacyCollection.IsNonPersistent)
			{
				originalLegacyCollection.IsManagedForDataRefresh = false;
			}

			originalLegacyCollection.SwapFactoryAndRemoveAll(factory);

			if (!originalLegacyCollection.IsNonPersistent)
			{
				originalLegacyCollection.IsManagedForDataRefresh = true;
			}
		}

		void HookCollection(IBusinessObjectCollection gridCollection)
		{
			gridCollection.ListChanged -= GridCollection_ListChanged;
			gridCollection.ListChanged += GridCollection_ListChanged;

			if (hookedCollection != null)
			{
				hookedCollection.ListChanged -= GridCollection_ListChanged;
			}

			hookedCollection = gridCollection;
		}
		IBusinessObjectCollection hookedCollection;

		void GridCollection_ListChanged(object sender, ListChangedEventArgs e)
		{
			var gridCollection = sender as IBusinessObjectCollection;
			if (e.ListChangedType != ListChangedType.ItemChanged)
			{
				resultCountMessage.UpdateResultCountMessage(gridCollection.Count);
				UpdateModuleResultsCache(gridCollection);
			}
		}

		protected override void Dispose(bool isDisposing)
		{
			pkCollection = null;

			var gridCollection = hookedCollection;
			if (gridCollection != null)
			{
				gridCollection.ListChanged -= GridCollection_ListChanged;
				if (originalFactory != null)
				{
					gridCollection.DecrementReadOnlyIncludingChildren(true);
					SwapFactory(gridCollection, originalFactory);

					var legacyGridCollection = gridCollection as BusinessObjectCollection;
					if (legacyGridCollection != null && legacyGridCollection.IsManagedForDataRefresh)
					{
						legacyGridCollection.IsManagedForDataRefresh = false;
					}
				}
			}
		}

		public void UpdateModuleResultsCache(IBusinessObjectCollection gridCollection)
		{
			if (enablePreviousNextSupport)
			{
				if (pkCollection == null)
				{
					pkCollection = ZModuleResults.Instance.GetPKCollectionForModule(id); // we need this reference to stop GC from collecting this while the module is still around
				}

				var pKs = gridCollection
					.Cast<BusinessObject>()
					.Select(bizO => (bizO as IPKDataProvider)?.GetPKData() ?? new PKData() { PK = bizO.PK });

				pkCollection.Rebuild(pKs.ToList());
			}
		}
		ZPKCollection pkCollection;

		protected BusinessObject[] LoadCollection(BusinessObjectFactory factory, Type type, ZQuery query)
		{
			var result = LoadCollectionCore(factory, type, query);
			if (ThrowExceptionOnMaximumRowsLoaded)
			{
				CheckMaximumRowsLoaded(result, query);
			}
			return result;
		}

		void CheckMaximumRowsLoaded(BusinessObject[] rows, ZQuery query)
		{
			if (rows.Length >= MaxRowsToLoad && rows.Length >= query.MaximumRows)
			{
				throw new MaxRowsLoadedException();
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Error Reporting")]
		protected virtual BusinessObject[] LoadCollectionCore(BusinessObjectFactory factory, Type type, ZQuery query)
		{
			try
			{
				return factory.Load(type, query);
			}
			catch (System.Data.Common.DbException ex) when (ex.Message.StartsWith("Invalid column name", StringComparison.OrdinalIgnoreCase))
			{
				ReportInvalidColumnNameException(query, ex);
				throw;
			}
		}

		void ReportInvalidColumnNameException(ZQuery query, System.Data.Common.DbException ex)
		{
			#region SuppressResourceStringsCheckRegion

			var filterReport = new ZStringBuilder();
			filterReport.AppendFormat("{0}: {1}", id.Name, ex.Message).AppendLine();
			filterReport.AppendLine("Generated Filter:");
			filterReport.AppendLine(query.LiteralTextSqlFormatted);
			filterReport.AppendLine("Filter Parts: ");
			var filterParts = "Empty, check the issue manager which key is [Exception when generate the filters parts.].";
			try
			{
				filterParts = GetFilterPartsReadable(query);
			}
			catch (Exception e)
			{
				ErrorReporter.ReportDeveloperExceptionOnce("Exception when generate the filters parts.", e);
			}
			filterReport.AppendLine(filterParts);

			if (filterBusinessObject != null)
			{
				var filters = filterBusinessObject.ActiveModuleFilters
					.Select(f => string.Format(CultureInfo.InvariantCulture, "Filter: {0} ({1}): {2}", f.Description, f.GetType().Name, f.Query.LiteralTextSqlFormatted))
					.ToArray();
				filterReport.AppendFormat("ActiveModuleFilters count: {0}", filters.Length.ToString(CultureInfo.InvariantCulture)).AppendLine();
				filterReport.Append(string.Join(System.Environment.NewLine, filters));
			}

			ErrorReporter.ReportOnce("FilterBusinessObject|" + ex.Message, filterReport.ToString());

			#endregion
		}

		string GetFilterPartsReadable(ZQuery query)
		{
			var stringBuilder = new ZStringBuilder();
			var filterParts = ZQuery.RecursiveGetFilterParts(query);

			foreach (var (index, filterPart) in filterParts)
			{
				if (filterPart == null)
				{
					stringBuilder.AppendLine($"{index} - null");
				}
				else
				{
					stringBuilder.AppendLine($"{index} - {filterPart.GetType().FullName} - {filterPart.LiteralTextADO}");
				}
			}

			return stringBuilder.ToString();
		}

		public virtual BusinessObjectReader CreateReader(ZQuery query)
			=> new BusinessObjectListReader(query, typeOfElements, GetNewFactory());
	}  
}
