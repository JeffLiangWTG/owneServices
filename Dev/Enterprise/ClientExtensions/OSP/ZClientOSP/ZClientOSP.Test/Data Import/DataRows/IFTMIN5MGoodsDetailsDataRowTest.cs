using NUnit.Framework;

namespace Enterprise.Client.OSP.Data_Import.Testing
{
	public class IFTMIN5MGoodsDetailsDataRowTest : TestCase
	{
		public void TestProperties()
		{
			string testString = "GDD001PO # 015117         0000004CARTONS   TEXTILES                                                                        000006740000000970ADR     CLAS3333LETX000";
			IFTMIN5MGoodsDetailsDataRow dataRow = new IFTMIN5MGoodsDetailsDataRow(testString);
			AssertEquals(1, dataRow.LineCounter);
			AssertEquals("PO # 015117", dataRow.MarksAndNumbers);
			AssertEquals(4, dataRow.NumberOfPackages);
			AssertEquals("CARTONS", dataRow.PackagingDescription);
			AssertEquals("TEXTILES", dataRow.GoodsDetailsDescription1);
			AssertEquals("", dataRow.GoodsDetailsDescription2);
			AssertEquals(67400m, dataRow.GrossWeightKG);
			AssertEquals(970m, dataRow.Volume);
			AssertEquals("ADR", dataRow.ADRNumber);
			AssertEquals("CLAS", dataRow.ADRClass);
			AssertEquals("3333", dataRow.ADRDigit);
			AssertEquals("LETX", dataRow.ADRLetter);
			AssertEquals(0, dataRow.ContainerProgr);
		}
	}
}
