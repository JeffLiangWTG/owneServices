using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.CC019C;
using CargoWise.Types;

namespace Enterprise.Customs.IE.NCTS.Messaging
{
	public class CC019CProvider
	{
		Cc019CType XmlObject { get; }

		public CC019CProvider(Cc019CType xmlObject)
		{
			XmlObject = xmlObject;
		}

		public ZString MRN => XmlObject.TransitOperation?.Mrn ?? ZString.Empty;
		public ZDate DiscrepancyDate => new ZDate(XmlObject.TransitOperation?.DiscrepanciesNotificationDate);
		public ZString DiscrepancyNotificationText => XmlObject.TransitOperation?.DiscrepanciesNotificationText ?? ZString.Empty;
	}
}
