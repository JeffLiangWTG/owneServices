using System.Xml.Serialization;
using CargoWise.EntityFramework;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class WarehouseOrderMilestoneEventUpdatesCollection : MilestoneEventUpdatesCollection
	{
		public WarehouseOrderMilestoneEventUpdatesCollection()
			: base()
		{
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new WarehouseOrderMilestoneEventUpdates();
		}

		public new WarehouseOrderMilestoneEventUpdates AddNew()
		{
			return (WarehouseOrderMilestoneEventUpdates)base.AddNew();
		}

		public new WarehouseOrderMilestoneEventUpdates this[int i]
		{
			get { return (WarehouseOrderMilestoneEventUpdates)base[i]; }
		}

		protected override MilestoneEventUpdatesCollection GetNewCollection()
		{
			return new WarehouseOrderMilestoneEventUpdatesCollection();
		}
	}
}
