using CargoWise.EntityFramework.Testing;
using Enterprise.BufferManagement.Business.Test;
using NUnit.Framework;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business.Test
{
	[TestedType(typeof(BMNCNLevelingRuleChannelLinkCollection))]
	class BMNCNLevelingRuleChannelLinkCollectionTest : ActiveBusinessObjectCollectionTestCase<BMNCNLevelingRuleChannelLinkCollection>
	{
		public void TestCollectionContents_ShouldFilterByRule()
		{
			var diagram1 = NetworkTestCase.CreateDiagram(Factory, isScaled: true);
			var diagram2 = NetworkTestCase.CreateDiagram(Factory, isScaled: true);

			var channel1 = NetworkTestCase.CreateChannel(diagram1, "Le boop");
			var channel2 = NetworkTestCase.CreateChannel(diagram2, "Teh snoot");

			var rule1 = NetworkTestCase.CreateLevelingRule(diagram1, name: "Trouble");
			var rule2 = NetworkTestCase.CreateLevelingRule(diagram2, name: "Tribbles");

			Factory.Save();

			var link1_1 = NetworkTestCase.CreateLevelingRuleChannelLink(rule1, channel1);
			var link1_2 = NetworkTestCase.CreateLevelingRuleChannelLink(rule1, channel2);
			var link2_1 = NetworkTestCase.CreateLevelingRuleChannelLink(rule2, channel1);

			AssertContainsExactElementsInAnyOrder(new[] { link1_1, link1_2, }, rule1.ChannelLinks);
			AssertContainsExactElementsInAnyOrder(new[] { link2_1 }, rule2.ChannelLinks);

			AssertNoExceptionThrown(Factory.Save);
		}

		#region Implementation

		protected override BMNCNLevelingRuleChannelLinkCollection GetCollectionToTest()
		{
			return NetworkTestCase.CreateLevelingRule(NetworkTestCase.CreateDiagram(Factory, isScaled: true)).ChannelLinks;
		}

		protected override void SetUp()
		{
			base.SetUp();

			BMSTestHelper.EnableBMSInRegistry();
		}

		#endregion
	}
}
