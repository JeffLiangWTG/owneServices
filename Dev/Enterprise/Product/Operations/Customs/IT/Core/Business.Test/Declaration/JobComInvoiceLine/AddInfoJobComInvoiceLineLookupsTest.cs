using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IT.Business.Testing;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

sealed class AddInfoJobComInvoiceLineLookupsTest : BusinessObjectLookupsTestCase
{
	public void TestSteelTypeList()
	{
		var steelTypeList = SetupAddInfoLookups().SteelTypeList;
		CombineAssertions(() =>
		{
			AssertEquals("Codes", "0, 1, 2, 3, 4", steelTypeList.CodesAsString);
			AssertSame("Cache", Factory.GetCachedValue<SteelTypeList>(), steelTypeList);
		});
	}

	public void TestPortTaxRateList()
	{
		new ITUniversalReferenceTestDataHelper(Factory).SetupPortTaxRates();
		Factory.Save();
		var portTaxRateList = SetupAddInfoLookups().PortTaxRateList;
		AssertEquals("Codes", "A1, A2, A3", portTaxRateList.CodesAsString);
	}

	AddInfoJobComInvoiceLineLookups SetupAddInfoLookups()
	{
		var jobComInvoiceLineAddInfo = new AddInfoJobComInvoiceLine(Factory.New<JobComInvoiceLine>().JI_AddInfoInfo);
		return new AddInfoJobComInvoiceLineLookups(jobComInvoiceLineAddInfo);
	}
}
