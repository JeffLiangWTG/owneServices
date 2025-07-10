using System.Linq;
using Enterprise.Customs.GUI;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;
using static Enterprise.Customs.EU.Business.EUCommonConstants;

namespace Enterprise.Customs.IE.GUI.Testing
{
	[TestedType(typeof(ImportInvoiceLineCalculationsLayoutBuilder))]
	sealed class ImportInvoiceLineCalculationsLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<ImportInvoiceLineCalculationsLayoutBuilder, JobComInvoiceLine, CommonInvoiceLineCalculationsControlBag>
	{
		protected override int ExpectedMaxColumns => 2;

		protected override ImportInvoiceLineCalculationsLayoutBuilder GetColumnLayoutBuilderForTesting() => new ImportInvoiceLineCalculationsLayoutBuilder();

		public void TestCurrentInvoiceLabel()
		{
			AssertEquals("To make sure of a correct CEI_Style.", false, entryInstruction.IsH2);
			AssertEquals(true, layout.IsVisible(CommonInvoiceLineCalculationsControlBag.Instance.CurrentInvoiceLabel, invoiceLine));

			entryInstruction.CEI_Style = ImportDeclarationTypeList.H2;
			AssertEquals(true, layout.IsVisible(CommonInvoiceLineCalculationsControlBag.Instance.CurrentInvoiceLabel, invoiceLine));
		}

		public void TestBalanceConvertToLocalCurrencyControl()
		{
			AssertEquals(true, layout.IsVisible(CommonInvoiceLineCalculationsControlBag.Instance.BalanceConvertToLocalCurrencyControl, invoiceLine));
		}

		public void TestLinesEnteredConvertToLocalCurrencyControl()
		{
			AssertEquals(true, layout.IsVisible(CommonInvoiceLineCalculationsControlBag.Instance.LinesEnteredConvertToLocalCurrencyControl, invoiceLine));
		}

		public void TestLinesTotalConvertToLocalCurrencyControl()
		{
			AssertEquals(true, layout.IsVisible(CommonInvoiceLineCalculationsControlBag.Instance.LinesTotalConvertToLocalCurrencyControl, invoiceLine));
		}

		public void TestSummaryLabel()
		{
			AssertEquals("To make sure of a correct CEI_Style.", false, entryInstruction.IsH2);
			AssertEquals(true, layout.IsVisible(CommonInvoiceLineCalculationsControlBag.Instance.SummaryLabel, invoiceLine));

			entryInstruction.CEI_Style = ImportDeclarationTypeList.H2;
			AssertEquals(true, layout.IsVisible(CommonInvoiceLineCalculationsControlBag.Instance.SummaryLabel, invoiceLine));
		}

		public void TestDutyAmountIncludingWHEstimateConvertToLocalCurrencyControl()
		{
			AssertEquals("To make sure of a correct CEI_Style.", false, entryInstruction.IsH2);
			AssertEquals(true, layout.IsVisible(CommonInvoiceLineCalculationsControlBag.Instance.DutyAmountIncludingWHEstimateConvertToLocalCurrencyControl, invoiceLine));

			entryInstruction.CEI_Style = ImportDeclarationTypeList.H2;
			AssertEquals(false, layout.IsVisible(CommonInvoiceLineCalculationsControlBag.Instance.DutyAmountIncludingWHEstimateConvertToLocalCurrencyControl, invoiceLine));
		}

		public void TestGSTVATAmountIncludingWHEstimateConvertToLocalCurrencyControl()
		{
			AssertEquals("To make sure of a correct CEI_Style.", false, entryInstruction.IsH2);
			AssertEquals(true, layout.IsVisible(CommonInvoiceLineCalculationsControlBag.Instance.GSTVATAmountIncludingWHEstimateConvertToLocalCurrencyControl, invoiceLine));

			entryInstruction.CEI_Style = ImportDeclarationTypeList.H2;
			AssertEquals(false, layout.IsVisible(CommonInvoiceLineCalculationsControlBag.Instance.GSTVATAmountIncludingWHEstimateConvertToLocalCurrencyControl, invoiceLine));
		}

