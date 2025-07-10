using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentScanning.DataTransfer.Universal;

namespace Enterprise.DocumentScanning.DataTransfer.Test.Universal.DocumentRequest.DocumentFilters
{
	public class DocumentFilterDateTimeTest : TestCaseWithFactory
	{
		public void TestAddFilterValue()
		{
			var filter = new DocumentFilterDateTime(DocumentFilterDateTime.ComparisonOption.Equal);
			Assert(filter.Value.IsEmpty);

			filter.AddFilterValue("2016-07-14T09:30");
			AssertEquals(new ZDateTime(2016, 7, 14, 9, 30, 0), filter.Value);
			filter.AddFilterValue("2016-07-15T09:30");
			AssertEquals(new ZDateTime(2016, 7, 14, 9, 30, 0), filter.Value);
			filter.AddFilterValue("2016-07-13T09:30");
			AssertEquals(new ZDateTime(2016, 7, 14, 9, 30, 0), filter.Value);

			filter = new DocumentFilterDateTime(DocumentFilterDateTime.ComparisonOption.From);
			Assert(filter.Value.IsEmpty);

			filter.AddFilterValue("2016-07-14T09:30");
			AssertEquals(new ZDateTime(2016, 7, 14, 9, 30, 0), filter.Value);
			filter.AddFilterValue("2016-07-15T09:30");
			AssertEquals("Should update filter date with more restricted value", new ZDateTime(2016, 7, 15, 9, 30, 0), filter.Value);
			filter.AddFilterValue("2016-07-13T09:30");
			AssertEquals(new ZDateTime(2016, 7, 15, 9, 30, 0), filter.Value);

			filter = new DocumentFilterDateTime(DocumentFilterDateTime.ComparisonOption.To);
			Assert(filter.Value.IsEmpty);

			filter.AddFilterValue("2016-07-14T09:30");
			AssertEquals(new ZDateTime(2016, 7, 14, 9, 30, 0), filter.Value);
			filter.AddFilterValue("2016-07-15T09:30");
			AssertEquals(new ZDateTime(2016, 7, 14, 9, 30, 0), filter.Value);
			filter.AddFilterValue("2016-07-13T09:30");
			AssertEquals("Should update filter date with more restricted value", new ZDateTime(2016, 7, 13, 9, 30, 0), filter.Value);
		}

		public void TestIsMatchEqual()
		{
			var eDoc1 = new eDocForTest { LastEdited = new ZDateTime(2016, 7, 13, 9, 30, 0) };
			var eDoc2 = new eDocForTest { LastEdited = new ZDateTime(2016, 7, 14, 9, 30, 0) };
			var eDoc3 = new eDocForTest { LastEdited = new ZDateTime(2016, 7, 15, 9, 30, 0) };

			var filter = new DocumentFilterDateTime(DocumentFilterDateTime.ComparisonOption.Equal);
			Assert(filter.IsMatch(eDoc1));
			Assert(filter.IsMatch(eDoc2));
			Assert(filter.IsMatch(eDoc3));

			filter.AddFilterValue("2016-07-14T09:30");
			Assert(!filter.IsMatch(eDoc1));
			Assert(filter.IsMatch(eDoc2));
			Assert(!filter.IsMatch(eDoc3));

			filter = new DocumentFilterDateTime(DocumentFilterDateTime.ComparisonOption.From);
			Assert(filter.IsMatch(eDoc1));
			Assert(filter.IsMatch(eDoc2));
			Assert(filter.IsMatch(eDoc3));

			filter.AddFilterValue("2016-07-14T09:30");
			Assert(!filter.IsMatch(eDoc1));
			Assert(filter.IsMatch(eDoc2));
			Assert(filter.IsMatch(eDoc3));

			filter = new DocumentFilterDateTime(DocumentFilterDateTime.ComparisonOption.To);
			Assert(filter.IsMatch(eDoc1));
			Assert(filter.IsMatch(eDoc2));
			Assert(filter.IsMatch(eDoc3));

			filter.AddFilterValue("2016-07-14T09:30");
			Assert(filter.IsMatch(eDoc1));
			Assert(filter.IsMatch(eDoc2));
			Assert(!filter.IsMatch(eDoc3));
		}
	}
}
