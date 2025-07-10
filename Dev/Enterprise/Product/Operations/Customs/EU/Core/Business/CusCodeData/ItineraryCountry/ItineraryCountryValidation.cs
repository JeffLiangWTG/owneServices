using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.EU.Business
{
	public class ItineraryCountryValidation : CusCodeDataValidation
	{
		public ItineraryCountryValidation(ItineraryCountry parent) : base(parent)
		{
		}

		protected override void CheckCY_Code()
		{
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.CY_CodeInfo);
		}
	}
}
