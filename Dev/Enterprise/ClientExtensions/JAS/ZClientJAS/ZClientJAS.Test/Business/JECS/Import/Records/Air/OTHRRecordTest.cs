using CargoWise.Types;
using Enterprise.Client.JAS.Business.AWB;
using Enterprise.Freight.Forwarding.Business.AWB;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.JAS.Business.JXC.Import.Testing
{
	internal class OTHRRecordTest : JXCRecordTestCase
	{
		[ExpectNoExceptions]
		public void TestUpdateAWBHeader_NullParam()
		{
			OTHRRecord record = (OTHRRecord)RecordFactory.NewRecord("OTHR3100;C;MYC;FRAIS DE FOB/FOB CHARGES;000000018.00;ATA;SOMEOTHER;19.22;AWC;LAST ONE;200");
			record.UpdateAWBHeader(null);
		}

		public void TestUpdateAWBHeader_HAWB()
		{
			JASShipmentExportAWBHeader aWBHeader = Factory.New<JASShipmentExportAWBHeader>();
			aWBHeader.EH_Table = JobShipmentSchema.Constants.TableName;
			OTHRRecord record = (OTHRRecord)RecordFactory.NewRecord("OTHR3100;P;MYC;FRAIS DE FOB/FOB CHARGES;000000018.00;ATA;SOMEOTHER;19.22;AWC;LAST ONE;200");
			AssertEquals("Pre-condition", 0, aWBHeader.AWBOtherCharges.Count);
			record.UpdateAWBHeader(aWBHeader);
			AssertEquals("There should be 3 other charges imported", 3, aWBHeader.AWBOtherCharges.Count);
			AssertAWBOtherCharges(aWBHeader.AWBOtherCharges[0], Core.Constants.AWB.ChargeCodes.MY, "", "FRAIS DE FOB/FOB CHARGES", 18m, ExportAWBHeader.Constants.PrepaidCollect3CharCodes.Prepaid);
			AssertAWBOtherCharges(aWBHeader.AWBOtherCharges[1], Core.Constants.AWB.ChargeCodes.AT, "", "SOMEOTHER", 19.22m, ExportAWBHeader.Constants.PrepaidCollect3CharCodes.Prepaid);
			AssertAWBOtherCharges(aWBHeader.AWBOtherCharges[2], Core.Constants.AWB.ChargeCodes.AW, "", "LAST ONE", 200m, ExportAWBHeader.Constants.PrepaidCollect3CharCodes.Prepaid);
		}

		public void TestUpdateAWBHeader_MAWB()
		{
			JASConsolExportAWBHeader aWBHeader = Factory.New<JASConsolExportAWBHeader>();
			OTHRRecord record = (OTHRRecord)RecordFactory.NewRecord("OTHR3100;P;MYC;SomeDescriptoin;009888.54;ATA;blah;19.22;;;");
			AssertEquals("Pre-condition", 0, aWBHeader.AWBOtherCharges.Count);
			record.UpdateAWBHeader(aWBHeader);
			AssertEquals("There should be 2 other charges imported", 2, aWBHeader.AWBOtherCharges.Count);
			AssertAWBOtherCharges(aWBHeader.AWBOtherCharges[0], Core.Constants.AWB.ChargeCodes.MY, Core.Constants.AWB.EntitlementCode.Carrier, "SomeDescriptoin", 9888.54m, "");
			AssertAWBOtherCharges(aWBHeader.AWBOtherCharges[1], Core.Constants.AWB.ChargeCodes.AT, Core.Constants.AWB.EntitlementCode.Agent, "blah", 19.22m, "");
		}

		public void TestUpdateAWBHeader_ExcessivelyLongStringIsTrimmed()
		{
			JASShipmentExportAWBHeader aWBHeader = Factory.New<JASShipmentExportAWBHeader>();
			aWBHeader.EH_Table = JobShipmentSchema.Constants.TableName;
			OTHRRecord record = (OTHRRecord)RecordFactory.NewRecord("OTHR3100;COLLECT;MYC;" + new ZString('0', 100) + ";000000018.00;ATA;" + new ZString('1', 100) + ";19.22;AWC;LAST ONE;200");
			AssertEquals("Pre-condition", 0, aWBHeader.AWBOtherCharges.Count);
			record.UpdateAWBHeader(aWBHeader);
			AssertEquals("There should be 3 other charges imported", 3, aWBHeader.AWBOtherCharges.Count);
			AssertAWBOtherCharges(aWBHeader.AWBOtherCharges[0], Core.Constants.AWB.ChargeCodes.MY, "", new ZString('0', ExportAWBOtherChargesSchema.EO_ChargeDescription.MaxLength), 18m, ExportAWBHeader.Constants.PrepaidCollect3CharCodes.Collect);
			AssertAWBOtherCharges(aWBHeader.AWBOtherCharges[1], Core.Constants.AWB.ChargeCodes.AT, "", new ZString('1', ExportAWBOtherChargesSchema.EO_ChargeDescription.MaxLength), 19.22m, ExportAWBHeader.Constants.PrepaidCollect3CharCodes.Collect);
			AssertAWBOtherCharges(aWBHeader.AWBOtherCharges[2], Core.Constants.AWB.ChargeCodes.AW, "", "LAST ONE", 200m, "COL");
		}

		protected override JXCRecord GetNewRecord(ZString lineType, ZString lineContent)
		{
			return new OTHRRecord(lineType, lineContent);
		}

		void AssertAWBOtherCharges(ExportAWBOtherCharges aWBOtherCharges, ZString expectedChargeCode, ZString expectedEntitlementCode, ZString expectedDescription, ZDecimal expectedAmount, ZString expectedPPDCLT)
		{
			AssertEquals(expectedChargeCode, aWBOtherCharges.EO_ChargeCode);
			AssertEquals(expectedEntitlementCode, aWBOtherCharges.EO_EntitlementCode);
			AssertEquals(expectedDescription, aWBOtherCharges.EO_ChargeDescription);
			AssertEquals(expectedAmount, aWBOtherCharges.EO_Amount);
			AssertEquals(expectedPPDCLT, aWBOtherCharges.EO_PPDCLT);
		}
	}
}
