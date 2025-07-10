using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.BR;

namespace Enterprise.Customs.BR.Business.LPCO.Testing
{
	class CompatibilityRequestProviderTest : TestCaseWithFactory
	{
		public void TestCancelRectificationRequestProvider()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.LPCO;

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_EntryStatus = "CUS";

			var messageObject = new LPCODeclarationMessageSendingObject(entryHeader);
			messageObject.MessageType = LPCOEntryActionCodeList.Codes.COM;
			messageObject.EntryNumber = "20BR0000000000";
			messageObject.EntryLineNumber = 1;
			messageObject.Version = "1.1";
			messageObject.Reason = "Test Reason";

			var provider = new CompatibilityRequestProvider(messageObject.RequestObject);
			CombineAssertions(() =>
			{
				AssertEquals("Test Reason", provider.Reason);
				AssertEquals("20BR0000000000", provider.DocumentNumber);
				AssertEquals(1, provider.DocumentItemNumber);
				AssertEquals("1.1", provider.Version);
			});

			messageObject.MessageType = LPCOEntryActionCodeList.Codes.ORI;
			CombineAssertions(() =>
			{
				AssertEquals(ZString.Empty, provider.DocumentNumber);
				AssertEquals(ZInt.Zero, provider.DocumentItemNumber);
				AssertEquals(ZString.Empty, provider.Version);
				AssertEquals(ZString.Empty, provider.Reason);
			});
		}
	}
}
