using CargoWise.EntityFramework;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.VisualBoards.Business.Test;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Module.Test
{
	[TestedType(typeof(BMTagRuleController))]
	class BMTagRuleControllerTest : BMControllerTest
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.BMTagRule;
		}

		public void TestGetForm()
		{
			var definition1 = BMSTestHelper.CreateTagDefinition(Factory, "AAA");
			var tagMagnitude1 = BMSTestHelper.CreateTagMagnitude(definition1, "Ma1");
			var rule1 = BMSTestHelper.CreateTagRule(tagMagnitude1, "Later Ron", TagRuleActionTypeList.Codes.AddTag);
			using (var form = new BMTagRuleController().ShowFormForNewEntity(rule1))
			{
				AssertNotNull(form);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			BMSTestHelper.EnableBMSInRegistry();
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			var definition1 = BMSTestHelper.CreateTagDefinition(Factory, "AAA");
			var tagMagnitude1 = BMSTestHelper.CreateTagMagnitude(definition1, "Ma1");
			var rule1 = BMSTestHelper.CreateTagRule(tagMagnitude1, "Later Ron", TagRuleActionTypeList.Codes.AddTag);

			Factory.Save();

			return rule1;
		}

		public override void TestDeleteForm()
		{
			var strategy = new TaskTrackingAsyncBaseStrategy();
			using (VisualBoardsTestCase.ApplyAsyncStrategy(strategy))
			{
				base.TestDeleteForm();
				strategy.AwaitAll(taskToIgnore: null);
			}
		}
	}
}
