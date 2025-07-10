using System.Globalization;
using CargoWise.EntityFramework.Testing;
using Enterprise.BatchProcessor;
using Enterprise.Customs.FR.Business.CusTempStorage;
using Enterprise.Customs.FR.Business.EdiMessages;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.FR.Business.MessageProcessors.Testing
{
	public class FRCINResponseMessageProcessorTest : TestCaseWithFactory
	{
		public void TestEDIMessagesProcessed_Valid()
		{
			var cust = Factory.NewWithValidTestData<OrgHeader>();
			cust.OH_Code = "UNITTEST";

			var jobHeader = CusTempStorageJobHeader.New(Factory, FRConstants.TemporaryStorage.AppCodeFRC);
			jobHeader.SJH_GB = GlbBranch.CurrentBranch.PK;
			jobHeader.SJH_JobReference = "UNITTEST";
			jobHeader.SJH_OH_Customer = cust.PK;

			var dec = jobHeader.CusTempStorageDec;
			var line = dec.CusTempStorageLines.AddNew();
			line.TSL_ReferenceNumber = "CIN-REF001";

			var message = Factory.New<CINImportResponseFREDIMessage>();
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_Status = EDIMessage.Status.Queued;
			message.EM_ApplicationCode = "FRC";
			message.EM_MessageType = MessageTypeList.Codes.CIN;
			message.EM_MessageSubType = MessageSubTypeList.Codes.CIN;
			message.EM_MessageNum = "1";

			dec.Messages.Add(message);

			message.EM_MessageText = string.Format(CultureInfo.InvariantCulture, @"<CinMessage type=""WarehouseMovement-In"">
  <Header from=""CIN"" to=""PUT-CIN-ID-HERE"" messageTime=""2011-12-13T14:15:16.017Z"" messageId=""{0}"" />
  <WarehouseMovementInResponse>Expecting some kind of response in this format but that has not been defined as yet</WarehouseMovementInResponse>
</CinMessage>", message.EM_MessageNum);

			Factory.Save();

			var processor = new FRCINImportResponseMessageProcessor(new LoggingInformation());

			processor.ProcessMessage(message);
			Factory.Save();

			AssertEquals(dec.PK, message.EM_LinkedObject.PK);

			dec.Reload();

			AssertEquals(CusTempStorageDec.DeclarationStatusForCorrectionMessage, dec.STH_MessageStatus);
		}
	}
}
