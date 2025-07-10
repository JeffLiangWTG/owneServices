using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Schema;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using CargoWiseOne.ResourceStrings.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Internal;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using GlowIndexQueryService.Business;
using NUnit.Framework;
using static Enterprise.ZArchitecture.Business.FilterOrCategory;
using Res = Enterprise.ZArchitecture.GUI.Res;

namespace Enterprise.ZArchitecture.Modules.Testing
{
	[TestsSubclassesOf(typeof(FilterStripBusinessObject), typeof(TestExcludeBusinessObjectsAllHaveTestCasesAttribute), ExcludeClientDlls = true)]
	public abstract class FilterStripBusinessObjectTestCase :  BusinessObjectBaseTestCase
	{
		public void TestFilterStripIsClonable()
		{
			var original = GetNewFilterStripBusinessObject();
			var clone = original.Clone();

			AssertEquals(original.QueryObjectType, clone.QueryObjectType);
			AssertEquals("The clone should be of the same type as the original FilterStripBusinessObject", original.GetType(), clone.GetType());
		}

		protected sealed override BusinessObject GetNewBusinessObject()
		{
			return GetNewFilterStripBusinessObject();
		}

		protected abstract FilterStripBusinessObject GetNewFilterStripBusinessObject();

		[ExpectNoExceptions]
		public void TestQueryObjectTypeIsNotNullAndValid()
		{
			var filterStrip = GetNewFilterStripBusinessObject();
			var sqlFilter = filterStrip.ModuleFilters[FilterStripBusinessObject.CustomSqlFilterDescription] as ModuleSQLFilter;
			if (sqlFilter != null)
			{
				AssertNotNull(sqlFilter.QueryObjectType);

				if (sqlFilter.QueryObjectType.IsInterface)
				{
					var methodInfo = typeof(ObjectFactory).GetMethod("Get");
					var genericMethod = methodInfo.MakeGenericMethod(sqlFilter.QueryObjectType);
					var resultObj = genericMethod.Invoke(null, null);
					AssertNotNull(resultObj);
				}
			}
		}

		public void TestGetExpectedBusinessObjectTypeReturnsFilterBusinessObjectType()
		{
			Assert(GetExpectedBusinessObjectType().IsSubclassOf(typeof(FilterStripBusinessObject)));
		}

		protected override bool CanTestMaxLength(int maximumLength)
		{
			return maximumLength > -1 && base.CanTestMaxLength(maximumLength);
		}

		public void TestLoadLayoutMultipleTimesReturnsConstantResult()
		{
			var filterBizO = GetNewFilterStripBusinessObject();
			var emptyLayout = new DataGridLayoutManager().SavePreconfiguredLayout(filterBizO, "emptyLayout", false, false, SaveColumnLayout.Ignore);

			var moduleFilters = filterBizO.ModuleFilters;
			var strips = filterBizO.FilterStrips;

			foreach (var filter in moduleFilters)
			{
				if (moduleFilters.GetVisibleModuleFilter(filter.Description, true) != null)
				{
					var strip = strips.AddNew();
					strip.FilterDescription = filter.Description;
				}
			}

			var filtersNumber = filterBizO.FilterStrips.Count;
			var layout = new DataGridLayoutManager().SavePreconfiguredLayout(filterBizO, "testLayout", false, false, SaveColumnLayout.Ignore);

			filterBizO.LoadLayout(layout);
			AssertEquals(filtersNumber, filterBizO.FilterStrips.Count);

			foreach (var filter in filterBizO.ModuleFilters)
			{
				if (moduleFilters.GetVisibleModuleFilter(filter.Description, true) != null)
				{
					Assert(filter.IsActive);
					filter.IsActive = false;
				}
			}

			filterBizO.LoadLayout(layout);
			AssertEquals(filtersNumber, filterBizO.FilterStrips.Count);

			foreach (var filter in filterBizO.ModuleFilters)
			{
				if (moduleFilters.GetVisibleModuleFilter(filter.Description, true) != null)
				{
					Assert(filter.IsActive);
				}
			}

			filterBizO.LoadLayout(emptyLayout);
			var emptyAmount = filterBizO.FilterStrips.Count;

			foreach (var strip in filterBizO.FilterStrips.OfType<FilterStrip>())
			{
				AssertEquals(FilterVisibility.AlwaysVisible, strip.CurrentModuleFilter.Visibility);
			}

			filterBizO.LoadLayout(layout);
			AssertEquals(filtersNumber, filterBizO.FilterStrips.Count);

			foreach (var filter in filterBizO.ModuleFilters)
			{
				if (moduleFilters.GetVisibleModuleFilter(filter.Description, true) != null)
				{
					Assert(filter.IsActive);
				}
			}

			filterBizO.LoadLayout(emptyLayout);
			AssertEquals(emptyAmount, filterBizO.FilterStrips.Count);

			foreach (var strip in filterBizO.FilterStrips.OfType<FilterStrip>())
			{
				AssertEquals(FilterVisibility.AlwaysVisible, strip.CurrentModuleFilter.Visibility);
			}

			filterBizO.LoadLayout(layout, shouldReset: false);
			AssertEquals(filtersNumber, filterBizO.FilterStrips.Count);

			foreach (var filter in filterBizO.ModuleFilters)
			{
				if (moduleFilters.GetVisibleModuleFilter(filter.Description, true) != null)
				{
					Assert(filter.IsActive);
				}
			}

			filterBizO.LoadLayout(emptyLayout, shouldReset: false);
			AssertEquals(emptyAmount, filterBizO.FilterStrips.Count);

			foreach (var strip in filterBizO.FilterStrips.OfType<FilterStrip>())
			{
				AssertEquals(FilterVisibility.AlwaysVisible, strip.CurrentModuleFilter.Visibility);
			}
		}

		public void TestAllFiltersUseMultilingualDescriptions()
		{
			if (ShouldBeLocalizable)
			{
				var readOnlyResourceStrings = ResourceStringsResearch.Instance.GetSystemDefinedResourceStrings(Res.DefaultLanguage);
				var filterStrip = GetNewFilterStripBusinessObject();
				CombineAssertions(delegate
				{
					foreach (var filter in filterStrip.ModuleFilters)
					{
						if (filter.Visibility != FilterVisibility.AlwaysAppliedAndHidden)
						{
							Assert(string.Format("Filter '{0}' should have a valid MultilingualDescription", filter.Description), filter.MultilingualDescription != null && !filter.MultilingualDescription.IsEmpty);
							var resString = filter.MultilingualDescription as ResourceString;
							if (resString != null)
							{
								AssertNotNull(string.Format("Resource string with key'{0}' on filter '{1}' should be analyzed", resString.ResourceKey, filter.Description), readOnlyResourceStrings.Get(resString.ResourceKey));
							}
						}
					}
				});
			}
			AssertEquals("should be no error report", "", ErrorReporter.LastMessageReported);
			AssertEquals("should have no committed db updates", 0, DbCommitTracker.CommittedUpdates.Count());
		}

