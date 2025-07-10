using System.Collections;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class CountryOfRoutingLookups : CusCodeDataLookups
	{
		public CountryOfRoutingLookups(CountryOfRouting countryOfRouting) : base(countryOfRouting)
		{
		}

		new CountryOfRouting Parent => (CountryOfRouting)base.Parent;

		public ICollection CountryList => Factory.GetCountryNC008Collection(Parent.DefaultDataGroupingCode);
	}
}
