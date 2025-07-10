using System;
using System.Drawing;
using CargoWise.EntityFramework;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business.Test
{
	[TestedType(typeof(BMNCNLevelingRule))]
	class BMNCNLevelingRuleTest : EnterpriseBusinessObjectTestCase
	{
		public void TestSaveRule_ForNonDiagram_ShouldThrowException()
		{
			var diagram = NetworkTestCase.CreateDiagram(Factory, isScaled: true);
			var shape = NetworkTestCase.CreateShape(diagram);

			Factory.Save();

			var rule = Factory.New<BMNCNLevelingRule>();

			rule.BNR_Name = "Don't walk up the northwest passage";
			rule.BNR_Type = "NWK";
			rule.BNR_BNS_Diagram = shape.PK;

			AssertExceptionThrown<NotSupportedException>(Factory.Save);

			rule.BNR_BNS_Diagram = diagram.PK;

			AssertNoExceptionThrown(Factory.Save);
		}

		public void TestRuleEffect()
		{
			var diagram = NetworkTestCase.CreateDiagram(Factory, isScaled: true);
			var rule = NetworkTestCase.CreateLevelingRule(diagram);

			void AssertRuleRuleEffect(string ruleType, string expectedDescription)
			{
				rule.BNR_Type = ruleType;

				AssertEquals(expectedDescription, rule.RuleEffect);
			}

			AssertRuleRuleEffect(string.Empty, string.Empty);

			rule.BNR_RuleValue = 69;

			AssertRuleRuleEffect(LevelingRuleTypeList.Codes.MaximumConcurrentEntities, "No more than 69 entities may be scheduled to run concurrently.");
			AssertRuleRuleEffect(LevelingRuleTypeList.Codes.MinimumEntityStartGapSize, "Entities may not be scheduled to start closer than 69 scale units from the start of the preceding entity.");
			AssertRuleRuleEffect(LevelingRuleTypeList.Codes.MinimumGapSizeBetweenEntities, "Entities may not be scheduled to start closer than 69 scale units from the end of the preceding entity.");

			rule.BNR_RuleValue = 1;

			AssertRuleRuleEffect(LevelingRuleTypeList.Codes.MaximumConcurrentEntities, "No more than 1 entity may be scheduled to run concurrently.");
			AssertRuleRuleEffect(LevelingRuleTypeList.Codes.MinimumEntityStartGapSize, "Entities may not be scheduled to start closer than 1 scale unit from the start of the preceding entity.");
			AssertRuleRuleEffect(LevelingRuleTypeList.Codes.MinimumGapSizeBetweenEntities, "Entities may not be scheduled to start closer than 1 scale unit from the end of the preceding entity.");

			rule.BNR_RuleValue = 0;

			AssertRuleRuleEffect(LevelingRuleTypeList.Codes.MaximumConcurrentEntities, string.Empty);
			AssertRuleRuleEffect(LevelingRuleTypeList.Codes.MinimumEntityStartGapSize, string.Empty);
			AssertRuleRuleEffect(LevelingRuleTypeList.Codes.MinimumGapSizeBetweenEntities, string.Empty);

			rule.BNR_RuleValue = -1;

			AssertRuleRuleEffect(LevelingRuleTypeList.Codes.MaximumConcurrentEntities, string.Empty);
			AssertRuleRuleEffect(LevelingRuleTypeList.Codes.MinimumEntityStartGapSize, string.Empty);
			AssertRuleRuleEffect(LevelingRuleTypeList.Codes.MinimumGapSizeBetweenEntities, string.Empty);
		}

		public void TestDelete_ShouldAlsoDeleteChannelLinks_ButLeaveChannel()
		{
			var diagram = NetworkTestCase.CreateDiagram(Factory, isScaled: true);
			var rule = NetworkTestCase.CreateLevelingRule(diagram);
			var channel = NetworkTestCase.CreateChannel(diagram);
			var link = NetworkTestCase.CreateLevelingRuleChannelLink(rule, channel);

			Factory.Save();

			rule.Delete();

			AssertEquals(true, link.IsDeleted);
			AssertEquals(false, channel.IsDeleted);
			AssertNoExceptionThrown(Factory.Save);
		}

		public void TestName_ShouldBeUniquePerDiagram()
		{
			var diagram1 = NetworkTestCase.CreateDiagram(Factory, isScaled: true);
			var diagram2 = NetworkTestCase.CreateDiagram(Factory, isScaled: true);

			var rule1 = NetworkTestCase.CreateLevelingRule(diagram1);
			var rule2 = NetworkTestCase.CreateLevelingRule(diagram2);

			AssertEquals(rule1.BNR_Name, rule2.BNR_Name);
			AssertNoExceptionThrown(Factory.Save);

			var rule3 = NetworkTestCase.CreateLevelingRule(diagram1);
			AssertExceptionThrown<ZSaveException>(Factory.Save);

			rule3.BNR_Name += "boop";

			AssertNoExceptionThrown(Factory.Save);
		}

		public void TestColorName()
		{
			var diagram = NetworkTestCase.CreateDiagram(Factory, isScaled: true);
			var rule = NetworkTestCase.CreateLevelingRule(diagram);

			rule.BNR_Color = 0;
			AssertEquals("Chartreuse", rule.ColorName);

			rule.ColorName = "Furious Greel";
			AssertEquals("Furious Greel", rule.ColorName);
			AssertEquals(0, rule.BNR_Color);

			rule.ColorName = "White";
			AssertEquals("White", rule.ColorName);
			AssertEquals((int)KnownColor.White, rule.BNR_Color);

			rule.BNR_Color = (int)KnownColor.Black;
			AssertEquals("Black", rule.ColorName);

			rule.ColorName = "Blanched Almond";
			AssertEquals("Multi word colour names should return same colour name", "Blanched Almond", rule.ColorName);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			BMSTestHelper.EnableBMSInRegistry();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var diagram = NetworkTestCase.CreateDiagram(Factory, isScaled: true);

			return diagram.LevelingRules.AddNew();
		}

		#endregion
	}
}
