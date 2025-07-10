using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.EU.EMCS.Business.Testing
{
	sealed class Message837HeaderProviderHelperTest : TestCaseWithFactory
	{
		public void TestExplanationCode()
		{
			explanationOnDelay.ExplanationCode = EMCSExplanationOnDelayCodeList.Codes.Accident;
			AssertEquals(EMCSExplanationOnDelayCodeList.Codes.Accident, helper.ExplanationCode);
		}

		public void TestMessageRole()
		{
			explanationOnDelay.MessageRole = EMCSExplanationOnDelayMessageRoleCodeList.Codes._2;
			AssertEquals(EMCSExplanationOnDelayMessageRoleCodeList.Codes._2, helper.MessageRole);
		}

		public void TestSubmitterType()
		{
			CombineAssertions(() =>
			{
				emcsDeclaration.JE_DeclarantType = ZString.Empty;
				AssertEquals(string.Empty, helper.SubmitterType);
				emcsDeclaration.JE_DeclarantType = EMCSEntryTypeList.Codes.Consignor;
				AssertEquals(EMCSEntryTypeList.Codes.Consignor, helper.SubmitterType);
			});
		}

		public void TestComplementaryInformationSpecified()
		{
			CombineAssertions(() =>
			{
				explanationOnDelay.ExplanationCode = EMCSExplanationOnDelayCodeList.Codes.Other;
				AssertEquals("ExplanationCode is '0'", true, helper.ComplementaryInformationSpecified);
				explanationOnDelay.ExplanationCode = EMCSExplanationOnDelayCodeList.Codes.Accident;
				AssertEquals("ExplanationCode isn't '0'", false, helper.ComplementaryInformationSpecified);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			emcsDeclaration = Factory.New<EMCSJobDeclaration>();
			explanationOnDelay = new ExplanationOnDelaySendingAction(emcsDeclaration);
			helper = new Message837HeaderProviderHelper(emcsDeclaration, explanationOnDelay);
		}
		EMCSJobDeclaration emcsDeclaration;
		Message837HeaderProviderHelper helper;
		ExplanationOnDelaySendingAction explanationOnDelay;
	}
}
