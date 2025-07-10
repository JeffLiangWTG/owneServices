namespace Enterprise.Customs.DE.Business
{
	public class ItineraryCountryValidation : EU.Business.ItineraryCountryValidation
	{
		public ItineraryCountryValidation(ItineraryCountry parent) : base(parent)
		{
			this.itineraryCountry = parent;
		}
		readonly ItineraryCountry itineraryCountry;

		protected override void CheckCY_Code()
		{
			base.CheckCY_Code();

			var jobDeclaration = itineraryCountry.Declaration;
			var itineraryCountryCollection = jobDeclaration.ItineraryCountries;

			if (itineraryCountryCollection.Count > 0)
			{
				if (itineraryCountryCollection.Count < 2)
				{
					Parent.CY_CodeInfo.AddMessageError(Res.GetString("4FE855B4-EEF7-4F8F-AF52-D33476709195", "At least two countries must be entered."));
				}
				else
				{
					if (Parent == itineraryCountryCollection[0] && Parent.CY_Code != jobDeclaration.JE_GoodsOrigin)
					{
						Parent.CY_CodeInfo.AddMessageError(Res.GetString("B2378AF0-B464-47BB-8F78-47D394426638", "The country must match the Dispatch country."));
					}

					if (Parent == itineraryCountryCollection[itineraryCountryCollection.Count - 1] && Parent.CY_Code != jobDeclaration.JE_GoodsDestination)
					{
						Parent.CY_CodeInfo.AddMessageError(Res.GetString("D9DA4869-9049-4826-BB10-092FE1679E2F", "The country must match the Destination country."));
					}
				}
			}
		}
	}
}
