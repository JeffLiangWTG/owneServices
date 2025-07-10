using NUnit.Framework;

namespace Enterprise.Client.WCB.DaimlerChrysler.Testing
{
	public class DecInvoiceHeaderDataRowTest : TestCase
	{
		public static void TestProperties()
		{
			DecInvoiceHeaderDataRow row = new DecInvoiceHeaderDataRow();
			AssertEquals("Row should have 8 fields", 8, row.FieldCount);
			row.RecordType = "D1";
			row.VesselCode = "MAN003";
			row.VesselName = "MANANON";
			row.Voyage = "SF223344";
			row.RegionalAllocation = "987564";
			row.CountOfVehicles = 10;
			row.FOBAmount = 6.40m;
			row.OceanBill = "OBL1234567";
			AssertEquals("RecordType", "D1", row.RecordType);
			AssertEquals("VesselCode", "MAN003", row.VesselCode);
			AssertEquals("VesselName", "MANANON", row.VesselName);
			AssertEquals("Voyage", "SF2233", row.Voyage);
			AssertEquals("RegionalAllocation", "987564", row.RegionalAllocation);
			AssertEquals("CountOfVehicles", 10, row.CountOfVehicles);
			AssertEquals("FOBAmount", 6.40m, row.FOBAmount);
			AssertEquals("OceanBill", "OBL1234567", row.OceanBill);
		}
	}
}
