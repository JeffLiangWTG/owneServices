using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class SendWithdrawalMessageProcessorTest : TestCaseWithFactory
	{
		public void TestProcess()
		{
			var mawb = Factory.New<CusMAWB>();
			var hawb = mawb.ChildBills.AddNew();
			var processor = new SendWithdrawalMessageProcessor(hawb);
			processor.Process(new NotificationBuffer());

			hawb.Messages.Load();
			AssertEquals("Message Sent", 1, hawb.Messages.Count);
			AssertEquals("WDW", hawb.Messages[0].EM_MessageSubType);
		}
	}
}
