using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentScanning.DataTransfer.Universal;

namespace Enterprise.DocumentScanning.DataTransfer.Test.Universal.DocumentRequest.DocumentFilters
{
	public class DocumentFilterCodeTest : TestCaseWithFactory
	{
		public void TestAddFilterValue()
		{
			var filter = new DocumentFilterCode(eDoc => eDoc.DocType);
			AssertEquals(0, filter.Values.Count);

			filter.AddFilterValue("aaa");
			AssertEquals(1, filter.Values.Count);
			AssertEquals("AAA", filter.Values[0]);

			filter.AddFilterValue("AAA");
			AssertEquals(1, filter.Values.Count);
			AssertEquals("AAA", filter.Values[0]);

			filter.AddFilterValue("Aaa");
			AssertEquals(1, filter.Values.Count);
			AssertEquals("AAA", filter.Values[0]);

			filter.AddFilterValue("BBB");
			AssertEquals(2, filter.Values.Count);
			AssertEquals("AAA", filter.Values[0]);
			AssertEquals("BBB", filter.Values[1]);
		}

		public void TestIsMatch()
		{
			var eDoc1 = new eDocForTest { DocType = "AAA" };
			var eDoc2 = new eDocForTest { DocType = "BBB" };
			var eDoc3 = new eDocForTest { DocType = "CCC" };

			var filter = new DocumentFilterCode(eDoc => eDoc.DocType);
			Assert(filter.IsMatch(eDoc1));
			Assert(filter.IsMatch(eDoc2));
			Assert(filter.IsMatch(eDoc3));

			filter.AddFilterValue("AAA");
			Assert(filter.IsMatch(eDoc1));
			Assert(!filter.IsMatch(eDoc2));
			Assert(!filter.IsMatch(eDoc3));

			filter.AddFilterValue("BBB");
			Assert(filter.IsMatch(eDoc1));
			Assert(filter.IsMatch(eDoc2));
			Assert(!filter.IsMatch(eDoc3));
		}
	}
}
