using CargoWise.Customs.IE.MessageDefinitions;
using CargoWise.Types;

namespace Enterprise.Customs.IE.Messaging
{
	public class IM862Provider
	{
		public IM862Provider(IIM862XmlObject xmlObject)
		{
			this.xmlObject = xmlObject;
		}
		readonly IIM862XmlObject xmlObject;

		public ZString MovementReferenceNumber => xmlObject.ImportOperation?.Mrn;

		public ZString AmendmentRequestCancellationReason => xmlObject.ImportOperation?.AmendmentRequestCancellationReason;

		public ZString CaseID => xmlObject.ImportOperation?.CaseId;
	}
}
