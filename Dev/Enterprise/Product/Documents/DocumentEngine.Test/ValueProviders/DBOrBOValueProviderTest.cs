using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentEngine.Testing;
using Enterprise.DocumentEngine.Testing.UtilityClasses;
using Enterprise.DocumentEngine.ValueProviders.Testing;
using Enterprise.DocumentEngine.Visualisation;
using Enterprise.DocumentEngine.Visualisation.Testing;
using Enterprise.DocumentEngineCore;
using Enterprise.DocumentEngineCore.DocumentParsing;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.ValueReplacers.Testing
{
	[TestedType(typeof(DBOrBOValueProvider))]
	sealed class DBOrBOValueProviderTest : ValueProviderTest
	{
		public void TestReplacementForZBlob()
		{
			var factory = new BusinessObjectFactory();
			var template = ConfigurableTemplateTestHelper.SetupSystemTemplateFromString(factory,
@"{A}-[#Config]
{A}-[DataContext=.DummyBODocSupportable]
{A}-[Name=Test]
{A}-[#SectionBody]
{B}-[<Z0_VarBinaryMax>]
{A}-[#EndOfReport]");
			template.SO_DataContext = ".DummyBODocSupportable";

			var dummy = factory.New<DummyBODocSupportable>();
			dummy.Z0_VarBinaryMax = new ZBlob(new System.Text.UTF8Encoding().GetBytes("Hello World"));

			var documentCommand = factory.New<DocumentCommand>();
			documentCommand.Parent = dummy;

			var document = documentCommand.Documents.AddNew();
			document.SI_SO = template.PK;
			document.SI_SU = documentCommand.PK;
			document.DocConfigs.AddNew();

			var printJobs = DeliveryTestHelper.DeliverDocument(documentCommand);

			using (var excelInterface = new ExcelInterface(printJobs[0].SP_CustomProperties))
			{
				AssertEquals("{B}-[Hello World]", excelInterface.WorkSheets[0].ToString());
			}
		}

		public void TestIsResponsibleForReplacing()
		{
			Assert(!ValueProviderToTest.IsResponsibleForReplacing("<>", Passes.FirstPass));
			Assert(ValueProviderToTest.IsResponsibleForReplacing("< tbl . col >", Passes.FirstPass));
			Assert(!ValueProviderToTest.IsResponsibleForReplacing(" <.col>", Passes.FirstPass));
			Assert(!ValueProviderToTest.IsResponsibleForReplacing("tbl.tbl.col>", Passes.FirstPass));
			Assert(ValueProviderToTest.IsResponsibleForReplacing("<tbl. col >", Passes.FirstPass));
			Assert(ValueProviderToTest.IsResponsibleForReplacing("< tbl . col>", Passes.FirstPass));
		}

		public void TestReplacement()
		{
			PrepareRenderer();
			Report.Renderer.CurrentAreaToProcess = null;
			AssertEquals("Header1                                 ", ValueProviderToTest.GetReplacement("<Header.Number>", Report));
			AssertEquals("Header1                                 ", ValueProviderToTest.GetReplacement("<  Header.Number>", Report));
			AssertEquals("Header1                                 ", ValueProviderToTest.GetReplacement("<Header.Number  >", Report));
		}

		public void TestReplacementWithCompression()
		{
			PrepareRenderer();
			Report.Renderer.CurrentAreaToProcess = null;
			AssertEquals("abc", ValueProviderToTest.GetReplacement("<Notes.CompressedImageField>", Report));
		}

		[GuiTest]
		public void TestInvalidDocumentWrapperParameterException()
		{
			using (Report.TemporarilyStopErrorsThrowingAnException())
			{
				using (var printTask = GetPrintTask_ForTestInvalidDocumentWrapperParameterException("<IDontThrow.Foo>", "<IDontThrow.Bar>"))
				{
					var report = printTask[0][0] as Report;
					Assert("Should log no errors", !report.ErrorManager.HasErrors);
				}

				using (var printTask = GetPrintTask_ForTestInvalidDocumentWrapperParameterException("<IThrowInConstructor.Foo>", "<IThrowInConstructor.Bar>"))
				{
					var report = printTask[0][0] as Report;
					AssertEquals("Should log two errors", @"Severity: [Warning (without error report)] Message: [Document Creation Error: blah] Cell: [B5]
Severity: [Warning (without error report)] Message: [Document Creation Error: blah] Cell: [B6]", report.ErrorManager.ToString("Severity: [{0}] Message: [{1}] Cell: [{2}]", false));
					report.ErrorManager.ClearErrors();
				}

				using (var printTask = GetPrintTask_ForTestInvalidDocumentWrapperParameterException("<MyFooPropertyThrows.Foo>", "<MyFooPropertyThrows.Bar>"))
				{
					var report = printTask[0][0] as Report;
					AssertEquals("Should log one error", "Severity: [Warning (without error report)] Message: [Document Creation Error: boo] Cell: [B5]", report.ErrorManager.ToString("Severity: [{0}] Message: [{1}] Cell: [{2}]", false));
					report.ErrorManager.ClearErrors();
				}
			}
		}

		[GuiTest]
		public void TestDocumentTypeConversionFailedException()
		{
			using (Report.TemporarilyStopErrorsThrowingAnException())
			{
				using (var printTask = GetPrintTask_ForDocumentTypeConversionFailedException("<IThrow.Bar>"))
				{
					var report = printTask[0][0] as Report;
					AssertEquals("Should log conversion error", @"Severity: [Warning (without error report)] Message: [Conversion Error: 'somevalue' template constant value cannot be converted to target type 'CargoWise.Types.ZInt'. Actual value is '<blank>'] Cell: [B5]", report.ErrorManager.ToString("Severity: [{0}] Message: [{1}] Cell: [{2}]", false));
					report.ErrorManager.ClearErrors();
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestNonBooleanFilterStatementReplacement()
		{
			var dataItem = (BusinessObject)Factory.New<Enterprise.Integration.TransportBooking.IDtbBooking>();
			var instruction = Factory.New<Enterprise.Integration.TransportBooking.IDtbBookingInstruction>();
			instruction.KN_KM_BookingMovement = dataItem.PK;
			var wrapper = DocumentWrapperFactory.GenerateGenericWrappers(Core.Constants.DataContext.GenericFreightJob, dataItem);
			var dataProviders = new DataProviderList(wrapper);

			var template = new ExcelTemplateForUnitTesting("InvalidFormatTemplate.xls", TestFilesSubFolder.ReportTestFiles);
			using (var report = new Report(Pack, template, dataProviders, "name", null, DocumentDirection.ANY, false))
			{
				string booleanFilter = "ThisIsNotABooleanStatement";
				PrepareRenderer(report);
				report.Renderer.CurrentAreaToProcess = new RendererSectionBodyTest.DummyArea(report);
				AssertNoExceptionThrown(() => ValueProviderToTest.GetReplacement("<BookingInstructions.Format(\"{ContainerNo}\", Comma, " + booleanFilter + ")>", report));
				Assert(report.ErrorManager.HasErrors);
				AssertEquals("Severity: [Warning (without error report)] Message: [" + $"Collection Formatting Error: {($"Unable to format collection as {booleanFilter} is not a valid filter statement.")}" + "] Cell: [N/A] Sheetname: [(unknown)]",
					report.ErrorManager.ToString());

				report.ErrorManager.ClearErrors();
				booleanFilter = "\"CNY\"!=\"USD\" && \"CNY!=\"EUR\"";
				AssertNoExceptionThrown(() => ValueProviderToTest.GetReplacement("<BookingInstructions.Format(\"{ContainerNo}\", Comma, " + booleanFilter + " )>", report));
				Assert(report.ErrorManager.HasErrors);
				AssertEquals("Severity: [Warning (without error report)] Message: [" + $"Collection Formatting Error: {($"Unable to format collection as {booleanFilter} is not a valid filter statement.")}" +	"] Cell: [N/A] Sheetname: [(unknown)]",
					report.ErrorManager.ToString());
			}
		}

		public void TestNonBreakingSpacesAreReplacedWithClassicSpaces()
		{
			using (Env.CurrentCompany.Country.SetCultureForTest(Culture.GetCulture(Core.Constants.CountryCodes.Poland)))
			{
				var stringWithNonBreakingSpaces = ZString.Format("1{0}234,56", (char)160);
				var stringWithBreakingSpaces = ZString.Format("1{0}234,56", (char)32);
				AssertNonBreakingSpacesAreReplacedWithClassicSpacesWhenRequired(stringWithNonBreakingSpaces, "Amount should contain non-breaking spaces", "Arial", stringWithNonBreakingSpaces);
				AssertNonBreakingSpacesAreReplacedWithClassicSpacesWhenRequired(stringWithNonBreakingSpaces, "Amount should contain classic spaces", "Lucida Console", stringWithBreakingSpaces);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestInvalidFindException()
		{
			var dataItem = (BusinessObject)Factory.New<Enterprise.Integration.TransportBooking.IDtbBooking>();
			var instruction = Factory.New<Enterprise.Integration.TransportBooking.IDtbBookingInstruction>();
			instruction.KN_KM_BookingMovement = dataItem.PK;
			var wrapper = DocumentWrapperFactory.GenerateGenericWrappers(Core.Constants.DataContext.GenericFreightJob, dataItem);
			var dataProviders = new DataProviderList(wrapper);

			var template = new ExcelTemplateForUnitTesting("InvalidFormatTemplate.xls", TestFilesSubFolder.ReportTestFiles);
			using (var report = new Report(Pack, template, dataProviders, "name", null, DocumentDirection.ANY, false))
			{
				const string filter = "<BookingInstructions.Find(";
				const string macro = "{P9_Description}\" == \"First Letter Delivered\" && \"";
				PrepareRenderer(report);
				report.Renderer.CurrentAreaToProcess = new RendererSectionBodyTest.DummyArea(report);
				ValueProviderToTest.GetReplacement(filter + macro + ")>", report);
				AssertEquals("Severity: [Warning (without error report)] Message: [" + $"Find Evaluation Error: {(string.Format("Could not evaluate the following match: {0}", macro))}" + "] Cell: [N/A] Sheetname: [(unknown)]",
					report.ErrorManager.ToString());
			}
		}

		[GuiTest]
		public void TestRtfToTextMissingContentException()
		{
			using (Report.TemporarilyStopErrorsThrowingAnException())
			{
				using (var printTask = GetPrintTask_ForTestRtfToTextMissingContentException("{\\rtf1\\ansi\\ansicpg1252\\deff0\\deflang3081{\\fonttbl{\\f0\\fnil\\fcharset0 Microsoft Sans Serif;}}\r\n\\viewkind4\\uc1\\pard\\f0\\fs17 x{\\v\\insrsid2294299\\charrsid2294299  hidden}\\par\r\n}  \r\n\0", isSystemDefined: true))
				{
					var report = printTask[0][0] as Report;
					Assert("Should log no errors", !report.ErrorManager.HasErrors);
				}

				var rftMissingContent = @"{\rtf1\ansi\ansicpg1252\deff0\nouicompat\deflang3081{\fonttbl{\f0\fnil\fcharset0 Microsoft Sans Serif;}}
{\colortbl ;\red0\green0\blue255;}
{\*\generator Riched20 10.0.20348}\viewkind4\uc1 
\pard\f0\fs20 Submitted proposal to Dilanjan. Will wait for his feedback.\par
\par
{\fs24{\field{\*\fldinst{H";
				var reportErrorMessage = "RTF text parsing error, please check that the relevant data is defined correctly";

				using (var printTask = GetPrintTask_ForTestRtfToTextMissingContentException(rftMissingContent, isSystemDefined: false))
				{
					var report = printTask[0][0] as Report;
					AssertEquals("Should log error(without error report)", string.Format("Severity: [Error (without error report)] Message: [{0}] Cell: [B5]", reportErrorMessage), report.ErrorManager.ToString("Severity: [{0}] Message: [{1}] Cell: [{2}]", false));
					ErrorReporter.Clear();
					report.ErrorManager.ClearErrors();
				}

				using (var printTask = GetPrintTask_ForTestRtfToTextMissingContentException(rftMissingContent, isSystemDefined: true))
				{
					var report = printTask[0][0] as Report;
					AssertEquals("Should log error", string.Format("Severity: [Error] Message: [{0}] Cell: [B5]", reportErrorMessage), report.ErrorManager.ToString("Severity: [{0}] Message: [{1}] Cell: [{2}]", false));
					ErrorReporter.Clear();
					report.ErrorManager.ClearErrors();
				}
			}
		}

		PrintTask GetPrintTask_ForTestRtfToTextMissingContentException(string rtf, bool isSystemDefined)
		{
			var factory = new BusinessObjectFactory();
			var template = ConfigurableTemplateTestHelper.SetupSystemTemplateFromString(factory,
@"{A}-[#Config]
{A}-[DataContext=.DummyBODocSupportableThrowing]
{A}-[Name=Test]
{A}-[#SectionBody]
{B}-[<Z0_VarBinaryMax>]
{A}-[#EndOfReport]");
			template.SO_IsSystemDefined = isSystemDefined;

			var dummy = factory.New<DummyBODocSupportableTypeConversion>();
			dummy.Z0_VarBinaryMax = new ZBlob(Compressor.Compress(new System.Text.UTF8Encoding().GetBytes(rtf)));
			var documentCommand = factory.New<DocumentCommand>();
			documentCommand.Parent = dummy;
			documentCommand.SU_IsSystemDefined = isSystemDefined;

			var document = documentCommand.Documents.AddNew();
			document.SI_SO = template.PK;
			document.SI_SU = documentCommand.PK;
			document.DocConfigs.AddNew();
			document.SI_IsSystemDefined = isSystemDefined;

			var deliveryInstructions = new DeliveryInstructions();
			deliveryInstructions.Language = Enterprise.Core.Constants.Languages.English;
			deliveryInstructions.Destination = DeliveryInstructionDestination.TakenFromContact;
			deliveryInstructions.Recipients.RemoveAndDeleteAll();

			var recipient = deliveryInstructions.Recipients.AddNew();
			recipient.DeliveryMethod = Enterprise.Core.Constants.ContactNotifyModes.Email;
			recipient.Email = "blah@bah.com";
			recipient.AttachmentType = OrgConstants.AttachmentType.XLS;

			var printTask = new DocumentPrintSet(documentCommand, null);
			AssertNoExceptionThrown(() => printTask.Run(deliveryInstructions));
			return printTask;
		}

		public override void TestTemplateVisualiserComponentType()
		{
			AssertEquals(VisualiserComponentTypes.TextEdit, GetNewValueProvider().ComponentType);
		}

		public override void TestExistsInValueProviderCollection()
		{
			Assert("This is a base class for other ValueProviders", true);
		}

		protected override ValueProvider GetNewValueProvider() => new DBOrBOValueProvider();

		protected override void PrepareDataForExamplesEvaluate()
		{
			PrepareRenderer();
			Report.Renderer.CurrentAreaToProcess = null;
		}

		PrintTask GetPrintTask_ForTestInvalidDocumentWrapperParameterException(string macro1, string macro2)
		{
			var factory = new BusinessObjectFactory();
			var template = ConfigurableTemplateTestHelper.SetupSystemTemplateFromString(factory,
@"{A}-[#Config]
{A}-[DataContext=.DummyBODocSupportableThrowing]
{A}-[Name=Test]
{A}-[#SectionBody]
{B}-[" + macro1 + @"]
{B}-[" + macro2 + @"]
{A}-[#EndOfReport]");

			var dummy = factory.New<DummyBODocSupportableThrowing>();
			var documentCommand = factory.New<DocumentCommand>();
			documentCommand.Parent = dummy;

			var document = documentCommand.Documents.AddNew();
			document.SI_SO = template.PK;
			document.SI_SU = documentCommand.PK;
			document.DocConfigs.AddNew();

			var deliveryInstructions = new DeliveryInstructions();
			deliveryInstructions.Language = Enterprise.Core.Constants.Languages.English;
			deliveryInstructions.Destination = DeliveryInstructionDestination.TakenFromContact;
			deliveryInstructions.Recipients.RemoveAndDeleteAll();

			var recipient = deliveryInstructions.Recipients.AddNew();
			recipient.DeliveryMethod = Enterprise.Core.Constants.ContactNotifyModes.Email;
			recipient.Email = "blah@bah.com";
			recipient.AttachmentType = OrgConstants.AttachmentType.XLS;

			var printTask = new DocumentPrintSet(documentCommand, null);
			printTask.Run(deliveryInstructions);

			return printTask;
		}

		PrintTask GetPrintTask_ForDocumentTypeConversionFailedException(string macro1)
		{
			var factory = new BusinessObjectFactory();
			var template = ConfigurableTemplateTestHelper.SetupSystemTemplateFromString(factory,
@"{A}-[#Config]
{A}-[DataContext=.DummyBODocSupportableThrowing]
{A}-[Name=Test]
{A}-[#SectionBody]
{B}-[" + macro1 + @"]
{A}-[#EndOfReport]");

			var dummy = factory.New<DummyBODocSupportableTypeConversion>();
			var documentCommand = factory.New<DocumentCommand>();
			documentCommand.Parent = dummy;

			var document = documentCommand.Documents.AddNew();
			document.SI_SO = template.PK;
			document.SI_SU = documentCommand.PK;
			document.DocConfigs.AddNew();

			var deliveryInstructions = new DeliveryInstructions();
			deliveryInstructions.Language = Enterprise.Core.Constants.Languages.English;
			deliveryInstructions.Destination = DeliveryInstructionDestination.TakenFromContact;
			deliveryInstructions.Recipients.RemoveAndDeleteAll();

			var recipient = deliveryInstructions.Recipients.AddNew();
			recipient.DeliveryMethod = Enterprise.Core.Constants.ContactNotifyModes.Email;
			recipient.Email = "blah@bah.com";
			recipient.AttachmentType = OrgConstants.AttachmentType.XLS;

			var printTask = new DocumentPrintSet(documentCommand, null);
			AssertNoExceptionThrown(() => printTask.Run(deliveryInstructions));
			return printTask;
		}

		void AssertNonBreakingSpacesAreReplacedWithClassicSpacesWhenRequired(ZString amount, string assertComment, string font, string expectedResult)
		{
			var template = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "Test",
@"{A}-[#Config]
{A}-[Name=Test]
{A}-[DataContext=UnitTest]
{A}-[#SectionBody:Data=Collection]
{B}-[<Collection.Text>]
{A}-[#EndOfReport]", "UnitTest");

			using (var excelInterface = new ExcelInterface())
			{
				excelInterface.LoadExcelFile(template.SO_Template);
				var workSheet = excelInterface.WorkSheets.First();
				var format = workSheet.GetCellFormat(4, 1);
				format.FontName = font;
				workSheet.SetCellFormat(4, 1, format);

				using (var stream = new MemoryStream())
				{
					excelInterface.SaveToStream(stream);
					template.SO_Template = stream.CopyToByteArray();
				}
			}

			var dummy = Factory.New<DummyDocumentSupportable>();
			var child = dummy.Collection.AddNew();
			child.Z0_VarCharMax = amount;

			var documentCommand = Factory.New<DocumentCommand>();
			documentCommand.Parent = dummy;

			var document = documentCommand.Documents.AddNew();
			document.SI_SU = documentCommand.PK;
			document.SI_SO = template.PK;

			var printJob = DeliveryTestHelper.DeliverDocument(documentCommand).First();

			using (var excelInterface = new ExcelInterface(printJob.SP_CustomProperties))
			{
				var workSheet = excelInterface.WorkSheets.First();
				AssertMultilineASCIIEquals(assertComment, string.Format("{{B}}-[{0}]", expectedResult), workSheet.ToString());
			}
		}

		sealed class DummyBODocSupportableThrowing : DummyBODocSupportable
		{
			public DummyBODocSupportableThrowing(BusinessObjectFactory factory, DataRow dataRow)
				: base(factory, dataRow)
			{
			}

			public override DocumentSupporter DocumentSupporter
			{
				get { return new DummyBODocSupportableDocumentSupporter_ForTestInvalidDocumentWrapperParameterException(this); }
			}
		}

		sealed class DummyBODocSupportableDocumentSupporter_ForTestInvalidDocumentWrapperParameterException : DummyBODocSupportableDocumentSupporter
		{
			public DummyBODocSupportableDocumentSupporter_ForTestInvalidDocumentWrapperParameterException(DummyBODocSupportable docDummyBusinessObject)
				: base(docDummyBusinessObject)
			{
			}

			protected override DocumentWrapper[] GetDocumentWrappersInternal(Core.Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
			{
				var result = new List<DocumentWrapper>();
				result.Add(new DocWrapper_ForTestInvalidDocumentWrapperParameterException());
				return result.ToArray();
			}
		}

		sealed class DocWrapper_ForTestInvalidDocumentWrapperParameterException : DocumentWrapper
		{
			public Dummy_ForTestInvalidDocumentWrapperParameterException IDontThrow
			{
				get { return new Dummy_ForTestInvalidDocumentWrapperParameterException("XXX", 10); }
			}

			public Dummy_ForTestInvalidDocumentWrapperParameterException IThrowInConstructor
			{
				get { return new Dummy_ForTestInvalidDocumentWrapperParameterException("XXX", -5); }
			}

			public Dummy_ForTestInvalidDocumentWrapperParameterException MyFooPropertyThrows
			{
				get { return new Dummy_ForTestInvalidDocumentWrapperParameterException("", 7); }
			}
		}

		sealed class Dummy_ForTestInvalidDocumentWrapperParameterException : NonPersistentBusinessObject
		{
			public Dummy_ForTestInvalidDocumentWrapperParameterException(ZString foo, ZDecimal bar)
			{
				this.foo = foo;
				if (bar <= 0)
				{
					throw new InvalidDocumentWrapperParameterException("blah");
				}
				this.bar = bar;
			}

			public ZString Foo
			{
				get
				{
					if (string.IsNullOrEmpty(foo))
					{
						throw new InvalidDocumentWrapperParameterException("boo");
					}
					return foo;
				}
			}
			readonly ZString foo;

			public ZDecimal Bar
			{
				get { return bar; }
			}
			readonly ZDecimal bar;
		}

		sealed class DummyBODocSupportableTypeConversion : DummyBODocSupportable
		{
			public DummyBODocSupportableTypeConversion(BusinessObjectFactory factory, DataRow dataRow)
				: base(factory, dataRow)
			{
			}

			public override DocumentSupporter DocumentSupporter
			{
				get { return new DummyBODocSupportableDocumentSupporter_ForTestDocumentTypeConversionFailedException(this); }
			}
		}

		sealed class DummyBODocSupportableDocumentSupporter_ForTestDocumentTypeConversionFailedException : DummyBODocSupportableDocumentSupporter
		{
			public DummyBODocSupportableDocumentSupporter_ForTestDocumentTypeConversionFailedException(DummyBODocSupportable docDummyBusinessObject)
				: base(docDummyBusinessObject)
			{
			}

			protected override DocumentWrapper[] GetDocumentWrappersInternal(Core.Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
			{
				var result = new List<DocumentWrapper>();
				result.Add(new DocWrapper_ForTestDocumentTypeConversionFailedException());
				return result.ToArray();
			}
		}

		sealed class DocWrapper_ForTestDocumentTypeConversionFailedException : DocumentWrapper
		{
			public Dummy_ForTestDocumentTypeConversionFailedException IThrow
			{
				get { return new Dummy_ForTestDocumentTypeConversionFailedException("somevalue", "<blank>"); }
			}
		}

		sealed class Dummy_ForTestDocumentTypeConversionFailedException : NonPersistentBusinessObject
		{
			public Dummy_ForTestDocumentTypeConversionFailedException(string foo, object bar)
			{
				var converter = System.ComponentModel.TypeDescriptor.GetConverter(typeof(ZInt));
				try
				{
					this.bar = (ZInt)converter.ConvertFrom(bar);
				}
				catch
				{
					var message = $"'{foo}' template constant value cannot be converted to target type '{typeof(ZInt).ToString()}'. Actual value is '{bar}'";
					throw new DocumentTypeConversionFailedException(message);
				}
			}

			public ZInt Bar
			{
				get { return bar; }
			}
			readonly ZInt bar;
		}
	}
}
