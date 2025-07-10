using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentVisualizer.FlexCelIntegration.Testing
{
	sealed class FlexCelColumnCollectionTest : TestCase
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGetColumnOutOfRange()
		{
			var worksheet = TestFiles.CreateWorksheet();
			var columns = new FlexCelColumnCollection(worksheet);

			AssertNull("column at -1", columns.GetAt(-1));
			AssertNull("column at 0", columns.GetAt(0));
			AssertNull("column at *", columns.GetAt(worksheet.ColCount + 1));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestColumnsCached()
		{
			var worksheet = TestFiles.CreateWorksheet();
			var columns = new FlexCelColumnCollection(worksheet);

			var column = columns.GetAt(1);

			AssertNotNull("non empty column", column);

			AssertEquals("column was cached", column, columns.GetAt(1));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCount()
		{
			var worksheet = TestFiles.CreateWorksheet();
			var columns = new FlexCelColumnCollection(worksheet);

			AssertEquals("column count", worksheet.ColCount, columns.Count);
		}
	}
}
