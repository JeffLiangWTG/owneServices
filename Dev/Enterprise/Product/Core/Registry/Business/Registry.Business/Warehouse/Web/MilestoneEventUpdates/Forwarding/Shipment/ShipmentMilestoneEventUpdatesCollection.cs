using System.Xml.Serialization;
using CargoWise.EntityFramework;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class ShipmentMilestoneEventUpdatesCollection : MilestoneEventUpdatesCollection
	{
		public ShipmentMilestoneEventUpdatesCollection()
			: base()
		{
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new ShipmentMilestoneEventUpdates();
		}

		public new ShipmentMilestoneEventUpdates AddNew()
		{
			return (ShipmentMilestoneEventUpdates)base.AddNew();
		}

		public new ShipmentMilestoneEventUpdates this[int i]
		{
			get { return (ShipmentMilestoneEventUpdates)base[i]; }
		}

		protected override MilestoneEventUpdatesCollection GetNewCollection()
		{
			return new ShipmentMilestoneEventUpdatesCollection();
		}
	}
}
