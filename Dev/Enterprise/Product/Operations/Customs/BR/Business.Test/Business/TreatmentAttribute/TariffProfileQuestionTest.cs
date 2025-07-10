using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.BR.Business.Testing
{
	sealed class TariffProfileQuestionTest : TestCaseWithFactory
	{
		public void TestNew_RefCusProfileQuestion()
		{
			var question = Factory.New<RefCusProfileQuestion>();
			question.XQ2_Code = "Q123";
			question.XQ2_Name = "NAME";
			question.XQ2_Note = "NOTE";
			question.XQ2_IsAnswerMandatory = true;
			question.XQ2_AnswerDataType = Universal.Constants.ProfileQuestion.AnswerDataTypes.String;
			question.XQ2_AnswerMaxLength = 10;
			question.XQ2_AnswerDecimalPlaces = 100;
			question.XQ2_AllowMultipleAnswers = true;

			var tariffQuestion = TariffProfileQuestion.New(question);
			CombineAssertions(() =>
			{
				AssertEquals("PK should be same", question.PK, tariffQuestion.PK);
				AssertEquals("Code should be ", "Q123", tariffQuestion.Code);
				AssertEquals("Name should be ", "NAME", tariffQuestion.Name);
				AssertEquals("Note should be ", "NOTE", tariffQuestion.Note);
				AssertEquals("Example should be ", ZString.Empty, tariffQuestion.Example);
				AssertEquals("IsAnswerMandatory should be ", true, tariffQuestion.IsAnswerMandatory);
				AssertEquals("AnswerDataType should be ", Universal.Constants.ProfileQuestion.AnswerDataTypes.String, tariffQuestion.AnswerDataType);
				AssertEquals("AnswerMaxLength should be ", (ZInt)10, tariffQuestion.AnswerMaxLength);
				AssertEquals("AnswerDecimalPlaces should be ", (ZShort)100, tariffQuestion.AnswerDecimalPlaces);
				AssertEquals("AllowMultipleAnswers should be ", true, tariffQuestion.AllowMultipleAnswers);
				AssertEquals("RefCusProfileQuestion should be ", question, tariffQuestion.RefCusProfileQuestion);
			});
		}

		public void TestNew_RefCusTariffBRCharacteristic()
		{
			var tariffCharacteristic = Factory.New<RefCusTariffBRCharacteristic>();
			tariffCharacteristic.ZB1_Code = "CODE";
			tariffCharacteristic.ZB1_Text = "Text";
			tariffCharacteristic.ZB1_Style = Universal.Constants.ProfileQuestion.AnswerDataTypes.String;
			tariffCharacteristic.ZB1_DecimalPlaces = 100;
			tariffCharacteristic.ZB1_MaxLength = 10;
			tariffCharacteristic.ZB1_IsMandatory = true;
			var attribute1 = tariffCharacteristic.Attributes.AddNew();
			attribute1.ZB3_Code = Universal.Constants.ProfileQuestion.AttributeNames.Caption;
			attribute1.ZB3_Value = "Note";
			var attribute2 = tariffCharacteristic.Attributes.AddNew();
			attribute2.ZB3_Code = Universal.Constants.ProfileQuestion.AttributeNames.Example;
			attribute2.ZB3_Value = "Example";

			var tariffQuestion = TariffProfileQuestion.New(tariffCharacteristic);
			CombineAssertions(() =>
			{
				AssertEquals("PK should be same", tariffCharacteristic.PK, tariffQuestion.PK);
				AssertEquals("Code should be ", "CODE", tariffQuestion.Code);
				AssertEquals("Name should be ", "Text", tariffQuestion.Name);
				AssertEquals("Note should be ", "Note", tariffQuestion.Note);
				AssertEquals("Example should be ", "Example", tariffQuestion.Example);
				AssertEquals("IsAnswerMandatory should be ", true, tariffQuestion.IsAnswerMandatory);
				AssertEquals("AnswerDataType should be ", Universal.Constants.ProfileQuestion.AnswerDataTypes.String, tariffQuestion.AnswerDataType);
				AssertEquals("AnswerMaxLength should be ", (ZInt)10, tariffQuestion.AnswerMaxLength);
				AssertEquals("AnswerDecimalPlaces should be ", (ZShort)100, tariffQuestion.AnswerDecimalPlaces);
				AssertEquals("AllowMultipleAnswers should be ", false, tariffQuestion.AllowMultipleAnswers);
			});
		}
	}
}
