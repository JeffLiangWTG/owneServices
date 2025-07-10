using CargoWise.EntityFramework;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.NonPersistentShipmentToHawbMatcher
{
	public class NonPersistentShipmentToHawbMatcherLineCollection : NonPersistentBusinessObjectCollection<NonPersistentShipmentToHawbMatcherLine>
	{
		public NonPersistentShipmentToHawbMatcherLineCollection(BusinessObjectFactory factory, NonPersistentShipmentToHawbMatcherHeader nonPersistentShipmentToHawbMatcherHeader) : base(factory)
		{
			this.nonPersistentShipmentToHawbMatcherHeader = nonPersistentShipmentToHawbMatcherHeader;
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		protected override bool AllowRemoveCore
		{
			get { return false; }
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new NonPersistentShipmentToHawbMatcherLine(nonPersistentShipmentToHawbMatcherHeader, Factory);
		}

		readonly NonPersistentShipmentToHawbMatcherHeader nonPersistentShipmentToHawbMatcherHeader;
	}
}
