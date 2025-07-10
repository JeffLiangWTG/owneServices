using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.EU.EMCS.Business.Testing
{
	sealed class ExplanationOnDelaySendingActionLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestExplanationCodeList()
		{
			CombineAssertions(() =>
			{
				var explanationCodeList = lookups.ExplanationCodeList;
				AssertEquals("List contents", "0, 1, 2, 3, 4, 5, 6", explanationCodeList.CodesAsString);
				AssertSame("cached", Factory.GetCachedValue<EMCSExplanationOnDelayCodeList>(), explanationCodeList);
			});
		}

		public void TestMessageRoleList()
		{
			var messageRoleList = lookups.MessageRoleList;
			AssertEquals("List contents", "1, 2", messageRoleList.CodesAsString);
			AssertSame("cached", Factory.GetCachedValue<EMCSExplanationOnDelayMessageRoleCodeList>(), messageRoleList);
		}

		protected override void SetUp()
		{
			base.SetUp();
			lookups = new ExplanationOnDelaySendingActionLookups(new ExplanationOnDelaySendingAction(Factory.New<EMCSJobDeclaration>()));
		}
		ExplanationOnDelaySendingActionLookups lookups;
	}
}
