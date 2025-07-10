using CargoWise.Types;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.Business.Declaration;
using static System.FormattableString;

namespace Enterprise.Customs.GB.CDS
{
	public class CDSInventoryLinkingMovementRequestEDIMessagePrettier : CDSEDIMessagePrettier<CDSInventoryLinkingMovementRequestEDIMessage>
	{
		public CDSInventoryLinkingMovementRequestEDIMessagePrettier(CDSInventoryLinkingMovementRequestEDIMessage requestMessage) : base(requestMessage)
		{
			request = Message.MessageDataObject;
		}

		public override ZString MakeHumanReadable()
		{
			var interpretation = MessagePrettierCss.CSS
				 + ToH3IfNotEmpty(Invariant($"Movement request message of type (EAL, EAA, EDL)"))
				 + ToKeyValuePairSection(new (ZString key, ZString value)[]
				 {
					 ("Message Code", request.messageCode.ToString()),
					 ("MUCR", request.masterUCR),
					 ("DUCR", UCRHelper.UcrBlockToString(request.ucrBlock)),
					 ("Goods Arrival Date Time", request.goodsArrivalDateTime.ToString()),
					 ("Goods Departure Date Time", request.goodsDepartureDateTime.ToString()),
					 ("Goods Location", request.goodsLocation),
					 ("Movement Reference Number", request.movementReference),
					 ("Shed Operator", request.shedOPID),
					 ("Transport ID", request.transportDetails.transportID),
					 ("Transport Mode", request.transportDetails.transportMode),
					 ("Transport Nationality", request.transportDetails.transportNationality)
				 });
			return interpretation;
		}

		readonly CargoWise.Customs.GB.MessageDefinitions.CDS.CDSInventoryLinking.inventoryLinkingMovementRequest request;
	}
}
