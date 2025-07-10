using System;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.ExitControl.Business.Testing
{
	class EALAESMessageWrapperTest : WrapperHelperTest<EALAESMessageWrapper>
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown("Constructor Throws Exception if Exit Header is null", typeof(ArgumentNullException),
					ArgumentExceptionMessageHelper.GetArgumentExceptionMessage("Value cannot be null.", "exitHeader"), () => GetWrapper(null));
			});
		}

		public void TestSender()
		{
			var address = Factory.New<OrgAddress>();
			exitHeader.CXH_OA_Carrier = address.PK;
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			exitHeader.Carrier.OA_OH = orgHeader.PK;

			CombineAssertions(() =>
			{
				AssertEquals("Expected empty Sender", ZString.Empty, wrapper.Sender);

				orgHeader.CustomsCodes.AddNew(OrgCusCode.CodeTypes.PassportID, "GB333333333", "GB");
				AssertEquals("Expected PAS Sender with country code when no NIF or EORI declared", "GB333333333", wrapper.Sender);

				orgHeader.CustomsCodes.AddNew(OrgCusCode.SpainCodeTypes.NIF, "NIF22222222");
				orgHeader.OH_Category = OrgConstants.Category.NaturalPersonIndividual;
				AssertEquals("Expected NIF Sender", "NIF22222222", wrapper.Sender);

				orgHeader.CustomsCodes.AddNew(OrgCusCode.SpainCodeTypes.NIF, "NIF22222222");
				orgHeader.OH_Category = OrgConstants.Category.Government;
				AssertEquals("Expected Country+NIF for non NAT Organizations Sender", "ESNIF22222222", wrapper.Sender);

				OrgCusCode eoriCusCode = orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "22222222", "FR");
				AssertEquals("Expected EORI Sender with country code", "FR22222222", wrapper.Sender);

				eoriCusCode.OK_CustomsRegNo = "ES22222222";
				eoriCusCode.OK_RN_NKCodeCountry = "ES";
				AssertEquals("Expected EORI Sender with country code not repeated", "ES22222222", wrapper.Sender);
			});
		}

		public void TestMessageIdentification()
		{
			AssertEquals("Expected filled MessageIdentification", "<<MSGNO PLACEHOLDER>>", wrapper.MessageIdentification);
		}

		protected override void SetUp()
		{
			base.SetUp();

			exitHeader = Factory.NewWithValidTestData<CusExitHeader>();

			wrapper = GetWrapper(exitHeader);
		}
		CusExitHeader exitHeader;
		EALAESMessageWrapper wrapper;

		EALAESMessageWrapper GetWrapper(CusExitHeader exitHeader) => new EALAESMessageWrapper(exitHeader);
		protected override EALAESMessageWrapper GetProvider() => wrapper;
	}
}
