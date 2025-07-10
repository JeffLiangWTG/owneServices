using CargoWise.EntityFramework;
using Enterprise.Customs.NL.Business.Declaration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NL.Business.Testing;

[TestedType(typeof(CusAuthorizationUsage))]
sealed class CusAuthorizationUsageTest : EnterpriseBusinessObjectTestCase
{
	protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObject();

	protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewBusinessObject();
	
	protected override BusinessObject GetNewBusinessObject()
	{
		var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
		var cusAuthorizationUsage = CreateAuthorizationUsage();
		cusAuthorizationUsage.AGC_Code = "abc";
		cusAuthorizationUsage.AGC_Number = "123";
		cusAuthorizationUsage.AGC_OH_Owner = orgHeader.PK;
		return cusAuthorizationUsage;
	}

	CusAuthorizationUsage CreateAuthorizationUsage()
	{
		var testDec = Factory.New<JobDeclaration>();
		var entryInstruction = testDec.CustomsEntryInstructions.AddNew();
		return entryInstruction.CusAuthorizationUsages.AddNew();
	}

	public void TestInvoiceLine()
	{
		var testDec = Factory.New<JobDeclaration>();
		var invoiceLine = testDec.Invoices.AddNew().InvoiceLines.AddNew();
		var authorizationsUsage = invoiceLine.CusAuthorizationUsages.AddNew();

		AssertEquals("InvoiceLine on CusAuthorizationsUsage", invoiceLine, authorizationsUsage.InvoiceLine);
	}

	public void TestDefaultGoodsLocationBasedOnAuthorization()
	{
		var authorizationHeader = Factory.New<CusAuthorisationHeader>();
		authorizationHeader.CPH_Number = "1523625B02";
		authorizationHeader.CPH_Type = NLCusAuthorisationHeaderTypeList.Codes.CentralizedClearance;
		var authorizationRule = authorizationHeader.CusAuthorisationRules.AddNew();
		authorizationRule.CPR_RuleCode = Customs.Business.CusAuthorisationRuleTypeList.Codes.Location;
		authorizationRule.GoodsLocation.CGL_Qualifier = Customs.Business.CusGoodsLocationQualifierList.Codes.PostcodeAddress;
		authorizationRule.GoodsLocation.CGL_Type = Customs.Business.CusGoodsLocationTypeList.Codes.AuthorizedPlace;
		authorizationRule.GoodsLocation.AdditionalIdentifier = "42";
		authorizationRule.GoodsLocation.Address.E2_RN_NKCountryCode = "NL";
		authorizationRule.GoodsLocation.Address.E2_Postcode = "4950 LC";

		var authorizationUsage = CreateAuthorizationUsage();
		authorizationUsage.AGC_Code = NLCusAuthorisationHeaderTypeList.Codes.CentralizedClearance;
		authorizationUsage.AGC_Number = "1523625B02";
		var entryInstruction = authorizationUsage.Parent as CusEntryInstruction;

		CombineAssertions(() =>
		{
			AssertEquals("GoodsLocation is defaulted when AdditionalIdentifier is equal to 42 (housenumber from address in authorizationHeader)", "42", entryInstruction.GoodsLocation.AdditionalIdentifier);
			AssertEquals("GoodsLocationDescription is updated", "T;B;4950 LC;42;NL", entryInstruction.GoodsLocationDescription);
		});
	}
}
