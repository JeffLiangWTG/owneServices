using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.BatchProcessor;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.ICS.CodeDescriptionPairLists;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using AsycudaManifestHeader = Enterprise.Customs.GB.ICS.Business.AsycudaManifestHeaderSS;

namespace Enterprise.Customs.GB.ICS.Testing
{
	public class IcsSsGreatBritainInboundInterchangeProcessorTest : TestCaseWithFactory
	{
		public void TestInboundInterchangeProcessorGB()
		{
			var aaaBranch = Factory.New<GlbBranch>();
			aaaBranch.GB_Code = "AAA";
			aaaBranch.GB_GC = GlbCompany.CurrentCompany.PK;
			Factory.Save();

			var manifestHeader = Factory.New<AsycudaManifestHeader>();
			manifestHeader.AMA_JobReference = "MAN12345";
			manifestHeader.AMA_ManifestType = ICSManifestTypes.Codes.SAS;
			manifestHeader.AMA_RN_NKCountry = Core.Constants.CountryCodes.UnitedKingdom;
			manifestHeader.AMA_GB = aaaBranch.PK;

			var outgoingMessage = Factory.New<IcsSsGreatBritainEDIMessage>();
			outgoingMessage.EM_ApplicationReference = "16763";
			outgoingMessage.EM_MessageText = "";
			outgoingMessage.MessageNumberStrategy = new GbMessageNumberStrategy(Factory, "2");
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_MessageNum = "999";
			outgoingMessage.EM_Status = EDIMessage.Status.Sent;
			outgoingMessage.EM_LinkedObject = manifestHeader;
			outgoingMessage.EM_GB = aaaBranch.PK;
			manifestHeader.Messages.Add(outgoingMessage);

			var outgoingInterchange = Factory.New<EDIInterchange>();
			outgoingInterchange.EI_InterchangeNum = "1";
			outgoingInterchange.EI_From = "WISETECHGLOBAL";
			outgoingInterchange.EI_To = "ICS";
			outgoingMessage.EM_EI = outgoingInterchange.PK;

			var incomingInterchange1 = Factory.New<EDIInterchange>();
			incomingInterchange1.EI_ApplicationCode = ApplicationCodeList.Codes.GbMessageICSGreatBritain;
			incomingInterchange1.EI_InterchangeNum = "00001";
			incomingInterchange1.EI_Status = EDIInterchange.Status.Queued;
			incomingInterchange1.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			incomingInterchange1.EI_BodyText = ICSGBCustomsBusinessResponseTest.NotificationResponseInterchangeBody;

			var incomingInterchange2 = Factory.New<EDIInterchange>();
			incomingInterchange2.EI_ApplicationCode = ApplicationCodeList.Codes.GbMessageICSGreatBritain;
			incomingInterchange2.EI_InterchangeNum = "00002";
			incomingInterchange2.EI_Status = EDIInterchange.Status.Queued;
			incomingInterchange2.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			incomingInterchange2.EI_BodyText = ICSGBCustomsBusinessResponseTest.OutcomeResponseInterchangeBody;

			Factory.Save();

			var logger = new LoggingInformation();
			var processor = new ICSInboundInterchangeProcessor(logger);
			processor.ExecuteBatch();

			var query = new ZQuery(EDIMessageSchema.EM_ApplicationCode, EDIInterchange.ApplicationCodes.GbMessageICSGreatBritain);
			query.AddToFilter(EDIMessageSchema.EM_EI, new[] { incomingInterchange1.PK, incomingInterchange2.PK });
			query.OrderBy = EDIMessage.Schema.EM_MessageNum;

			var messages = new BusinessObjectFactory().Load<EDIMessage>(query);
			AssertEquals(2, messages.Length);

			var message1 = messages.Single(m => m.EM_EI == incomingInterchange1.PK);
			AssertStartsWith("Message should be a CC351A XML", "<cc3:CC351A xmlns:cc3=\"http://ics.dgtaxud.ec/CC351A\">", message1.EM_MessageText);
			AssertEquals("Message.EM_ApplicationReference should be CorrelationID", "Am95Tr4n9aYAsi", message1.EM_ApplicationReference);
			AssertEquals("00001/Am95Tr4n9aYAsi", message1.EM_MessageNum);
			AssertEquals(EDIMessage.Direction.Receive, message1.EM_ReceiveTransmit);
			AssertEquals("351", message1.EM_MessageType);
			AssertEquals("351", message1.EM_MessageSubType);

			var message2 = messages.Single(m => m.EM_EI == incomingInterchange2.PK);
			AssertStartsWith("Message should be a CC328A XML", "<cc3:CC328A xmlns:cc3=\"http://ics.dgtaxud.ec/CC328A\">", message2.EM_MessageText);
			AssertEquals("Message.EM_ApplicationReference should be CorrelationID", "dwbFjvcb42T2Tt", message2.EM_ApplicationReference);
			AssertEquals("00002/dwbFjvcb42T2Tt", message2.EM_MessageNum);
			AssertEquals(EDIMessage.Direction.Receive, message2.EM_ReceiveTransmit);
			AssertEquals("328", message2.EM_MessageType);
			AssertEquals("328", message2.EM_MessageSubType);
		}
	}
}
