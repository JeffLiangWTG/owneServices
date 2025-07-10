using CargoWise.Types;
using Enterprise.MasterFiles.Business.MessageDelivery;
using Enterprise.MasterFiles.Business.MessageDelivery.Testing;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.GB.EMCS.Business.Testing
{
	[TestedType(typeof(PreValidateTraderEHubDelivery))]
	sealed class PreValidateTraderEHubDeliveryTest : EServicesDeliveryTest
	{
		protected override EServicesDelivery Delivery => new PreValidateTraderEHubDelivery(null);

		protected override ZString MessageDataLogLinkerEventCode() => Events.ServiceRequestedCode;
	}
}
