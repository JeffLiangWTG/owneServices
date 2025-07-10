using System.Collections;
using CargoWise.Types;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.IE.Business.Declaration
{
	public class AddInfoJobComInvoiceLineLookups : EU.Business.Declaration.AddInfoJobComInvoiceLineLookups
	{
		public AddInfoJobComInvoiceLineLookups(EU.Business.Declaration.AddInfoJobComInvoiceLine parent) : base(parent)
		{
		}

		protected override ICollection RegionOfDestinationListCore => Factory.GetCachedValue<RegionOfDestinationList>();

		protected override ICollection CountryOfSupplyListCore() => GetNewCountriesList();

		protected ICollection GetNewCountriesList() => ZZRefCusCodeListCombinedCollection.GetCachedCollection(Factory, Core.Constants.CountryCodes.Ireland, UniversalReferenceConstants.RefCusCodeListTypes.Codes.Country, ZDateTime.Today);
	}
}
