using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.Messaging;

namespace Enterprise.Customs.IE.Business.Testing
{
	class DocumentsSendingActionLookupsTest : TestCaseWithFactory
	{
		public void TestSendingActionTypeList()
		{
			var lookups = documentsSendingAction.Lookups;
			AssertEquals("SendingActionTypeList.CodesAsString", AESOutgoingMessageTypeList.Codes.DocumentUpload, lookups.SendingActionTypeList.CodesAsString);

			var entryHeader = (CusEntryHeader)Factory.New<JobDeclaration>().ActiveEntryHeaders.AddNew();
			var anotherSendingAction = new DocumentsSendingAction(entryHeader);
			AssertSame("Should be cached.", lookups.SendingActionTypeList, anotherSendingAction.Lookups.SendingActionTypeList);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var entryHeader = (CusEntryHeader)Factory.New<JobDeclaration>().ActiveEntryHeaders.AddNew();
			documentsSendingAction = new DocumentsSendingAction(entryHeader);
		}
		DocumentsSendingAction documentsSendingAction;
	}
}
