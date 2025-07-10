using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.BR.Business;
using Enterprise.Customs.Common.BR;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.BR.GUI.Testing
{
	class SpecialCasesUserControlTest : TestCaseWithFactory
	{
		public void TestDataSourceType()
		{
			using (var control = new SpecialCasesUserControl())
			{
				AssertEquals("DataSourceType", typeof(JobComInvoiceLine), control.DataSourceType);
			}
		}

		public void TestGrid()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;

			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			var specialCaseTax = invoiceLine.SpecialCaseTaxes.AddNew();
			specialCaseTax.TaxGroup = Constants.RateCodes.PIS;

			using (var testForm = new JobDeclarationForm(declaration))
			{
				testForm.Show();
				var brokerageControl = testForm.CustomsBrokerageUserControl;
				brokerageControl.MainTabControl.SelectedTab = brokerageControl.InvoiceLinesTabPage;
				using (var control = brokerageControl.InvoiceLinesUserControl as ImportSiscomexInvoiceLineUserControl)
				{
					control.LineDetailTabControl.SelectedTab = control.SpecialCasesTabPage;
					using (var specialCasesControl = control.SpecialCasesUserControl)
					{
						AssertEquals("SpecialCasesGroupBox caption must be", "Special Cases", specialCasesControl.SpecialCasesGroupBox.CaptionResourceString.Caption);
						AssertEquals("Should have TaxGroup column", true, specialCasesControl.SpecialCasesGrid.Columns.Contains(nameof(SpecialCaseTax.TaxGroup)));
						AssertEquals("Should have TaxGroupDescription column", true, specialCasesControl.SpecialCasesGrid.Columns.Contains(nameof(SpecialCaseTax.TaxGroupDescription)));
						AssertEquals("Should have TaxType column", true, specialCasesControl.SpecialCasesGrid.Columns.Contains(nameof(SpecialCaseTax.TaxType)));
						AssertEquals("Should have RateOrUnitValue column", true, specialCasesControl.SpecialCasesGrid.Columns.Contains(nameof(SpecialCaseTax.RateOrUnitValue)));
					}
				}
			}
		}

		public void TestComponents()
		{
			using (var control = new SpecialCasesUserControl())
			{
				CombineAssertions(() =>
				{
					AssertType<ZGroupBox>("SpecialCasesGroupBox must be ZGroupBox", control.SpecialCasesGroupBox);
					AssertType<ZGrid>("SpecialCasesGrid must be ZGrid", control.SpecialCasesGrid);
					AssertType<ZDropEdit>("TaxGroupDropEdit must be ZDropEdit", control.TaxGroupDropEdit);
					AssertType<ZDropEdit>("TaxTypeDropEdit must be ZDropEdit", control.TaxTypeDropEdit);
					AssertType<ZCalcEdit>("RateOrUnitValueCalcEdit must be ZCalcEdit", control.RateOrUnitValueCalcEdit);
					AssertType<ZCodeFindBox>("CurrencyCodeFindBox must be ZCodeFindBox", control.CurrencyCodeFindBox);
					AssertType<ZCalcEdit>("QuantityCalcEdit must be ZCalcEdit", control.QuantityCalcEdit);
					AssertType<ZDropEdit>("UnitOfMeasureDropEdit must be ZDropEdit", control.UnitOfMeasureDropEdit);
					AssertType<ZDropEdit>("LegalActTypeDropEdit must be ZDropEdit", control.LegalActTypeDropEdit);
					AssertType<ZDropEdit>("LegalActIssuingBodyDropEdit must be ZDropEdit", control.LegalActIssuingBodyDropEdit);
					AssertType<ZTextBox>("LegalActNumberTextBox must be ZTextBox", control.LegalActNumberTextBox);
					AssertType<ZTextBox>("LegalActYearTextBox must be ZTextBox", control.LegalActYearTextBox);
				});
			}
		}
	}
}

