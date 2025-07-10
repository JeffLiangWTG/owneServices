using CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.EX864;
using CargoWise.Types;

namespace Enterprise.Customs.IE.Messaging.AES
{
	public class EX864Provider
	{
		public EX864Provider(Ex864 xmlObject)
		{
			this.xmlObject = xmlObject;
		}
		readonly Ex864 xmlObject;

		public ZString MovementReferenceNumber => xmlObject.ExportOperation?.Mrn;
		public ZString CaseId => xmlObject.ExportOperation?.CaseId;
		public ZString InvalidationRequestCancellationReason => xmlObject.ExportOperation?.InvalidationRequestCancellationReason;
	}
}
