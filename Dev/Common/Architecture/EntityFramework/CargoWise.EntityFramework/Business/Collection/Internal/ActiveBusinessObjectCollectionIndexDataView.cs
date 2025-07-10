using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.Schema;
using WTG.StaticAnalysis.Annotation;

namespace CargoWise.EntityFramework
{
	[CodeAlive("Used in ActiveBusinessObjectCollectionIndexCache")]
	internal class ActiveBusinessObjectCollectionIndexDataView<T> : ActiveBusinessObjectCollectionIndex<T> where T : BusinessObject
	{
		public ActiveBusinessObjectCollectionIndexDataView(BusinessObjectFactory factory, Type collectionType, Type elementType, ICollectionRelationship relationship, ZQuery additionalFilter, IComparer sortComparer, object[] collectionState,
			ActiveBusinessObjectCollectionIndexCache<T>.CacheKey cacheKey) : base(factory, collectionType, elementType, relationship, additionalFilter, sortComparer, collectionState, cacheKey)
		{
			factory.ThreadSentry.EnsureCurrentThreadIsOwner("ActiveBusinessObjectCollectionIndexDataView");
		}

		DataViewBusinessObjectMapping DataViewBusinessObjectMapping
		{
			get
			{
				if (dataViewBusinessObjectMapping == null)
				{
					ResetListWithCache();
				}
				return dataViewBusinessObjectMapping;
			}
		}
		DataViewBusinessObjectMapping dataViewBusinessObjectMapping;

		protected override BusinessObjectList PopulateCacheCore()
		{
			try
			{
				dataViewListChangedSuspendIndex++;
				BusinessObjectList newList = new BusinessObjectList(this);
				DataViewBusinessObjectMapping newDataViewBusinessObjectMapping = new DataViewBusinessObjectMapping();

				int dataViewIndex = 0;
				EnsureDataLoaded();

				IEnumerator dataViewEnumerator;
				try
				{
					dataViewEnumerator = GetDataViewEnumerator();
				}
				catch (KeyNotFoundException)
				{
					InvalidateCacheHard(true, false);
					dataViewEnumerator = DataView.GetEnumerator();
				}
				catch (ArgumentNullException)
				{
					InvalidateCacheHard(true, false);
					dataViewEnumerator = DataView.GetEnumerator();
				}

				var rows = new List<DataRow>();
				while (dataViewEnumerator.MoveNext())
				{
					rows.Add((DataRow)dataViewEnumerator.Current);
				}

				foreach (var row in rows)
				{
					if (row.RowState.Equals(DataRowState.Deleted) || row.RowState.Equals(DataRowState.Detached))
					{
						continue;
					}
					bool excludeRowFilterForPerformance = !CompleteFilter.IsDBOnlyQuery;
					if (excludeRowFilterForPerformance ? MatchesFilter_ExcludingRowFilter(LoadWithExtraExceptionHandling((Guid)row[0])) : MatchesFilter(GetBusinessObjectFromRowSafe(row)))
					{
						var businessObject = LoadWithExtraExceptionHandling((Guid)row[0]);
						int businessObjectIndex = -1;
						if (inPopulateCacheCounter <= 3)
						{
							businessObjectIndex = InsertSorted(newList, businessObject, false);
						}
						else
						{
							OnLoadingIntoCollection(businessObject);
							newList.Add(businessObject);
							businessObjectIndex = newList.Count - 1;
						}
						if (businessObjectIndex < dataViewIndex) //then we need to bump everything it went below up
						{
							for (int i = 0; i < dataViewIndex; ++i)
							{
								if (newDataViewBusinessObjectMapping[i] >= businessObjectIndex)
								{
									newDataViewBusinessObjectMapping[i] += 1;
								}
							}
						}
						newDataViewBusinessObjectMapping[dataViewIndex] = businessObjectIndex;
						if (CompleteFilter.MaximumRows != null && newList.Count == CompleteFilter.MaximumRows)
						{
							break;
						}
					}
					dataViewIndex++;
				}

				newList.AddRange(UncommittedObjects);

				dataViewBusinessObjectMapping = newDataViewBusinessObjectMapping;
				return newList;
			}
			finally
			{
				dataViewListChangedSuspendIndex--;
			}
		}

		protected virtual IEnumerator GetDataViewEnumerator()
		{
			var dataView = DataView;
			return (Relationship as ICollectionRelationshipWithCustomEnumerator)?.GetDataEnumerator(AdditionalFilter) ?? dataView.GetEnumerator();
		}

