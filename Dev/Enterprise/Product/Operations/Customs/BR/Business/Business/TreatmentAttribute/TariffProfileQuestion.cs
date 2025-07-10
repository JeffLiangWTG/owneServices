using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.BR.Business
{
	public class TariffProfileQuestion
	{
		public ZGuid PK { get; set; }
		public ZString Code { get; set; }
		public ZString Name { get; set; }
		public ZString Note { get; set; }
		public ZString Example { get; set; }
		public ZBool IsAnswerMandatory { get; set; }
		public ZString AnswerDataType { get; set; }
		public ZInt AnswerMaxLength { get; set; }
		public ZShort AnswerDecimalPlaces { get; set; }
		public ZBool AllowMultipleAnswers { get; set; }
		public ZDateTime StartDate { get; set; }
		public ZDateTime EndDate { get; set; }
		public IReadOnlyList<TariffProfileQuestionAnswer> AnswerList { get; set; }

		public RefCusProfileQuestion RefCusProfileQuestion { get; set; }

		public static TariffProfileQuestion New(RefCusProfileQuestion question) => new()
		{
			PK = question.PK,
			Code = question.XQ2_Code,
			Name = question.XQ2_Name,
			Note = question.XQ2_Note,
			IsAnswerMandatory = question.XQ2_IsAnswerMandatory,
			AnswerDataType = question.XQ2_AnswerDataType,
			AnswerMaxLength = question.XQ2_AnswerMaxLength,
			AnswerDecimalPlaces = question.XQ2_AnswerDecimalPlaces,
			AllowMultipleAnswers = question.XQ2_AllowMultipleAnswers,
			AnswerList = question.Answers.Select(TariffProfileQuestionAnswer.New).ToArray(),
			RefCusProfileQuestion = question,
			StartDate = question.XQ2_StartDate,
			EndDate = question.XQ2_EndDate,
		};

		public static TariffProfileQuestion New(RefCusTariffBRCharacteristic characteristic) => new()
		{
			PK = characteristic.PK,
			Code = characteristic.ZB1_Code,
			Name = characteristic.ZB1_Text,
			Note = characteristic.Attributes.FirstOrDefault(x => x.ZB3_Code == Enterprise.Customs.Universal.Constants.ProfileQuestion.AttributeNames.Caption)?.ZB3_Value ?? ZString.Empty,
			Example = characteristic.Attributes.FirstOrDefault(x => x.ZB3_Code == Enterprise.Customs.Universal.Constants.ProfileQuestion.AttributeNames.Example)?.ZB3_Value ?? ZString.Empty,
			IsAnswerMandatory = characteristic.ZB1_IsMandatory,
			AnswerDataType = characteristic.ZB1_Style,
			AnswerMaxLength = characteristic.ZB1_MaxLength,
			AnswerDecimalPlaces = characteristic.ZB1_DecimalPlaces,
			AllowMultipleAnswers = false,
			AnswerList = characteristic.Values.Select(TariffProfileQuestionAnswer.New).ToArray(),
			StartDate = characteristic.ZB1_StartDate,
			EndDate = characteristic.ZB1_EndDate,
		};
	}
}
