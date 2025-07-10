using System.Xml.Serialization;
using CargoWise.EntityFramework;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class OrderMilestoneEventUpdatesCollection : MilestoneEventUpdatesCollection
	{
		public OrderMilestoneEventUpdatesCollection()
			: base()
		{
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new OrderMilestoneEventUpdates();
		}

		public new OrderMilestoneEventUpdates AddNew()
		{
			return (OrderMilestoneEventUpdates)base.AddNew();
		}

		public new OrderMilestoneEventUpdates this[int i]
		{
			get { return (OrderMilestoneEventUpdates)base[i]; }
		}

		protected override MilestoneEventUpdatesCollection GetNewCollection()
		{
			return new OrderMilestoneEventUpdatesCollection();
		}
	}
}
