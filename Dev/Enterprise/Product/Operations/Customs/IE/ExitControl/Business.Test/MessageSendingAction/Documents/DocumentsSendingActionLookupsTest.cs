using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IE.Messaging;

namespace Enterprise.Customs.IE.ExitControl.Business.Testing
{
	class DocumentsSendingActionLookupsTest : TestCaseWithFactory
	{
		public void TestSendingActionTypeList()
		{
			var lookups = documentsSendingAction.Lookups;
			AssertEquals("SendingActionTypeList.CodesAsString", AESOutgoingMessageTypeList.Codes.DocumentUpload, lookups.SendingActionTypeList.CodesAsString);

			var exitReport = Factory.New<CusExitHeader>().CusExitReports.AddNew();
			var anotherSendingAction = new DocumentsSendingAction(exitReport);
			AssertSame("Should be cached.", lookups.SendingActionTypeList, anotherSendingAction.Lookups.SendingActionTypeList);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var exitReport = Factory.New<CusExitHeader>().CusExitReports.AddNew();
			documentsSendingAction = new DocumentsSendingAction(exitReport);
		}
		DocumentsSendingAction documentsSendingAction;
	}
}
