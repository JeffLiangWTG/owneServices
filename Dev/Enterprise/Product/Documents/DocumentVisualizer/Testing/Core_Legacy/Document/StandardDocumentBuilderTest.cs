using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Macros;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.DocDataObjects.Testing;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.DocumentVisualizer.Presentation;
using Enterprise.DocumentVisualizer.Testing.GUI;
using Enterprise.MasterFiles.Business;
using Moq;
using BorderStyle = Enterprise.DocumentVisualizer.Core.BorderStyle;
using Constants = Enterprise.Core.Constants;
using Res = Enterprise.DocumentVisualizer.Business.Res;
using StandardDocumentBuilder = Enterprise.DocumentVisualizer.Core.StandardDocumentBuilder;

namespace Enterprise.DocumentVisualizer.Testing
{
	sealed class StandardDocumentBuilderTest : TestCaseWithFactory
	{
		#region Base Sections

		#region TestCreateDocument_TemplateWithAllBodySections_Static

		public void TestCreateDocument_TemplateWithAllBodySections_Static()
		{
			var worksheet = DummyWorksheet.Parse(
@"#Config:Name=""Test Document""
#End
#PageHeader
	page header
#End
#Body
#Header
	section header
#End
#PageHeader
	section page header
#End
	<Z0_Description>
#PageFooter
	section page footer
#End
#Footer
	section footer
#End
#End
#PageFooter
	page <PageNumber> footer
#End");

			var template = new StandardTemplate(worksheet);

			var dummy = Factory.New<DummyBusinessObject>();
			dummy.Z0_Description = "some description";

			var documentBuilder = CreateDocumentBuilder(template, dummy);

			var document = documentBuilder.Build();

			AssertMultilineASCIIEquals("template has been parsed with no errors", "",
				string.Join("\r\n", template.Notifications.Select(n => n.Message)));

			var stringBuilder = new DocumentStringBuilder(document);

			AssertMultilineASCIIEquals("expected document",
@"	{A}	{B}
--- Page 1 ---
{1}		{B} - page header
{2}		{B} - section header
{3}		{B} - some description
{4}		{B} - section footer
{5}
{6}
{7}		{B} - page 1 footer",
				stringBuilder.ToString());
		}

		#endregion

		#region TestCreateDocument_TemplateWithAllBodySections_Expandable

		public void TestCreateDocument_TemplateWithAllBodySections_Expandable()
		{
			var worksheet = DummyWorksheet.Parse(
@"#Config:Name=""Test Document""
#End
#PageHeader
	page <PageNumber> header
#End
#Body:Data=Collection
#Header
	section header
#End
#PageHeader
	section page header
#End
	<Z0_Description>
#PageFooter
	section page footer
#End
#Footer
	section footer
#End
#End
#PageFooter
	page <PageNumber> footer
#End");

			var template = new StandardTemplate(worksheet);

			var dummy = Factory.New<DummyBusinessObject>();
			dummy.Z0_Description = "some description";

			var child = dummy.Collection.AddNew();
			child.Z0_Description = "child 1 description";

			child = dummy.Collection.AddNew();
			child.Z0_Description = "child 2 description";

			child = dummy.Collection.AddNew();
			child.Z0_Description = "child 3 description";

			var documentBuilder = CreateDocumentBuilder(template, dummy);

			var document = documentBuilder.Build();

			AssertMultilineASCIIEquals("template has been parsed with no errors", "",
				string.Join("\r\n", template.Notifications.Select(n => n.Message)));

			var stringBuilder = new DocumentStringBuilder(document);

			AssertMultilineASCIIEquals("expected document",
@"	{A}	{B}
--- Page 1 ---
{1}		{B} - page 1 header
{2}		{B} - section header
{3}		{B} - child 1 description
{4}		{B} - child 2 description
{5}		{B} - child 3 description
{6}		{B} - section footer
{7}
{8}
{9}		{B} - page 1 footer",
				stringBuilder.ToString());
		}

		#endregion

		#region TestCreateDocument_Truncate

		public void TestCreateDocument_Truncate()
		{
			var worksheet = DummyWorksheet.Parse(
@"#Config:Name=""Test Document""
#End
#Body:Data=Collection
	<Z0_Description>
#End", 100d);

			var template = new StandardTemplate(worksheet);

			var dummy = Factory.New<DummyBusinessObject>();
			dummy.Z0_Description = "some description";

			for (int i = 0; i < 1000; i++)
			{
				var child = dummy.Collection.AddNew();
				child.Z0_Description = string.Format("child {0} description", i);
			}
			var documentBuilder = CreateDocumentBuilder(template, dummy);

			var document = documentBuilder.Build();

			AssertMultilineASCIIEquals("template has been parsed with no errors", "",
				string.Join("\r\n", template.Notifications.Select(n => n.Message)));

			AssertMultilineASCIIEquals("document notifications",
				"Warning|This document has more than 100 pages and has been truncated.",
				string.Join("\r\n", document.Notifications.Select(n => string.Format("{0}|{1}", n.Type, n.Message))));

			AssertEquals("document pages", 100, document.Pages.Count());
		}

		#endregion

		#region TestCreateDocument_WithTwoBodySections

		public void TestCreateDocument_WithTwoBodySections()
		{
			var worksheet = DummyWorksheet.Parse(
@"#Config:Name=""Test Document""
#End
#PageHeader
	page <PageNumber> header
#End
#Body
	<Z0_Description>
#End
#Body:Data=Collection
	<Z0_Description>
#End
#PageFooter
	page <PageNumber> footer
#End");

			var template = new StandardTemplate(worksheet);

			var dummy = Factory.New<DummyBusinessObject>();
			dummy.Z0_Description = "some description";

			var child = dummy.Collection.AddNew();
			child.Z0_Description = "child 1 description";

			child = dummy.Collection.AddNew();
			child.Z0_Description = "child 2 description";

			child = dummy.Collection.AddNew();
			child.Z0_Description = "child 3 description";

			var documentBuilder = CreateDocumentBuilder(template, dummy);

			var document = documentBuilder.Build();

			AssertMultilineASCIIEquals("template has been parsed with no errors", "",
				string.Join("\r\n", template.Notifications.Select(n => n.Message)));

			var stringBuilder = new DocumentStringBuilder(document);

			AssertMultilineASCIIEquals("expected document",
@"	{A}	{B}
--- Page 1 ---
{1}		{B} - page 1 header
{2}		{B} - some description
{3}		{B} - child 1 description
{4}		{B} - child 2 description
{5}		{B} - child 3 description
{6}
{7}
{8}		{B} - page 1 footer",
				stringBuilder.ToString());
		}

		#endregion

		#region TestCreateDocument_WithTwoEmptyBodySectionsHavingFootersAndHeaders

		public void TestCreateDocument_WithTwoEmptyBodySectionsHavingFootersAndHeaders()
		{
			var worksheet = DummyWorksheet.Parse(
@"#Config:Name=""Test Document""
#End
#Body:Data=Collection
#Header
	collection header
#End
	first section body
#Footer
	collection footer
#End
#End
#Body:Data=FilteredCollection
#Header
	filtered collection header
#End
	second section body
#Footer
	filtered collection footer
#End
#End
#Body
	third body section
#End
#PageFooter
	page <PageNumber> footer
#End");

			var template = new StandardTemplate(worksheet);

			var dummy = Factory.New<DummyBusinessObject>();

			var documentBuilder = CreateDocumentBuilder(template, dummy);

			var document = documentBuilder.Build();

			AssertMultilineASCIIEquals("template has been parsed with no errors", "",
				string.Join("\r\n", template.Notifications.Select(n => n.Message)));

			var stringBuilder = new DocumentStringBuilder(document);

			AssertMultilineASCIIEquals("expected document",
@"	{A}	{B}
--- Page 1 ---
{1}		{B} - third body section
{2}
{3}
{4}		{B} - page 1 footer",
				stringBuilder.ToString());
		}

		#endregion

		#region TestCreateDocument_TemplateWithSectionPageHeader_Static

		public void TestCreateDocument_TemplateWithSectionPageHeader_Static()
		{
			var worksheet = DummyWorksheet.Parse(
@"#Config:Name=""Test Document""
#End
#PageHeader
	page <PageNumber> header
#End
#Body
#PageHeader
	section page header
#End
	<Z0_Description>
#PageFooter
	section page footer
#End
#End
#PageFooter
	page <PageNumber> footer
#End");

			var template = new StandardTemplate(worksheet);

			var dummy = Factory.New<DummyBusinessObject>();
			dummy.Z0_Description = "some description";

			var documentBuilder = CreateDocumentBuilder(template, dummy);

			var document = documentBuilder.Build();

			AssertMultilineASCIIEquals("template has been parsed with no errors", "",
				string.Join("\r\n", template.Notifications.Select(n => n.Message)));

			var stringBuilder = new DocumentStringBuilder(document);

			AssertMultilineASCIIEquals("expected document",
@"	{A}	{B}
--- Page 1 ---
{1}		{B} - page 1 header
{2}		{B} - section page header
{3}		{B} - some description
{4}		{B} - section page footer
{5}
{6}
{7}		{B} - page 1 footer",
				stringBuilder.ToString());
		}

		#endregion

		#region TestCreateDocument_TemplateWithSectionPageHeader_Expandable

		public void TestCreateDocument_TemplateWithSectionPageHeader_Expandable()
		{
			var worksheet = DummyWorksheet.Parse(
@"#Config:Name=""Test Document""
#End
#PageHeader
	page <PageNumber> header
#End
#Body:Data=Collection
#PageHeader
	section page header
#End
	<Z0_Description>
#PageFooter
	section page footer
#End
#End
#PageFooter
	page <PageNumber> footer
#End");

			var template = new StandardTemplate(worksheet);

			var dummy = Factory.New<DummyBusinessObject>();
			dummy.Z0_Description = "some description";

			var child = dummy.Collection.AddNew();
			child.Z0_Description = "child 1 description";

			child = dummy.Collection.AddNew();
			child.Z0_Description = "child 2 description";

			child = dummy.Collection.AddNew();
			child.Z0_Description = "child 3 description";

			var documentBuilder = CreateDocumentBuilder(template, dummy);

			var document = documentBuilder.Build();

			AssertMultilineASCIIEquals("template has been parsed with no errors", "",
				string.Join("\r\n", template.Notifications.Select(n => n.Message)));

			var stringBuilder = new DocumentStringBuilder(document);

			AssertMultilineASCIIEquals("expected document",
@"	{A}	{B}
--- Page 1 ---
{1}		{B} - page 1 header
{2}		{B} - section page header
{3}		{B} - child 1 description
{4}		{B} - child 2 description
{5}		{B} - child 3 description
{6}		{B} - section page footer
{7}
{8}
{9}		{B} - page 1 footer",
				stringBuilder.ToString());
		}

		#endregion

		#region TestCreateDocument_WithPageHeaderAndPageFooterFitOnOnePage

		public void TestCreateDocument_WithPageHeaderAndPageFooterFitOnOnePage()
		{
			var worksheet = DummyWorksheet.Parse(
@"#Config:Name=""Test Document""
#End
#PageHeader
	page <PageNumber> header
#End
#Body:Data=Collection
	<Z0_Description>
#End
#PageFooter
	page <PageNumber> footer
#End", rowHeight: 100);

			var template = new StandardTemplate(worksheet);

			var dummy = Factory.New<DummyBusinessObject>();

			for (int i = 1; i <= 6; i++)
			{
				var child = dummy.Collection.AddNew();
				child.Z0_Description = string.Format("child {0} description", i);
			}

			var documentBuilder = CreateDocumentBuilder(template, dummy);

			var document = documentBuilder.Build();

			AssertMultilineASCIIEquals("template has been parsed with no errors", "",
				string.Join("\r\n", template.Notifications.Select(n => n.Message)));

			var stringBuilder = new DocumentStringBuilder(document);

			AssertMultilineASCIIEquals("expected document",
@"	{A}	{B}
--- Page 1 ---
{1}		{B} - page 1 header
{2}		{B} - child 1 description
{3}		{B} - child 2 description
{4}		{B} - child 3 description
{5}		{B} - child 4 description
{6}		{B} - child 5 description
{7}		{B} - child 6 description
{8}
{9}		{B} - page 1 footer",
				stringBuilder.ToString());
		}

		#endregion

		#region TestCreateDocument_TemplateSectionPageFooter

		public void TestCreateDocument_TemplateSectionPageFooter()
		{
			var worksheet = DummyWorksheet.Parse(
@"#Config:Name=""Test Document""
#End
#PageHeader
	page <PageNumber> header
#End
#Body:Data=Collection
#PageHeader
	section page header
#End
	<Z0_Description>
#PageFooter
	section page footer
#End
#End
#PageFooter
	page <PageNumber> footer
#End");

			var template = new StandardTemplate(worksheet);

			var dummy = Factory.New<DummyBusinessObject>();

			for (int i = 1; i <= 50; i++)
			{
				var child = dummy.Collection.AddNew();
				child.Z0_Description = string.Format("child {0} description", i);
			}

			var documentBuilder = CreateDocumentBuilder(template, dummy);

			var document = documentBuilder.Build();

			AssertMultilineASCIIEquals("template has been parsed with no errors", "",
				string.Join("\r\n", template.Notifications.Select(n => n.Message)));

			var stringBuilder = new DocumentStringBuilder(document);

			AssertMultilineASCIIEquals("expected document",
@"	{A}	{B}
--- Page 1 ---
{1}		{B} - page 1 header
{2}		{B} - section page header
{3}		{B} - child 1 description
{4}		{B} - child 2 description
{5}		{B} - child 3 description
{6}		{B} - child 4 description
{7}		{B} - child 5 description
{8}		{B} - child 6 description
{9}		{B} - child 7 description
{10}	{B} - child 8 description
{11}	{B} - child 9 description
{12}	{B} - child 10 description
{13}	{B} - child 11 description
{14}	{B} - child 12 description
{15}	{B} - child 13 description
{16}	{B} - child 14 description
{17}	{B} - child 15 description
{18}	{B} - child 16 description
{19}	{B} - child 17 description
{20}	{B} - child 18 description
{21}	{B} - child 19 description
{22}	{B} - child 20 description
{23}	{B} - child 21 description
{24}	{B} - child 22 description
{25}	{B} - child 23 description
{26}	{B} - child 24 description
{27}	{B} - child 25 description
{28}	{B} - child 26 description
{29}	{B} - child 27 description
{30}	{B} - child 28 description
{31}	{B} - child 29 description
{32}	{B} - child 30 description
{33}	{B} - child 31 description
{34}	{B} - child 32 description
{35}	{B} - child 33 description
{36}	{B} - child 34 description
{37}	{B} - child 35 description
{38}	{B} - child 36 description
{39}	{B} - child 37 description
{40}	{B} - child 38 description
{41}	{B} - child 39 description
{42}	{B} - child 40 description
{43}	{B} - child 41 description
{44}	{B} - child 42 description
{45}	{B} - section page footer
{46}
{47}	{B} - page 1 footer
--- Page 2 ---
{48}	{B} - page 2 header
{49}	{B} - section page header
{50}	{B} - child 43 description
{51}	{B} - child 44 description
{52}	{B} - child 45 description
{53}	{B} - child 46 description
{54}	{B} - child 47 description
{55}	{B} - child 48 description
{56}	{B} - child 49 description
{57}	{B} - child 50 description
{58}	{B} - section page footer
{59}
{60}
{61}	{B} - page 2 footer",
				stringBuilder.ToString());
		}

		#endregion

		#region TestCreateDocument_WithMultiplePages

		public void TestCreateDocument_WithMultiplePages()
		{
			var worksheet = DummyWorksheet.Parse(
@"#Config:Name=""Test Document""
#End
#PageHeader
	page <PageNumber> header
#End
#Body
	1
	2
	3
	4
	5
	6
	7
	8
	9
	10
	11
	12
	13
	14
	15
	16
	17
	18
	19
	20
	21
	22
	23
	24
	25
	26
	27
	28
	29
	30
	31
	32
	33
	34
	35
	36
	37
	38
	39
	40
	41
	42
	43
	44
	45
	46
	47
	48
	49
	50
#End
#Body:Data=Collection
	Child description is: <Z0_Description>
#End
#PageFooter
	page <PageNumber> footer
#End");

			var template = new StandardTemplate(worksheet);

			var dummy = Factory.New<DummyBusinessObject>();
			dummy.Z0_Description = "some description";

			for (int i = 1; i <= 50; i++)
			{
				var child = dummy.Collection.AddNew();
				child.Z0_Description = string.Format("child {0} description", i);
			}

			var documentBuilder = CreateDocumentBuilder(template, dummy);

			var document = documentBuilder.Build();

			AssertMultilineASCIIEquals("template has been parsed with no errors", "",
				string.Join("\r\n", template.Notifications.Select(n => n.Message)));

			var doc = new DocumentStringBuilder(document);

			AssertMultilineASCIIEquals("expected document",
@"	{A}	{B}
--- Page 1 ---
{1}		{B} - page 1 header
{2}		{B} - 1
{3}		{B} - 2
{4}		{B} - 3
{5}		{B} - 4
{6}		{B} - 5
{7}		{B} - 6
{8}		{B} - 7
{9}		{B} - 8
{10}	{B} - 9
{11}	{B} - 10
{12}	{B} - 11
{13}	{B} - 12
{14}	{B} - 13
{15}	{B} - 14
{16}	{B} - 15
{17}	{B} - 16
{18}	{B} - 17
{19}	{B} - 18
{20}	{B} - 19
{21}	{B} - 20
{22}	{B} - 21
{23}	{B} - 22
{24}	{B} - 23
{25}	{B} - 24
{26}	{B} - 25
{27}	{B} - 26
{28}	{B} - 27
{29}	{B} - 28
{30}	{B} - 29
{31}	{B} - 30
{32}	{B} - 31
{33}	{B} - 32
{34}	{B} - 33
{35}	{B} - 34
{36}	{B} - 35
{37}	{B} - 36
{38}	{B} - 37
{39}	{B} - 38
{40}	{B} - 39
{41}	{B} - 40
{42}	{B} - 41
{43}	{B} - 42
{44}	{B} - 43
{45}	{B} - 44
{46}
{47}	{B} - page 1 footer
--- Page 2 ---
{48}	{B} - page 2 header
{49}	{B} - 45
{50}	{B} - 46
{51}	{B} - 47
{52}	{B} - 48
{53}	{B} - 49
{54}	{B} - 50
{55}	{B} - Child description is: child 1 description
{56}	{B} - Child description is: child 2 description
{57}	{B} - Child description is: child 3 description
{58}	{B} - Child description is: child 4 description
{59}	{B} - Child description is: child 5 description
{60}	{B} - Child description is: child 6 description
{61}	{B} - Child description is: child 7 description
{62}	{B} - Child description is: child 8 description
{63}	{B} - Child description is: child 9 description
{64}	{B} - Child description is: child 10 description
{65}	{B} - Child description is: child 11 description
{66}	{B} - Child description is: child 12 description
{67}	{B} - Child description is: child 13 description
{68}	{B} - Child description is: child 14 description
{69}	{B} - Child description is: child 15 description
{70}	{B} - Child description is: child 16 description
{71}	{B} - Child description is: child 17 description
{72}	{B} - Child description is: child 18 description
{73}	{B} - Child description is: child 19 description
{74}	{B} - Child description is: child 20 description
{75}	{B} - Child description is: child 21 description
{76}	{B} - Child description is: child 22 description
{77}	{B} - Child description is: child 23 description
{78}	{B} - Child description is: child 24 description
{79}	{B} - Child description is: child 25 description
{80}	{B} - Child description is: child 26 description
{81}	{B} - Child description is: child 27 description
{82}	{B} - Child description is: child 28 description
{83}	{B} - Child description is: child 29 description
{84}	{B} - Child description is: child 30 description
{85}	{B} - Child description is: child 31 description
{86}	{B} - Child description is: child 32 description
{87}	{B} - Child description is: child 33 description
{88}	{B} - Child description is: child 34 description
{89}	{B} - Child description is: child 35 description
{90}	{B} - Child description is: child 36 description
{91}	{B} - Child description is: child 37 description
{92}	{B} - Child description is: child 38 description
{93}
{94}	{B} - page 2 footer
--- Page 3 ---
{95}	{B} - page 3 header
{96}	{B} - Child description is: child 39 description
{97}	{B} - Child description is: child 40 description
{98}	{B} - Child description is: child 41 description
{99}	{B} - Child description is: child 42 description
{100}	{B} - Child description is: child 43 description
{101}	{B} - Child description is: child 44 description
{102}	{B} - Child description is: child 45 description
{103}	{B} - Child description is: child 46 description
{104}	{B} - Child description is: child 47 description
{105}	{B} - Child description is: child 48 description
{106}	{B} - Child description is: child 49 description
{107}	{B} - Child description is: child 50 description
{108}
{109}
{110}	{B} - page 3 footer",
				doc.ToString());
		}

		#endregion

		#region TestSectionHeaderDoesNotPrintIfTheresNoData

		public void TestSectionHeaderDoesNotPrintIfTheresNoData()
		{
			var worksheet = DummyWorksheet.Parse(
@"#Config:Name=""Test Document""
#End
#Body
	some content
#End
#Body:Data=Collection
#Header
	section header
#End
	<Collection.Z0_Description>
#End");

			var template = new StandardTemplate(worksheet);

			var dummy = Factory.New<DummyBusinessObject>();
			dummy.Z0_Description = "some description";

			var documentBuilder = CreateDocumentBuilder(template, dummy);

			var document = documentBuilder.Build();

			AssertMultilineASCIIEquals("template has been parsed with no errors", "",
				string.Join("\r\n", template.Notifications.Select(n => n.Message)));

			var stringBuilder = new DocumentStringBuilder(document);

			AssertMultilineASCIIEquals("expected document",
@"	{A}	{B}
--- Page 1 ---
{1}		{B} - some content",
				stringBuilder.ToString());
		}

		#endregion

		#region TestTemplateWithMergedCell

		public void TestTemplateWithMergedCell()
		{
			var worksheet = DummyWorksheet.Parse(
@"#Config:Name=""Test Document""
#End
#Body
	a	b	c


#End");

			var cell = new DummyCell
			{
				TopRow = 5,
				LeftColumn = 2,
				BottomRow = 6,
				RightColumn = 4,
				Value = "merged cell"
			};

			worksheet.AddCell(cell);

			var template = new StandardTemplate(worksheet);

			var dummy = Factory.New<DummyBusinessObject>();

			var documentBuilder = CreateDocumentBuilder(template, dummy);

			var document = documentBuilder.Build();

			AssertMultilineASCIIEquals("template has been parsed with no errors", "",
				string.Join("\r\n", template.Notifications.Select(n => n.Message)));

			var stringBuilder = new DocumentStringBuilder(document);

			AssertMultilineASCIIEquals("expected document",
@"	{A}	{B}	{C}	{D}
--- Page 1 ---
{1}		{B} - a	{C} - b	{D} - c
{2}		{B} - merged cell
{3}", stringBuilder.ToString());
		}

		#endregion

		#region TestCreateDocument_FillerContinuesBorders

		public void TestCreateDocument_FillerContinuesBorders()
		{
			var worksheet = DummyWorksheet.Parse(
@"#Config:Name=""Test Document""
#End
#Body:Data=Collection:Height=100
	<Z0_Description>
#End");
			var format = new Format
			{
				Borders = new Borders(Border.Empty, Border.Empty, new Border(BorderStyle.Medium, Color.Empty), new Border(BorderStyle.Thick, Color.Empty), Border.Empty, Border.Empty)
			};

			var cell = new Cell
			{
				BottomRow = 4,
				TopRow = 4,

				LeftColumn = 2,
				RightColumn = 2,

				Height = 20.2d,

				Value = "<Z0_Description>",

				Format = format
			};

			worksheet.AddCell(cell);

			var template = new StandardTemplate(worksheet);

			var dummy = Factory.New<DummyBusinessObject>();

			for (int i = 1; i <= 2; i++)
			{
				var child = dummy.Collection.AddNew();
				child.Z0_Description = string.Format("child {0} description", i);
			}

			var documentBuilder = CreateDocumentBuilder(template, dummy);

			var document = documentBuilder.Build();

			AssertMultilineASCIIEquals("template has been parsed with no errors", "",
				string.Join("\r\n", template.Notifications.Select(n => n.Message)));

			var stringBuilder = new DocumentStringBuilder(document);

			AssertMultilineASCIIEquals("expected document",
@"	{A}	{B}
--- Page 1 ---
{1}		{B} - child 1 description
{2}		{B} - child 2 description
{3}", stringBuilder.ToString());

			var fillerCell = document.GetDocumentCell(3, 2);

			AssertNotNull("filler cell format", fillerCell.Format);
			AssertNotNull("filler cell format borders", fillerCell.Format.Borders);

			AssertEquals("filler cell format borders left",
				fillerCell.Format.Borders.Left.Style, format.Borders.Left.Style);

			AssertEquals("filler cell format borders right",
				fillerCell.Format.Borders.Right.Style, format.Borders.Right.Style);

			AssertEquals("filler cell format borders top",
				fillerCell.Format.Borders.Top, format.Borders.Top);

			AssertEquals("filler cell format borders bottom",
				fillerCell.Format.Borders.Bottom, format.Borders.Bottom);
		}

		#endregion

		#region TestCreateDocument_InLanguageOfCurrentlyLoggedInUser

		public void TestCreateDocument_InLanguageOfCurrentlyLoggedInUser()
		{
			GlbStaff.CurrentUser.GS_WorkingLanguage = Constants.Languages.Spanish;

			var worksheet = DummyWorksheet.Parse(
				@"#Config:Name=""Test Document"":EnableTranslation
#End
#PageHeader
	page <PageNumber> header
#End
#Body
	<Text>
#End
#PageFooter
	page <PageNumber> footer
#End");

			var template = new StandardTemplate(worksheet);
			AssertMultilineASCIIEquals("template has been parsed with no errors", "",
				string.Join("\r\n", template.Notifications.Select(n => n.Message)));

			using (var mockCache = Res.GetLanguageInstance(Constants.Languages.Spanish).UseMockData())
			{
				var resStringKey = "page <PageNumber> header".GetResStringKey();
				mockCache.Put(resStringKey,
					new ResourceStringData(resStringKey, "página {0} encabezamiento"));

				resStringKey = "page <PageNumber> footer".GetResStringKey();
				mockCache.Put(resStringKey,
					new ResourceStringData(resStringKey, "página {0} pie de página"));

				var dummy = new DummyDocDataObject();
				dummy.Text = "some description";

				var documentBuilder = CreateDocumentBuilder(template, dummy);

				var document = documentBuilder.Build();

				AssertNotEquals("current language is diffrent than document language", document.Language, Res.CurrentLanguage);

				var stringBuilder = new DocumentStringBuilder(document);

				AssertMultilineASCIIEquals("expected document",
	@"	{A}	{B}
--- Page 1 ---
{1}		{B} - página 1 encabezamiento
{2}		{B} - some description
{3}
{4}
{5}		{B} - página 1 pie de página",
	stringBuilder.ToString());
			}
		}

		#endregion

		#endregion

		#region ShowIf

		#region TestCreateDocument_TemplateWithConditionalBodySection_Shown

		public void TestCreateDocument_TemplateWithConditionalBodySection_Shown()
		{
			var worksheet = DummyWorksheet.Parse(
@"#Config:Name=""Test Document""
#End
#Body:ShowIf=Z0_Code == ""xxx""
	<Z0_Description>
#End");

			var template = new StandardTemplate(worksheet);

			var dummy = Factory.New<DummyBusinessObject>();
			dummy.Z0_Code = "xxx";
			dummy.Z0_Description = "some description";

			var documentBuilder = CreateDocumentBuilder(template, dummy);

			var document = documentBuilder.Build();

			AssertMultilineASCIIEquals("template has been parsed with no errors", "",
				string.Join("\r\n", template.Notifications.Select(n => n.Message)));

			var stringBuilder = new DocumentStringBuilder(document);

			AssertMultilineASCIIEquals("expected document",
@"	{A}	{B}
--- Page 1 ---
{1}		{B} - some description",
				stringBuilder.ToString());
		}

		#endregion

		#region TestCreateDocument_TemplateWithConditionalBodySection_NotShown

		public void TestCreateDocument_TemplateWithConditionalBodySection_NotShown()
		{
			var worksheet = DummyWorksheet.Parse(
@"#Config:Name=""Test Document""
#End
#Body:ShowIf=Z0_Number > 1 + (2 * 1.1)
	<Z0_Description>
#End");

			var template = new StandardTemplate(worksheet);

			var dummy = Factory.New<DummyBusinessObject>();
			dummy.Z0_Number = 2;
			dummy.Z0_Description = "some description";

			var documentBuilder = CreateDocumentBuilder(template, dummy);

			var document = documentBuilder.Build();

			AssertMultilineASCIIEquals("template has been parsed with no errors", "",
				string.Join("\r\n", template.Notifications.Select(n => n.Message)));

			var stringBuilder = new DocumentStringBuilder(document);

			AssertMultilineASCIIEquals("expected document",
@"	{A}	{B}",
				stringBuilder.ToString());
		}

		#endregion

		#region TestCreateDocument_WithValidation

		public void TestCreateDocument_WithValidation()
		{
			var worksheet = DummyWorksheet.Parse(
@"#Config:Name=""Test Document""
#End
#Body:ShowIf=Z0_Code == ""xxx""
	<Z0_Description>
#End");

			var template = new StandardTemplate(worksheet);

			var dummy = Factory.New<DummyBusinessObject>();
			dummy.Z0_Code = "xxx";
			dummy.Z0_Description = "some description";

			var documentBuilder = CreateDocumentBuilder(template, dummy.MakeDynamic());

			var document = documentBuilder.Build();

			AssertMultilineASCIIEquals("template has been parsed with no errors", "",
				string.Join("\r\n", template.Notifications.Select(n => n.Message)));

			AssertEquals("IsValid", true, document.IsValid);

			var data = document.Data;
			var code = data.GetDynamicProperty(DummyBusinessObject.Schema.Z0_Code);

			AssertNotNull("prerequisite: Z0_Code has been found", code);

			code.SetValue("zzz");

			AssertEquals("IsValid", false, document.IsValid);
		}

		#endregion

		#region TestCreateDocument_WithValidation

		public void TestCreateDocument_WithValidation_CustomFieldWithZBool()
		{
			var worksheet = DummyWorksheet.Parse(
@"#Config:Name=""Test Document""
#End
#Body:ShowIf=ZProperty
	<Z0_Description>
#End");

			var template = new StandardTemplate(worksheet);

			var dummy = Factory.New<DummyBusinessObject>();
			var data = dummy.MakeDynamic();
			data.Properties.GetOrCreate("ZProperty", () => ZBool.True);

			var documentBuilder = CreateDocumentBuilder(template, data);

			var document = documentBuilder.Build();

			AssertMultilineASCIIEquals("template has been parsed with no errors", "",
				string.Join("\r\n", template.Notifications.Select(n => n.Message)));

			AssertEquals("IsValid", true, document.IsValid);

			var property = data.GetDynamicProperty("ZProperty");

			AssertNotNull("prerequisite: Property has been found", property);

			property.SetValue(false);

			AssertEquals("IsValid", false, document.IsValid);
		}

		#endregion

		#endregion

		#region PageBreak

		#region TestCreateDocument_WithPageBreak

		public void TestCreateDocument_WithPageBreak()
		{
			var worksheet = DummyWorksheet.Parse(
@"#Config:Name=""Test Document""
#End
#Body
	aaaa
#PageBreak
	bbb
#End");

			var template = new StandardTemplate(worksheet);

			var dummy = Factory.New<DummyBusinessObject>();
			dummy.Z0_Code = "xxx";
			dummy.Z0_Description = "some description";

			var documentBuilder = CreateDocumentBuilder(template, dummy.MakeDynamic());

			var document = documentBuilder.Build();

			AssertMultilineASCIIEquals("template has been parsed with no errors", "",
				string.Join("\r\n", template.Notifications.Select(n => n.Message)));

			var doc = new DocumentStringBuilder(document);

			AssertMultilineASCIIEquals("expected document",
@"	{A}	{B}
--- Page 1 ---
{1}		{B} - aaaa
--- Page 2 ---
{2}		{B} - bbb",
			doc.ToString());
		}

		#endregion

		#region TestCreateDocument_WithPageBreakHeaderAndFooter

		public void TestCreateDocument_WithPageBreakHeaderAndFooter()
		{
			var worksheet = DummyWorksheet.Parse(
@"#Config:Name=""Test Document""
#End
#Body
#PageHeader
	page <PageNumber> header
#End
	aaaa
#PageBreak
	bbb
#PageFooter
	page <PageNumber> footer
#End
#End");

			var template = new StandardTemplate(worksheet);

			var dummy = Factory.New<DummyBusinessObject>();
			dummy.Z0_Code = "xxx";
			dummy.Z0_Description = "some description";

			var documentBuilder = CreateDocumentBuilder(template, dummy.MakeDynamic());

			var document = documentBuilder.Build();

			AssertMultilineASCIIEquals("template has been parsed with no errors", "",
				string.Join("\r\n", template.Notifications.Select(n => n.Message)));

			var doc = new DocumentStringBuilder(document);

			AssertMultilineASCIIEquals("expected document",
@"	{A}	{B}
--- Page 1 ---
{1}		{B} - page 1 header
{2}		{B} - aaaa
{3}		{B} - page 1 footer
--- Page 2 ---
{4}		{B} - page 2 header
{5}		{B} - bbb
{6}		{B} - page 2 footer",
			doc.ToString());
		}

		#endregion

		#endregion

		#region KeepTogether

		#region TestCreateDocument_WithKeepTogether

		public void TestCreateDocument_WithKeepTogether()
		{
			var worksheet = DummyWorksheet.Parse(
@"#Config:Name=""Test Document""
#End
#Body
	aaa
	bbb
	ccc
	ddd
	eee
	fff
#End
#Body:Data=Collection:KeepTogether
	<Z0_Description>
#End");

			var template = new StandardTemplate(worksheet);

			var dummy = Factory.New<DummyBusinessObject>();
			dummy.Z0_Description = "dummy description";

			for (int i = 1; i <= 45; i++)
			{
				var child = dummy.Collection.AddNew();
				child.Z0_Description = string.Format("{0} child description", i);
			}

			var documentBuilder = CreateDocumentBuilder(template, dummy.MakeDynamic());

			var document = documentBuilder.Build();

			AssertMultilineASCIIEquals("template has been parsed with no errors", "",
				string.Join("\r\n", template.Notifications.Select(n => n.Message)));

			var doc = new DocumentStringBuilder(document);

			AssertMultilineASCIIEquals("expected document",
@"	{A}	{B}
--- Page 1 ---
{1}		{B} - aaa
{2}		{B} - bbb
{3}		{B} - ccc
{4}		{B} - ddd
{5}		{B} - eee
{6}		{B} - fff
--- Page 2 ---
{7}		{B} - 1 child description
{8}		{B} - 2 child description
{9}		{B} - 3 child description
{10}	{B} - 4 child description
{11}	{B} - 5 child description
{12}	{B} - 6 child description
{13}	{B} - 7 child description
{14}	{B} - 8 child description
{15}	{B} - 9 child description
{16}	{B} - 10 child description
{17}	{B} - 11 child description
{18}	{B} - 12 child description
{19}	{B} - 13 child description
{20}	{B} - 14 child description
{21}	{B} - 15 child description
{22}	{B} - 16 child description
{23}	{B} - 17 child description
{24}	{B} - 18 child description
{25}	{B} - 19 child description
{26}	{B} - 20 child description
{27}	{B} - 21 child description
{28}	{B} - 22 child description
{29}	{B} - 23 child description
{30}	{B} - 24 child description
{31}	{B} - 25 child description
{32}	{B} - 26 child description
{33}	{B} - 27 child description
{34}	{B} - 28 child description
{35}	{B} - 29 child description
{36}	{B} - 30 child description
{37}	{B} - 31 child description
{38}	{B} - 32 child description
{39}	{B} - 33 child description
{40}	{B} - 34 child description
{41}	{B} - 35 child description
{42}	{B} - 36 child description
{43}	{B} - 37 child description
{44}	{B} - 38 child description
{45}	{B} - 39 child description
{46}	{B} - 40 child description
{47}	{B} - 41 child description
{48}	{B} - 42 child description
{49}	{B} - 43 child description
{50}	{B} - 44 child description
{51}	{B} - 45 child description",
			doc.ToString());
		}

		public void TestCreateDocument_WithKeepTogetherOnNestedSection()
		{
			var worksheet = DummyWorksheet.Parse(
@"#Config:Name=""Test Document""
#End
#Body
	<Number>
#End
#Body:Data=Containers
#Body:KeepTogether
	<Description>
#Body:Data=Packlines
	<Cargo>
#End // cargo
#End // keep together
#End // containers");

			var consol = new Consol
			{
				Number = "C00000001"
			};

			for (int i = 1; i <= 10; i++)
			{
				var container = new Container
				{
					Description = string.Format("container {0}", i)
				};

				for (int j = 1; j <= 5 + i; j++)
				{
					var packline = new Packline
					{
						Cargo = string.Format("pack {0}", j)
					};

					container.Packlines.Add(packline);
				}

				consol.Containers.Add(container);
			}

			var template = new StandardTemplate(worksheet);

			var documentBuilder = CreateDocumentBuilder(template, consol);

			var document = documentBuilder.Build();

			AssertEquals("Prerequisite: row height", 20.2, ((IWorksheet)template).Rows.GetAt(1).Height);

			AssertMultilineASCIIEquals("template has been parsed with no errors", "",
				string.Join("\r\n", template.Notifications.Select(n => n.Message)));

			var doc = new DocumentStringBuilder(document);

			AssertMultilineASCIIEquals("expected document",
@"	{A}	{B}
--- Page 1 ---
{1}		{B} - C00000001
{2}		{B} - container 1
{3}		{B} - pack 1
{4}		{B} - pack 2
{5}		{B} - pack 3
{6}		{B} - pack 4
{7}		{B} - pack 5
{8}		{B} - pack 6
{9}		{B} - container 2
{10}	{B} - pack 1
{11}	{B} - pack 2
{12}	{B} - pack 3
{13}	{B} - pack 4
{14}	{B} - pack 5
{15}	{B} - pack 6
{16}	{B} - pack 7
{17}	{B} - container 3
{18}	{B} - pack 1
{19}	{B} - pack 2
{20}	{B} - pack 3
{21}	{B} - pack 4
{22}	{B} - pack 5
{23}	{B} - pack 6
{24}	{B} - pack 7
{25}	{B} - pack 8
{26}	{B} - container 4
{27}	{B} - pack 1
{28}	{B} - pack 2
{29}	{B} - pack 3
{30}	{B} - pack 4
{31}	{B} - pack 5
{32}	{B} - pack 6
{33}	{B} - pack 7
{34}	{B} - pack 8
{35}	{B} - pack 9
{36}	{B} - container 5
{37}	{B} - pack 1
{38}	{B} - pack 2
{39}	{B} - pack 3
{40}	{B} - pack 4
{41}	{B} - pack 5
{42}	{B} - pack 6
{43}	{B} - pack 7
{44}	{B} - pack 8
{45}	{B} - pack 9
{46}	{B} - pack 10
--- Page 2 ---
{47}	{B} - container 6
{48}	{B} - pack 1
{49}	{B} - pack 2
{50}	{B} - pack 3
{51}	{B} - pack 4
{52}	{B} - pack 5
{53}	{B} - pack 6
{54}	{B} - pack 7
{55}	{B} - pack 8
{56}	{B} - pack 9
{57}	{B} - pack 10
{58}	{B} - pack 11
{59}	{B} - container 7
{60}	{B} - pack 1
{61}	{B} - pack 2
{62}	{B} - pack 3
{63}	{B} - pack 4
{64}	{B} - pack 5
{65}	{B} - pack 6
{66}	{B} - pack 7
{67}	{B} - pack 8
{68}	{B} - pack 9
{69}	{B} - pack 10
{70}	{B} - pack 11
{71}	{B} - pack 12
{72}	{B} - container 8
{73}	{B} - pack 1
{74}	{B} - pack 2
{75}	{B} - pack 3
{76}	{B} - pack 4
{77}	{B} - pack 5
{78}	{B} - pack 6
{79}	{B} - pack 7
{80}	{B} - pack 8
{81}	{B} - pack 9
{82}	{B} - pack 10
{83}	{B} - pack 11
{84}	{B} - pack 12
{85}	{B} - pack 13
--- Page 3 ---
{86}	{B} - container 9
{87}	{B} - pack 1
{88}	{B} - pack 2
{89}	{B} - pack 3
{90}	{B} - pack 4
{91}	{B} - pack 5
{92}	{B} - pack 6
{93}	{B} - pack 7
{94}	{B} - pack 8
{95}	{B} - pack 9
{96}	{B} - pack 10
{97}	{B} - pack 11
{98}	{B} - pack 12
{99}	{B} - pack 13
{100}	{B} - pack 14
{101}	{B} - container 10
{102}	{B} - pack 1
{103}	{B} - pack 2
{104}	{B} - pack 3
{105}	{B} - pack 4
{106}	{B} - pack 5
{107}	{B} - pack 6
{108}	{B} - pack 7
{109}	{B} - pack 8
{110}	{B} - pack 9
{111}	{B} - pack 10
{112}	{B} - pack 11
{113}	{B} - pack 12
{114}	{B} - pack 13
{115}	{B} - pack 14
{116}	{B} - pack 15",
			doc.ToString());
		}

		public void TestCreateDocument_WithKeepTogetherOverflowOnTheFirstPage()
		{
			var worksheet = DummyWorksheet.Parse(
@"#Config:Name=""Test Document""
#End
#Body:Data=Collection:KeepTogether
	<Z0_Description>
#End");

			var template = new StandardTemplate(worksheet);

			var dummy = Factory.New<DummyBusinessObject>();
			dummy.Z0_Description = "dummy description";

			for (int i = 1; i <= 50; i++)
			{
				var child = dummy.Collection.AddNew();
				child.Z0_Description = string.Format("{0} child description", i);
			}

			var documentBuilder = CreateDocumentBuilder(template, dummy.MakeDynamic());

			var document = documentBuilder.Build();

			AssertMultilineASCIIEquals("template has been parsed with no errors", "",
				string.Join("\r\n", template.Notifications.Select(n => n.Message)));

			var doc = new DocumentStringBuilder(document);

			AssertMultilineASCIIEquals("expected document",
@"	{A}	{B}
--- Page 1 ---
{1}		{B} - 1 child description
{2}		{B} - 2 child description
{3}		{B} - 3 child description
{4}		{B} - 4 child description
{5}		{B} - 5 child description
{6}		{B} - 6 child description
{7}		{B} - 7 child description
{8}		{B} - 8 child description
{9}		{B} - 9 child description
{10}	{B} - 10 child description
{11}	{B} - 11 child description
{12}	{B} - 12 child description
{13}	{B} - 13 child description
{14}	{B} - 14 child description
{15}	{B} - 15 child description
{16}	{B} - 16 child description
{17}	{B} - 17 child description
{18}	{B} - 18 child description
{19}	{B} - 19 child description
{20}	{B} - 20 child description
{21}	{B} - 21 child description
{22}	{B} - 22 child description
{23}	{B} - 23 child description
{24}	{B} - 24 child description
{25}	{B} - 25 child description
{26}	{B} - 26 child description
{27}	{B} - 27 child description
{28}	{B} - 28 child description
{29}	{B} - 29 child description
{30}	{B} - 30 child description
{31}	{B} - 31 child description
{32}	{B} - 32 child description
{33}	{B} - 33 child description
{34}	{B} - 34 child description
{35}	{B} - 35 child description
{36}	{B} - 36 child description
{37}	{B} - 37 child description
{38}	{B} - 38 child description
{39}	{B} - 39 child description
{40}	{B} - 40 child description
{41}	{B} - 41 child description
{42}	{B} - 42 child description
{43}	{B} - 43 child description
{44}	{B} - 44 child description
{45}	{B} - 45 child description
{46}	{B} - 46 child description
{47}	{B} - 47 child description
--- Page 2 ---
{48}	{B} - 48 child description
{49}	{B} - 49 child description
{50}	{B} - 50 child description",
			doc.ToString());
		}

		public void TestCreateDocument_WithKeepTogetherAndHeaderAndFooter()
		{
			var worksheet = DummyWorksheet.Parse(
@"#Config:Name=""Test Document""
#End
#Body
	aaa
	bbb
	ccc
	ddd
	eee
	fff
#End
#Body:Data=Collection:KeepTogether
#Header
	Let it go! Leeeeet it go!
#End
	<Z0_Description>
#Footer
	Sorry, got a bit frozen...
#End
#End");

			var template = new StandardTemplate(worksheet);

			var dummy = Factory.New<DummyBusinessObject>();
			dummy.Z0_Description = "dummy description";

			for (int i = 1; i <= 45; i++)
			{
				var child = dummy.Collection.AddNew();
				child.Z0_Description = string.Format("{0} child description", i);
			}

			var documentBuilder = CreateDocumentBuilder(template, dummy.MakeDynamic());

			var document = documentBuilder.Build();

			AssertMultilineASCIIEquals("template has been parsed with no errors", "",
				string.Join("\r\n", template.Notifications.Select(n => n.Message)));

			var doc = new DocumentStringBuilder(document);

			AssertMultilineASCIIEquals("expected document",
@"	{A}	{B}
--- Page 1 ---
{1}		{B} - aaa
{2}		{B} - bbb
{3}		{B} - ccc
{4}		{B} - ddd
{5}		{B} - eee
{6}		{B} - fff
--- Page 2 ---
{7}		{B} - Let it go! Leeeeet it go!
{8}		{B} - 1 child description
{9}		{B} - 2 child description
{10}	{B} - 3 child description
{11}	{B} - 4 child description
{12}	{B} - 5 child description
{13}	{B} - 6 child description
{14}	{B} - 7 child description
{15}	{B} - 8 child description
{16}	{B} - 9 child description
{17}	{B} - 10 child description
{18}	{B} - 11 child description
{19}	{B} - 12 child description
{20}	{B} - 13 child description
{21}	{B} - 14 child description
{22}	{B} - 15 child description
{23}	{B} - 16 child description
{24}	{B} - 17 child description
{25}	{B} - 18 child description
{26}	{B} - 19 child description
{27}	{B} - 20 child description
{28}	{B} - 21 child description
{29}	{B} - 22 child description
{30}	{B} - 23 child description
{31}	{B} - 24 child description
{32}	{B} - 25 child description
{33}	{B} - 26 child description
{34}	{B} - 27 child description
{35}	{B} - 28 child description
{36}	{B} - 29 child description
{37}	{B} - 30 child description
{38}	{B} - 31 child description
{39}	{B} - 32 child description
{40}	{B} - 33 child description
{41}	{B} - 34 child description
{42}	{B} - 35 child description
{43}	{B} - 36 child description
{44}	{B} - 37 child description
{45}	{B} - 38 child description
{46}	{B} - 39 child description
{47}	{B} - 40 child description
{48}	{B} - 41 child description
{49}	{B} - 42 child description
{50}	{B} - 43 child description
{51}	{B} - 44 child description
{52}	{B} - 45 child description
{53}	{B} - Sorry, got a bit frozen...",
			doc.ToString());
		}

		public void TestCreateDocument_WithKeepTogetherAndPageHeaderAndPageFooter()
		{
			var worksheet = DummyWorksheet.Parse(
@"#Config:Name=""Test Document""
#End
#Body
	aaa
	bbb
	ccc
	ddd
	eee
	fff
#End
#Body:Data=Collection:KeepTogether
#PageHeader
	Let it go! Leeeeet it go!
#End
	<Z0_Description>
#PageFooter
	Sorry, got a bit frozen...
#End
#End");

			var template = new StandardTemplate(worksheet);

			var dummy = Factory.New<DummyBusinessObject>();
			dummy.Z0_Description = "dummy description";

			for (int i = 1; i <= 45; i++)
			{
				var child = dummy.Collection.AddNew();
				child.Z0_Description = string.Format("{0} child description", i);
			}

			var documentBuilder = CreateDocumentBuilder(template, dummy.MakeDynamic());

			var document = documentBuilder.Build();

			AssertMultilineASCIIEquals("template has been parsed with no errors", "",
				string.Join("\r\n", template.Notifications.Select(n => n.Message)));

			var doc = new DocumentStringBuilder(document);

			AssertMultilineASCIIEquals("expected document",
@"	{A}	{B}
--- Page 1 ---
{1}		{B} - aaa
{2}		{B} - bbb
{3}		{B} - ccc
{4}		{B} - ddd
{5}		{B} - eee
{6}		{B} - fff
--- Page 2 ---
{7}		{B} - Let it go! Leeeeet it go!
{8}		{B} - 1 child description
{9}		{B} - 2 child description
{10}	{B} - 3 child description
{11}	{B} - 4 child description
{12}	{B} - 5 child description
{13}	{B} - 6 child description
{14}	{B} - 7 child description
{15}	{B} - 8 child description
{16}	{B} - 9 child description
{17}	{B} - 10 child description
{18}	{B} - 11 child description
{19}	{B} - 12 child description
{20}	{B} - 13 child description
{21}	{B} - 14 child description
{22}	{B} - 15 child description
{23}	{B} - 16 child description
{24}	{B} - 17 child description
{25}	{B} - 18 child description
{26}	{B} - 19 child description
{27}	{B} - 20 child description
{28}	{B} - 21 child description
{29}	{B} - 22 child description
{30}	{B} - 23 child description
{31}	{B} - 24 child description
{32}	{B} - 25 child description
{33}	{B} - 26 child description
{34}	{B} - 27 child description
{35}	{B} - 28 child description
{36}	{B} - 29 child description
{37}	{B} - 30 child description
{38}	{B} - 31 child description
{39}	{B} - 32 child description
{40}	{B} - 33 child description
{41}	{B} - 34 child description
{42}	{B} - 35 child description
{43}	{B} - 36 child description
{44}	{B} - 37 child description
{45}	{B} - 38 child description
{46}	{B} - 39 child description
{47}	{B} - 40 child description
{48}	{B} - 41 child description
{49}	{B} - 42 child description
{50}	{B} - 43 child description
{51}	{B} - 44 child description
{52}	{B} - 45 child description
{53}	{B} - Sorry, got a bit frozen...",
			doc.ToString());
		}

		#endregion

		#endregion

		#region Expandable

		public void TestCreateDocument_WithKeepTogether_WithExpandable()
		{
			var worksheet = DummyWorksheet.Parse(
@"#Config:Name=""Test Document""
#End
#Body
	aaa
	bbb
	ccc
	ddd
	eee
	fff
#End
#Body:Data=Collection:KeepTogether:Expandable=false
	<Z0_Description>
#End");

			var template = new StandardTemplate(worksheet);
			var dummy = Factory.New<DummyBusinessObject>();
			dummy.Z0_Description = "dummy description";

			for (var i = 1; i <= 45; i++)
			{
				var child = dummy.Collection.AddNew();
				child.Z0_Description = string.Format("{0} child description", i);
			}

			var documentBuilder = CreateDocumentBuilder(template, dummy.MakeDynamic());
			var document = documentBuilder.Build();
			AssertMultilineASCIIEquals("template has been parsed with no errors", "",
				string.Join("\r\n", template.Notifications.Select(n => n.Message)));

			var doc = new DocumentStringBuilder(document);
			AssertMultilineASCIIEquals("expected document",
@"	{A}	{B}
--- Page 1 ---
{1}		{B} - aaa
{2}		{B} - bbb
{3}		{B} - ccc
{4}		{B} - ddd
{5}		{B} - eee
{6}		{B} - fff
{7}",
			doc.ToString());
		}

		public void TestCreateDocument_WithKeepTogetherAndPageHeaderAndPageFooter_WithExpandable()
		{
			var worksheet = DummyWorksheet.Parse(
@"#Config:Name=""Test Document""
#End
#Body
	aaa
	bbb
	ccc
	ddd
	eee
	fff
#End
#Body:Data=Collection:KeepTogether:Expandable=false
#PageHeader
	Let it go! Leeeeet it go!
#End
	<Z0_Description>
#PageFooter
	Sorry, got a bit frozen...
#End
#End");

			var template = new StandardTemplate(worksheet);

			var dummy = Factory.New<DummyBusinessObject>();
			dummy.Z0_Description = "dummy description";

			for (int i = 1; i <= 45; i++)
			{
				var child = dummy.Collection.AddNew();
				child.Z0_Description = string.Format("{0} child description", i);
			}

			var documentBuilder = CreateDocumentBuilder(template, dummy.MakeDynamic());

			var document = documentBuilder.Build();

			AssertMultilineASCIIEquals("template has been parsed with no errors", "",
				string.Join("\r\n", template.Notifications.Select(n => n.Message)));

			var doc = new DocumentStringBuilder(document);

			AssertMultilineASCIIEquals("expected document",
@"	{A}	{B}
--- Page 1 ---
{1}		{B} - aaa
{2}		{B} - bbb
{3}		{B} - ccc
{4}		{B} - ddd
{5}		{B} - eee
{6}		{B} - fff
{7}		{B} - Let it go! Leeeeet it go!
{8}
{9}		{B} - Sorry, got a bit frozen...",
			doc.ToString());
		}

		#endregion

		#region Center Horizontally

		public void TestPrintContentCenteredHorizontally()
		{
			var worksheet = DummyWorksheet.Parse(
@"#Config:Name=""Test Document""
#End
#Body
	aaa
#End");
			worksheet.PrintContentCenteredHorizontally = true;

			IStandardTemplate template = new StandardTemplate(worksheet);

			AssertEquals("PrintContentCenteredHorizontally on template has been set",
				true, template.PrintContentCenteredHorizontally);

			var dummy = Factory.New<DummyBusinessObject>();

			var documentBuilder = CreateDocumentBuilder(template, dummy.MakeDynamic());

			var document = documentBuilder.Build();

			AssertEquals("PrintContentCenteredHorizontally on document has been set",
				true, document.PrintContentCenteredHorizontally);

			AssertEquals("HorizontalPrintOffset has been calculated",
				319.95, document.HorizontalPrintOffset);
		}

		#endregion

		#region Images

		#region TestDrawing_OverText

		public void TestDrawing_OverText()
		{
			var worksheet = DummyWorksheet.Parse(
@"#Config:Name=""Test Document""
#End
#Body
	<Z0_Code>
	<Z0_Description>
#End");

			var drawing = new Drawing
			{
				TopRow = 4,
				LeftColumn = 2,
				BottomRow = 5,
				RightColumn = 2
			};

			var cell = (DummyCell)worksheet.GetCell(drawing.TopRow, drawing.LeftColumn);
			cell.SetDrawing(drawing);

			var template = new StandardTemplate(worksheet);

			var dummy = Factory.New<DummyBusinessObject>();
			dummy.Z0_Code = "code";
			dummy.Z0_Description = "some description";

			var documentBuilder = CreateDocumentBuilder(template, dummy);

			var document = documentBuilder.Build();

			AssertMultilineASCIIEquals("template has been parsed with no errors", "",
				string.Join("\r\n", template.Notifications.Select(n => n.Message)));

			var stringBuilder = new DocumentStringBuilder(document);

			AssertMultilineASCIIEquals("expected document",
@"	{A}	{B}
--- Page 1 ---
{1}		{B} - code
{2}		{B} - some description",
			stringBuilder.ToString());

			var documentCell = document.GetDocumentCell(1, 2);

			AssertNotNull("drawing", documentCell.Drawing);
		}

		#endregion

		#region TestDrawing_OverEmptyRows

		public void TestDrawing_OverEmptyRows()
		{
			const string tab = "\t";

			var worksheet = DummyWorksheet.Parse(
$@"#Config:Name=""Test Document""
#End
#Body
{tab}
{tab}
#End");

			AssertEquals("prerequisite: worksheet has 2 columns", 2, worksheet.Columns.Count);

			var drawing = new Drawing
			{
				TopRow = 4,
				LeftColumn = 2,
				BottomRow = 5,
				RightColumn = 2
			};

			var cell = (DummyCell)worksheet.GetCell(drawing.TopRow, drawing.LeftColumn);
			cell.SetDrawing(drawing);

			var template = new StandardTemplate(worksheet);

			var dummy = Factory.New<DummyBusinessObject>();

			var documentBuilder = CreateDocumentBuilder(template, dummy);

			var document = documentBuilder.Build();

			AssertMultilineASCIIEquals("template has been parsed with no errors", "",
				string.Join("\r\n", template.Notifications.Select(n => n.Message)));

			var stringBuilder = new DocumentStringBuilder(document);

			AssertMultilineASCIIEquals("expected document",
@"	{A}	{B}
--- Page 1 ---
{1}
{2}",
			stringBuilder.ToString());

			var documentCell = document.GetDocumentCell(1, 2);

			AssertNotNull("drawing", documentCell.Drawing);
		}

		#endregion

		#region TestDrawing_SpanningAccrossSectionsIsIgnored

		public void TestDrawing_SpanningAccrossSectionsIsIgnored()
		{
			var worksheet = DummyWorksheet.Parse(
@"#Config:Name=""Test Document""
#End
#Body
	<Z0_Code>
	<Z0_Description>
#End");

			var drawing = new Drawing
			{
				TopRow = 1,
				LeftColumn = 2,
				BottomRow = 5,
				RightColumn = 2
			};

			var cell = (DummyCell)worksheet.GetCell(drawing.TopRow, drawing.LeftColumn);
			cell.SetDrawing(drawing);

			var template = new StandardTemplate(worksheet);

			var dummy = Factory.New<DummyBusinessObject>();
			dummy.Z0_Code = "code";
			dummy.Z0_Description = "some description";

			var documentBuilder = CreateDocumentBuilder(template, dummy);

			var document = documentBuilder.Build();

			AssertMultilineASCIIEquals("template has been parsed with no errors", "",
				string.Join("\r\n", template.Notifications.Select(n => n.Message)));

			var stringBuilder = new DocumentStringBuilder(document);

			AssertMultilineASCIIEquals("expected document",
@"	{A}	{B}
--- Page 1 ---
{1}		{B} - code
{2}		{B} - some description",
			stringBuilder.ToString());

			foreach (var row in document.Rows)
			{
				foreach (var column in document.Columns)
				{
					var documentCell = document.GetDocumentCell(row.Number, column.Number);

					AssertNull(string.Format("Cell at [{0},{1}] has no drawing", cell.TopRow, cell.BottomRow),
						documentCell.Drawing);
				}
			}
		}

		#endregion

		#region TestDrawing_PageHeader

		public void TestDrawing_PageHeader()
		{
			var worksheet = DummyWorksheet.Parse(
@"#Config:Name=""Test Document""
#End
#PageHeader
	Page Header
#End
#Body
	<Z0_Code>
	<Z0_Description>
#End");

			var drawing = new Drawing
			{
				TopRow = 4,
				LeftColumn = 2,
				BottomRow = 4,
				RightColumn = 2
			};

			var cell = (DummyCell)worksheet.GetCell(drawing.TopRow, drawing.LeftColumn);
			cell.SetDrawing(drawing);

			var template = new StandardTemplate(worksheet);

			var dummy = Factory.New<DummyBusinessObject>();
			dummy.Z0_Code = "code";
			dummy.Z0_Description = "some description";

			var documentBuilder = CreateDocumentBuilder(template, dummy);

			var document = documentBuilder.Build();

			AssertMultilineASCIIEquals("template has been parsed with no errors", "",
				string.Join("\r\n", template.Notifications.Select(n => n.Message)));

			var stringBuilder = new DocumentStringBuilder(document);

			AssertMultilineASCIIEquals("expected document",
@"	{A}	{B}
--- Page 1 ---
{1}		{B} - Page Header
{2}		{B} - code
{3}		{B} - some description",
			stringBuilder.ToString());

			var documentCell = document.GetDocumentCell(1, 2);

			AssertNotNull("drawing", documentCell.Drawing);
		}

		#endregion

		#region TestDrawing_PageFooter

		public void TestDrawing_PageFooter()
		{
			var worksheet = DummyWorksheet.Parse(
@"#Config:Name=""Test Document""
#End
#Body
	<Z0_Code>
	<Z0_Description>
#End
#PageFooter
	Page Footer
#End");

			var drawing = new Drawing
			{
				TopRow = 8,
				LeftColumn = 2,
				BottomRow = 8,
				RightColumn = 2
			};

			var cell = (DummyCell)worksheet.GetCell(drawing.TopRow, drawing.LeftColumn);
			cell.SetDrawing(drawing);

			var template = new StandardTemplate(worksheet);

			var dummy = Factory.New<DummyBusinessObject>();
			dummy.Z0_Code = "code";
			dummy.Z0_Description = "some description";

			var documentBuilder = CreateDocumentBuilder(template, dummy);

			var document = documentBuilder.Build();

			AssertMultilineASCIIEquals("template has been parsed with no errors", "",
				string.Join("\r\n", template.Notifications.Select(n => n.Message)));

			var stringBuilder = new DocumentStringBuilder(document);

			AssertMultilineASCIIEquals("expected document",
@"	{A}	{B}
--- Page 1 ---
{1}		{B} - code
{2}		{B} - some description
{3}
{4}
{5}		{B} - Page Footer",
			stringBuilder.ToString());

			var documentCell = document.GetDocumentCell(5, 2);

			AssertNotNull("drawing", documentCell.Drawing);
		}

		#endregion

		#region TestDrawing_RepeatableSection

		public void TestDrawing_RepeatableSection()
		{
			var worksheet = DummyWorksheet.Parse(
@"#Config:Name=""Test Document""
#End
#Body:Data=Collection
	<Z0_Code>
	<Z0_Description>
#End");

			var drawing = new Drawing
			{
				TopRow = 4,
				LeftColumn = 2,
				BottomRow = 4,
				RightColumn = 2
			};

			var cell = (DummyCell)worksheet.GetCell(drawing.TopRow, drawing.LeftColumn);
			cell.SetDrawing(drawing);

			var template = new StandardTemplate(worksheet);

			var dummy = Factory.New<DummyBusinessObject>();

			for (int i = 1; i <= 30; i++)
			{
				var child = dummy.Collection.AddNew();
				child.Z0_Code = i.ToString();
				child.Z0_Description = string.Format("child {0} description", i);
			}

			var documentBuilder = CreateDocumentBuilder(template, dummy);

			var document = documentBuilder.Build();

			AssertMultilineASCIIEquals("template has been parsed with no errors", "",
				string.Join("\r\n", template.Notifications.Select(n => n.Message)));

			var stringBuilder = new DocumentStringBuilder(document);

			AssertMultilineASCIIEquals("expected document",
@"	{A}	{B}
--- Page 1 ---
{1}		{B} - 1
{2}		{B} - child 1 description
{3}		{B} - 2
{4}		{B} - child 2 description
{5}		{B} - 3
{6}		{B} - child 3 description
{7}		{B} - 4
{8}		{B} - child 4 description
{9}		{B} - 5
{10}	{B} - child 5 description
{11}	{B} - 6
{12}	{B} - child 6 description
{13}	{B} - 7
{14}	{B} - child 7 description
{15}	{B} - 8
{16}	{B} - child 8 description
{17}	{B} - 9
{18}	{B} - child 9 description
{19}	{B} - 10
{20}	{B} - child 10 description
{21}	{B} - 11
{22}	{B} - child 11 description
{23}	{B} - 12
{24}	{B} - child 12 description
{25}	{B} - 13
{26}	{B} - child 13 description
{27}	{B} - 14
{28}	{B} - child 14 description
{29}	{B} - 15
{30}	{B} - child 15 description
{31}	{B} - 16
{32}	{B} - child 16 description
{33}	{B} - 17
{34}	{B} - child 17 description
{35}	{B} - 18
{36}	{B} - child 18 description
{37}	{B} - 19
{38}	{B} - child 19 description
{39}	{B} - 20
{40}	{B} - child 20 description
{41}	{B} - 21
{42}	{B} - child 21 description
{43}	{B} - 22
{44}	{B} - child 22 description
{45}	{B} - 23
{46}	{B} - child 23 description
{47}	{B} - 24
--- Page 2 ---
{48}	{B} - child 24 description
{49}	{B} - 25
{50}	{B} - child 25 description
{51}	{B} - 26
{52}	{B} - child 26 description
{53}	{B} - 27
{54}	{B} - child 27 description
{55}	{B} - 28
{56}	{B} - child 28 description
{57}	{B} - 29
{58}	{B} - child 29 description
{59}	{B} - 30
{60}	{B} - child 30 description",
			stringBuilder.ToString());

			CombineAssertions(() =>
			{
				// page 1 images
				for (int i = 1; i <= 24; i++)
				{
					var rowNumber = (i - 1) * 2 + 1;

					var documentCell = document.GetDocumentCell(rowNumber, 2);

					AssertNotNull(string.Format("page 1 cell at [{0},2] expected to contain drawing", rowNumber),
						documentCell.Drawing);
				}

				// page 2 images
				for (int i = 25; i <= 30; i++)
				{
					var rowNumber = i * 2 - 1;

					var documentCell = document.GetDocumentCell(rowNumber, 2);

					AssertNotNull(string.Format("page 2 cell at [{0},2] expected to contain drawing", rowNumber),
						documentCell.Drawing);
				}
			});
		}

		#endregion

		#endregion

		#region Event Section

		public void TestVariablesAreUsedInBeforeDocumentCreatedEventSection()
		{
			var companyProxy = Factory.NewWithValidTestData<OrgHeader>();
			companyProxy.OH_FullName = "EDI CUSTOMS BROKERS";
			Factory.Save();

			GlbBranch.CurrentBranch.GB_OH_OrgProxy = ZGuid.Empty;
			GlbBranch.CurrentBranch.Factory.Save();
			GlbCompany.CurrentCompany.GC_OH_OrgProxy = companyProxy.PK;
			GlbCompany.CurrentCompany.Factory.Save();

			var worksheet = DummyWorksheet.Parse(
@"#Config:Name=""Test Document""
#Event:Type=""BeforeDocumentCreated""
	def val = @env.Company.Organization.Name
#End
#End
#Body
	<@val>
#End");

			IStandardTemplate template = new StandardTemplate(worksheet);

			var environment = new Enterprise.MasterFiles.Business.Macros.Environment();

			AssertEquals("prerequisite", environment.Company.Organization.Name, "EDI CUSTOMS BROKERS");

			var scope = new MacroScope(Factory.New<DummyBusinessObject>().MakeDynamic());
			scope.SetVariable(VariableNames.Environment, environment);

			var document = template.CreateDocument(
				template.Name,
				"test",
				scope,
				new[] { new StandardLibrary() }.CreateContext(),
				null);

			AssertMultilineASCIIEquals("document has been created with no errors", "",
				string.Join("\r\n", document.Notifications.Select(n => n.Message)));

			var stringBuilder = new DocumentStringBuilder(document);

			AssertMultilineASCIIEquals("expected document",
@"	{A}	{B}
--- Page 1 ---
{1}		{B} - EDI CUSTOMS BROKERS",
				stringBuilder.ToString());

			AssertContainsExactElementsInAnyOrder("document variables",
				new[]
				{
					"val|EDI CUSTOMS BROKERS",
					"env|Enterprise.MasterFiles.Business.Macros.Environment",
					"@document|Enterprise.DocumentVisualizer.Core.Document",
				},
				document.Scope.Select(var => string.Format("{0}|{1}", var.Name, var.Value)));
		}

		public void TestVariablesFromBeforeDocumentCreatedEventSectionCarryOnToDocument()
		{
			var worksheet = DummyWorksheet.Parse(
@"#Config:Name=""Test Document""
#Event:Type=""BeforeDocumentCreated""
	def var1 = 666
	def var2 = ""zzz""
#End
#End
#Body
	<Z0_Description>
#End");

			IStandardTemplate template = new StandardTemplate(worksheet);

			var dummy = Factory.New<DummyBusinessObject>();
			dummy.Z0_Description = "some description";

			var document = template.CreateDocument(
				template.Name,
				"test",
				new MacroScope(dummy.MakeDynamic()),
				new[] { new StandardLibrary() }.CreateContext(),
				null);

			AssertMultilineASCIIEquals("document has been created with no errors", "",
				string.Join("\r\n", document.Notifications.Select(n => n.Message)));

			AssertContainsExactElementsInAnyOrder("document variables",
				new[]
				{
					"var1|666",
					"var2|zzz",
					"@document|Enterprise.DocumentVisualizer.Core.Document",
				},
				document.Scope.Select(var => string.Format("{0}|{1}", var.Name, var.Value)));
		}

		#endregion

		#region Test classes

		class Consol : NonPersistentBusinessObject
		{
			public ZString Number
			{
				get { return number; }
				set { SetNonPersistentPropertyValue(NumberInfo, ref number, value); }
			}

			ZString number;

			public ZPropertyInfo NumberInfo
			{
				get { return GetZPropertyInfo(nameof(Number)); }
			}

			public ContainerCollection Containers
			{
				get { return containers ?? (containers = new ContainerCollection()); }
				set { containers = value; }
			}

			ContainerCollection containers;
		}

		class Container : NonPersistentBusinessObject
		{
			public ZString Type
			{
				get { return type; }
				set { SetNonPersistentPropertyValue(TypeInfo, ref type, value); }
			}

			ZString type;

			public ZPropertyInfo TypeInfo => GetZPropertyInfo(nameof(Type));

			public ZString Description
			{
				get { return description; }
				set { SetNonPersistentPropertyValue(DescriptionInfo, ref description, value); }
			}

			ZString description;

			public ZPropertyInfo DescriptionInfo => GetZPropertyInfo(nameof(Description));

			public PacklineCollection Packlines
			{
				get { return packlines ?? (packlines = new PacklineCollection()); }
				set { packlines = value; }
			}

			PacklineCollection packlines;
		}

		class ContainerCollection : NonPersistentBusinessObjectCollection<Container>
		{
			protected override BusinessObject CreateNonPersistentBusinessObject()
			{
				return new Container();
			}

			public static ContainerCollection Create(IEnumerable<Container> containers)
			{
				var collection = new ContainerCollection();
				foreach (var container in containers)
				{
					collection.Add(container);
				}

				return collection;
			}
		}

		class Packline : NonPersistentBusinessObject
		{
			public ZString Cargo
			{
				get { return cargo; }
				set { SetNonPersistentPropertyValue(CargoInfo, ref cargo, value); }
			}

			ZString cargo;

			public ZPropertyInfo CargoInfo
			{
				get { return GetZPropertyInfo(nameof(Cargo)); }
			}

			public DangerousGoodsCollection DangerousGoods
			{
				get { return dangerousGoods ?? (dangerousGoods = new DangerousGoodsCollection()); }
			}

			DangerousGoodsCollection dangerousGoods;
		}

		class PacklineCollection : NonPersistentBusinessObjectCollection<Packline>
		{
			protected override BusinessObject CreateNonPersistentBusinessObject()
			{
				return new Packline();
			}

			public static PacklineCollection Create(IEnumerable<Packline> packLines)
			{
				var collection = new PacklineCollection();
				foreach (var packLine in packLines)
				{
					collection.Add(packLine);
				}

				return collection;
			}
		}

		class DangerousGoods : NonPersistentBusinessObject
		{
			public ZString Code
			{
				get { return code; }
				set { SetNonPersistentPropertyValue(CodeInfo, ref code, value); }
			}

			ZString code;

			public ZPropertyInfo CodeInfo
			{
				get { return GetZPropertyInfo(nameof(Code)); }
			}
		}

		class DangerousGoodsCollection : NonPersistentBusinessObjectCollection<DangerousGoods>
		{
			protected override BusinessObject CreateNonPersistentBusinessObject()
			{
				return new DangerousGoods();
			}
		}

		#endregion

		#region Implementation

		StandardDocumentBuilder CreateDocumentBuilder(IStandardTemplate template, object data, ILogger logger = null)
		{
			return CreateDocumentBuilder(template, data.MakeDynamic(), logger);
		}

		StandardDocumentBuilder CreateDocumentBuilder(IStandardTemplate template, IDynamicData data, ILogger logger = null)
		{
			return new StandardDocumentBuilder(
				template.Name,
				"test",
				template,
				new MacroScope(data),
				new IMacroLibrary[]
				{
					new StandardLibrary(),
					new DocumentLibrary()
				}.CreateContext(),
				logger,
				new CurrentUserLanguageProvider());
		}

		#endregion

		#region CatchOomException

		public void TestMacroOutOfMemoryExceptionReported()
		{
			ErrorReporter.Clear();
			var documentDescriptorMock = new Mock<IDocumentDescriptor>();
			documentDescriptorMock.Setup(x => x.Name ).Throws(new MacroOutOfMemoryException("macroOOM"));
			Command command = new Command("1","commandName");
			Command[] commands = new Command[] { command };
			var services = new ServiceContainer();
			services.Register<IEventBroker>(new EventBroker());
			services.Register<IResourceProvider>(new DummyResourcesProvider());
			services.Register<INotificationViewBuildService>(new DummyNotificationViewBuildService());
			var parameters = new DocumentVisualizer.Business.StandardDocumentBuilder.Parameters
			{
				Descriptor = documentDescriptorMock.Object,
				Template = new Mock<IStandardTemplate>().Object,
				Services = services,
				DocumentData = new Mock<IVisualizerDocumentData>().Object,
				MacroEvaluationContext = new Mock<IMacroEvaluationContext>().Object,
				Scope = new Mock<IMacroScope>().Object,
				Logger = new Mock<ILogger>().Object,
				Commands = commands
			};
			var builder = new DocumentVisualizer.Business.StandardDocumentBuilder(parameters);
			AssertExceptionThrown("macroOOM", typeof(MacroOutOfMemoryException), () => builder.Build());
			AssertContains("MacroOutOfMemoryException", ErrorReporter.LastKeyReported);
			ErrorReporter.Clear();
		}
		#endregion
	}
}
