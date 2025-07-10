using System.Xml.Serialization;
using CargoWise.EntityFramework;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class ISFMilestoneEventUpdatesCollection : MilestoneEventUpdatesCollection
	{
		public ISFMilestoneEventUpdatesCollection()
			: base()
		{
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new ISFMilestoneEventUpdates();
		}

		public new ISFMilestoneEventUpdates AddNew()
		{
			return (ISFMilestoneEventUpdates)base.AddNew();
		}

		public new ISFMilestoneEventUpdates this[int i]
		{
			get { return (ISFMilestoneEventUpdates)base[i]; }
		}

		protected override MilestoneEventUpdatesCollection GetNewCollection()
		{
			return new ISFMilestoneEventUpdatesCollection();
		}
	}
}
