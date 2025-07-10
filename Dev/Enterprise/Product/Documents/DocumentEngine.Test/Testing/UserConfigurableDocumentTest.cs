using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentEngine.Business;
using Enterprise.DocumentEngine.DocBuilder;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentEngine.MacroValueProviders;
using Enterprise.DocumentEngine.Testing.UtilityClasses;
using Enterprise.Environment;
using Enterprise.ExcelTemplates;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.Testing
{
	sealed class UserConfigurableDocumentTest : TestCaseWithFactory
	{
		public void TestNoRedundantSectionsAreUsedInSystemConfigurations()
		{
			var redundantFields = new Dictionary<string, string> { { "Transport Info + ETD + ETA", "Shipment Routes" } };

			var groupedErrors = new GroupedErrorList();
			var query = new ZQuery(StmTemplateSchema.SO_IsSystemDefined, true);
			query.AddToFilter(StmTemplateSchema.SO_Name, SectionRepositoryTemplateNames.System);

			var template = Factory.LoadTop1<StmTemplateBase>(query);

			var configs = Factory.Load<StmMenuDocumentConfig>(new ZQuery(StmMenuDocumentConfigSchema.S3_IsSystem, true));
			foreach (var config in configs)
			{
				var menuItem = config.MenuTemplatePivot.MenuItem;
				var group = string.Format("{0} ({1})",
					menuItem.SU_MenuName,
					menuItem.SU_BusinessContext);

				foreach (StmMenuDocumentConfigItem configItem in config.ConfigItems)
				{
					if (redundantFields.TryGetValue(configItem.S4_SectionItemName, out var replacementSection))
					{
						var error = string.Format("{0}. [{1}] should not be used as it is redundant. Use [{2}] instead.",
							configItem.S4_PrintOrder,
							configItem.S4_SectionItemName,
							replacementSection);

						groupedErrors.Add(group, error);
					}
				}
			}

			groupedErrors.Assert("The following StmMenuDocumentConfigs were found with items that use sections that are redundant:");
		}

		public void TestSectionsUsedInDocBuilderDocumentsAllExistInTheSystemSectionRepository()
		{
			var groupedErrors = new GroupedErrorList();
			var query = new ZQuery(StmTemplateSchema.SO_IsSystemDefined, true);
			query.AddToFilter(StmTemplateSchema.SO_Name, SectionRepositoryTemplateNames.System);

			var template = Factory.LoadTop1<StmTemplateBase>(query);

			var configs = Factory.Load<StmMenuDocumentConfig>(new ZQuery(StmMenuDocumentConfigSchema.S3_IsSystem, true));
			foreach (var config in configs)
			{
				var menuItem = config.MenuTemplatePivot.MenuItem;
				var group = string.Format("{0} ({1})",
					menuItem.SU_MenuName,
					menuItem.SU_BusinessContext);

				foreach (StmMenuDocumentConfigItem configItem in config.ConfigItems)
				{
					if (!template.TemplateSections.Contains(configItem.S4_SectionItemName))
					{
						var error = string.Format("{0}. {1}",
							configItem.S4_PrintOrder,
							configItem.S4_SectionItemName);

						groupedErrors.Add(group, error);
					}
				}
			}

			groupedErrors.Assert("The following StmMenuDocumentConfigs were found with items that use sections that no longer exist:");
		}

		[TestDate(2008, 6, 2)]
		public void TestDateInFrenchLanguage()
		{
			var dummy = Factory.New<DummyBODocSupportable>();

			var template = ConfigurableTemplateTestHelper.SetupSystemTemplateFromString(Factory,
@"{A}-[#Config]
{A}-[DataContext=.DummyBODocSupportable]
{A}-[Name=Test]
{A}-[#ConfigurableSection:GEN, Test Date]
{B}-[<DateTimeAsString('<Now>', 'dd-MMM-yyyy')>]
{A}-[#EndOfReport]");
			template.SO_DataContext = ".DummyBODocSupportable";

			var documentCommand = Factory.New<DocumentCommand>();
			documentCommand.Parent = dummy;

			var document = documentCommand.Documents.AddNew();
			document.SI_SO = template.PK;
			document.SI_SU = documentCommand.PK;

			var docConfig = document.DocConfigs.AddNew();
			docConfig.ConfigItems.AddFromTemplateSection(template.TemplateSections.Find("Test Date"));

			var printJobs = DeliveryTestHelper.DeliverDocument(documentCommand, Enterprise.Core.Constants.Languages.French);
			AssertEquals("There should be one print job created.", 1, printJobs.Length);

			using (var excelInterface = new ExcelInterface())
			{
				excelInterface.LoadExcelFile(printJobs[0].SP_CustomProperties);

				AssertMultilineASCIIEquals("Should be French date.", "{B}-[02-JUIN-2008]", excelInterface.WorkSheets[0].ToString().ToUpper());
			}
		}

		[TestDate(2008, 6, 2)]
		public void TestDateInEnglishLanguage()
		{
			var dummy = Factory.New<DummyBODocSupportable>();

			var template = ConfigurableTemplateTestHelper.SetupSystemTemplateFromString(Factory,
@"{A}-[#Config]
{A}-[DataContext=.DummyBODocSupportable]
{A}-[Name=Test]
{A}-[#ConfigurableSection:GEN, Test Date]
{B}-[<DateTimeAsString('<Now>', 'dd-MMM-yyyy')>]
{A}-[#EndOfReport]");
			template.SO_DataContext = ".DummyBODocSupportable";

			var documentCommand = Factory.New<DocumentCommand>();
			documentCommand.Parent = dummy;

			var document = documentCommand.Documents.AddNew();
			document.SI_SO = template.PK;
			document.SI_SU = documentCommand.PK;

			var docConfig = document.DocConfigs.AddNew();
			docConfig.ConfigItems.AddFromTemplateSection(template.TemplateSections.Find("Test Date"));

			var printJobs = DeliveryTestHelper.DeliverDocument(documentCommand, Enterprise.Core.Constants.Languages.English);
			AssertEquals("There should be one print job created.", 1, printJobs.Length);

			using (var excelInterface = new ExcelInterface())
			{
				excelInterface.LoadExcelFile(printJobs[0].SP_CustomProperties);

				AssertMultilineASCIIEquals("Should be English date.", "{B}-[02-Jun-2008]", excelInterface.WorkSheets[0].ToString());
			}
		}

		public void TestUpperCurrencyToWordsInTurkish()
		{
			var currency = Factory.LoadTop1<RefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, "TRY"));
			var helper = new RefCurrencyTestHelper(Factory);
			helper.CreateRefLanguageText("RX_SubUnitName", currency.PK, "TR-TR", "RX", "kuruş");
			Factory.Save();
			var dummy = Factory.New<DummyBODocSupportable>();
			dummy.Z0_AnotherDecimal = 123.45;

			var template = ConfigurableTemplateTestHelper.SetupSystemTemplateFromString(Factory,
@"{A}-[#Config]
{A}-[DataContext=.DummyBODocSupportable]
{A}-[Name=Test]
{A}-[#ConfigurableSection:GEN, Test CurrencyToWords]
{B}-[<Upper(""<CurrencyToWords(<Z0_AnotherDecimal>,TRY)>"")>]
{B}-[<Upper(""<NumberToWords(<Z0_AnotherDecimal>)>"")>]
{A}-[#EndOfReport]");
			template.SO_DataContext = ".DummyBODocSupportable";

			var documentCommand = Factory.New<DocumentCommand>();
			documentCommand.Parent = dummy;

			var document = documentCommand.Documents.AddNew();
			document.SI_SO = template.PK;
			document.SI_SU = documentCommand.PK;

			var docConfig = document.DocConfigs.AddNew();
			docConfig.ConfigItems.AddFromTemplateSection(template.TemplateSections.Find("Test CurrencyToWords"));

			var printJobs = DeliveryTestHelper.DeliverDocument(documentCommand, Enterprise.Core.Constants.Languages.Turkish);
			AssertEquals("There should be one print job created.", 1, printJobs.Length);

			using (var excelInterface = new ExcelInterface())
			{
				excelInterface.LoadExcelFile(printJobs[0].SP_CustomProperties);

				AssertMultilineASCIIEquals("Should be Turkish currency to words.", "{B}-[YÜZYİRMİÜÇLİRAKIRKBEŞKURUŞ]\r\n{B}-[YÜZYİRMİÜÇ VİRGÜL KIRKBEŞ]", excelInterface.WorkSheets[0].ToString());
			}
		}

		public void TestDocumentTitleWithNestedMacrosWorkWithReportNameMacro()
		{
			var template = ConfigurableTemplateTestHelper.SetupSystemTemplateFromString(Factory,
@"{A}-[#Config]
{A}-[DataContext=.DummyBODocSupportable]
{A}-[Name=TestContinuedOver]
{A}-[#ConfigurableSection:GEN, Page Number of Page Total]
{B}-[Page <CurrentPage> of <TotalPages>]
{A}-[#ConfigurableSection:GEN, Document Title]
{B}-[<ReportName>]
{A}-[#ConfigurableSection:GEN, Document Title (Short)]
{B}-[<ReportNameShort>]
{A}-[#EndOfReport]");
			template.SO_DataContext = ".DummyBODocSupportable";

			var dummy = Factory.New<DummyBODocSupportable>();
			dummy.Z0_VarCharMax = "Testing... 1, 2, 3...";

			var config = ConfigurableTemplateTestHelper.CreateDocumentConfig(template);
			config.S3_IsSystem = ZBool.True;
			config.S3_Description = "testDocTemplate";
			config.ConfigItems.AddFromTemplateSection(config.TemplateSections.Find("Page Number of Page Total")).S4_SectionType = GenericSectionUsageList.Codes.PageHeaderAll;
			config.ConfigItems.AddFromTemplateSection(config.TemplateSections.Find("Document Title")).S4_SectionType = GenericSectionUsageList.Codes.BodySection;
			config.ConfigItems.AddFromTemplateSection(config.TemplateSections.Find("Document Title (Short)")).S4_SectionType = GenericSectionUsageList.Codes.BodySection;
			config.MenuTemplatePivot.SI_DocumentTitle = "Shipment Cartage Advice <Z0_VarCharMax>";

			Factory.Save();

			var documentCommand = Factory.Load<DocumentCommand>(config.MenuTemplatePivot.SI_SU);

			AssertMultilineASCIIEquals("",
@"{B}-[Page 1 of 1]
{B}-[Shipment Cartage Advice Testing... 1, 2, 3...]
{B}-[Cartage Advice Testing... 1, 2, 3...]", DocumentEngineTestHelper.ExecuteDocumentCommand(documentCommand, dummy));
		}

		[GuiTest]
		public void TestPageFooterThatBiggerThanOnePageWithAutoHeight()
		{
			var template = ConfigurableTemplateTestHelper.SetupSystemTemplateFromString(Factory,
@"{A}-[#Config]
{A}-[DataContext=.DummyBODocSupportable]
{A}-[Name=TestPageFooterThatBiggerThanOnePageWithAutoHeight]
{A}-[#ConfigurableSection:GEN, Page Number of Page Total]
{B}-[Page <CurrentPage> of <TotalPages>]
{A}-[#ConfigurableSection:GEN, My AutoHeight Section]
{B}-[<AutoHeight>(Begin) This is a very long text block. This is a very long text block. This is a very long text block. This is a very long text block. This is a very long text block. This is a very long text block. This is a very long text block. This is a very long text block. This is a very long text block. This is a very long text block. This is a very long text block. This is a very long text block. This is a very long text block. This is a very long text block. This is a very long text block. This is a very long text block. This is a very long text block. This is a very long text block. (End)]
{A}-[#EndOfReport]");
			template.SO_DataContext = ".DummyBODocSupportable";

			var config = ConfigurableTemplateTestHelper.CreateDocumentConfig(template);
			config.S3_IsSystem = ZBool.True;
			config.ConfigItems.AddFromTemplateSection(config.TemplateSections.Find("Page Number of Page Total")).S4_SectionType = GenericSectionUsageList.Codes.PageHeaderAll;
			config.ConfigItems.AddFromTemplateSection(config.TemplateSections.Find("My AutoHeight Section")).S4_SectionType = GenericSectionUsageList.Codes.BodySection;
			config.ConfigItems.AddFromTemplateSection(config.TemplateSections.Find("Page Number of Page Total")).S4_SectionType = GenericSectionUsageList.Codes.PageFooterAll;
			config.ConfigItems.AddFromTemplateSection(config.TemplateSections.Find("My AutoHeight Section")).S4_SectionType = GenericSectionUsageList.Codes.PageFooterAll;
			config.S3_Description = "testDocConfig";
			var dummy = Factory.New<DummyBODocSupportable>();
			for (var index = 0; index < 30; index++)
			{
				dummy.Collection.AddNew();
			}

			Factory.Save();

			var documentCommand = Factory.Load<DocumentCommand>(config.MenuTemplatePivot.SI_SU);
			documentCommand.SU_IsSystemDefined = true;

			config.MenuTemplatePivot.SI_IsSystemDefined = true;

			using (Report.TemporarilyStopErrorsThrowingAnException())
			{
				var result = DocumentEngineTestHelper.ExecuteDocumentCommand(documentCommand, dummy, null);

				try
				{
					AssertNotNull(ErrorReporter.LastExceptionReported);
					AssertEquals(typeof(ReportProcessingException), ErrorReporter.LastExceptionReported.GetType());
					AssertContains("The footers of this report do not fit. Some pages may not be rendered correctly", ErrorReporter.LastMessageReported);
				}
				finally
				{
					ErrorReporter.Clear();
				}
			}
		}

		public void TestPageFooterAllExceptLastPageWithAutoHeight()
		{
			var template = ConfigurableTemplateTestHelper.SetupSystemTemplateFromString(Factory,
@"{A}-[#Config]
{A}-[DataContext=.DummyBODocSupportable]
{A}-[Name=TestContinuedOver]
{A}-[#ConfigurableSection:BEX, Body Section Expanding 1]
{A}-[#SectionBody:Data=Collection]
{B}-[This is Body Section Expanding 1.]
{A}-[#ConfigurableSection:GEN, Page Number of Page Total]
{B}-[Page <CurrentPage> of <TotalPages>]
{A}-[#ConfigurableSection:GEN, My AutoHeight Section]
{B}-[<AutoHeight>(Begin) This is a very long text block. This is a very long text block. This is a very long text block. This is a very long text block. This is a very long text block. This is a very long text block. (End)]
{A}-[#ConfigurableSection:GEN, Continued Over...]
{B}-[Continued Over...]

{A}-[#EndOfReport]");
			template.SO_DataContext = ".DummyBODocSupportable";

			var config = ConfigurableTemplateTestHelper.CreateDocumentConfig(template);
			config.S3_IsSystem = ZBool.True;
			config.S3_Description = "TestDocDescription";
			config.ConfigItems.AddFromTemplateSection(config.TemplateSections.Find("Page Number of Page Total")).S4_SectionType = GenericSectionUsageList.Codes.PageHeaderAll;
			config.ConfigItems.AddFromTemplateSection(config.TemplateSections.Find("Body Section Expanding 1")).S4_SectionType = GenericSectionUsageList.Codes.BodySectionExpanding;
			config.ConfigItems.AddFromTemplateSection(config.TemplateSections.Find("Page Number of Page Total")).S4_SectionType = GenericSectionUsageList.Codes.PageFooterAll;
			config.ConfigItems.AddFromTemplateSection(config.TemplateSections.Find("Continued Over...")).S4_SectionType = GenericSectionUsageList.Codes.PageFooterAllExceptLastPage;
			config.ConfigItems.AddFromTemplateSection(config.TemplateSections.Find("My AutoHeight Section")).S4_SectionType = GenericSectionUsageList.Codes.PageFooterAll;

			var dummy = Factory.New<DummyBODocSupportable>();
			for (var index = 0; index < 30; index++)
			{
				dummy.Collection.AddNew();
			}

			Factory.Save();

			var documentCommand = Factory.Load<DocumentCommand>(config.MenuTemplatePivot.SI_SU);

			AssertMultilineASCIIEquals("",
@"{B}-[Page 1 of 2]
{B}-[This is Body Section Expanding 1.]
{B}-[This is Body Section Expanding 1.]
{B}-[This is Body Section Expanding 1.]
{B}-[This is Body Section Expanding 1.]
{B}-[This is Body Section Expanding 1.]
{B}-[This is Body Section Expanding 1.]
{B}-[This is Body Section Expanding 1.]
{B}-[This is Body Section Expanding 1.]
{B}-[This is Body Section Expanding 1.]
{B}-[This is Body Section Expanding 1.]
{B}-[This is Body Section Expanding 1.]
{B}-[This is Body Section Expanding 1.]
{B}-[This is Body Section Expanding 1.]
{B}-[This is Body Section Expanding 1.]
{B}-[This is Body Section Expanding 1.]
{B}-[This is Body Section Expanding 1.]
{B}-[This is Body Section Expanding 1.]
{B}-[This is Body Section Expanding 1.]
{B}-[This is Body Section Expanding 1.]
{B}-[This is Body Section Expanding 1.]
{B}-[This is Body Section Expanding 1.]
{B}-[This is Body Section Expanding 1.]
{B}-[This is Body Section Expanding 1.]
{B}-[This is Body Section Expanding 1.]
{B}-[This is Body Section Expanding 1.]

{B}-[(Begin)]
{B}-[This is a]
{B}-[very long]
{B}-[text]
{B}-[block.]
{B}-[This is a]
{B}-[very long]
{B}-[text]
{B}-[block.]
{B}-[This is a]
{B}-[very long]
{B}-[text]
{B}-[block.]
{B}-[This is a]
{B}-[very long]
{B}-[text]
{B}-[block.]
{B}-[This is a]
{B}-[very long]
{B}-[text]
{B}-[block.]
{B}-[This is a]
{B}-[very long]
{B}-[text]
{B}-[block.]
{B}-[(End)]
{B}-[Continued Over...]

{B}-[Page 1 of 2]
{B}-[Page 2 of 2]
{B}-[This is Body Section Expanding 1.]
{B}-[This is Body Section Expanding 1.]
{B}-[This is Body Section Expanding 1.]
{B}-[This is Body Section Expanding 1.]
{B}-[This is Body Section Expanding 1.]

{B}-[(Begin)]
{B}-[This is a]
{B}-[very long]
{B}-[text]
{B}-[block.]
{B}-[This is a]
{B}-[very long]
{B}-[text]
{B}-[block.]
{B}-[This is a]
{B}-[very long]
{B}-[text]
{B}-[block.]
{B}-[This is a]
{B}-[very long]
{B}-[text]
{B}-[block.]
{B}-[This is a]
{B}-[very long]
{B}-[text]
{B}-[block.]
{B}-[This is a]
{B}-[very long]
{B}-[text]
{B}-[block.]
{B}-[(End)]
{B}-[Page 2 of 2]", DocumentEngineTestHelper.ExecuteDocumentCommand(documentCommand, dummy));
		}

		public void TestAutoHeightInFooterDoesNotOverflowIntoNextPage()
		{
			var template = ConfigurableTemplateTestHelper.SetupSystemTemplateFromString(Factory,
@"{A}-[#config]
{A}-[Name=Test]
{A}-[#ConfigurableSection:GEN, Page Numbers]
{B}-[Page <CurrentPage> of <TotalPages>]
{A}-[#ConfigurableSection:GEN, My AutoHeight Section]
{B}-[<AutoHeight><Z0_VarCharMax>]
{A}-[#EndOfReport]");

			var parent = Factory.New<DummyBODocSupportable>();
			parent.Z0_VarCharMax = "This is a very long text field. This is a very long text field.";

			var config = ConfigurableTemplateTestHelper.CreateDocumentConfig(template);
			config.S3_IsSystem = ZBool.True;
			config.S3_Description = "testDocTemptlate";

			var configItem1 = ConfigurableTemplateTestHelper.AddFromTemplateSection(config, "Page Numbers");
			configItem1.S4_SectionType = GenericSectionUsageList.Codes.PageHeaderAll;

			var configItem2 = ConfigurableTemplateTestHelper.AddFromTemplateSection(config, "My AutoHeight Section");
			configItem2.S4_SectionType = GenericSectionUsageList.Codes.BodySection;

			var configItem3 = ConfigurableTemplateTestHelper.AddFromTemplateSection(config, "My AutoHeight Section");
			configItem3.S4_SectionType = GenericSectionUsageList.Codes.PageFooterFallbackDefault;

			var documentCommand = Factory.Load<DocumentCommand>(config.MenuTemplatePivot.MenuItem.PK);

			Factory.Save();

			var result = DocumentEngineTestHelper.ExecuteDocumentCommand(documentCommand, parent, null);

			using (var excelInterface = new ExcelInterface())
			{
				excelInterface.LoadExcelFile(result.Output);

				using (var tiffStream = new MemoryStream())
				{
					excelInterface.ExportToMultiPageTiffAndScale(tiffStream, false, 100, false, Env.Registry.PDFTIFResolution, PixelFormat.Format24bppRgb, null);

					using (var image = System.Drawing.Image.FromStream(tiffStream))
					{
						AssertEquals("image.GetPageCount()", 1, image.GetPageCount());
					}
				}
			}
		}

		public void TestAllSystemDefinedUserConfigurableTemplateColumnsHaveCorrectWidths()
		{
			foreach (var template in ConfigurableTemplateTestHelper.GetAllSystemConfigurableTemplates(Factory))
			{
				using (var excelInterface = new ExcelInterface())
				{
					excelInterface.LoadExcelFile(template.SO_Template);

					var workSheet = excelInterface.WorkSheets[0];

					CombineAssertions(delegate()
					{
						AssertEquals(
							string.Format("Asserting Left Edge: Template: [{0}]  Column: [{1}]", template.SO_Name, 1),
							182,
							workSheet.GetColWidth(1));

						AssertEquals(
							string.Format("Asserting Right Edge: Template: [{0}]  Column: [{1}]", template.SO_Name, 71),
							182,
							workSheet.GetColWidth(71));

						for (var column = 2; column <= 70; column += 2)
						{
							AssertEquals(
								string.Format("Asserting Body Column: Template: [{0}]  Column: [{1}]", template.SO_Name, column),
								914,
								workSheet.GetColWidth(column));
						}

						for (var column = 3; column <= 69; column += 2)
						{
							AssertEquals(
								string.Format("Asserting Separating Column: Template: [{0}]  Column: [{1}]", template.SO_Name, column),
								109,
								workSheet.GetColWidth(column));
						}
					});
				}
			}
		}

		public void TestAllDocBuilderDocumentsHaveDefaultPageFooterSectionInTheLastPosition()
		{
			var errorsByCategory = new Dictionary<string, IList<string>>();
			var errors = new List<string>();
			errorsByCategory.Add("These documents must have a default page footer section in the last position", errors);

			var sectionName = "Default Page Footer for All Documents";
			foreach (var config in ConfigurableTemplateTestHelper.GetAllDocBuilderDocuments(Factory))
			{
				var notBackPageQuery = new ZQuery(StmMenuDocumentConfigItemSchema.S4_SectionType, SQLComparisonOperator.NotEqual, ConfigurableSectionTypeList.Codes.BackPage);
				notBackPageQuery.AddToFilter(StmMenuDocumentConfigItemSchema.S4_SectionType, SQLComparisonOperator.NotEqual, GenericSectionUsageList.Codes.BackPage);

				var query = new ZQuery(StmMenuDocumentConfigItemSchema.S4_S3, config.PK);
				query.AddToFilter(notBackPageQuery, JoinCondition.And);
				query.OrderBy = string.Format("{0} DESC", StmMenuDocumentConfigItem.Schema.S4_PrintOrder);

				var configItem = Factory.LoadTop1<StmMenuDocumentConfigItem>(query);
				if (!configItem.S4_SectionItemName.Equals(sectionName))
				{
					var menuItem = config.MenuTemplatePivot.MenuItem;
					errors.Add(string.Format("Menu Name: [{0}]  Business Context: [{1}]  Path: [{2}]",
						menuItem.SU_MenuName, menuItem.SU_BusinessContext, menuItem.SU_MenuPath));
				}
			}

			this.AssertErrorsByCategory("The following errors have occured:-", errorsByCategory);
		}

		public void TestAllTemplateSectionsHaveACategory()
		{
			foreach (var template in ConfigurableTemplateTestHelper.GetEnglishSystemConfigurableTemplates(Factory))
			{
				TestAllTemplateSectionsHaveACategoryCore(template);
			}
		}

		void TestAllTemplateSectionsHaveACategoryCore(StmTemplateBase template)
		{
			var sections = new TemplateSectionCollection(template.GetExcelTemplate());
			var categoryIsEmptyError = String.Format("These template sections for \"{0}\" do not have a category", template.SO_Name);
			var errorsByCategory = new Dictionary<string, IList<string>>();
			errorsByCategory.Add(categoryIsEmptyError, new List<string>());

			foreach (TemplateSection section in sections)
			{
				if (section.Category.Trim().IsEmpty && !section.IsControlSection)
				{
					errorsByCategory[categoryIsEmptyError].Add(section.ToString());
				}
			}

			AssertErrorsByCategory("The following errors have occured", errorsByCategory);
		}

		public void TestNoDuplicateSections()
		{
			var ignoredSections = new List<string>()
			{
				"Default Page Footer for All Documents",
				//Temporary Solution because wont detect difference between portrait, landscape, row height and borders
				"Landscape Company Name + Page",
				"Landscape Document Title",
				"Landscape Job Number",
				"Landscape External Reference",
				"Landscape Job Info - Job Header Section Label",
				"Landscape Job Info - Warehouse Name + Job Related Client Name",
				"Landscape ConfirmationInstructions",
				"Landscape SupplierDocAddress/TransportCOAddress",
				"Landscape Customer Reference",
				"Landscape Arrival Date",
				"Handling Instructions (Landscape)",
				"Packages (Separator Line)",
				"Packages (Separator Space)",
				"Packages (Separator Space x2)",
				"Packages (Separator Space x3)",
				"Packages (Separator Space x7)",
				"Packages (Separator Space - Large)",
				"Packages (Separator |¯¯¯|¯¯¯|)",
				"Packages (Separator |___|___|)",
				"Packages (Separator |¯           ¯|)",
				"Packages (Separator |_           _|)",
				"Packages (Separator |¯   ¯|¯   ¯|)",
				"Packages (Separator |_   _|_   _|)",
				"Packages (Separator |¯¯¯¯¯¯¯|)",
				"Packages (Separator |_______|)",
				"Label - Separator (|               |)",
				"Label (End of label)",
				"Label - Job Lines (Product Code Barcode)",
				"Label - Job Lines (Product Code Barcode Extension 5)",
				"Label - Job Lines (Product Code Barcode Extension 10)",
				"Label - Job Lines (Product Code Barcode Extension 20)",
				"Label - Job Lines (Product Code Barcode Extension 40)",
				"Label - Job Lines (Product Code Barcode Extension 80)",
				"Label - Job Lines (Product Code Barcode Extension 160)",
				"Label - Job Lines (Part Attribute 1 with Barcode)",
				"Label - Job Lines (Part Attribute 1 with Barcode Extension 5)",
				"Label - Job Lines (Part Attribute 1 with Barcode Extension 10)",
				"Label - Job Lines (Part Attribute 1 with Barcode Extension 20)",
				"Label - Job Lines (Part Attribute 1 with Barcode Extension 40)",
				"Label - Job Lines (Part Attribute 1 with Barcode Extension 80)",
				"Label - Job Lines (Part Attribute 1 with Barcode Extension 160)",
				"Label - Job Lines (Part Attribute 2 with Barcode)",
				"Label - Job Lines (Part Attribute 2 with Barcode Extension 5)",
				"Label - Job Lines (Part Attribute 2 with Barcode Extension 10)",
				"Label - Job Lines (Part Attribute 2 with Barcode Extension 20)",
				"Label - Job Lines (Part Attribute 2 with Barcode Extension 40)",
				"Label - Job Lines (Part Attribute 2 with Barcode Extension 80)",
				"Label - Job Lines (Part Attribute 2 with Barcode Extension 160)",
				"Label - Job Lines (Part Attribute 3 with Barcode)",
				"Label - Job Lines (Part Attribute 3 with Barcode Extension 5)",
				"Label - Job Lines (Part Attribute 3 with Barcode Extension 10)",
				"Label - Job Lines (Part Attribute 3 with Barcode Extension 20)",
				"Label - Job Lines (Part Attribute 3 with Barcode Extension 40)",
				"Label - Job Lines (Part Attribute 3 with Barcode Extension 80)",
				"Label - Job Lines (Part Attribute 3 with Barcode Extension 160)",
				"Label - Job Lines (Expiry Date with Barcode)",
				"Label - Job Lines (Expiry Date with Barcode Extension 5)",
				"Label - Job Lines (Expiry Date with Barcode Extension 10)",
				"Label - Job Lines (Expiry Date with Barcode Extension 20)",
				"Label - Job Lines (Expiry Date with Barcode Extension 40)",
				"Label - Job Lines (Expiry Date with Barcode Extension 80)",
				"Label - Job Lines (Expiry Date with Barcode Extension 160)",
				"Label - Job Lines (Packing Date with Barcode)",
				"Label - Job Lines (Packing Date with Barcode Extension 5)",
				"Label - Job Lines (Packing Date with Barcode Extension 10)",
				"Label - Job Lines (Packing Date with Barcode Extension 20)",
				"Label - Job Lines (Packing Date with Barcode Extension 40)",
				"Label - Job Lines (Packing Date with Barcode Extension 80)",
				"Label - Job Lines (Packing Date with Barcode Extension 160)",
				"Label - Job Lines (Serial Number with Barcode)",
				"Label - Job Lines (Serial Number with Barcode Extension 5)",
				"Label - Job Lines (Serial Number with Barcode Extension 10)",
				"Label - Job Lines (Serial Number with Barcode Extension 20)",
				"Label - Job Lines (Serial Number with Barcode Extension 40)",
				"Label - Job Lines (Serial Number with Barcode Extension 80)",
				"Label - Job Lines (Serial Number with Barcode Extension 160)",
				"Labels (Company Name)",
				"Separator - Dashed",
				"Packages (Start of Package)",
				"Packages (End of Package)",
				"Packages (Separator Space x3 - Border)",
				"Packages (Separator Space x3 - Top Border)",
				"Packages (Separator Space x3 - Bottom Border)",
				"Carrier Service Level (eParcel Logo)",
				"Carrier Service Level (Package Consignment Reference Number, AP Article ID)",
				"Invoice Charges and Description titles",
				"Separator - Continued Over… with PrintAccumulativeTotalAmountsInMultipageInvoices",
				"Malaysia eInvoice QR Code",
				"Cash Advance Document Lines",
				"Cash Advance Page Footer",
				"Cash Advance Payment Footer",
				"Cash Advance Letterhead",
			};

			CombineAssertions(delegate()
			{
				foreach (var template in GetTemplatesToTest())
				{
					var templateHasProblems = false;
					var excelTemplate = template.GetExcelTemplate();

					using (var excelInterface = new ExcelInterface())
					{
						using (var stream = excelTemplate.GetAsTemplateStream())
						{
							excelInterface.LoadExcelFile(stream);
						}

						var workSheet = excelInterface.WorkSheets[0];
						var templateSections = new TemplateSectionCollection(excelTemplate);
						var sectionByContent = new Dictionary<string, TemplateSection>(StringComparer.InvariantCultureIgnoreCase);

						for (var index = 0; index < templateSections.Count; index++)
						{
							var templateSection = templateSections[index];

							if (!templateSection.IsControlSection && !ignoredSections.Contains(templateSection.SectionName))
							{
								var contents = workSheet.ToString(templateSection.StartingRowNumber, templateSection.LastRowNumber - 1);
								sectionByContent.TryGetValue(contents, out var duplicateSection);
								if (duplicateSection != null)
								{
									if (!templateHasProblems)
									{
										templateHasProblems = true;
										Fail(string.Format("The template [{0}, {1}] contains the following sections that seem to be duplicates of one another:-", template.SO_Name, template.ExcelTemplateFullPath));
									}

									Fail(string.Format(
@"    [{0}, {1}] Row: {2}
{3}

	and

	[{4}, {5}] Row: {6}

",
										templateSection.TypeCode, templateSection.SectionName, templateSection.StartingRowNumber, contents,
										duplicateSection.TypeCode, duplicateSection.SectionName, duplicateSection.StartingRowNumber));
								}
								else
								{
									sectionByContent.Add(contents, templateSection);
								}
							}
						}
					}
				}
			});

			Assert("No Errors Found", true);
		}

		public void TestAllSectionsDoNotUseConditionalStatements()
		{
			CombineAssertions(delegate()
			{
				foreach (var template in GetTemplatesToTest())
				{
					var templateHasProblems = false;
					var excelTemplate = template.GetExcelTemplate();

					using (var excelInterface = new ExcelInterface())
					{
						using (var stream = excelTemplate.GetAsTemplateStream())
						{
							excelInterface.LoadExcelFile(stream);
						}

						var workSheet = excelInterface.WorkSheets[0];
						var templateSections = new TemplateSectionCollection(excelTemplate);

						foreach (TemplateSection templateSection in templateSections)
						{
							if (!IsSectionNameIgnored(templateSection.SectionName))
							{
								var conditionalStatementCount = 0;

								for (var rowIndex = templateSection.StartingRowNumber; rowIndex < templateSection.LastRowNumber; rowIndex++)
								{
									var cellContent = workSheet[rowIndex, 0].ToString();

									if (IsConditionalCellContent(cellContent))
									{
										conditionalStatementCount++;
									}
								}

								if (conditionalStatementCount > 0)
								{
									if (!templateHasProblems)
									{
										templateHasProblems = true;
										Fail(string.Format("The template [{0}, {1}] contains the following sections that have conditional statements:-", template.SO_Name, template.ExcelTemplateFullPath));
									}

									Fail(string.Format("    [{0}, {1}] Row: {2}", templateSection.TypeCode, templateSection.SectionName, templateSection.StartingRowNumber));
								}
							}
						}
					}
				}
			});

			Assert("No Errors Found", true);
		}

		public void TestNoLegacyBodySectionsAreUsed()
		{
			CombineAssertions(() =>
			{
				foreach (var template in GetTemplatesToTest())
				{
					var templateHasProblems = false;
					var excelTemplate = template.GetExcelTemplate();

					using (var excelInterface = new ExcelInterface())
					{
						using (var stream = excelTemplate.GetAsTemplateStream())
						{
							excelInterface.LoadExcelFile(stream);
						}

						var workSheet = excelInterface.WorkSheets[0];
						var templateSections = new TemplateSectionCollection(excelTemplate);

						foreach (TemplateSection templateSection in templateSections)
						{
							if (templateSection.TypeCode.Equals(ConfigurableSectionTypeList.Codes.BodySection))
							{
								if (!templateHasProblems)
								{
									templateHasProblems = true;
									Fail(string.Format(
										"The template [{0}, {1}] contains the following sections that are the old section body type:-",
										template.SO_Name, template.ExcelTemplateFullPath));
								}

								Fail(string.Format("    [{0}, {1}] Row: {2}", templateSection.TypeCode, templateSection.SectionName,
									templateSection.StartingRowNumber));
							}
						}
					}
				}
			});

			Assert("No Errors Found", true);
		}

		public void TestNoLegacySectionsAreUsed()
		{
			CombineAssertions(() =>
			{
				foreach (var template in GetTemplatesToTest())
				{
					var templateHasProblems = false;
					var excelTemplate = template.GetExcelTemplate();

					using (var excelInterface = new ExcelInterface())
					{
						using (var stream = excelTemplate.GetAsTemplateStream())
						{
							excelInterface.LoadExcelFile(stream);
						}

						var workSheet = excelInterface.WorkSheets[0];
						var templateSections = new TemplateSectionCollection(excelTemplate);

						foreach (TemplateSection templateSection in templateSections)
						{
							if (!IsSectionNameIgnored(templateSection.SectionName) &&
								(templateSection.TypeCode.Equals(ConfigurableSectionTypeList.Codes.BodySection) ||
								 templateSection.TypeCode.Equals(ConfigurableSectionTypeList.Codes.DocumentHeader) ||
								 templateSection.TypeCode.Equals(ConfigurableSectionTypeList.Codes.DocumentFooter) ||
								 templateSection.TypeCode.Equals(ConfigurableSectionTypeList.Codes.PageHeader) ||
								 templateSection.TypeCode.Equals(ConfigurableSectionTypeList.Codes.PageFooter)))
							{
								if (!templateHasProblems)
								{
									templateHasProblems = true;
									Fail(string.Format(
										"The template [{0}, {1}] contains the following sections that are the old section body type:-",
										template.SO_Name, template.ExcelTemplateFullPath));
								}

								Fail(string.Format("    [{0}, {1}] Row: {2}", templateSection.TypeCode, templateSection.SectionName,
									templateSection.StartingRowNumber));
							}
						}
					}
				}
			});

			Assert("No Errors Found", true);
		}

		public void TestAllExpandingBodySectionsAreExpanding()
		{
			CombineAssertions(() =>
			{
				foreach (var template in GetTemplatesToTest())
				{
					var ignoredSections = new List<string>()
					{
						"Invoice Charges and Description titles" //This docstrip is empty, we cannot delete it
					};
					var templateHasProblems = false;
					var excelTemplate = template.GetExcelTemplate();

					using (var excelInterface = new ExcelInterface())
					{
						using (var stream = excelTemplate.GetAsTemplateStream())
						{
							excelInterface.LoadExcelFile(stream);
						}

						var workSheet = excelInterface.WorkSheets[0];
						var templateSections = new TemplateSectionCollection(excelTemplate);

						foreach (TemplateSection templateSection in templateSections)
						{
							if (!ignoredSections.Contains(templateSection.SectionName) && templateSection.TypeCode.EqualsIgnoringCase(ConfigurableSectionTypeList.Codes.BodySectionExpanding))
							{
								var isExpandingSection = false;

								for (var rowIndex = templateSection.StartingRowNumber; rowIndex < templateSection.LastRowNumber; rowIndex++)
								{
									var cellContent = workSheet[rowIndex, 0].ToString();

									if (IsExpandingCellContent(cellContent))
									{
										isExpandingSection = true;
									}
								}

								if (!isExpandingSection)
								{
									if (!templateHasProblems)
									{
										templateHasProblems = true;
										Fail(string.Format(
											"The template [{0}, {1}] contains the following expanding body sections that are not expanding:-",
											template.SO_Name, template.ExcelTemplateFullPath));
									}

									Fail(string.Format("    [{0}, {1}] Row: {2}", templateSection.TypeCode, templateSection.SectionName,
										templateSection.StartingRowNumber));
								}
							}
						}
					}
				}
			});

			Assert("No Errors Found", true);
		}

		public void TestAllGenericSectionsAreNotExpanding()
		{
			CombineAssertions(() =>
			{
				foreach (var template in GetTemplatesToTest())
				{
					var templateHasProblems = false;
					var excelTemplate = template.GetExcelTemplate();

					using (var excelInterface = new ExcelInterface())
					{
						using (var stream = excelTemplate.GetAsTemplateStream())
						{
							excelInterface.LoadExcelFile(stream);
						}

						var workSheet = excelInterface.WorkSheets[0];
						var templateSections = new TemplateSectionCollection(excelTemplate);

						foreach (TemplateSection templateSection in templateSections)
						{
							if (templateSection.TypeCode.EqualsIgnoringCase(ConfigurableSectionTypeList.Codes.GenericSection))
							{
								var isExpandingSection = false;

								for (var rowIndex = templateSection.StartingRowNumber; rowIndex < templateSection.LastRowNumber; rowIndex++)
								{
									var cellContent = workSheet[rowIndex, 0].ToString();

									if (IsExpandingCellContent(cellContent))
									{
										isExpandingSection = true;
										break;
									}
								}

								if (isExpandingSection)
								{
									if (!templateHasProblems)
									{
										templateHasProblems = true;
										Fail(string.Format("The template [{0}, {1}] contains the following generic sections that are expanding:-",
											template.SO_Name, template.ExcelTemplateFullPath));
									}

									Fail(string.Format("    [{0}, {1}] Row: {2}", templateSection.TypeCode, templateSection.SectionName,
										templateSection.StartingRowNumber));
								}
							}
						}
					}
				}
			});

			Assert("No Errors Found", true);
		}

		public void TestPageFooterAllExceptLastPage()
		{
			var template = ConfigurableTemplateTestHelper.SetupSystemTemplateFromString(Factory,
@"{A}-[#Config]
{A}-[DataContext=.DummyBODocSupportable]
{A}-[Name=TestContinuedOver]
{A}-[#ConfigurableSection:BEX, Body Section Expanding 1]
{A}-[#SectionBody:Data=Collection]
{B}-[This is Body Section Expanding 1.]
{A}-[#ConfigurableSection:GEN, Page Number of Page Total]
{B}-[Page <CurrentPage> of <TotalPages>]

{A}-[#ConfigurableSection:GEN, Continued Over...]
{B}-[Continued Over...]

{A}-[#EndOfReport]");
			template.SO_DataContext = ".DummyBODocSupportable";

			var config = ConfigurableTemplateTestHelper.CreateDocumentConfig(template);
			config.S3_IsSystem = ZBool.True;
			config.S3_Description = "testDocDescrription";
			config.ConfigItems.AddFromTemplateSection(config.TemplateSections.Find("Page Number of Page Total")).S4_SectionType = GenericSectionUsageList.Codes.PageHeaderAll;
			config.ConfigItems.AddFromTemplateSection(config.TemplateSections.Find("Body Section Expanding 1")).S4_SectionType = GenericSectionUsageList.Codes.BodySectionExpanding;
			config.ConfigItems.AddFromTemplateSection(config.TemplateSections.Find("Continued Over...")).S4_SectionType = GenericSectionUsageList.Codes.PageFooterAllExceptLastPage;

			var dummy = Factory.New<DummyBODocSupportable>();
			for (var index = 0; index < 120; index++)
			{
				dummy.Collection.AddNew();
			}

			Factory.Save();

			var documentCommand = Factory.Load<DocumentCommand>(config.MenuTemplatePivot.SI_SU);

			AssertMultilineASCIIEquals("",
@"{B}-[Page 1 of 3]

{B}-[This is Body Section Expanding 1.]
{B}-[This is Body Section Expanding 1.]
{B}-[This is Body Section Expanding 1.]
{B}-[This is Body Section Expanding 1.]
{B}-[This is Body Section Expanding 1.]
{B}-[This is Body Section Expanding 1.]
{B}-[This is Body Section Expanding 1.]
{B}-[This is Body Section Expanding 1.]
{B}-[This is Body Section Expanding 1.]
{B}-[This is Body Section Expanding 1.]
{B}-[This is Body Section Expanding 1.]
{B}-[This is Body Section Expanding 1.]
{B}-[This is Body Section Expanding 1.]
{B}-[This is Body Section Expanding 1.]
{B}-[This is Body Section Expanding 1.]
{B}-[This is Body Section Expanding 1.]
{B}-[This is Body Section Expanding 1.]
{B}-[This is Body Section Expanding 1.]
{B}-[This is Body Section Expanding 1.]
{B}-[This is Body Section Expanding 1.]
{B}-[This is Body Section Expanding 1.]
{B}-[This is Body Section Expanding 1.]
{B}-[This is Body Section Expanding 1.]
{B}-[This is Body Section Expanding 1.]
{B}-[This is Body Section Expanding 1.]
{B}-[This is Body Section Expanding 1.]
{B}-[This is Body Section Expanding 1.]
{B}-[This is Body Section Expanding 1.]
{B}-[This is Body Section Expanding 1.]
{B}-[This is Body Section Expanding 1.]
{B}-[This is Body Section Expanding 1.]
{B}-[This is Body Section Expanding 1.]
{B}-[This is Body Section Expanding 1.]
{B}-[This is Body Section Expanding 1.]
{B}-[This is Body Section Expanding 1.]
{B}-[This is Body Section Expanding 1.]
{B}-[This is Body Section Expanding 1.]
{B}-[This is Body Section Expanding 1.]
{B}-[This is Body Section Expanding 1.]
{B}-[This is Body Section Expanding 1.]
{B}-[This is Body Section Expanding 1.]
{B}-[This is Body Section Expanding 1.]
{B}-[This is Body Section Expanding 1.]
{B}-[This is Body Section Expanding 1.]
{B}-[This is Body Section Expanding 1.]
{B}-[This is Body Section Expanding 1.]
{B}-[This is Body Section Expanding 1.]
{B}-[This is Body Section Expanding 1.]
{B}-[This is Body Section Expanding 1.]
{B}-[This is Body Section Expanding 1.]
{B}-[This is Body Section Expanding 1.]

{B}-[Continued Over...]

{B}-[Page 2 of 3]

{B}-[This is Body Section Expanding 1.]
{B}-[This is Body Section Expanding 1.]
{B}-[This is Body Section Expanding 1.]
{B}-[This is Body Section Expanding 1.]
{B}-[This is Body Section Expanding 1.]
{B}-[This is Body Section Expanding 1.]
{B}-[This is Body Section Expanding 1.]
{B}-[This is Body Section Expanding 1.]
{B}-[This is Body Section Expanding 1.]
{B}-[This is Body Section Expanding 1.]
{B}-[This is Body Section Expanding 1.]
{B}-[This is Body Section Expanding 1.]
{B}-[This is Body Section Expanding 1.]
{B}-[This is Body Section Expanding 1.]
{B}-[This is Body Section Expanding 1.]
{B}-[This is Body Section Expanding 1.]
{B}-[This is Body Section Expanding 1.]
{B}-[This is Body Section Expanding 1.]
{B}-[This is Body Section Expanding 1.]
{B}-[This is Body Section Expanding 1.]
{B}-[This is Body Section Expanding 1.]
{B}-[This is Body Section Expanding 1.]
{B}-[This is Body Section Expanding 1.]
{B}-[This is Body Section Expanding 1.]
{B}-[This is Body Section Expanding 1.]
{B}-[This is Body Section Expanding 1.]
{B}-[This is Body Section Expanding 1.]
{B}-[This is Body Section Expanding 1.]
{B}-[This is Body Section Expanding 1.]
{B}-[This is Body Section Expanding 1.]
{B}-[This is Body Section Expanding 1.]
{B}-[This is Body Section Expanding 1.]
{B}-[This is Body Section Expanding 1.]
{B}-[This is Body Section Expanding 1.]
{B}-[This is Body Section Expanding 1.]
{B}-[This is Body Section Expanding 1.]
{B}-[This is Body Section Expanding 1.]
{B}-[This is Body Section Expanding 1.]
{B}-[This is Body Section Expanding 1.]
{B}-[This is Body Section Expanding 1.]
{B}-[This is Body Section Expanding 1.]
{B}-[This is Body Section Expanding 1.]
{B}-[This is Body Section Expanding 1.]
{B}-[This is Body Section Expanding 1.]
{B}-[This is Body Section Expanding 1.]
{B}-[This is Body Section Expanding 1.]
{B}-[This is Body Section Expanding 1.]
{B}-[This is Body Section Expanding 1.]
{B}-[This is Body Section Expanding 1.]
{B}-[This is Body Section Expanding 1.]
{B}-[This is Body Section Expanding 1.]

{B}-[Continued Over...]

{B}-[Page 3 of 3]

{B}-[This is Body Section Expanding 1.]
{B}-[This is Body Section Expanding 1.]
{B}-[This is Body Section Expanding 1.]
{B}-[This is Body Section Expanding 1.]
{B}-[This is Body Section Expanding 1.]
{B}-[This is Body Section Expanding 1.]
{B}-[This is Body Section Expanding 1.]
{B}-[This is Body Section Expanding 1.]
{B}-[This is Body Section Expanding 1.]
{B}-[This is Body Section Expanding 1.]
{B}-[This is Body Section Expanding 1.]
{B}-[This is Body Section Expanding 1.]
{B}-[This is Body Section Expanding 1.]
{B}-[This is Body Section Expanding 1.]
{B}-[This is Body Section Expanding 1.]
{B}-[This is Body Section Expanding 1.]
{B}-[This is Body Section Expanding 1.]
{B}-[This is Body Section Expanding 1.]", DocumentEngineTestHelper.ExecuteDocumentCommand(documentCommand, dummy));
		}

		public void TestAllDocumentConfigItemsAreInOrder()
		{
			CombineAssertions(() =>
			{
				var comparer = new StmMenuDocumentConfigItemComparer();

				foreach (var config in Factory.LoadAll<StmMenuDocumentConfig>())
				{
					var configItems = config.ConfigItems;

					configItems.SortByPrintOrder();

					for (var index = 1; index < configItems.Count; index++)
					{
						Assert(string.Format("The config items of [{0}, {1}, {2}] are in the wrong order.",
								config.MenuTemplatePivot.MenuItem.SU_MenuName,
								config.MenuTemplatePivot.MenuItem.SU_BusinessContext,
								config.PK),
							comparer.Compare(configItems[index - 1], configItems[index]) <= 0);
					}
				}
			});
		}

		[StressTestAttribute]
		public void TestSignoffAsDocumentFooterIsNotUsed()
		{
			var errors = new ZStringBuilder();
			var template = GetSystemSectionRepositoryTemplate();
			var excelTemplate = new ExcelTemplateReadFromStmTemplateTable(template);
			var templateSections = new TemplateSectionCollection(excelTemplate);

			AssertNull("[DFT, Signoff] should have been deleted.", DocumentEngineTestHelper.FindTemplateSection(templateSections, "DFT", "Signoff"));

			var configItems = Factory.LoadAll<StmMenuDocumentConfigItem>();

			foreach (var configItem in configItems)
			{
				if (configItem.S4_SectionType == "DFT" && configItem.S4_SectionItemName == "Signoff")
				{
					var config = configItem.ParentConfig;
					errors.Append(string.Format("Template Section: [{0}]    BusinessContext: [{1}]    MenuName: [{2}]",
						configItem.S4_SectionItemName,
						config.MenuTemplatePivot.MenuItem.SU_BusinessContext,
						config.MenuTemplatePivot.MenuItem.SU_MenuName));
				}
			}

			Assert(
@"The following config items still use [DFT, Signoff]:

" + errors.ToStringWithNewLineBetweenAppends(),
				errors.IsEmpty);
		}

		public void TestTemplateSectionsHaveBeenTranslated()
		{
			var errors = new ZStringBuilder();
			var template = GetSystemSectionRepositoryTemplate();

			ExcelTemplate excelTemplate = new ExcelTemplateReadFromStmTemplateTable(template);

			var templateSections = new TemplateSectionCollection(excelTemplate);

			foreach (TemplateSection templateSection in templateSections)
			{
				if (templateSection.SectionName.Contains("GoodsDescription"))
				{
					errors.Append(string.Format("Template Section: [{0}] requires [{1}] to be changed to [{2}].",
						templateSection.SectionName,
						"GoodsDescription",
						"Goods Description"));
				}
			}

			Assert(
@"The following template sections contains a string that should be changed.

" + errors.ToStringWithNewLineBetweenAppends(),
				errors.IsEmpty);
		}

		[StressTestAttribute]
		public void TestAllSectionsUsedArePresentInTheSystemSectionRepository()
		{
			var errors = new ZStringBuilder();
			var template = GetSystemSectionRepositoryTemplate();
			var excelTemplate = new ExcelTemplateReadFromStmTemplateTable(template);
			var sectionRepositry = new SectionRepository(excelTemplate);

			var configItems = Factory.LoadAll<StmMenuDocumentConfigItem>();

			foreach (var configItem in configItems)
			{
				var templateSection = sectionRepositry.AllSections.Find(configItem.S4_SectionItemName);
				if (templateSection == null)
				{
					var config = configItem.ParentConfig;
					errors.Append(string.Format("Template Section: [{0}]   BusinessContext: [{1}]   MenuName: [{2}]",
						configItem.S4_SectionItemName,
						config.MenuTemplatePivot.MenuItem.SU_BusinessContext,
						config.MenuTemplatePivot.MenuItem.SU_MenuName));
				}
			}

			var disclaimer = ZString.Empty;
			if (!TestingState.IsRunningOnDAT)
			{
				disclaimer = "NB: Please ignore if the following sections are to be moved into [System Documents Elements.xml] upon check-in.  In other words, do not check-in [Customize Documents Elements.xml].\r\n\r\n";
			}

			Assert(
@"The following template sections do not exist in the system section repository.
Either remove the config item from the document config, or create the section in [System Documents Elements.xml].

" + disclaimer + errors.ToStringWithNewLineBetweenAppends(),
				errors.IsEmpty);
		}

		public void TestNoDuplicateSectionNamesAfterPunctuationAndSpacingIsRemoved()
		{
			var ignoredSections = new List<string>()
			{
				"Packages (Separator |_           _|)",
				"Packages (Separator |_   _|_   _|)",
				"Packages (Separator |_______|)",
				"Packages (Separator |___|___|)"
			};
			var errors = new ZStringBuilder();
			var templates = ConfigurableTemplateTestHelper.GetAllSystemConfigurableTemplates(Factory);

			foreach (var template in templates)
			{
				ExcelTemplate excelTemplate = new ExcelTemplateReadFromStmTemplateTable(template);
				var templateSections = new TemplateSectionCollection(excelTemplate);
				var existingTemplateSections = new Dictionary<string, TemplateSection>();

				foreach (TemplateSection templateSection in templateSections)
				{
					var sectionNameWithoutSpacesOrPunctuation = templateSection.SectionName.ExcludeChars(" ,./<>?:;\"'{[}]|\\-_=+!@#$%^&*()").ToUpper();

					TemplateSection existingTemplateSection;
					if (existingTemplateSections.TryGetValue(sectionNameWithoutSpacesOrPunctuation, out existingTemplateSection))
					{
						if (!ignoredSections.Contains(templateSection.SectionName))
						{
							errors.Append(string.Format("Template section [{0}, {1}] is similar to [{2}, {3}].",
								templateSection.TypeCode, templateSection.SectionName, existingTemplateSection.TypeCode, existingTemplateSection.SectionName));
						}
					}
					else
					{
						existingTemplateSections.Add(sectionNameWithoutSpacesOrPunctuation, templateSection);
					}
				}
			}

			Assert(
@"The following template sections may be duplicates.
If the section is not a duplicate, add to the exclusions list.

" + errors.ToStringWithNewLineBetweenAppends(), errors.IsEmpty);
		}

		public void TestAllSectionNamesAreUniqueInAllCustomizableTemplates()
		{
			var errors = new ZStringBuilder();
			var templates = ConfigurableTemplateTestHelper.GetAllSystemConfigurableTemplates(Factory);

			foreach (var template in templates)
			{
				ExcelTemplate excelTemplate = new ExcelTemplateReadFromStmTemplateTable(template);
				var templateSections = new TemplateSectionCollection(excelTemplate);
				var existingTemplateSections = new Dictionary<string, TemplateSection>();

				foreach (TemplateSection templateSection in templateSections)
				{
					TemplateSection existingTemplateSection;
					if (existingTemplateSections.TryGetValue(templateSection.SectionName, out existingTemplateSection))
					{
						errors.Append(string.Format("The name of template section [{0}, {1}] is the same as [{2}, {3}] in template [{4}].",
								templateSection.TypeCode, templateSection.SectionName, existingTemplateSection.TypeCode, existingTemplateSection.SectionName, template.ExcelTemplateFullPath));
					}
					else
					{
						existingTemplateSections.Add(templateSection.SectionName, templateSection);
					}
				}
			}

			Assert(
@"The following template sections names are not unique.
Rename the sections to be unique.

" + errors.ToStringWithNewLineBetweenAppends(), errors.IsEmpty);
		}

		#region Implementation

		bool IsExpandingCellContent(string cellContent)
		{
			return
				cellContent.StartsWith("#SectionBody:Data", StringComparison.InvariantCultureIgnoreCase) ||
				cellContent.StartsWith("#SectionHeader", StringComparison.InvariantCultureIgnoreCase) ||
				cellContent.StartsWith("#SectionPageHeader", StringComparison.InvariantCultureIgnoreCase) ||
				cellContent.StartsWith("#GroupBy", StringComparison.InvariantCultureIgnoreCase) ||
				cellContent.StartsWith("#SectionPageFooter", StringComparison.InvariantCultureIgnoreCase) ||
				cellContent.StartsWith("#SectionFooter", StringComparison.InvariantCultureIgnoreCase) ||
				AutoHeight.RegexToFindMacroAnyWhereInString.IsMatch(cellContent);
		}

		bool IsConditionalCellContent(string cellContent)
		{
			return
				cellContent.StartsWith("#if", StringComparison.InvariantCultureIgnoreCase) ||
				cellContent.StartsWith("#else", StringComparison.InvariantCultureIgnoreCase) ||
				cellContent.StartsWith("#endif", StringComparison.InvariantCultureIgnoreCase);
		}

		StmTemplateBase[] GetTemplatesToTest()
		{
			var query = new ZQuery(StmTemplateSchema.SO_Name, SectionRepositoryTemplateNames.System)
			{
				OrderBy = StmTemplateSchema.SO_Name.Name
			};
			var result = Factory.Load<StmTemplateBase>(query);
			AssertEquals(1, result.Length);
			return result;
		}

		bool IsSectionNameIgnored(string sectionName)
		{
			return SectionNamesToIgnore.Find(item => item.Equals(sectionName, StringComparison.InvariantCultureIgnoreCase)) != null;
		}

		List<string> SectionNamesToIgnore
		{
			get
			{
				var result = new List<string>(InvoiceSectionNames);
				result.AddRange(SectionsWithConditionsThatAreLongerThan128Characters);
				result.Add("Current Branch Bank Account Details + Disbursements Total"); // Total Macro needs to be changed.
				return result;
			}
		}

		readonly string[] InvoiceSectionNames = new string[]
		{
			"Invoice Header w/Invoice Letterhead",
			"Invoice Body (inc. Grouping, Roll-Ups and Subtotalling)",
			"Invoice Page Footer (Separator Bar w/Continued)",
			"Invoice Document Footer (inc. Totals and EFT Details)",
			"Invoice Terms and Condions",
			"Deposit Slip Bank(Header)",
			"Deposit Slip Bank(Body)",
			"Deposit Slip Office(Document Header)",
			"Deposit Slip Office(Cheque)",
			"Deposit Slip Office(Cash)",
			"Deposit Slip Office(Direct Credit)",
			"Deposit Slip Office(Credit Card)",
			"Compliance Number Information",
			"Invoice Body (inc. Grouping, Roll-Ups and Subtotalling) - TaxCore",
			"Invoice Charges and Description titles",
			"Separator - Continued Over… with PrintAccumulativeTotalAmountsInMultipageInvoices",
			"Invoice Body (inc. Grouping, Roll-Ups and Subtotalling, CMB and TME Calculator Breakdown Information)"
		};

		readonly string[] SectionsWithConditionsThatAreLongerThan128Characters = new string[]
		{
			"Container List Link To Extra Page",
			"Receiving Dates (SEA)",
		};

		StmTemplateBase GetSystemSectionRepositoryTemplate()
		{
			var query = new ZQuery(StmTemplateSchema.SO_Name, SectionRepositoryTemplateNames.System);
			var templates = Factory.Load<StmTemplateBase>(query);
			AssertEquals("The should be only 1 configurable system template.", 1, templates.Length);
			return templates[0];
		}

		void AssertErrorsByCategory(string message, IDictionary<string, IList<string>> errorsByCategory)
		{
			if (errorsByCategory.Count != 0)
			{
				if (errorsByCategory.Values.Any(categoryErrors => categoryErrors.Count > 0))
				{
					AssertGroupedErrorList(message, errorsByCategory);
					return;
				}
			}

			Assert(true);
		}

		#endregion
	}
}
