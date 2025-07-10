using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using Enterprise.Customs.FR.Business.CusTempStorage;
using Enterprise.Customs.FR.Business.EdiMessages;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.FR.Business.MessageProcessors.Testing
{
	sealed class FRPortsIncomingMessageProcessorTest : TestCaseWithFactory
	{
		public void TestEDIMessageCreated()
		{
			var usCompany = Factory.New<GlbCompany>();
			usCompany.GC_Code = "USC";
			usCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			var usBranch = Factory.New<GlbBranch>();
			usBranch.GB_Code = "USB";
			usBranch.GB_GC = usCompany.PK;

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
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.EM_Status = EDIMessage.Status.Sent;
			message.EM_ApplicationCode = "FRC";
			message.EM_MessageType = MessageTypeList.Codes.CIN;
			message.EM_MessageText = "Not Important at this point";
			message.MessageNumberStrategy = new FRMessageNumberStrategy(jobHeader.Factory, FREDIMessage.ApplicationCodes.FRCustomsMessage);

			var messageText = resourceRetriever.Value.GetString("Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.DOA_Response.xml");
			var intchg = Factory.NewWithValidTestData<EDIInterchange>();
			intchg.EI_From = "FRCUS";
			intchg.EI_To = "TEST";
			intchg.EI_ApplicationCode = ApplicationCodeList.Codes.GenericMessageDelivery;
			intchg.EI_InterchangeType = GenericMessageDeliveryInterchangeTypeList.Codes.FRPorts;
			intchg.EI_InterchangeNum = "0000000000000000008";
			intchg.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			intchg.EI_Status = EDIInterchange.Status.Queued;
			intchg.EI_IsActive = true;
			intchg.EI_GB = GlbBranch.CurrentBranch.PK;
			intchg.EI_BodyText = messageText;

			Factory.Save();

			var processor = new FRPortsIncomingMessageProcessor(new BatchProcessor.LoggingInformation());
			processor.ProcessInterchangesAndExecuteBatch(new System.Threading.CancellationToken());

			intchg.Reload();

			AssertEquals(EDIInterchange.Status.Received, intchg.EI_Status);

			var msg = Factory.Load<EDIMessage>(new ZQuery(EDIMessageSchema.EM_EI, intchg.PK));

			AssertEquals(1, msg.Length);
		}

		readonly Lazy<EmbeddedResourceRetriever> resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever());
	}
}
