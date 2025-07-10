using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.CC035C;
using CargoWise.Types;

namespace Enterprise.Customs.IE.NCTS.Messaging
{
	public class CC035CProvider
	{
		Cc035CType XmlObject { get; }

		public CC035CProvider(Cc035CType xmlObject)
		{
			XmlObject = xmlObject;
		}

		public ZString MRN => XmlObject.TransitOperation?.Mrn ?? ZString.Empty;
		public ZDate DeclarationAcceptanceDate => new ZDate(XmlObject.TransitOperation?.DeclarationAcceptanceDate);
		public ZDate RecoveryNotificationDate => new ZDate(XmlObject.RecoveryNotification?.RecoveryNotificationDate);
		public ZString RecoveryNotificationText => XmlObject.RecoveryNotification?.RecoveryNotificationText ?? ZString.Empty;
		public ZString AmountClaimed => XmlObject.RecoveryNotification is null ? ZString.Empty : (ZString)(XmlObject.RecoveryNotification.AmountClaimed + XmlObject.RecoveryNotification.Currency);
		public ZString CustomsOfficeOfRecoveryAtDeparture => XmlObject.CustomsOfficeOfRecoveryAtDeparture?.ReferenceNumber ?? ZString.Empty;
	}
}
