using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentVisualizer.FlexCelIntegration.Testing
{
	sealed class FlexCelDrawingsCollectionTest : TestCase
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGetDrawingOutOfRange()
		{
			var worksheet = TestFiles.CreateWorksheet();
			var drawings = new FlexCelDrawingsCollection(worksheet);

			AssertNull("drawing at -1", drawings.GetAt(-1));
			AssertNull("drawing at 0", drawings.GetAt(0));
			AssertNull("drawing at *", drawings.GetAt(worksheet.RowCount + 1));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestRowCached()
		{
			var worksheet = TestFiles.CreateWorksheet();
			var drawings = new FlexCelDrawingsCollection(worksheet);

			var drawing = drawings.GetAt(1);

			AssertNotNull("non empty drawing", drawing);

			AssertEquals("drawing was cached", drawing, drawings.GetAt(1));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCount()
		{
			var worksheet = TestFiles.CreateWorksheet();
			var drawings = new FlexCelDrawingsCollection(worksheet);

			AssertEquals("drawing count", worksheet.ImageCount, drawings.Count);
		}
	}
}