		public void TestAllFlagsFilterFlagNamesAreTranslatable()
		{
			if (ShouldBeLocalizable)
			{
				using (var mockData = Res.UseMockData())
				{
					mockData.SetResourceGetter(new ResourceStringGetter(key =>
					{
						if (key.StartsWith("DateTimeFormat|"))
						{
							return new ResourceStringData(key, string.Empty);
						}
						else
						{
							return new ResourceStringData(key, "~");
						}
					}));
					var readOnlyResourceStrings = ResourceStringsResearch.Instance.GetSystemDefinedResourceStrings(Res.DefaultLanguage);
					var filterStrip = GetNewFilterStripBusinessObject();
					CombineAssertions(delegate
					{
						foreach (var filter in filterStrip.ModuleFilters)
						{
							if (filter.Visibility != FilterVisibility.AlwaysAppliedAndHidden)
							{
								var flagFilter = filter as ModuleFlagsFilter;
								if (flagFilter != null && flagFilter.GetType().Name != "OrgTypeModuleFilter")
								{
									foreach (var flagName in flagFilter.FlagNames)
									{
										Assert(string.Format("FlagName '{0}' in filter '{1}' should be translatable, use Res.GetString()", flagName, filter.MultilingualDescription.GetUnresolvedString()), string.IsNullOrWhiteSpace(flagName.Replace("~", "")));
									}
								}
							}
						}
					});
				}
			}
			Assert(true);
		}

		protected virtual bool ShouldBeLocalizable
		{
			get { return !IsCountrySpecific && !IsClientSpecific; }
		}

		bool IsCountrySpecific
		{
			get { return this.GetType().Assembly.GetName().Name.StartsWith("Enterprise.Customs."); }
		}

		bool IsClientSpecific
		{
			get { return this.GetType().Assembly.GetName().Name.StartsWith("ZClient"); }
		}

		public void TestFilterDescriptionsAreNotLocalized()
		{
			const string localizedPostfix = " (KEY)";
			using (var mockRes = Res.UseMockData())
			{
				mockRes.SetResourceGetter(new ResourceStringGetter(key =>
				{
					ResourceStringData result = null;
					if (!key.StartsWith("DateTimeFormat"))
					{
						if (key.Length > ModuleFilterWithSubDescriptions.MaxFilterSubDescriptionLength - localizedPostfix.Length)
						{
							key = key.Substring(0, ModuleFilterWithSubDescriptions.MaxFilterSubDescriptionLength - localizedPostfix.Length);
						}
						result = new ResourceStringData(key, key + localizedPostfix);
					}
					return result;
				}));
				var stripsWithMultilingualDescription = GetNewFilterStripBusinessObject()
					.Select(filter => filter.Description)
					.Where(description => description.EndsWith(localizedPostfix))
					.ToList();

				Assert(@"A Filter Description should not be localized as it is used as the key for persistence, MultilingualDescription is used for display. " +
					"\r\nThe following Res strings must be removed: '" + string.Join(", ", stripsWithMultilingualDescription) + "'", !stripsWithMultilingualDescription.Any());
			}
			Assert(true);
		}

		public void TestFiltersMatchOperator_GetQuery_ShouldNotThrowExceptions()
		{
			AssertFiltersMatchOperator_ShouldGenerateValidQueries();
		}

		protected virtual void AssertFiltersMatchOperator_ShouldGenerateValidQueries()
		{
			var filterBizo = GetNewFilterStripBusinessObject();

			var filters = filterBizo.ModuleFilters.OfType<IModuleFilterWithSelectedFilters>().Where(x => x.AllowedComparisonOperators.Contains(ModuleTextFilter.ComparisonConstants.FiltersMatch));

			CombineAssertions(() =>
			{
				foreach (var filter in filters)
				{
					var failureInfo = string.Format(CultureInfo.InvariantCulture, "Filter: [{0}]. Schema Column: [{1}]. FilterBusinessObject type: [{2}]", filter.Description, (filter.FilterColumn?.Name ?? "None Specified"), filterBizo.GetType().Name);

					filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.FiltersMatch;
					ZQuery query = null;
					var moduleSpecifiedFilter = filter as ModuleGuidModuleSpecifiedFilter;

					if (moduleSpecifiedFilter != null)
					{
						moduleSpecifiedFilter.SetModuleId_ForTest(ModuleIDs.JobShipment); // Otherwise the query will be empty.
						AssertEquals("The text entered should have selected a valid module option. You may need to override SetModuleId_ForTestCore on your filter. SAD!", ModuleIDs.JobShipment, moduleSpecifiedFilter.ModuleId);
					}

					var failureMessage = string.Format(CultureInfo.InvariantCulture, "When generating the query for this filter, the 'filters match' option has caused an exception for the {0}.", failureInfo);
					AssertNoExceptionThrown(failureMessage, () => query = filter.Query);

					var queryText = query.LiteralTextADOFormatted;
					AssertEquals(string.Format(CultureInfo.InvariantCulture, "The query should not be empty, and yet... {0}.{1}{2}", failureInfo, System.Environment.NewLine, queryText), false, query.IsEmpty);
					AssertContains("The query should contain the keyword IN because there should always be a subquery, even with no selected filters. And yet..." + queryText, "IN", queryText);
				}
			});

			Assert(true); // for FilterStripBusinessObjects with no filters match-compatible filters
		}

		protected virtual string AdditionalQueryForFiltersMatchTest => string.Empty;

		public void TestFindLayout_QueryPerformance()
		{
			var filterBizo = GetNewFilterStripBusinessObject();
			AssertFilterTypeClauseWasIncluded(() => filterBizo.FindLayout("Squanch"));
			AssertFilterTypeClauseWasIncluded(() => filterBizo.FindLayout("Squanch", true));
			AssertFilterTypeClauseWasIncluded(() => filterBizo.FindLayout("Squanch", true, EnvProxy.Instance.CurrentCompany.PK));

			void AssertFilterTypeClauseWasIncluded(Action findLayoutAction)
			{
				using (Db.Connection.TrackExecutedCommands(includeStackTrace: true))
				{
					findLayoutAction.Invoke();
					var matchingCommands = Db.Connection.ExecutedCommands.Where(x => x.Contains("FROM dbo.StmModuleFilter") && x.Contains("S9_ModuleID =")).ToArray();

					foreach (var command in matchingCommands)
					{
						AssertContains("It's important to add the filter type clause so that the correct index is used. SAD!", "S9_FilterType <> 'FRU'", command);
					}
				}
			}
		}

