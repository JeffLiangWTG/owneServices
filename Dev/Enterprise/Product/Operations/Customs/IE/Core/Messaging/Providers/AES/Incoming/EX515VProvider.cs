using CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.EX515V;
using CargoWise.Types;

namespace Enterprise.Customs.IE.Messaging.AES
{
	public class EX515VProvider
	{
		public EX515VProvider(Ex515V xmlObject)
		{
			this.xmlObject = xmlObject;
		}
		readonly Ex515V xmlObject;

		public ZString LocalReferenceNumber => xmlObject.ExportOperation?.Lrn;

		public ZString MovementReferenceNumber => xmlObject.ExportOperation?.Mrn;

		public ZDateTime DeclarationAcknowledgementDate => new ZDateTime(xmlObject.ExportOperation?.DeclarationAcknowledgementDate);
	}
}
