using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.BR.Business
{
	public static class BRRefCusProfile
	{
		public static bool IsMandatory(this RefCusProfile profile)
		{
			var mandatoryAttribute = profile?.GetAttribute(Constants.Profile.AttributeNames.Mandatory) ?? ZString.Empty;
			return mandatoryAttribute.IsEmpty ? false : ZBool.ParseSafe(mandatoryAttribute, defaultValue: false);
		}

		public static ZString GetLegalCode(this RefCusProfile profile) => profile?.GetAttribute(Constants.Profile.AttributeNames.LegalCode) ?? ZString.Empty;

		public static ZString GetRegime(this RefCusProfile profile) => profile?.GetAttribute(Constants.Profile.AttributeNames.Regime) ?? ZString.Empty;

		public static ZString GetTaxType(this RefCusProfile profile) => profile?.GetAttribute(Constants.Profile.AttributeNames.TaxType) ?? ZString.Empty;

		public static TariffProfileQuestion[] GetProfileQuestions(RefCusProfile[] profiles, ZDateTime effectiveDate)
		{
			return profiles?.Length > 0 ? GetProfileQuestions() : Array.Empty<TariffProfileQuestion>();

			TariffProfileQuestion[] GetProfileQuestions()
			{
				var questions = new RefCusProfileQuestion.Loader(profiles[0].Factory).Load(profiles, Core.Constants.CountryCodes.Brazil, effectiveDate);
				questions.ForEach(x => x.FetchForLoadChildEditableObjectsIfNeeded());
				return questions.Select(TariffProfileQuestion.New).ToArray();
			}
		}
	}
}
