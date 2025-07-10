using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Client.OSP.Data_Import.Testing
{
	public class IFTMIN5MConsolidationManifestInformationDataRowTest : TestCase
	{
		public void TestProperties()
		{
			string testString = "ICMMI08910641001       20080519CAU   NM117S  ITLSP 040   NZAKL 2008052920080603";
			IFTMIN5MConsolidationManifestInformationDataRow dataRow = new IFTMIN5MConsolidationManifestInformationDataRow(testString);
			AssertEquals("MI08910641001", dataRow.ManifestNumber);
			AssertEquals(new ZDateTime(2008, 5, 19), dataRow.ManifestDate);
			AssertEquals("CAU", dataRow.CodeVessel);
			AssertEquals("NM117S", dataRow.VojageNumber);
			AssertEquals("ITLSP", dataRow.DeparturePortCode);
			AssertEquals("040", dataRow.SealineCode);
			AssertEquals("NZAKL", dataRow.ArrivePortCode);
			AssertEquals(new ZDateTime(2008, 5, 29), dataRow.ETD);
			AssertEquals(new ZDateTime(2008, 6, 3), dataRow.ETA);
		}
	}
}
