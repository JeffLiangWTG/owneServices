using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using NUnit.Framework;

namespace Enterprise.Client.JAS.Business.JXC.Export.Testing
{
	internal class APRSLineTest : AirProfitShareLineBaseTestCase
	{
		[ExpectException(typeof(ArgumentNullException))]
		public void TestConstructorNullConsol()
		{
			new APRSLine(null);
		}

		public void TestLineAsString()
		{
			string expectedString = "APRS3100;N;PER;ATL;081;12345678;12/11/2006;C001;2;25;240.5;2200;1100;3350;150;AUD";
			AssertEquals(expectedString, Line.LineAsString);
		}

		public void TestLineAsString_PortOfLoadingAndDischargeDontExist()
		{
			Consol.JK_RL_NKLoadPort = "";
			Consol.JK_RL_NKDischargePort = "";
			Consol.SetDefaultReceivingForwarderAddress(ReceivingForwarder);
			string expectedString = "APRS3100;N;;;081;12345678;12/11/2006;C001;2;25;240.5;2200;1100;3350;150;AUD";
			AssertEquals(expectedString, Line.LineAsString);
		}

		public void TestLineAsString_UseFirstAirTransportRatherThanTheMainTransportIfAvailable()
		{
			Consol.Transports[0].JW_TransportMode = Core.Constants.TransportModes.Road;
			Transport newTransport = Consol.Transports.AddNew();
			newTransport.JW_LegOrder = 2;
			newTransport.JW_TransportMode = Core.Constants.TransportModes.Air;
			newTransport.JW_ETD = new ZDateTime(2006, 1, 1);
			string expectedString = "APRS3100;N;PER;ATL;081;12345678;01/01/2006;C001;2;25;240.5;2200;1100;3350;150;AUD";
			AssertEquals(expectedString, Line.LineAsString);
		}

		public void TestLineAsString_ShouldNotThrowExceptionIfNoTransportAvailable()
		{
			Consol.Transports.RemoveAll();
			string expectedString = "APRS3100;N;PER;ATL;081;12345678;;C001;2;25;240.5;2200;1100;3350;150;AUD";
			AssertEquals(expectedString, Line.LineAsString);
		}

		protected override MessageLine GetMessageLine()
		{
			return new APRSLine(Consol);
		}

		protected override int ExpectedFieldCount
		{
			get
			{
				return JXCConstants.APRSFieldCount;
			}
		}

		protected override ZString ExpectedLineType
		{
			get
			{
				return JXCConstants.LineTypes.APRS;
			}
		}

		JASForwardingConsol Consol
		{
			get
			{
				if (fConsol == null)
				{
					fConsol = Factory.New<JASForwardingConsol>();
					fConsol.JK_TransportMode = Core.Constants.TransportModes.Air;
					fConsol.JK_RL_NKLoadPort = "AUPER";
					fConsol.JK_RL_NKDischargePort = "USATL";
					fConsol.Transports[0].JW_ETD = new ZDateTime(2006, 11, 12);
					fConsol.JK_UniqueConsignRef = "C001";
					fConsol.JK_MasterBillNum = "08112345678";
					fConsol.SetDefaultReceivingForwarderAddress(ReceivingForwarder);
					fConsol.Shipments.Add(Shipment1);
					fConsol.Shipments.Add(Shipment2);
				}

				return fConsol;
			}
		}

		JASForwardingShipment Shipment1
		{
			get
			{
				if (fShipment1 == null)
				{
					fShipment1 = Factory.New<JASForwardingShipment>();
					fShipment1.JS_TransportMode = Core.Constants.TransportModes.Air;
					fShipment1.JS_RL_NKOrigin = "AUBME";
					fShipment1.JS_RL_NKDestination = "USCHI";
					fShipment1.JS_OuterPacks = 20;
					fShipment1.JS_ActualChargeable = 100m;
					AddCharge(Shipment1, ZGuid.Empty, 0, 1000, 1100, 0, ReceivingForwarder);
					AddCharge(Shipment1, Env.Registry.FreightChargeCode, 2000, 0, 0, 3000, ReceivingForwarder);
					AddCharge(Shipment1, AccountingConfigurationRegistry.Instance.ProfitShareChargeCode.Value, 0, 0, 0, -100, ReceivingForwarder);
				}

				return fShipment1;
			}
		}

		JASForwardingShipment Shipment2
		{
			get
			{
				if (fShipment2 == null)
				{
					fShipment2 = Factory.New<JASForwardingShipment>();
					fShipment2.JS_TransportMode = Core.Constants.TransportModes.Air;
					fShipment2.JS_RL_NKOrigin = "AUPER";
					fShipment2.JS_RL_NKDestination = "USATL";
					fShipment2.JS_OuterPacks = 5;
					fShipment2.JS_ActualChargeable = 140.5m;
					AddCharge(Shipment2, ZGuid.Empty, 100, 0, 0, 200, ReceivingForwarder);
					AddCharge(Shipment2, Env.Registry.FreightChargeCode, 0, 200, 350, 0, ReceivingForwarder);
					AddCharge(Shipment2, AccountingConfigurationRegistry.Instance.ProfitShareChargeCode.Value, 0, 0, 0, -100, ReceivingForwarder);
					AddCharge(Shipment2, AccountingConfigurationRegistry.Instance.ProfitShareChargeCode.Value, 0, 0, 50, 0, ReceivingForwarder);
				}

				return fShipment2;
			}
		}

		JASOrgHeader ReceivingForwarder
		{
			get
			{
				if (fReceivingForwarder == null)
				{
					fReceivingForwarder = new BusinessObjectFactory().NewWithValidTestData<JASOrgHeader>();
					fReceivingForwarder.OH_IsDebtor = true;
					fReceivingForwarder.OH_Code = "ZUBTED123";
					fReceivingForwarder.Factory.Save();
				}

				return fReceivingForwarder;
			}
		}

		JASForwardingConsol fConsol;
		JASForwardingShipment fShipment1;
		JASForwardingShipment fShipment2;
		JASOrgHeader fReceivingForwarder;
	}
}
