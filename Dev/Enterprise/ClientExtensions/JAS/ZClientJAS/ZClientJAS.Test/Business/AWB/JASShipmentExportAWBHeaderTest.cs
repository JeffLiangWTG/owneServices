using CargoWise.Types;
using Enterprise.Client.JAS.Business.JXC;
using Enterprise.Client.JAS.Business.JXC.Import;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Business.AWB;
using Enterprise.Freight.Forwarding.Business.AWB.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.JAS.Business.AWB.Testing
{
	[TestedType(typeof(JASShipmentExportAWBHeader))]
	internal class JASShipmentExportAWBHeaderTest : ShipmentExportAWBHeaderTest
	{
		public void TestRightTypeIsReturnedFromParentBizO()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals(typeof(JASShipmentExportAWBHeader), shipment.AWBHeader.GetType());
		}

		public void TestShipper()
		{
			Shipment.ConsignorPK = Factory.New(typeof(OrgHeader)).PK;
			IJASExportAWBHeader jASAWBHeader = AWBHeader;
			AssertEquals(Shipment.Consignor.PK, jASAWBHeader.Shipper.PK);
		}

		public void TestConsignee()
		{
			Shipment.ConsigneePK = Factory.New(typeof(OrgHeader)).PK;
			IJASExportAWBHeader jASAWBHeader = AWBHeader;
			AssertEquals(Shipment.Consignee.PK, jASAWBHeader.Consignee.PK);
		}

		public void TestShipperAccountForJXC()
		{
			IJASExportAWBHeader jASAWBHeader = AWBHeader;
			AWBHeader.EH_ShipperAccount = "";
			Shipment.ConsignorPK = ZGuid.Empty;
			AssertEquals("No shipper specified yet", JXCConstants.NotAvailable, jASAWBHeader.ShipperAccountForJXC);
			AWBHeader.EH_ShipperAccount = "TEST";
			AssertEquals("TEST", jASAWBHeader.ShipperAccountForJXC);
			AWBHeader.EH_ShipperAccount = "";
			Shipment.ConsignorPK = Factory.New(typeof(JASOrgHeader)).PK;
			Shipment.Consignor.OH_Code = "HAHA";
			AssertEquals("HAHA", jASAWBHeader.ShipperAccountForJXC);
		}

		public void TestConsigneeAccountForJXC()
		{
			IJASExportAWBHeader jASAWBHeader = AWBHeader;
			AWBHeader.EH_ConsigneeAccount = "";
			Shipment.ConsigneePK = ZGuid.Empty;
			AssertEquals("No Consignee specified yet", JXCConstants.NotAvailable, jASAWBHeader.ConsigneeAccountForJXC);
			AWBHeader.EH_ConsigneeAccount = "TEST";
			AssertEquals("TEST", jASAWBHeader.ConsigneeAccountForJXC);
			AWBHeader.EH_ConsigneeAccount = "";
			Shipment.ConsigneePK = Factory.New(typeof(JASOrgHeader)).PK;
			Shipment.Consignee.OH_Code = "HAHA";
			AssertEquals("HAHA", jASAWBHeader.ConsigneeAccountForJXC);
		}

		public void TestCreateChargesData_NullParam()
		{
			AWBHeader.EH_Currency = "IDR";
			AWBHeader.AWBRateLines[0].ER_Total = 100m;
			AWBHeader.AWBRateLines[1].ER_Total = 200m;
			ExportAWBOtherCharges otherCharge1 = AWBHeader.AWBOtherCharges.AddNew();
			AccChargeCode accChargeCode = Factory.New<AccChargeCode>();
			accChargeCode.AC_Code = "XXX";
			accChargeCode.AC_IATA_ChargeCodeMap = "AA";
			otherCharge1.EO_ChargeCode = "AA";
			otherCharge1.EO_ChargeDescription = "foooo";
			otherCharge1.EO_Amount = 200m;
			ExportAWBOtherCharges otherCharge2 = AWBHeader.AWBOtherCharges.AddNew();
			otherCharge2.EO_ChargeCode = "BB";
			otherCharge2.EO_ChargeDescription = "LALALA";
			otherCharge2.EO_Amount = 300m;
			AWBHeader.EH_OtherPPDCOL = ExportAWBHeader.Constants.PrepaidCollect3CharCodes.Collect;
			Shipment.JS_INCO = Core.Constants.IncoTerms.FreeOnBoard;
			IJobChargeData[] chargesData = AWBHeader.CreateChargesData(null);
			AssertEquals("Should return empty array if branch is not specified", 0, chargesData.Length);
		}

		public void TestCreateChargesData_BelongsToDifferentCompany()
		{
			GlbCompany otherCompany = Factory.LoadFromNaturalKey<GlbCompany>(GlbCompanySchema.GC_Code, "DEM");
			Assert("Sanity check", otherCompany.PK != GlbCompany.CurrentCompany.PK);
			GlbBranch otherBranch = otherCompany.Branches[0];
			AWBHeader.EH_Currency = "IDR";
			AWBHeader.AWBRateLines[0].ER_Total = 100m;
			AWBHeader.AWBRateLines[1].ER_Total = 200m;
			AccChargeCode freightChargeCode = Factory.Load<AccChargeCode>(Env.Registry.GetFreightChargeCode(otherCompany.PK.ToGuid()));
			freightChargeCode.AC_Code = "DOM";
			freightChargeCode.AC_Desc = "Domestic freight";
			ExportAWBOtherCharges otherCharge1 = AWBHeader.AWBOtherCharges.AddNew();
			AccChargeCode accChargeCode = Factory.New<AccChargeCode>();
			accChargeCode.AC_Code = "XXX";
			accChargeCode.AC_IATA_ChargeCodeMap = "AA";
			otherCharge1.EO_ChargeCode = "AA";
			otherCharge1.EO_ChargeDescription = "foooo";
			otherCharge1.EO_Amount = 200m;
			ExportAWBOtherCharges otherCharge2 = AWBHeader.AWBOtherCharges.AddNew();
			otherCharge2.EO_ChargeCode = "BB";
			otherCharge2.EO_ChargeDescription = "LALALA";
			otherCharge2.EO_Amount = 300m;
			AWBHeader.EH_OtherPPDCOL = ExportAWBHeader.Constants.PrepaidCollect3CharCodes.Collect;
			Shipment.JS_INCO = Core.Constants.IncoTerms.FreeOnBoard;
			IJobChargeData[] chargesData = AWBHeader.CreateChargesData(otherBranch);
			AssertEquals(3, chargesData.Length);
			AssertJobChargeData(chargesData[0], "DOM", "Domestic freight", "IDR", 300m);
			AssertJobChargeData(chargesData[1], "AA", "foooo", "IDR", 200m);
			AssertJobChargeData(chargesData[2], "BB", "LALALA", "IDR", 300m);
		}

		public void TestCreateChargesData()
		{
			AWBHeader.EH_Currency = "IDR";
			AWBHeader.AWBRateLines[0].ER_Total = 100m;
			AWBHeader.AWBRateLines[1].ER_Total = 200m;
			ExportAWBOtherCharges otherCharge1 = AWBHeader.AWBOtherCharges.AddNew();
			AccChargeCode accChargeCode = Factory.New<AccChargeCode>();
			accChargeCode.AC_Code = "XXX";
			accChargeCode.AC_IATA_ChargeCodeMap = "AA";
			otherCharge1.EO_ChargeCode = "AA";
			otherCharge1.EO_ChargeDescription = "foooo";
			otherCharge1.EO_Amount = 200m;
			ExportAWBOtherCharges otherCharge2 = AWBHeader.AWBOtherCharges.AddNew();
			otherCharge2.EO_ChargeCode = "BB";
			otherCharge2.EO_ChargeDescription = "LALALA";
			otherCharge2.EO_Amount = 300m;
			AWBHeader.EH_OtherPPDCOL = ExportAWBHeader.Constants.PrepaidCollect3CharCodes.Collect;
			Shipment.JS_INCO = Core.Constants.IncoTerms.FreeOnBoard;
			IJobChargeData[] chargesData = AWBHeader.CreateChargesData(GlbBranch.CurrentBranch);
			AssertEquals(3, chargesData.Length);
			AssertJobChargeData(chargesData[0], "FRT", "International Freight", "IDR", 300m);
			AssertJobChargeData(chargesData[1], "XXX", "foooo", "IDR", 200m);
			AssertJobChargeData(chargesData[2], "BB", "LALALA", "IDR", 300m);
		}

		public void TestCreateChargesData_PrepaidChargesShouldNotBeIncluded()
		{
			AWBHeader.EH_Currency = "IDR";
			AWBHeader.AWBRateLines[0].ER_Total = 100m;
			AWBHeader.AWBRateLines[1].ER_Total = 200m;
			ExportAWBOtherCharges otherCharge1 = AWBHeader.AWBOtherCharges.AddNew();
			otherCharge1.EO_ChargeCode = "AA";
			otherCharge1.EO_ChargeDescription = "foooo";
			otherCharge1.EO_Amount = 200m;
			ExportAWBOtherCharges otherCharge2 = AWBHeader.AWBOtherCharges.AddNew();
			otherCharge2.EO_ChargeCode = "BB";
			otherCharge2.EO_ChargeDescription = "LALALA";
			otherCharge2.EO_Amount = 300m;
			AWBHeader.EH_OtherPPDCOL = ExportAWBHeader.Constants.PrepaidCollect3CharCodes.Prepaid;
			Shipment.JS_INCO = Core.Constants.IncoTerms.CostAndFreight;
			IJobChargeData[] chargesData = AWBHeader.CreateChargesData(GlbBranch.CurrentBranch);
			AssertEquals("There should not be any IJobChargeData created as the related charges are all prepaid", 0, chargesData.Length);
		}

		public void TestCreateChargesData_OnlyCollectOtherChargesShouldBeIncluded()
		{
			AWBHeader.EH_Currency = "IDR";
			AWBHeader.AWBRateLines[0].ER_Total = 100m;
			AWBHeader.AWBRateLines[1].ER_Total = 200m;
			ExportAWBOtherCharges otherCharge1 = AWBHeader.AWBOtherCharges.AddNew();
			AccChargeCode accChargeCode = Factory.New<AccChargeCode>();
			accChargeCode.AC_Code = "XXX";
			accChargeCode.AC_IATA_ChargeCodeMap = "AA";
			otherCharge1.EO_ChargeCode = "AA";
			otherCharge1.EO_ChargeDescription = "foooo";
			otherCharge1.EO_Amount = 200m;
			otherCharge1.EO_PPDCLT = ExportAWBHeader.Constants.PrepaidCollect3CharCodes.Prepaid;
			ExportAWBOtherCharges otherCharge2 = AWBHeader.AWBOtherCharges.AddNew();
			otherCharge2.EO_ChargeCode = "BB";
			otherCharge2.EO_ChargeDescription = "LALALA";
			otherCharge2.EO_Amount = 300m;
			otherCharge2.EO_PPDCLT = ExportAWBHeader.Constants.PrepaidCollect3CharCodes.Collect;
			AWBHeader.EH_OtherPPDCOL = ShipmentExportAWBHeader.Constants.PrepaidCollect3CharCodes.Both;
			Shipment.JS_INCO = Core.Constants.IncoTerms.FreeOnBoard;
			IJobChargeData[] chargesData = AWBHeader.CreateChargesData(GlbBranch.CurrentBranch);
			AssertEquals(2, chargesData.Length);
			AssertJobChargeData(chargesData[0], "FRT", "International Freight", "IDR", 300m);
			AssertJobChargeData(chargesData[1], "BB", "LALALA", "IDR", 300m);
		}

		public void TestPopulateEH_HouseCustomsValueCurrency()
		{
			AWBHeader.EH_HouseDeclaredValueCurrency = "";
			AssertEquals("Pre-condition", "", AWBHeader.EH_HouseDeclaredValueCurrency);
			AWBHeader.Populate();
			AssertEquals("Declared Value Currency should be populated using the current company's currency code", GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, AWBHeader.EH_HouseDeclaredValueCurrency);
		}

		public void TestAWBRateLines()
		{
			AssertEquals(typeof(JASShipmentExportAWBRateLineCollection), AWBHeader.AWBRateLines.GetType());
		}

		#region Implementation
		void AssertJobChargeData(IJobChargeData jobChargeData, ZString expectedChargeCode, ZString expectedChargeDesc, ZString expectedCurrency, ZDecimal expectedAmount)
		{
			AssertEquals(expectedChargeCode, jobChargeData.ChargeCode);
			AssertEquals(expectedChargeDesc, jobChargeData.ChargeDescription);
			AssertEquals(expectedCurrency, jobChargeData.Currency);
			AssertEquals(expectedAmount, jobChargeData.ChargeAmount);
		}

		JASForwardingShipment Shipment
		{
			get
			{
				if (fShipment == null)
				{
					fShipment = Factory.New<JASForwardingShipment>();
					fShipment.JS_TransportMode = Core.Constants.TransportModes.Air;
				}

				return fShipment;
			}
		}

		JASShipmentExportAWBHeader AWBHeader
		{
			get
			{
				if (fAWBHeader == null)
				{
					fAWBHeader = (JASShipmentExportAWBHeader)Shipment.AWBHeader;
				}

				return fAWBHeader;
			}
		}

		JASForwardingShipment fShipment;
		JASShipmentExportAWBHeader fAWBHeader;
		#endregion
	}
}
