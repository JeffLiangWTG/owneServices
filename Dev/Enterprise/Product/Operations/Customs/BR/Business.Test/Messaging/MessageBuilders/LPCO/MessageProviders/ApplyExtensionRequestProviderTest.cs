using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.BR;

namespace Enterprise.Customs.BR.Business.LPCO.Testing
{
	class ApplyExtensionRequestProviderTest : TestCaseWithFactory
	{
		public void TestApplyExtensionRequestProvider()
		{
			var effectivedate = new ZDate(2021, 04, 30);
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.LPCO;

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_EntryStatus = "CUS";

			var messageObject = new LPCODeclarationMessageSendingObject(entryHeader);
			messageObject.MessageType = LPCOEntryActionCodeList.Codes.ALE;

			var provider = new ApplyExtensionRequestProvider(messageObject.RequestObject);
			CombineAssertions(() =>
			{
				AssertEquals(ZString.Empty, provider.Reason);
				AssertNull(provider.NewEffectiveDate);
			});

			messageObject.Reason = "Test Reason";
			messageObject.NewEffectiveDate = effectivedate;

			provider = new ApplyExtensionRequestProvider(messageObject.RequestObject);
			CombineAssertions(() =>
			{
				AssertEquals("Test Reason", provider.Reason);
				AssertEquals(effectivedate, provider.NewEffectiveDate);
			});
		}
	}
}
