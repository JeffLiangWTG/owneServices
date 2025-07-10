using Enterprise.Freight.Forwarding.Business.AWB;

namespace Enterprise.Client.JAS.Business.JXC.Export.Testing
{
	internal class OTHRLineTest : MessageLineTestCase
	{
		public void TestLineAsString()
		{
			ExportAWBOtherCharges aWBOtherCharges = ConsolAWBHeader.AWBOtherCharges.AddNew();
			aWBOtherCharges.EO_Amount = 250;
			aWBOtherCharges.EO_ChargeCode = Core.Constants.AWB.ChargeCodes.AC;
			aWBOtherCharges.EO_EntitlementCode = Core.Constants.AWB.EntitlementCode.Carrier;
			aWBOtherCharges.EO_ChargeDescription = "Description";
			ConsolAWBHeader.EH_OtherPPDCOL = Core.Constants.PaymentType.Collect;
			OTHRLine line = new OTHRLine(ConsolAWBHeader, aWBOtherCharges);
			AssertEquals("OTHR3100;C;ACC;Description;250;;;;;;", line.LineAsString);
			aWBOtherCharges = ShipmentAWBHeader.AWBOtherCharges.AddNew();
			aWBOtherCharges.EO_Amount = 111.34m;
			aWBOtherCharges.EO_ChargeCode = Core.Constants.AWB.ChargeCodes.AS;
			aWBOtherCharges.EO_EntitlementCode = Core.Constants.AWB.EntitlementCode.Carrier;
			aWBOtherCharges.EO_ChargeDescription = "TEST";
			ShipmentAWBHeader.EH_OtherPPDCOL = Core.Constants.PaymentType.Prepaid;
			line = new OTHRLine(ShipmentAWBHeader, aWBOtherCharges);
			AssertEquals("OTHR3100;P;ASA;TEST;111.34;;;;;;", line.LineAsString);
			ShipmentAWBHeader.EH_OtherPPDCOL = "BTH";
			aWBOtherCharges.EO_EntitlementCode = "C";
			AssertEquals("OTHR3100;C;ASA;TEST;111.34;;;;;;", line.LineAsString);
			aWBOtherCharges.EO_EntitlementCode = "P";
			AssertEquals("OTHR3100;P;ASA;TEST;111.34;;;;;;", line.LineAsString);
		}

		public void TestChargeDescriptionMaxLength()
		{
			var otherCharges = ConsolAWBHeader.AWBOtherCharges.AddNew();
			otherCharges.EO_Amount = 250;
			otherCharges.EO_ChargeCode = Core.Constants.AWB.ChargeCodes.AC;
			otherCharges.EO_EntitlementCode = Core.Constants.AWB.EntitlementCode.Carrier;
			otherCharges.EO_ChargeDescription = "Charge Description with length exceeding JCX ChargeDescriptionMaxLength";
			ConsolAWBHeader.EH_OtherPPDCOL = Core.Constants.PaymentType.Collect;
			string message = "Charge Description should never exceed the JAS max length, if the schema is changed, then the Charge Description field needs to be trimmed";
			var line = new OTHRLine(ConsolAWBHeader, otherCharges);
			AssertEquals(message, "OTHR3100;C;ACC;Charge Description with length exce;250;;;;;;", line.LineAsString);
		}

		#region Overrides
		protected override int ExpectedFieldCount
		{
			get
			{
				return 10;
			}
		}

		protected override CargoWise.Types.ZString ExpectedLineType
		{
			get
			{
				return "OTHR";
			}
		}

		protected override MessageLine GetMessageLine()
		{
			ExportAWBOtherCharges aWBOtherCharges = ConsolAWBHeader.AWBOtherCharges.AddNew();
			return new OTHRLine(ConsolAWBHeader, aWBOtherCharges);
		}

		#endregion
		#region Implementation
		ConsolExportAWBHeader ConsolAWBHeader
		{
			get
			{
				if (fConsolAWBHeader == null)
				{
					JASForwardingConsol consol = Factory.New<JASForwardingConsol>();
					consol.JK_TransportMode = Core.Constants.TransportModes.Air;
					fConsolAWBHeader = (ConsolExportAWBHeader)consol.AWBHeader;
				}

				return fConsolAWBHeader;
			}
		}

		ShipmentExportAWBHeader ShipmentAWBHeader
		{
			get
			{
				if (fShipmentAWBHeader == null)
				{
					JASForwardingShipment shipment = Factory.New<JASForwardingShipment>();
					shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
					fShipmentAWBHeader = (ShipmentExportAWBHeader)shipment.AWBHeader;
				}

				return fShipmentAWBHeader;
			}
		}

		ConsolExportAWBHeader fConsolAWBHeader;
		ShipmentExportAWBHeader fShipmentAWBHeader;
		#endregion
	}
}
