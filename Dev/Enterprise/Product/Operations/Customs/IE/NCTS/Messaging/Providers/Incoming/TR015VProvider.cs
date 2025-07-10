using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.TR015V;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.IE.NCTS.Messaging
{
	public class TR015VProvider
	{
		Tr015V XmlObject { get; }

		public TR015VProvider(Tr015V xmlObject)
		{
			XmlObject = xmlObject;
		}
		public ZString MovementReferenceNumber => XmlObject.Declaration?.Mrn;
		public ZString LocalReferenceNumber => XmlObject.Declaration?.Lrn;
		public ZDateTime DeclarationAcknowledgementDate => (XmlObject.Declaration?.DeclarationAcknowledgmentDate).ConvertToZDateTime();
	}
}
