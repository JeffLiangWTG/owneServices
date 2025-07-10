using CargoWise.EntityFramework;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business.Test
{
	[TestedType(typeof(BMNCNLevelingRuleChannelLink))]
	class BMNCNLevelingRuleChannelLinkTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var diagram = NetworkTestCase.CreateDiagram(Factory, isScaled: true);
			var rule = diagram.LevelingRules.AddNew();
			var channel = diagram.Channels.AddNew();

			return NetworkTestCase.CreateLevelingRuleChannelLink(rule, channel);
		}

		protected override void SetUp()
		{
			base.SetUp();

			BMSTestHelper.EnableBMSInRegistry();
		}
	}
}
