using CargoWise.Customs.FR.MessageDefinitions.TP5.CC928C;
using CargoWise.Types;

namespace Enterprise.Customs.FR.Business.EdiMessages
{
	public class CC928CMessagePrettier : NCTSMessagePrettier<Cc928CType>
	{
		public CC928CMessagePrettier(CC928CMessageDataObject messageDataObject)
			: base(messageDataObject)
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Core strings")]
		protected override ZString GetMessageInterpretationCore(Cc928CType messageObject)
		{
			var transitOperation = messageObject.TransitOperation;
			return ToKeyValuePairSection(new (ZString key, ZString value)[]
			{
				("Status", "Positive Acknowledge"),
				("LRN", transitOperation?.Lrn ?? ZString.Empty)
			});
		}
	}
}
