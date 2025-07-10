using CargoWise.Customs.FR.MessageDefinitions.PNTS.Response.IETS928;
using CargoWise.Types;

namespace Enterprise.Customs.FR.Business.EdiMessages
{
	public class IETS928MessagePrettier : PNTSMessagePrettier<Iets928>
	{
		public IETS928MessagePrettier(IETS928MessageDataObject messageDataObject) : base(messageDataObject)
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Core strings")]
		protected override ZString GetMessageInterpretationCore(Iets928 messageObject)
		{
			return ToKeyValuePairSection(new (ZString key, ZString value)[]
			{
				("Status", "Message was successfully received in customs"),
				("Correlation ID", messageObject.CorrelationId),
			});
		}
	}
}
