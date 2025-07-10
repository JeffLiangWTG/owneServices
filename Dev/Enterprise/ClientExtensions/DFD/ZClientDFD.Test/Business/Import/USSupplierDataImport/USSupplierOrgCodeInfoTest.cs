using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.DFD.Business.Import.Testing
{
	class USSupplierOrgCodeInfoTest : TestCaseWithFactory
	{
		public void TestProperties()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_RL_NKClosestPort = "AUSYD";
			var codeInfo = new USSupplierOrgCodeInfo(org);
			AssertNotNull(codeInfo);
			AssertEquals("AU", codeInfo.CountryCode);
			AssertEquals("SYD", codeInfo.IataCode);
			Assert(!codeInfo.IsCreditorForAnyCompany);
			Assert(!codeInfo.IsDebtorForAnyCompany);
			AssertEquals(org.OH_Code, codeInfo.OH_Code);
			AssertEquals(org.OH_IsBroker, codeInfo.OH_IsBroker);
			AssertEquals(org.OH_IsCompetitor, codeInfo.OH_IsCompetitor);
			AssertEquals(org.OH_IsConsignee, codeInfo.OH_IsConsignee);
			AssertEquals(org.OH_IsConsignor, codeInfo.OH_IsConsignor);
			AssertEquals(org.OH_IsForwarder, codeInfo.OH_IsForwarder);
			AssertEquals(org.OH_IsGlobalAccount, codeInfo.OH_IsGlobalAccount);
			AssertEquals(org.OH_IsMiscFreightServices, codeInfo.OH_IsMiscFreightServices);
			AssertEquals(org.OH_IsNationalAccount, codeInfo.OH_IsNationalAccount);
			AssertEquals(org.OH_IsSalesLead, codeInfo.OH_IsSalesLead);
			AssertEquals(org.OH_IsShippingProvider, codeInfo.OH_IsShippingProvider);
			AssertEquals(org.OH_IsTransportClient, codeInfo.OH_IsTransportClient);
			AssertEquals(org.OH_IsWarehouseClient, codeInfo.OH_IsWarehouseClient);
			AssertEquals(org.OH_Language, codeInfo.OH_Language);
			AssertEquals(org.OH_FullName, codeInfo.OH_FullName);
			AssertEquals("AUSYD", codeInfo.UnlocoCode);
			AssertEquals(org.PK, codeInfo.PK);
			AssertEquals("Sydney", codeInfo.PortName);
			AssertEquals("Australia", codeInfo.CountryName);
		}
	}
}
