using CargoWise.Customs.IE.MessageDefinitions;
using CargoWise.Types;

namespace Enterprise.Customs.IE.Messaging
{
	public class IM864Provider
	{
		public IM864Provider(IIM864XmlObject xmlObject)
		{
			this.xmlObject = xmlObject;
		}
		readonly IIM864XmlObject xmlObject;

		public ZString MovementReferenceNumber => xmlObject.ImportOperation?.Mrn;

		public ZString CaseId => xmlObject.ImportOperation?.CaseId;

		public ZString InvalidationRequestCancellationReason => xmlObject.ImportOperation?.InvalidationRequestCancellationReason;
	}
}
