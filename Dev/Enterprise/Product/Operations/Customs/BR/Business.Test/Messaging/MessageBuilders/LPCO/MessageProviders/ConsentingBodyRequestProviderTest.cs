using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.BR;

namespace Enterprise.Customs.BR.Business.LPCO.Testing
{
	class ConsentingBodyRequestProviderTest : TestCaseWithFactory
	{
		public void TestConsentingBodyRequestProvider()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.LPCO;

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_EntryStatus = "CUS";

			var messageObject = new LPCODeclarationMessageSendingObject(entryHeader);
			messageObject.MessageType = LPCOEntryActionCodeList.Codes.MSG;
			messageObject.Message = "Test Message";

			var provider = new ConsentingBodyRequestProvider(messageObject.RequestObject);
			CombineAssertions(() =>
			{
				AssertEquals("Test Message", provider.Message);
			});
		}
	}
}
