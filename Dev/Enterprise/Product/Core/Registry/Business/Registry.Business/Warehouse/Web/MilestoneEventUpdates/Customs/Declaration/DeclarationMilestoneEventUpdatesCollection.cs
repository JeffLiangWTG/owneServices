using System.Xml.Serialization;
using CargoWise.EntityFramework;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class DeclarationMilestoneEventUpdatesCollection : MilestoneEventUpdatesCollection
	{
		public DeclarationMilestoneEventUpdatesCollection()
			: base()
		{
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new DeclarationMilestoneEventUpdates();
		}

		public new DeclarationMilestoneEventUpdates AddNew()
		{
			return (DeclarationMilestoneEventUpdates)base.AddNew();
		}

		public new DeclarationMilestoneEventUpdates this[int i]
		{
			get { return (DeclarationMilestoneEventUpdates)base[i]; }
		}

		protected override MilestoneEventUpdatesCollection GetNewCollection()
		{
			return new DeclarationMilestoneEventUpdatesCollection();
		}
	}
}
