using CargoWise.IO;
using NUnit.Framework;

namespace Enterprise.ExcelComparator.Testing
{
	sealed class InsertMissingSectionsComparableTextOptionTest : TestCase
	{
		public void TestRunOptionOnWorkSheetContents()
		{
			using (var resourceRetriever = new EmbeddedResourceRetriever())
			{
				var configurableSectionsInDifferentOrderAndStripContentsXlsPath = resourceRetriever.SaveResourceToFile("Enterprise.ExcelComparator.Testing.TestFiles.FileWithConfigurableSectionsInDifferentOrderAndStripContents.xls", "FileWithConfigurableSectionsInDifferentOrderAndStripContents.xls");
				var option = new InsertMissingSectionsOption(configurableSectionsInDifferentOrderAndStripContentsXlsPath);
				var original = @"
A --- Section Start --------------------------
 {A} Text 1 {B} Text 1b
A --- Section End --------------------------
B --- Section Start --------------------------
 {A} Text 2
B --- Section End --------------------------
C --- Section Start --------------------------
 {A} Text 3 {B} Text 3b
C --- Section End --------------------------
".Trim();

				AssertEquals("option.GetModifiedWorkSheetContents(original)", @"
A --- Section Start --------------------------
 {A} Text 1 {B} Text 1b
A --- Section End --------------------------
B --- Section Start --------------------------
 {A} Text 2
B --- Section End --------------------------
C --- Section Start --------------------------
 {A} Text 3 {B} Text 3b
C --- Section End --------------------------
D --- Section Start --------------------------
  <Section Missing>
D --- Section End --------------------------
".Trim(), option.RunOptionOnWorkSheetContents(original, 1).Trim());
			}
		}
	}
}
