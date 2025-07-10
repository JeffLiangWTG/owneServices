using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using static Enterprise.Customs.EU.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.EU.Business.CusTempStorage.Testing
{
	sealed class TemporaryStorageMessagingProviderBaseOnlyTest : TestCaseWithFactory
	{
		public void TestGetMessageType()
		{
			var temporaryStorageHeader = Factory.New<TemporaryStorageHeader>();
			var temporaryStorageProvider = new InitializiableTemporaryStorageMessagingProvider();

			temporaryStorageHeader.CustomsStatus = ZString.Empty;

			CombineAssertions("GetMessageTypes depend on temporaryStorageHeader CustomsStatus, CustomsStatus is empty", () =>
			{
				temporaryStorageHeader.AMA_MessageType = PNTSMessageTypeList.Codes.CombinedTemporaryStorage;
				AssertContainsExactElementsInAnyOrder("CombinedTemporaryStorage", new ZString[] { TemporaryStorageMessageTypeList.Codes.CombinedTSD }, temporaryStorageProvider.GetMessageTypes(temporaryStorageHeader).GetAllCodes());

				temporaryStorageHeader.AMA_MessageType = PNTSMessageTypeList.Codes.PreLodgedTempStorage;
				AssertContainsExactElementsInAnyOrder("PreLodgedTempStorage", new ZString[] { TemporaryStorageMessageTypeList.Codes.PreLodgedTSD }, temporaryStorageProvider.GetMessageTypes(temporaryStorageHeader).GetAllCodes());

				temporaryStorageHeader.AMA_MessageType = PNTSMessageTypeList.Codes.PresentationNotification;
				AssertContainsExactElementsInAnyOrder("PresentationNotification", new ZString[] { TemporaryStorageMessageTypeList.Codes.PresentationNotification }, temporaryStorageProvider.GetMessageTypes(temporaryStorageHeader).GetAllCodes());
			});

			CombineAssertions("GetMessageTypes depend on temporaryStorageHeader CustomsStatus, CustomsStatus is TemporaryStoragePreLodged", () =>
			{
				temporaryStorageHeader.CustomsStatus = PNTS.CustomsStatus.TemporaryStoragePreLodged;
				temporaryStorageHeader.AMA_MessageType = PNTSMessageTypeList.Codes.CombinedTemporaryStorage;
				AssertContainsExactElementsInAnyOrder("CombinedTemporaryStorage", new ZString[] { TemporaryStorageMessageTypeList.Codes.AmendmentRequestTSD, TemporaryStorageMessageTypeList.Codes.InvalidationRequestTSD, TemporaryStorageMessageTypeList.Codes.PresentationNotification }, temporaryStorageProvider.GetMessageTypes(temporaryStorageHeader).GetAllCodes());

				temporaryStorageHeader.AMA_MessageType = PNTSMessageTypeList.Codes.PreLodgedTempStorage;
				AssertContainsExactElementsInAnyOrder("PreLodgedTempStorage", new ZString[] { TemporaryStorageMessageTypeList.Codes.AmendmentRequestTSD, TemporaryStorageMessageTypeList.Codes.InvalidationRequestTSD }, temporaryStorageProvider.GetMessageTypes(temporaryStorageHeader).GetAllCodes());

				temporaryStorageHeader.AMA_MessageType = PNTSMessageTypeList.Codes.PresentationNotification;
				AssertContainsExactElementsInAnyOrder("PresentationNotification", new ZString[] { TemporaryStorageMessageTypeList.Codes.PresentationNotification }, temporaryStorageProvider.GetMessageTypes(temporaryStorageHeader).GetAllCodes());
			});

			CombineAssertions("GetMessageTypes depend on temporaryStorageHeader CustomsStatus, CustomsStatus is TemporaryStorageActivated", () =>
			{
				temporaryStorageHeader.CustomsStatus = PNTS.CustomsStatus.TemporaryStorageActivated;
				temporaryStorageHeader.AMA_MessageType = PNTSMessageTypeList.Codes.CombinedTemporaryStorage;
				AssertContainsExactElementsInAnyOrder("CombinedTemporaryStorage", new ZString[] { TemporaryStorageMessageTypeList.Codes.AmendmentRequestTSD }, temporaryStorageProvider.GetMessageTypes(temporaryStorageHeader).GetAllCodes());

				temporaryStorageHeader.AMA_MessageType = PNTSMessageTypeList.Codes.PreLodgedTempStorage;
				AssertContainsExactElementsInAnyOrder("PreLodgedTempStorage", new ZString[] { TemporaryStorageMessageTypeList.Codes.AmendmentRequestTSD }, temporaryStorageProvider.GetMessageTypes(temporaryStorageHeader).GetAllCodes());

				temporaryStorageHeader.AMA_MessageType = PNTSMessageTypeList.Codes.PresentationNotification;
				AssertContainsExactElementsInAnyOrder("PresentationNotification", new ZString[] { TemporaryStorageMessageTypeList.Codes.AmendmentRequestTSD }, temporaryStorageProvider.GetMessageTypes(temporaryStorageHeader).GetAllCodes());
			});
		}
	}

	class InitializiableTemporaryStorageMessagingProvider : TemporaryStorageMessagingProvider
	{
		protected override TemporaryStorageMessageBuilder GetTemporaryStorageMessageBuilder(TemporaryStorageMessageSendingObject messageSendingObject, TemporaryStorageMessageFunction messageFunction)
		{
			throw new NotImplementedException();
		}
	}
}