		#region TestModuleFiltersUseSubGroupWhenNecessary

		[DeveloperOnlyTest]
		public void TestModuleFiltersUseSubGroupWhenNecessary()
		{
			var msgBuilder = new ZStringBuilder();
			var filterStripBizO = GetNewFilterStripBusinessObject();

			foreach (var moduleFilter in filterStripBizO.ModuleFilters.ToArray())
			{
				if (moduleFilter.FilterColumn != null && moduleFilter.FilterColumn.IsNonPersistent)
				{
					continue;
				}

				var filterStrips = new FilterStripCollection(filterStripBizO.ModuleFilters);

				try
				{
					AddActiveFilter(moduleFilter, filterStrips);
					var excludedTableNames = FiltersExcludedFromSubgroupCheck.Where(x => x.Item2 == moduleFilter.Description).Select(y => y.Item1).ToArray();

					var counts = GetTablesAndViewsCount(filterStripBizO.Filter.LiteralTextADO, excludedTableNames);
					if (counts.Any())
					{
						AddActiveFilter(moduleFilter, filterStrips);
						var msg = AssertUsedTablesAndViewsCountsDoNotIncrease(moduleFilter.Description, counts, GetTablesAndViewsCount(filterStripBizO.Filter.LiteralTextADO, excludedTableNames));
						msgBuilder.Append(msg);
					}
				}
				catch (TargetInvocationException ex)
				{
					HandlerException(moduleFilter.Description, ex, msgBuilder);
					continue;
				}
				catch (InvalidOperationException ex)
				{
					if (!ex.Message.StartsWith("Cannot use non-persistent column", StringComparison.InvariantCulture))
					{
						HandlerException(moduleFilter.Description, ex, msgBuilder);
					}
					continue;
				}
				finally
				{
					filterStrips.RemoveAll();
					filterStripBizO.ActiveModuleFilters.ToArray().ForEach(x => x.IsActive = false);
				}
			}

			if (!msgBuilder.IsEmpty)
			{
				msgBuilder.AppendLine();
				msgBuilder.AppendLine("Use SubGroup on reported Module Filters to implement joining sub query for related tables. For more details search 'ModuleFilter SubGroup' on WiseStackOverflow.");
			}

			Assert("THIS TEST DOES NOT YET FAIL ON DAT.\r\n" + msgBuilder.ToString(), msgBuilder.IsEmpty);
		}

		[DeveloperOnlyTest]
		public void TestModuleFiltersUseSubGroupWhenFilterByCommonTables()
		{
			var filterStripBizO = GetNewFilterStripBusinessObject();
			var msgBuilder = new ZStringBuilder();
			var countsPerFilter = new Dictionary<string, Dictionary<int, int>>();
			var filtersPerTableOrView = new Dictionary<int, List<string>>();

			foreach (var moduleFilter in filterStripBizO.ModuleFilters.ToArray())
			{
				if (moduleFilter.FilterColumn != null && moduleFilter.FilterColumn.IsNonPersistent)
				{
					continue;
				}

				var filterStrips = new FilterStripCollection(filterStripBizO.ModuleFilters);
				try
				{
					AddActiveFilter(filterStripBizO, moduleFilter.Description, filterStrips);

					var counts = GetTablesAndViewsCount(filterStripBizO.Filter.LiteralTextADO);
					if (counts.Any())
					{
						countsPerFilter.Add(moduleFilter.Description, counts);
						UpdateFiltersPerTableOrView(filtersPerTableOrView, moduleFilter.Description, counts);
					}
				}
				catch (TargetInvocationException ex)
				{
					HandlerException(moduleFilter.Description, ex, msgBuilder);
					continue;
				}
				catch (InvalidOperationException ex)
				{
					if (!ex.Message.StartsWith("Cannot use non-persistent column", StringComparison.InvariantCulture))
					{
						HandlerException(moduleFilter.Description, ex, msgBuilder);
					}
					continue;
				}
				finally
				{
					filterStrips.RemoveAll();
					filterStripBizO.ActiveModuleFilters.ToArray().ForEach(x => x.IsActive = false);
				}
			}

			foreach (var item in filtersPerTableOrView.Where(x => x.Value.Count > 1))
			{
				filterStripBizO = GetNewFilterStripBusinessObject();
				var filterStrips = new FilterStripCollection(filterStripBizO.ModuleFilters);
				Dictionary<int, int> countsToCompare = null;
				var appliedFilterDescriptions = new List<string>();

				try
				{
					var excludedFilters = FiltersExcludedFromSubgroupCheckForCommonTables.Where(x => x.Item1 == TablesAndViews[item.Key]).Select(y => y.Item2);

					foreach (var moduleFilterDescription in item.Value.Where(x => !excludedFilters.Contains(x)))
					{
						AddActiveFilter(filterStripBizO, moduleFilterDescription, filterStrips);
						try
						{
							var counts = GetTablesAndViewsCount(filterStripBizO.Filter.LiteralTextADO);

							if (countsToCompare != null)
							{
								var msg = AssertUsedTableOrViewCountDoesNotIncrease(item.Key, appliedFilterDescriptions, moduleFilterDescription, countsToCompare, counts);
								if (!string.IsNullOrEmpty(msg))
								{
									msgBuilder.AppendLine(msg);
								}
							}

							countsToCompare = counts;
							appliedFilterDescriptions.Add(moduleFilterDescription);
						}
						catch (TargetInvocationException ex)
						{
							HandlerException(moduleFilterDescription, ex, msgBuilder);
							break;
						}
					}
				}
				finally
				{
					filterStrips.RemoveAll();
					filterStripBizO.ActiveModuleFilters.ToArray().ForEach(x => x.IsActive = false);
				}
			}

			if (!msgBuilder.IsEmpty)
			{
				msgBuilder.AppendLine();
				msgBuilder.AppendLine("Use SubGroup on reported Module Filters to implement joining sub query for related tables. For more details search 'ModuleFilter SubGroup' on WiseStackOverflow.");
				msgBuilder.AppendLine("If Module Filter has a unique way of joining some related tables which cannot be shared via SubGroup, it could be excluded from this unit test by overriding GetFiltersExcludedFromSubgroupCheckForCommonTables method.");
			}

			Assert("THIS TEST DOES NOT YET FAIL ON DAT.\r\n" + msgBuilder.ToString(), msgBuilder.IsEmpty);
		}

