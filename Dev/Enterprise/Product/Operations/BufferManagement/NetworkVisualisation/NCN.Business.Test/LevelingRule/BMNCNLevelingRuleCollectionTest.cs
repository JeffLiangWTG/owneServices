using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business.Test
{
	[TestedType(typeof(BMNCNLevelingRuleCollection))]
	class BMNCNLevelingRuleCollectionTest : ActiveBusinessObjectCollectionTestCase<BMNCNLevelingRuleCollection>
	{
		public void TestCollectionContents_ShouldFilterByParentShape()
		{
			var diagram1 = NetworkTestCase.CreateDiagram(Factory, isScaled: true);
			var diagram2 = NetworkTestCase.CreateDiagram(Factory, isScaled: true);

			var rule1 = Factory.NewWithValidTestData<BMNCNLevelingRule>();
			var rule2 = Factory.NewWithValidTestData<BMNCNLevelingRule>();
			var rule3 = Factory.NewWithValidTestData<BMNCNLevelingRule>();

			rule1.BNR_BNS_Diagram = diagram1.PK;
			rule2.BNR_BNS_Diagram = diagram2.PK;
			rule3.BNR_BNS_Diagram = ZGuid.Empty;

			AssertContainsExactElementsInAnyOrder(new[] { rule1 }, diagram1.LevelingRules);
			AssertContainsExactElementsInAnyOrder(new[] { rule2 }, diagram2.LevelingRules);
		}

		protected override BMNCNLevelingRuleCollection GetCollectionToTest()
		{
			return NetworkTestCase.CreateDiagram(Factory, isScaled: true).LevelingRules;
		}
	}
}
