using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Testing
{
	public class InsuranceRuleEngineTest : TestCaseWithFactory
	{
		[TestDate(2024, 11, 12)]
		public void TestGetInsuranceFlatValue()
		{
			(var rule, var declaration) = GetInsuranceData();
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 900m;
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.EuropeanUnion;
			var flatrate = InsuranceRuleEngine.GetInsuranceFlatValue(declaration, invoice);
			AssertEquals("Flat rate fetching", 11m, flatrate);
		}

		[TestDate(2024, 11, 12)]
		public void TestGetInsuranceUpliftPercent()
		{
			(var rule, var declaration) = GetInsuranceData();
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 1100m;
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.EuropeanUnion;
			var flatrate = InsuranceRuleEngine.GetInsuranceFlatValue(declaration, invoice);
			AssertEquals("Flat rate fetching", 0m, flatrate);
			var percentRate = InsuranceRuleEngine.GetInsuranceUpliftPercent(declaration, invoice);
			AssertEquals("Uplift rate fetching from Calculatrion Rule", 5.3m, percentRate);

			var supplier = Factory.NewWithValidTestData<OrgHeader>();
			declaration.JE_OH_Supplier = supplier.PK;
			var link = declaration.Importer.SupplierLinks.AddNew();
			link.OL_OH_Supplier = declaration.JE_OH_Supplier;
			link.OL_InsuranceUplift = 10.5;

			declaration.JE_TransportMode = TransportTypeList.Codes.Sea; // Rule is not queried

			Factory.Save();

			percentRate = InsuranceRuleEngine.GetInsuranceUpliftPercent(declaration, invoice);
			AssertEquals("Uplift rate fetching from Org links", 10.5m, percentRate);
		}

		(CusCalculationRule, JobDeclaration) GetInsuranceData()
		{
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			var insurance1 = Factory.NewWithValidTestData<CusCalculationRule>();
			insurance1.CCR_StartDate = new ZDateTime(2024, 11, 01).ToOffset();
			insurance1.CCR_EndDate = new ZDateTime(2024, 11, 30).ToOffset();
			insurance1.CCR_OH_Importer = importer.PK;
			insurance1.CCR_TransportMode = TransportTypeList.Codes.Air;
			insurance1.CCR_RX_NKCurrency = Core.Constants.CurrencyCodes.EuropeanUnion;
			var rate = insurance1.CalculationRuleRateCollection.AddNew();
			rate.ValueFrom = 0;
			rate.FlatRate = 11;
			var rate2 = insurance1.CalculationRuleRateCollection.AddNew();
			rate2.ValueFrom = 1000;
			rate2.Uplift = 5.3;
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			return (insurance1, declaration);
		}
	}
}
