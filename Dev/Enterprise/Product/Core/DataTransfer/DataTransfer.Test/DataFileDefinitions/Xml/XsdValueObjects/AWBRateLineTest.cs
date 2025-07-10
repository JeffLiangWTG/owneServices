using Enterprise.DataTransfer.Xml.Testing;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.Xml.XsdVersion1.Testing
{
	[TestedType(typeof(AWBRateLine))]
	sealed class AWBRateLineTest : ValueObjectTestCase
	{
		public void TestIsSpecified_UpdatedIfNewFieldsAdded()
		{
			AssertEquals(
				"If the number of properties changes, you should update IsSpecified",
				30, typeof(AWBRateLine).GetProperties().Length);
		}

		public void TestIsSpecified()
		{
			Xsd.AWBRateLine awbRateLine = new Xsd.AWBRateLine();
			AssertEquals("Should not be specified by default", false, awbRateLine.IsSpecified);

			//number of pieces
			awbRateLine.NoOfPiecesOrRCP = "123";
			AssertEquals("isSpecified should be true as NoOfPiecesOrRCP != 0", true, awbRateLine.IsSpecified);

			awbRateLine.NoOfPiecesOrRCP = "0";
			AssertEquals("isSpecified should be false as NoOfPiecesOrRCP = '0'", false, awbRateLine.IsSpecified);

			awbRateLine.NoOfPiecesOrRCP = "";
			AssertEquals("isSpecified should be false as NoOfPiecesOrRCP = ''", false, awbRateLine.IsSpecified);

			awbRateLine.NoOfPiecesOrRCP = "a2b";
			AssertEquals("isSpecified should be true as NoOfPiecesOrRCP !=''", true, awbRateLine.IsSpecified);

			awbRateLine.NoOfPiecesOrRCP = "";
			awbRateLine.CommodityItem = "abc";
			AssertEquals("isSpecified should be true as CommodityItem is not empty", true, awbRateLine.IsSpecified);

			awbRateLine.CommodityItem = "";
			awbRateLine.RateClass = "Ab";
			AssertEquals("isSpecified should be true as rate class is not empty", true, awbRateLine.IsSpecified);

			awbRateLine.RateClass = "";
			awbRateLine.CharageableWeight.Value = 23.43m;
			AssertEquals("isSpecified should be true as CharageableWeight !=0", true, awbRateLine.IsSpecified);

			awbRateLine.CharageableWeight.Value = 0m;
			awbRateLine.GrossWeight.Value = 23.43m;
			AssertEquals("isSpecified should be true as GrossWeight !=0", true, awbRateLine.IsSpecified);

			awbRateLine.GrossWeight.Value = 0m;
			awbRateLine.RateOrCharge = 23m;
			AssertEquals("isSpecified should be true as RateOrCharge !=0", true, awbRateLine.IsSpecified);
		}
	}
}
