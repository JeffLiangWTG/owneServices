using CargoWise.EntityFramework;
using Enterprise.Customs.Universal.Testing;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	[TestedType(typeof(JobComInvoiceLineTax))]
	public class JobComInvoiceLineTaxTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var dec = Factory.New<JobDeclaration>();
			var invHeader = dec.Invoices.AddNew();
			var invLine = invHeader.InvoiceLines.AddNew();
			var frTax = invLine.Taxes.AddNew();
			return frTax;
		}

		public void TestJLT_MethodOfCalculationNotReadonly()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			var taxLine = invoiceLine.Taxes.AddNew();

			declaration.JE_MessageType = "IMP";
			invoiceLine.JI_TariffBypassCode = "X";

			AssertEquals("Method of Calc should not be readonly", false, taxLine.JLT_MethodOfCalculationInfo.ReadOnly);
		}

		public void TestTariffBypassCodeFromInvoiceLineTariffBypassCode()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();

			declaration.JE_MessageType = "IMP";
			invoiceLine.JI_TariffBypassCode = "D";

			var taxLine = invoiceLine.Taxes.AddNew();

			AssertEquals("Taxline Tariff Bypass Code  should be same as invoice line JI_TariffBypassCode", invoiceLine.JI_TariffBypassCode, taxLine.TariffBypassCode);
		}

		public void TestTaxTypeLookup()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var euGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.France, "France", euGrouping);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Spain, "Spain", euGrouping);
			CusRefRateCodeViewTestHelper.CreateAndSaveCusRateCodeForCountryIfNotExists(Factory, Core.Constants.CountryCodes.Spain, Customs.Business.UniversalReferenceConstants.RefCusRateTypes.Dty, "ZZZZ", true, false, "ZZZZ_Description");
			CusRefRateCodeViewTestHelper.CreateAndSaveCusRateCodeForCountryIfNotExists(Factory, Core.Constants.CountryCodes.France, Customs.Business.UniversalReferenceConstants.RefCusRateTypes.Dty, UniversalReferenceConstants.RefCusRateCodes.U165, true, false, "U165_Description");
			CusRefRateCodeViewTestHelper.CreateAndSaveCusRateCodeForCountryIfNotExists(Factory, Core.Constants.CountryCodes.France, Customs.Business.UniversalReferenceConstants.RefCusRateTypes.Dty, UniversalReferenceConstants.RefCusRateCodes.U167, true, false, "U167_Description");
			CusRefRateCodeViewTestHelper.CreateAndSaveCusRateCodeForCountryIfNotExists(Factory, Core.Constants.CountryCodes.France, "DEV", UniversalReferenceConstants.RefCusRateCodes.Q416, true, false, "Q416_Description");

			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			var taxLine = invoiceLine.Taxes.AddNew();
			var nationalFeeTypeCodeList = new NationalFeeTypeCodeList(Factory);
			AssertNotNull("Missing National Fee Type code pair list.", nationalFeeTypeCodeList);
			Assert("National Fee Type code pair list should not be empty", nationalFeeTypeCodeList.Count > 0);
			AssertEquals("Tax line Type code lookup shoud be tied to the National Fee Type code pair list", taxLine.Lookups.TypeList, nationalFeeTypeCodeList);
		}

		public void TestMethodOfCalculationLookup()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			var taxLine = invoiceLine.Taxes.AddNew();
			var methodOfCalculationCodePairList = new MethodOfCalculationList();
			AssertNotNull("Missing Method Of Calculation code pair list.", methodOfCalculationCodePairList);
			Assert("Method Of Calculation code pair list should not be empty", methodOfCalculationCodePairList.Count > 0);
			AssertEquals("Tax line Method of Calculation lookup shoud be tied to the Method Of Calculation code pair list ", taxLine.Lookups.MethodOfCalculationList, methodOfCalculationCodePairList);
		}

		public void TestDefaultValues()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_TariffBypassCode = "G";
			var taxLine = invoiceLine.Taxes.AddNew();
			AssertEquals("Tax line Tariff bypass code doesn't match invoice line Tariff bypass code", "G", taxLine.TariffBypassCode);
			AssertNullOrEmpty("Tax line method of calculation should be empty.", taxLine.JLT_MethodOfCalculation);
		}
	}
}
