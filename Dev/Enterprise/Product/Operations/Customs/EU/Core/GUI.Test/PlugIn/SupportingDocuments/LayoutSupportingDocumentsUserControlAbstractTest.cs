using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI.PlugIn.Testing
{
	abstract class LayoutSupportingDocumentsUserControlAbstractTest<TUserControl, TBindingSource> : TestCaseWithFactory
		where TUserControl : LayoutSupportingDocumentsUserControl, new()
		where TBindingSource : JobDeclaration
	{
		public void TestBindingSourceType()
		{
			SupportingDocumentsControlTestHelper.AssertBindingSource(SupportingDocumentsUserControlType, ExpectedBindingSourceType);
		}

		public void TestSupportingDocumentsFieldsControlType()
		{
			AssertEquals("SupportingDocumentsFieldsControlType", ExpectedSupportingDocumentsFieldsControlType, supportingDocumentsUserControl.SupportingDocumentsFieldsControl.GetType());
		}

		public void TestSupportingDocumentsFieldsControlDockStyle()
		{
			AssertEquals("SupportingDocumentsFieldsControl.Dock", DockStyle.Fill, supportingDocumentsUserControl.SupportingDocumentsFieldsControl.Dock);
		}

		public void TestCaptionRenderingEnabled()
		{
			var supportingDocumentsFieldsControl = supportingDocumentsUserControl.SupportingDocumentsFieldsControl;
			var controlCaptionRenderingEnabled = supportingDocumentsUserControl.CaptionRenderingEnabled ?? false;
			var fieldsControlCaptionRenderingEnabled = supportingDocumentsFieldsControl.CaptionRenderingEnabled ?? false;
			CombineAssertions(() =>
			{
				AssertEquals(ExpectedSupportingDocumentsFieldsControlType.FullName, true, controlCaptionRenderingEnabled);
				AssertEquals(supportingDocumentsFieldsControl.GetType().FullName, true, fieldsControlCaptionRenderingEnabled);
			});
		}

		public void TestUCC6AndExportAvailableColumnNames()
		{
			var declaration = Factory.New<TBindingSource>();
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfiguration(declaration, "IsUCC6Core", true))
			using (var form = new ZForm(declaration))
			using (var supportingDocumentsUserControl = new TUserControl())
			{
				form.Controls.Add(supportingDocumentsUserControl);
				form.Show();

				AssertColumnNamesAndColumnStyle(supportingDocumentsUserControl.SupportingDocumentsGrid, ExpectedUCC6AndExportAvailableColumnNamesAndColumnStyle);
			}
		}

		public void TestUCC6AndImportAvailableColumnNames()
		{
			var declaration = Factory.New<TBindingSource>();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfiguration(declaration, "IsUCC6Core", true))
			using (var form = new ZForm(declaration))
			using (var supportingDocumentsUserControl = new TUserControl())
			{
				form.Controls.Add(supportingDocumentsUserControl);
				form.Show();

				AssertColumnNamesAndColumnStyle(supportingDocumentsUserControl.SupportingDocumentsGrid, ExpectedUCC6AndImportAvailableColumnNamesAndColumnStyle);
			}
		}

		public void TestNonUCC6AvailableColumnNames()
		{
			var declaration = Factory.New<TBindingSource>();
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfiguration(declaration, "IsUCC6Core", false))
			using (var form = new ZForm(declaration))
			using (var supportingDocumentsUserControl = new TUserControl())
			{
				form.Controls.Add(supportingDocumentsUserControl);
				form.Show();

				AssertColumnNamesAndColumnStyle(supportingDocumentsUserControl.SupportingDocumentsGrid, ExpectedNonUCC6AvailableColumnNamesAndColumnStyle);
			}
		}

		public abstract void TestSupportingDocumentsFieldsControlBindingString();

		protected void AssertColumnNamesAndColumnStyle(ZGrid grid, IEnumerable<(string ColumnName, Type ColumnType)> expectedColumnNamesAndColumnStyle)
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
			supportingDocumentsUserControl = new TUserControl();
			var declaration = Factory.New<TBindingSource>();
			supportingDocumentsUserControl.SetDataBinding(declaration, null);
		}
		TUserControl supportingDocumentsUserControl;

		protected override void TearDown()
		{
			base.TearDown();
			supportingDocumentsUserControl?.Dispose();
		}

		protected Type SupportingDocumentsUserControlType => typeof(TUserControl);

		protected virtual Type ExpectedBindingSourceType => typeof(TBindingSource);

		protected virtual Type ExpectedSupportingDocumentsFieldsControlType => typeof(LayoutSupportingDocumentsFieldsControl);

		protected virtual string ExceptedSupportingDocumentsFieldsControlBindingString => "FilteredInvoiceLines.SupportingDocuments";

		protected abstract IEnumerable<(string ColumnName, Type ColumnType)> ExpectedUCC6AndExportAvailableColumnNamesAndColumnStyle { get; }

		protected virtual IEnumerable<(string ColumnName, Type ColumnType)> ExpectedUCC6AndImportAvailableColumnNamesAndColumnStyle => new (string, Type)[]
		{
			(nameof(SupportingDocument.CSI_Code), typeof(ZCodeFindBoxColumnStyle)),
			(nameof(SupportingDocument.CSI_CodeDescription), typeof(ZTextBoxColumnStyle)),
			(nameof(SupportingDocument.CSI_ReferenceNumber), typeof(ZMultiControlColumnStyle)),
			(nameof(SupportingDocument.CSI_AdditionalDescription), typeof(ZTextBoxColumnStyle)),
			(nameof(SupportingDocument.CSI_Status), typeof(ZDropEditColumnStyle)),
			(nameof(SupportingDocument.CSI_Quantity), typeof(ZCalcEditColumnStyle)),
			(nameof(SupportingDocument.CSI_UnitOfQuantity), typeof(ZMultiControlColumnStyle)),
			(nameof(SupportingDocument.CSI_Quantity2), typeof(ZCalcEditColumnStyle)),
			(nameof(SupportingDocument.CSI_UnitOfQuantity2), typeof(ZTextBoxColumnStyle)),
			(nameof(SupportingDocument.CSI_Value), typeof(ZCalcEditColumnStyle)),
			(nameof(SupportingDocument.CSI_RX_NKCurrency), typeof(ZCodeFindBoxColumnStyle)),
			(nameof(SupportingDocument.CSI_DateOfIssue), typeof(ZDateEditColumnStyle)),
			(nameof(SupportingDocument.CSI_DateOfExpiry), typeof(ZDateEditColumnStyle))
		};

		protected abstract IEnumerable<(string ColumnName, Type ColumnType)> ExpectedNonUCC6AvailableColumnNamesAndColumnStyle { get; }
	}
}
