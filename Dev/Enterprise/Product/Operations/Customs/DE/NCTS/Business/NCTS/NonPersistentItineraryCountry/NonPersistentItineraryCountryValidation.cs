using System.Linq;

namespace Enterprise.Customs.DE.NCTS.Business
{
	public class NonPersistentItineraryCountryValidation : EU.NCTS.Business.NonPersistentItineraryCountryValidation
	{
		public NonPersistentItineraryCountryValidation(EU.NCTS.Business.AutoNonPersistentItineraryCountry parent)
			: base(parent)
		{
		}

		public new NonPersistentItineraryCountry Parent => (NonPersistentItineraryCountry)base.Parent;

		public override void ValidateAll()
		{
			Parent.ClearRowNotifications();
			base.ValidateAll();
			ValidationNeededCountries();
		}

		void ValidationNeededCountries()
		{
			if (Parent.Header != null)
			{
				if (!Parent.CountryCode.IsEmpty)
				{
					var itineraryCountries = Parent.Header.Itinerary.Cast<NonPersistentItineraryCountry>().ToArray();
					if (itineraryCountries.Select(x => x.CountryCode).Distinct().Count() == 1)
					{
						Parent.AddRowMessageError(Res.GetString("af0b73c9-be14-4899-92e4-d9df8247f703", "At least two different countries/regions must be entered."));
					}

					if (itineraryCountries.All(x => x.CountryCode != Core.Constants.CountryCodes.Germany))
					{
						Parent.AddRowMessageError(Res.GetString("22dae8f9-b3ae-48e5-a2ed-cd0d3e6d208c", "At least one of the entered countries/regions must be DE."));
					}
				}
			}
		}
	}
}
