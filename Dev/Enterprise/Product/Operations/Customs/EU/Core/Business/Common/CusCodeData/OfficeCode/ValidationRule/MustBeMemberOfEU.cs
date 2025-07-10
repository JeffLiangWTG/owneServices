using Enterprise.Customs.Business.Extensions;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.Business
{
	public class MustBeMemberOfEU : RefCountryValidationRule
	{
		public override bool IsApplied => true;

		protected override ValidationResult ValidateCore(RefCountry country)
		{
			return !country.Factory.IsMemberOfEU(country.Code)
				? ValidationResult.Invalid(GetInvalidString(country))
				: ValidationResult.Valid;
		}

		protected virtual string GetInvalidString(RefCountry country)
		{
			return Res.GetString("206947e1-6e74-423b-9786-2b03e3b3767a", "{0} is not listed as being in the European Union. Check the value of your office code or check that your list of economic groupings is up to date.", country.RN_DescMultilingual);
		}
	}
}
