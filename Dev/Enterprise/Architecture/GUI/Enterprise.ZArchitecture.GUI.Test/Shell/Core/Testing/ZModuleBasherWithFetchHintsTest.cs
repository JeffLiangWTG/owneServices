#if DEBUG

using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Modules.Testing
{
	public abstract class ZModuleBasherWithFetchHintsTest : ZModuleBasherTest
	{
		#region TestFetchHintsHaveBeenAddedIfNeeded
		[SnailTest]
		[StressTest]
		[RequiresSTA]
		public void TestFetchHintsHaveBeenAddedIfNeeded()
		{
			SetupDataForFetchHintsTest();
			using (var module = CreateModuleForFetchHintsTest())
			using (var form = new ZForm())
			{
				var filterControl = (ZFilterStripControl)module.EmbeddedControl;
				filterControl.Dock = System.Windows.Forms.DockStyle.Fill;
				var filteredGrid = filterControl.FilteredGrid;
				filteredGrid.SetAllColumnsVisible(true);
				form.Size = new System.Drawing.Size(100, 100);
				form.Controls.Add(filterControl);
				form.Show();

				var moduleForTesting = (IFilterModuleInternalsForTesting)module;
				moduleForTesting.FilterBusinessObject.ResetToDefaultValues();
				moduleForTesting.PerformSearch();
				var collection = module.GridCollection;
				AssertNotEquals(0, collection.Count);
				var tableColumns = new TableColumnCalculator().GetTableColumnsOnThisObject((BusinessObject)collection[0], filteredGrid.Columns);

				// Better to do the individual first to identify them one at a time before trying to run against all columns
				foreach (var tableColumn in tableColumns)
				{
					using (filteredGrid.SuspendRefreshTableStylesAndRefreshAtDisposal())
					{
						filteredGrid.SetAllColumnsVisible(false);
						filteredGrid.SetColumnVisible(true, tableColumn.ColumnName);
					}
					moduleForTesting.FilterBusinessObject.ResetToDefaultValues();
					moduleForTesting.PerformSearch();
					collection = module.GridCollection;
					AssertNotEquals(0, collection.Count);
					AssertColumnsHaveValue(filteredGrid, new TableColumn[] { tableColumn });
				}

				filteredGrid.SetAllColumnsVisible(true);
				moduleForTesting.FilterBusinessObject.ResetToDefaultValues();
				moduleForTesting.PerformSearch();
				collection = module.GridCollection;
				AssertNotEquals(0, collection.Count);
				AssertColumnsHaveValue(filteredGrid, tableColumns);
			}
		}

		protected abstract void SetupDataForFetchHintsTest();

		protected abstract ZFilterModule CreateModuleForFetchHintsTest();

		void AssertColumnsHaveValue(ZDisplayGrid filteredGrid, TableColumn[] tableColumns)
		{
			var collection = filteredGrid.List as IBusinessObjectCollection;
			using (collection.Factory.EnableFetchHintsProcessingWithoutTableHitCounter())
			{
				collection.FetchStrategy.FetchForView(collection.ToArray(), tableColumns); // Collection FetchForView should be called before individual FetchForView
				foreach (BusinessObject declaration in collection)
				{
					if (declaration.FetchStrategy != null)
					{
						declaration.FetchStrategy.FetchForView(tableColumns);
					}
				}

				var fieldsWithValues = new Dictionary<string, int>(tableColumns.Length);
				foreach (var tableColumn in tableColumns)
				{
					fieldsWithValues.Add(tableColumn.ColumnName, 0);
					foreach (BusinessObject declaration in collection)
					{
						var data = declaration[tableColumn.ColumnName] as IZType;
						if (!data.IsEmpty)
						{
							fieldsWithValues[tableColumn.ColumnName]++;
						}
					}
				}
				var errorBuilder = new ZStringBuilder();
				foreach (var pair in fieldsWithValues)
				{
					if (pair.Value < 6 && !FetchHintIgnoreField.Contains(pair.Key))
					{
						errorBuilder.Append(string.Format("{0} ({1})", pair.Key, filteredGrid.GetColumnCaption(pair.Key)));
					}
				}

				if (!errorBuilder.IsEmpty)
				{
					Fail("In order to test properly, the system requires to have at least 6 BusinessObjects that have data in the following fields:\r\n" + errorBuilder.ToStringWithNewLineBetweenAppends());
				}

				var tableSelects = collection.Factory.TableSelects;
				AssertNoMoreThan5DBHitsPerTable(tableColumns, tableSelects);
			}
		}

		protected virtual List<string> FetchHintIgnoreField
		{
			get { return new List<string>(); }
		}

		void AssertNoMoreThan5DBHitsPerTable(TableColumn[] tableColumns, TableHitCount[] tableSelects)
		{
			if (tableSelects.Length > 0)
			{
				var errorBuilder = new ZStringBuilder();
				foreach (var tableSelect in tableSelects.OrderBy(x => x.TableName))
				{
					if (HasFailedFetchHint(tableSelect))
					{
						errorBuilder.Append(string.Format("{0} ({1})", tableSelect.TableName, tableSelect.Value));
					}
				}
				if (!errorBuilder.IsEmpty)
				{
					Fail(new ZStringBuilder(tableColumns.Select(x => x.ColumnName)).ToStringWithDelimiterBetweenAppends(", ") +  "\r\nThe following tables had more than five hits:\r\n" + errorBuilder.ToStringWithNewLineBetweenAppends());
				}
			}
		}

		protected virtual bool HasFailedFetchHint(TableHitCount tableSelect)
		{
			return tableSelect.Value > 5;
		}

		#endregion
	}
}

#endif
