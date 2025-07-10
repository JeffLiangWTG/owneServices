using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.BR;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.BR.Business.Duimp.Testing
{
	class SellingConditionProviderTest : TestCaseWithFactory
	{
		public void TestNew()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertNull(SellingConditionProvider.New(null, null));
			var invoice = declaration.Invoices.AddNew();
			AssertNull(SellingConditionProvider.New(invoice, null));
			var entryLine = declaration.ActiveEntryHeaders.AddNew().AllEntryLines.AddNew();
			AssertNull(SellingConditionProvider.New(null, entryLine));
			AssertType<SellingConditionProvider>(SellingConditionProvider.New(invoice, entryLine));
		}

		public void TestSellingCondition()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			var entryLine = declaration.ActiveEntryHeaders.AddNew().AllEntryLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			var dataProvider = SellingConditionProvider.New(invoice, entryLine);
			CombineAssertions(() =>
			{
				AssertEquals("ValuationMethodCode should be Zero", 0, dataProvider.ValuationMethodCode);
				AssertEquals("IncoTermCode should be Empty", ZString.Empty, dataProvider.IncoTermCode);
				AssertEquals("IncoTermComplement should be Empty", ZString.Empty, dataProvider.IncoTermComplement);
			});

			invoice.JZ_ValuationCode = ValuationCodeList.Codes._01;
			invoice.JZ_IncoTerm = BRIncoTermList.Codes.FOB;
			invoice.JZ_AdditionalTerms = "Test Additional Terms";
			invoiceLine.ComplementaryDescription = "Test Complement";
			dataProvider = SellingConditionProvider.New(invoice, entryLine);
			CombineAssertions(() =>
			{
				AssertEquals("ValuationMethodCode", 1, dataProvider.ValuationMethodCode);
				AssertEquals("IncoTermCode", BRIncoTermList.Codes.FOB, dataProvider.IncoTermCode);
				AssertEquals("IncoTermComplement", "Test Additional Terms", dataProvider.IncoTermComplement);
			});
		}

		public void TestAdditionsDeductions()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var entryLine = declaration.ActiveEntryHeaders.AddNew().AllEntryLines.AddNew();
			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			invoiceLine1.JI_CL = entryLine.PK;
			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_CL = entryLine.PK;

			var dataProvider = SellingConditionProvider.New(invoice, entryLine);
			AssertEquals("AdditionsDeductions count should be Zero", 0, dataProvider.AdditionsDeductions.Count());

			var charge = invoiceLine1.Charges.AddNew();
			dataProvider = SellingConditionProvider.New(invoice, entryLine);
			AssertEquals("AdditionsDeductions count should be Zero", 0, dataProvider.AdditionsDeductions.Count());

			charge.J7_ChargeType = ImportCustomsChargeTypeList.Codes.OtherExpensesICMS;
			dataProvider = SellingConditionProvider.New(invoice, entryLine);
			AssertEquals("AdditionsDeductions.Count", 0, dataProvider.AdditionsDeductions.Count());

			invoiceLine1.Charges.AddNew(ImportCustomsChargeTypeList.Codes.InternalFreightImportingCountry, 10m, "USD");
			invoiceLine2.Charges.AddNew(ImportCustomsChargeTypeList.Codes.InternalFreightImportingCountry, 10m, "USD");
			dataProvider = SellingConditionProvider.New(invoice, entryLine);
			AssertEquals("AdditionsDeductions.Count", 1, dataProvider.AdditionsDeductions.Count());
			AssertEquals("AdditionsDeduction.Amount", 20d, dataProvider.AdditionsDeductions.ElementAt(0).Amount);

			invoiceLine1.Charges.AddNew(ImportCustomsChargeTypeList.Codes.InternalFreightImportingCountry, 5m, "BRL");
			invoiceLine2.Charges.AddNew(ImportCustomsChargeTypeList.Codes.InternalFreightImportingCountry, 5m, "BRL");
			dataProvider = SellingConditionProvider.New(invoice, entryLine);
			AssertEquals("AdditionsDeductions.Count", 2, dataProvider.AdditionsDeductions.Count());
			AssertEquals("AdditionsDeduction.Amount", 20d, dataProvider.AdditionsDeductions.Single(x => x.CurrencyCode == "USD").Amount);
			AssertEquals("AdditionsDeduction.Amount", 10d, dataProvider.AdditionsDeductions.Single(x => x.CurrencyCode == "BRL").Amount);

			invoiceLine1.ApportionedCharges.AddNew(ImportCustomsChargeTypeList.Codes.RightsOtherTaxes, 15m, "USD");
			dataProvider = SellingConditionProvider.New(invoice, entryLine);
			AssertEquals("AdditionsDeductions.Count", 3, dataProvider.AdditionsDeductions.Count());

			invoiceLine1.ApportionedCharges.AddNew(ImportCustomsChargeTypeList.Codes.FreightComponents, 0m);
			dataProvider = SellingConditionProvider.New(invoice, entryLine);
			AssertEquals("AdditionsDeductions.Count", 3, dataProvider.AdditionsDeductions.Count());
		}

		public void TestFreightAndInsuranceInLocalCurrency()
		{
			var newCurrency = RefCurrency.New(Factory);
			newCurrency.RX_Code = Core.Constants.CurrencyCodes.UnitedStates;
			newCurrency.SetCustomsRate(ZDateTime.Today.AddDays(-10), ZDateTime.Today.AddDays(10), 0.5m);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			declaration.JE_DispatchModality = "1";

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.UnitedStates;

			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			invoiceLine1.Charges.AddNew(ImportCommonChargesProvider.OverseasFreightCollect.Code, 50m, Core.Constants.CurrencyCodes.UnitedStates);
			invoiceLine1.Charges.AddNew(ImportCommonChargesProvider.OverseasFreightPrepaid.Code, 20m, Core.Constants.CurrencyCodes.UnitedStates);

			invoiceLine1.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasInsurance, 30m, Core.Constants.CurrencyCodes.UnitedStates);

			var entryLine = declaration.CustomsEntryHeaders.AddNew().MergedLines.AddNew();

			entryLine.InvoiceLines.Add(invoiceLine1);

			var dataProvider = SellingConditionProvider.New(invoice, entryLine);
			AssertEquals("FreightInLocalCurrency should be", 140d, dataProvider.FreightInLocalCurrency);
			AssertEquals("InsuranceInLocalCurrency should be", 60d, dataProvider.InsuranceInLocalCurrency);

			declaration.JE_DispatchModality = string.Empty;
			dataProvider = SellingConditionProvider.New(invoice, entryLine);
			AssertNull("FreightInLocalCurrency should be Null", dataProvider.FreightInLocalCurrency);
			AssertNull("InsuranceInLocalCurrency should be Null", dataProvider.InsuranceInLocalCurrency);
		}
	}
}
