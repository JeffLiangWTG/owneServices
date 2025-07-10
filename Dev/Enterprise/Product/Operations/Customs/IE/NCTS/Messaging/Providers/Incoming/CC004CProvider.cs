using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.CC004C;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.IE.NCTS.Messaging
{
	public class CC004CProvider
	{
		Cc004CType XmlObject { get; }

		public CC004CProvider(Cc004CType xmlObject)
		{
			XmlObject = xmlObject;
		}

		public ZString MRN => XmlObject.TransitOperation?.Mrn ?? ZString.Empty;

		public ZDateTime AmendmentSubmissionDateTime => (XmlObject.TransitOperation?.AmendmentSubmissionDateAndTime).ConvertToZDateTime();

		public ZDateTime AmendmentAcceptanceDateTime => (XmlObject.TransitOperation?.AmendmentAcceptanceDateAndTime).ConvertToZDateTime();
	}
}
