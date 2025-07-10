using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.Customs.EU.GUI.PlugIn;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.TemporaryStorage.GUI.Testing
{
	public abstract class UCC6TemporaryStoragePreviousDocumentsUserControlWithGridAbstractTest<TUserControl, TBindingSource> : TestCaseWithFactory
		where TUserControl : ZUserControl, ISupportingInfoUserControls, new()
		where TBindingSource : TemporaryStorageHeader
	{
		public abstract void TestPreviousDocumentsFieldsControlBinding();

		public void TestPreviousDocumentsGridColumns()
		{
			var previousDocument = Factory.New<TemporaryStoragePreviousDocument>();
			var previousDocumentCollection = new TemporaryStoragePreviousDocumentCollection<TemporaryStoragePreviousDocument>(previousDocument);
			var columns = GetOrderedGridColumns().ToArray();

			using (var form = new ZForm(TemporaryStorage))
			using (var control = new TUserControl())
			{
				form.Controls.Add(control);
				form.Show();

				ISupportingInfoUserControls supportInfoUserControls = control;
				var grid = supportInfoUserControls.Grid;
				grid.SetDataBinding(previousDocumentCollection, "");

				CombineAssertions(() =>
				{
					AssertEquals("Grid Column Count", columns.Length, grid.Columns.Count);

					var columnIndexCounter = 0;

					foreach (var (columnName, columnType) in columns)
					{
						var gridColumn = grid.Columns.SingleOrDefault(c => string.Equals(c.ColumnName, columnName, StringComparison.OrdinalIgnoreCase));
						AssertNotNull($"Column: {columnName}", gridColumn);
						if (gridColumn != null)
						{
							var actualColumnStyleType = gridColumn.ColumnStyle.GetType();
							AssertEquals($"Column: {columnName}: exists at expected position '{columnIndexCounter}'", columnName, grid.Columns[columnIndexCounter].ColumnName);
							AssertEquals($"Column: {columnName}: columnStyleType", columnType, actualColumnStyleType);
							Assert($"Column: {columnName}: visible", gridColumn.IsVisible);
						}

						columnIndexCounter++;
					}
				});
			}
		}

		protected abstract IEnumerable<(string, Type)> GetOrderedGridColumns();

		protected override void SetUp()
		{
			base.SetUp();
			TemporaryStorage = Factory.New<TBindingSource>();
		}

		protected TBindingSource TemporaryStorage { get; private set; }

		protected TUserControl CreateControl() => new TUserControl();
	}
}
