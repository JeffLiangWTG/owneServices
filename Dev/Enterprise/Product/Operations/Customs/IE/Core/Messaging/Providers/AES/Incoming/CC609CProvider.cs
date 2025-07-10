using CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.CC609C;
using CargoWise.Types;

namespace Enterprise.Customs.IE.Messaging.AES
{
	public class CC609CProvider
	{
		public CC609CProvider(Cc609C xmlObject)
		{
			this.xmlObject = xmlObject;
		}
		readonly Cc609C xmlObject;

		public ZString MovementReferenceNumber => xmlObject.ExportOperation?.Mrn;

		public ZDateTime InvalidationDecisionDateTime => new ZDateTime(xmlObject.ExportOperation?.InvalidationDecisionDateAndTime);

		public ZDateTime InvalidationRequestDateTime => new ZDateTime(xmlObject.ExportOperation?.InvalidationRequestDateAndTime);
	}
}
