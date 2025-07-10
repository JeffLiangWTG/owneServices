using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.CC023C;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.ctypes;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.IE.NCTS.Messaging
{
	public class CC023CProvider
	{
		public CC023CProvider(Cc023CType xmlObject)
		{
			this.xmlObject = xmlObject;
		}
		readonly Cc023CType xmlObject;

		public ZString MRN => xmlObject.TransitOperation?.Mrn;
		public ZDateTime DeclarationAcceptanceDate => (xmlObject.TransitOperation?.DeclarationAcceptanceDate).ConvertToZDateTime();
		public ZString CustomsOfficeOfRecoveryAtdeparture => xmlObject.CustomsOfficeOfDeparture?.ReferenceNumber;
		public ZString NameOfGuarantor => xmlObject.Guarantor?.Name;
		public ZString AddressOfGuarantor => xmlObject.Guarantor?.Address is AddressType16 address ? ZString.Join(", ", new ZString[] { address.StreetAndNumber, address.City, address.Country.ToString().ToUpper() }) : ZString.Empty;
		public ZDateTime GuarantorNotificationDate => (xmlObject.GuarantorNotification?.GuarantorNotificationDate).ConvertToZDateTime();
		public ZString GuarantorNotificationText => xmlObject.GuarantorNotification?.GuarantorNotificationText;
	}
}
