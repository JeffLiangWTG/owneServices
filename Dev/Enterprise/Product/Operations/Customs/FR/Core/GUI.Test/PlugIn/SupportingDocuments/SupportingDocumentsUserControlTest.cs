using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.GUI.PlugIn.Testing;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.FR.Registry;
using Enterprise.Environment;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.FR.GUI.PlugIn.Testing
{
	public class SupportingDocumentsUserControlTest : TestCaseWithFactory
	{
		public void TestBindingSourceType()
		{
			SupportingDocumentsControlTestHelper.AssertBindingSource(typeof(SupportingDocumentsUserControl), typeof(JobDeclaration));
		}

		public void TestSupportingDocumentsFieldsControlType()
		{
			SupportingDocumentsControlTestHelper.AssertSupportingDocumentsFieldsControlType(typeof(SupportingDocumentsUserControl), typeof(SupportingDocumentsFieldsControl));
		}

		public void TestCaptionRenderingEnabled()
		{
			SupportingDocumentsControlTestHelper.AssertCaptionRenderingEnabledForUserControlAndFieldsControl(typeof(SupportingDocumentsUserControl));
		}

		public void TestBottomPanelMinimumHeight()
		{
			using (var control = new SupportingDocumentsUserControl())
			{
				control.Show();
				var panel = control.FindSingleOrDefault<ZPanel>("BottomPanel");
				AssertEquals(181, panel.MinimumSize.Height);
			}
		}

		public void TestSupportingDocumentsGridContainsRequiredColumns()
		{
			string[] requiredColumns = new[]
				{
					SupportingDocument.Schema.CSI_Code,
					SupportingDocument.Schema.CSI_ReferenceNumber,
					SupportingDocument.Schema.CSI_Description,
					SupportingDocument.Schema.CSI_IsDTP,
					SupportingDocument.Schema.CSI_Quantity,
					SupportingDocument.Schema.CSI_UnitOfQuantity,
					SupportingDocument.Schema.CSI_Quantity2,
					SupportingDocument.Schema.CSI_UnitOfQuantity2,
					SupportingDocument.Schema.CSI_Value,
					SupportingDocument.Schema.CSI_RX_NKCurrency,
					SupportingDocument.Schema.CSI_DateOfIssue,
					SupportingDocument.Schema.CSI_DateOfExpiry,
					SupportingDocument.Schema.CSI_Quantity3,
					SupportingDocument.Schema.CSI_LineNo,
					SupportingDocument.Schema.CSI_AdditionalDescription,
					SupportingDocument.Schema.CSI_ReferenceNumber2,
					SupportingDocument.Schema.CSI_ItemNumber,
				};

			using (var control = new SupportingDocumentsUserControl())
			{
				var grid = control.Controls.Find("SupportingDocumentsGrid", true).FirstOrDefault() as ZGrid;

				foreach (var column in requiredColumns)
				{
					AssertNotNull(grid.GetColumnStyle(column));
				}
			}
		}

		public void TestGridColumnStyleProperties()
		{
			var declarationNonUCC = Factory.New<JobDeclaration>();
			declarationNonUCC.JE_ApplicationCode = Business.DeclarationApplicationCodeList.Codes.DeltaG;
			declarationNonUCC.JE_MessageType = JobMessageTypeList.Codes.Import;
			declarationNonUCC.Invoices.AddNew().InvoiceLines.AddNew();
			declarationNonUCC.SupportingDocuments.AddNew();
			using (var form = new JobDeclarationForm(declarationNonUCC))
			using (var controlNonUCC = new SupportingDocumentsUserControl())
			{
				form.Controls.Add(controlNonUCC);
				controlNonUCC.JobDeclaration = declarationNonUCC;
				form.Show();
				var grid = controlNonUCC.Controls.Find("SupportingDocumentsGrid", true).FirstOrDefault() as ZGrid;
				AssertGridColumnStyleProperties(grid, isUCC: false);
			}

			using (FRCustomsDataRegistry.Instance.EnableDeltaIEForImports.SetTemporaryValue(Guid.Empty, Env.CurrentBranchPK, Guid.Empty, true))
			{
				var declarationUCC = Factory.New<JobDeclaration>();
				declarationUCC.JE_ApplicationCode = Business.DeclarationApplicationCodeList.Codes.DeltaIE;
				declarationUCC.JE_MessageType = JobMessageTypeList.Codes.Import;
				declarationUCC.Invoices.AddNew().InvoiceLines.AddNew();
				declarationUCC.SupportingDocuments.AddNew();

				using (var form = new JobDeclarationForm(declarationUCC))
				using (var controlUCC = new SupportingDocumentsUserControl())
				{
					form.Controls.Add(controlUCC);
					controlUCC.JobDeclaration = declarationUCC;
					form.Show();
					var grid = controlUCC.Controls.Find("SupportingDocumentsGrid", true).FirstOrDefault() as ZGrid;
					AssertGridColumnStyleProperties(grid, isUCC: true);
				}
			}
		}

		void AssertGridColumnStyleProperties(ZGrid grid, bool isUCC)
		{
			CombineAssertions(() =>
			{
				var csi_Code = grid.GetColumnStyle(SupportingDocument.Schema.CSI_Code);
				AssertEquals("CSI_Code: CharacterCasing", CharacterCasing.Upper, csi_Code.CharacterCasing);
				AssertEquals("CSI_Code: Visibility", true, csi_Code.IsVisible);
				AssertEquals("CSI_Code: Availability", false, csi_Code.IsUnavailable);

				var csi_ReferenceNumber = (ZMultiControlColumnStyleInfo)grid.GetColumnStyle(SupportingDocument.Schema.CSI_ReferenceNumber);
				AssertEquals("CSI_ReferenceNumber: FieldTypeColumnName", "ReferenceNumberFieldType", csi_ReferenceNumber.FieldTypeColumnName);
				AssertEquals("CSI_ReferenceNumber: CharacterCasing", CharacterCasing.Normal, csi_ReferenceNumber.CharacterCasing);
				AssertEquals("CSI_ReferenceNumber: Visibility", true, csi_ReferenceNumber.IsVisible);
				AssertEquals("CSI_ReferenceNumber: Availability", false, csi_ReferenceNumber.IsUnavailable);

				var csi_Description = grid.GetColumnStyle(SupportingDocument.Schema.CSI_Description);
				AssertEquals("CSI_Description: CharacterCasing", CharacterCasing.Normal, csi_Description.CharacterCasing);
				AssertEquals("CSI_Description: Visibility", true, csi_Description.IsVisible);
				AssertEquals("CSI_Description: Availability", false, csi_Description.IsUnavailable);

				var csi_IsDTP = grid.GetColumnStyle(SupportingDocument.Schema.CSI_IsDTP);
				AssertEquals("CSI_IsDTP: Visibility", true, csi_IsDTP.IsVisible);
				AssertEquals("CSI_IsDTP: Availability", false, csi_IsDTP.IsUnavailable);

				var csi_Quantity = ((ZCalcEditColumnStyleInfo)grid.GetColumnStyle(SupportingDocument.Schema.CSI_Quantity));
				AssertEquals("CSI_Quantity: BindToDecimalPlaces", "QuantityDecimalPlaces", csi_Quantity.BindToDecimalPlaces);
				AssertEquals("CSI_Quantity: Visibility", true, csi_Quantity.IsVisible);
				AssertEquals("CSI_Quantity: Availability", false, csi_Quantity.IsUnavailable);

				var csi_UnitOfQuantity = grid.GetColumnStyle(SupportingDocument.Schema.CSI_UnitOfQuantity);
				AssertEquals("CSI_UnitOfQuantity: CharacterCasing", CharacterCasing.Normal, csi_UnitOfQuantity.CharacterCasing);
				AssertEquals("CSI_UnitOfQuantity: Visibility", true, csi_UnitOfQuantity.IsVisible);
				AssertEquals("CSI_UnitOfQuantity: Availability", false, csi_UnitOfQuantity.IsUnavailable);

				var csi_Quantity2 = ((ZCalcEditColumnStyleInfo)grid.GetColumnStyle(SupportingDocument.Schema.CSI_Quantity2));
				AssertEquals("CSI_Quantity2: BindToDecimalPlaces", "Quantity2DecimalPlaces", csi_Quantity2.BindToDecimalPlaces);
				AssertEquals("CSI_Quantity2: Visibility", true, csi_Quantity2.IsVisible);
				AssertEquals("CSI_Quantity2: Availability", false, csi_Quantity2.IsUnavailable);

				var csi_UnitOfQuantity2 = grid.GetColumnStyle(SupportingDocument.Schema.CSI_UnitOfQuantity2);
				AssertEquals("CSI_UnitOfQuantity2: CharacterCasing", CharacterCasing.Normal, csi_UnitOfQuantity2.CharacterCasing);
				AssertEquals("CSI_UnitOfQuantity2: Visibility", true, csi_UnitOfQuantity2.IsVisible);
				AssertEquals("CSI_UnitOfQuantity2: Availability", false, csi_UnitOfQuantity2.IsUnavailable);

				var csi_Value = ((ZCalcEditColumnStyleInfo)grid.GetColumnStyle(SupportingDocument.Schema.CSI_Value));
				AssertEquals("CSI_Value: Decimals", 4, csi_Value.Decimals);
				AssertEquals("CSI_Value: Visibility", true, csi_Value.IsVisible);
				AssertEquals("CSI_Value: Availability", false, csi_Value.IsUnavailable);

				var csi_RX_NKCurrency = grid.GetColumnStyle(SupportingDocument.Schema.CSI_RX_NKCurrency);
				AssertEquals("CSI_RX_NKCurrency: CharacterCasing", CharacterCasing.Upper, csi_RX_NKCurrency.CharacterCasing);
				AssertEquals("CSI_RX_NKCurrency: Visibility", true, csi_RX_NKCurrency.IsVisible);
				AssertEquals("CSI_RX_NKCurrency: Availability", false, csi_RX_NKCurrency.IsUnavailable);

				var csi_DateOfIssue = (ZDateEditColumnStyleInfo)grid.GetColumnStyle(SupportingDocument.Schema.CSI_DateOfIssue);
				AssertEquals("CSI_DateOfIssue: Format", ZDateTimePickerFormat.Short, csi_DateOfIssue.DateTimeFormat);
				AssertEquals("CSI_DateOfIssue: Visibility", true, csi_DateOfIssue.IsVisible);
				AssertEquals("CSI_DateOfIssue: Availability", false, csi_DateOfIssue.IsUnavailable);

				var csi_DateOfExpiry = ((ZDateEditColumnStyleInfo)grid.GetColumnStyle(SupportingDocument.Schema.CSI_DateOfExpiry));
				AssertEquals("CSI_DateOfExpiry: Format", ZDateTimePickerFormat.Short, csi_DateOfExpiry.DateTimeFormat);
				AssertEquals("CSI_DateOfExpiry: Visibility", true, csi_DateOfExpiry.IsVisible);
				AssertEquals("CSI_DateOfExpiry: Availability", false, csi_DateOfExpiry.IsUnavailable);

				var csi_Quantity3 = grid.GetColumnStyle(SupportingDocument.Schema.CSI_Quantity3);
				AssertEquals("CSI_Quantity3: Visibility", !isUCC, csi_Quantity3.IsVisible);
				AssertEquals("CSI_Quantity3: Availability", isUCC, csi_Quantity3.IsUnavailable);

				var csi_LineNo = grid.GetColumnStyle(SupportingDocument.Schema.CSI_LineNo);
				AssertEquals("CSI_LineNo: Visibility", true, csi_LineNo.IsVisible);
				AssertEquals("CSI_LineNo: Availability", false, csi_LineNo.IsUnavailable);

				var csi_AdditionalDescription = grid.GetColumnStyle(SupportingDocument.Schema.CSI_AdditionalDescription);
				AssertEquals("CSI_AdditionalDescription: CharacterCasing", CharacterCasing.Normal, csi_AdditionalDescription.CharacterCasing);
				AssertEquals("CSI_AdditionalDescription: Visibility", true, csi_AdditionalDescription.IsVisible);
				AssertEquals("CSI_AdditionalDescription: Availability", false, csi_AdditionalDescription.IsUnavailable);

				var csi_ReferenceNumber2 = grid.GetColumnStyle(SupportingDocument.Schema.CSI_ReferenceNumber2);
				AssertEquals("CSI_ReferenceNumber2: CharacterCasing", CharacterCasing.Normal, csi_ReferenceNumber2.CharacterCasing);
				AssertEquals("CSI_ReferenceNumber2: Visibility", isUCC, csi_ReferenceNumber2.IsVisible);
				AssertEquals("CSI_ReferenceNumber2: Availability", !isUCC, csi_ReferenceNumber2.IsUnavailable);

				var csi_ItemNumber = grid.GetColumnStyle(SupportingDocument.Schema.CSI_ItemNumber);
				AssertEquals("csi_ItemNumber: Visibility", isUCC, csi_ItemNumber.IsVisible);
				AssertEquals("csi_ItemNumber: Availability", !isUCC, csi_ItemNumber.IsUnavailable);
			});
		}
	}
}
