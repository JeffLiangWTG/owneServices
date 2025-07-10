using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.BR;

namespace Enterprise.Customs.BR.Business.LPCO.Testing
{
	class AcceptCompatibilityRequestProviderTest : TestCaseWithFactory
	{
		public void TestAcceptCompatibilityRequestProvider()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.LPCO;

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_EntryStatus = "CUS";

			var messageObject = new LPCODeclarationMessageSendingObject(entryHeader);
			messageObject.MessageType = LPCOEntryActionCodeList.Codes.REQ;
			messageObject.Reason = "Test Reason";

			var provider = new AcceptCompatibilityRequestProvider(messageObject.RequestObject);
			CombineAssertions(() =>
			{
				AssertEquals("Test Reason", provider.Reason);
			});
		}
	}
}
