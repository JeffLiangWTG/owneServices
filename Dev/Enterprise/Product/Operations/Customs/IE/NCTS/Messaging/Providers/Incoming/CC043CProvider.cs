using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.CC043C;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.ctypes;
using CargoWise.Types;

namespace Enterprise.Customs.IE.NCTS.Messaging
{
	public class CC043CProvider
	{
		Cc043CType XmlObject { get; }

		public CC043CProvider(Cc043CType xmlObject)
		{
			XmlObject = xmlObject;
		}

		public ZString MRN => XmlObject.TransitOperation?.Mrn;

		public ZString Security => XmlObject.TransitOperation?.Security;

		public ZString CustomsOfficeOfDestinationActual => XmlObject.CustomsOfficeOfDestinationActual?.ReferenceNumber;

		public ZString TraderAtDestination => XmlObject.TraderAtDestination?.IdentificationNumber;

		public ZString Address => XmlObject.HolderOfTheTransitProcedure?.Address is AddressType10 address ? ZString.Join(", ", new ZString[] { address.StreetAndNumber, address.City, address.Country }) : ZString.Empty;

		public CC043CConsignmentProvider Consignment => consignment ?? (consignment = new CC043CConsignmentProvider(XmlObject.Consignment));
		CC043CConsignmentProvider consignment;
	}
}