		void EnsureDataLoaded()
		{
			if (!isDataLoaded)
			{
				isDataLoaded = true;
				dataViewListChangedSuspendIndex++;
				try
				{
					Relationship.LoadBusinessObjects(Factory, AdditionalFilter);					
				}
				catch (SqlException)
				{
					isDataLoaded = false;
					throw;
				}
				finally
				{
					dataViewListChangedSuspendIndex--;
				}
			}
		}

		protected override string GetDataViewSortExpression()
		{
			return DataViewSortExpression;
		}

		protected override void CheckInvariantCore()
		{
			if (Count == CompleteFilter.MaximumRows)
			{
				return; //DataView rows can exceed collection rows if MaximumRows is set on filter.
			}

			var bizOsInDataViewMatchingFilter = new List<BusinessObject>();
			for (var i = 0; i < Math.Min(Count, 100); ++i)
			{
				T businessObject = GetBusinessObjectFromRow(DataView[i]);
				if (businessObject != null && MatchesFilter(businessObject))
				{
					bizOsInDataViewMatchingFilter.Add(businessObject);

					var indexInList = DataViewBusinessObjectMapping[i];
					if (indexInList < 0 || indexInList >= Count)
					{
						ErrorReporter.ReportOnce(string.Format(CultureInfo.InvariantCulture,
							"Mapping is invalid. Expected {0}, mappings = {1}",
							businessObject.PK, DataViewBusinessObjectMapping));
					}
					else
					{
						T bizoInList = List[DataViewBusinessObjectMapping[i]];
						if (businessObject.PK != bizoInList.PK)
						{
							ErrorReporter.ReportOnce(string.Format(CultureInfo.InvariantCulture,
								"Business object in DataView not found in list where mapping says it should be. Expected {0}, was {1}, mappings = {2}",
								businessObject.PK, bizoInList.PK, DataViewBusinessObjectMapping));
						}
					}
				}
			}

			//if it's less, it might be hitting the 100 row limit or whatever. So don't worry about it.
			if (bizOsInDataViewMatchingFilter.Count > Count)
			{
				ErrorReporter.ReportOnce(string.Format(CultureInfo.InvariantCulture,
							"bizOsInDataViewMatchingFilter.Count and Count differ. Expected {0}, was {1}, mappings = {2}",
							bizOsInDataViewMatchingFilter.Count, Count, DataViewBusinessObjectMapping));
			}
		}

		#region DataView

		string DataViewSortExpression
		{
			get
			{
				if (dataViewSortExpression == null)
				{
					dataViewSortExpression = "";
					if (SortDescriptions != null)
					{
						for (int i = 0; i < SortDescriptions.Count; i++)
						{
							if (!Table.Columns.Contains(SortDescriptions[i].PropertyDescriptor.Name))
							{
								break;
							}
							if (i >= 1)
							{
								dataViewSortExpression += ", ";
							}
							dataViewSortExpression += SortDescriptions[i].PropertyDescriptor.Name;
							if (SortDescriptions[i].SortDirection == ListSortDirection.Descending)
							{
								dataViewSortExpression += " DESC";
							}
						}
					}
				}
				return dataViewSortExpression;
			}
		}

		string dataViewSortExpression;

		DataViewIndexer DataView
		{
			get
			{
				EnsureDataViewIfRequired();
				return dataView;
			}
		}
		DataViewIndexer dataView;

		DataView PKSortedDataView
		{
			get
			{
				var view = DataView.View;
				if (view != null && (pkSortedDataView == null || pkSortedDataView.RowFilter != view.RowFilter))
				{
					pkSortedDataView = DataViewCache.GetPKSortedDataView(Table, view.RowFilter);
				}
				return pkSortedDataView;
			}
		}
		DataView pkSortedDataView;

		protected void EnsureDataViewIfRequired()
		{
			if (dataView == null)
			{
				dataView = GetDataViewIndex();
			}
		}

		DataViewIndexer GetDataViewIndex()
		{
			var completeFilter = CompleteFilter;
			if (string.IsNullOrEmpty(DataViewSortExpression) && TryGetStrongFK(completeFilter, out var schemaColumn, out var key, out var replacementQuery))
			{
				var heapIndex = SharedDataTableIndexCache.GetSharedDataTableIndex(Table, replacementQuery, schemaColumn, DataViewRowState.CurrentRows);
				return new DataViewIndexer(heapIndex, schemaColumn.Name, key, DataView_ListChanged);
			}
			else
			{
				var view = DataViewCache.GetDataView(Table, CompleteAdoReductionFilter, DataViewSortExpression, DataViewRowState.CurrentRows);
				return new DataViewIndexer(view, DataView_ListChanged);
			}
		}

