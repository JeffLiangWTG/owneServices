using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class ClassificationTestCase : TestCaseWithFactory
	{
		public void TestNeedToRegenerateQuestionsGetsSet()
		{
			Classification importClassification = Factory.New<Classification>();
			AssertEquals("Need to regenerate ", true, importClassification.LineAttacheeHolderWrapper.NeedToGenerateQuestions);

			importClassification.LineAttacheeHolderWrapper.NeedToGenerateQuestions = false;
			importClassification.CC_TariffNum = "0000.00.00 00";
			AssertEquals("Need to regenerate ", true, importClassification.LineAttacheeHolderWrapper.NeedToGenerateQuestions);

			importClassification.LineAttacheeHolderWrapper.NeedToGenerateQuestions = false;
			importClassification.CC_AddInfo = "ORG=AU";
			AssertEquals("Need to regenerate ", true, importClassification.LineAttacheeHolderWrapper.NeedToGenerateQuestions);
		}
	}
}
