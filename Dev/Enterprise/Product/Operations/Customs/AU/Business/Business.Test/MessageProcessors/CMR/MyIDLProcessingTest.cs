using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class MyIDLProcessingTest : TestCaseWithFactory
	{
		public void TestIDLDoesntResetStatus()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C00001208";
			CommonShipment shipment1 = consol.Shipments.AddNew();
			shipment1.JS_UniqueConsignRef = "S00001235";
			CommonShipment shipment2 = consol.Shipments.AddNew();
			shipment2.JS_UniqueConsignRef = "S00001234";

			FreightConsolWrapper wrapper = new FreightConsolWrapper(consol);
			CMRESMMessage outgoingMessage = (CMRESMMessage)consol.Messages.AddNew(typeof(CMRESMMessage));
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_SystemCreateTimeUtc = ZDateTime.Now;

			CMRESMRMessage incomingMessage = (CMRESMRMessage)consol.Messages.AddNew(typeof(CMRESMRMessage));
			incomingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage.EM_MessageText = embeddedResourceRetriever.GetString(GetEmbeddedResourcePath("ESMRClearMessage.txt")).Replace("\r\n", "");
			incomingMessage.EM_SystemCreateTimeUtc = ZDateTime.Now.AddDays(1);
			LoggingInformation logger = new LoggingInformation();
			ESMRMessageProcessor esmrProcessor = new ESMRMessageProcessor(logger);
			esmrProcessor.ProcessMessage(incomingMessage);

			AssertEquals("Status", Constants.CMRConsolStatus.Clear, wrapper.GetCMRStatus(CMRMessage.CMRMessageTypes.ESM));

			AssertEquals("Should have a CLO log because we got a clear status", "CLO", consol.Logs.GetAllLogs()[consol.Logs.GetAllLogs().Count - 1].SL_Reference);

			AUCusEntryNumber permit = wrapper.GetPermit();
			AssertEquals("PermitNumber", "AAAACNPMT", permit.CE_EntryNum);

			CMRIDLMessage incomingMessage2 = (CMRIDLMessage)consol.Messages.AddNew(typeof(CMRIDLMessage));
			incomingMessage2.EM_SystemCreateTimeUtc = ZDateTime.Now.AddDays(2);
			incomingMessage2.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incomingMessage2.EM_MessageText = embeddedResourceRetriever.GetString(GetEmbeddedResourcePath("IDLMessage.txt")).Replace("\r\n", "").Replace("B00122382/1", "C00001208/1").Replace("AAAAEFFME", "AAAACNPMT");
			IDLMessageProcessor processor = new IDLMessageProcessor(logger);
			processor.ProcessMessage(incomingMessage2);

			permit = wrapper.GetPermit();
			AssertEquals("PermitNumber", "AAAACNPMT", permit.CE_EntryNum);

			AssertEquals("AAAACNPMT", new ESMManifestStatus(consol).E2_CustomsEntryNumber);

			AssertEquals(3, consol.Messages.Count);

			outgoingMessage.EM_MessageText += CMRMessage.SendersReferencePlaceHolder + CMRMessage.MessageNumberPlaceHolder;
			Factory.Save();
			Factory.RefreshEnabled = false;

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			factory2.RefreshEnabled = false;
			ForwardingConsol consol2 = Factory.Load<ForwardingConsol>(consol.PK);
			AssertEquals("AAAACNPMT", new ESMManifestStatus(consol2).E2_CustomsEntryNumber);
			AssertEquals("Idle", new ESMManifestStatus(consol2).E2_MessageStatus);
		}

		string GetEmbeddedResourcePath(string fileName) => "Enterprise.Customs.AU.Declaration.Business.Testing.MessageProcessors.CMR.TestFiles." + fileName;

		protected override void SetUp()
		{
			base.SetUp();
			embeddedResourceRetriever = new EmbeddedResourceRetriever();
		}

		EmbeddedResourceRetriever embeddedResourceRetriever;
	}
}
