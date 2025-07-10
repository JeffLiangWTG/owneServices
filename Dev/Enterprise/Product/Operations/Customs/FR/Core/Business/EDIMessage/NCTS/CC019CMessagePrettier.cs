using CargoWise.Customs.FR.MessageDefinitions.TP5.CC019C;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.FR.Business.EdiMessages
{
	public class CC019CMessagePrettier : NCTSMessagePrettier<Cc019CType>
	{
		public CC019CMessagePrettier(CC019CMessageDataObject messageDataObject)
			: base(messageDataObject)
		{
		}

		protected override ZString GetMessageInterpretationCore(Cc019CType messageObject)
		{
			var transitOperation = messageObject.TransitOperation;
			return ToKeyValuePairSection(new (ZString key, ZString value)[]
			{
				((NoResString)"Status", NCTS5DepartureCustomsStatusList.Descriptions.DiscrepanciesAtDestination ?? ZString.Empty),
				("MRN", transitOperation?.Mrn ?? ZString.Empty),
				((NoResString)"Discrepancies Notification Date", transitOperation?.DiscrepanciesNotificationDate.ToString() ?? ZString.Empty),
				((NoResString)"Discrepancies Notification Text", transitOperation?.DiscrepanciesNotificationText ?? ZString.Empty),
			});
		}
	}
}
