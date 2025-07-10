using System.Xml.Serialization;
using CargoWise.EntityFramework;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class LinerAndAgencyContainerMilestoneEventUpdatesCollection : MilestoneEventUpdatesCollection
	{
		public LinerAndAgencyContainerMilestoneEventUpdatesCollection()
			: base()
		{
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new LinerAndAgencyContainerMilestoneEventUpdates();
		}

		public new LinerAndAgencyContainerMilestoneEventUpdates AddNew()
		{
			return (LinerAndAgencyContainerMilestoneEventUpdates)base.AddNew();
		}

		public new LinerAndAgencyContainerMilestoneEventUpdates this[int i]
		{
			get { return (LinerAndAgencyContainerMilestoneEventUpdates)base[i]; }
		}

		protected override MilestoneEventUpdatesCollection GetNewCollection()
		{
			return new LinerAndAgencyContainerMilestoneEventUpdatesCollection();
		}
	}
}
