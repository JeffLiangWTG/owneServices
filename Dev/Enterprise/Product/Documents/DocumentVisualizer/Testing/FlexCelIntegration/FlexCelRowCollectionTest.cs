using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentVisualizer.FlexCelIntegration.Testing
{
	sealed class FlexCelRowCollectionTest : TestCase
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGetRowOutOfRange()
		{
			var worksheet = TestFiles.CreateWorksheet();
			var rows = new FlexCelRowCollection(worksheet);

			AssertNull("row at -1", rows.GetAt(-1));
			AssertNull("row at 0", rows.GetAt(0));
			AssertNull("row at *", rows.GetAt(worksheet.RowCount + 1));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestRowCached()
		{
			var worksheet = TestFiles.CreateWorksheet();
			var rows = new FlexCelRowCollection(worksheet);

			var row = rows.GetAt(1);

			AssertNotNull("non empty row", row);

			AssertEquals("row was cached", row, rows.GetAt(1));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCount()
		{
			var worksheet = TestFiles.CreateWorksheet();
			var rows = new FlexCelRowCollection(worksheet);

			AssertEquals("row count", worksheet.RowCount, rows.Count);
		}
	}
}
