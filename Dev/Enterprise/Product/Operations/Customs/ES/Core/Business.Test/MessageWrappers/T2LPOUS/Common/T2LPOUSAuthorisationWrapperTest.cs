using System;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Business.Testing;

public class T2LPOUSAuthorisationWrapperTest : WrapperHelperTest<T2LPOUSAuthorisationWrapper>
{
	public void TestConstructor()
	{
		AssertExceptionThrown("Constructor Throws Exception if authorization is null", typeof(ArgumentNullException),
			ArgumentExceptionMessageHelper.GetArgumentExceptionMessage("Value cannot be null.","authorization"), () => new T2LPOUSAuthorisationWrapper(null));
	}

	public void TestTypeOfAuthorisation()
	{
		authorization.AGC_Code = "ACP";
		AssertEquals("Expected filled TypeOfAuthorisation", "C511", wrapper.TypeOfAuthorisation);
	}

	public void TestDecisionReferenceNumber()
	{
		authorization.AGC_Number = "reference";
		AssertEquals("Expected filled DecisionReferenceNumber", "reference", wrapper.DecisionReferenceNumber);
	}

	public void TestHolderOfTheAuthorisation()
	{
		var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
		authorization.AGC_OH_Owner = orgHeader.PK;

		CombineAssertions(() =>
		{
			AssertEquals("Expected empty HolderOfTheAuthorisation", ZString.Empty, wrapper.HolderOfTheAuthorisation);

			orgHeader.CustomsCodes.AddNew(OrgCusCode.CodeTypes.PassportID, "GB333333333", "GB");
			AssertEquals("Expected PAS HolderOfTheAuthorisation with country code when no NIF or EORI declared", "GB333333333", wrapper.HolderOfTheAuthorisation);

			orgHeader.CustomsCodes.AddNew(OrgCusCode.SpainCodeTypes.NIF, "NIF22222222");
			orgHeader.OH_Category = OrgConstants.Category.NaturalPersonIndividual;
			AssertEquals("Expected NIF HolderOfTheAuthorisation", "NIF22222222", wrapper.HolderOfTheAuthorisation);

			OrgCusCode eoriCusCode = orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "22222222", "FR");
			AssertEquals("Expected EORI HolderOfTheAuthorisation with country code when authorization code is BOI or BTI", "FR22222222", wrapper.HolderOfTheAuthorisation);

			eoriCusCode.OK_CustomsRegNo = "ES22222222";
			eoriCusCode.OK_RN_NKCodeCountry = "ES";
			AssertEquals("Expected EORI HolderOfTheAuthorisation with country code not repeated when authorization code is BOI or BTI (BOI)", "ES22222222", wrapper.HolderOfTheAuthorisation);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();

		authorization = Factory.New<CusAuthorizationUsage>();

		wrapper = new T2LPOUSAuthorisationWrapper(authorization);
	}

	CusAuthorizationUsage authorization;
	T2LPOUSAuthorisationWrapper wrapper;

	protected override T2LPOUSAuthorisationWrapper GetProvider() => wrapper;
}
