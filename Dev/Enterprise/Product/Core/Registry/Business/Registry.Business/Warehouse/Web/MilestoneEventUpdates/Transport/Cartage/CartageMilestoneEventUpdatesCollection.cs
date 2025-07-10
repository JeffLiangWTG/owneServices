using System.Xml.Serialization;
using CargoWise.EntityFramework;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class CartageMilestoneEventUpdatesCollection : MilestoneEventUpdatesCollection
	{
		public CartageMilestoneEventUpdatesCollection()
			: base()
		{
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new CartageMilestoneEventUpdates();
		}

		public new CartageMilestoneEventUpdates AddNew()
		{
			return (CartageMilestoneEventUpdates)base.AddNew();
		}

		public new CartageMilestoneEventUpdates this[int i]
		{
			get { return (CartageMilestoneEventUpdates)base[i]; }
		}

		protected override MilestoneEventUpdatesCollection GetNewCollection()
		{
			return new CartageMilestoneEventUpdatesCollection();
		}
	}
}