		bool TryGetStrongFK(ZQuery completeFilter, out SchemaColumn schemaColumn, out object key, out ZQuery replacementQuery)
		{
			if (string.IsNullOrEmpty(completeFilter.OrderBy) && !completeFilter.IsDBOnlyQuery && !completeFilter.ContainsOrOperator && !completeFilter.IsNoResultQuery)
			{
				var clonedQuery = completeFilter.DeepClone();
				clonedQuery.Simplify();
				if (clonedQuery.FilterParts.All(p => p is ZSqlParameter || p is JoinCondition || p is ZSQLInFilter))
				{
					var (foundColumn, value, bestFilterPart) = FindBestFilterPart(clonedQuery);
					if (bestFilterPart != null)
					{
						schemaColumn = foundColumn;
						key = value;
						replacementQuery = clonedQuery.FilterParts.Except(bestFilterPart).Aggregate(new ZQuery(), (accum, q) => accum.AddToFilter(new ZQuery(q)));
						replacementQuery.Simplify();
						return true;
					}
				}
			}

			schemaColumn = null;
			key = null;
			replacementQuery = null;
			return false;
		}

		static (SchemaColumn column, object value, IFilterPart part) FindBestFilterPart(ZQuery reducedQuery)
		{
			var bestZSqlParameter = reducedQuery.FilterParts.OfType<ZSqlParameter>()
				.Where(p => p.ComparisonOperator == SQLComparisonOperator.Equal && p.SchemaColumn is SchemaGuidColumn)
				.MinBySafe(p => p.SchemaColumn.Ordinal);
			if (bestZSqlParameter != null)
			{
				return (bestZSqlParameter.SchemaColumn, bestZSqlParameter.ValueAdjustedForComparison, bestZSqlParameter);
			}
			else
			{
				var bestInFilter = reducedQuery.FilterParts.OfType<ZSQLInFilter>()
						.Where(p => p.ComparisonOperator == SQLComparisonOperator.Equal && p.Column is SchemaGuidColumn && p.GetValues().Take(2).Count() == 1)
						.MinBySafe(p => p.Column.Ordinal);
				if (bestInFilter != null)
				{
					return (bestInFilter.Column, bestInFilter.ComparisonOperator.ValueAdjustedForComparisonOperator(bestInFilter.GetValues().Single()), bestInFilter);
				}
				else
				{
					return default;
				}
			}
		}

		#endregion

		#region Implementation

		protected override void Table_RowDeleting(object sender, DataRowChangeEventArgs e)
		{
			if (DataViewContainsRow(e.Row))
			{
				var businessObject = GetBusinessObjectFromRowSafe(e.Row);
				if (!businessObject.IsDeletingForDataRefresh && MatchesFilter(businessObject))
				{
					HasChangesFromDelete = true;
				}
			}
		}

		protected override void InvalidateCacheHard(bool forceNewDataView = false, bool fireResetEvent = true)
		{
			if (dataView != null)
			{
				if (forceNewDataView)
				{
					DataViewCache.RemoveDataView(Table, dataView.View);
				}

				dataView.Dispose();
			}
			dataView = null;
			base.InvalidateCacheHard();
		}

		bool DataViewContainsRow(DataRow row)
		{
			var pKSortedDataView = PKSortedDataView;
			if (pkSortedDataView != null)
			{
				return pKSortedDataView.Find(row[0]) != -1;
			}
			else
			{
				return DataView.MatchesIndex(row);
			}
		}

		bool MatchesFilter_ExcludingRowFilter(T businessObject)
		{
			bool result = false;
			if (businessObject != null)
			{
				result = MatchesFilterBaseBehaviour_ExcludingRowFilter(businessObject);
				var activeOwner = ActiveOwner;
				if (activeOwner != null && ActiveBusinessObjectCollection<T>.IsMatchesFilterOverridden(CollectionType))
				{
					result = result && activeOwner.MatchesFilterExcludingBaseBehaviour(businessObject);
				}
			}
			return result;
		}

