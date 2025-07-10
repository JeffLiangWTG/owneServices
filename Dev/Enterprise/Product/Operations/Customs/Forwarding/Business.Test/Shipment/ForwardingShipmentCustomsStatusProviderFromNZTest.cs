using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.Forwarding.Business.Testing
{
	[CountrySpecificTest(Core.Constants.CountryCodes.NewZealand)]
	public class ForwardingShipmentCustomsStatusProviderFromNZTest : TestCaseWithFactory
	{
		public void TestGetsTheRightForwardingShipmentCustomsStatusProviderWhenCallingStaticNew()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var statusProvider = ForwardingShipmentCustomsStatusProvider.New(shipment);
			NUnit.Framework.Assert.That(statusProvider, Is.Not.EqualTo(default(ForwardingShipmentCustomsStatusProvider)), "ForwardingShipmentCustomsStatusProvider.New(shipment) - should not be [null]");
			NUnit.Framework.Assert.That(statusProvider.GetType(), Is.EqualTo(ObjectFactory.GetType<Integration.Customs.NZ.IForwardingShipmentCustomsStatusProvider>()), "ForwardingShipmentCustomsStatusProvider.New(shipment).GetType()");

			AssertNoExceptionThrown(delegate
			{
				statusProvider.CustomsCargoStatus();
				statusProvider.CustomsMessageStatus();
			});
		}

		[ExpectNoExceptions]
		public void TestCustomsStatusRefreshedCorrectly()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var declaration = Factory.New<Integration.Customs.NZ.IJobDeclaration>();
			declaration.JE_JS = shipment.PK;
			declaration.JE_EntryStatus = "STC";
			var statusProvider = ForwardingShipmentCustomsStatusProvider.New(shipment);
			NUnit.Framework.Assert.That(statusProvider.CustomsCargoStatus(), Is.EqualTo("Sent to Customs").Using(CustomComparers.TypeComparison));
			declaration.JE_EntryStatus = "CLR";
			statusProvider = ForwardingShipmentCustomsStatusProvider.New(shipment);
			NUnit.Framework.Assert.That(statusProvider.CustomsCargoStatus(), Is.EqualTo("Entry Cleared").Using(CustomComparers.TypeComparison));
		}
	}
}
