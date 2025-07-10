using System;
using System.Collections.Generic;
using CargoWise.Windows.UI;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI.PlugIn.Testing
{
	sealed class InvoiceLayoutSupportingDocumentsUserControlTest : LayoutSupportingDocumentsUserControlAbstractTest<InvoiceLayoutSupportingDocumentsUserControl, JobDeclaration>
	{
		public void TestUCC6AndImportAvailableColumnNames_Eucdm()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;

			using (ConfigurationTestHelper.TemporarySetupUseEucdmSupportingDocumentGoodsShipmentAndItem(ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfigurationAndReturnMock(declaration, "IsUCC6Core", true), true, true))
			using (var form = new ZForm(declaration))
			using (var supportingDocumentsUserControl = new InvoiceLayoutSupportingDocumentsUserControl())
			{
				form.Controls.Add(supportingDocumentsUserControl);
				form.Show();

				AssertColumnNamesAndColumnStyle(supportingDocumentsUserControl.SupportingDocumentsGrid, new (string, Type)[]
				{
					(nameof(SupportingDocument.CSI_Code), typeof(ZCodeFindBoxColumnStyle)),
					(nameof(SupportingDocument.CSI_CodeDescription), typeof(ZTextBoxColumnStyle)),
					(nameof(SupportingDocument.CSI_ReferenceNumber), typeof(ZMultiControlColumnStyle)),
					(nameof(SupportingDocument.CSI_AdditionalDescription), typeof(ZTextBoxColumnStyle)),
					(nameof(SupportingDocument.CSI_DateOfExpiry), typeof(ZDateEditColumnStyle))
				});
			}
		}

		public void TestSupportingDocumentsFieldsControlBindingMember()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;

			AssertSupportingDocumentsFieldsControlBindingMember(string.Empty, "Invoices.SupportingDocuments");
			AssertSupportingDocumentsFieldsControlBindingMember("Invoices.SupportingDocuments", "Invoices.SupportingDocuments");
			AssertSupportingDocumentsFieldsControlBindingMember("CustomsEntryInstructions.SupportingDocuments", "CustomsEntryInstructions.SupportingDocuments");

			void AssertSupportingDocumentsFieldsControlBindingMember(string gridBindingMember, string expectedControlBindingMember)
			{
				using var form = new ZForm(declaration);
				using var userControl = new InvoiceLayoutSupportingDocumentsUserControl();
				userControl.SetDataBinding(declaration, string.Empty);

				CombineAssertions($"When grid binding member is '{gridBindingMember}'", () =>
				{
					AssertEquals("[PRE-CONDITION] Initial SupportingDocumentsGrid binding member is empty", string.Empty, userControl.SupportingDocumentsGrid.DataMember);

					userControl.SupportingDocumentsGrid.SetDataBinding(declaration, gridBindingMember);
					var fieldsControlBindingMember = ((ICompositeControlBindingSourceProvider)userControl).BindingSource.GetBindingMember(userControl.SupportingDocumentsFieldsControl);

					AssertEquals("[POST-CONDITION] SupportingDocumentsFieldsControl binding member", expectedControlBindingMember, fieldsControlBindingMember);
				});
			}
		}

		public override void TestSupportingDocumentsFieldsControlBindingString()
		{
			using (var control = new InvoiceLayoutSupportingDocumentsUserControlForTest())
			{
				control.SupportingDocumentsGrid.DataMember = "";
				AssertEquals("SupportingDocumentsFieldsControlBindingString Invoices", "Invoices.SupportingDocuments", control.GetSupportingDocumentsFieldsControlBindingString);

				control.SupportingDocumentsGrid.DataMember = nameof(Business.Declaration.JobDeclaration.CustomsEntryInstructions) + "." + nameof(JobComInvoiceLine.SupportingDocuments);
				AssertEquals("SupportingDocumentsFieldsControlBindingString CustomsEntryInstructions", "CustomsEntryInstructions.SupportingDocuments", control.GetSupportingDocumentsFieldsControlBindingString);
			}
		}

		protected override IEnumerable<(string ColumnName, Type ColumnType)> ExpectedUCC6AndExportAvailableColumnNamesAndColumnStyle => new (string, Type)[]
		{
			(nameof(SupportingDocument.CSI_Code), typeof(ZCodeFindBoxColumnStyle)),
			(nameof(SupportingDocument.CSI_ReferenceNumber), typeof(ZMultiControlColumnStyle)),
			(nameof(SupportingDocument.CSI_AdditionalDescription), typeof(ZTextBoxColumnStyle)),
			(nameof(SupportingDocument.CSI_DateOfExpiry), typeof(ZDateEditColumnStyle)),
			(nameof(SupportingDocument.CSI_ItemNumber), typeof(ZCalcEditColumnStyle)),
		};

		protected override IEnumerable<(string ColumnName, Type ColumnType)> ExpectedUCC6AndImportAvailableColumnNamesAndColumnStyle => new (string, Type)[]
		{
			(nameof(SupportingDocument.CSI_Code), typeof(ZCodeFindBoxColumnStyle)),
			(nameof(SupportingDocument.CSI_CodeDescription), typeof(ZTextBoxColumnStyle)),
			(nameof(SupportingDocument.CSI_ReferenceNumber), typeof(ZMultiControlColumnStyle)),
			(nameof(SupportingDocument.CSI_AdditionalDescription), typeof(ZTextBoxColumnStyle)),
			(nameof(SupportingDocument.CSI_Status), typeof(ZDropEditColumnStyle)),
			(nameof(SupportingDocument.CSI_DateOfIssue), typeof(ZDateEditColumnStyle)),
			(nameof(SupportingDocument.CSI_DateOfExpiry), typeof(ZDateEditColumnStyle))
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

		protected override Type ExpectedSupportingDocumentsFieldsControlType => typeof(InvoiceLayoutSupportingDocumentsFieldsControl);
	}

	public class InvoiceLayoutSupportingDocumentsUserControlForTest : InvoiceLayoutSupportingDocumentsUserControl
	{
		public new string GetSupportingDocumentsFieldsControlBindingString => base.GetSupportingDocumentsFieldsControlBindingString();
	}
}
