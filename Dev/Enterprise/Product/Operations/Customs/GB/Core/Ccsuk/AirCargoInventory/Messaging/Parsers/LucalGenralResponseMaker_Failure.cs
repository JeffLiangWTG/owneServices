using System.Globalization;
using CargoWise.Types;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.Genral;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging
{
	class LucalGenralResponseMaker_Failure : LucalGenralResponseMaker
	{
		public LucalGenralResponseMaker_Failure(string exactFailureReason, EDIMessage inboundMessage, string enquiryString, ZString commonAccessReference)
			: base(inboundMessage, enquiryString, commonAccessReference)
		{
			this.exactFailureReason = exactFailureReason;
		}

		internal override void MakeAndSendAllResponses()
		{
			var payload = MakePayloadResponseForFailure(exactFailureReason);
			var generalEdiMessage = GenralEdiMessage.MakeNewOutboundFromPayload(payload, inboundEdiMessage.Interchange.EI_From, inboundEdiMessage.Factory, inboundEdiMessage.Interchange.EI_To, true, commonAccessReference);
		}

		ZString MakePayloadResponseForFailure(string exactErrorText)
		{
			return ZString.Format(@"DEP response for: {0}
From:{1} sent: {2}

{3}
PAGE 01/END",
				this.enquiryString,
				inboundEdiMessage.Interchange.EI_To.Right(6).PadRight(40),
				ZDateTime.Now.ToString("dd/MM/yyyy H:mm:ss", CultureInfo.InvariantCulture),
				exactErrorText
				);
		}

		readonly string exactFailureReason;
	}
}
