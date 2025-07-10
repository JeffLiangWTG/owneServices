using NUnit.Framework;

namespace Enterprise.ExcelComparator.Testing
{
	sealed class ComparableSectionTextGeneratorTest : TestCase
	{
		public void TestRunOptionOnWorkSheetContents()
		{
			var option = new CompareSectionsOption();
			var original = @"
[#ConfigurableSection:XXX, A]
{A}-[Text 1] {B}-[Text 1b]
[#ConfigurableSection:XXX, C]
{A}-[Text 3] {B}-[=A5]
{A}-[=A4]
[#ConfigurableSection:YYY, B]
{A}-[=IF(FALSE, A7, B7)]
".Trim();

			AssertEquals("option.GetModifiedWorkSheetContents(original)", @"
A --- Section Start --------------------------
 {A}-[Text 1] {B}-[Text 1b]
A --- Section End --------------------------
B --- Section Start --------------------------
 {A}-[=IF(FALSE, A1, B1)]
B --- Section End --------------------------
C --- Section Start --------------------------
 {A}-[Text 3] {B}-[=A2]
 {A}-[=A1]
C --- Section End --------------------------
".Trim(), option.RunOptionOnWorkSheetContents(original, 1).Trim());
		}
	}
}
