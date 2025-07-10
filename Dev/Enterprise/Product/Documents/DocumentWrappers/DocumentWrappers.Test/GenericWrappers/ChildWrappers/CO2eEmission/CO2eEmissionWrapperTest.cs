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
	[TestedType(typeof(CO2eEmissionWrapper))]
	sealed class CO2eEmissionWrapperTest : GenericWrapperTest
	{
		public void TestCO2eEmissionWrapper()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C00001111";

			var shipment = consol.Shipments.AddNew();
			shipment.JS_UnitOfWeight = Constants.Weight.Pounds;
			shipment.JS_ActualWeight = 100m;

			var transport = consol.Transports[0];
			transport.JW_TransportMode = Constants.TransportModes.Sea;
			transport.JW_RL_NKLoadPort = "CNSHA";
			transport.JW_RL_NKDiscPort = "AUSYD";
			transport.SetCO2ePerTonneInKg(20m);

			var wrapper = new CO2eEmissionWrapper(shipment, transport, Factory);
			AssertEquals(100m, wrapper.Weight);
			AssertEquals(new ZString("CNSHA to AUSYD"), wrapper.RouteText);
			AssertEquals("SEA", wrapper.TransportMode);
			AssertEquals("0.907", wrapper.FormattedCO2e);

			transport.SetCO2eStatus(CO2eStatusList.Codes.NotCurrent);
			wrapper = new CO2eEmissionWrapper(shipment, transport, Factory);
			AssertEquals(ZString.Empty, wrapper.FormattedCO2e);
		}

		public override void TestWrapperMappingsEmpty()
		{
			var consol = Factory.New<ForwardingConsol>();
			var shipment = consol.Shipments.AddNew();
			var transport = consol.Transports[0];

			var emptyWrapper = new CO2eEmissionWrapper(shipment, transport, Factory);
			AssertEquals(ZDecimal.Zero, emptyWrapper.Weight);
			AssertEquals(ZString.Empty, emptyWrapper.RouteText);
			AssertEquals(ZString.Empty, emptyWrapper.TransportMode);
			AssertEquals(ZString.Empty, emptyWrapper.FormattedCO2e);
		}

		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			var shipment = consol.Shipments.AddNew();
			var transport = consol.Transports[0];
			return new CO2eEmissionWrapper(shipment, transport, Factory);
		}

		protected override string ExpectedFieldMap
		{
			get
			{
				return @"
CO2eEmission
======================================================================
Name                                    Type
----------------------------------------------------------------------
FormattedCO2e                           String
RouteText                               String
TransportMode                           String
Weight                                  Decimal
";
			}
		}

		protected override ZString ExpectedDefaultFormatting
		{
			get
			{
				return @"
Registry : (No Default Field Value Available on Registry)
";
			}
		}

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C00001111";

			var shipment = consol.Shipments.AddNew();
			shipment.JS_UnitOfWeight = Constants.Weight.Pounds;
			shipment.JS_ActualWeight = 100m;

			var transport = consol.Transports[0];
			transport.JW_TransportMode = Constants.TransportModes.Sea;
			transport.JW_RL_NKLoadPort = "CNSHA";
			transport.JW_RL_NKDiscPort = "AUSYD";
			transport.SetCO2ePerTonneInKg(11m);

			return new CO2eEmissionWrapper(shipment, transport, Factory);
		}
	}
}
