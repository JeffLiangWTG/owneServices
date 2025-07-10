using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business.Testing;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	abstract class CMRMessageResponseProcessorTest : DeclarationsAndShipmentsCreatedCancelledTestCase
	{
		public void TestMessageCode()
		{
			var processor = GetMessageProcessor();
			AssertEquals("MessageCode", GetExpectedMessageCode(), processor.GetMessageTypeDelegate(incomingMessage));
		}

		public void TestMessageName()
		{
			var processor = GetMessageProcessor();
			AssertEquals("MessageCode", GetExpectedMessageName(), processor.MessageFriendlyName);
		}

		protected abstract CMRMessageResponseProcessor GetMessageProcessor();
		protected abstract ZString GetExpectedMessageName();
		protected abstract ZString GetExpectedMessageCode();

		protected EDIMessage outgoingMessage;
		protected EDIMessage incomingMessage;
		protected LoggingInformation logger;
		protected EmbeddedResourceRetriever embeddedResourceRetriever;
		protected string GetEmbeddedResourcePath(string fileName) => "Enterprise.Customs.AU.Declaration.Business.Testing.MessageProcessors.CMR.TestFiles." + fileName;

		protected override void SetUp()
		{
			base.SetUp();
			incomingMessage = (EDIMessage)Factory.New(IncomingMessageType);
			incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.CMR;
			logger = new LoggingInformation();
			embeddedResourceRetriever = new EmbeddedResourceRetriever();
		}

		protected abstract Type IncomingMessageType
		{
			get;
		}

		protected JobSailing CreateSailing(ZString lloydsNumber, ZString voyageNumber)
		{
			var voyage = Factory.New<JobVoyage>();
			var vessel = Factory.LoadTop1<RefVessel>(new ZQuery(RefVesselSchema.RV_LloydsNumber, lloydsNumber));
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			voyage.JV_VoyageFlight = voyageNumber;
			var origin = voyage.Origins.AddNew();
			var destination = voyage.Destinations.AddNew();
			origin.JA_RL_NKPortOfLoading = "SGSIN";
			destination.JB_RL_NKPortOfDischarge = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			destination.JB_E_ARV = ZDateTime.Today;
			voyage.GenerateSailings();
			return voyage.Sailings[0];
		}

		protected sealed class OutgoingEDIMessageTestHelper : CMRMessage
		{
			public OutgoingEDIMessageTestHelper(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			protected override void GetNumberFountainNumbersAndFillInPlaceHolders()
			{
				//do nothing so save does not look for placeholders etc
			}

			protected override void SetDefaultValues()
			{
				base.SetDefaultValues();
				EM_Status = EDIMessage.Status.Sent;
				EM_MessageSubType = "ORG";
				EM_SystemCreateTimeUtc = ZDateTime.Now.AddHours(-1);
			}
		}
	}
}
