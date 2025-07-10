using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Forwarding.Business.Testing
{
	[CountrySpecificTest(Core.Constants.CountryCodes.Australia)]
	class ForwardingShipmentCustomsStatusProviderFromAUTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestStatusDefaultsToEmpty()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var statusProvider = ForwardingShipmentCustomsStatusProvider.New(shipment);

			NUnit.Framework.Assert.That(statusProvider.CustomsCargoStatus(), Is.EqualTo(ZString.Empty));
			NUnit.Framework.Assert.That(statusProvider.CustomsMessageStatus(), Is.EqualTo(ZString.Empty));
		}

		[ExpectNoExceptions]
		public void TestStatusWhenSea()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportCodes.Sea;
			var statusProvider = ForwardingShipmentCustomsStatusProvider.New(shipment);

			NUnit.Framework.Assert.That(statusProvider.CustomsCargoStatus(), Is.EqualTo(ZString.Empty));
			NUnit.Framework.Assert.That(statusProvider.CustomsMessageStatus(), Is.EqualTo(ZString.Empty));
		}

		[ExpectNoExceptions]
		public void TestStatusWhenAir()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportCodes.Air;
			var statusProvider = ForwardingShipmentCustomsStatusProvider.New(shipment);

			NUnit.Framework.Assert.That(statusProvider.CustomsCargoStatus(), Is.EqualTo(ZString.Empty));
			NUnit.Framework.Assert.That(statusProvider.CustomsMessageStatus(), Is.EqualTo(ZString.Empty));
		}
	}
}
