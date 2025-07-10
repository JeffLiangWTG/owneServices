using System;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Business.Testing
{
	public class DeclarationDVDAuthorisationWrapperTest : WrapperHelperTest<DeclarationDVDAuthorisationWrapper>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown("Constructor Throws Exception if authorization is null", typeof(ArgumentNullException),
				ArgumentExceptionMessageHelper.GetArgumentExceptionMessage("Value cannot be null.","authorization"), () => new DeclarationDVDAuthorisationWrapper(null));
		}

		public void TestType()
		{
			authorization.AGC_Code = "CW1";
			AssertEquals("Expected filled Type", "CW1", wrapper.Type);
		}

		public void TestOwnerId()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			authorization.AGC_OH_Owner = orgHeader.PK;

			CombineAssertions(() =>
			{
				AssertEquals("Expected empty OwnerId", ZString.Empty, wrapper.OwnerId);

				OrgCusCode eoriCusCode = orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "22222222", "FR");
				AssertEquals("Expected EORI OwnerId with country code", "FR22222222", wrapper.OwnerId);

				eoriCusCode.OK_CustomsRegNo = "ES22222222";
				eoriCusCode.OK_RN_NKCodeCountry = "ES";
				AssertEquals("Expected EORI OwnerId with country code not repeated", "ES22222222", wrapper.OwnerId);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			authorization = Factory.New<CusAuthorizationUsage>();

			wrapper = new DeclarationDVDAuthorisationWrapper(authorization);
		}

		CusAuthorizationUsage authorization;
		DeclarationDVDAuthorisationWrapper wrapper;

		protected override DeclarationDVDAuthorisationWrapper GetProvider() => wrapper;
	}
}
