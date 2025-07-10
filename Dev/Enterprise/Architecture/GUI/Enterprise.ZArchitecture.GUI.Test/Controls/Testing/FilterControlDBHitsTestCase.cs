using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI.Internal;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	public abstract class FilterControlDBHitsTestCase<TCollection, TFilterBizO> : TestCaseWithFactory
			where TCollection : IBusinessObjectCollection
			where TFilterBizO : FilterBusinessObject
	{
		#region TestFetchForView

		[RequiresSTA]
		public void TestFetchForView()
		{
			SetupData();
			Factory.Save();

			var tableColumns = new List<TableColumn>();

			var collection1 = GetNewCollection(Factory);
			var filterBizO = GetNewFilterBusinessObject();
			using (var filterControl = GetNewFilterControl(collection1, filterBizO))
			using (var form = new ZForm())
			using (RowFactory.SetCachedTables())
			{
				form.Controls.Add(filterControl);
				form.Show();

				LoadCollection(collection1);

				var grid = filterControl.Grid;
				grid.SetAllColumnsVisible(true);
				tableColumns.AddRange(new TableColumnCalculator().GetTableColumnsOnThisObject((BusinessObject)collection1[0], grid.Columns));
				AssertEquals("Precondition.", true, tableColumns.Any());
			}

			var baseHits = GetBaseHits();
			var hitsDictionary = GetExpectedHitsDictionary();

			CombineAssertions(() =>
			{
				var collection2 = GetNewCollection(new BusinessObjectFactory { RefreshEnabled = false });
				var columnsWithIgnoreUnspecifiedTablesFromTableHitsTestEnabled = GetTableColumnsWithIgnoreUnspecifiedTablesFromTableHitsTestEnabled();

				using (var form = new ZForm())
				using (var filterControl = GetNewFilterControl(collection2, filterBizO))
				{
					var grid = filterControl.Grid;
					grid.SetAllColumnsVisible(false);

					form.Controls.Add(filterControl);

					TableColumn previousColumn = null;

					foreach (var column in tableColumns)
					{
						hitsDictionary.TryGetValue(column.ColumnName, out var overridenHitsForThisColumn);
						var hitsToTestForThisColumn = overridenHitsForThisColumn ?? baseHits;

						var factory = new BusinessObjectFactory { RefreshEnabled = false };
						SwapFactory(collection2, factory);

						using (factory.EnableTableHitQueryCollection(hitsToTestForThisColumn.Keys.ToArray()))
						using (RowFactory.SetCachedTables())
						{
							grid.SetColumnVisible(true, column.ColumnName);

							if (previousColumn != null)
							{
								grid.SetColumnVisible(false, previousColumn.ColumnName);
							}

							// store previous column so we can simply unhide and show one column
							previousColumn = column;

							form.Show();
							LoadCollection(collection2);
							Application.DoEvents(); // to paint grid

							var ignoreUnspecifiedTables = columnsWithIgnoreUnspecifiedTablesFromTableHitsTestEnabled.Contains(column.ColumnName);
							AssertDbHits(string.Format(CultureInfo.InvariantCulture, "<strong>Test Fetch Hints for column {0}.</strong>", column.ColumnName), hitsToTestForThisColumn, factory, ignoredNotSpecifiedUnlessGreaterThan5Hits: ignoreUnspecifiedTables);
							AssertAdditionalDbHitsDetails(factory, column.ColumnName);

							if (ShouldCheckForUnusedFetchHints(column.ColumnName))
							{
								var unusedFetchHints = factory.GetAllFetchHintedTableNames().Except(GetTableNamesToIgnoreForUnusedFetchHints(column.ColumnName));
								var fetchedTables = new ZStringBuilder(unusedFetchHints).ToStringWithDelimiterBetweenAppends(",");
								AssertEquals(string.Format(CultureInfo.InvariantCulture, "Column {0}: Unused Fetch Hints for this column should be removed, or the column should be suppressed from this test using ShouldCheckForUnusedFetchHints.", column.ColumnName), string.Empty, fetchedTables);
							}

							form.Hide();
						}

						// Grid caches FetchForView, we need to reset for each column
						filterControl.Grid.ForcePreFetch();
					}
				}
			});
		}

		void SwapFactory(IBusinessObjectCollection collection, BusinessObjectFactory factory)
		{
			if (collection is BusinessObjectCollection legacyCollection)
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

			if (collection is IActiveBusinessObjectCollection activeCollection)
			{
				activeCollection.AdditionalFilter = ZQuery.NoResultQuery;
				activeCollection.SetFactory(factory);
			}
		}

		#region Test Unused Fetch Hint options

		protected virtual bool ShouldCheckForUnusedFetchHints(string columnName) => true;

		protected virtual IEnumerable<string> GetTableNamesToIgnoreForUnusedFetchHints(string columnName) => Enumerable.Empty<string>();

		#endregion

		void LoadCollection(TCollection collection)
		{
			var active = collection as IActiveBusinessObjectCollection;
			var legacy = collection as BusinessObjectCollection;

			if (active != null)
			{
				active.AdditionalFilter = new ZQuery();
			}
			else if (legacy != null)
			{
				legacy.Load();
			}
		}

		protected virtual HashSet<string> GetTableColumnsWithIgnoreUnspecifiedTablesFromTableHitsTestEnabled()
		{
			return new HashSet<string>();
		}

		protected virtual void AssertAdditionalDbHitsDetails(BusinessObjectFactory factory, string columnName)
		{
		}

		protected abstract void SetupData();
		protected abstract Dictionary<string, int> GetBaseHits();
		protected abstract Dictionary<string, Dictionary<string, int>> GetExpectedHitsDictionary();
		protected abstract TCollection GetNewCollection(BusinessObjectFactory factory);
		protected abstract TFilterBizO GetNewFilterBusinessObject();
		protected abstract ZFilterStripControl GetNewFilterControl(TCollection collection, TFilterBizO filterBizO);

		#endregion
	}
}
