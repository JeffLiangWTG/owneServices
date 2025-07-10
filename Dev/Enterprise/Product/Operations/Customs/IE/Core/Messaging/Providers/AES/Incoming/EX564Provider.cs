using CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.EX564;
using CargoWise.Types;

namespace Enterprise.Customs.IE.Messaging.AES
{
	public class EX564Provider
	{
		public EX564Provider(Ex564 xmlObject)
		{
			this.xmlObject = xmlObject;
		}
		readonly Ex564 xmlObject;

		public ZString MovementReferenceNumber => xmlObject.ExportOperation?.Mrn;
		public ZString CaseId => xmlObject.ExportOperation?.CaseId;
		public ZString Remarks => xmlObject.ExportOperation?.Remarks;
	}
}
