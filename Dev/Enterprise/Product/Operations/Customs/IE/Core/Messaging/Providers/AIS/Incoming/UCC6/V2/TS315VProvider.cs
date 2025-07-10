using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.TS315V;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.IE.Messaging
{
	public class TS315VProvider
	{
		public TS315VProvider(Ts315V xmlObject)
		{
			this.xmlObject = xmlObject;
		}

		readonly Ts315V xmlObject;

		public ZString LocalReferenceNumber => xmlObject.Declaration?.Lrn;

		public ZString MovementReferenceNumber => xmlObject.Declaration?.Mrn;

		public ZDateTime DeclarationAcknowledgementDate => (xmlObject.Declaration?.DeclarationAcknowledgementDate).ConvertToZDateTime();
	}
}
