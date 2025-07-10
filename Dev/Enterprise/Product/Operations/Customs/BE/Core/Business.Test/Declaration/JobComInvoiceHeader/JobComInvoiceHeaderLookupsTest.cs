using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.BE.Business.Declaration.Testing;

sealed class JobComInvoiceHeaderLookupsTest : Customs.Business.Testing.JobComInvoiceHeaderLookupsTest
{
	public void TestInvoice()
	{
		var parent = Factory.New<JobComInvoiceHeader>();
		AssertEquals(parent.Lookups.Invoice, parent);
	}

	public void TestValuationCodeList()
	{
		var lookups = new JobComInvoiceHeaderLookups(Factory.New<JobComInvoiceHeader>());
		var codeList = (CodeDescriptionPairList)lookups.ValuationCodeList;
		CombineAssertions(() =>
		{
			AssertEquals("CodesAsString", codeList.CodesAsString, "1, 11, 12, 2, 21, 22, 23, 3, 31, 32, 33, 34, 4, 41, 42, 5, 51, 52, 6, 7, 71, 72, 8, 9, 91, 99");
			AssertSame("Cached", codeList, lookups.ValuationCodeList);
		});
	}
}
