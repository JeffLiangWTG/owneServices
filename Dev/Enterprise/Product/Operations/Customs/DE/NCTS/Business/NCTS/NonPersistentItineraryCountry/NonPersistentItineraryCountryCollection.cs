using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.DE.NCTS.Business
{
	public class NonPersistentItineraryCountryCollection : EU.NCTS.Business.NonPersistentItineraryCountryCollection
	{
		public NonPersistentItineraryCountryCollection(NctsHeader header)
			: base(header)
		{
		}

		public new NonPersistentItineraryCountry this[int i] => (NonPersistentItineraryCountry)base[i];

		public new NonPersistentItineraryCountry AddNew() => (NonPersistentItineraryCountry)base.AddNew();

		protected override EU.NCTS.Business.NonPersistentItineraryCountry CreateNewNonPersistentItineraryCountry(ZInt sequenceNumber, BusinessObjectFactory factory)
		{
			return new NonPersistentItineraryCountry(sequenceNumber, factory);
		}
	}
}
