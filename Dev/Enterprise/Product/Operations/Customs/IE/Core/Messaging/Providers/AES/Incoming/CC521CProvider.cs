using CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.CC521C;
using CargoWise.Types;

namespace Enterprise.Customs.IE.Messaging.AES
{
	public class CC521CProvider
	{
		public CC521CProvider(Cc521C xmlObject)
		{
			this.xmlObject = xmlObject;
		}
		readonly Cc521C xmlObject;

		public ZString MovementReferenceNumber => xmlObject.ExportOperation?.Mrn;
		public ZString DiversionRejectionReasonCode => xmlObject.ExportOperation?.DiversionRejectionReasonCode;
		public ZString DiversionRejectionText => xmlObject.ExportOperation?.DiversionRejectionText;
		public ZString CustomsOfficeOfExitActual => xmlObject.CustomsOfficeOfExitActual?.ReferenceNumber;
	}
}
