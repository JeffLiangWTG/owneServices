using System.Xml.Serialization;
using CargoWise.EntityFramework;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class BookingMilestoneEventUpdatesCollection : MilestoneEventUpdatesCollection
	{
		public BookingMilestoneEventUpdatesCollection()
			: base()
		{
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new BookingMilestoneEventUpdates();
		}

		public new BookingMilestoneEventUpdates AddNew()
		{
			return (BookingMilestoneEventUpdates)base.AddNew();
		}

		public new BookingMilestoneEventUpdates this[int i]
		{
			get { return (BookingMilestoneEventUpdates)base[i]; }
		}

		protected override MilestoneEventUpdatesCollection GetNewCollection()
		{
			return new BookingMilestoneEventUpdatesCollection();
		}
	}
}
