using CargoWise.Customs.FR.MessageDefinitions.TP5.CC035C;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.FR.Business.EdiMessages
{
	public class CC035CMessagePrettier : NCTSMessagePrettier<Cc035CType>
	{
		public CC035CMessagePrettier(NCTSMessageDataObject<Cc035CType> messageDataObject) : base(messageDataObject)
		{
		}

		protected override ZString GetMessageInterpretationCore(Cc035CType messageObject) => ToKeyValuePairSection(new (ZString key, ZString value)[]
		{
				((NoResString)"Status", TP5ResponseMessageSubTypeList.Descriptions.RecoveryNotification ?? ZString.Empty),
				("MRN", messageObject.TransitOperation?.Mrn ?? ZString.Empty),
				((NoResString)"Recovery Notification Date", messageObject.RecoveryNotification?.RecoveryNotificationDate?.ToString("dd/MM/yyyy") ?? ZString.Empty),
				((NoResString)"Recovery Notification Text", messageObject.RecoveryNotification?.RecoveryNotificationText ?? ZString.Empty),
				((NoResString)"Amount Claimed And Currency", GetAmountClaimedAndCurrency(messageObject)),
		});

		ZString GetAmountClaimedAndCurrency(Cc035CType messageObject) => messageObject.RecoveryNotification is null
			? ZString.Empty
			: string.Join(" ", messageObject.RecoveryNotification.AmountClaimed, messageObject.RecoveryNotification.Currency);
	}
}
