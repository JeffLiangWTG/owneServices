using CargoWise.EntityFramework;
using Enterprise.Customs.BE.Business.Declaration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.BE.Business.Testing;

[TestedType(typeof(CusAuthorizationUsage))]
sealed class CusAuthorizationUsageTest : EnterpriseBusinessObjectTestCase
{
	public void TestLookups()
	{
		AssertType<CusAuthorizationUsageLookups>(Factory.New<CusAuthorizationUsage>().Lookups);
	}

	protected override BusinessObject GetNewBusinessObject() => GetCusAuthorizationUsage(Factory);

	protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetCusAuthorizationUsage(factory);

	static CusAuthorizationUsage GetCusAuthorizationUsage(BusinessObjectFactory factory)
	{
		var orgHeader = factory.NewWithValidTestData<OrgHeader>();
		var cusAuthorizationUsage = factory.New<JobDeclaration>().Invoices.AddNew().InvoiceLines.AddNew().CusAuthorizationUsages.AddNew();
		cusAuthorizationUsage.AGC_Number = "123";
		cusAuthorizationUsage.AGC_OH_Owner = orgHeader.PK;
		cusAuthorizationUsage.AGC_Code = "BOI";
		return cusAuthorizationUsage;
	}
}
