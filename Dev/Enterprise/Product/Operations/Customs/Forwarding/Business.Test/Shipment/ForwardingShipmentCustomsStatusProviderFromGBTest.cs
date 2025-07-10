using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Forwarding.Business.Testing
{
	[CountrySpecificTest(Core.Constants.CountryCodes.UnitedKingdom)]
	class ForwardingShipmentCustomsStatusProviderFromGBTest : TestCaseWithFactory
	{
		public void TestGetsTheRightForwardingShipmentCustomsStatusProvider()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var statusProvider = ForwardingShipmentCustomsStatusProvider.New(shipment);
			NUnit.Framework.Assert.That(statusProvider, Is.Not.EqualTo(default(ForwardingShipmentCustomsStatusProvider)), "ForwardingShipmentCustomsStatusProvider.New(shipment) - should not be [null]");
			NUnit.Framework.Assert.That(statusProvider.GetType(), Is.EqualTo(ObjectFactory.GetType<Integration.Customs.GB.CCSUK.ICcsukForwardingShipmentCustomsStatusProvider>()), "ForwardingShipmentCustomsStatusProvider.New(shipment).GetType()");

			AssertNoExceptionThrown(delegate
			{
				statusProvider.CustomsCargoStatus();
				statusProvider.CustomsMessageStatus();
			});
		}
	}
}
