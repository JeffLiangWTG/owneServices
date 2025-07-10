using CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.EX884;
using CargoWise.Types;

namespace Enterprise.Customs.IE.Messaging.AES
{
	public class EX884Provider
	{
		public EX884Provider(Ex884 xmlObject)
		{
			this.xmlObject = xmlObject;
		}
		readonly Ex884 xmlObject;

		public ZString MovementReferenceNumber => xmlObject.ExportOperation?.Mrn;
		public ZString CaseId => xmlObject.ExportOperation?.CaseId;
		public ZString DocumentsPresentRequestCancellationReason => xmlObject.ExportOperation?.DocumentsPresentRequestCancellationReason;
	}
}
