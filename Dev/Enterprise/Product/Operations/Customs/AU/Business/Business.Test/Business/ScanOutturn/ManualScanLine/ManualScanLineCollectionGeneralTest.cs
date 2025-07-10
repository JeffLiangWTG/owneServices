using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class ManualScanLineCollectionGeneralTest : TestCaseWithFactory
	{
		public void TestManualScanLineCollectionOnAddOnDeleteContainsBarcode()
		{
			ManualScanLineCollection collection = new ManualScanLineCollection(Factory);
			collection.Add(new ManualScanLine() { Time = ZDateTime.Now, Barcode = "12345", Instruction = "Held" });
			var line = new ManualScanLine() { Time = ZDateTime.Now, Barcode = "123456", Instruction = "Held" };
			collection.Add(line);
			Assert(collection.ContainsBarcode("123456"));
			Assert(!collection.ContainsBarcode("1234567"));
			collection.Remove(line);
			Assert(collection.ContainsBarcode("12345"));
			Assert(!collection.ContainsBarcode("123456"));
		}

		public void TestCountNumberOfManualScansByBarcode()
		{
			ManualScanLineCollection collection = new ManualScanLineCollection(Factory) {
				(new ManualScanLine() { Time = ZDateTime.Now, Barcode = "1", Instruction = "Held" }),
				(new ManualScanLine() { Time = ZDateTime.Now, Barcode = "2", Instruction = "Held" }),
				(new ManualScanLine() { Time = ZDateTime.Now, Barcode = "2", Instruction = "Held" })
			};

			AssertEquals(1, collection.CountNumberOfManualScansByBarcode("1"));
			AssertEquals(2, collection.CountNumberOfManualScansByBarcode("2"));
			AssertEquals(0, collection.CountNumberOfManualScansByBarcode("3"));
			AssertEquals(0, collection.CountNumberOfManualScansByBarcode(""));
		}
	}
}
