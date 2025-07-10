using CargoWise.Customs.FR.MessageDefinitions.PNTS.Response.IETS410;
using CargoWise.Types;

namespace Enterprise.Customs.FR.Business.EdiMessages
{
	public class IETS410MessagePrettier : PNTSMessagePrettier<Iets410>
	{
		public IETS410MessagePrettier(IETS410MessageDataObject messageDataObject) : base(messageDataObject)
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Core strings")]
		protected override ZString GetMessageInterpretationCore(Iets410 messageObject)
		{
			return ToKeyValuePairSection(new (ZString key, ZString value)[]
			{
				("Status", MessageDataObject.GetCustomsStatusDescriptionFromMessage()),
				("CRN", messageObject.Crn),
				("Notification Date", messageObject.NotificationDate.ToString()),
				("Invalidation Initiated By Customs", messageObject.InvalidationInitiatedByCustoms == 1 ? "True" : "False"),
			});
		}
	}
}
