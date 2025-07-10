using System;
using System.Collections.Generic;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI.PlugIn.Testing
{
	class InvoiceLineLayoutSupportingDocumentsUserControlTest : LayoutSupportingDocumentsUserControlAbstractTest<InvoiceLineLayoutSupportingDocumentsUserControl, JobDeclaration>
	{
		public void TestHideColumnsForEucdm_Export()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Export;

			using (ConfigurationTestHelper.TemporarySetupUseEucdmSupportingDocumentGoodsShipmentAndItem(ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfigurationAndReturnMock(declaration, "IsUCC6Core", true), true, true))
			using (var form = new ZForm(declaration))
			using (var supportingDocumentsUserControl = new InvoiceLineLayoutSupportingDocumentsUserControl())
			{
				form.Controls.Add(supportingDocumentsUserControl);
				form.Show();

				AssertColumnNamesAndColumnStyle(supportingDocumentsUserControl.SupportingDocumentsGrid, new (string, Type)[]
				{
					(nameof(SupportingDocument.CSI_Code), typeof(ZCodeFindBoxColumnStyle)),
					(nameof(SupportingDocument.CSI_ReferenceNumber), typeof(ZMultiControlColumnStyle)),
					(nameof(SupportingDocument.CSI_AdditionalDescription), typeof(ZTextBoxColumnStyle)),
					(nameof(SupportingDocument.CSI_Quantity), typeof(ZCalcEditColumnStyle)),
					(nameof(SupportingDocument.CSI_UnitOfQuantity), typeof(ZMultiControlColumnStyle)),
					(nameof(SupportingDocument.CSI_Value), typeof(ZCalcEditColumnStyle)),
					(nameof(SupportingDocument.CSI_RX_NKCurrency), typeof(ZCodeFindBoxColumnStyle)),
					(nameof(SupportingDocument.CSI_DateOfExpiry), typeof(ZDateEditColumnStyle)),
					(nameof(SupportingDocument.CSI_ItemNumber), typeof(ZCalcEditColumnStyle))
				});
			}
		}

		public void TestHideColumnsForEucdm_Import()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;

			using (ConfigurationTestHelper.TemporarySetupUseEucdmSupportingDocumentGoodsShipmentAndItem(ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfigurationAndReturnMock(declaration, "IsUCC6Core", true), true, true))
			using (var form = new ZForm(declaration))
			using (var supportingDocumentsUserControl = new InvoiceLineLayoutSupportingDocumentsUserControl())
			{
				form.Controls.Add(supportingDocumentsUserControl);
				form.Show();

				AssertColumnNamesAndColumnStyle(supportingDocumentsUserControl.SupportingDocumentsGrid, new (string, Type)[]
				{
					(nameof(SupportingDocument.CSI_Code), typeof(ZCodeFindBoxColumnStyle)),
					(nameof(SupportingDocument.CSI_CodeDescription), typeof(ZTextBoxColumnStyle)),
					(nameof(SupportingDocument.CSI_ReferenceNumber), typeof(ZMultiControlColumnStyle)),
					(nameof(SupportingDocument.CSI_AdditionalDescription), typeof(ZTextBoxColumnStyle)),
					(nameof(SupportingDocument.CSI_Quantity), typeof(ZCalcEditColumnStyle)),
					(nameof(SupportingDocument.CSI_UnitOfQuantity), typeof(ZMultiControlColumnStyle)),
					(nameof(SupportingDocument.CSI_Quantity2), typeof(ZCalcEditColumnStyle)),
					(nameof(SupportingDocument.CSI_UnitOfQuantity2), typeof(ZTextBoxColumnStyle)),
					(nameof(SupportingDocument.CSI_Value), typeof(ZCalcEditColumnStyle)),
					(nameof(SupportingDocument.CSI_RX_NKCurrency), typeof(ZCodeFindBoxColumnStyle)),
					(nameof(SupportingDocument.CSI_DateOfExpiry), typeof(ZDateEditColumnStyle))
				});
			}
		}

		public override void TestSupportingDocumentsFieldsControlBindingString()
		{
			using (var control = new InvoiceLineLayoutSupportingDocumentsUserControlForTest())
			{
				AssertEquals("SupportingDocumentsFieldsControlBindingString", "FilteredInvoiceLines.SupportingDocuments", control.GetSupportingDocumentsFieldsControlBindingString);
			}
		}

		protected override IEnumerable<(string ColumnName, Type ColumnType)> ExpectedUCC6AndExportAvailableColumnNamesAndColumnStyle => new (string, Type)[]
		{
			(nameof(SupportingDocument.CSI_Code), typeof(ZCodeFindBoxColumnStyle)),
			(nameof(SupportingDocument.CSI_ReferenceNumber), typeof(ZMultiControlColumnStyle)),
			(nameof(SupportingDocument.CSI_AdditionalDescription), typeof(ZTextBoxColumnStyle)),
			(nameof(SupportingDocument.CSI_Quantity), typeof(ZCalcEditColumnStyle)),
			(nameof(SupportingDocument.CSI_UnitOfQuantity), typeof(ZMultiControlColumnStyle)),
			(nameof(SupportingDocument.CSI_Value), typeof(ZCalcEditColumnStyle)),
			(nameof(SupportingDocument.CSI_RX_NKCurrency), typeof(ZCodeFindBoxColumnStyle)),
			(nameof(SupportingDocument.CSI_DateOfExpiry), typeof(ZDateEditColumnStyle)),
			(nameof(SupportingDocument.CSI_ItemNumber), typeof(ZCalcEditColumnStyle))
		};

		protected override IEnumerable<(string ColumnName, Type ColumnType)> ExpectedNonUCC6AvailableColumnNamesAndColumnStyle => new (string, Type)[]
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

		protected override Type ExpectedSupportingDocumentsFieldsControlType => typeof(InvoiceLineLayoutSupportingDocumentsFieldsControl);
	}

	public class InvoiceLineLayoutSupportingDocumentsUserControlForTest : InvoiceLineLayoutSupportingDocumentsUserControl
	{
		public new string GetSupportingDocumentsFieldsControlBindingString => base.GetSupportingDocumentsFieldsControlBindingString();
	}
}