		bool MatchesFilterBaseBehaviour_ExcludingRowFilter(T businessObject)
		{
			return
				IsNonCommittedElement(businessObject) ||
				(businessObject.Row.RowState != DataRowState.Detached &&
				 (!CollectionRelationship.IsMatchesRelationshipFilterOverridden(Relationship.GetType()) || Relationship.MatchesRelationshipFilter(businessObject, AdditionalFilter.IgnoreActiveFilter, false)));
		}

		bool RowExistsInLastEntity(DataRow row)
		{
			bool result = false;
			if (Count > 0)
			{
				DataRow existingRow = ((INeedRow)List[Count - 1]).Row;
				result = (existingRow == row);
			}
			return result;
		}

		protected override void InvalidateCacheCore()
		{
			dataViewBusinessObjectMapping = null;
		}

		#endregion

		#region DataView.ListChanged

		int dataViewListChangedSuspendIndex;

		void DataView_ListChanged(object sender, ListChangedEventArgs e)
		{
			if (dataViewListChangedSuspendIndex == 0)
			{
				CollectionListChangedSuspender.GetInstance(Factory).RunDelayableListChanged(DataView_ListChanged_Delayed, sender, e);
			}
		}

		void DataView_ListChanged_Delayed(object sender, ListChangedEventArgs e)
		{
			if (!IsDisposed && // Index could have been disposed while event was suspended
				e.ListChangedType != ListChangedType.PropertyDescriptorAdded &&
				e.ListChangedType != ListChangedType.PropertyDescriptorChanged &&
				e.ListChangedType != ListChangedType.PropertyDescriptorDeleted)
			{
				DecrementReadOnlyIncludingChildrenAndResumeValidationIfRequired();

				using (MakeActiveOwnerStrongTemporarilyForPerformance())
				{
					if (ActiveOwner == null)
					{
						if (!IsDisposed)
						{
							Dispose();
						}
					}
					else
					{
						if (!Factory.IsLoading)
						{
							HandleDataViewListChanged(e);
						}
					}
				}
			}
		}

		void HandleDataViewListChanged(ListChangedEventArgs e)
		{
			e = CorrectDataViewListChangedEventArgs(e);

			int oldIndex = e.OldIndex;
			int newIndex = e.NewIndex;
			bool dontBubbleEvent = false;
			ListChangedType listChangedType = e.ListChangedType;

			switch (e.ListChangedType)
			{
				case ListChangedType.ItemAdded:
					HandleDataViewItemAdded(ref listChangedType, ref newIndex, ref dontBubbleEvent);
					break;
				case ListChangedType.ItemChanged:
					HandleDataViewItemChanged(ref listChangedType, ref oldIndex, ref newIndex, ref dontBubbleEvent);
					break;
				case ListChangedType.ItemDeleted:
					HandleDataViewItemDeleted(ref listChangedType, ref newIndex, ref dontBubbleEvent);
					break;
				case ListChangedType.ItemMoved:
					HandleDataViewItemMoved(ref listChangedType, ref oldIndex, ref newIndex, ref dontBubbleEvent);
					break;
				case ListChangedType.Reset:
					InvalidateCache();
					break;
				default:
					dontBubbleEvent = true;
					break;
			}

			if (!dontBubbleEvent && ShouldFireChangeEvents())
			{
				FireChangeEvents(listChangedType, newIndex, oldIndex);
			}
			CheckInvariantIfEnabled();
		}

