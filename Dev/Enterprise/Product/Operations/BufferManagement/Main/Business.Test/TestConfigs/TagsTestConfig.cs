using System.Drawing;
using CargoWise.EntityFramework;

namespace Enterprise.BufferManagement.Business.Test
{
	public class TagsTestConfig : VisualBoardTestConfig
	{
		protected TagsTestConfig(BusinessObjectFactory factory, string workflowType, bool shouldUseExistingSystem)
			: base(factory, new[] { workflowType }, "WTGDEV", shouldUseExistingSystem)
		{
			PriorityTags = BMSTestHelper.CreateTagDefinition(factory, "PRI", "Priority Tags", isExclusive: true, scope: TagScopeList.Codes.Workflow);
			GreenTag = BMSTestHelper.CreateTagMagnitude(PriorityTags, "GRN", "CCPM Project", nudge: 400, color: Color.LawnGreen);
			PlatinumTag = BMSTestHelper.CreateTagMagnitude(PriorityTags, "PLT", "Executive Override", nudge: 400, color: Color.DodgerBlue);
			RedTag = BMSTestHelper.CreateTagMagnitude(PriorityTags, "RED", "Defect", nudge: 200, color: Color.Pink);
			GoldTag = BMSTestHelper.CreateTagMagnitude(PriorityTags, "GLD", "Gold", nudge: 100, color: Color.Yellow);

			PlatinumTag.ApplyColorToBackground = true;
			RedTag.ApplyColorToBackground = true;

			PonyTags = BMSTestHelper.CreateTagDefinition(factory, "PON", "Ponies");
			PrincessCelestiaTag = BMSTestHelper.CreateTagMagnitude(PonyTags, "CEL", "Princess Celestia", nudge: 1000, color: Color.LightGoldenrodYellow);
			DerpyHoovesTag = BMSTestHelper.CreateTagMagnitude(PonyTags, "DRP", "Derpy Hooves", nudge: -1000, color: Color.LightSlateGray);
			RainbowDashTag = BMSTestHelper.CreateTagMagnitude(PonyTags, "RBD", "Rainbow Dash", nudge: 10, color: Color.LightBlue);
			PrincessLunaTag = BMSTestHelper.CreateTagMagnitude(PonyTags, "PRL", "Princess Luna", nudge: 20, color: Color.MidnightBlue);

			RuleTags = BMSTestHelper.CreateTagDefinition(factory, "RUL", "Rules", false, "RUL");
			RuleTag = BMSTestHelper.CreateTagMagnitude(RuleTags, "RUL", "Rule Test", nudge: 15, color: Color.DarkRed);
		}

		internal static TagsTestConfig Create(BusinessObjectFactory factory, string workflowType, bool shouldUseExistingSystem)
		{
			return new TagsTestConfig(factory, workflowType, shouldUseExistingSystem);
		}

		public TagDefinition PriorityTags { get; private set; }
		public TagMagnitude GreenTag { get; private set; }
		public TagMagnitude PlatinumTag { get; private set; }
		public TagMagnitude RedTag { get; private set; }
		public TagMagnitude GoldTag { get; private set; }

		public TagDefinition PonyTags { get; private set; }
		public TagMagnitude PrincessCelestiaTag { get; private set; }
		public TagMagnitude DerpyHoovesTag { get; private set; }
		public TagMagnitude RainbowDashTag { get; private set; }
		public TagMagnitude PrincessLunaTag { get; private set; }

		public TagDefinition RuleTags { get; private set; }
		public TagMagnitude RuleTag { get; private set; }
	}
}
