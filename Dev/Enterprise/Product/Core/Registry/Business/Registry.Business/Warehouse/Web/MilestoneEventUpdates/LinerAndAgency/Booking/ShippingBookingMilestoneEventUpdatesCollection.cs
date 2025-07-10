using System.Xml.Serialization;
using CargoWise.EntityFramework;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class ShippingBookingMilestoneEventUpdatesCollection : MilestoneEventUpdatesCollection
	{
		public ShippingBookingMilestoneEventUpdatesCollection()
			: base()
		{
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new ShippingBookingMilestoneEventUpdates();
		}

		public new ShippingBookingMilestoneEventUpdates AddNew()
		{
			return (ShippingBookingMilestoneEventUpdates)base.AddNew();
		}

		public new ShippingBookingMilestoneEventUpdates this[int i]
		{
			get { return (ShippingBookingMilestoneEventUpdates)base[i]; }
		}

		protected override MilestoneEventUpdatesCollection GetNewCollection()
		{
			return new ShippingBookingMilestoneEventUpdatesCollection();
		}
	}
}
