using CargoWise.Customs.IE.MessageDefinitions;
using CargoWise.Types;

namespace Enterprise.Customs.IE.Messaging
{
	public class IM464Provider
	{
		public IM464Provider(IIM464XmlObject xmlObject)
		{
			this.xmlObject = xmlObject;
		}
		readonly IIM464XmlObject xmlObject;

		public ZString MovementReferenceNumber => xmlObject.ImportOperation?.Mrn;

		public ZString CaseId => xmlObject.ImportOperation?.CaseId;

		public ZString Remarks => xmlObject.ImportOperation?.Remarks;
	}
}
