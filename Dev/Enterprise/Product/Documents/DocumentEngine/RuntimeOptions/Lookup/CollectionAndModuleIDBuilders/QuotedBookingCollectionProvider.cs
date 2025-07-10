using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Freight.Integration.QuotedBooking;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	internal class QuotedBookingCollectionProvider : CollectionProvider
	{
		public QuotedBookingCollectionProvider(BusinessObjectFactory businessObjectFactory)
			: base(businessObjectFactory)
		{
		}

		protected override IBusinessObjectCollection CreateCollection()
		{
			return ObjectFactory.Get<IViewQuotedBookingCollection>("IViewQuotedBookingCollection", BusinessObjectFactory);
		}

		public override ModuleIdentifier ModuleID => ModuleIDs.QuotedBookings;
	}
}
