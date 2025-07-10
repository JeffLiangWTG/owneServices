using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.DE.Business;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.EU.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.DE.GUI.Testing
{
	public class ImportInvoiceLineInwardProcessingUserControlTest : TestCaseWithFactory
	{
		public void TestFields()
		{
			CombineAssertions(() =>
			{
				using (var control = new ImportInvoiceLineInwardProcessingUserControl())
				{
					AssertEquals("EconomicConditionsDropEdit", true, control.FindSingle<ZDropEdit>("EconomicConditionsDropEdit").Visible);
					AssertEquals("ExtraInfoForClassificationTextBox", true, control.FindSingle<ZTextBox>("ExtraInfoForClassificationTextBox").Visible);
					AssertEquals("IdentificationMeansTypeDropEdit", true, control.FindSingle<ZDropEdit>("IdentificationMeansTypeDropEdit").Visible);
				}
			});
		}

		public void TestColumns()
		{
			CombineAssertions(() =>
			{
				using (var control = new ImportInvoiceLineInwardProcessingUserControl())
				{
					var grid = control.FindSingle<ZGrid>("ProcessedProductGrid");
					AssertEquals("ColumnStyles.Count", 4, grid.ColumnStyles.Count);

					AssertType<Universal.GUI.TariffColumnStyleInfo>("FormattedTariff: Type", grid.GetColumnStyle("FormattedTariff"));
					AssertType<ZDropEditColumnStyleInfo>("CSI_SubType: Type", grid.GetColumnStyle("CSI_SubType"));

					var csi_descriptionColumnStyle = grid.GetColumnStyle("CSI_Description");
					AssertType<ZTextBoxColumnStyleInfo>("CSI_Description: Type", csi_descriptionColumnStyle);
					AssertEquals("CSI_Description: CharacterCasing", CharacterCasing.Normal, csi_descriptionColumnStyle.CharacterCasing);

					var csi_additionalDescriptionColumnStyle = grid.GetColumnStyle("CSI_AdditionalDescription");
					AssertType<ZMultiLineTextBoxColumnInfo>("CSI_AdditionalDescription: Type", csi_additionalDescriptionColumnStyle);
					AssertEquals("CSI_AdditionalDescription: CharacterCasing", CharacterCasing.Normal, csi_additionalDescriptionColumnStyle.CharacterCasing);
				}
			});
		}

		public void TestColumnFormattedTariff()
		{
			CombineAssertions(() =>
			{
				using (var control = new ImportInvoiceLineInwardProcessingUserControl())
				{
					var grid = control.FindSingle<ZGrid>("ProcessedProductGrid");
					var tariffColumnStyleInfo = (Universal.GUI.TariffColumnStyleInfo)grid.GetColumnStyle("FormattedTariff");

					AssertEquals("GetCountryCode", Core.Constants.CountryCodes.Germany, tariffColumnStyleInfo.GetCountryCode());
					AssertEquals("GetDataGrouping", Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, tariffColumnStyleInfo.GetDataGrouping());
					AssertEquals("TariffType", Customs.Business.UniversalReferenceConstants.CusTariffTypes.ExportTariff, tariffColumnStyleInfo.TariffType);
				}
			});
		}

		public void TestJI_ExtraInfoForClassification_CharacterCasing()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine.JI_ExtraInfoForClassification = UpperLowerTestString;

			using (var form = new ZForm(declaration))
			using (var control = new ImportInvoiceLineInwardProcessingUserControl())
			{
				form.Controls.Add(control);
				form.Show();

				var descriptionTextBox = control.FindSingle<ZTextBox>("ExtraInfoForClassificationTextBox");
				AssertEquals(UpperLowerTestString, descriptionTextBox.Text);
			}
		}

		public void TestProcessedProductGrid_NormalCharacterCasing()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			var inwardProcessingProduct = declaration.Invoices.AddNew().InvoiceLines.AddNew().InwardProcessingProducts.AddNew();
			inwardProcessingProduct.CSI_AdditionalDescription = UpperLowerTestString;
			inwardProcessingProduct.CSI_Description = UpperLowerTestString;

			using (var form = new ZForm(declaration))
			using (var control = new ImportInvoiceLineInwardProcessingUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				var grid = control.FindSingle<ZGrid>("ProcessedProductGrid");
				grid.Select(0);
				var row = (InwardProcessingProduct)grid.GetFirstSelectedRow();
				CombineAssertions(() =>
				{
					AssertEquals("CSI_Description", UpperLowerTestString, row.CSI_Description);
					AssertEquals("CSI_AdditionalDescription", UpperLowerTestString, row.CSI_AdditionalDescription);
				});
			}
		}

		const string UpperLowerTestString = "UPPERlower";
	}
}
