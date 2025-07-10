using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.IN.Business.Testing;

sealed class ExtensionMethodsTest : TestCaseWithFactory
{
	public void TestGetIncoTermChargeFactoryCacheKey_JobDeclaration()
	{
		CombineAssertions(() =>
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals("Import", "", declaration.GetIncoTermChargeFactoryCacheKey());
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals("Export", "EXP", declaration.GetIncoTermChargeFactoryCacheKey());
		});
	}

	public void TestGetCustomsChargeTypeListCacheKey_JobComInvoiceGroupHeader()
	{
		CombineAssertions(() =>
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals("Import", "", groupInvoice.GetCustomsChargeTypeListCacheKey());
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals("Export", "EXP", groupInvoice.GetCustomsChargeTypeListCacheKey());
		});
	}

	public void TestGetCustomsChargeTypeListCacheKey_JobComInvoiceHeader()
	{
		CombineAssertions(() =>
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals("Import", "", invoice.GetCustomsChargeTypeListCacheKey());
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals("Export", "EXP", invoice.GetCustomsChargeTypeListCacheKey());
		});
	}

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
		groupInvoice = declaration.JobComInvoiceGroupHeaders[0];
		invoice = declaration.Invoices.AddNew();
	}

	JobDeclaration declaration;
	JobComInvoiceGroupHeader groupInvoice;
	JobComInvoiceHeader invoice;
}
