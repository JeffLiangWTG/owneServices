using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.CH.Business.Testing;

internal class CusLineTariffDetailLookupsTest : BusinessObjectLookupsTestCase
{
	public void TestCusLineTariffDetailLookups()
	{
		var cusLineTariffDetail = Factory.New<CusLineTariffDetail>();
		var cusLineTariffDetailLookups = new CusLineTariffDetailLookups(cusLineTariffDetail);

		AssertNotNull("TariffCollection", cusLineTariffDetailLookups.TariffList);
	}

	public void TestTariffList()
	{
		var tariffCode1 = "290-001";
		const string description1 = "Description 1";
		var tariffCode2 = "290-02";
		const string description2 = "Description 2";
		var tariffCode3 = "280-001";
		const string description3 = "Description 3";

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
		var invoiceLine = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();

		var additionalTaxTariffHelper = new AdditionalTaxTariffTestHelper(Factory);
		var parentTariff = additionalTaxTariffHelper.CreateParentImportTariff();
		var tariff1 = additionalTaxTariffHelper.CreateAdditionalTaxTariffWithAdditionalCode(tariffCode1, tariffDescription: description1);
		var tariff2 = additionalTaxTariffHelper.CreateAdditionalTaxTariffWithAdditionalCode(tariffCode2, tariffDescription: description2);
		var tariff3 = additionalTaxTariffHelper.CreateAdditionalTaxTariffWithAdditionalCode(tariffCode3, tariffDescription: description3);

		invoiceLine.JI_CountryOfOrigin = AdditionalTaxTariffTestHelper.DefaultTradeGroupCountry;
		invoiceLine.JI_Tariff = parentTariff.ZZ1_TariffCode;

		var cusLineTariffDetail290 = invoiceLine.AdditionalTaxes.Cast<CusLineTariffDetail>().Single(at => at.BZ_TaxType == "290");
		var cusLineTariffDetail280 = invoiceLine.AdditionalTaxes.Cast<CusLineTariffDetail>().Single(at => at.BZ_TaxType == "280");

		var cusLineTariffDetailLookups290 = new CusLineTariffDetailLookups(cusLineTariffDetail290);
		var cusLineTariffDetailLookups280 = new CusLineTariffDetailLookups(cusLineTariffDetail280);

		CombineAssertions("Tariff 290", () =>
		{
			AssertEquals("Count", 2, cusLineTariffDetailLookups290.TariffList.Count);
			AssertEquals("Item 1 code", tariffCode1, cusLineTariffDetailLookups290.TariffList[0].Code);
			AssertEquals("Item 1 description", description1, cusLineTariffDetailLookups290.TariffList[0].Description);
			AssertEquals("Item 2 code", tariffCode2, cusLineTariffDetailLookups290.TariffList[1].Code);
			AssertEquals("Item 2 description", description2, cusLineTariffDetailLookups290.TariffList[1].Description);
		});

		CombineAssertions("Tariff 280", () =>
		{
			AssertEquals("Count", 1, cusLineTariffDetailLookups280.TariffList.Count);
			AssertEquals("Item 1 code", tariffCode3, cusLineTariffDetailLookups280.TariffList[0].Code);
			AssertEquals("Item 1 description", description3, cusLineTariffDetailLookups280.TariffList[0].Description);
		});
	}
}
