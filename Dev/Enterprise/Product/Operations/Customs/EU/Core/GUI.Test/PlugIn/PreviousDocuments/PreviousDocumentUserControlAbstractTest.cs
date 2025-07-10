using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI.PlugIn.Testing
{
	public abstract class PreviousDocumentUserControlAbstractTest<TUserControl, TBindingSource> : TestCaseWithFactory
		where TUserControl : BaseCustomsEntryUserControl, ISupportingInfoUserControls, new()
		where TBindingSource : JobDeclaration
	{
		public void TestPreviousDocumentsGridColumns()
		{
			var previousDocument = Factory.New<PreviousDocument>();
			var previousDocumentCollection = new PreviousDocumentCollection(previousDocument);
			var columns = GetOrderedGridColumns().ToArray();

			using (var form = new ZForm(Declaration))
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
			Declaration = Factory.New<TBindingSource>();
		}

		protected TBindingSource Declaration { get; private set; }

		protected TUserControl CreateControl() => new TUserControl();
	}
}
