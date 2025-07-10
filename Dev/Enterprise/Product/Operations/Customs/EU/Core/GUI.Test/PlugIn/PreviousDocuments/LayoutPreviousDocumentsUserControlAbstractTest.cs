using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI.PlugIn.Testing
{
	abstract class LayoutPreviousDocumentsUserControlAbstractTest<TUserControl, TBindingSource> : TestCaseWithFactory
		where TUserControl : LayoutPreviousDocumentsUserControl, new()
		where TBindingSource : JobDeclaration
	{
		public void TestBindingSourceType()
		{
			using (var control = (BaseCustomsEntryUserControl)Activator.CreateInstance(PreviousDocumentsUserControlType))
			{
				AssertEquals(ExpectedBindingSourceType, control.BindingSource.DataSourceType);
			}
		}

		public void TestUCC6ImportOrExportAvailableColumnNames()
		{
			var declaration = Factory.New<TBindingSource>();
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfiguration(declaration, "IsUCC6Core", true))
			using (var form = new ZForm(declaration))
			using (var previousDocumentsUserControl = new TUserControl())
			{
				form.Controls.Add(previousDocumentsUserControl);
				form.Show();

				AssertColumnNamesAndColumnStyle(previousDocumentsUserControl.PreviousDocumentsGrid, ExpectedUCC6ImportOrExportAvailableColumnNamesAndColumnStyle);
			}
		}

		public void TestNonUCC6AvailableColumnNames()
		{
			var declaration = Factory.New<TBindingSource>();
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfiguration(declaration, "IsUCC6Core", false))
			using (var form = new ZForm(declaration))
			using (var previousDocumentsUserControl = new TUserControl())
			{
				form.Controls.Add(previousDocumentsUserControl);
				form.Show();

				AssertColumnNamesAndColumnStyle(previousDocumentsUserControl.PreviousDocumentsGrid, ExpectedNonUCC6AvailableColumnNamesAndColumnStyle);
			}
		}

		public abstract void TestFieldsUserControlInternalBindingString();

		void AssertColumnNamesAndColumnStyle(ZGrid grid, IEnumerable<(string ColumnName, Type ColumnType)> expectedColumnNamesAndColumnStyle)
		{
			CombineAssertions(() =>
			{
				AssertEquals("columns count", expectedColumnNamesAndColumnStyle.Count(), grid.Columns.Count);

				var indexCounter = 0;
				foreach (var (columnName, columnType) in expectedColumnNamesAndColumnStyle)
				{
					var column = grid.Columns.SingleOrDefault(x => x.ColumnName == columnName);
					if (column == null)
					{
						Assert($"Column: {columnName} does not exists", false);
					}
					else
					{
						var actualColumnStyleType = column.ColumnStyle.GetType();
						AssertEquals($"Column: {columnName}: exists at expected position '{indexCounter}'", columnName, grid.Columns[indexCounter].ColumnName);
						AssertEquals($"Column: {columnName}: columnStyleType", columnType, actualColumnStyleType);
						Assert($"Column: {columnName}: visible", column.IsVisible);
					}
					indexCounter++;
				}
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			previousDocumentsUserControl = new TUserControl();
			var declaration = Factory.New<TBindingSource>();
			previousDocumentsUserControl.SetDataBinding(declaration, null);
		}
		protected TUserControl previousDocumentsUserControl;

		protected override void TearDown()
		{
			base.TearDown();
			previousDocumentsUserControl?.Dispose();
		}

		protected Type PreviousDocumentsUserControlType => typeof(TUserControl);

		protected virtual Type ExpectedBindingSourceType => typeof(TBindingSource);

		protected virtual string ExceptedFieldsUserControlInternalBindingString => "FilteredInvoiceLines.PreviousDocuments";

		protected abstract IEnumerable<(string ColumnName, Type ColumnType)> ExpectedUCC6ImportOrExportAvailableColumnNamesAndColumnStyle { get; }

		protected abstract IEnumerable<(string ColumnName, Type ColumnType)> ExpectedNonUCC6AvailableColumnNamesAndColumnStyle { get; }
	}
}
