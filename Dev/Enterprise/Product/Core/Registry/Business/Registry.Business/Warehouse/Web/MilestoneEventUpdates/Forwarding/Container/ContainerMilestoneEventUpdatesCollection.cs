using System.Xml.Serialization;
using CargoWise.EntityFramework;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class ContainerMilestoneEventUpdatesCollection : MilestoneEventUpdatesCollection
	{
		public ContainerMilestoneEventUpdatesCollection()
			: base()
		{
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new ContainerMilestoneEventUpdates();
		}

		public new ContainerMilestoneEventUpdates AddNew()
		{
			return (ContainerMilestoneEventUpdates)base.AddNew();
		}

		public new ContainerMilestoneEventUpdates this[int i]
		{
			get { return (ContainerMilestoneEventUpdates)base[i]; }
		}

		protected override MilestoneEventUpdatesCollection GetNewCollection()
		{
			return new ContainerMilestoneEventUpdatesCollection();
		}
	}
}
