using CargoWise.Customs.IE.MessageDefinitions;
using CargoWise.Types;

namespace Enterprise.Customs.IE.Messaging
{
	public class IM882Provider
	{
		public IM882Provider(IIM882XmlObject xmlObject)
		{
			this.xmlObject = xmlObject;
		}
		readonly IIM882XmlObject xmlObject;

		public ZString MovementReferenceNumber => xmlObject.ImportOperation?.Mrn;

		public ZString CaseId => xmlObject.ImportOperation?.CaseId;

		public ZString DocumentsUploadRequestCancellationReason => xmlObject.ImportOperation?.DocumentsUploadRequestCancellationReason;
	}
}
