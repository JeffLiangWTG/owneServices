using System.Collections.Generic;
using System.Text.Json;
using CargoWise.Customs.IE.MessageDefinitions;
using CargoWise.Customs.IE.MessageDefinitions.Common.MessageAcknowledgement;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.IE.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.IE.Messaging.Testing
{
	public static class CustomsAndExciseReportInterchangeProcessorTestHelper
	{
		public static EDIInterchange CreateIncomingPSRInterchange(BusinessObjectFactory factory, ZGuid sessionGuid)
		{
			return InterchangeProcessorTestHelper.CreateIncomingInterchange(factory, EDIInterchange.ApplicationCodes.IECustomsAndExcise, CustomsAndExciseReportTypeList.Codes.PSR, CreatePSRText(), sessionGuid, wrapInSOAPEnvelope: false);
		}

		public static PSRMessage CreatePSRMessage()
		{
			return new PSRMessage
			{
				Timestamp = "1660211772103",
				Eori = "IE1234567A",
				Period = "20220801",
				TaxTotal = 400.0M,
				TaxBreakdowns = new List<TaxBreakdown>
				{
					new() { TaxType43 = "A00", PayableAmount46 = 150.0M },
					new() { TaxType43 = "B00", PayableAmount46 = 250.0M }
				},
				DailyBreakdowns = new List<DailyBreakdown>
				{
					new() { Date = "20220810", TaxTotal = 3200.0M },
					new() { Date = "20220811", TaxTotal = 200.0M }
				}
			};
		}
		public static string CreatePSRText()
		{
			var psrMessage = CreatePSRMessage();
			return JsonSerializer.Serialize(psrMessage);
		}

		public static PCTMessage CreatePCTMessage()
		{
			return new PCTMessage
			{
				Timestamp = "1660211772103",
				Eori = "IE1234567A",
				Period = "20220801",
				PaidOrders = new List<PctPaidOrder>
				{
					new() { ImporterName = "MR Test ONeill", Mrn = "22IEDUB4BBFC22PER2" , Version = 1, Amendment = false, DeclarationMsgType = "H1", Payer = "IE1234567A", Importer = "IE1234567A", Declarant = "IE7654321A", DeclarantName = "MR Test Murphy", DtReceived =  "2022-08-11T10:56:11.903+0100", TaxTotal = 200.0M, TotalDuty = 150.0M, VatOnDuty = 50M, TotalExcise = 0, VatOnExcise = 0, PostponedVat = 100M, Lrn = "EXA214094_06AaxY", Ucr = "123422342", CommercialTransportDoc = "N703124242" },
					new() { ImporterName = "MR Test ONeill", Mrn = "22IEDUB4BBFC22PER2" , Version = 2, Amendment = true, DeclarationMsgType = "H1", Payer = "IE1234567A", Importer = "IE1234567A", Declarant = "IE7654321A", DeclarantName = "MR Test Murphy", DtReceived =  "2022-08-13T17:46:11.903+0100", TaxTotal = 500.0M, TotalDuty = 50.0M, VatOnDuty = 50M, TotalExcise = 350M, VatOnExcise = 50M, PostponedVat = 100M, Lrn = "EXA214094_06AaxY", Ucr = "1234787878342", CommercialTransportDoc = "N703124242" }
				}
			};
		}

		public static string CreatePCTText()
		{
			return JsonSerializer.Serialize(CreatePCTMessage());
		}

		public static PTTMessage CreatePTTMessage()
		{
			return new PTTMessage
			{
				Timestamp = "1660211772103",
				Eori = "IE1234567A",
				Period = "20220801",
				TaxDetails = new List<TaxDetail>
				{
					new() { Mrn = "22IEDUB4BBFC22PER2", Version = 1, _1D3 = 0, _1A1 = 0, _1B2 = 0, _A00 = 5, _1B3 = 0, _1D5 = 0, _A45 = 0, _B00 = 5, _1D6 = 0, _A35 = 0, _B00EX = 0, _1S1 = 0, _1E1 = 0, _A40 = 0, _A30 = 0, _1C1 = 0, _2E2 = 0, _A20 = 0 },
					new() { Mrn = "22IEDUB4BBFC22AZR2", Version = 3, _1D3 = 0, _1A1 = 0, _1B2 = 0, _A00 = 5000, _1B3 = 200, _1D5 = 0, _A45 = 0, _B00 = 50, _1D6 = 0, _A35 = 0, _B00EX = 0, _1S1 = 0, _1E1 = 0, _A40 = 0, _A30 = 0, _1C1 = 0, _2E2 = 0, _A20 = 0 }
				}
			};
		}

		public static string CreatePTTText()
		{
			return JsonSerializer.Serialize(CreatePTTMessage());
		}

		public static PCIMessage CreatePCIMessage()
		{
			return new PCIMessage
			{
				Timestamp = "1660211772103",
				Eori = "IE1234567A",
				Period = "20220801",
				PaidOrders = new List<PciPaidOrder>
				{
					new() { PayerName = "MR Test ONeill", Mrn = "22IEDUB4BBFC22PER2" , Version = 1, Amendment = false, DeclarationMsgType = "H1", Payer = "IE1234567A", Importer = "IE1234567A", Declarant = "IE7654321A", DeclarantName = "MR Test Murphy", DtReceived =  "2022-08-11T10:56:11.903+0100", TaxTotal = 200.0M, TotalDuty = 150.0M, VatOnDuty = 50M, TotalExcise = 0, VatOnExcise = 0, PostponedVat = 100M, Lrn = "EXA214094_06AaxY", Ucr = "123422342", CommercialTransportDoc = "N703124242" },
					new() { PayerName = "MR Test ONeill", Mrn = "22IEDUB4BBFC22PER2" , Version = 2, Amendment = true, DeclarationMsgType = "H1", Payer = "IE1234567A", Importer = "IE1234567A", Declarant = "IE7654321A", DeclarantName = "MR Test Murphy", DtReceived =  "2022-08-13T17:46:11.903+0100", TaxTotal = 500.0M, TotalDuty = 50.0M, VatOnDuty = 50M, TotalExcise = 350M, VatOnExcise = 50M, PostponedVat = 100M, Lrn = "EXA214094_06AaxY", Ucr = "1234787878342", CommercialTransportDoc = "N703124242" }
				}
			};
		}

		public static string CreatePCIText()
		{
			return JsonSerializer.Serialize(CreatePCIMessage());
		}

		public static DSRMessage CreateDSRMessage()
		{
			return new DSRMessage
			{
				Timestamp = "1660211772103",
				Eori = "IE1234567A",
				Date = "20220801",
				TaxTotal = 400.0M,
				TaxBreakdowns = new List<TaxBreakdown>
				{
					new() { TaxType43 = "A00", PayableAmount46 = 150.0M },
					new() { TaxType43 = "B00", PayableAmount46 = 250.0M }
				}
			};
		}
		public static string CreateDSRText()
		{
			return JsonSerializer.Serialize(CreateDSRMessage());
		}

		public static DCTMessage CreateDCTMessage()
		{
			return new DCTMessage
			{
				Timestamp = "1660211772103",
				Eori = "IE1234567A",
				Day = "20220801",
				PaidOrders = new List<DctPaidOrder>
				{
					new() { ImporterName = "MR Test ONeill", Mrn = "22IEDUB4BBFC22PER2" , Version = 1, Amendment = false, DeclarationMsgType = "H1", Payer = "IE1234567A", Importer = "IE1234567A", Declarant = "IE7654321A", DeclarantName = "MR Test Murphy", DtReceived =  "2022-08-11T10:56:11.903+0100", TaxTotal = 200.0M, TotalDuty = 150.0M, VatOnDuty = 50M, TotalExcise = 0, VatOnExcise = 0, PostponedVat = 100M, Lrn = "EXA214094_06AaxY", Ucr = "123422342", CommercialTransportDoc = "N703124242", Period = "20220101" },
					new() { ImporterName = "MR Test ONeill", Mrn = "22IEDUB4BBFC22PER2" , Version = 2, Amendment = true, DeclarationMsgType = "H1", Payer = "IE1234567A", Importer = "IE1234567A", Declarant = "IE7654321A", DeclarantName = "MR Test Murphy", DtReceived =  "2022-08-13T17:46:11.903+0100", TaxTotal = 500.0M, TotalDuty = 50.0M, VatOnDuty = 50M, TotalExcise = 350M, VatOnExcise = 50M, PostponedVat = 100M, Lrn = "EXA214094_06AaxY", Ucr = "1234787878342", CommercialTransportDoc = "N703124242", Period = "20220101" }
				}
			};
		}

		public static string CreateDCTText()
		{
			return JsonSerializer.Serialize(CreateDCTMessage());
		}

		public static DTTMessage CreateDTTMessage()
		{
			return new DTTMessage
			{
				Timestamp = "1660211772103",
				Eori = "IE1234567A",
				Day = "20220801",
				TaxDetails = new List<TaxDetail>
				{
					new() { Mrn = "22IEDUB4BBFC22PER2", Version = 1, _1D3 = 0, _1A1 = 0, _1B2 = 0, _A00 = 5, _1B3 = 0, _1D5 = 0, _A45 = 0, _B00 = 5, _1D6 = 0, _A35 = 0, _B00EX = 0, _1S1 = 0, _1E1 = 0, _A40 = 0, _A30 = 0, _1C1 = 0, _2E2 = 0, _A20 = 0 },
					new() { Mrn = "22IEDUB4BBFC22AZR2", Version = 3, _1D3 = 0, _1A1 = 0, _1B2 = 0, _A00 = 5000, _1B3 = 200, _1D5 = 0, _A45 = 0, _B00 = 50, _1D6 = 0, _A35 = 0, _B00EX = 0, _1S1 = 0, _1E1 = 0, _A40 = 0, _A30 = 0, _1C1 = 0, _2E2 = 0, _A20 = 0 }
				}
			};
		}

		public static string CreateDTTText()
		{
			return JsonSerializer.Serialize(CreateDTTMessage());
		}

		public static UDRMessage CreateUDRMessage()
		{
			return new UDRMessage
			{
				Timestamp = "1660211772103",
				Eori = "IE1234567A",
				UnpaidOrders = new List<UnpaidOrder>
				{
					new() { Mrn = "22IEDUB4BBFC22BPP3" , Version = 1, TaxTotal = 1000.0M },
					new() { Mrn = "22IEDUB4BBFC22BPP1" , Version = 1, TaxTotal = 2000.0M },
					new() { Mrn = "22IEDUB4BBFC22BPP2" , Version = 1, TaxTotal = 2000.0M }
				}
			};
		}

		public static string CreateUDRText()
		{
			return JsonSerializer.Serialize(CreateUDRMessage());
		}

		public static BALMessage CreateBALMessage()
		{
			return new BALMessage
			{
				Timestamp = "1660211772103",
				Total = "11061096.00",
				Cash = "9361096.00",
				Deferred = "1700000.00",
			};
		}
		public static string CreateBALText()
		{
			return JsonSerializer.Serialize(CreateBALMessage());
		}

		public static MessageAcknowledgement CreateErrorMessage()
		{
			return new MessageAcknowledgement
			{
				ErrorReference = new ErrorReference
				{
					ErrorCode = "111008"
				}
			};
		}
		public static string CreateErrorText() => IEXmlObjectSerializer.Serialize(CreateErrorMessage());

		public static EDIInterchange CreateOutgoingPSRInterchange(BusinessObjectFactory factory, ZGuid sessionGUID, CustomsAndExciseReportOutboundMessage message)
		{
			var interchange = CreateOutgoingInterchange(factory, CustomsAndExciseReportTypeList.Codes.PSR, sessionGUID);
			interchange.ContainedMessages.Add(message);
			return interchange;
		}

		static EDIInterchange CreateOutgoingInterchange(BusinessObjectFactory factory, string interchangeType, ZGuid sessionGUID, ZGuid? branchPK = null)
		{
			var result = factory.New<EDIInterchange>();
			result.EI_ApplicationCode = EDIInterchange.ApplicationCodes.IECustomsAndExcise;
			result.EI_InterchangeType = interchangeType;
			result.EI_ReceiveTransmit = EDIInterchange.Direction.Transmit;
			result.EI_TransportType = EDIInterchangeTransportTypeList.Codes.xT;
			result.EI_SessionGUID = sessionGUID;
			result.EI_From = "IECustomsTest";
			result.EI_To = "IECustomsTest";
			result.EI_Status = EDIInterchange.Status.Queued;
			if (branchPK.HasValue)
			{
				result.EI_GB = branchPK.Value;
			}
			return result;
		}

		public static CustomsAndExciseReportOutboundMessage CreateOutgoingMessage(BusinessObjectFactory factory, string messageType)
		{
			var message = factory.New<CustomsAndExciseReportOutboundMessage>();
			message.EM_ApplicationCode = EDIInterchange.ApplicationCodes.IECustomsAndExcise;
			message.EM_MessageType = messageType;
			return message;
		}
	}
}
