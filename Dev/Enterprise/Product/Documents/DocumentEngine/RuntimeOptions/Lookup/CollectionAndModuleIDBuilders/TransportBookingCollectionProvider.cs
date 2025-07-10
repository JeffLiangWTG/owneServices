using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Integration.TransportBooking;
using Enterprise.TransportBookings.Shared;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	internal class TransportBookingCollectionProvider : CollectionProvider
	{
		public TransportBookingCollectionProvider(BusinessObjectFactory businessObjectFactory)
			: base(businessObjectFactory)
		{
		}

		protected override IBusinessObjectCollection CreateCollection()
		{
			return ObjectFactory.Get<IDtbBookingCollection>("IDtbBookingCollection", BusinessObjectFactory, new AdhocCollectionRelationship(ObjectFactory.GetType<IDtbBooking>()));
		}

		protected override IBusinessObjectCollection GetCollectionForFindbox()
		{
			return ObjectFactory.Get<IDtbBookingCollection>("IDtbBookingCollection", BusinessObjectFactory);
		}

		public override ModuleIdentifier ModuleID => ModuleIDs.DtbBooking;
	}
}
