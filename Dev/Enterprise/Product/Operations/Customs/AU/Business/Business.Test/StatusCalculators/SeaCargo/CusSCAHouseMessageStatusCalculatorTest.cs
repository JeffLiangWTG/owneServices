using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CusSCAHouseMessageStatusCalculator))]
	sealed class CusSCAHouseMessageStatusCalculatorTest : NonPersistentBusinessObjectTestCase
	{
		public void TestInterestedMessageTypes()
		{
			AssertEquals("Length", 1, Calculator.InterestedMessageTypes.Length);
			AssertEquals("InterestedMessageTypes[0]", CMRMessage.CMRMessageTypes.SEACR, Calculator.InterestedMessageTypes[0]);
		}

		public void TestParentForCargoReportingEvents()
		{
			var house = Factory.New<CusSCAHouse>();
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			house.CA_JS = shipment.PK;
			CusSCAHouseMessageStatusCalculator calculator = new CusSCAHouseMessageStatusCalculator(house);
			AssertEquals("correct parent for events", shipment, ((ICMRCargoReportEventsLogger)calculator).ParentForCargoReportingEvents);
		}

		protected override BusinessObject GetNewBusinessObject() => new CusSCAHouseMessageStatusCalculator(CusSCAHouse.New(Factory));

		CusSCAHouseMessageStatusCalculator calculator;
		CusSCAHouseMessageStatusCalculator Calculator => calculator ?? (calculator = (CusSCAHouseMessageStatusCalculator)GetNewBusinessObject());
	}
}
