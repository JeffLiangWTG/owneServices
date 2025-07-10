using CargoWise.Customs.FR.MessageDefinitions.DeltaIE.Response.IE431;
using CargoWise.Types;

namespace Enterprise.Customs.FR.Business.EdiMessages
{
	public class IE431MessagePrettier : DeltaIEMessagePrettier<CC431BType>
	{
		public IE431MessagePrettier(IE431MessageDataObject messageDataObject, DeltaIEFREDIMessage message)
			: base(messageDataObject)
		{
			this.message = message;
		}

		readonly DeltaIEFREDIMessage message;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Core strings")]
		protected override ZString GetMessageInterpretationCore(CC431BType messageObject)
		{
			var timerExpiryForSupplementaryDeclaration = messageObject.TimerExpiryForSupplementaryDeclaration;

			var result = ToKeyValuePairSection(new (ZString key, ZString value)[]
			{
				("Status", "Timer Expired"),
				("Status Date", message.EM_MessageDateTime.ToISO8601ShortDateString()),
				("LRN", messageObject.ImportOperation?.LRN ?? ZString.Empty),
			});

			if (timerExpiryForSupplementaryDeclaration != null)
			{
				result += ToStrongIfNotEmpty(ToLargerPIfNotEmpty("Timer Expiry for Supplementary Declaration"));
				result += ToKeyValuePairSection(new (ZString key, ZString value)[]
				{
					("Declaration Start Date", timerExpiryForSupplementaryDeclaration?.LodgementOfSupplementaryDeclarationStartDate ?? ZString.Empty),
					("Declaration Expiry Date", timerExpiryForSupplementaryDeclaration?.LodgementOfSupplementaryDeclarationExpiryDate ?? ZString.Empty),
					("Timer Expiry information", timerExpiryForSupplementaryDeclaration?.TimerExpiryInformation ?? ZString.Empty)
				});
			}

			return result;
		}
	}
}
