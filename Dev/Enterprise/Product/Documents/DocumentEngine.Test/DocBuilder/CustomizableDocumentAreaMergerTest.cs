using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentEngine.Testing;

namespace Enterprise.DocumentEngine.DocBuilder.Testing
{
	sealed class CustomizableDocumentAreaMergerTest : TestCaseWithFactory
	{
		public void TestMergeEmptyAreaDoesNotCauseException()
		{
			var template = DocumentEngineTestHelper.CreateTemplateFromString(Factory, Name,
@"{A}-[#Config]
{A}-[Name=System Document Element Template]
{A}-[DataContext=GenericFreightJob]
{A}-[EmailSubject=<ReportName> - <JobNumber>]
{A}-[#PageFooter]
{B}-[My Page Footer 1]
{A}-[#LastPageFooter]
{A}-[#PageFooter]
{A}-[#PageHeader]
{A}-[#EndOfReport]");

			using (var excelInterface = new ExcelInterface())
			{
				excelInterface.LoadExcelFile(template.SO_Template);

				var merger = new AreaMerger();
				merger.Merge(excelInterface);

				var workSheet = excelInterface.WorkSheets[0];

				AssertMultilineASCIIEquals("Merging empty areas do not cause exception.",
@"{A}-[#Config]
{A}-[Name=System Document Element Template]
{A}-[DataContext=GenericFreightJob]
{A}-[EmailSubject=<ReportName> - <JobNumber>]
{A}-[#PageHeader]
{A}-[#PageFooter]
{B}-[My Page Footer 1]
{A}-[#LastPageFooter]
{A}-[#EndOfReport]",
					workSheet.ToString());
			}
		}

		public void TestReOrderingWorks()
		{
			var template = DocumentEngineTestHelper.CreateTemplateFromString(Factory, Name,
@"{A}-[#Config]
{A}-[Name=System Document Element Template]
{A}-[HideColumnIf]   {AB}-[1==1]   {AC}-[1==1]   {AD}-[1==1]   {AE}-[1==1]   {AY}-[1==1]   {AZ}-[1==1]
{A}-[DataContext=GenericFreightJob]
{A}-[EmailSubject=<ReportName> - <JobNumber>]
{A}-[#DocumentHeader]
{B}-[1]
{A}-[#LASTPAGEFOOTER]
{B}-[1]
{A}-[#SectionBody]
{B}-[1]
{A}-[#SECTIONPAGEHEADER]
{B}-[1]
{A}-[#SECTIONBODY]
{B}-[1]
{A}-[#PageHeader:StartFromSecondPage]
{B}-[1]
{A}-[#SECTIONHEADER]
{B}-[1]
{A}-[#GROUPBY:anything]
{B}-[1]
{A}-[#SectionBody]
{B}-[1]
{A}-[#SECTIONPAGEFOOTER]
{B}-[1]
{A}-[#SectionBody]
{B}-[1]
{A}-[#SECTIONFOOTER]
{B}-[1]
{A}-[#PAGEFOOTER]
{B}-[1]
{A}-[#FIRSTPAGEFOOTER]
{B}-[1]
{A}-[#ONLYONEPAGEFOOTER]
{B}-[1]
{A}-[#DocumentHeader]
{B}-[2]
{A}-[#LASTPAGEFOOTER]
{B}-[2]
{A}-[#PageHeader:StartFromSecondPage]
{B}-[2]
{A}-[#SectionBody]
{B}-[2]
{A}-[#SECTIONPAGEHEADER]
{B}-[2]
{A}-[#SECTIONBODY]
{B}-[2]
{A}-[#SECTIONHEADER]
{B}-[2]
{A}-[#GROUPBY:anything else]
{B}-[2]
{A}-[#SectionBody]
{B}-[2]
{A}-[#SECTIONPAGEFOOTER]
{B}-[2]
{A}-[#SectionBody]
{B}-[2]
{A}-[#SECTIONFOOTER]
{B}-[2]
{A}-[#PAGEFOOTER]
{B}-[2]
{A}-[#FIRSTPAGEFOOTER]
{B}-[2]
{A}-[#ONLYONEPAGEFOOTER]
{B}-[2]
{A}-[#EndOfReport]");

			using (var excelInterface = new ExcelInterface())
			{
				using (var stream = template.GetExcelTemplate().GetAsTemplateStream())
				{
					excelInterface.LoadExcelFile(stream);
				}

				var merger = new AreaMerger();
				merger.Merge(excelInterface);

				excelInterface.ActiveWorksheet = 0;
				var workSheet = excelInterface.WorkSheets[0];
				AssertMultilineASCIIEquals("merger.Merge()",
@"{A}-[#Config]
{A}-[Name=System Document Element Template]
{A}-[HideColumnIf]   {AB}-[1==1]   {AC}-[1==1]   {AD}-[1==1]   {AE}-[1==1]   {AY}-[1==1]   {AZ}-[1==1]
{A}-[DataContext=GenericFreightJob]
{A}-[EmailSubject=<ReportName> - <JobNumber>]
{A}-[#DocumentHeader]
{B}-[1]
{B}-[2]
{A}-[#PageHeader:StartFromSecondPage]
{B}-[1]
{B}-[2]
{A}-[#SectionBody]
{B}-[1]
{A}-[#SECTIONPAGEHEADER]
{B}-[1]
{A}-[#SECTIONBODY]
{B}-[1]
{A}-[#SECTIONHEADER]
{B}-[1]
{A}-[#GROUPBY:anything]
{B}-[1]
{A}-[#SectionBody]
{B}-[1]
{A}-[#SECTIONPAGEFOOTER]
{B}-[1]
{A}-[#SectionBody]
{B}-[1]
{A}-[#SECTIONFOOTER]
{B}-[1]
{A}-[#SectionBody]
{B}-[2]
{A}-[#SECTIONPAGEHEADER]
{B}-[2]
{A}-[#SECTIONBODY]
{B}-[2]
{A}-[#SECTIONHEADER]
{B}-[2]
{A}-[#GROUPBY:anything else]
{B}-[2]
{A}-[#SectionBody]
{B}-[2]
{A}-[#SECTIONPAGEFOOTER]
{B}-[2]
{A}-[#SectionBody]
{B}-[2]
{A}-[#SECTIONFOOTER]
{B}-[2]
{A}-[#FIRSTPAGEFOOTER]
{B}-[1]
{B}-[2]
{A}-[#PAGEFOOTER]
{B}-[1]
{B}-[2]
{A}-[#ONLYONEPAGEFOOTER]
{B}-[1]
{B}-[2]
{A}-[#LASTPAGEFOOTER]
{B}-[1]
{B}-[2]
{A}-[#EndOfReport]", workSheet.ToString());
			}
		}

		public void TestMergeMultipleDocumentHeadersAndPageHeaders()
		{
			var template = DocumentEngineTestHelper.CreateTemplateFromString(Factory, Name,
@"{A}-[#Config]
{A}-[Name=System Document Element Template]
{A}-[HideColumnIf]   {AB}-[1==1]   {AC}-[1==1]   {AD}-[1==1]   {AE}-[1==1]   {AY}-[1==1]   {AZ}-[1==1]
{A}-[DataContext=GenericFreightJob]
{A}-[EmailSubject=<ReportName> - <JobNumber>]
{A}-[#DocumentHeader]
{C}-[<DateTimeAsString('<Now>', 'dd-MMM-yy HH:mm')>]   {Q}-[<ReportNameShort>]   {AO}-[Page <Current Page> of <TotalPages>]

{A}-[#PageHeader:StartFromSecondPage]
{C}-[<DateTimeAsString('<Now>', 'dd-MMM-yy HH:mm')>]   {Q}-[<ReportNameShort>]   {AO}-[Page <Current Page> of <TotalPages>]

{A}-[#DocumentHeader]
{C}-[<Image(CompanyLogo,1,47)>]

{C}-[<ShrinkToFit><ReportNameShort>]   {AR}-[Page <Current Page> of <TotalPages>]

{G}-[<RecipientNameAndAddress>]   {AG}-[<Upper(""<JobNumberHeading>"")>]   {AO}-[<JobNumber>]

{AG}-[<Upper(""<SecondaryHeading>"")>]   {AO}-[<SecondaryNumber>]

{AG}-[DATE]   {AO}-[<DateTimeAsString('<Now>', 'dd-MMM-yy HH:mm')>]

{A}-[#PageHeader:StartFromSecondPage]

{C}-[<ShrinkToFit><ReportNameShort>]   {AR}-[Page <Current Page> of <TotalPages>]

{G}-[<RecipientNameAndAddress>]   {AG}-[<Upper(""<JobNumberHeading>"")>]   {AO}-[<JobNumber>]

{AG}-[<Upper(""<SecondaryHeading>"")>]   {AO}-[<SecondaryNumber>]

{AG}-[DATE]   {AO}-[<DateTimeAsString('<Now>', 'dd-MMM-yy HH:mm')>]

{A}-[#EndOfReport]");

			using (var excelInterface = new ExcelInterface())
			{
				using (var stream = template.GetExcelTemplate().GetAsTemplateStream())
				{
					excelInterface.LoadExcelFile(stream);
				}

				var merger = new AreaMerger();
				merger.Merge(excelInterface);

				excelInterface.ActiveWorksheet = 0;
				var workSheet = excelInterface.WorkSheets[0];
				AssertMultilineASCIIEquals("merger.Merge()",
@"{A}-[#Config]
{A}-[Name=System Document Element Template]
{A}-[HideColumnIf]   {AB}-[1==1]   {AC}-[1==1]   {AD}-[1==1]   {AE}-[1==1]   {AY}-[1==1]   {AZ}-[1==1]
{A}-[DataContext=GenericFreightJob]
{A}-[EmailSubject=<ReportName> - <JobNumber>]
{A}-[#DocumentHeader]
{C}-[<DateTimeAsString('<Now>', 'dd-MMM-yy HH:mm')>]   {Q}-[<ReportNameShort>]   {AO}-[Page <Current Page> of <TotalPages>]

{C}-[<Image(CompanyLogo,1,47)>]

{C}-[<ShrinkToFit><ReportNameShort>]   {AR}-[Page <Current Page> of <TotalPages>]

{G}-[<RecipientNameAndAddress>]   {AG}-[<Upper(""<JobNumberHeading>"")>]   {AO}-[<JobNumber>]

{AG}-[<Upper(""<SecondaryHeading>"")>]   {AO}-[<SecondaryNumber>]

{AG}-[DATE]   {AO}-[<DateTimeAsString('<Now>', 'dd-MMM-yy HH:mm')>]

{A}-[#PageHeader:StartFromSecondPage]
{C}-[<DateTimeAsString('<Now>', 'dd-MMM-yy HH:mm')>]   {Q}-[<ReportNameShort>]   {AO}-[Page <Current Page> of <TotalPages>]

{C}-[<ShrinkToFit><ReportNameShort>]   {AR}-[Page <Current Page> of <TotalPages>]

{G}-[<RecipientNameAndAddress>]   {AG}-[<Upper(""<JobNumberHeading>"")>]   {AO}-[<JobNumber>]

{AG}-[<Upper(""<SecondaryHeading>"")>]   {AO}-[<SecondaryNumber>]

{AG}-[DATE]   {AO}-[<DateTimeAsString('<Now>', 'dd-MMM-yy HH:mm')>]

{A}-[#EndOfReport]", workSheet.ToString());
			}
		}

		public void TestMerge()
		{
			var template = DocumentEngineTestHelper.CreateTemplateFromString(Factory, Name,
@"{A}-[#Config]
{A}-[Name=TestMerge]
{A}-[DataContext=GenericFreightJob]
{A}-[#DocumentHeader]
{B}-[This is Document Header 1.]

{A}-[#DocumentHeader]
{B}-[This is Document Header 2.]

{A}-[#SectionBody]
{B}-[This is Section Body 1.]

{A}-[#SectionBody]
{B}-[This is Section Body 2.]

{A}-[#DocumentHeader]
{B}-[This is Document Header 3.]

{A}-[#EndOfReport]");

			using (var excelInterface = new ExcelInterface())
			{
				using (var stream = template.GetExcelTemplate().GetAsTemplateStream())
				{
					excelInterface.LoadExcelFile(stream);
				}

				AssertEquals("PreCondition:", 19, excelInterface.WorkSheets[0].RowCount);

				var merger = new AreaMerger();
				merger.Merge(excelInterface);

				excelInterface.ActiveWorksheet = 0;
				var workSheet = excelInterface.WorkSheets[0];

				AssertEquals(17, workSheet.RowCount);
				AssertMultilineASCIIEquals("merger.Merge()",
@"{A}-[#Config]
{A}-[Name=TestMerge]
{A}-[DataContext=GenericFreightJob]
{A}-[#DocumentHeader]
{B}-[This is Document Header 1.]

{B}-[This is Document Header 2.]

{B}-[This is Document Header 3.]

{A}-[#SectionBody]
{B}-[This is Section Body 1.]

{A}-[#SectionBody]
{B}-[This is Section Body 2.]

{A}-[#EndOfReport]", workSheet.ToString());
			}
		}
	}
}
