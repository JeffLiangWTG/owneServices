using CargoWise.Types;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.BR.Business
{
	public class TariffProfileQuestionAnswer
	{
		public ZString Value { get; set; }
		public ZString Description { get; set; }

		public static TariffProfileQuestionAnswer New(RefCusProfileQuestionAnswerList answer) => new()
		{
			Value = answer.XQ4_Value,
			Description = answer.XQ4_Description,
		};

		public static TariffProfileQuestionAnswer New(RefCusTariffBRCharacteristicValue value) => new()
		{
			Value = value.ZB2_Value,
			Description = value.ZB2_Description,
		};
	}
}
