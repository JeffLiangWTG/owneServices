using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.DocumentWrappers.GenericWrappers.Base.Testing;
using Enterprise.Freight.CarbonEmissions.Business;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.Forwarding.Business;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(CO2eEmissionWrapperCollection))]
	sealed class CO2eEmissionWrapperCollectionTest : GenericWrapperCollectionTest<CO2eEmissionWrapperCollection>
	{
		#region Constructors

		public void TestConstructorFromShipment()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C00001111";

			var shipment = consol.Shipments.AddNew();
			shipment.JS_UnitOfWeight = Constants.Weight.Pounds;
			shipment.JS_ActualWeight = 100m;

			var transport1 = shipment.Transports.AddNew();
			transport1.JW_TransportMode = Constants.TransportModes.Sea;
			transport1.JW_RL_NKLoadPort = "CNSHA";
			transport1.JW_RL_NKDiscPort = "AUSYD";
			transport1.SetCO2ePerTonneInKg(11m);

			var transport2 = consol.Transports[0];
			transport2.JW_TransportMode = Constants.TransportModes.Air;
			transport2.JW_RL_NKLoadPort = "USANY";
			transport2.JW_RL_NKDiscPort = "AUSYD";
			transport2.SetCO2ePerTonneInKg(20m);

			shipment.SetCO2ePerTonneInKg(200m);
			shipment.SetCO2eStatus(CO2eStatusList.Codes.Current);

			var wrapperCollection = new CO2eEmissionWrapperCollection(shipment, Factory);
			AssertEquals(1, wrapperCollection.Count);
			AssertEquals(100m, wrapperCollection[0].Weight);
			AssertEquals(new ZString("USANY to AUSYD"), wrapperCollection[0].RouteText);
			AssertEquals("AIR", wrapperCollection[0].TransportMode);
			AssertEquals("0.907", wrapperCollection[0].FormattedCO2e);
		}

		#endregion

		#region Implementation

		protected override GenericWrapper GetNewWrapperToAddToTheCollection()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			var shipment = consol.Shipments.AddNew();
			var transport = consol.Transports[0];
			return new CO2eEmissionWrapper(shipment, transport, Factory);
		}

		protected override CO2eEmissionWrapperCollection GetNewDocumentWrapperCollection()
		{
			return new CO2eEmissionWrapperCollection(null, Factory);
		}

		#endregion
	}
}