		void HandlerException(ZString modulefilterDescription, Exception ex, ZStringBuilder msgBuilder)
		{
			var exception = ex.InnerException ?? ex;
			msgBuilder.AppendLine($"Getting '{modulefilterDescription}' filter caused {exception.GetType()} with the call stack:\r\n{exception.StackTrace}");
		}

		List<Tuple<string, string>> FiltersExcludedFromSubgroupCheckForCommonTables => filtersExcludedFromSubgroupCheckForCommonTables ?? (filtersExcludedFromSubgroupCheckForCommonTables = GetFiltersExcludedFromSubgroupCheckForCommonTables());
		List<Tuple<string, string>> filtersExcludedFromSubgroupCheckForCommonTables;

		/// <summary>
		///	override this method to specify filters that have unique way to join some related table/view and should be excluded from the TestModuleFiltersUseSubGroupWhenFilterByCommonTables for this table.
		///	The table will be also excluded from the TestModuleFiltersUseSubGroupWhenNesessary comparing table usage count for the same filter joined with itself via OR Group.
		///	Add 'table_name', 'filter_description' pair using TableFilter helper function. See example in the comments of this virtual method.
		/// </summary>
		protected virtual List<Tuple<string, string>> GetFiltersExcludedFromSubgroupCheckForCommonTables()
		{
			var result = new List<Tuple<string, string>>();

			// --- Example of adding pair to exclude from the unit test.
			// result.Add(TableFilter("table_name", "filter_description"));
			// --- Both parameters are case sensitive

			return result;
		}

		List<Tuple<string, string>> FiltersExcludedFromSubgroupCheck => filtersExcludedFromSubgroupCheck ?? (filtersExcludedFromSubgroupCheck = GetFiltersExcludedFromSubgroupCheck());
		List<Tuple<string, string>> filtersExcludedFromSubgroupCheck;

		protected virtual List<Tuple<string, string>> GetFiltersExcludedFromSubgroupCheck()
		{
			var result = new List<Tuple<string, string>>();
			return result;
		}

		protected Tuple<string, string> TableFilter(string table, string filter)
		{
			return new Tuple<string, string>(table, filter);
		}

		void AddActiveFilter(ModuleFilter filter, FilterStripCollection filterStrips, FilterOrCategory orCategory = FilterOrCategory.None)
		{
			if (filter.Visibility == FilterVisibility.AlwaysAppliedAndHidden)
			{
				filter.Visibility = FilterVisibility.Visible;
			}

			var filterStrip = filterStrips.AddNew(filter.Description);
			filterStrip.OrCategory = orCategory;

			var addedFilter = filterStrip.CurrentModuleFilter;
			AssertNotNull($"CurrentModuleFilter is null for '{filter.Description}'", addedFilter);
			addedFilter.FillWithValidTestFilterValue();
			Assert($"Filter '{filter.Description}' must be active after FillWithValidTestData call. The {filter.GetType()} should have an override of the FillWithValidTestFilterValue method to fill filter properties with some values to make it active.", addedFilter.IsActive);
		}

		void AddActiveFilter(FilterStripBusinessObject filterStripBO, string filterDescription, FilterStripCollection filterStrips)
		{
			var filter = filterStripBO.ModuleFilters[filterDescription];
			AssertNotNull($"Filter '{filterDescription}' must exist on the {filterStripBO.GetType()}", filter);

			AddActiveFilter(filter, filterStrips);
		}

		string AssertUsedTablesAndViewsCountsDoNotIncrease(string filterDescription, Dictionary<int, int> previousCounts, Dictionary<int, int> newCounts)
		{
			var msgBuilder = new ZStringBuilder();

			foreach (var index in previousCounts.Keys)
			{
				if (newCounts.ContainsKey(index) && newCounts[index] > previousCounts[index])
				{
					msgBuilder.AppendLine($"Table/View '{TablesAndViews[index]}' was referred {previousCounts[index]} times for the '{filterDescription}' filter but {newCounts[index]} times when it was combined with itself by OR group. To exclude: exclusions.Add(TableFilter({TablesAndViews[index]}Schema.Constants.TableName, \"{filterDescription}\"));");
				}
			}

			return msgBuilder.ToString();
		}

		string AssertUsedTableOrViewCountDoesNotIncrease(int tableOrViewIndex, IEnumerable<string> previousFilterDescriptions, string newFilterDescription, Dictionary<int, int> previousCounts, Dictionary<int, int> newCounts)
		{
			var msg = ZString.Empty;

			if (newCounts.ContainsKey(tableOrViewIndex) && previousCounts.ContainsKey(tableOrViewIndex) &&
				newCounts[tableOrViewIndex] > previousCounts[tableOrViewIndex])
			{
				var descriptionBuilder = new ZStringBuilder();
				foreach (var filterDescription in previousFilterDescriptions)
				{
					descriptionBuilder.Append(filterDescription);
				}

				msg = $"Table/View '{TablesAndViews[tableOrViewIndex]}' was referred {newCounts[tableOrViewIndex] - previousCounts[tableOrViewIndex]} more time(s) when '{newFilterDescription}' filter was combined with other filters using the same Table/View: " + descriptionBuilder.ToStringWithDelimiterBetweenAppends(", ") +
							$".  To exclude: exclusions.Add(TableFilter({TablesAndViews[tableOrViewIndex]}Schema.Constants.TableName, \"{newFilterDescription}\"));";
			}

			return msg;
		}

		Dictionary<int, int> GetTablesAndViewsCount(string sql, string[] excludedTableNames = null)
		{
			var result = new Dictionary<int, int>();

			for (var i = 0; i < TablesAndViews.Length; i++)
			{
				if (excludedTableNames != null && excludedTableNames.Contains(TablesAndViews[i]))
				{
					continue;
				}

				var pattern = string.Format(@"(?<=\W|^){0}(?=\W|$)", TablesAndViews[i]);
				var matchCount = Regex.Matches(sql, pattern, RegexOptions.CultureInvariant | RegexOptions.IgnoreCase).Count;

				if (matchCount > 0)
				{
					result.Add(i, matchCount);
				}
			}

			return result;
		}

