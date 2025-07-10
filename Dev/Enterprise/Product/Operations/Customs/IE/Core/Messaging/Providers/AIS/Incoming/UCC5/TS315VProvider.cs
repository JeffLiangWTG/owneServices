using CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.TS315V;
using CargoWise.Types;

namespace Enterprise.Customs.IE.Messaging.UCC5
{
	public class TS315VProvider
	{
		public TS315VProvider(Ts315V xmlObject)
		{
			this.xmlObject = xmlObject;
		}
		readonly Ts315V xmlObject;

		public ZString MovementReferenceNumber => xmlObject.Declaration.Mrn;

		public ZString LRN => xmlObject.Declaration.Lrn25;

		public ZDateTime AcknowledgementDate
		{
			get
			{
				new ZString(xmlObject.Declaration.DeclarationAcknowledgementDate).TryParseToDate(out var ackDate);
				return ackDate;
			}
		}
	}
}
