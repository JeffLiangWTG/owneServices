using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(BookingMilestoneEventUpdatesCollection))]
	class BookingMilestoneEventUpdatesCollectionTest : MilestoneEventUpdatesCollectionTest<BookingMilestoneEventUpdatesCollection>
	{
		#region overrides

		protected override BookingMilestoneEventUpdatesCollection GetCollectionToTest()
		{
			return new BookingMilestoneEventUpdatesCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new BookingMilestoneEventUpdates("TST");
		}

		#endregion
	}
}