		void HandleDataViewItemAdded(ref ListChangedType listChangedType, ref int newIndex, ref bool dontBubbleEvent)
		{
			bool wasCachePopulated = IsCachePopulated();
			int originalNewIndex = newIndex;
			var businessObject = GetBusinessObjectFromRowSafe(DataView[originalNewIndex]);

			bool isMaximumRowsViolation =
				CompleteFilter.MaximumRows != null &&
				Count == (int)CompleteFilter.MaximumRows;
			if ((!wasCachePopulated || isMaximumRowsViolation) && DataView[originalNewIndex].RowState != DataRowState.Detached)
			{
				dontBubbleEvent = true;
			}
			else if (originalNewIndex == DataView.Count - 1)
			{
				if (businessObject != null && MatchesFilter(businessObject))
				{
					newIndex = Count - 1;
					if (!RowExistsInLastEntity(DataView[originalNewIndex]))
					{
						var item = LoadWithExtraExceptionHandling(businessObject.PK);
						newIndex = InsertSorted(List, item, false);
					}
					if (newIndex == Count - 1 && originalNewIndex == Count - 1)
					{
						DataViewBusinessObjectMapping[originalNewIndex] = newIndex;
					}
					else
					{
						if (newIndex < 0)
						{
							//In some cases, HandleDataViewItemAdded might be called for something already in the List. There might be things we can do to
							//update mappings and salvage this, but resetting is probably safer.
							InvalidateCache();
							listChangedType = ListChangedType.Reset;
							//DataViewBusinessObjectMapping[originalNewIndex] = wherever it is in the list right now;
						}
						else
						{
							//shove everything up to make room
							//(Count - 1 is the slot of the new highest element, so we start by moving Count - 2 up one.)
							//(We don't have to do this part ATM since we only insert at the end of mappings. Leaving this in for if that condition changes.)
							/*for (var i = Count - 2; i >= originalNewIndex; --i)
							{
								DataViewBusinessObjectMapping[i + 1] = DataViewBusinessObjectMapping[i];
							}*/
							//For each entry that has an equal or higher value than newIndex, increment it by 1 (the other half of the 'sliding everything up' operation).
							for (var i = 0; i <= Count; ++i)
							{
								if (DataViewBusinessObjectMapping[i] >= newIndex)
								{
									DataViewBusinessObjectMapping[i] = DataViewBusinessObjectMapping[i] + 1;
								}
							}
							//Then finally insert new value in the gap.
							DataViewBusinessObjectMapping[originalNewIndex] = newIndex;
						}
					}
				}
				else
				{
					businessObject = null;
					// if the last AddNew'd record is still there, fire the deleted event
					if (RowExistsInLastEntity(DataView[originalNewIndex]))
					{
						newIndex = DataViewBusinessObjectMapping[originalNewIndex];
						if (wasCachePopulated)
						{
							List.RemoveAt(newIndex);
						}
						newIndex = Count;
						listChangedType = ListChangedType.ItemDeleted;
					}
					else
					{
						// otherwise it never existed in the collection in the first place
						dontBubbleEvent = true;
					}
					DataViewBusinessObjectMapping[originalNewIndex] = -1;
				}
			}
			else
			{
				InvalidateCache();
				listChangedType = ListChangedType.Reset;
			}

			var activeOwner = ActiveOwner;
			if (activeOwner != null && IsLoaded && businessObject != null && MatchesFilter(businessObject))
			{
				activeOwner.OnAddIntoRelationship(businessObject);
			}
		}

		void HandleDataViewItemChanged(ref ListChangedType listChangedType, ref int oldIndex, ref int newIndex, ref bool dontBubbleEvent)
		{
			if (newIndex < DataView.Count)
			{
				int dataViewOldIndex = oldIndex;
				int dataViewNewIndex = newIndex;

				bool wasCachePopulated = IsCachePopulated();
				var businessObject = GetBusinessObjectFromRowSafe(DataView[dataViewNewIndex]);
				oldIndex = DataViewBusinessObjectMapping[dataViewOldIndex];
				int currentBusinessIndex = DataViewBusinessObjectMapping[dataViewNewIndex];
				if (!(SortComparer is IFreezeSortOnElementModifyComparer) && UniqueBusinessObjectComparer != null)
				{
					newIndex =
						currentBusinessIndex == -1 || currentBusinessIndex >= List.Count ? -1 :
							List.BinarySearchAssumingItsNotInTheRightOrder(currentBusinessIndex, UniqueBusinessObjectComparer);
				}
				else
				{
					newIndex =
						currentBusinessIndex == -1 || currentBusinessIndex >= List.Count ? -1 :
							currentBusinessIndex;
				}

				if ((currentBusinessIndex == -1 || currentBusinessIndex != newIndex) &&
					businessObject != null &&
					MatchesFilter(businessObject))
				{
					if (currentBusinessIndex == -1 && (Count == 0 || newIndex == Count))
					{
						listChangedType = ListChangedType.ItemAdded;
						if (wasCachePopulated)
						{
							newIndex = InsertSorted(List, businessObject is T b ? b : GetBusinessObjectFromRow(businessObject.Row), false);
							if (newIndex >= 0)
							{
								DataViewBusinessObjectMapping[dataViewNewIndex] = newIndex;
							}
							else
							{
								InvalidateCache();
								listChangedType = ListChangedType.Reset;
							}
						}
					}
					else
					{
						InvalidateCache();
						listChangedType = ListChangedType.Reset;
					}
				}
				else if (DataViewBusinessObjectMapping[dataViewNewIndex] != -1 && !MatchesFilter(businessObject))
				{
					InvalidateCache();
					listChangedType = ListChangedType.Reset;
				}
				else
				{
					dontBubbleEvent = true;
				}
			}
		}

