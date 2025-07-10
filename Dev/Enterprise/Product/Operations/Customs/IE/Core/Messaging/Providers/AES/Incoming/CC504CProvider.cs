using CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.CC504C;
using CargoWise.Types;

namespace Enterprise.Customs.IE.Messaging.AES
{
	public class CC504CProvider
	{
		public CC504CProvider(Cc504C xmlObject)
		{
			this.xmlObject = xmlObject;
		}
		readonly Cc504C xmlObject;

		public ZString LocalReferenceNumber => xmlObject.ExportOperation?.Lrn;

		public ZString MovementReferenceNumber => xmlObject.ExportOperation?.Mrn;

		public ZDateTime AmendmentSubmissionDate => new ZDateTime(xmlObject.ExportOperation?.AmendmentDateAndTime);

		public ZDateTime AmendmentAcceptanceDate => new ZDateTime(xmlObject.ExportOperation?.AmendmentAcceptanceDateAndTime);
	}
}
