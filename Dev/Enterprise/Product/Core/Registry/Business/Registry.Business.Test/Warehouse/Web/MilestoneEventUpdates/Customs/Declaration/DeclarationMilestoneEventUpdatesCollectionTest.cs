using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(DeclarationMilestoneEventUpdatesCollection))]
	sealed class DeclarationMilestoneEventUpdatesCollectionTest : MilestoneEventUpdatesCollectionTest<DeclarationMilestoneEventUpdatesCollection>
	{
		#region overrides

		protected override DeclarationMilestoneEventUpdatesCollection GetCollectionToTest()
		{
			return new DeclarationMilestoneEventUpdatesCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new DeclarationMilestoneEventUpdates("TST");
		}

		#endregion
	}
}
