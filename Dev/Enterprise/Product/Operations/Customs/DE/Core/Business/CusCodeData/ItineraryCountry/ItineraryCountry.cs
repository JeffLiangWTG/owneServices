using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.DE.Business.Declaration;

namespace Enterprise.Customs.DE.Business
{
	public class ItineraryCountry : EU.Business.ItineraryCountry
	{
		public ItineraryCountry(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new ItineraryCountryValidation Validation => (ItineraryCountryValidation)base.Validation;

		protected override CusCodeDataValidation GetNewValidation() => new ItineraryCountryValidation(this);

		public JobDeclaration Declaration => (JobDeclaration)Parent;
	}
}
