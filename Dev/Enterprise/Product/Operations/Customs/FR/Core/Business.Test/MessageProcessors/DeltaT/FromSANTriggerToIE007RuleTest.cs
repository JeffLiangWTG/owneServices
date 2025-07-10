using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.NCTS.Business.MessageGeneration;
using Enterprise.Customs.FR.Business.NCTS;

namespace Enterprise.Customs.FR.Business.MessageProcessors.Testing
{
	public class FromSANTriggerToIE007RuleTest : TestCaseWithFactory
	{
		public void TestRuleMessageFunction()
		{
			var rule = new FromSANTriggerToIE007Rule();
			AssertType<NctsMessageFunctionSet.ArrivalNotificationMessage>(rule.NewMessageFunction);
		}

		public void TestCanSendMessage()
		{
			var rule = new FromSANTriggerToIE007Rule();
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Arrival);
			Assert(rule.CanSendMessage(header));
		}
	}
}