		public void TestJGSTVATDeferredConvertToLocalCurrencyControl()
		{
			AssertEquals("To make sure of a correct CEI_Style.", false, entryInstruction.IsH2);
			AssertEquals(true, layout.IsVisible(EU.GUI.InvoiceLineSummaryControlBag.Instance.GSTVATDeferredConvertToLocalCurrencyControl, invoiceLine));

			entryInstruction.CEI_Style = ImportDeclarationTypeList.H2;
			AssertEquals(false, layout.IsVisible(EU.GUI.InvoiceLineSummaryControlBag.Instance.GSTVATDeferredConvertToLocalCurrencyControl, invoiceLine));
		}

		public void TestCustomsValueConvertToLocalCurrencyControl()
		{
			AssertEquals("To make sure of a correct CEI_Style.", false, entryInstruction.IsH2);
			AssertEquals(true, layout.IsVisible(EU.GUI.InvoiceLineSummaryControlBag.Instance.CustomsValueConvertToLocalCurrencyControl, invoiceLine));

			entryInstruction.CEI_Style = ImportDeclarationTypeList.H2;
			AssertEquals(false, layout.IsVisible(EU.GUI.InvoiceLineSummaryControlBag.Instance.CustomsValueConvertToLocalCurrencyControl, invoiceLine));
		}

		public void TestValueForVatConvertToLocalCurrencyControl()
		{
			AssertEquals("To make sure of a correct CEI_Style.", false, entryInstruction.IsH2);
			AssertEquals(true, layout.IsVisible(EU.GUI.InvoiceLineSummaryControlBag.Instance.ValueForVatConvertToLocalCurrencyControl, invoiceLine));

			entryInstruction.CEI_Style = ImportDeclarationTypeList.H2;
			AssertEquals(false, layout.IsVisible(EU.GUI.InvoiceLineSummaryControlBag.Instance.ValueForVatConvertToLocalCurrencyControl, invoiceLine));
		}

		public void TestStatisticalValueConvertToLocalCurrencyControl()
		{
			AssertEquals("To make sure of a correct CEI_Style.", false, entryInstruction.IsH2);
			AssertEquals(true, layout.IsVisible(EU.GUI.InvoiceLineSummaryControlBag.Instance.StatisticalValueConvertToLocalCurrencyControl, invoiceLine));

			entryInstruction.CEI_Style = ImportDeclarationTypeList.H2;
			AssertEquals(true, layout.IsVisible(EU.GUI.InvoiceLineSummaryControlBag.Instance.StatisticalValueConvertToLocalCurrencyControl, invoiceLine));
		}

		public void TestCIFConvertToLocalCurrencyControl()
		{
			AssertEquals("To make sure of a correct CEI_Style.", false, entryInstruction.IsH2);
			AssertEquals(true, layout.IsVisible(CommonInvoiceLineCalculationsControlBag.Instance.CIFConvertToLocalCurrencyControl, invoiceLine));

			entryInstruction.CEI_Style = ImportDeclarationTypeList.H2;
			AssertEquals(false, layout.IsVisible(CommonInvoiceLineCalculationsControlBag.Instance.CIFConvertToLocalCurrencyControl, invoiceLine));
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			entryInstruction = declaration.CustomsEntryInstructions.FirstOrDefault() ?? declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_Style = ImportDeclarationTypeList.H1;
			var invoiceHeader = (JobComInvoiceHeader)declaration.Invoices.FirstOrDefault() ?? declaration.Invoices.AddNew();
			invoiceLine = (JobComInvoiceLine)declaration.InvoiceLines.FirstOrDefault() ?? declaration.InvoiceLines.AddNew();
			layout = ((IPanelLayoutProvider)new ImportInvoiceLineCalculationsLayout()).Layout;
		}

		JobDeclaration declaration;
		CusEntryInstruction entryInstruction;
		JobComInvoiceLine invoiceLine;
		PanelLayout layout;
	}
}
