using Enterprise.Customs.Business.Extensions;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class MustBeMemberOfEUOrCommonTransit : RefCountryValidationRule
	{
		public override bool IsApplied => true;

		protected virtual string GetInvalidString(RefCountry country)
		{
			return Res.GetString("3027421F-EBBF-495F-84ED-ADD92BB521A6", "{0} is not listed as being in the European Union or a member of the Common Transit convention. Check the value of your office code or check that your list of economic groupings is up to date.", country.RN_DescMultilingual);
		}

		protected override ValidationResult ValidateCore(RefCountry country)
		{
			return !country.Factory.IsCountryEuOrCtCountry(country.Code)
				? ValidationResult.Invalid(GetInvalidString(country))
				: ValidationResult.Valid;
		}
	}
}

