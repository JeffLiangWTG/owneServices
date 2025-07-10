using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IE.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	[TestedType(typeof(GuaranteeAccessCodesSendingAction))]
	sealed class GuaranteeAccessCodesSendingActionTest : NonPersistentBusinessObjectTestCase
	{
		public void TestAddMessage()
		{
			AssertEquals(0, sendingAction.CusGuaranteeHeader.Messages.Count);
			var message = Factory.New<NCTSOutboundEDIMessage>();
			sendingAction.AddMessage(message);
			AssertEquals(1, sendingAction.CusGuaranteeHeader.Messages.Count);
		}

		public void TestSenderType()
		{
			AssertType("Should have defined a correct SenderType.", typeof(NctsMessageSender), sendingAction.CreateSender());
		}

		protected override BusinessObject GetNewBusinessObject() => new GuaranteeAccessCodesSendingAction(cusGuaranteeHeader);

		protected override void SetUp()
		{
			base.SetUp();
			cusGuaranteeHeader = Factory.NewWithValidTestData<CusGuaranteeHeader>();
			sendingAction = new GuaranteeAccessCodesSendingAction(cusGuaranteeHeader);
		}

		CusGuaranteeHeader cusGuaranteeHeader;
		GuaranteeAccessCodesSendingAction sendingAction;
	}
}
