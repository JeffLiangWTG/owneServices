using System;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.NL.Business.Declaration;

namespace Enterprise.Customs.NL.Business.Testing;

sealed class AuthorisationWrapperTest : DataProviderTestCase<AuthorisationWrapper>
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>(() => new AuthorisationWrapper(null, 1));
	}

	public void TestSequenceNumeric()
	{
		AssertEquals("SequenceNumeric", 1, GetProvider().SequenceNumeric);
	}

	public void TestId()
	{
		AssertEquals("3456789", GetProvider().Id);
	}

	public void TestTypeCode()
	{
		AssertEquals("C502", GetProvider().TypeCode);
	}

	public void TestHolderId()
	{
		var orgHeader = WrapperTestHelper.CreateOrgHeader(Factory, "Authorization Full Name", "", "112233");
		cusAuthorizationUsage.AGC_OH_Owner = orgHeader.PK;
		AssertEquals("NL112233", GetProvider().HolderId);
	}

	protected override AuthorisationWrapper GetProvider() => wrapper;

	protected override void SetUp()
	{
		base.SetUp();
		var declaration = Factory.New<JobDeclaration>();
		var cusEntryInstruction = declaration.CustomsEntryInstructions.AddNew();

		cusAuthorizationUsage = cusEntryInstruction.CusAuthorizationUsages.AddNew();
		cusAuthorizationUsage.AGC_Code = "C502";
		cusAuthorizationUsage.AGC_Number = "3456789";

		wrapper = new AuthorisationWrapper(cusAuthorizationUsage, 1);
	}
	CusAuthorizationUsage cusAuthorizationUsage;
	AuthorisationWrapper wrapper;
}
