using NUnit.Framework;

namespace Enterprise.Client.OSP.Data_Import.Testing
{
	public class IFTMIN5MGoodsDataRowTest : TestCase
	{
		public void TestProperties()
		{
			string testString = "GD 001999   TEXTILES                           CR    00000040000067400000000000000000009700000000      00000022001FF    PO # 015117         USD   0000000000463";
			IFTMIN5MGoodsDataRow dataRow = new IFTMIN5MGoodsDataRow(testString);
			AssertEquals("999", dataRow.GoodsCode);
			AssertEquals("TEXTILES", dataRow.GoodsDescription);
			AssertEquals("CR", dataRow.PackageTypeCode);
			AssertEquals(4m, dataRow.NumberOfPackages);
			AssertEquals(67400m, dataRow.GrossWeightKG);
			AssertEquals(0m, dataRow.NetWeightKG);
			AssertEquals(970m, dataRow.Volume);
			AssertEquals(0m, dataRow.LenghtMetres);
			AssertEquals("", dataRow.AdditionalMeasureUnitCode);
			AssertEquals(22m, dataRow.AdditionalQuantity);
			AssertEquals(1, dataRow.NumberOfPallets);
			AssertEquals("FF", dataRow.TypeOfPalletCode);
			AssertEquals("PO # 015117", dataRow.MarksAndNumbers);
			AssertEquals("USD", dataRow.GoodsValueCurrencyCode);
			AssertEquals(463m, dataRow.GoodsValueAmount);
		}
	}
}