		void UpdateFiltersPerTableOrView(Dictionary<int, List<string>> filtersPerTableOrView, string moduleFilterDescription, Dictionary<int, int> counts)
		{
			foreach (var count in counts)
			{
				if (filtersPerTableOrView.ContainsKey(count.Key))
				{
					filtersPerTableOrView[count.Key].Add(moduleFilterDescription);
				}
				else
				{
					filtersPerTableOrView.Add(count.Key, new List<string>(new string[] { moduleFilterDescription }));
				}
			}
		}

		string[] TablesAndViews
		{
			get { return tablesAndViews ?? (tablesAndViews = GetTablesAndViews()); }
		}
		string[] tablesAndViews;

		string[] GetTablesAndViews()
		{
			var result = new List<string>();
			var sql = @"
select name
from sys.objects as o
where 1=1
	 and o.is_ms_shipped = 0
	 and o.schema_id = SCHEMA_ID(N'dbo')
	 and o.type in ('U', 'V')
order by name";

			using (var reader = Db.Connection.Command(sql).ExecuteReader())
			{
				while (reader.Read())
				{
					result.Add(reader[0].ToString());
				}
			}
			return result.ToArray();
		}

		#endregion

		public void TestMatchesFilter()
		{
			var filterBizO = GetNewFilterStripBusinessObject();
			Type type = null;
			BusinessObject bizO = null, bizO2 = null;
			try
			{
				if (filterBizO.QueryObjectType != null)
				{
					type = filterBizO.QueryObjectType;
				}
				else
				{
					List<string> tableNames = filterBizO.Where(x => x.SubGroup == null).Select(x => x.FilterColumn?.TableName ?? "").Where(x => !string.IsNullOrEmpty(x)).ToList();
					if (tableNames.Any())
					{
						var mostCommonTableName = (from i in tableNames
												   group i by i into grp
												   orderby grp.Count() descending
												   select grp.Key).First();
						var tablePrefix = ObjectFactory.Get<IApplicationSchemaResolver>().GetColumnNamePrefix(mostCommonTableName);
						type = BusinessObjectFactory.GetBusinessObjectBaseTypeFromTablePrefix(tablePrefix);
					}
					else
					{
						List<string> tablePrefixes = filterBizO.Where(x => (x.Query == null || !x.Query.IsDataViewOptimisable) && x.SubGroup == null).Select(x => x.Query.LiteralTextSqlFormatted != null && x.Query.LiteralTextSqlFormatted.Contains("_") ? x.Query.LiteralTextSqlFormatted.Split('_')[0] : null).Where(x => !string.IsNullOrEmpty(x)).ToList();
						if (tablePrefixes.Any())
						{
							var mostCommonTablePrefix = (from i in tablePrefixes
														 group i by i into grp
														 orderby grp.Count() descending
														 select grp.Key).First();
							type = BusinessObjectFactory.GetBusinessObjectBaseTypeFromTablePrefix(mostCommonTablePrefix);
						}
					}
				}

				if (type != null)
				{
					bizO = filterBizO.Factory.New(type);
					bizO2 = filterBizO.Factory.NewWithValidTestData(type);
				}
			}
			catch (Exception)
			{
				Assert(true); //can't make bizo for this, oh well
				ErrorReporter.Clear();
				return;
			}
			if (bizO == null || bizO2 == null)
			{
				Assert(true); //can't make bizo for this, oh well
				ErrorReporter.Clear();
				return;
			}
			ErrorReporter.Clear(); //might have been a developer notification while constructing object (Job constructor)

			CombineAssertions(() =>
			{
				foreach (var filter in filterBizO.Where(x => x.Query != null && x.Query.IsDataViewOptimisable && x.SubGroup == null))
				{
					AssertNoExceptionThrown(filter.Code, () => bizO.MatchesFilter(filter.Query));
					AssertNoExceptionThrown(filter.Code, () => bizO2.MatchesFilter(filter.Query));
				}

				foreach (var filter in filterBizO.Select(SetupFilterForMatchesFilterTest).Where(x => x.Query != null && x.Query.IsDataViewOptimisable && x.SubGroup == null))
				{
					AssertNoExceptionThrown(filter.Code, () => bizO.MatchesFilter(filter.Query));
					AssertNoExceptionThrown(filter.Code, () => bizO2.MatchesFilter(filter.Query));
				}
			});
			Assert("In case if no applicable filters are found", true);
		}

		[DeveloperOnlyTest]
		public void TestMaxLengthFilterParameterIsValid()
		{
			CombineAssertions(() =>
			{
				foreach (var filter in GetNewFilterStripBusinessObject().Select(SetupFilterForTest).Where(f => f != null))
				{
					var maxColumn = GetColumnOfMaxLengthFromDelegate(filter);
					var tmp = filter.Query;
					if (!string.IsNullOrEmpty(ErrorReporter.LastMessageReported))
					{
						ErrorReporter.ReportOnce(GetMaxLengthHelperMessage(filter, maxColumn));
					}
				}
			});
			Assert("In case if no applicable filters are found", true);
		}

