using System.IO;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentEngine.DataProviders.BOFunctionExtractors.Core.Testing;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentEngine.Testing;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.ExcelTemplates;
using Enterprise.Integration.DocumentEngine;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.DataProviders.BOFunctionExtractors.Testing
{
	sealed class FindFunctionExtractorTest : BaseFunctionExtractorTest
	{
		class BookCollection : NonPersistentBusinessObjectCollection<Book>
		{
			protected override BusinessObject CreateNonPersistentBusinessObject()
			{
				return new Book();
			}
		}

		class Person : NonPersistentBusinessObject
		{
			public ZString FirstName { get; set; }
			public ZString LastName { get; set; }
		}

		class Book : NonPersistentBusinessObject
		{
			public Person Author { get; set; }
		}

		class BookShelf : NonPersistentBusinessObject
		{
			public BookShelf()
			{
				this.books = new BookCollection();
			}

			readonly BookCollection books;

			public BookCollection Books
			{
				get { return books; }
			}
		}

		public void TestEvaluateFindFunctionUsingFieldWithDot()
		{
			var bookShelf = new BookShelf();

			var book = bookShelf.Books.AddNew();
			book.Author = new Person { FirstName = "Steve", LastName = "Jobs" };

			using (var documentPack = new DocumentPack())
			{
				using (var report = new Report(documentPack, null, BODocDataProvider.Get(bookShelf), "My Books", null, DocumentDirection.ANY, false))
				{
					var foo = new MacroTranslator(report);

					AssertEquals("Failed to evaluate a macro in a find function with a dot.", "Jobs", foo.GetValue(@"<Books.Find(""{Author.FirstName}"" == ""Steve"").Author.LastName>", Passes.FirstPass));
				}
			}
		}

		public void TestEndToEnd()
		{
			var factory = new BusinessObjectFactory();

			var dummy = factory.New<DummyBODocSupportable>();
			dummy.Z0_VarCharMax = "Text Parent";

			var element1 = dummy.Collection.AddNew();
			element1.Z0_Number = 1;
			element1.Z0_VarCharMax = "Text 1";

			var element2 = dummy.Collection.AddNew();
			element2.Z0_Number = 2;
			element2.Z0_VarCharMax = "Text 2";

			var element3 = dummy.Collection.AddNew();
			element3.Z0_Number = 3;
			element3.Z0_VarCharMax = "Text 3";

			var documentCommand = factory.New<DocumentCommand>();
			documentCommand.SU_IsSystemDefined = ZBool.True;
			documentCommand.Parent = dummy;

			var helper = new TemplateTestHelper();
			helper.AddWorkSheet("Document",
@"{A}-[#Config]
{A}-[DataContext=.DummyBODocSupportable]
{A}-[Name=Test]
{A}-[#SectionBody]
{B}-[<Z0_VarCharMax>]
{B}-[<Collection.Count>]
{B}-[<Collection[First].Z0_VarCharMax>]
{B}-[<Collection[1].Z0_VarCharMax>]
{B}-[<Collection[2].Z0_VarCharMax>]
{B}-[<Collection[3].Z0_VarCharMax>]
{B}-[<Collection[First].Format(""{Z0_Number}-{Z0_VarCharMax}"")>]
{B}-[<Collection.Total(""Z0_Number"")>]
{B}-[<Collection.Find({Z0_Number} == 1).Z0_VarCharMax>]
{B}-[<Collection.Find({Z0_Number} == 2).Z0_VarCharMax>]
{B}-[<Collection.Find({Z0_Number} == 3).Z0_VarCharMax>]
{B}-[<Collection.Find({Z0_Number} == 3).Format(""Number: {Z0_Number}    Text: {Z0_VarCharMax}"")>]
{A}-[#EndOfReport]");

			var template = helper.CreateTemplate(factory, "Test");
			template.SO_DataContext = ".DummyBODocSupportable";

			var document = documentCommand.Documents.AddNew();
			document.SI_SU = documentCommand.PK;
			document.SI_SO = template.PK;

			var printJobs = DeliveryTestHelper.DeliverDocument(documentCommand);
			AssertEquals("printJobs.Length", 1, printJobs.Length);

			using (var excelInterface = new ExcelInterface())
			{
				excelInterface.LoadExcelFile(printJobs[0].SP_CustomProperties);

				AssertMultilineASCIIEquals("",
@"{B}-[Text Parent]
{B}-[3]
{B}-[Text 1]
{B}-[Text 1]
{B}-[Text 2]
{B}-[Text 3]
{B}-[1-Text 1]
{B}-[6]
{B}-[Text 1]
{B}-[Text 2]
{B}-[Text 3]
{B}-[Number: 3    Text: Text 3]",
					excelInterface.WorkSheets[0].ToString());
			}
		}

		[ExpectExceptionMessage(typeof(DataProviderException), "The Find function only works on collections. Please check your Find macro and make sure it is used on a collection.")]
		public void TestFind_OnNonBizoCollection_ShouldThrowDataProviderException()
		{
			var bookShelf = new BookShelf();
			var book = bookShelf.Books.AddNew();
			book.Author = new Person { FirstName = "Dexter", LastName = "Morgan" };

			using (var documentPack = new DocumentPack())
			using (var report = new Report(documentPack, null, BODocDataProvider.Get(bookShelf), "My Books", null, DocumentDirection.ANY, false))
			{
				var foo = new MacroTranslator(report);
				foo.GetValue(@"<Books.Author.Find(""{FirstName}"" == ""Dexter"").LastName>", Passes.FirstPass);
			}
		}

		[ExpectNoExceptions]
		public void TestEndToEnd_OnNonBizoCollection_SimpleMAcroEvaluation_NoExceptionThrown()
		{
			var bookShelf = new BookShelf();
			var book = bookShelf.Books.AddNew();
			book.Author = new Person { FirstName = "Dexter", LastName = "Morgan" };
			var textMacroProcessor = ObjectFactory.Get<ITextMacroProcessor>();
			textMacroProcessor.Replace(@"<Books.Author.Find(""{FirstName}"" == ""Dexter"").LastName>", new[] { bookShelf });

			AssertEquals(
				@"Cannot evaluate <Books.Author.Find(""{FirstName}"" == ""Dexter"").LastName>. The Find function only works on collections. Please check your Find macro and make sure it is used on a collection.",
				textMacroProcessor.ReportErrors.First().Message);
		}

		[ExpectNoExceptions]
		public void TestEndToEnd_OnNonBizoCollection_InTemplate_NoExceptionThrown()
		{
			var factory = new BusinessObjectFactory();

			var dummy = factory.New<DummyBODocSupportable>();
			dummy.Z0_VarCharMax = "Text Parent";

			var element1 = dummy.Collection.AddNew();
			element1.Z0_Number = 1;
			element1.Z0_VarCharMax = "Text 1";

			var element2 = dummy.Collection.AddNew();
			element2.Z0_Number = 2;
			element2.Z0_VarCharMax = "Text 2";

			var element3 = dummy.Collection.AddNew();
			element3.Z0_Number = 3;
			element3.Z0_VarCharMax = "Text 3";

			var documentCommand = factory.New<DocumentCommand>();
			documentCommand.SU_IsSystemDefined = ZBool.True;
			documentCommand.Parent = dummy;

			using (var templateStream = new MemoryStream())
			{
				using (var creationExcelInterface = new ExcelInterface())
				{
					creationExcelInterface.NewExcelFile(1);
					ExcelWorkSheet workSheet = creationExcelInterface.WorkSheets[0];
					workSheet[0, 0] = "#config";
					workSheet[1, 0] = "DataContext=.DummyBODocSupportable";
					workSheet[2, 0] = "Name=Test";
					workSheet[3, 0] = "#SectionBody";
					workSheet[4, 1] = "<Collection[2].Find({Z0_Number} == 2).Z0_VarCharMax>";
					workSheet[5, 0] = "#EndOfReport";
					creationExcelInterface.SaveToStream(templateStream);
				}

				var excelTemplate = new ExcelTemplateWrappingStream("TemplateFromStream", templateStream);
				using (var report = new Report(new DocumentPack(Factory.New<DocumentCommand>()), excelTemplate, BODocDataProvider.Get(dummy), "Test_" + excelTemplate.TemplateName, null, DocumentDirection.ANY, false))
				using (var outputStream = new MemoryStream())
				{
					((IReportForUnitTesting)report).GenerateRegardlessOfAnyErrors = true;
					AssertNoExceptionThrown(() => report.Save(outputStream));
					AssertEquals("Report.Errors", @"
Severity: [Fatal Error (without error report)] Message: [The report could not be generated. Please ensure that the report is being run in the correct company. Error details:
The Find function only works on collections. Please check your Find macro and make sure it is used on a collection.]".Trim(), report.ErrorManager.ToString("Severity: [{0}] Message: [{1}]", false));
				}
			}
		}

		public override void TestGetMethodInfoChainLink()
		{
			var collection = new DummyBusinessObjectCollection(Factory);

			var element1 = collection.AddNew();
			element1.Z0_Number = 1;
			element1.Z0_VarCharMax = "Text 1";

			var element2 = collection.AddNew();
			element2.Z0_Number = 2;
			element2.Z0_VarCharMax = "Text 2";

			var extractor = new FindFunctionExtractor("{Z0_Number} == 1");
			var chainLink = extractor.GetMethodInfoChainLink(collection.GetType());
			AssertEquals("chainLink.ReflectOutObject(collection, collection)", element1, chainLink.ReflectOutObject(collection, collection));
			AssertEquals("chainLink.TypeToReflect", typeof(DummyBusinessObject), chainLink.TypeToReflect);

			extractor = new FindFunctionExtractor("{Z0_Number} == 2");
			chainLink = extractor.GetMethodInfoChainLink(collection.GetType());
			AssertEquals("chainLink.ReflectOutObject(collection, collection)", element2, chainLink.ReflectOutObject(collection, collection));

			extractor = new FindFunctionExtractor("{Z0_Number} == 3");
			chainLink = extractor.GetMethodInfoChainLink(collection.GetType());
			AssertEquals("chainLink.ReflectOutObject(collection, collection)", null, chainLink.ReflectOutObject(collection, collection));
		}

		public void TestGetMethodInfoChainLinkForIBODocDataProviderCollection()
		{
			var collection = new DummyBusinessObjectCollection(Factory);

			var element1 = collection.AddNew();
			element1.Z0_Number = 1;
			element1.Z0_VarCharMax = "Text 1";

			var element2 = collection.AddNew();
			element2.Z0_Number = 2;
			element2.Z0_VarCharMax = "Text 2";

			var boDocDataProviderCollection = new DummyBODocDataProviderCollection(collection);

			var extractor = new FindFunctionExtractor("{Z0_Number} == 1");
			var chainLink = extractor.GetMethodInfoChainLink(boDocDataProviderCollection.GetType());
			AssertEquals("chainLink.ReflectOutObject(boDocDataProviderCollection, boDocDataProviderCollection)", element1, chainLink.ReflectOutObject(boDocDataProviderCollection, boDocDataProviderCollection));

			extractor = new FindFunctionExtractor("{Z0_Number} == 2");
			chainLink = extractor.GetMethodInfoChainLink(boDocDataProviderCollection.GetType());
			AssertEquals("chainLink.ReflectOutObject(boDocDataProviderCollection, boDocDataProviderCollection)", element2, chainLink.ReflectOutObject(boDocDataProviderCollection, boDocDataProviderCollection));

			extractor = new FindFunctionExtractor("{Z0_Number} == 3");
			chainLink = extractor.GetMethodInfoChainLink(boDocDataProviderCollection.GetType());
			AssertEquals("chainLink.ReflectOutObject(boDocDataProviderCollection, boDocDataProviderCollection)", null, chainLink.ReflectOutObject(boDocDataProviderCollection, boDocDataProviderCollection));
		}

		#region implementation

		class DummyBODocDataProviderCollection : IBODocDataProviderCollection
		{
			internal DummyBODocDataProviderCollection(DummyBusinessObjectCollection collection)
			{
				this.helper = new BODocDataProviderCollectionHelper(collection);
			}

			readonly BODocDataProviderCollectionHelper helper;

			public int Count
			{
				get { return helper.Count; }
			}

			public BusinessObject Find(ZString match)
			{
				return helper.Find(match);
			}

			public ZString Format(ZString formatString, ZString delimiter, ZString filterString, ZString groupByParameters, ZInt maxItems)
			{
				return helper.Format(formatString, delimiter, filterString, groupByParameters, maxItems);
			}

			public object Total(ZString fieldName, ZString decimalPlaces, ZString filter)
			{
				return helper.Total(fieldName, decimalPlaces, filter);
			}

			public IBODocDataProvider this[int index]
			{
				get { return helper[index]; }
			}

			public IBODocDataProvider this[string index]
			{
				get { return helper[index]; }
			}
		}

		#endregion
	}
}
