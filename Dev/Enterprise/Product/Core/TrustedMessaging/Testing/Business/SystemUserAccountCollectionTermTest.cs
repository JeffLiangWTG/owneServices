using CargoWise.EntityFramework.Testing;
using Enterprise.TrustedMessaging.Business;

namespace Enterprise.TrustedMessaging.Testing
{
	public class SystemUserAccountCollectionTermTest : TestCaseWithFactory
	{
		public void TestImplements()
		{
			var term = new SystemUserAccountCollectionTermForTest();
			AssertEquals("UAC", term.Type);
			AssertEquals(true, term.IsCurrentUserAllowedToAcknowledgeAgreement);
			AssertEquals(null, term.ErrorMessageForAcknowledgementNotAllowed);
			AssertEquals(true, term.IsLocalDisplayConditionSatisfied_Expoesed());
		}

		public class SystemUserAccountCollectionTermForTest : SystemUserAccountCollectionTerm
		{
			public bool IsLocalDisplayConditionSatisfied_Expoesed() => base.IsLocalDisplayConditionSatisfied();
		}
	}
}