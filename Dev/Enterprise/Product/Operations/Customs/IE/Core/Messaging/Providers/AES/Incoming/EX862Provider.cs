using CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.EX862;
using CargoWise.Types;

namespace Enterprise.Customs.IE.Messaging
{
	public class EX862Provider
	{
		public EX862Provider(Ex862 xmlObject)
		{
			this.xmlObject = xmlObject;
		}
		readonly Ex862 xmlObject;

		public ZString MovementReferenceNumber => xmlObject.ExportOperation?.Mrn;
		public ZString CaseId => xmlObject.ExportOperation?.CaseId;
		public ZString AmendmentRequestCancellationReason => xmlObject.ExportOperation?.AmendmentRequestCancellationReason;
	}
}
