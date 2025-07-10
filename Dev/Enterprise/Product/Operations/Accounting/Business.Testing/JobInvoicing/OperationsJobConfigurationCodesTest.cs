using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	public class OperationsJobConfigurationCodesTest : TestCaseWithFactory
	{
		public void TestConsumerTypeCode()
		{
			AssertEquals("ConsumerTypeCode", JobInvoicingConsumerTypes.Shipment.Code, CodesForShipment.ConsumerTypeCode);

			AssertEquals("ConsumerTypeCode", JobInvoicingConsumerTypes.ForwardingConsol.Code, CodesForConsol.ConsumerTypeCode);
		}

		public void TestDirectionCode_Shipment()
		{
			Shipment.JS_RL_NKOrigin = "AUSYD";
			Shipment.JS_RL_NKDestination = "NZAKL";
			AssertEquals("DirectionCode", Constants.FreightShipmentDirection.Code.Export, CodesForShipment.DirectionCode);

			Shipment.JS_RL_NKOrigin = "NZAKL";
			Shipment.JS_RL_NKDestination = "AUSYD";
			AssertEquals("DirectionCode", Constants.FreightShipmentDirection.Code.Import, CodesForShipment.DirectionCode);

			Shipment.JS_RL_NKOrigin = "AUSYD";
			Shipment.JS_RL_NKDestination = "AUMEL";
			AssertEquals("DirectionCode", Constants.FreightShipmentDirection.Code.Domestic, CodesForShipment.DirectionCode);

			Shipment.JS_RL_NKOrigin = "USLAX";
			Shipment.JS_RL_NKDestination = "NZAKL";
			AssertEquals("DirectionCode", Constants.FreightShipmentDirection.Code.Other, CodesForShipment.DirectionCode);
		}

		public void TestDirectionCode_Consol()
		{
			Consol.JK_RL_NKLoadPort = "AUSYD";
			Consol.JK_RL_NKDischargePort = "NZAKL";
			AssertEquals("DirectionCode", Constants.FreightShipmentDirection.Code.Export, CodesForConsol.DirectionCode);

			Consol.JK_RL_NKLoadPort = "NZAKL";
			Consol.JK_RL_NKDischargePort = "AUSYD";
			AssertEquals("DirectionCode", Constants.FreightShipmentDirection.Code.Import, CodesForConsol.DirectionCode);

			Consol.JK_RL_NKLoadPort = "AUSYD";
			Consol.JK_RL_NKDischargePort = "AUMEL";
			AssertEquals("DirectionCode", Constants.FreightShipmentDirection.Code.Domestic, CodesForConsol.DirectionCode);

			Consol.JK_RL_NKLoadPort = "USLAX";
			Consol.JK_RL_NKDischargePort = "NZAKL";
			AssertEquals("DirectionCode", Constants.FreightShipmentDirection.Code.Other, CodesForConsol.DirectionCode);
		}

		public void TestTransportMode()
		{
			Shipment.JS_TransportMode = Constants.TransportModes.Air;
			AssertEquals("TransportMode", Constants.TransportModes.Air, CodesForShipment.TransportMode);

			Shipment.JS_TransportMode = Constants.TransportModes.Sea;
			AssertEquals("TransportMode", Constants.TransportModes.Sea, CodesForShipment.TransportMode);

			Shipment.JS_TransportMode = string.Empty;
			AssertEquals("TransportMode", InvoiceDateConfigurationLookups.ModeAdditionalCodes.All, CodesForShipment.TransportMode);

			Consol.JK_TransportMode = Constants.TransportModes.Air;
			AssertEquals("TransportMode", Constants.TransportModes.Air, CodesForConsol.TransportMode);

			Consol.JK_TransportMode = Constants.TransportModes.Sea;
			AssertEquals("TransportMode", Constants.TransportModes.Sea, CodesForConsol.TransportMode);

			Consol.JK_TransportMode = string.Empty;
			AssertEquals("TransportMode", InvoiceDateConfigurationLookups.ModeAdditionalCodes.All, CodesForConsol.TransportMode);
		}

		public void TestBroker()
		{
			AssertEquals("Broker", string.Empty, CodesForConsol.Broker);

			Shipment.JS_RL_NKOrigin = "AUSYD";
			Shipment.JS_RL_NKDestination = "AUMEL";
			AssertEquals("Broker", InvoiceDateConfigurationLookups.BrokerCodes.All, CodesForShipment.Broker);

			Shipment.JS_RL_NKOrigin = "NZAKL";
			Shipment.JS_RL_NKDestination = "AUSYD";
			Shipment.JS_OH_ImportBroker = Creator.ABIGAS.PK;
			AssertEquals("Broker", InvoiceDateConfigurationLookups.BrokerCodes.External, CodesForShipment.Broker);

			Shipment.JS_RL_NKOrigin = "NZAKL";
			Shipment.JS_RL_NKDestination = "AUSYD";
			Shipment.JS_OH_ImportBroker = Creator.ABIGAS.PK;
			GlbCompany.CurrentCompany.GC_OH_OrgProxy = Creator.ABIGAS.PK;
			AssertEquals("Broker", InvoiceDateConfigurationLookups.BrokerCodes.Internal, CodesForShipment.Broker);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			Creator = new TestObjectCreator(Factory);

			Shipment = Creator.CreateShipment("S00001000");
			CodesForShipment = new OperationsJobConfigurationCodes(Shipment);

			Consol = Creator.CreateConsol("AUSYD", "NZAKL", "C00001000");
			CodesForConsol = new OperationsJobConfigurationCodes(Consol);
		}

		protected TestObjectCreator Creator;
		protected ForwardingShipment Shipment;
		protected ForwardingConsol Consol;
		protected OperationsJobConfigurationCodes CodesForShipment;
		protected OperationsJobConfigurationCodes CodesForConsol;

		#endregion
	}
}