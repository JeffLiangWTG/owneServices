using CargoWise.EntityFramework;

namespace Enterprise.Customs.IE.Business
{
	public class CustomsAndExciseReportOutboundMessageCollection : ActiveBusinessObjectCollection<CustomsAndExciseReportOutboundMessage>
	{
		public CustomsAndExciseReportOutboundMessageCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		protected override bool AllowNew => false;
	}
}
