using CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.EX882;
using CargoWise.Types;

namespace Enterprise.Customs.IE.Messaging.AES
{
	public class EX882Provider
	{
		public EX882Provider(Ex882 xmlObject)
		{
			this.xmlObject = xmlObject;
		}
		readonly Ex882 xmlObject;

		public ZString MovementReferenceNumber => xmlObject.ExportOperation?.Mrn;
		public ZString CaseId => xmlObject.ExportOperation?.CaseId;
		public ZString DocumentsUploadRequestCancellationReason => xmlObject.ExportOperation?.DocumentsUploadRequestCancellationReason;
	}
}
