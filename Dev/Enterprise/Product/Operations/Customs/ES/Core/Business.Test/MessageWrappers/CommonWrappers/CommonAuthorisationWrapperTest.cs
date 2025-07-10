using System;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Business.Testing;

public class CommonAuthorisationWrapperTest : WrapperHelperTest<CommonAuthorisationWrapper>
{
	public void TestConstructor()
	{
		AssertExceptionThrown("Constructor Throws Exception if authorization is null", typeof(ArgumentNullException),
			ArgumentExceptionMessageHelper.GetArgumentExceptionMessage("Value cannot be null.","authorization"), () => new CommonAuthorisationWrapper(null, 1));
	}

	public void TestType()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		var eun = helper.CreateNewOrGetExistingDataGrouping("EUN", "European Union");
		helper.CreateCusCodeType("AUTH", "Authorisation");
		helper.CreateCusCodeList("EUN", "AUTH", "SAS", "Self-Assessment", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
		helper.CreateCusMapType("EUNAU", "OUT", "Authorisation Codes to Document Type Code", true);
		helper.CreateCusMap("EUNAU", "SAS", "C019", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date, "EUN");
		Factory.Save();

		CombineAssertions(() =>
		{
			authorization.AGC_Code = "SAS";
			AssertEquals("Prereq: CustomsCode is not empty", "C019", authorization.CustomsCode);
			AssertEquals("Expected filled Type with value in CustomsCode", "C019", wrapper.Type);

			authorization.AGC_Code = "REP";
			AssertEquals("Prereq: CustomsCode is empty", ZString.Empty, authorization.CustomsCode);
			AssertEquals("Expected filled Type with value in AGC_Code", "REP", wrapper.Type);
		});
	}

	public void TestReferenceNumber()
	{
		authorization.AGC_Number = "reference";
		AssertEquals("Expected filled ReferenceNumber", "reference", wrapper.ReferenceNumber);
	}

	public void TestHolder()
	{
		var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
		authorization.AGC_OH_Owner = orgHeader.PK;
		authorization.AGC_Code = "BOI";

		CombineAssertions(() =>
		{
			AssertEquals("Expected empty Holder", ZString.Empty, wrapper.Holder);

			orgHeader.CustomsCodes.AddNew(OrgCusCode.CodeTypes.PassportID, "GB333333333", "GB");
			AssertEquals("Expected PAS Holder with country code when no NIF or EORI declared", "GB333333333", wrapper.Holder);

			orgHeader.CustomsCodes.AddNew(OrgCusCode.SpainCodeTypes.NIF, "NIF22222222");
			orgHeader.OH_Category = OrgConstants.Category.NaturalPersonIndividual;
			AssertEquals("Expected NIF Holder", "NIF22222222", wrapper.Holder);

			OrgCusCode eoriCusCode = orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "22222222", "FR");
			AssertEquals("Expected EORI Holder with country code when authorization code is BOI or BTI", "FR22222222", wrapper.Holder);

			eoriCusCode.OK_CustomsRegNo = "ES22222222";
			eoriCusCode.OK_RN_NKCodeCountry = "ES";
			AssertEquals("Expected EORI Holder with country code not repeated when authorization code is BOI or BTI (BOI)", "ES22222222", wrapper.Holder);

			authorization.AGC_Code = "AAA";
			AssertEquals("Expected empty Holder when authorization code is not BOI nor BTI", ZString.Empty, wrapper.Holder);

			authorization.AGC_Code = "BTI";
			AssertEquals("Expected EORI Holder with country code not repeated when authorization code is BOI or BTI (BTI)", "ES22222222", wrapper.Holder);
		});
	}

	public void TestSequenceNumber()
	{
		AssertEquals("Expected filled SequenceNumber", "1", wrapper.SequenceNumber);
	}

	protected override void SetUp()
	{
		base.SetUp();

		authorization = Factory.NewWithValidTestData<CusAuthorizationUsage>();

		wrapper = new CommonAuthorisationWrapper(authorization, 1);
	}

	CusAuthorizationUsage authorization;
	CommonAuthorisationWrapper wrapper;

	protected override CommonAuthorisationWrapper GetProvider() => wrapper;
}
