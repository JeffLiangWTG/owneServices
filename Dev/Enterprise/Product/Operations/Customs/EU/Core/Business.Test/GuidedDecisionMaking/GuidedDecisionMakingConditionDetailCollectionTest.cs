using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Testing
{
	[TestedType(typeof(GuidedDecisionMakingConditionDetailCollection))]
	sealed class GuidedDecisionMakingConditionDetailCollectionTest : NonPersistentBusinessObjectCollectionTestCase<GuidedDecisionMakingConditionDetailCollection>
	{
		protected override GuidedDecisionMakingConditionDetailCollection GetCollectionToTest()
		{
			var condition = new GuidedDecisionMakingCondition();
			return new GuidedDecisionMakingConditionDetailCollection(condition);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection() => new GuidedDecisionMakingConditionDetail();

		public void TestSetParentForNewElement()
		{
			var collection = GetCollectionToTest();
			Assert("Condition who is the parent of the GuidedDecisionMakingConditionDetailCollection should be added into the Parents collection of the added conditionDetail.", collection.AddNew().Parents.Contains(collection.Parent));
		}
	}
}