		void HandleDataViewItemDeleted(ref ListChangedType listChangedType, ref int newIndex, ref bool dontBubbleEvent)
		{
			BusinessObject bizo = null;
			var originalNewIndex = newIndex;
			if (IsCachePopulated())
			{
				newIndex = DataViewBusinessObjectMapping[originalNewIndex];
				if (CompleteFilter.MaximumRows != null && List.Count == CompleteFilter.MaximumRows)
				{
					InvalidateCache();
					listChangedType = ListChangedType.Reset;
				}
				else if (newIndex != -1)
				{
					bizo = List[newIndex];

					if (newIndex == (Count - 1))
					{
						//Remove deleted entry from the list where it was mapped to.
						List.RemoveAt(newIndex);
						//Remove entry at DataViewBusinessObjectMapping[originalNewIndex] and slide everything down to fill the gap.
						for (var i = originalNewIndex; i <= Count; ++i)
						{
							DataViewBusinessObjectMapping[i] = DataViewBusinessObjectMapping[i + 1];
						}
						//For each entry that has a higher value than newIndex, decrement it by 1 (the other half of the 'sliding everything down' operation).
						//(We don't have to do this if newIndex is the highest value, which the current if condition guarantees. Leaving this in for if that condition changes.)
						/*for (var i = 0; i <= Count; ++i)
						{
							if (DataViewBusinessObjectMapping[i] > newIndex)
							{
								DataViewBusinessObjectMapping[i] = DataViewBusinessObjectMapping[i] - 1;
							}
						}*/
					}
					else
					{
						InvalidateCache();
					}
				}
				else
				{
					dontBubbleEvent = true;
				}
			}
			else
			{
				listChangedType = ListChangedType.Reset;
			}

			var activeOwner = ActiveOwner;
			if (activeOwner != null)
			{
				activeOwner.OnRemoveFromRelationship(bizo);
			}
		}

		void HandleDataViewItemMoved(ref ListChangedType listChangedType, ref int oldIndex, ref int newIndex, ref bool dontBubbleEvent)
		{
			int dataViewOldIndex = oldIndex;
			int dataViewNewIndex = newIndex;

			if (!IsCachePopulated())
			{
				InvalidateCache();
				listChangedType = ListChangedType.Reset;
			}
			else if (dataViewOldIndex < DataView.Count && dataViewNewIndex < DataView.Count)
			{
				oldIndex = DataViewBusinessObjectMapping[dataViewOldIndex];
				newIndex = DataViewBusinessObjectMapping[dataViewNewIndex];
				var businessObject = GetBusinessObjectFromRowSafe(DataView[dataViewNewIndex]);

				if (oldIndex == -1 && newIndex == -1)
				{
					if (MatchesFilter(businessObject))
					{
						InvalidateCache();
						listChangedType = ListChangedType.Reset;
					}
					else
					{
						dontBubbleEvent = true;
					}
				}
				else if (SortComparer is IFreezeSortOnElementModifyComparer && oldIndex != -1 && newIndex != -1 && MatchesFilter(businessObject))
				{
					//Even if we don't want to change our sort, we still need to fix our mappings.
					FixMappings_SourceChanged(dataViewOldIndex, dataViewNewIndex);
					dontBubbleEvent = true;
				}
				else
				{
					InvalidateCache();
					listChangedType = ListChangedType.Reset;
				}
			}
			else
			{
				InvalidateCache();
				listChangedType = ListChangedType.Reset;
			}
		}

		protected void FixMappings_SourceChanged(int oldSource, int newSource)
		{
			//Worked examples: https://pastebin.com/0xpqG8XJ
			if (oldSource < 0 || oldSource >= Count || newSource < 0 || newSource >= Count || oldSource == newSource)
			{
				return;
			}
			var destinationToMove = DataViewBusinessObjectMapping[oldSource];
			if (oldSource > newSource) //moving downwards: destinations all ascend by one, and then we slot at the start.
			{
				for (var i = oldSource; i > newSource; --i)
				{
					DataViewBusinessObjectMapping[i] = DataViewBusinessObjectMapping[i - 1];
				}
			}
			else
			{
				//moving upwards: destinations all descend by one, and then we slot at the end.
				for (var i = oldSource; i < newSource; ++i)
				{
					DataViewBusinessObjectMapping[i] = DataViewBusinessObjectMapping[i + 1];
				}
			}
			DataViewBusinessObjectMapping[newSource] = destinationToMove;
		}

