using CargoWise.EntityFramework;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class NonPersistentItineraryCountryValidation : AutoNonPersistentItineraryCountryValidation
	{
		public NonPersistentItineraryCountryValidation(AutoNonPersistentItineraryCountry parent)
			: base(parent)
		{ }

		protected override void CheckCountryCode()
		{
			base.CheckCountryCode();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.CountryCodeInfo);
		}
	}
}
