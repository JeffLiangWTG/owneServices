using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.FR.Business.NCTS;
using Enterprise.Customs.FR.NCTS.Messaging;

namespace Enterprise.Customs.FR.GUI.NCTS.Testing
{
	class NctsPhase5MessagingMenuProviderTest : TestCaseWithFactory
	{
		public void TestGetProvider()
		{
			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			header.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			AssertType<NctsPhase5MessagingMenuProvider>("Provider Type", EU.NCTS.GUI.Phase5MessagingMenuProvider.GetProvider(header));
		}

		public void TestMessageSendingForm()
		{
			var header = Factory.New<NctsHeader>();
			var sendingObjectParent = new TP5MessageSendingObjectParent(header);

			var menuProvider = new NctsPhase5MessagingMenuProviderForTest(header);
			using var messageSendingForm = menuProvider.GetMessageSendingFormExposed(sendingObjectParent);
			AssertType<MessageSendingForm>("Message Sending Form Type", messageSendingForm);
		}

		class NctsPhase5MessagingMenuProviderForTest : NctsPhase5MessagingMenuProvider
		{
			public NctsPhase5MessagingMenuProviderForTest(NctsHeader header) : base(header)
			{
			}

			public EU.NCTS.GUI.MessageSendingForm GetMessageSendingFormExposed(TP5MessageSendingObjectParent sendingObjectParent)
				=> GetMessageSendingFormCore(sendingObjectParent);
		}
	}
}
