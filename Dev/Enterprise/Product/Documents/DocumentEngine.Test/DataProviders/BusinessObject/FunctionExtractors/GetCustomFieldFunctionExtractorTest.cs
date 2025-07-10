using System.IO;
using System.Linq;
using CargoWise.Types;
using Enterprise.DocumentEngine.DataProviders.BOFunctionExtractors.Core.Testing;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentEngine.Testing;
using Enterprise.MasterFiles.Business.CustomValues;

namespace Enterprise.DocumentEngine.DataProviders.BOFunctionExtractors.Testing
{
	sealed class GetCustomFieldFunctionExtractorTest : BaseFunctionExtractorTest
	{
		public void TestGetCustomFieldOnChildrenOfCollection()
		{
			var dummy = Factory.New<DummyDocumentSupportable>();
			dummy.SetUserDefinedValue("MyCustomField", (ZString)"Parent");

			var child1 = dummy.Collection.AddNew();
			child1.Z0_VarCharMax = "Child 1 Text";
			child1.SetUserDefinedValue("MyCustomField", (ZString)"Child 1 Custom Field");

			var child2 = dummy.Collection.AddNew();
			child2.Z0_VarCharMax = "Child 2 Text";
			child2.SetUserDefinedValue("MyCustomField", (ZString)"Child 2 Custom Field");

			var documentCommand = Factory.New<DocumentCommand>();
			documentCommand.Parent = dummy;

			var template = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "Test",
@"{A}-[#Config]
{A}-[Name=Test]
{A}-[DataContext=UnitTest]
{A}-[#SectionBody]
{B}-[<GetCustomField(MyCustomField)>]
{A}-[#SectionBody:Data=Collection]
{B}-[<Collection.Text>]
{B}-[<Collection.GetCustomField(MyCustomField)>]
{B}-[<Collection.GetCustomFieldWithType(MyCustomField, STR)>]
{A}-[#EndOfReport]");
			template.SO_DataContext = "UnitTest";

			var expected =
@"{B}-[Parent]
{B}-[Child 1 Text]
{B}-[Child 1 Custom Field]
{B}-[Child 1 Custom Field]
{B}-[Child 2 Text]
{B}-[Child 2 Custom Field]
{B}-[Child 2 Custom Field]
";

			var document = documentCommand.Documents.AddNew();
			document.SI_SU = documentCommand.PK;
			document.SI_SO = template.PK;

			using (var documentPack = new DocumentPack(documentCommand, dummy, null, null))
			{
				var report = documentPack.GetFirstReport();
				using (var stream = new MemoryStream())
				{
					report.Save(stream);

					using (var excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(stream);

						AssertMultilineASCIIEquals("Custom fields on children of collection should work.", expected, excelInterface.WorkSheets[0].ToString());
					}
				}
			}
		}

		public void TestGetCustomFieldWithDifferentCultureFormat()
		{
			var dummy = Factory.New<DummyDocumentSupportable>();
			dummy.SetUserDefinedValue("MyCustomField", new ZDateTime(2014, 4, 1));

			var child1 = dummy.Collection.AddNew();
			child1.Z0_VarCharMax = "Child 1 Bool";
			child1.SetUserDefinedValue("MyCustomField", ZBool.True);

			var child2 = dummy.Collection.AddNew();
			child2.Z0_VarCharMax = "Child 2 Decimal";
			child2.SetUserDefinedValue("MyCustomField", (ZDecimal)22.22);

			var child3 = dummy.Collection.AddNew();
			child3.Z0_VarCharMax = "Child 3 Int";
			child3.SetUserDefinedValue("MyCustomField", (ZInt)78);

			var child4 = dummy.Collection.AddNew();
			child4.Z0_VarCharMax = "Child 4 String";
			child4.SetUserDefinedValue("MyCustomField", (ZString)"Blah");

			var documentCommand = Factory.New<DocumentCommand>();
			documentCommand.Parent = dummy;

			var template = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "Test",
@"{A}-[#Config]
{A}-[Name=Test]
{A}-[DataContext=UnitTest]
{A}-[#SectionBody]
{B}-[<GetCustomField(MyCustomField)>]
{A}-[#SectionBody:Data=Collection]
{B}-[<Collection.Text>]
{B}-[<Collection.GetCustomField(MyCustomField)>]
{B}-[<Collection.GetCustomFieldWithType(MyCustomField, INT)>]
{A}-[#EndOfReport]");
			template.SO_DataContext = "UnitTest";

			using (var excelInterface = new ExcelInterface())
			{
				excelInterface.LoadExcelFile(template.SO_Template);
				var workSheet = excelInterface.WorkSheets.First();
				workSheet.SetCellFormat(4, 1, new DocumentEngineIntegration.CellFormat() { FormatPattern = "d/mm/yyyy h:mm:ss AM/PM" });

				using (var stream = new MemoryStream())
				{
					excelInterface.SaveToStream(stream);
					template.SO_Template = stream.CopyToByteArray();
				}
			}

			// Note that only the int will appear twice 
			var expectedResult =
@"{B}-[1/04/2014 12:00:00 AM]
{B}-[Child 1 Bool]
{B}-[Y]
{B}-[Child 2 Decimal]
{B}-[22.22]
{B}-[Child 3 Int]
{B}-[78]
{B}-[78]
{B}-[Child 4 String]
{B}-[Blah]
";

			var document = documentCommand.Documents.AddNew();
			document.SI_SU = documentCommand.PK;
			document.SI_SO = template.PK;

			AssertDocumentOutput(dummy, documentCommand, expectedResult, Enterprise.Core.Constants.Languages.English);
			AssertDocumentOutput(dummy, documentCommand, expectedResult, Enterprise.Core.Constants.Languages.French);
		}

		void AssertDocumentOutput(DummyDocumentSupportable dummy, DocumentCommand documentCommand, string expectedOutput, string outputLanguage)
		{
			using (var documentPack = new DocumentPack(documentCommand, dummy, null, null))
			{
				documentPack.Language = outputLanguage;
				var report = documentPack.GetFirstReport();
				using (var stream = new MemoryStream())
				{
					report.Save(stream);// should not throw format exception here

					using (var excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(stream);

						AssertMultilineASCIIEquals("Wrong ouptut", expectedOutput, excelInterface.WorkSheets[0].ToString(new CellFormatterUsingFormattedValue()));
					}
				}
			}
		}

		public override void TestGetMethodInfoChainLink()
		{
			var dummy = Factory.New<DummyDocumentSupportable>();
			dummy.SetUserDefinedValue("Test1", (ZString)"Hello");

			var extractor = new GetCustomFieldFunctionExtractor("Test1");
			var chainLink = extractor.GetMethodInfoChainLink(dummy.GetType());

			var extractorWithType = new GetCustomFieldWithTypeFunctionExtractor("Test1");
			var chainLinkWithType = extractorWithType.GetMethodInfoChainLink(dummy.GetType());

			AssertEquals("chainLink.ReflectOutObject(dummy, dummy)", "Hello", chainLink.ReflectOutObject(dummy, dummy));
			AssertEquals("chainLink.ReflectOutObject(dummy, dummy)", "Hello", chainLinkWithType.ReflectOutObject(dummy, dummy));
		}
	}
}
