using System.Xml.Serialization;
using CargoWise.EntityFramework;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class BillOfLadingMilestoneEventUpdatesCollection : MilestoneEventUpdatesCollection
	{
		public BillOfLadingMilestoneEventUpdatesCollection()
			: base()
		{
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new BillOfLadingMilestoneEventUpdates();
		}

		public new BillOfLadingMilestoneEventUpdates AddNew()
		{
			return (BillOfLadingMilestoneEventUpdates)base.AddNew();
		}

		public new BillOfLadingMilestoneEventUpdates this[int i]
		{
			get { return (BillOfLadingMilestoneEventUpdates)base[i]; }
		}

		protected override MilestoneEventUpdatesCollection GetNewCollection()
		{
			return new BillOfLadingMilestoneEventUpdatesCollection();
		}
	}
}
