using CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.CC604C;
using CargoWise.Types;

namespace Enterprise.Customs.IE.Messaging.AES
{
	public class CC604CProvider
	{
		public CC604CProvider(Cc604C xmlObject)
		{
			this.xmlObject = xmlObject;
		}
		readonly Cc604C xmlObject;

		public ZString MovementReferenceNumber => xmlObject.ExportOperation?.Mrn;

		public ZDateTime AmendmentSubmissionDateTime => new ZDateTime(xmlObject.ExportOperation?.AmendmentDateAndTime);

		public ZDateTime AmendmentAcceptanceDateTime => new ZDateTime(xmlObject.ExportOperation?.AmendmentAcceptanceDateAndTime);
	}
}
