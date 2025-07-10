using CargoWise.Types;

namespace Enterprise.Client.JAS.Business.JXC.Export.Testing
{
	internal class ProfitShareMessageExporterTest : JXCMessageExporterTestCase
	{
		public void TestConstructor()
		{
			AssertEquals("Should be assigned in the constructor", Consol, Exporter.HeaderData);
			AssertEquals("Export Logger should be initialised in the constructor with the Consol as the Export Source", Consol, ((JXCExportLogger)Exporter.NotificationSubscriber).ExportSource);
		}

		public void TestExportValidationTypeToUse()
		{
			AssertEquals("Should use JXC profit share validation", JXCExportValidationType.ProfitShare, Exporter.ExportValidationTypeToUse);
		}

		public void TestExportedMessage()
		{
			MessageLine[] expectedMessageLines = new MessageLine[3];
			expectedMessageLines[0] = new APRSLine(Consol);
			expectedMessageLines[1] = new APRHLine(Consol, Shipment1);
			expectedMessageLines[2] = new APRHLine(Consol, Shipment2);
			ExpectedMessageExporter.HeaderData.FreightDest = "USATL";
			ExpectedMessageExporter.ExpectedMessageFileNamesAndContentLines = new JXCMessageExporter.MessageFileNameAndContents[1];
			ExpectedMessageExporter.ExpectedMessageFileNamesAndContentLines[0] = new JXCMessageExporter.MessageFileNameAndContents("PS_12345678.081", expectedMessageLines);
			AssertExportedMessage(Exporter);
		}

		#region Implementation
		ProfitShareMessageExporter Exporter
		{
			get
			{
				if (fExporter == null)
				{
					fExporter = new ProfitShareMessageExporter(Consol);
				}

				return fExporter;
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
					fConsol.JK_OA_SendingForwarderAddress = ZGuid.Empty;
					fConsol.JK_OA_ReceivingForwarderAddress = ZGuid.Empty;
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
				}

				return fShipment2;
			}
		}

		ProfitShareMessageExporter fExporter;
		JASForwardingConsol fConsol;
		JASForwardingShipment fShipment1;
		JASForwardingShipment fShipment2;
		#endregion
	}
}
