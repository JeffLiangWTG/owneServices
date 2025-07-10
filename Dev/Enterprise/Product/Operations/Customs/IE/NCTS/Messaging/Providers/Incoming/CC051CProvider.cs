using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.CC051C;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.IE.NCTS.Messaging
{
	public class CC051CProvider
	{
		Cc051CType XmlObject { get; }

		public CC051CProvider(Cc051CType xmlObject)
		{
			XmlObject = xmlObject;
		}

		public ZString MovementReferenceNumber => XmlObject.TransitOperation?.Mrn ?? ZString.Empty;

		public ZString NoReleaseMotivationCode => XmlObject.TransitOperation?.NoReleaseMotivationCode ?? ZString.Empty;

		public ZString NoReleaseMotivationText => XmlObject.TransitOperation?.NoReleaseMotivationText ?? ZString.Empty;

		public ZString CustomsOfficeOfDeparture => XmlObject.CustomsOfficeOfDeparture?.ReferenceNumber ?? ZString.Empty;

		public ZDateTime DeclarationSubmissionDateAndTime => (XmlObject.TransitOperation?.DeclarationSubmissionDateAndTime).ConvertToZDateTime();
	}
}
