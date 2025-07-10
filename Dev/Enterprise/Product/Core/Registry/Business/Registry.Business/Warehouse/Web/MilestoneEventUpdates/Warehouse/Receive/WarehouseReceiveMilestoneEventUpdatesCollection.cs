using System.Xml.Serialization;
using CargoWise.EntityFramework;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class WarehouseReceiveMilestoneEventUpdatesCollection : MilestoneEventUpdatesCollection
	{
		public WarehouseReceiveMilestoneEventUpdatesCollection()
			: base()
		{
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new WarehouseReceiveMilestoneEventUpdates();
		}

		public new WarehouseReceiveMilestoneEventUpdates AddNew()
		{
			return (WarehouseReceiveMilestoneEventUpdates)base.AddNew();
		}

		public new WarehouseReceiveMilestoneEventUpdates this[int i]
		{
			get { return (WarehouseReceiveMilestoneEventUpdates)base[i]; }
		}

		protected override MilestoneEventUpdatesCollection GetNewCollection()
		{
			return new WarehouseReceiveMilestoneEventUpdatesCollection();
		}
	}
}
