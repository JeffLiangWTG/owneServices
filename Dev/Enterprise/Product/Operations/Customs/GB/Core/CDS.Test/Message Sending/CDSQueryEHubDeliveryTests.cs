using CargoWise.Types;
using Enterprise.MasterFiles.Business.MessageDelivery;
using Enterprise.MasterFiles.Business.MessageDelivery.Testing;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.GB.CDS.Messaging.MessageManagers.Testing
{
	public class CDSQueryEHubDeliveryTests : EServicesDeliveryTest
	{
		protected override EServicesDelivery Delivery
		{
			get
			{
				return new CDSQueryEHubDelivery(null);
			}
		}

		protected override ZString MessageDataLogLinkerEventCode()
		{
			return Events.ServiceRequestedCode;
		}
	}
}
