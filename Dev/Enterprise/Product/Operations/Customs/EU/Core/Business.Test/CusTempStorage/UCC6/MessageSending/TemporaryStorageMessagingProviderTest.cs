using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using static Enterprise.Customs.EU.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.EU.Business.CusTempStorage.Testing
{
	public abstract class TemporaryStorageMessagingProviderTest<T> : TestCaseWithFactory
		where T : TemporaryStorageMessagingProvider
	{
		public void TestDefaultingOfMessageType()
		{
			var header = Factory.New<TemporaryStorageHeader>();
			header.AMA_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			var sendingObject = new TemporaryStorageMessageSendingObject(header);
			AssertEquals("Sending Object MessageType should be left empty when main form Message Type is empty.", ZString.Empty, sendingObject.MessageType);

			header.AMA_MessageType = PNTSMessageTypeList.Codes.CombinedTemporaryStorage;
			sendingObject = new TemporaryStorageMessageSendingObject(header);
			AssertEquals("Sending Object MessageType should be 115 when main form Message Type is TC.", TemporaryStorageMessageTypeList.Codes.CombinedTSD, sendingObject.MessageType);

			header.AMA_MessageType = PNTSMessageTypeList.Codes.PreLodgedTempStorage;
			sendingObject = new TemporaryStorageMessageSendingObject(header);
			AssertEquals("Sending Object MessageType should be 015 when main form Message Type is TS.", TemporaryStorageMessageTypeList.Codes.PreLodgedTSD, sendingObject.MessageType);

			header.AMA_MessageType = PNTSMessageTypeList.Codes.PresentationNotification;
			sendingObject = new TemporaryStorageMessageSendingObject(header);
			AssertEquals("Sending Object MessageType should be 007 when main form Message Type is PN.", TemporaryStorageMessageTypeList.Codes.PresentationNotification, sendingObject.MessageType);

			header.AMA_MessageType = "BLA";
			sendingObject = new TemporaryStorageMessageSendingObject(header);
			AssertEquals("Sending Object MessageType should be left empty when main form Message Type is unknown.", ZString.Empty, sendingObject.MessageType);

			header.CustomsStatus = PNTS.CustomsStatus.TemporaryStoragePreLodged;
			header.AMA_MessageType = PNTSMessageTypeList.Codes.CombinedTemporaryStorage;
			sendingObject = new TemporaryStorageMessageSendingObject(header);
			AssertEquals("Sending Object MessageType should be left empty when the count of the list is not equal to 1.", ZString.Empty, sendingObject.MessageType);

			header.AMA_MessageType = PNTSMessageTypeList.Codes.PreLodgedTempStorage;
			sendingObject = new TemporaryStorageMessageSendingObject(header);
			AssertEquals("Sending Object MessageType should be left empty when the count of the list is not equal to 1.", ZString.Empty, sendingObject.MessageType);

			header.AMA_MessageType = PNTSMessageTypeList.Codes.PresentationNotification;
			sendingObject = new TemporaryStorageMessageSendingObject(header);
			AssertEquals("Sending Object MessageType should be left empty when the count of the list is not equal to 1.", "007", sendingObject.MessageType);

			header.CustomsStatus = PNTS.CustomsStatus.TemporaryStorageActivated;
			header.AMA_MessageType = PNTSMessageTypeList.Codes.CombinedTemporaryStorage;
			sendingObject = new TemporaryStorageMessageSendingObject(header);
			AssertEquals("Sending Object MessageType should be set to the value of only member in the list when the count of the list is equal to 1.", TemporaryStorageMessageTypeList.Codes.AmendmentRequestTSD, sendingObject.MessageType);

			header.AMA_MessageType = PNTSMessageTypeList.Codes.PreLodgedTempStorage;
			sendingObject = new TemporaryStorageMessageSendingObject(header);
			AssertEquals("Sending Object MessageType should be set to the value of only member in the list when the count of the list is equal to 1.", TemporaryStorageMessageTypeList.Codes.AmendmentRequestTSD, sendingObject.MessageType);

			header.AMA_MessageType = PNTSMessageTypeList.Codes.PresentationNotification;
			sendingObject = new TemporaryStorageMessageSendingObject(header);
			AssertEquals("Sending Object MessageType should be set to the value of only member in the list when the count of the list is equal to 1.", TemporaryStorageMessageTypeList.Codes.AmendmentRequestTSD, sendingObject.MessageType);
		}

		public void TestSendMessage()
		{
			var header = Factory.New<TemporaryStorageHeader>();
			header.AMA_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			var sendingObject = new TemporaryStorageMessageSendingObject(header);
			sendingObject.MessageType = TemporaryStorageMessageTypeList.Codes.PresentationNotification;
			var provider = GetProvider(header);
			provider.SendMessage(sendingObject, new SendsMessagesToCustomsShutterUpperer(), null);
			AssertEquals(1, header.Messages.Count);
		}

		protected abstract T GetProvider(TemporaryStorageHeader header);
	}
}
