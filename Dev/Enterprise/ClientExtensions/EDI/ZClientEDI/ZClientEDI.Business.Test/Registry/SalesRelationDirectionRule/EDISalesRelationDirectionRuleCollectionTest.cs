using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.Business.Test
{
	[TestedType(typeof(EDISalesRelationDirectionRuleCollection))]
	public class EDISalesRelationDirectionRuleCollectionTest : NonPersistentBusinessObjectCollectionTestCase<EDISalesRelationDirectionRuleCollection>
	{
		protected override EDISalesRelationDirectionRuleCollection GetCollectionToTest()
		{
			return new EDISalesRelationDirectionRuleCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var collection = new EDISalesRelationDirectionRuleCollection();
			var rule = collection.AddNew();

			return rule;
		}
	}
}
