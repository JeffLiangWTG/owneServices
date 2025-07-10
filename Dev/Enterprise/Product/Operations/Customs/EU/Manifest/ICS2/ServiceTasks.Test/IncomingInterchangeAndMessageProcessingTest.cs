using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Manifest.ICS2.Business;
using Enterprise.Customs.EU.Manifest.ICS2.Business.Test;
using Enterprise.Customs.ServiceTasks.UniversalCustomsMessaging;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Customs.EU.Manifest.ICS2.ServiceTasks.Test
{
	sealed class IncomingInterchangeAndMessageProcessingTest : TestCaseWithFactory
	{
		public void TestEndToEnd_NormalAcknowledgementUnpacker_ShouldUseNTextOrEvenBetterUseMessageDataAsItsCompressed_True() => AssertInterchangeAndMessageSucessfullyProcessed();

		public void TestEndToEnd_NormalAcknowledgementUnpacker_ShouldUseNTextOrEvenBetterUseMessageDataAsItsCompressed_False()
		{
			using (SystemDataRegistry.Instance.EMMessageDataActive.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AssertInterchangeAndMessageSucessfullyProcessed();
			}
		}

		void AssertInterchangeAndMessageSucessfullyProcessed()
		{
			var outgoingInterchange = Factory.NewWithValidTestData<EDIInterchange>();
			outgoingInterchange.EI_ApplicationCode = EDIInterchange.ApplicationCodes.IC2;
			outgoingInterchange.EI_InterchangeType = "F24";
			outgoingInterchange.EI_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
			outgoingInterchange.EI_TransportType = EDIInterchangeTransportTypeList.Codes.xT;
			outgoingInterchange.EI_Status = EDIInterchangeStatusList.Codes.Sent;
			outgoingInterchange.EI_IsActive = true;
			outgoingInterchange.EI_GB = GlbBranch.CurrentBranch.PK;
			outgoingInterchange.EI_SessionGUID = ZGuid.BrettsGuid;

			var outgoingMessage = Factory.NewWithValidTestData<ICS2OutboundEDIMessage>();
			outgoingMessage.EM_Status = EDIMessageStatusList.Codes.ProcessedOK;
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_EI = outgoingInterchange.PK;
			outgoingMessage.EM_LinkedObject = Factory.NewWithValidTestData<AsycudaManifestHeader>();

			var incomingInterchange = Factory.New<EDIInterchange>();
			incomingInterchange.EI_ApplicationCode = EDIInterchange.ApplicationCodes.IC2;
			incomingInterchange.EI_InterchangeType = "F24";
			incomingInterchange.EI_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;
			incomingInterchange.EI_TransportType = EDIInterchangeTransportTypeList.Codes.xT;
			incomingInterchange.EI_Status = EDIInterchangeStatusList.Codes.Queued;
			incomingInterchange.EI_IsActive = true;
			incomingInterchange.EI_GB = GlbBranch.CurrentBranch.PK;
			incomingInterchange.EI_InterchangeNum = "ICS22023001";
			incomingInterchange.EI_SessionGUID = ZGuid.BrettsGuid;
			var newBodyContent = ics2EmptyQueryResponseDataAsString.Replace("4ce12480-cb84-4a86-8ac8-cab55c67a8d1", outgoingInterchange.PK.ToString());
			incomingInterchange.EI_BodyText = newBodyContent;
			Factory.Save();

			var ucuServiceTask = new UCUServiceTask();
			ucuServiceTask.ServiceLogger = new TestServiceLogger();
			ucuServiceTask.RunTask();
			var interchangeInDifferentFactory = NewFactory().Load<EDIInterchange>(incomingInterchange.PK);
			AssertEquals(EDIInterchangeStatusList.Codes.Received, interchangeInDifferentFactory.EI_Status);

			var createdEDIMessagePK = interchangeInDifferentFactory.ContainedMessages.Cast<EDIMessage>().Single().PK;
			var eupServiceTask = new MessageProcessorServiceTask();
			eupServiceTask.ServiceLogger = new TestServiceLogger();
			eupServiceTask.RunTask();
			var createdEDIMessageInADifferentFactory = NewFactory().Load<EDIMessage>(createdEDIMessagePK);
			AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, createdEDIMessageInADifferentFactory.EM_Status);
		}

		string ics2EmptyQueryResponseDataAsString => ics2EmptyQueryResponseDataAsStringCached ?? (ics2EmptyQueryResponseDataAsStringCached = EUICS2MessageTestHelper.ICS2EmptyQueryResponseDataAsString);
		string ics2EmptyQueryResponseDataAsStringCached;
	}
}
