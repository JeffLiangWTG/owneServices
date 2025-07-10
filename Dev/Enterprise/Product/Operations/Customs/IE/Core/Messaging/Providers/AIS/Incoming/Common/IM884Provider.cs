using CargoWise.Customs.IE.MessageDefinitions;
using CargoWise.Types;

namespace Enterprise.Customs.IE.Messaging
{
	public class IM884Provider
	{
		public IM884Provider(IIM884XmlObject xmlObject)
		{
			this.xmlObject = xmlObject;
		}
		readonly IIM884XmlObject xmlObject;

		public ZString MRN => xmlObject.ImportOperation?.Mrn;

		public ZString CaseId => xmlObject.ImportOperation?.CaseId;

		public ZString CancellationReason => xmlObject.ImportOperation?.DocumentsPresentRequestCancellationReason;
	}
}
