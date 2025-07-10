using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(ISFMilestoneEventUpdatesCollection))]
	sealed class ISFMilestoneEventUpdatesCollectionTest : MilestoneEventUpdatesCollectionTest<ISFMilestoneEventUpdatesCollection>
	{
		#region overrides

		protected override ISFMilestoneEventUpdatesCollection GetCollectionToTest()
		{
			return new ISFMilestoneEventUpdatesCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new ISFMilestoneEventUpdates("TST");
		}

		#endregion
	}
}
