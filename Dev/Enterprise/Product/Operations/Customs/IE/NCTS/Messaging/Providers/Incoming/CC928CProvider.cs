using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.CC928C;
using CargoWise.Types;

namespace Enterprise.Customs.IE.NCTS.Messaging
{
	public class CC928CProvider
	{
		Cc928CType XmlObject { get; }

		public CC928CProvider(Cc928CType xmlObject)
		{
			XmlObject = xmlObject;
		}

		public ZString LocalReferenceNumber => XmlObject.TransitOperation?.Lrn ?? ZString.Empty;

		public ZString ReferenceNumber => XmlObject.CustomsOfficeOfDeparture?.ReferenceNumber ?? ZString.Empty;
	}
}
