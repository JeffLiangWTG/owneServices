using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.UniversalDataBuss.Integration;
using NUnit.Framework;

namespace Enterprise.UniversalDataBuss.DataObjects.Core.Testing
{
	class ActionInfoTest : TestCaseWithFactory
	{
		[TestDate(2012, 12, 12)]
		public void TestConstruction()
		{
			var dummyBO = Factory.New<DummyBusinessObject>();
			var actionInfo = new ActionInfo(RecipientRoleType.NFP, dummyBO);

			CombineAssertions(delegate
			{
				AssertEquals("actionInfo.ActionType", "", actionInfo.ActionType);
				AssertEquals("actionInfo.FactoryForProcessing", Factory, actionInfo.FactoryForProcessing);
				AssertEquals("actionInfo.ParentBO", dummyBO, actionInfo.ParentBO);
				AssertEquals("actionInfo.PurposeCode", "", actionInfo.PurposeCode);
				AssertEquals("actionInfo.RecipientRoleDetails.Length", 1, actionInfo.RecipientRoleDetails.Length);
				AssertEquals("actionInfo.RecipientRoleDetails[0].Type", RecipientRoleType.NFP, actionInfo.RecipientRoleDetails[0].Type);
				AssertEquals("actionInfo.TriggerActualDate", ZDateTimeOffset.Now, actionInfo.TriggerActualDate);
				AssertEquals("actionInfo.TriggerCount", 0, actionInfo.TriggerCount);
				AssertEquals("actionInfo.TriggerDescription", "", actionInfo.TriggerDescription);
				AssertEquals("actionInfo.TriggerEventBranch", Env.CurrentBranch, actionInfo.TriggerEventBranch);
				AssertEquals("actionInfo.TriggerEventCode", "", actionInfo.TriggerEventCode);
				AssertEquals("actionInfo.TriggerEventDepartment", Env.CurrentDepartment, actionInfo.TriggerEventDepartment);
				AssertEquals("actionInfo.TriggerReference", "", actionInfo.TriggerReference);
				AssertEquals("actionInfo.TriggerEventUser", Env.CurrentUser, actionInfo.TriggerEventUser);
				AssertEquals("actionInfo.TriggerScheduledDate", ZDateTimeOffset.Empty, actionInfo.TriggerScheduledDate);
				AssertEquals("actionInfo.TriggerType", TriggerType.Manual, actionInfo.TriggerType);
			});
		}
	}
}
