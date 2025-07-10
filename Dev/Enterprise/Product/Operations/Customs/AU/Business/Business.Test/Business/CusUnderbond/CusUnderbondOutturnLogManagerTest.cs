using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public class CusUnderbondOutturnLogManagerTest : TestCaseWithFactory
	{
		public void TestLogsForNominatedEvent()
		{
			AssertEquals("Nominated Event - SplitMessageFailed", Events.Cancelled, testManager.SplitMessageFailedLog.NominatedEvent);
			AssertEquals("Nominated Event - NonExistantLine", Events.UnderbondOutturnRejected, testManager.NonExistantLineLog.NominatedEvent);
		}

		public void TestHasSplitMessageOriginalRejectedLog()
		{
			AssertEquals("Has no outstanding amendments yet", false, testManager.HasSplitMessageOriginalRejectedLog);

			StmALog newLog = dummySupporter.Logs.AddNew(Events.UnderbondSplitOutturnOriginalRejected, "Test");
			AssertEquals("Has Outturn SplitMessageOriginalRejectedLog", true, testManager.HasSplitMessageOriginalRejectedLog);

			newLog.Cancel();
			AssertEquals("Has no Outturn SplitMessageOriginalRejectedLog", 0, testManager.SplitMessageOriginalRejectedLog.Count);
			AssertEquals("Has no Outturn SplitMessageOriginalRejectedLog", false, testManager.HasSplitMessageOriginalRejectedLog);
		}

		public void TestHasOutturnSplitMessageFailedLog()
		{
			AssertEquals("Has no outstanding amendments yet", false, testManager.HasSplitMessageFailedLog);

			StmALog newLog = dummySupporter.Logs.AddNew(Events.Cancelled, "Test");
			AssertEquals("Has Outturn SplitMessageFailedLog", true, testManager.HasSplitMessageFailedLog);

			newLog.Cancel();
			AssertEquals("Has no Outturn SplitMessageFailedLog", 0, testManager.SplitMessageFailedLog.Count);
			AssertEquals("Has no Outturn SplitMessageFailedLog", false, testManager.HasSplitMessageFailedLog);
		}

		public void TestHasNonExistantLineAtCustoms()
		{
			AssertEquals("Has no outturn HasNonExistantLineAtCustoms", false, testManager.HasNonExistantLineAtCustoms);
			dummySupporter.Logs.AddNew(Events.UnderbondOutturnRejected, "Partial Amendment Received");
			AssertEquals("Has HasNonExistantLineAtCustoms", true, testManager.HasNonExistantLineAtCustoms);
		}

		CusUnderbond dummySupporter;
		CusUnderbondOutturnLogManager testManager;

		protected override void SetUp()
		{
			base.SetUp();
			dummySupporter = Factory.New<CusUnderbond>();
			testManager = new CusUnderbondOutturnLogManager(dummySupporter);
		}
	}
}
