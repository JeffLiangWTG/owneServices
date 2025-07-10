using CargoWise.EntityFramework;

namespace Enterprise.Customs.DE.NCTS.Business
{
	public class NonPersistentItineraryCountry : EU.NCTS.Business.NonPersistentItineraryCountry
	{
		public NonPersistentItineraryCountry(int sequence, BusinessObjectFactory factory)
			: base(sequence, factory)
		{
		}

		public new NonPersistentItineraryCountryValidation Validation => (NonPersistentItineraryCountryValidation)base.Validation;

		protected override EU.NCTS.Business.NonPersistentItineraryCountryValidation GetNewValidation()
		{
			return new NonPersistentItineraryCountryValidation(this);
		}
	}
}
