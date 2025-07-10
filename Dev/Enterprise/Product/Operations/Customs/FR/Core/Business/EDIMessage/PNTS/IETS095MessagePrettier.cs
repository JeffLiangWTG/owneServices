using CargoWise.Customs.FR.MessageDefinitions.PNTS.Response.IETS095;
using CargoWise.Types;

namespace Enterprise.Customs.FR.Business.EdiMessages
{
	public class IETS095MessagePrettier : PNTSMessagePrettier<Iets095>
	{
		public IETS095MessagePrettier(IETS095MessageDataObject messageDataObject) : base(messageDataObject)
		{
		}

		public new IETS095MessageDataObject MessageDataObject => (IETS095MessageDataObject)base.MessageDataObject;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Core strings")]
		protected override ZString GetMessageInterpretationCore(Iets095 messageObject)
		{
			return ToKeyValuePairSection(new (ZString key, ZString value)[]
			{
				("Status", MessageDataObject.GetCustomsStatusDescriptionFromMessage()),
				("Status Reason", messageObject.StatusReason != null ? MessageDataObject.GetStatusReasonDescriptionFromMessage() : ZString.Empty),
				("MRN", messageObject.Mrn),
				("CRN", messageObject.Crn),
				("FRN", messageObject.Frn),
				("Notification Date", messageObject.NotificationDate.ToString()),
				("Timer For Temporary Storage", messageObject.TimerForTemporaryStorage.ToString()),
				("Remarks", messageObject.Remarks),
			});
		}
	}
}