		protected override void FixMappingsCore(int oldDestination, int newDestination)
		{
			//worked examples: https://pastebin.com/5JHhzyKx
			int whereToMoveFrom = -1;
			for (var i = 0; i <= Count; ++i)
			{
				if (DataViewBusinessObjectMapping[i] == oldDestination)
				{
					whereToMoveFrom = i;
					break;
				}
			}

			if (oldDestination == newDestination)
			{
				return;
			}
			else if (oldDestination < newDestination) //increasing: everything between old destination (Exclusive) and new destination (Inclusive) -1
			{
				for (var i = 0; i <= Count; ++i)
				{
					if (DataViewBusinessObjectMapping[i] > oldDestination && DataViewBusinessObjectMapping[i] <= newDestination)
					{
						DataViewBusinessObjectMapping[i] = DataViewBusinessObjectMapping[i] - 1;
					}
				}
			}
			else //decreasing: everything between new destination (Inclusive) and old destination (Exclusive) +1
			{
				for (var i = 0; i <= Count; ++i)
				{
					if (DataViewBusinessObjectMapping[i] >= newDestination && DataViewBusinessObjectMapping[i] < oldDestination)
					{
						DataViewBusinessObjectMapping[i] = DataViewBusinessObjectMapping[i] + 1;
					}
				}
			}
			//finally move our moving entry
			if (whereToMoveFrom >= 0)
			{
				DataViewBusinessObjectMapping[whereToMoveFrom] = newDestination;
			}
		}
		ListChangedEventArgs CorrectDataViewListChangedEventArgs(ListChangedEventArgs e)
		{
			var result = e;
			if (e.ListChangedType == ListChangedType.ItemMoved && e.OldIndex < 0)
			{
				result = new ListChangedEventArgs(ListChangedType.ItemAdded, e.NewIndex);
			}
			return result;
		}

		#endregion

		#region ICancelAddNew

		protected override void DeleteCore(T element)
		{
			var listChangedSuspender = CollectionListChangedSuspender.GetInstance(Factory);
			if (listChangedSuspender.IsDelayerEnabled && List != null && dataView != null)
			{
				dataViewListChangedSuspendIndex++;
				try
				{
					if (!element.IsDeleted)
					{
						element.Delete();
					}
					if (List != null) // In case it has been reset in element.Delete()
					{
						List.Remove(element);
						listChangedSuspender.RunDelayableListChanged(DataView_ListChanged_Delayed, dataView, new ListChangedEventArgs(ListChangedType.Reset, -1));
					}
				}
				finally
				{
					dataViewListChangedSuspendIndex--;
				}
			}
			else
			{
				element.Delete();
			}
		}

		#endregion

		#region BusinessObject events

		protected override void BusinessObject_NotificationsChangedCore(NotificationsChangedEventArgs e)
		{
			var listChangedSuspender = CollectionListChangedSuspender.GetInstance(Factory);
			if (listChangedSuspender.IsDelayerEnabled && List != null && dataView != null)
			{
				var element = e.SourceOfNotificationChange as T;
				if (element != null && element.IsDeleted)
				{
					List.Remove(element);

					var activeOwner = ActiveOwner;
					if (activeOwner != null)
					{
						activeOwner.OnRemoveFromRelationship(element);
					}

					listChangedSuspender.RunDelayableListChanged(DataView_ListChanged_Delayed, dataView, new ListChangedEventArgs(ListChangedType.Reset, -1));
				}
			}
		}

		#endregion

		#region ConsolidateListResetEvents / SuspendListChanged

		protected override void OnHasChangesChangedHandlerAdded()
		{
			EnsureDataViewIfRequired();
		}

		#endregion

		#region Dispose

		[SuppressMessage("Microsoft.Usage", "CA2215:DisposeMethodsShouldCallBaseClassDispose", Justification = "Base dispose is always called, just indirectly.")]
		protected override void Dispose(bool isDisposing)
		{
			if (isDisposing && dataView != null)
			{
				dataView.Dispose();
			}
		}

		#endregion
	}
}
