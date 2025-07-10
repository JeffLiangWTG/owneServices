using CargoWise.Types;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.BR.Business
{
	public class TariffProfile
	{
		public ZGuid PK { get; set; }
		public ZString QuestionCode { get; set; }
		public ZString Regime { get; set; }
		public ZString LegalCode { get; set; }
		public ZString TaxType { get; set; }
		public bool IsMandatory { get; set; }

		public RefCusProfile RefCusProfile { get; set; }

		public static TariffProfile New(RefCusProfile profile) => new()
		{
			PK = profile.PK,
			QuestionCode = profile.XX0_QuestionCode,
			Regime = profile.GetRegime(),
			LegalCode = profile.GetLegalCode(),
			TaxType = profile.GetTaxType(),
			IsMandatory = profile.IsMandatory(),
			RefCusProfile = profile,
		};
	}
}
