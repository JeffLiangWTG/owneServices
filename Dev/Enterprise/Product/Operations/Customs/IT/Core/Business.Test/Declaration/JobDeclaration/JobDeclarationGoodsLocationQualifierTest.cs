using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

sealed class JobDeclarationGoodsLocationQualifierTest : TestCaseWithFactory
{
	public void TestIsLocationQualifierValid()
	{
		CombineAssertions(() =>
		{
			declaration.JE_LocationQualifier = ZString.Empty;
			AssertEquals("Empty location qualifier", false, declaration.IsLocationQualifierValid);

			declaration.JE_LocationQualifier = "X";
			AssertEquals("Invalid location qualifier", false, declaration.IsLocationQualifierValid);

			declaration.JE_LocationQualifier = "D";
			AssertEquals("Valid location qualifier", true, declaration.IsLocationQualifierValid);

			declaration.JE_LocationQualifier = "LC";
			AssertEquals("Valid authorization location qualifier but authorization not set", false, declaration.IsLocationQualifierValid);

			declaration.ZG_AuthorisationNumber = "1111CDE";
			declaration.JE_LocationQualifier = "LC";
			AssertEquals("Valid authorization location qualifier and authorization is set", true, declaration.IsLocationQualifierValid);
		});
	}

	public void TestIsLocationQualifierValidAndD()
	{
		CombineAssertions(() =>
		{
			declaration.JE_LocationQualifier = ZString.Empty;
			AssertEquals("Empty location qualifier", false, declaration.IsLocationQualifierValidAndD);

			declaration.JE_LocationQualifier = "D";
			AssertEquals("D", true, declaration.IsLocationQualifierValidAndD);

			declaration.ZG_AuthorisationNumber = "1111CDE";
			AssertEquals("Code is D but authorization is set", false, declaration.IsLocationQualifierValidAndD);
		});
	}

	public void TestIsLocationQualifierValidAndF()
	{
		CombineAssertions(() =>
		{
			declaration.JE_LocationQualifier = ZString.Empty;
			AssertEquals("Empty location qualifier", false, declaration.IsLocationQualifierValidAndF);

			declaration.JE_LocationQualifier = "F";
			AssertEquals("F", true, declaration.IsLocationQualifierValidAndF);

			declaration.ZG_AuthorisationNumber = "1111CDE";
			AssertEquals("Code is F but authorization is set", false, declaration.IsLocationQualifierValidAndF);
		});
	}

	public void TestIsLocationQualifierValidAndFC()
	{
		CombineAssertions(() =>
		{
			declaration.JE_LocationQualifier = ZString.Empty;
			AssertEquals("Empty location qualifier", false, declaration.IsLocationQualifierValidAndFC);

			declaration.JE_LocationQualifier = "FC";
			AssertEquals("FC", true, declaration.IsLocationQualifierValidAndFC);

			declaration.ZG_AuthorisationNumber = "1111CDE";
			AssertEquals("Code is FC but authorization is set", false, declaration.IsLocationQualifierValidAndFC);
		});
	}

	public void TestIsLocationQualifierValidAndLBorLC()
	{
		CombineAssertions(() =>
		{
			declaration.JE_LocationQualifier = "LC";
			AssertEquals("LC but authorization is not set", false, declaration.IsLocationQualifierValidAndLBorLC);

			declaration.ZG_AuthorisationNumber = "1111CDE";

			declaration.JE_LocationQualifier = ZString.Empty;
			AssertEquals("Empty location qualifier", false, declaration.IsLocationQualifierValidAndLBorLC);

			declaration.JE_LocationQualifier = "LB";
			AssertEquals("Empty location qualifier", true, declaration.IsLocationQualifierValidAndLBorLC);

			declaration.JE_LocationQualifier = "LC";
			AssertEquals("Empty location qualifier", true, declaration.IsLocationQualifierValidAndLBorLC);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();

		declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
	}
	JobDeclaration declaration;
}
