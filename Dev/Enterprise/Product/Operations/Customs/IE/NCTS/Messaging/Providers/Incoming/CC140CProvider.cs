using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.CC140C;
using CargoWise.Types;

namespace Enterprise.Customs.IE.NCTS.Messaging
{
	public class CC140CProvider
	{
		Cc140CType XmlObject { get; }

		public CC140CProvider(Cc140CType xmlObject)
		{
			XmlObject = xmlObject;
		}

		public ZString MovementReferenceNumber => XmlObject.TransitOperation?.Mrn ?? ZString.Empty;

		public ZDate RequestOnNonArrivedMovementDate => new ZDate(XmlObject.TransitOperation?.RequestOnNonArrivedMovementDate);

		public ZDate LimitForResponseDate => new ZDate(XmlObject.TransitOperation?.LimitForResponseDate);

		public ZString CustomsOfficeOfDeparture => XmlObject.CustomsOfficeOfDeparture?.ReferenceNumber ?? ZString.Empty;

		public ZString CustomsOfficeOfEnquiry => XmlObject.CustomsOfficeOfEnquiryAtDeparture?.ReferenceNumber ?? ZString.Empty;
	}
}