		string GetMaxLengthHelperMessage(ModuleFilter filter, SchemaColumn maxColumn)
		{
			var filterName = GetReadableFilterName(filter);
			return $@"{filterName}.MaxLength == {filter.MaxLength} is larger than the length of the corresponding DB field.
Possible cause: '{filterName}.MaxLength' is not set.
{(maxColumn != null ? $@"Proposed solution:
<code>{filter.GetType().Name}.MaxLength = {maxColumn.TableSchema.GetType().Name}.{maxColumn.Name}.MaxLength</code>" : "")}";
		}

		string GetReadableFilterName(ModuleFilter filter)
		{
			return $"{filter.GetType().Name}{{ Description = \"{filter.Description}\" }}";
		}

		protected virtual ModuleFilter SetupFilterForMatchesFilterTest(ModuleFilter moduleFilter)
		{
			var testString1 = "1";
			var testString2 = "2";

			switch (moduleFilter)
			{
				case ModulePeriodFilter filter:
					filter.Property = testString1;
					break;
				case ModuleGuidFilter filter:
					filter.Property = ZGuid.Invalid;
					break;
				case ModuleGuidsFilter filter:
					filter.Property1 = ZGuid.Invalid;
					break;
				case ModuleDateFilter filter:
					filter.Property1 = ZDateTime.Now;
					break;
				case ModuleFlagsFilter filter:
					filter.Property0 = true;
					break;
				case ModuleNumberRangeFilter filter:
					filter.PropertySearch = ModuleNumberRangeFilter.SearchTexts.Between;
					filter.Property1 = 1;
					filter.Property2 = 2;
					break;
				case ModuleFilterWithListAndComparisonOperators<ZString> filter:
					filter.Property = testString1;
					filter.ComparisonOperator = ModuleTextBaseFilter.ComparisonConstants.StartsWith;
					break;
				case ModuleTextRangeFilter filter:
					filter.Property1 = testString1;
					filter.Property2 = testString2;
					break;
				case ModuleCodeFilter filter:
					filter.Property1 = testString1;
					filter.Property2 = testString2;
					break;
				default:
					moduleFilter.FillWithValidTestData();
					break;
			}
			if (moduleFilter.GetType().ToString() == "Enterprise.Rating.GUI.ConversionFactorFilter")
			{
				((ModuleTextFilter)moduleFilter).Property = "10 KG/LB";
			}
			else if (moduleFilter.GetType().ToString() == "Enterprise.Messaging.Module.EDIInterchangeEHubIdFilter")
			{
				((ModuleTextFilter)moduleFilter).Property = Guid.NewGuid().ToString();
			}
			else if (moduleFilter.Code == "Transport Mode" || moduleFilter.Code == "Attribute Transport Mode") //for ZZRefCusCodeListFSBO
			{
				((ModuleTextFilter)moduleFilter).Property = "AIR";
			}
			return moduleFilter;
		}

		protected virtual ModuleFilter SetupFilterForTest(ModuleFilter moduleFilter)
		{
			const int SaneStringLength = int.MaxValue;
			string CreateTestParameterString(char charToRepeat)
			{
				return new string(charToRepeat, moduleFilter.MaxLength);
			}
			var testString1 = new Lazy<string>(() => CreateTestParameterString('1'));
			var testString2 = new Lazy<string>(() => CreateTestParameterString('2'));

			switch (moduleFilter)
			{
				case ModulePeriodFilter _:
				case ModuleGuidFilter _:
				case ModuleGuidsFilter _:
				case ModuleDateFilter _:
				case ModuleFlagsFilter _:
				case ModuleNumberRangeFilter _:
					moduleFilter = null; // Do not test these filters
					break;
				case ModuleFilter filter when filter.MaxLength <= 0 || filter.MaxLength >= SaneStringLength:
					Fail($"{GetReadableFilterName(filter)}.MaxLength == {filter.MaxLength}");
					moduleFilter = null;
					break;
				case ModuleFilterWithListAndComparisonOperators<ZString> filter:
					filter.Property = testString1.Value;
					filter.ComparisonOperator = ModuleTextBaseFilter.ComparisonConstants.StartsWith;
					break;
				case ModuleTextRangeFilter filter:
					filter.Property1 = testString1.Value;
					filter.Property2 = testString2.Value;
					break;
				case ModuleCodeFilter filter:
					filter.Property1 = testString1.Value;
					filter.Property2 = testString2.Value;
					break;
				default:
					moduleFilter.FillWithValidTestData();
					break;
			}
			return moduleFilter;
		}

		protected void AssertSensitiveFiltersWithSecurityRights(SecurityCheckpoint securityCheckpoint, string filterDescription)
		{
			securityCheckpoint.IsAllowed = true;
			var filter1 = GetNewFilterStripBusinessObject();
			AssertNotNull(filterDescription, filter1[filterDescription]);

			securityCheckpoint.IsAllowed = false;
			var filter2 = GetNewFilterStripBusinessObject();
			AssertNull(filterDescription, filter2[filterDescription]);
		}

		protected List<string> AllLanguages
		{
			get
			{
				if (allLanguages == null)
				{
					allLanguages = LanguageHelper.GetDefaultLanguageForOLookUpEditType().Keys.ToList();
				}
				return allLanguages;
			}
		}

		List<string> allLanguages;

		protected override void SetUp()
		{
			base.SetUp();
			((IWorkflowFilterStripsHelper)ObjectFactory.Get("IWorkflowFilterStripsHelper")).ClearCache();
		}

		static SchemaColumn GetColumnOfMaxLengthFromDelegate(ModuleFilter filter)
		{
			SchemaColumn result = null;
			try
			{
				switch (filter.QueryDelegate)
				{
					case Func<SQLComparisonOperator, ZString, ZQuery> queryDelegate:
						result = GetMaxLengthForParameter(queryDelegate);
						break;
					case Func<ZString, ZQuery> queryDelegate:
						result = GetMaxLengthForParameter(queryDelegate);
						break;
					case GetNkQuery queryDelegate:
						result = GetMaxLengthForParameter(new Func<ZString, ZQuery>(queryDelegate));
						break;
					case GetTextQueryWithOperator queryDelegate:
						result = GetMaxLengthForParameter(new Func<SQLComparisonOperator, ZString, ZQuery>(queryDelegate));
						break;
					case GetTextRangeQuery queryDelegate:
						result = GetMaxLengthForParameter(new Func<ZString, ZString, ZQuery>(queryDelegate));
						break;
					case GetTextAndNkQuery queryDelegate:
						result = GetMaxLengthForParameter(new Func<SQLComparisonOperator, ZString, ZString, ZQuery>(queryDelegate));
						break;
				}
			}
			catch (NotSupportedException)
			{
			}
			return result;
		}

		static SchemaColumn GetMaxLengthForParameter(Func<ZString, ZString, ZQuery> func, string value1 = "0", string value2 = "0")
		{
			var tag1 = new ZString(value1);
			var tag2 = new ZString(value2);
			var query = func(tag1, tag2);
			return new[]
			{
				FindLargestSchemaColumn(query, value1),
				FindLargestSchemaColumn(query, value2)
			}.MaxBy(c => c?.MaxLength);
		}

		static SchemaColumn GetMaxLengthForParameter(Func<ZString, ZQuery> func, string value = "0")
		{
			var tag = new ZString(value);
			return FindLargestSchemaColumn(func(tag), value);
		}

		static SchemaColumn GetMaxLengthForParameter(Func<SQLComparisonOperator, ZString, ZQuery> func, string value = "0")
		{
			return GetMaxLengthForParameter(arg => func(SQLComparisonOperator.Equal, arg), value);
		}

		static SchemaColumn GetMaxLengthForParameter(Func<SQLComparisonOperator, ZString, ZString, ZQuery> func, string value1 = "0", string value2 = "0")
		{
			return GetMaxLengthForParameter((arg1, arg2) => func(SQLComparisonOperator.Equal, arg1, arg2), value1, value2);
		}

		static SchemaColumn FindLargestSchemaColumn(ZQuery query, object value)
		{
			return GetAllParameters(query)
				.Where(p => object.ReferenceEquals(p.Value, value))
				.MaxBySafe(p => p.SchemaColumn.MaxLength)?.SchemaColumn;
		}

		static IEnumerable<ZSqlParameter> GetAllParameters(IFilterPart filterPart)
		{
			switch (filterPart)
			{
				case ZSqlParameter param:
					return new[] { param };
				case IFilterPartsProvider provider:
					return provider.FilterParts.SelectMany(GetAllParameters);
				default:
					return Enumerable.Empty<ZSqlParameter>();
			}
		}

		#region Indexing Search Filter Test

		SearchFieldCollection GetSearchFieldCollection()
		{
			var field1 = SearchField.Create("CODE", "Code");
			var field2 = SearchField.Create("NAME", "Full Name");
			var ret = new SearchFieldCollection(null, new SearchField[] { field1, field2 });
			return ret;
		}

		SearchFieldCollection GetSearchFieldCollectionWithActiveStatus(bool isActiveField)
		{
			var field1 = SearchField.Create("CODE", "Code");
			var field2 = SearchField.Create(isActiveField ? "ISACTIVE" : "ISCANCELLED", "Active Status");
			var ret = new SearchFieldCollection(null, new SearchField[] { field1, field2 });
			return ret;
		}

		public void TestHasSearchField()
		{
			var filterStrip = GetNewFilterStripBusinessObject();
			AssertEquals(false, filterStrip.HasIndexSearchFields);

			filterStrip.IndexSearchFields = new SearchFieldCollection(null, Array.Empty<SearchField>());
			AssertEquals(false, filterStrip.HasIndexSearchFields);

			filterStrip.IndexSearchFields = GetSearchFieldCollection();
			AssertEquals(true, filterStrip.HasIndexSearchFields);
		}
		public void TestGetIndexSearchFilter()
		{
			using (new GlowIndexQueryEngineMock())
			{
				var filterStrip = GetNewFilterStripBusinessObject();
				filterStrip.IndexSearchFields = GetSearchFieldCollection();
				filterStrip.SearchType = SearchType.Index;
				filterStrip.LoadModuleFilters();

				var filter = filterStrip["CODE"];
				Assert(filter is IIndexSearchModuleFilter);
			}
		}

		public void TestGetIndexSearchFilter_AddDefaultCommonSearchFilter()
		{
			using (new GlowIndexQueryEngineMock())
			{
				var filterStrip = GetNewFilterStripBusinessObject();
				filterStrip.IndexSearchFields = GetSearchFieldCollection();
				filterStrip.SearchType = SearchType.Index;
				filterStrip.LoadModuleFilters();

				var filter = filterStrip["Common"];
				Assert(filter is IndexSearchModuleTextFilter indexSearchModuleTextFilter && indexSearchModuleTextFilter.Visibility == FilterVisibility.AlwaysAppliedAndHidden);
			}
		}

		public void TestGetActiveFilterUrls_WhenEnableIndexSearch()
		{
			TestGetActiveFilterUrls(
				"Same orCategory in same group should be combined with or",
				new List<TestFilterData>()
				{
					new TestFilterData("CODE", "HAY", Red),
					new TestFilterData("NAME", "hank", Red),
				}, ["((startswith(CODE,'HAY')) or (startswith(NAME,'hank')))"]);

			TestGetActiveFilterUrls(
				"Same groupOrCategory should be combined with or",
				new List<TestFilterData>()
				{
					new TestFilterData("CODE", "HAY", None, "TestGroup1", Black),
					new TestFilterData("NAME", "hank", Red, "TestGroup2", Black),
				}, ["((startswith(CODE,'HAY')) or (startswith(NAME,'hank')))"]);

			TestGetActiveFilterUrls(
				"In same group, OrCategory is None and orGroupCategory is none",
				new List<TestFilterData>()
				{
					new TestFilterData("CODE", "HAY"),
					new TestFilterData("NAME", "hank"),
				}, ["((startswith(CODE,'HAY')) and (startswith(NAME,'hank')))"]);

			TestGetActiveFilterUrls(
				"In same group, different orCategory and orGroupCategory is none should be combined with and",
				new List<TestFilterData>()
				{
					new TestFilterData("CODE", "HAY", Yellow),
					new TestFilterData("NAME", "hank", Red),
				}, ["((startswith(CODE,'HAY')) and (startswith(NAME,'hank')))"]);

			TestGetActiveFilterUrls(
				"In same group, diferent orCategory and same groupOrCategory should be combined with and",
				new List<TestFilterData>()
				{
					new TestFilterData("CODE", "HAY", None, "TestGroup1", Black),
					new TestFilterData("NAME", "hank", Red, "TestGroup1", Black),
				}, ["((startswith(CODE,'HAY')) and (startswith(NAME,'hank')))"]);

			TestGetActiveFilterUrls(
				"In different groups, same orCategory and orGroupCategory is none should be separated",
				new List<TestFilterData>()
				{
					new TestFilterData("CODE", "HAY", Red),
					new TestFilterData("NAME", "hank", Red, "TestGroup2"),
				}, ["(startswith(CODE,'HAY'))", "(startswith(NAME,'hank'))"]);

			TestGetActiveFilterUrls(
				"In different groups, different orCategory and orGroupCategory is none should be separated",
				new List<TestFilterData>()
				{
					new TestFilterData("CODE", "HAY", White),
					new TestFilterData("NAME", "hank", Red, "TestGroup2"),
				}, ["(startswith(CODE,'HAY'))", "(startswith(NAME,'hank'))"]);
		}

		void TestGetActiveFilterUrls(string description, IEnumerable<TestFilterData> testFilterData, IEnumerable[] expected)
		{
			using (new GlowIndexQueryEngineMock())
			{
				var filterStrip = GetNewFilterStripBusinessObject();
				filterStrip.IndexSearchFields = GetSearchFieldCollection();
				filterStrip.SearchType = SearchType.Index;
				filterStrip.LoadModuleFilters();

				foreach (var testFilter in testFilterData)
				{
					var filter = filterStrip[testFilter.Name] as IndexSearchModuleTextFilter;
					filter.Property = testFilter.Property;
					filter.OrCategory = testFilter.OrCategory;
					filter.IsActive = true;
					filter.ComparisonOperator = testFilter.ComparisonOperator;
					filter.GroupName = testFilter.GroupName;
					filter.GroupOrCategory = testFilter.GroupOrCategory;
				}

				var actual = filterStrip.GetActiveFiltersQueriesWithoutAlwaysAppliedAndHiddenFilters().Select(m => m.ToUrlComponent()).Where(urlComponent => !urlComponent.IsNullOrEmpty());

				AssertContainsExactElementsInAnyOrder(description, expected, actual);
			}
		}

		public void TestGetActiveFilterUrlsWithIsActiveStatus_WhenEnableIndexSearch()
		{
			using (new GlowIndexQueryEngineMock())
			{
				var filterStrip = GetNewFilterStripBusinessObject();
				filterStrip.IndexSearchFields = GetSearchFieldCollectionWithActiveStatus(true);
				filterStrip.SearchType = SearchType.Index;
				filterStrip.LoadModuleFilters();

				var codeFilter = filterStrip["CODE"] as IndexSearchModuleTextFilter;
				codeFilter.IsActive = true;
				codeFilter.Property = "HAY";
				codeFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;

				var actual = filterStrip.GetActiveFiltersQueriesWithoutAlwaysAppliedAndHiddenFilters().Select(m => m.ToUrlComponent());
				if (filterStrip["Active Status"].Visibility != FilterVisibility.AlwaysApplied)
				{
					AssertContainsExactElementsInAnyOrder("Should not create default Active Status filter", new string[] { "(startswith(CODE,'HAY'))" }, actual);
				}
				else
				{
					var activeFilter = filterStrip["Active Status"] as IndexSearchModuleTextFilter;
					activeFilter.IsActive = true;
					activeFilter.Property = "Active";
					AssertContainsExactElementsInAnyOrder("Should create default Active Status filter", new string[] { "((startswith(CODE,'HAY')) and ((ISACTIVE eq true) or (ISACTIVE eq null)))" }, actual);
					
					activeFilter.Property = "Inactive";
					actual = filterStrip.GetActiveFiltersQueriesWithoutAlwaysAppliedAndHiddenFilters().Select(m => m.ToUrlComponent());
					AssertContainsExactElementsInAnyOrder("Should create default Active Status filter", new string[] { "((startswith(CODE,'HAY')) and (ISACTIVE eq false))" }, actual);

					activeFilter.Property = "All";
					actual = filterStrip.GetActiveFiltersQueriesWithoutAlwaysAppliedAndHiddenFilters().Select(m => m.ToUrlComponent());
					AssertContainsExactElementsInAnyOrder("Should not create default Active Status filter", new string[] { "(startswith(CODE,'HAY'))" }, actual);
				}
			}
		}

		public void TestGetActiveFilterUrlsWithIsCancelled_WhenEnableIndexSearch()
		{
			using (new GlowIndexQueryEngineMock())
			{
				var filterStrip = GetNewFilterStripBusinessObject();
				filterStrip.IndexSearchFields = GetSearchFieldCollectionWithActiveStatus(false);
				filterStrip.SearchType = SearchType.Index;
				filterStrip.LoadModuleFilters();

				var codeFilter = filterStrip["CODE"] as IndexSearchModuleTextFilter;
				codeFilter.IsActive = true;
				codeFilter.Property = "HAY";
				codeFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;

				var actual = filterStrip.GetActiveFiltersQueriesWithoutAlwaysAppliedAndHiddenFilters().Select(m => m.ToUrlComponent());
				if (filterStrip["Active Status"].Visibility != FilterVisibility.AlwaysApplied)
				{
					AssertContainsExactElementsInAnyOrder("Should not create default Active Status filter", new string[] { "(startswith(CODE,'HAY'))" }, actual);
				}
				else
				{
					var activeFilter = filterStrip["Active Status"] as IndexSearchModuleTextFilter;
					activeFilter.IsActive = true;
					activeFilter.Property = "Active";
					AssertContainsExactElementsInAnyOrder("Should create default Active Status filter", new string[] { "((startswith(CODE,'HAY')) and (ISCANCELLED eq false))" }, actual);

					activeFilter.Property = "Inactive";
					actual = filterStrip.GetActiveFiltersQueriesWithoutAlwaysAppliedAndHiddenFilters().Select(m => m.ToUrlComponent());
					AssertContainsExactElementsInAnyOrder("Should create default Active Status filter", new string[] { "((startswith(CODE,'HAY')) and ((ISCANCELLED eq true) or (ISCANCELLED eq null)))" }, actual);

					activeFilter.Property = "All";
					actual = filterStrip.GetActiveFiltersQueriesWithoutAlwaysAppliedAndHiddenFilters().Select(m => m.ToUrlComponent());
					AssertContainsExactElementsInAnyOrder("Should not create default Active Status filter", new string[] { "(startswith(CODE,'HAY'))" }, actual);
				}
			}
		}

		public class TestFilterData
		{
			public TestFilterData(string name, string property, FilterOrCategory orCategory = None, string groupName = "", FilterOrCategory groupOrCategory = None, string comparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith )
			{
				Name = name;
				Property = property;
				ComparisonOperator = comparisonOperator;
				OrCategory = orCategory;
				GroupName = groupName;
				GroupOrCategory = groupOrCategory;
			}

			public string Name { get; set; }
			public string Property { get; set; }
			public string GroupName { get; set; }
			public string ComparisonOperator { get; set; }
			public FilterOrCategory OrCategory { get; set; }
			public FilterOrCategory GroupOrCategory { get; set; }
		}

		public void TestSearchType()
		{
			var filterStripBizo = GetNewFilterStripBusinessObject();
			AssertEquals("default is sql", SearchType.Sql, filterStripBizo.SearchType);
			filterStripBizo.SearchType = SearchType.Index;
			AssertEquals("if registry not set, default is sql", SearchType.Sql, filterStripBizo.SearchType);

			using (new GlowIndexQueryEngineMock())
			{
				filterStripBizo = GetNewFilterStripBusinessObject();
				AssertEquals("default is sql", SearchType.Sql, filterStripBizo.SearchType);

				filterStripBizo.SearchType = SearchType.Index;
				AssertEquals("has field and registry, type is index", SearchType.Index, filterStripBizo.SearchType);
			}
		}

		public void TestIndexSearchWithSqlLayout()
		{
			using (new GlowIndexQueryEngineMock())
			{
				var filterStrip = GetNewFilterStripBusinessObject();
				filterStrip.IndexSearchFields = GetSearchFieldCollection();
				filterStrip.SearchType = SearchType.Index;
				filterStrip.LoadModuleFilters();

				var layout = new DataGridLayoutManager().SavePreconfiguredLayout(filterStrip, "testLayout", false, false, SaveColumnLayout.Ignore);
				AssertEquals(true, layout.S9_IsIndexSearch);

				filterStrip.LoadLayout(layout);
				AssertEquals(SearchType.Index, filterStrip.SearchType);
			}
		}
		#endregion
	}
}
