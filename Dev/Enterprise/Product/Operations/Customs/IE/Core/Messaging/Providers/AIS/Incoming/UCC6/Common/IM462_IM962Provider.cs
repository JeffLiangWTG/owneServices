using CargoWise.Customs.IE.MessageDefinitions;
using CargoWise.Types;

namespace Enterprise.Customs.IE.Messaging
{
	public class IM462_IM962Provider
	{
		public IM462_IM962Provider(IIM462_IM962XmlObject xmlObject)
		{
			this.xmlObject = xmlObject;
		}
		readonly IIM462_IM962XmlObject xmlObject;

		public ZString MovementReferenceNumber => xmlObject.ImportOperation?.Mrn;

		public ZString CaseId => xmlObject.ImportOperation?.CaseId;

		public ZString AmendReason => xmlObject.ImportOperation?.AmendReason;
	}
}
