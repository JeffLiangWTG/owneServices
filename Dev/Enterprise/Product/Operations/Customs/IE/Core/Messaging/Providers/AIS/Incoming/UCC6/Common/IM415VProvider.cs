using CargoWise.Customs.IE.MessageDefinitions;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.IE.Messaging
{
	public class IM415VProvider
	{
		public IM415VProvider(IIM415VXmlObject xmlObject)
		{
			this.xmlObject = xmlObject;
		}
		readonly IIM415VXmlObject xmlObject;

		public ZString AdditionalDeclarationType => xmlObject.ImportOperation?.AdditionalDeclarationType;

		public ZString LocalReferenceNumber => xmlObject.ImportOperation?.Lrn;

		public ZString MovementReferenceNumber => xmlObject.ImportOperation?.Mrn;

		public ZDateTime DeclarationAcknowledgementDate => (xmlObject.ImportOperation?.DeclarationAcknowledgementDate).ConvertToZDateTime();
	}
}
