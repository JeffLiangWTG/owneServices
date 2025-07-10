using CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.CC574C;
using CargoWise.Types;

namespace Enterprise.Customs.IE.Messaging.AES
{
	public class CC574CProvider
	{
		public CC574CProvider(Cc574C xmlObject)
		{
			this.xmlObject = xmlObject;
		}
		readonly Cc574C xmlObject;

		public ZString MovementReferenceNumber => xmlObject.ExportOperation?.Mrn;

		public ZDateTime AmendmentDateAndTime => new ZDateTime(xmlObject.ExportOperation?.AmendmentDateAndTime);

		public ZDateTime AmendmentAcceptanceDateAndTime => new ZDateTime(xmlObject.ExportOperation?.AmendmentAcceptanceDateAndTime);
	}
}
