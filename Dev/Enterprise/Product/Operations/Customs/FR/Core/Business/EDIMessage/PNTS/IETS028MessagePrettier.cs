using CargoWise.Customs.FR.MessageDefinitions.PNTS.Response.IETS028;
using CargoWise.Types;

namespace Enterprise.Customs.FR.Business.EdiMessages
{
	public class IETS028MessagePrettier : PNTSMessagePrettier<Iets028>
	{
		public IETS028MessagePrettier(IETS028MessageDataObject messageDataObject) : base(messageDataObject)
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Core strings")]
		protected override ZString GetMessageInterpretationCore(Iets028 messageObject)
		{
			return ToKeyValuePairSection(new (ZString key, ZString value)[]
			{
				("Status", MessageDataObject.GetCustomsStatusDescriptionFromMessage()),
				("MRN", messageObject.Mrn),
				("CRN", messageObject.Crn),
				("FRN", messageObject.Frn),
				("Notification Date", messageObject.NotificationDate.ToString()),
				("Expiration Date", messageObject.ExpirationDate.ToString()),
				("Remarks", messageObject.Remarks),
			});
		}
	}
}
