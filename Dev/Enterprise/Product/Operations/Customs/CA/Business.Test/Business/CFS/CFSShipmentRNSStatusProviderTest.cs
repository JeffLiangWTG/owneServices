using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.CFS.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	public class CFSShipmentRNSStatusProviderTest : TestCaseWithFactory
	{
		public void TestRNSStatus()
		{
			var shipment = Factory.New<CFSShipment>();
			var provider = new CFSShipmentRNSStatusProvider(shipment);

			AssertEquals("ReleaseStatusCode", "", provider.ReleaseStatusCode);
			AssertEquals("ReleaseStatus", "Not Sent", provider.ReleaseStatus);
			AssertEquals("ReleaseDate", ZDateTime.Empty, provider.ReleaseDate);

			AddAWOMessages(shipment);

			AssertEquals("ReleaseStatusCode", "AWO", provider.ReleaseStatusCode);
			AssertEquals("ReleaseStatus", "Awaiting Warehouse Arrival Certification Message Original", provider.ReleaseStatus);
			AssertEquals("ReleaseDate", ZDateTime.Empty, provider.ReleaseDate);

			AddCLRMessages(shipment);

			AssertEquals("TransactionNumber", "12345000067897", provider.TransactionNumber);
			AssertEquals("ReleaseStatusCode", "CLR", provider.ReleaseStatusCode);
			AssertEquals("ReleaseStatus", "4 - Goods Released", provider.ReleaseStatus);
			AssertEquals("ReleaseDate", new ZDateTime(2010, 06, 22, 10, 28, 00), provider.ReleaseDate);
		}

		[TestDate(2014, 12, 5)]
		public void TestArrivalCertificationStatus()
		{
			var shipment = Factory.New<CFSShipment>();
			var provider = new CFSShipmentRNSStatusProvider(shipment);

			AssertEquals("ArrivalCertificationStatusCode", "", provider.ArrivalCertificationStatusCode);
			AssertEquals("ArrivalCertificationStatus", "Not Sent", provider.ArrivalCertificationStatus);
			AssertEquals("ArrivalCertificationDate", ZDateTime.Empty, provider.ArrivalCertificationDate);

			AddREJMessages(shipment);

			AssertEquals("ArrivalCertificationStatusCode", "REJ", provider.ArrivalCertificationStatusCode);
			AssertEquals("ArrivalCertificationStatus", "Rejected", provider.ArrivalCertificationStatus);
			AssertEquals("ArrivalCertificationDate", ZDateTime.Empty, provider.ArrivalCertificationDate);

			AddSNTMessages(shipment);

			AssertEquals("ArrivalCertificationStatusCode", "SNT", provider.ArrivalCertificationStatusCode);
			AssertEquals("ArrivalCertificationStatus", "Sent", provider.ArrivalCertificationStatus);
			AssertEquals("ArrivalCertificationDate", new ZDateTime(2014, 12, 5), provider.ArrivalCertificationDate);
		}

		public static void AddAWOMessages(CFSShipment shipment)
		{
			var request = shipment.Messages.AddNew(typeof(RNSRequestMessage));
			request.EM_MessageSubType = RNSMessageTypes.Codes.ArrivalCertification;
			request.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			request.EM_Status = EDIMessage.Status.Sent;
			request.EM_SystemCreateTimeUtc = ZDateTime.Now;

			var mf1 = shipment.Messages.AddNew(typeof(ACIHouseBillMessage));
			mf1.EM_MessageSubType = MessageTypeList.Codes.ManifestForwardHouse;
			mf1.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			mf1.EM_Status = EDIMessage.Status.Received;
			mf1.EM_SystemCreateTimeUtc = request.EM_SystemCreateTimeUtc.AddMinutes(5);
		}

		public static void AddCLRMessages(CFSShipment shipment)
		{
			var response = shipment.Messages.AddNew(typeof(EDIReleaseMessage));
			response.EM_MessageSubType = EDIReleaseImportEntryStatusList.Codes.GoodsReleased;
			response.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			response.EM_Status = EDIMessage.Status.Received;
			response.EM_MessageText = @"UNH+1+CUSRES:D:96A:UN'BGM+:::125+12345000067897+11'DTM+58:201006221028:203'GIS+4'RFF+CN:CCN123456'UNT+6+1'";
			response.EM_SystemCreateTimeUtc = ZDateTime.Now.AddMinutes(10);
			response.EM_MessageInterpretation = "HTML INTERPRETATION";
			response.SetSystemDefinedValue(EDIReleaseMessage.Schema.TransactionNumber, new ZString("12345000067897"));

			var reject = shipment.Messages.AddNew(typeof(EDIReleaseMessage));
			reject.EM_MessageSubType = EDIReleaseImportEntryStatusList.Codes.MessageContentRejected;
			reject.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			reject.EM_Status = EDIMessage.Status.Received;
			reject.EM_MessageText = @"UNH+1+CUSRES:D:96A:UN'BGM+:::489+12345000067897+11'DTM+58:201006221028:203'GIS+2'RFF+CN:CCN123456'UNT+6+1'";
			reject.EM_SystemCreateTimeUtc = response.EM_SystemCreateTimeUtc.AddMinutes(5);
			reject.SetSystemDefinedValue(EDIReleaseMessage.Schema.TransactionNumber, new ZString("12345000067897"));

			var error = shipment.Messages.AddNew(typeof(EDIReleaseMessage));
			error.EM_MessageSubType = EDIReleaseImportEntryStatusList.Codes.Error;
			error.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			error.EM_Status = EDIMessage.Status.Received;
			error.EM_MessageText = @"UNH+1+CUSRES:D:96A:UN'BGM+:::489+12345000067897+11'DTM+58:201006221028:203'GIS+14'RFF+CN:CCN123456'UNT+6+1'";
			error.EM_SystemCreateTimeUtc = response.EM_SystemCreateTimeUtc.AddMinutes(10);
			error.SetSystemDefinedValue(EDIReleaseMessage.Schema.TransactionNumber, new ZString("12345000067897"));

			var syntax = shipment.Messages.AddNew(typeof(EDIReleaseMessage));
			syntax.EM_MessageSubType = EDIReleaseImportEntryStatusList.Codes.SyntaxError;
			syntax.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			syntax.EM_Status = EDIMessage.Status.Received;
			syntax.EM_MessageText = @"UNH+1+CUSRES:D:96A:UN'BGM+:::489+12345000067897+11'DTM+58:201006221028:203'GIS+2'RFF+CN:CCN123456'UNT+6+1'";
			syntax.EM_SystemCreateTimeUtc = response.EM_SystemCreateTimeUtc.AddMinutes(15);
			syntax.SetSystemDefinedValue(EDIReleaseMessage.Schema.TransactionNumber, new ZString("12345000067897"));

			var mf2 = shipment.Messages.AddNew(typeof(ACIHouseBillMessage));
			mf2.EM_MessageSubType = MessageTypeList.Codes.ManifestForwardHouse;
			mf2.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			mf2.EM_Status = EDIMessage.Status.Received;
			mf2.EM_SystemCreateTimeUtc = ZDateTime.Now.AddMinutes(10);
		}

		public static void AddREJMessages(CFSShipment shipment)
		{
			var message = shipment.Messages.AddNew(typeof(RNSRequestMessage));
			message.EM_MessageSubType = RNSMessageTypes.Codes.ArrivalCertification;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.EM_Status = EDIMessage.Status.Rejected;
			message.EM_SystemCreateTimeUtc = ZDateTime.UtcNow.AddMinutes(-5);
		}

		public static void AddSNTMessages(CFSShipment shipment)
		{
			var message = shipment.Messages.AddNew(typeof(RNSRequestMessage));
			message.EM_MessageSubType = RNSMessageTypes.Codes.ArrivalCertification;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.EM_Status = EDIMessage.Status.Sent;
			message.EM_SystemCreateTimeUtc = ZDateTime.UtcNow;
		}

		public void TestMessageInterpretation()
		{
			var shipment = Factory.New<CFSShipment>();
			var provider = new CFSShipmentRNSStatusProvider(shipment);

			AssertEquals("MessageInterpretation", "", provider.MessageInterpretation);

			AddAWOMessages(shipment);
			AssertEquals("MessageInterpretation", "", provider.MessageInterpretation);

			AddCLRMessages(shipment);
			AssertEquals("MessageInterpretation", "HTML INTERPRETATION", provider.MessageInterpretation);
		}
	}
}
