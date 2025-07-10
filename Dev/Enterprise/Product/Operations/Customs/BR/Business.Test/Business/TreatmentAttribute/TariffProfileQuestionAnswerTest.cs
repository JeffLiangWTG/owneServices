using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.BR.Business.Testing
{
	sealed class TariffProfileQuestionAnswerTest : TestCaseWithFactory
	{
		public void TestNew_RefCusProfileQuestionAnswerList()
		{
			var question = Factory.New<RefCusProfileQuestionAnswerList>();
			question.XQ4_Value = "Q123";
			question.XQ4_Description = "NAME";

			var tariffAnswer = TariffProfileQuestionAnswer.New(question);
			CombineAssertions(() =>
			{
				AssertEquals("Value should be ", "Q123", tariffAnswer.Value);
				AssertEquals("Description should be ", "NAME", tariffAnswer.Description);
			});
		}

		public void TestNew_RefCusTariffBRCharacteristicValue()
		{
			var tariffCharacteristic = Factory.New<RefCusTariffBRCharacteristicValue>();
			tariffCharacteristic.ZB2_Value = "CODE";
			tariffCharacteristic.ZB2_Description = "Text";

			var tariffAnswer = TariffProfileQuestionAnswer.New(tariffCharacteristic);
			CombineAssertions(() =>
			{
				AssertEquals("Value should be ", "CODE", tariffAnswer.Value);
				AssertEquals("Description should be ", "Text", tariffAnswer.Description);
			});
		}
	}
}
