using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using CargoWiseOne.ResourceStrings.Testing;
using Enterprise.DocumentEngine.Business;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentEngine.ReportErrorManagement;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.DocumentEngine.Testing.UtilityClasses;
using Enterprise.DocumentEngine.Visualisation;
using Enterprise.DocumentEngineCore;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.DocumentEngineIntegration;
using Enterprise.Environment;
using Enterprise.ExcelTemplates;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using FlexCel.Core;
using NUnit.Framework;
using static Enterprise.DocumentEngine.Testing.DocumentPackTest;
using DataContext = Enterprise.Core.Constants.DataContext;

namespace Enterprise.DocumentEngine.Testing
{
	sealed class DocumentOutputTest : BaseOutputTest
	{
		public void TestOverflowNotesTemplateOutput()
		{
			var overflowDocument = new OverflowNoteDocument();
			overflowDocument.OverflowNotes.Add(new OverflowNote("Test Title", "This is the contents"));
			overflowDocument.OverflowNotes.Add(new OverflowNote("Test Title 2", "This is some other contents"));

			var excelTemplate = new ExcelTemplateReadFromExcelTemplatesSolution(ExcelTemplateReadFromExcelTemplatesSolution.TemplateNames.OverflowNotesTemplate);

			using (var report = GetNewReport(BODocDataProvider.Get(overflowDocument), excelTemplate))
			using (var outputStream = new MemoryStream())
			{
				report.Save(outputStream);

				using (var excel = new ExcelInterface())
				{
					excel.LoadExcelFile(outputStream);

					AssertMultilineASCIIEquals("excel.WorkSheets[0].ToString()", @"
{C}-[Test_OverflowNotesTemplate]   {AR}-[Page 1 of 1]

{C}-[NOTES CONTINUED FROM DOCUMENT BODY]

{C}-[Test Title]
{C}-[This is the contents]

{C}-[Test Title 2]
{C}-[This is some other contents]
".Trim(), excel.WorkSheets[0].ToString());
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestOverflowToFollowPage()
		{
			var dataSource = Factory.New<DummyBusinessObject>();
			dataSource.Z0_VarCharMax = "This is a test to determine how many characters fit on one line which has waaaaaaaay too many characters to fit on the number of lines I've intended. Oh my god... this is so freaking insane I just can't stand it! Now there are so many characters that it's very unlikely that three spare lines will cut it anymore.";

			var dataProviders = new DataProviderList(BODocDataProvider.Get(dataSource));
			var excelTemplate = new ExcelTemplateForUnitTesting("OverflowToFollowPage.xls", TestFilesSubFolder.DocumentTestFiles);

			using (var documentPack = new DocumentPack())
			using (var report = new Report(documentPack, excelTemplate, dataProviders, "Test", null, DocumentDirection.ANY, false))
			using (var outputStream = new MemoryStream())
			{
				report.Save(outputStream);

				using (var excelInterface = new ExcelInterface())
				{
					excelInterface.LoadExcelFile(outputStream);

					AssertMultilineASCIIEquals("excelInterface.WorkSheets[0].ToString()", @"
{B}-[--- Start of Wrap With Continued ---]
{B}-[This is a test to (continued...)]

{B}-[--- Start of Wrap Without Continued ---]
{B}-[This is a test to determine how many]

{B}-[--- Start of Move With Continued ---]
{B}-[(continued...)]

{B}-[--- Start of Move Without Continued ---]

{B}-[--- Start of Move And Keep Original ---]
{B}-[This is a test to determine how many]

{B}-[--- THE END ---]

{C}-[Follow Page]   {AR}-[Page 1 of 1]

{C}-[NOTES CONTINUED FROM DOCUMENT BODY]

{C}-[Header Wrap With Continued]
{C}-[determine how many characters fit on one line which has waaaaaaaay too many characters to fit on the number of lines I've intended. Oh]
{C}-[my god... this is so freaking insane I just can't stand it! Now there are so many characters that it's very unlikely that three spare lines will]
{C}-[cut it anymore.]

{C}-[Header Wrap Without Continued]
{C}-[characters fit on one line which has waaaaaaaay too many characters to fit on the number of lines I've intended. Oh my god... this is so]
{C}-[freaking insane I just can't stand it! Now there are so many characters that it's very unlikely that three spare lines will cut it anymore.]

{C}-[Header Move With Continued]
{C}-[This is a test to determine how many characters fit on one line which has waaaaaaaay too many characters to fit on the number of lines]
{C}-[I've intended. Oh my god... this is so freaking insane I just can't stand it! Now there are so many characters that it's very unlikely that three]
{C}-[spare lines will cut it anymore.]

{C}-[Header Move Without Continued]
{C}-[This is a test to determine how many characters fit on one line which has waaaaaaaay too many characters to fit on the number of lines]
{C}-[I've intended. Oh my god... this is so freaking insane I just can't stand it! Now there are so many characters that it's very unlikely that three]
{C}-[spare lines will cut it anymore.]

{C}-[Header Move And Keep Original]
{C}-[This is a test to determine how many characters fit on one line which has waaaaaaaay too many characters to fit on the number of lines]
{C}-[I've intended. Oh my god... this is so freaking insane I just can't stand it! Now there are so many characters that it's very unlikely that three]
{C}-[spare lines will cut it anymore.]
".Trim(), excelInterface.WorkSheets[0].ToString());
				}
			}
		}

		public void TestDocumentCanHaveNullTopLevelDataSource()
		{
			using (MemoryStream templateStream = new MemoryStream())
			{
				using (ExcelInterface creationExcelInterface = new ExcelInterface())
				{
					creationExcelInterface.NewExcelFile(1);
					ExcelWorkSheet workSheet = creationExcelInterface.WorkSheets[0];
					workSheet[0, 0] = "#config";
					workSheet[1, 0] = "Name=TestDocumentCanHaveNullTopLevelDataSource";
					workSheet[2, 0] = "#SectionBody";
					workSheet[3, 1] = "<CurrentPage> of <TotalPages>";
					workSheet[4, 0] = "#EndOfReport";
					creationExcelInterface.SaveToStream(templateStream);
				}

				var excelTemplate = new ExcelTemplateWrappingStream("Test", templateStream);

				DocumentPack documentPack = new DocumentPack();
				using (var report = new Report(documentPack, excelTemplate, null, "Test", null, DocumentDirection.ANY, false))
				{
					documentPack.Add(report);
					((IReportForUnitTesting)report).GenerateRegardlessOfAnyErrors = true;

					var uiProvider = new PrintTaskForcePreviewTestingUIProvider();
					using (new PrintTaskUIProviderFactory.OverriderForTesting(uiProvider))
					{
						try
						{
							PrintTask printTask = new PrintTask();
							printTask.Add(documentPack);
							printTask.Run(EnvProxy.Instance.Security.None);
						}
						finally
						{
							ExceptionReporterTestListener.Instance.Clear();
						}
					}

					AssertEquals("Report.Errors", ReportErrorManager.HasNoErrors, report.ErrorManager.ToString("Severity: [{0}] Message: [{1}] Cell Content: [{5}]", false));

					AssertMultilineASCIIEquals("uiProvider.LastErrors", null, uiProvider.LastErrors);
					AssertMultilineASCIIEquals("uiProvider.LastSheetRendered", "{B}-[1 of 1]", uiProvider.LastSheetRendered);
				}
			}
		}

		public void TestICanGetToOrgCusCodesFromTheOrgHeader()
		{
			var organisation = Factory.New<OrgHeader>();
			var germany = Factory.Load<RefCountry>(Enterprise.Core.CountryGuids.Instance.Germany);
			organisation.CustomsCodes.AddNew(GermanyOrgCusCodeInfo.OrgCusCodes.ZAP, "12343387A", germany);

			using (MemoryStream templateStream = new MemoryStream())
			{
				using (ExcelInterface creationExcelInterface = new ExcelInterface())
				{
					creationExcelInterface.NewExcelFile(1);
					ExcelWorkSheet workSheet = creationExcelInterface.WorkSheets[0];
					workSheet[0, 0] = "#config";
					workSheet[1, 0] = "Name=TestICanGetToOrgCusCodesFromTheOrgHeader";
					workSheet[2, 0] = "#SectionBody";
					workSheet[3, 1] = "<CustomsCodes[\"ZAP\", \"DE\"].OK_CustomsRegNo>";
					workSheet[4, 0] = "#EndOfReport";
					creationExcelInterface.SaveToStream(templateStream);
				}

				var excelTemplate = new ExcelTemplateWrappingStream("Test", templateStream);
				using (var report = GetNewReport(BODocDataProvider.Get(organisation), excelTemplate))
				using (var outputStream = new MemoryStream())
				{
					((IReportForUnitTesting)report).GenerateRegardlessOfAnyErrors = true;
					report.Save(outputStream);

					AssertEquals("Report.Errors", ReportErrorManager.HasNoErrors, report.ErrorManager.ToString("Severity: [{0}] Message: [{1}] Cell Content: [{5}]", false));

					using (ExcelInterface excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(outputStream);
						ExcelWorkSheet workSheet = excelInterface.WorkSheets[0];
						AssertMultilineASCIIEquals("excelInterface.WorkSheets[0].ToString()", @"{B}-[12343387A]", excelInterface.WorkSheets[0].ToString());
					}
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestContinuousDocumentWithRowsToRepeatAtTopWithAnotherDocumentInPack()
		{
			var documentCommand = Factory.New<DocumentCommand>();
			documentCommand.SU_IsSystemDefined = ZBool.True;

			var template = CreateTemplate("RowsToRepeatAtTop.xls", TestFilesSubFolder.DocumentTestFiles);
			var pivot = documentCommand.Documents.AddNew();
			pivot.SI_SU = documentCommand.PK;
			pivot.SI_SO = template.PK;

			var template2 = CreateTemplate("RowsToRepeatAtTop.xls", TestFilesSubFolder.DocumentTestFiles);
			template2.SO_Name = "Template 2";
			var pivot2 = documentCommand.Documents.AddNew();
			pivot2.SI_SU = documentCommand.PK;
			pivot2.SI_SO = template2.PK;

			var dummy = Factory.New<DummyBODocSupportable>();
			for (var index = 0; index < 100; index++)
			{
				var child = dummy.Collection.AddNew();
				child.Z0_VarCharMax = string.Format("Dummy {0}", index);
			}

			Factory.Save();

			var result = DocumentEngineTestHelper.ExecuteDocumentCommand(documentCommand, dummy, null);

			using (var excelInterface = new ExcelInterface())
			{
				excelInterface.LoadExcelFile(result.Output);

				AssertEquals("excelInterface.WorkSheets.Count", 3, excelInterface.WorkSheets.Count);

				CombineAssertions(() =>
				{
					AssertEquals("PrintTitlesRangeFormula for sheet 1", "='1 - Document'!$1:$2", DocumentEngineTestHelper.GetPrintTitlesRangeFormula(excelInterface, 0));
					AssertEquals("PrintTitlesRangeFormula for sheet 2", "='2 - Document'!$1:$2", DocumentEngineTestHelper.GetPrintTitlesRangeFormula(excelInterface, 1));
				});
			}
		}

		public void TestNoFilteringOccursIfThereIsFieldNotFoundException()
		{
			var dataSource = Factory.New<DummyBusinessObject>();
			dataSource.Z0_VarCharMax = "Main Data";

			for (int index = 1; index < 5; index++)
			{
				var line = dataSource.Collection.AddNew();
				line.Z0_VarCharMax = "Child Thing " + index.ToString();
				line.Z0_Number = index;
			}

			using (MemoryStream templateStream = new MemoryStream())
			{
				DocumentEngineTestHelper.GenerateTemplateStreamFromString(templateStream, @"{A}-[#Config]
{A}-[Name=TestReportWithDataProviderListPassedIntoConstructor]
{A}-[#SectionBody:Data=Collection:FilterBy(<Self.XXX> == 3)]
{B}-[<Collection.Z0_VarCharMax>]
{A}-[#EndOfReport]");

				var excelTemplate = new ExcelTemplateWrappingStream("Test", templateStream);
				using (var report = GetNewReport(BODocDataProvider.Get(dataSource), excelTemplate))
				using (var outputStream = new MemoryStream())
				{
					((IReportForUnitTesting)report).GenerateRegardlessOfAnyErrors = true;
					report.Save(outputStream);
					AssertEquals("report.ErrorManager.HasErrors", true, report.ErrorManager.HasErrors);
					AssertEquals("report.ErrorManager.HasWarningsOnly", true, report.ErrorManager.HasWarningsOnly);
					AssertEquals("Report.Errors", @"Severity: [Warning (without error report)] Message: [Field <Self.XXX from [Self.XXX == 3]> not found on DataSource Type [DummyChildBusinessObject].] Cell: [A3] Cell Content: [#SectionBody:Data=Collection:FilterBy(<Self.XXX> == 3)]",
												report.ErrorManager.ToString("Severity: [{0}] Message: [{1}] Cell: [{2}] Cell Content: [{5}]", false));

					using (ExcelInterface excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(outputStream);
						ExcelWorkSheet workSheet = excelInterface.WorkSheets[0];
						AssertMultilineASCIIEquals("excelInterface.WorkSheets[0].ToString()", @"
{B}-[Child Thing 1]
{B}-[Child Thing 2]
{B}-[Child Thing 3]
{B}-[Child Thing 4]
".Trim(), excelInterface.WorkSheets[0].ToString());
					}
				}
			}
		}

		public void TestFiltersRespondToChildBusinessObjects()
		{
			var dataSource = Factory.New<DummyBusinessObject>();
			dataSource.Z0_VarCharMax = "Main Data";

			for (int index = 1; index < 5; index++)
			{
				DummyChildBusinessObject line = dataSource.Collection.AddNew();
				line.Z0_VarCharMax = "Child Thing " + index.ToString();
				line.Z0_Number = index;
			}

			var dataProviders = new DataProviderList(BODocDataProvider.Get(dataSource));

			using (Stream templateStream = new MemoryStream())
			{
				DocumentEngineTestHelper.GenerateTemplateStreamFromString(templateStream, @"{A}-[#Config]
{A}-[Name=TestReportWithDataProviderListPassedIntoConstructor]
{A}-[#SectionBody:Data=Collection:FilterBy(<Self.Self.Z0_Number> < 3)]
{B}-[<Collection.Z0_VarCharMax>]
{A}-[#EndOfReport]");
				var excelTemplate = new ExcelTemplateWrappingStream("Test", templateStream);

				using (var documentPack = new DocumentPack())
				using (var report = new Report(documentPack, excelTemplate, dataProviders, "Test", null, DocumentDirection.ANY, false))
				using (MemoryStream outputStream = new MemoryStream())
				{
					((IReportForUnitTesting)report).GenerateRegardlessOfAnyErrors = true;
					report.Save(outputStream);
					using (ExcelInterface excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(outputStream);
						ExcelWorkSheet workSheet = excelInterface.WorkSheets[0];
						AssertMultilineASCIIEquals("excelInterface.WorkSheets[0].ToString()", @"
{B}-[Child Thing 1]
{B}-[Child Thing 2]
".Trim(), excelInterface.WorkSheets[0].ToString());
					}
				}
			}
		}

		public void TestBackPageGetsTheSamePageNumberAsTheFrontPageItsRenderedOnTheBackOf()
		{
			var dataSource = Factory.New<DummyBusinessObject>();
			dataSource.Z0_VarCharMax = "Main Data";

			for (int index = 0; index < 40; index++)
			{
				dataSource.Collection.AddNew().Z0_VarCharMax = "Child Thing";
			}

			var dataProviders = new DataProviderList(BODocDataProvider.Get(dataSource));

			using (Stream templateStream = new MemoryStream())
			{
				DocumentEngineTestHelper.GenerateTemplateStreamFromString(templateStream, @"{A}-[#Config]
{A}-[Name=TestReportWithDataProviderListPassedIntoConstructor]
{A}-[#SectionBody:Data=Collection]
{B}-[<Collection.Z0_VarCharMax>]
{B}-[Page: <CurrentPage>]
{A}-[#BackPage]

{B}-[<Z0_VarCharMax>]
{B}-[Back Page: <CurrentPage>]

{A}-[#EndOfReport]");
				var excelTemplate = new ExcelTemplateWrappingStream("Test", templateStream);

				using (var documentPack = new DocumentPack())
				using (var report = new Report(documentPack, excelTemplate, dataProviders, "Test", null, DocumentDirection.ANY, false))
				using (MemoryStream outputStream = new MemoryStream())
				{
					((IReportForUnitTesting)report).GenerateRegardlessOfAnyErrors = true;
					report.Save(outputStream);
					using (ExcelInterface excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(outputStream);
						ExcelWorkSheet workSheet = excelInterface.WorkSheets[0];
						AssertMultilineASCIIEquals("excelInterface.WorkSheets[0].ToString()", @"
{B}-[Child Thing]
{B}-[Page: 1]
{B}-[Child Thing]
{B}-[Page: 1]
{B}-[Child Thing]
{B}-[Page: 1]
{B}-[Child Thing]
{B}-[Page: 1]
{B}-[Child Thing]
{B}-[Page: 1]
{B}-[Child Thing]
{B}-[Page: 1]
{B}-[Child Thing]
{B}-[Page: 1]
{B}-[Child Thing]
{B}-[Page: 1]
{B}-[Child Thing]
{B}-[Page: 1]
{B}-[Child Thing]
{B}-[Page: 1]
{B}-[Child Thing]
{B}-[Page: 1]
{B}-[Child Thing]
{B}-[Page: 1]
{B}-[Child Thing]
{B}-[Page: 1]
{B}-[Child Thing]
{B}-[Page: 1]
{B}-[Child Thing]
{B}-[Page: 1]
{B}-[Child Thing]
{B}-[Page: 1]
{B}-[Child Thing]
{B}-[Page: 1]
{B}-[Child Thing]
{B}-[Page: 1]
{B}-[Child Thing]
{B}-[Page: 1]
{B}-[Child Thing]
{B}-[Page: 1]
{B}-[Child Thing]
{B}-[Page: 1]
{B}-[Child Thing]
{B}-[Page: 1]
{B}-[Child Thing]
{B}-[Page: 1]
{B}-[Child Thing]
{B}-[Page: 1]
{B}-[Child Thing]
{B}-[Page: 1]
{B}-[Child Thing]
{B}-[Page: 1]
{B}-[Child Thing]
{B}-[Page: 1]
{B}-[Child Thing]

{B}-[Main Data]
{B}-[Back Page: 1]

{B}-[Page: 2]
{B}-[Child Thing]
{B}-[Page: 2]
{B}-[Child Thing]
{B}-[Page: 2]
{B}-[Child Thing]
{B}-[Page: 2]
{B}-[Child Thing]
{B}-[Page: 2]
{B}-[Child Thing]
{B}-[Page: 2]
{B}-[Child Thing]
{B}-[Page: 2]
{B}-[Child Thing]
{B}-[Page: 2]
{B}-[Child Thing]
{B}-[Page: 2]
{B}-[Child Thing]
{B}-[Page: 2]
{B}-[Child Thing]
{B}-[Page: 2]
{B}-[Child Thing]
{B}-[Page: 2]
{B}-[Child Thing]
{B}-[Page: 2]

{B}-[Main Data]
{B}-[Back Page: 2]

".Trim(), excelInterface.WorkSheets[0].ToString());
					}
				}
			}
		}

		public void TestReportWithDataProviderListPassedIntoConstructor()
		{
			var fred = new Fred();
			var bill = new Bill();

			var dataProviders = new DataProviderList(fred, bill);

			using (Stream templateStream = new MemoryStream())
			{
				DocumentEngineTestHelper.GenerateTemplateStreamFromString(templateStream, @"{A}-[#Config]
{A}-[Name=TestReportWithDataProviderListPassedIntoConstructor]
{A}-[#SectionBody]
{B}-[<Name>]
{B}-[<Job>]
{B}-[<UnemploymentBenefits>]
{B}-[<HasALife>]
{A}-[#EndOfReport]");
				var excelTemplate = new ExcelTemplateWrappingStream("Test", templateStream);

				using (var documentPack = new DocumentPack())
				using (var report = new Report(documentPack, excelTemplate, dataProviders, "Test", null, DocumentDirection.ANY, false))
				using (MemoryStream outputStream = new MemoryStream())
				{
					((IReportForUnitTesting)report).GenerateRegardlessOfAnyErrors = true;
					report.Save(outputStream);
					using (ExcelInterface excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(outputStream);
						ExcelWorkSheet workSheet = excelInterface.WorkSheets[0];
						CombineAssertions(delegate
						{
							AssertMultilineASCIIEquals("excelInterface.WorkSheets[0].ToString()", @"
{B}-[Fred]
{B}-[Eating Hot Dogs]
{B}-[Undeserved]
".Trim(), excelInterface.WorkSheets[0].ToString());

							AssertMultilineASCIIEquals("Report.Errors", @"
Severity: [Warning (without error report)] Message: [Field <HasALife> not found on any of the DataSource Types: [Fred], [Bill].]
".Trim(), report.ErrorManager.ToString("Severity: [{0}] Message: [{1}]", false));
						});
					}
				}
			}
		}

		public void TestAutoHeightMacroWithMinimumRowSet()
		{
			var dummy = Factory.New<DummyBusinessObject>();
			dummy.Z0_VarCharMax = "Line1\r\nLine2";
			var documentSupportable = BODocDataProvider.Get(dummy);
			using (Stream templateStream = new MemoryStream())
			using (var documentPack = new DocumentPack())
			using (var report = DocumentEngineTestHelper.CreateReportFromExcelTemplateContents(documentPack, BODocDataProvider.Get(dummy), templateStream,
@"{A}-[#Config]
{A}-[Name=TestIsDraft]
{A}-[#SectionBody]
{B}-[<Z0_VarCharMax>]
{B}-[<AutoHeight(5)><Z0_VarCharMax>]
{B}-[<AutoHeight><Z0_VarCharMax>]
{B}-[End of Test]
{A}-[#EndOfReport]"))
			{
				using (var printTask = new PrintTask())
				{
					printTask.Add(documentPack);
					documentPack.Add(report);

					var deliveryInstructions = new DeliveryInstructions(documentPack);
					deliveryInstructions.IsDraft = true;
					deliveryInstructions.Destination = DeliveryInstructionDestination.TakenFromContact;
					deliveryInstructions.Recipients.RemoveAndDeleteAll();
					var recipient = deliveryInstructions.Recipients.AddNew();
					recipient.DeliveryMethod = Enterprise.Core.Constants.ContactNotifyModes.Email;
					recipient.AttachmentType = "PDF";
					recipient.Email = "unit.test@cargowise.com";

					printTask.Run(deliveryInstructions);
				}

				var printJobs = new StmPrintJobCollection(Factory);
				printJobs.Load();

				AssertEquals("Pre-condition: printJobs.Length", 1, printJobs.Count);

				using (var excelInterface = new ExcelInterface())
				{
					using (var stream = new MemoryStream(printJobs[0].SP_CustomProperties))
					{
						excelInterface.LoadExcelFile(stream);
						AssertEquals("excelInterface.WorkSheets.Count", 1, excelInterface.WorkSheets.Count);

						AssertMultilineASCIIEquals("excelInterface.WorkSheets[0].ToString()",
@"{B}-[Line1|>Line2]
{B}-[Line1]
{B}-[Line2]

{B}-[Line1]
{B}-[Line2]
{B}-[End of Test]",
							excelInterface.WorkSheets[0].ToString());
					}
				}
			}
		}

		public void TestIsDraftIsTrue()
		{
			TestCaseHelper.ClearTable(StmPrintJobCopyRecipient.Schema.TableName);
			TestCaseHelper.ClearTable(StmPrintJob.Schema.TableName);
			AssertEquals("Pre-condition: printJobs.Length", 0, Factory.Load<StmPrintJob>(new ZQuery()).Length);

			using (Stream templateStream = new MemoryStream())
			using (var documentPack = new DocumentPack())
			using (var report = DocumentEngineTestHelper.CreateReportFromExcelTemplateContents(documentPack, templateStream,
@"{A}-[#Config]
{A}-[Name=TestIsDraft]
{A}-[#SectionBody]
{A}-[#if ""<IsDraft>"" == ""Y""]
{B}-[This is a Draft]
{A}-[#else]
{B}-[This is a Final]
{A}-[#endif]
{A}-[#if ""<IsDraft>"" == ""N""]
{B}-[This is a Final, Don't Place Draft Image in Box]
{A}-[#else]
{B}-[This is a Draft, Place Draft Image in Box]
{A}-[#endif]
{A}-[#EndOfReport]"))
			{
				using (var printTask = new PrintTask())
				{
					printTask.Add(documentPack);
					documentPack.Add(report);

					var deliveryInstructions = new DeliveryInstructions(documentPack);
					deliveryInstructions.IsDraft = true;
					deliveryInstructions.Destination = DeliveryInstructionDestination.TakenFromContact;
					deliveryInstructions.Recipients.RemoveAndDeleteAll();
					var recipient = deliveryInstructions.Recipients.AddNew();
					recipient.DeliveryMethod = Enterprise.Core.Constants.ContactNotifyModes.Email;
					recipient.AttachmentType = "PDF";
					recipient.Email = "unit.test@cargowise.com";

					printTask.Run(deliveryInstructions);
				}

				var printJobs = new StmPrintJobCollection(Factory);
				printJobs.Load();

				AssertEquals("Pre-condition: printJobs.Length", 1, printJobs.Count);

				using (var excelInterface = new ExcelInterface())
				{
					using (var stream = new MemoryStream(printJobs[0].SP_CustomProperties))
					{
						excelInterface.LoadExcelFile(stream);
						AssertEquals("excelInterface.WorkSheets.Count", 1, excelInterface.WorkSheets.Count);

						AssertMultilineASCIIEquals("excelInterface.WorkSheets[0].ToString()",
@"{B}-[This is a Draft]
{B}-[This is a Draft, Place Draft Image in Box]",
							excelInterface.WorkSheets[0].ToString());
					}
				}
			}
		}

		public void TestIsDraftIsFalse()
		{
			TestCaseHelper.ClearTable(StmPrintJobCopyRecipient.Schema.TableName);
			TestCaseHelper.ClearTable(StmPrintJob.Schema.TableName);
			AssertEquals("Pre-condition: printJobs.Length", 0, Factory.Load<StmPrintJob>(new ZQuery()).Length);

			using (Stream templateStream = new MemoryStream())
			using (var documentPack = new DocumentPack())
			using (var report = DocumentEngineTestHelper.CreateReportFromExcelTemplateContents(documentPack, templateStream,
@"{A}-[#Config]
{A}-[Name=TestIsDraft]
{A}-[#SectionBody]
{A}-[#if ""<IsDraft>"" == ""Y""]
{B}-[This is a Draft]
{A}-[#else]
{B}-[This is a Final]
{A}-[#endif]
{A}-[#if ""<IsDraft>"" == ""N""]
{B}-[This is a Final, Don't Place Draft Image in Box]
{A}-[#else]
{B}-[This is a Draft, Place Draft Image in Box]
{A}-[#endif]
{A}-[#EndOfReport]"))
			{
				using (var printTask = new PrintTask())
				{
					printTask.Add(documentPack);
					documentPack.Add(report);

					var deliveryInstructions = new DeliveryInstructions(documentPack);
					deliveryInstructions.IsDraft = false;
					deliveryInstructions.Destination = DeliveryInstructionDestination.TakenFromContact;
					deliveryInstructions.Recipients.RemoveAndDeleteAll();
					var recipient = deliveryInstructions.Recipients.AddNew();
					recipient.DeliveryMethod = Enterprise.Core.Constants.ContactNotifyModes.Email;
					recipient.AttachmentType = "PDF";
					recipient.Email = "unit.test@cargowise.com";

					printTask.Run(deliveryInstructions);
				}

				var printJobs = new StmPrintJobCollection(Factory);
				printJobs.Load();

				AssertEquals("Pre-condition: printJobs.Length", 1, printJobs.Count);

				using (var excelInterface = new ExcelInterface())
				{
					using (var stream = new MemoryStream(printJobs[0].SP_CustomProperties))
					{
						excelInterface.LoadExcelFile(stream);
						AssertEquals("excelInterface.WorkSheets.Count", 1, excelInterface.WorkSheets.Count);

						AssertMultilineASCIIEquals("excelInterface.WorkSheets[0].ToString()",
@"{B}-[This is a Final]
{B}-[This is a Final, Don't Place Draft Image in Box]",
							excelInterface.WorkSheets[0].ToString());
					}
				}
			}
		}

		public void TestResCurrentLanguage()
		{
			using (IMockResourceStringCache mockResourceStrings = Res.GetLanguageInstance(Core.SharedConstants.Languages.German).UseMockData())
			{
				mockResourceStrings.Put("StringForTestingKey", new ResourceStringData("", "Müssen es besser machen"));

				BusinessObject bo = Factory.New<DummyBusinessObjectWithResCurrentLanguage>();

				using (MemoryStream templateStream1 = new MemoryStream())
				using (MemoryStream templateStream2 = new MemoryStream())
				{
					using (ExcelInterface creationExcelInterface = new ExcelInterface())
					{
						creationExcelInterface.NewExcelFile(1);
						ExcelWorkSheet workSheet = creationExcelInterface.WorkSheets[0];
						workSheet[0, 0] = "#config";
						workSheet[1, 0] = "Name=TemplateFromStream";
						workSheet[2, 0] = "PageStyle=Portrait";
						workSheet[3, 0] = "#SectionBody";
						workSheet[4, 1] = "<CurrentLanguage>";
						workSheet[5, 1] = "<ResDotGetStringProperty>";
						workSheet[6, 1] = "<MultiLanguageRegistryItemCastOutToZString>";
						workSheet[7, 0] = "#EndOfReport";
						creationExcelInterface.SaveToStream(templateStream1);
						creationExcelInterface.SaveToStream(templateStream2);
					}

					Pack.StmMenuCommand.SU_IsLocalDocument = true;
					ExcelTemplateWrappingStream excelTemplate = new ExcelTemplateWrappingStream("TemplateFromStream", templateStream1);
					using (Report report = GetNewReport(BODocDataProvider.Get(bo), excelTemplate))
					using (MemoryStream outputStream = new MemoryStream())
					{
						report.Save(outputStream);
						using (ExcelInterface xlInterface = new ExcelInterface())
						{
							xlInterface.LoadExcelFile(outputStream);
							ExcelWorkSheet workSheet = xlInterface.WorkSheets[0];
							AssertMultilineASCIIEquals("Generated Results", "{B}-[EN]\r\n{B}-[Must do better]\r\n{B}-[Hello]".Trim(), workSheet.ToString(new CellFormatterExposingFormulae()));
						}
					}

					excelTemplate = new ExcelTemplateWrappingStream("TemplateFromStream", templateStream2);
					using (Report report = GetNewReport(BODocDataProvider.Get(bo), excelTemplate))
					using (MemoryStream outputStream = new MemoryStream())
					{
						report.Parent.Language = Core.SharedConstants.Languages.German;
						report.Save(outputStream);
						using (ExcelInterface xlInterface = new ExcelInterface())
						{
							xlInterface.LoadExcelFile(outputStream);
							ExcelWorkSheet workSheet = xlInterface.WorkSheets[0];
							AssertMultilineASCIIEquals("Generated Results", "{B}-[DE-DE]\r\n{B}-[Müssen es besser machen]\r\n{B}-[Güten Tag]".Trim(), workSheet.ToString(new CellFormatterExposingFormulae()));
						}
					}
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestAutoHeightMacroInPageHeaderThatStartsOnSecondPageHasCorrectPages()
		{
			DummyBusinessObject dummy = Factory.New<DummyBusinessObject>();
			dummy.Z0_VarCharMax = "Very Long Text For Page Header. Very Long Text For Page Header. Very Long Text For Page Header.";

			for (int index = 1; index <= 100; index++)
			{
				var childDummy = dummy.Collection.AddNew();
				childDummy.Z0_VarCharMax = ZString.Format("Dummy {0}", index);
			}

			ExcelTemplate excelTemplate = new ExcelTemplateForUnitTesting("AutoHeightMacroInPageHeaderThatStartsOnSecondPage.xls", TestFilesSubFolder.DocumentTestFiles);
			using (Report report = GetNewReport(BODocDataProvider.Get(dummy), excelTemplate))
			{
				using (Stream stream = new MemoryStream())
				{
					report.Save(stream);

					using (ExcelInterface excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(stream);
						AssertEquals("excelInterface.WorkSheets.Count", 1, excelInterface.WorkSheets.Count);
						ExcelWorkSheet excelWorkSheet = excelInterface.WorkSheets[0];

						AssertNotContains("excelWorkSheet.ToString()", "Page 1 of 3", excelWorkSheet.ToString());
						AssertContains("excelWorkSheet.ToString()", "Page 2 of 3", excelWorkSheet.ToString());
						AssertContains("excelWorkSheet.ToString()", "Page 3 of 3", excelWorkSheet.ToString());

						using (Stream tiffStream = new MemoryStream())
						{
							excelInterface.ExportToMultiPageTiffAndScale(tiffStream, false, 100, false, Env.Registry.PDFTIFResolution, PixelFormat.Format24bppRgb, null);

							using (Image image = Image.FromStream(tiffStream))
							{
								AssertEquals("image.GetPageCount()", report.Renderer.Pages.Count, image.GetPageCount());
							}
						}
					}
				}
			}
		}

		public void TestIfWithGreaterThanInMacroContent()
		{
			DocumentEngineTestHelper helper = new DocumentEngineTestHelper();

			using (Stream templateStream = helper.GetNewTemplateStream("<If(1==0, \"<Z0_VarCharMax>\", \"<Z0_Description>\")>"))
			{
				DummyBusinessObject businessObject = Factory.New<DummyBusinessObject>();
				businessObject.Z0_Description = "<<<-\"->>>";
				ExcelTemplateWrappingStream excelTemplate = new ExcelTemplateWrappingStream("TemplateFromStream", templateStream);

				DocumentCommand stmMenuItem = Factory.New<DocumentCommand>();
				using (DocumentPack pack = new DocumentPack(stmMenuItem))
				using (Report report = new Report(pack, excelTemplate, BODocDataProvider.Get(businessObject), "Test_" + excelTemplate.TemplateName, null, DocumentDirection.ANY, false))
				using (MemoryStream outputStream = new MemoryStream())
				{
					report.Save(outputStream);

					using (ExcelInterface excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(outputStream);
						AssertEquals("Funny string should have got through no worries.", "{B}-[<<<-\"->>>]", excelInterface.WorkSheets[0].ToString());
					}
				}
			}
		}

		public void TestCountMacroReportsErrorsElegantly()
		{
			DocumentEngineTestHelper helper = new DocumentEngineTestHelper();

			using (MemoryStream templateStream = new MemoryStream())
			{
				using (ExcelInterface creationExcelInterface = new ExcelInterface())
				{
					creationExcelInterface.NewExcelFile(1);
					ExcelWorkSheet workSheet = creationExcelInterface.WorkSheets[0];
					workSheet[0, 0] = "#config";
					workSheet[1, 0] = "Name=TemplateFromStream";
					workSheet[2, 0] = "PageStyle=Portrait";
					workSheet[3, 0] = "#DocumentHeader";
					workSheet[4, 1] = "<Count(abc)>";
					workSheet[5, 0] = "#EndOfReport";
					creationExcelInterface.SaveToStream(templateStream);
				}

				DummyBusinessObject businessObject = Factory.New<DummyBusinessObject>();
				ExcelTemplateWrappingStream excelTemplate = new ExcelTemplateWrappingStream("TemplateFromStream", templateStream);

				using (Report report = new Report(Pack, excelTemplate, BODocDataProvider.Get(businessObject), "Test_" + excelTemplate.TemplateName, null, DocumentDirection.ANY, false))
				{
					using (MemoryStream outputStream = new MemoryStream())
					{
						((IReportForUnitTesting)report).GenerateRegardlessOfAnyErrors = true;
						report.Save(outputStream);

						AssertEquals("Report.Errors", @"Severity: [Warning (without error report)] Message: [Error in Count Macro: Evaluating <Count(abc)> :- Field <abc> not found on DataSource Type [DummyBusinessObject]. Input macro: [<Count(abc)>]] Cell: [B5]",
													report.ErrorManager.ToString("Severity: [{0}] Message: [{1}] Cell: [{2}]", false));
					}
				}
			}
		}

		public void TestFieldNotFoundExceptionReplacesWithBlankInExcelFunctions()
		{
			DummyBusinessObject bo = Factory.New<DummyBusinessObject>();
			bo.Z0_VarCharMax = "Other";

			using (MemoryStream templateStream = new MemoryStream())
			{
				using (ExcelInterface creationExcelInterface = new ExcelInterface())
				{
					creationExcelInterface.NewExcelFile(1);
					ExcelWorkSheet workSheet = creationExcelInterface.WorkSheets[0];
					workSheet[0, 0] = "#config";
					workSheet[1, 0] = "Name=TemplateFromStream";
					workSheet[2, 0] = "PageStyle=Portrait";
					workSheet[3, 0] = "#SectionBody";
					workSheet[4, 1] = new TFormula(@"=IF(""<Z0_FieldDoesNotExistInAFitOfBlueCheese>""=""Fred"",""Yes"",""<Z0_VarCharMax>"")");
					workSheet[5, 0] = "#EndOfReport";
					creationExcelInterface.SaveToStream(templateStream);
				}

				ExcelTemplateWrappingStream excelTemplate = new ExcelTemplateWrappingStream("TemplateFromStream", templateStream);
				using (Report report = GetNewReport(BODocDataProvider.Get(bo), excelTemplate))
				using (MemoryStream outputStream = new MemoryStream())
				{
					((IReportForUnitTesting)report).GenerateRegardlessOfAnyErrors = true;
					report.Save(outputStream);
					AssertEquals("report.ErrorManager.HasErrors", true, report.ErrorManager.HasErrors);
					AssertEquals("report.ErrorManager.IsWarningOnly", true, report.ErrorManager.HasWarningsOnly);
					AssertEquals("Report.Errors", @"Severity: [Warning (without error report)] Message: [Field <Z0_FieldDoesNotExistInAFitOfBlueCheese> not found on DataSource Type [DummyBusinessObject].] Cell: [B5]",
												report.ErrorManager.ToString("Severity: [{0}] Message: [{1}] Cell: [{2}]", false));

					using (ExcelInterface excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(outputStream);
						ExcelWorkSheet workSheet = excelInterface.WorkSheets[0];
						TFormula formula = workSheet[0, 1] as TFormula;
						AssertNotNull("workSheet[0, 1] as TFormula", formula);
						AssertEquals("workSheet[0, 1].FormulaText", @"=IF(""""=""Fred"",""Yes"",""Other"")", formula.Text);
						AssertEquals("Whole Template should be empty", "{B}-[Other]", workSheet.ToString());
					}
				}
			}
		}

		public void TestExcelIfWithOverlengthParameterReportsErrorGracefully()
		{
			DummyBusinessObject bo = Factory.New<DummyBusinessObject>();
			bo.Z0_VarCharMax = "This string is 300 characters long. WAY too long. ".PadRight(300, 'X');

			using (MemoryStream templateStream = new MemoryStream())
			{
				using (ExcelInterface creationExcelInterface = new ExcelInterface())
				{
					creationExcelInterface.NewExcelFile(1);
					ExcelWorkSheet workSheet = creationExcelInterface.WorkSheets[0];
					workSheet[0, 0] = "#config";
					workSheet[1, 0] = "Name=TemplateFromStream";
					workSheet[2, 0] = "PageStyle=Portrait";
					workSheet[3, 0] = "#SectionBody";
					workSheet[4, 1] = new TFormula(@"=IF(""<Z0_VarCharMax>""=""Fred"",""Yes"",""No"")");
					workSheet[5, 0] = "#EndOfReport";
					creationExcelInterface.SaveToStream(templateStream);
				}

				ExcelTemplateWrappingStream excelTemplate = new ExcelTemplateWrappingStream("TemplateFromStream", templateStream);
				using (Report report = GetNewReport(BODocDataProvider.Get(bo), excelTemplate))
				using (MemoryStream outputStream = new MemoryStream())
				{
					((IReportForUnitTesting)report).GenerateRegardlessOfAnyErrors = true;
					report.Save(outputStream);
					AssertEquals("report.ErrorManager.HasErrors", true, report.ErrorManager.HasErrors);
					AssertEquals("report.ErrorManager.IsWarningOnly", true, report.ErrorManager.HasWarningsOnly);
					AssertEquals("Report.Errors", @"Severity: [Warning (without error report)] Message: [Error Replacing Macros in [=IF(""<Z0_VarCharMax>""=""Fred"",""Yes"",""No"")] - String constant ""This string is 300 characters long. WAY too long. XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX ... Yes"",""No"")"" is too long. (max 255 chars)] Cell: [B5] Sheetname: [Sheet1]",
												report.ErrorManager.ToString("Severity: [{0}] Message: [{1}] Cell: [{2}] Sheetname: [{3}]", false));

					using (ExcelInterface excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(outputStream);
						ExcelWorkSheet workSheet = excelInterface.WorkSheets[0];
						AssertEquals("Formula in this cell should be wiped out.", "", workSheet[0, 1]);
						AssertEquals("Whole Template should be empty", "", workSheet.ToString());
					}
				}
			}
		}

		public void TestSecondTemplateErrorsGetAddedIntoReportWhenAnalysed()
		{
			DummyBusinessObject bo = Factory.New<DummyBusinessObject>();
			bo.Z0_Code = "EUR";

			using (MemoryStream templateStream = new MemoryStream())
			{
				using (ExcelInterface creationExcelInterface = new ExcelInterface())
				{
					creationExcelInterface.NewExcelFile(3);

					// Add First Tab page with no error.			
					ExcelWorkSheet workSheet = creationExcelInterface.WorkSheets[0];
					workSheet[0, 0] = "#config";
					workSheet[1, 0] = "Name=TemplateFromStream";
					workSheet[2, 0] = "PageStyle=Portrait";
					workSheet[3, 0] = "#SectionBody";
					workSheet[4, 1] = "<Hide RowIf(\"AB CD !=\")>";
					workSheet[4, 2] = "<Z0_Code>";
					workSheet[5, 0] = "#EndOfReport";
					workSheet.SheetNameOverride = "Pizza Hut";
					workSheet.UpdateSheetName();

					//Add Second Tab page with error.			
					workSheet = creationExcelInterface.WorkSheets[1];
					workSheet[0, 0] = "#config";
					workSheet[1, 0] = "Name=TemplateFromStream";
					workSheet[2, 0] = "PageStyle=Portrait";
					workSheet[3, 0] = "#SectionBody:Data=##LinesTest:StartingRowToShow=2:MaximumNumberOfRowsToShow=1";
					workSheet[4, 2] = "<Z0_Code>";
					workSheet[5, 0] = "#EndOfReport";
					workSheet.SheetNameOverride = "Felix the Cat";
					workSheet.UpdateSheetName();

					// Add Third Tab page with no error.
					workSheet = creationExcelInterface.WorkSheets[2];
					workSheet[0, 0] = "#config";
					workSheet[1, 0] = "Name=TemplateFromStream";
					workSheet[2, 0] = "PageStyle=Portrait";
					workSheet[3, 0] = "#SectionBody";
					workSheet[4, 1] = "<Hide RowIf(\"AB CD !=\")>";
					workSheet[4, 2] = "<Z0_Code>";
					workSheet[5, 0] = "#EndOfReport";
					workSheet.SheetNameOverride = "Hello World";
					workSheet.UpdateSheetName();

					creationExcelInterface.SaveToStream(templateStream);
				}

				Pack.StmMenuCommand.SU_IsLocalDocument = true;
				ExcelTemplateWrappingStream excelTemplate = new ExcelTemplateWrappingStream("TemplateFromStream", templateStream);
				using (Report report = GetNewReport(BODocDataProvider.Get(bo), excelTemplate))
				using (MemoryStream outputStream = new MemoryStream())
				{
					((IReportForUnitTesting)report).GenerateRegardlessOfAnyErrors = true;
					AssertNoExceptionThrown(delegate
					{ report.Save(outputStream); });
					AssertEquals("Report.Errors", @"
Severity: [Error (without error report)] Message: [The StartingRowToShow can not be more than MaximumNumberOfRowsToShow] Cell: [A3] Sheetname: [Felix the Cat]
Severity: [Warning (without error report)] Message: [Error in HideRowIf Macro: Result of ""AB CD !="" is not a True/False expression. Input macro: [<Hide RowIf(""AB CD !="")>]] Cell: [B5] Sheetname: [Pizza Hut]
".Trim(),
											report.ErrorManager.ToString("Severity: [{0}] Message: [{1}] Cell: [{2}] Sheetname: [{3}]", false));
				}
			}
		}

		public void TestHideRowIfMacro()
		{
			GlbGroup postMasterGroup = Factory.Load<GlbGroup>(Core.Constants.Groups.PostMastersGroupPK);
			GlbStaff postMaster = postMasterGroup.Staff.AddNew();
			postMaster.GS_EmailAddress = "postmaster@sample.org";
			postMaster.GS_Code = "_O_";
			postMaster.GS_LoginName = "postmastersample";
			Factory.Save();

			DummyBusinessObject bo = Factory.New<DummyBusinessObject>();
			bo.Z0_Code = "EUR";

			using (MemoryStream templateStream = new MemoryStream())
			{
				using (ExcelInterface creationExcelInterface = new ExcelInterface())
				{
					creationExcelInterface.NewExcelFile(1);
					ExcelWorkSheet workSheet = creationExcelInterface.WorkSheets[0];
					workSheet[0, 0] = "#config";
					workSheet[1, 0] = "Name=TemplateFromStream";
					workSheet[2, 0] = "PageStyle=Portrait";
					workSheet[3, 0] = "#SectionBody";
					workSheet[4, 1] = "<Hide RowIf(\"AB CD !=\")>";
					workSheet[4, 2] = "<Z0_Code>";
					workSheet[5, 0] = "#EndOfReport";
					workSheet.SheetNameOverride = "Felix the Cat";
					workSheet.UpdateSheetName();
					creationExcelInterface.SaveToStream(templateStream);
				}

				Pack.StmMenuCommand.SU_IsLocalDocument = true;
				ExcelTemplateWrappingStream excelTemplate = new ExcelTemplateWrappingStream("TemplateFromStream", templateStream);
				using (Report report = GetNewReport(BODocDataProvider.Get(bo), excelTemplate))
				{
					using (MemoryStream outputStream = new MemoryStream())
					{
						((IReportForUnitTesting)report).GenerateRegardlessOfAnyErrors = true;
						report.StTemplate = Factory.New<StmTemplate>();
						report.StTemplate.SO_Name = "DA893";
						report.StTemplate.SO_IsSystemDefined = false;
						report.StTemplate.SO_IsClientSpecific = false;
						report.StTemplate.SO_DataContext = "Shipping";
						report.StTemplate.SO_ExcelTemplatePath = @"abc.xls";

						AssertNoExceptionThrown(delegate
						{ report.Save(outputStream); });
						report.ErrorManager.ReportErrors();

						using (ExcelInterface excelInterface = new ExcelInterface())
						{
							excelInterface.LoadExcelFile(outputStream);
							ExcelWorkSheet workSheet = excelInterface.WorkSheets[0];
							AssertMultilineASCIIEquals("Generated Results", "{C}-[EUR]", workSheet.ToString());
						}
						AssertEquals("report.ErrorManager.HasErrors is true", true, report.ErrorManager.HasErrors);
						AssertEquals("report.ErrorManager.IsWarningOnly is true", true, report.ErrorManager.HasWarningsOnly);
					}

					AssertEquals("Emails to Client", 1, Env.OutgoingMailManager.EmailsCreated.Count);
					EmailDef email = Env.OutgoingMailManager.EmailsCreated[0];
					AssertEquals("email.Recipients", "postmaster@sample.org", email.Recipients.RecipientsAsDelimitedString());
					AssertEquals("Error Generating Document [Test_TemplateFromStream]", email.Subject);
					AssertMultilineASCIIEquals("email.Body", string.Format(@"Report Information:

MenuItem:-
   BusinessContext = []
   Name with Path = []
   Filter = []
   PK = [{0}]
   IsSystemDefined = [N]
   IsClientSpecific = [N]

Template:-
   Name = [DA893]
   DataContext = [Shipping]
   ExcelFilePath = [abc.xls]
   PK = [{1}]
   IsSystemDefined = [N]
   IsClientSpecific = [N]

Not Running from Scheduled Report.

Errors Found
---------------
Severity: [Warning (without error report)] Message: [Error in HideRowIf Macro: Result of ""AB CD !="" is not a True/False expression. Input macro: [<Hide RowIf(""AB CD !="")>]] Cell: [B5] Sheetname: [Felix the Cat] Cell Content: [<Hide RowIf(""AB CD !="")>] Occurences: [1]"
, report.MenuItem.PK, report.StTemplate.PK).Trim(), email.Body.Trim());
				}
			}
			Env.OutgoingMailManager.EmailsCreated.Clear();
		}

		public void TestConditionalTemplates()
		{
			using (MemoryStream templateStream = new MemoryStream())
			{
				Load2PageOptionalTemplateInto(templateStream);
				ExcelTemplateWrappingStream excelTemplate = new ExcelTemplateWrappingStream("TemplateFromStream", templateStream);

				DummyBusinessObject bo = Factory.New<DummyBusinessObject>();
				bo.Z0_VarCharMax = "Hide";

				using (Report report = GetNewReport(BODocDataProvider.Get(bo), excelTemplate))
				using (MemoryStream outputStream = new MemoryStream())
				{
					report.Save(outputStream);

					using (ExcelInterface excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(outputStream);
						AssertEquals("xlInterface.WorkSheets.Count", 2, excelInterface.WorkSheets.Count);
						AssertEquals("Sheet 1 IsHidden", false, excelInterface.WorkSheets[0].IsHidden);
						AssertEquals("Sheet 1 Value", "{B}-[First Worksheet]", excelInterface.WorkSheets[0].ToString());
						AssertEquals("Sheet 2 IsHidden", true, excelInterface.WorkSheets[1].IsHidden);
						AssertEquals("Sheet 2 Value", "", excelInterface.WorkSheets[1].ToString());
					}
				}
			}

			using (MemoryStream templateStream = new MemoryStream())
			{
				Load2PageOptionalTemplateInto(templateStream);
				ExcelTemplateWrappingStream excelTemplate = new ExcelTemplateWrappingStream("TemplateFromStream", templateStream);

				DummyBusinessObject bo = Factory.New<DummyBusinessObject>();
				bo.Z0_VarCharMax = "Show";

				using (Report report = GetNewReport(BODocDataProvider.Get(bo), excelTemplate))
				using (MemoryStream outputStream = new MemoryStream())
				{
					report.Save(outputStream);

					using (ExcelInterface excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(outputStream);
						AssertEquals("xlInterface.WorkSheets.Count", 2, excelInterface.WorkSheets.Count);
						AssertEquals("Sheet 1 IsHidden", false, excelInterface.WorkSheets[0].IsHidden);
						AssertEquals("Sheet 1 Value", "{B}-[First Worksheet]", excelInterface.WorkSheets[0].ToString());
						AssertEquals("Sheet 2 IsHidden", false, excelInterface.WorkSheets[1].IsHidden);
						AssertEquals("Sheet 2 Value", "{B}-[Second Worksheet]", excelInterface.WorkSheets[1].ToString());
					}
				}
			}
		}

		public void TestRecipientNameAndIntendedRecipientAddressMacroIncludesCoverPageWithOfficialContactWithCoverPageRegistryWhenOfficialIsDifferentFromDeliveryContact()
		{
			var helper = new DocumentEngineTestHelper();

			using (var templateStream = helper.GetNewTemplateStream("<RecipientNameAndIntendedRecipientAddress>"))
			{
				var businessObject = Factory.New<DummyBusinessObject>();
				var excelTemplate = new ExcelTemplateWrappingStream("TemplateFromStream", templateStream);

				var stmMenuItem = Factory.New<DocumentCommand>();
				using (var pack = new DocumentPackForDeliveredTest(stmMenuItem))
				{
					using (var deliverable = new Report(pack, excelTemplate, BODocDataProvider.Get(businessObject), "Test_" + excelTemplate.TemplateName, null, DocumentDirection.ANY, false))
					{
						var deliveryContact = DocumentEngineTestHelper.GetNewDocDeliveryContact("Bill", Factory);
						var mostOfficialContact = DocumentEngineTestHelper.GetNewDocDeliveryContact("Fred", Factory);

						DocumentsDataRegistry.Instance.RedirectedDocumentAddressFormatting.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, RedirectedDocumentAddressFormattingOptionList.Codes.OfficialContactWithCoverPage);

						var instructions = new DeliveryInstructions(pack);
						instructions.Recipients.RemoveAndDeleteAll();
						instructions.Recipients.Add(deliveryContact);
						instructions.SetOfficialRecipientForTesting(mostOfficialContact);
						instructions.DeliverablesToBePrinted.Add(deliverable);
						pack.Delivered += (sender, e) =>
						{
							AssertEquals("Pre-condition: pack.Count", 2, pack.Count);
							var coverSheet = pack[0] as Report;
							AssertNotNull("Pre-condition: coverSheet should not be null.", coverSheet);
							AssertContains("coverSheet.WorkSheetCurrentlyBeingProcessed.SheetName", "Cover Sheet", coverSheet.WorkSheetCurrentlyBeingProcessed.SheetName);
						};
						pack.Run(instructions);

						AssertEquals("Pre-condition: pack.Count", 1, pack.Count);
					}
				}
			}
		}

		public void TestRecipientNameAndIntendedRecipientAddressMacroDoesNotIncludesCoverPageWithOfficialContactWithCoverPageRegistryWhenOfficialIsSameAsDeliveryContact()
		{
			DocumentEngineTestHelper helper = new DocumentEngineTestHelper();

			using (Stream templateStream = helper.GetNewTemplateStream("<RecipientNameAndIntendedRecipientAddress>"))
			{
				DummyBusinessObject businessObject = Factory.New<DummyBusinessObject>();
				ExcelTemplateWrappingStream excelTemplate = new ExcelTemplateWrappingStream("TemplateFromStream", templateStream);

				DocumentCommand stmMenuItem = Factory.New<DocumentCommand>();
				using (DocumentPack pack = new DocumentPack(stmMenuItem))
				{
					using (Report deliverable = new Report(pack, excelTemplate, BODocDataProvider.Get(businessObject), "Test_" + excelTemplate.TemplateName, null, DocumentDirection.ANY, false))
					{
						DocDeliveryContact deliveryContact = new DocDeliveryContact(Factory);
						DocDeliveryContact mostOfficialContact = new DocDeliveryContact(Factory);

						mostOfficialContact.DeliveryMethod = Core.Constants.ContactNotifyModes.Print;
						mostOfficialContact.Address1 = "Bill";
						deliveryContact.DeliveryMethod = Core.Constants.ContactNotifyModes.Print;
						deliveryContact.Address1 = "Bill";

						DocumentsDataRegistry.Instance.RedirectedDocumentAddressFormatting.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, RedirectedDocumentAddressFormattingOptionList.Codes.OfficialContactWithCoverPage);

						DeliveryInstructions instructions = new DeliveryInstructions(pack);
						instructions.Recipients.RemoveAndDeleteAll();
						instructions.Recipients.Add(deliveryContact);
						instructions.SetOfficialRecipientForTesting(mostOfficialContact);
						instructions.DeliverablesToBePrinted.Add(deliverable);
						pack.Run(instructions);

						AssertEquals("Pre-condition: pack.Count", 1, pack.Count);
					}
				}
			}
		}

		public void TestRecipientNameAndIntendedRecipientAddressMacroEvaluatesWithOfficialContactOnly()
		{
			DocumentEngineTestHelper helper = new DocumentEngineTestHelper();

			using (Stream templateStream = helper.GetNewTemplateStream("<RecipientNameAndIntendedRecipientAddress>"))
			{
				DummyBusinessObject businessObject = Factory.New<DummyBusinessObject>();
				ExcelTemplateWrappingStream excelTemplate = new ExcelTemplateWrappingStream("TemplateFromStream", templateStream);
				using (Report report = GetNewReport(BODocDataProvider.Get(businessObject), excelTemplate))
				{
					using (MemoryStream fileContent = new MemoryStream())
					{
						DocDeliveryContact deliveryContact = new DocDeliveryContact(Factory);
						DocDeliveryContact mostOfficialContact = new DocDeliveryContact(Factory);

						mostOfficialContact.Address1 = "Bill";
						deliveryContact.Address1 = "Fred";

						DocumentsDataRegistry.Instance.RedirectedDocumentAddressFormatting.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, RedirectedDocumentAddressFormattingOptionList.Codes.OfficialContactOnly);
						report.Save(deliveryContact, mostOfficialContact, fileContent);

						using (ExcelInterface excelInterface = new ExcelInterface())
						{
							excelInterface.LoadExcelFile(fileContent);
							AssertEquals("excelInterface.WorkSheets[0].ToString()", "{B}-[BILL]", excelInterface.WorkSheets[0].ToString());
						}
					}
				}
			}
		}

		public void TestRecipientNameAndIntendedRecipientAddressMacroEvaluatesWithDeliveryContactOnly()
		{
			DocumentEngineTestHelper helper = new DocumentEngineTestHelper();

			using (Stream templateStream = helper.GetNewTemplateStream("<RecipientNameAndIntendedRecipientAddress>"))
			{
				DummyBusinessObject businessObject = Factory.New<DummyBusinessObject>();
				ExcelTemplateWrappingStream excelTemplate = new ExcelTemplateWrappingStream("TemplateFromStream", templateStream);
				using (Report report = GetNewReport(BODocDataProvider.Get(businessObject), excelTemplate))
				{
					using (MemoryStream fileContent = new MemoryStream())
					{
						DocDeliveryContact deliveryContact = new DocDeliveryContact(Factory);
						DocDeliveryContact mostOfficialContact = new DocDeliveryContact(Factory);

						mostOfficialContact.Address1 = "Bill";
						deliveryContact.Address1 = "Fred";

						DocumentsDataRegistry.Instance.RedirectedDocumentAddressFormatting.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, RedirectedDocumentAddressFormattingOptionList.Codes.DeliveryContactOnly);
						report.Save(deliveryContact, mostOfficialContact, fileContent);

						using (ExcelInterface excelInterface = new ExcelInterface())
						{
							excelInterface.LoadExcelFile(fileContent);
							AssertEquals("excelInterface.WorkSheets[0].ToString()", "{B}-[FRED]", excelInterface.WorkSheets[0].ToString());
						}
					}
				}
			}
		}

		public void TestConditionalSectionsEvaluateInnerMacros()
		{
			DummyBusinessObject bo = Factory.New<DummyBusinessObject>();
			bo.Z0_VarCharMax = "Long Long String with 33,000 characters in it:-".PadRight(33000, 'X');
			Assert("Precondition: Z0_VarCharMax.Length > CellContentReplacer.MaximumStringLengthForOneCellInExcel", bo.Z0_VarCharMax.Length > FlxConsts.Max_StringLenInCell);

			using (MemoryStream templateStream = new MemoryStream())
			{
				using (ExcelInterface creationExcelInterface = new ExcelInterface())
				{
					creationExcelInterface.NewExcelFile(1);
					ExcelWorkSheet workSheet = creationExcelInterface.WorkSheets[0];
					workSheet[0, 0] = "#config";
					workSheet[1, 0] = "Name=TemplateFromStream";
					workSheet[2, 0] = "PageStyle=Portrait";
					workSheet[3, 0] = "#SectionBody";
					workSheet[4, 0] = "#if \"<DateTimeAsString('<Now>', 'yyyyMM')>\" > \"200904\"";
					workSheet[5, 1] = "WINNER!!";
					workSheet[6, 0] = "#else";
					workSheet[7, 1] = "LOSER!!";
					workSheet[8, 0] = "#endif";
					workSheet[10, 0] = "#EndOfReport";
					creationExcelInterface.SaveToStream(templateStream);
				}

				ExcelTemplateWrappingStream excelTemplate = new ExcelTemplateWrappingStream("TemplateFromStream", templateStream);
				using (Report report = GetNewReport(BODocDataProvider.Get(bo), excelTemplate))
				using (MemoryStream outputStream = new MemoryStream())
				{
					report.Save(outputStream);

					using (ExcelInterface excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(outputStream);
						ExcelWorkSheet workSheet = excelInterface.WorkSheets[0];
						AssertMultilineASCIIEquals("Expected Output", @"
{B}-[WINNER!!]
".Trim(), workSheet.ToString());
					}
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestPreviewAndVisualiserLooksTheSameWithNewLineInMacro()
		{
			DummyBusinessObject topLevelBO = Factory.New<DummyBusinessObject>();
			topLevelBO.Z0_Date = new ZDateTime(2009, 4, 5);
			topLevelBO.Z0_AnotherDate = new ZDateTime(2009, 6, 7);
			ExcelTemplateForUnitTesting excelTemplate = new ExcelTemplateForUnitTesting("NewLine.xls", TestFilesSubFolder.DocumentTestFiles);

			using (Report report = new Report(Pack, excelTemplate, BODocDataProvider.Get(topLevelBO), "Report Name", null, DocumentDirection.ANY, false))
			{
				AssertType("Pre-condition: Cell is of type TRichString.", typeof(TRichString), report.WorkSheetCurrentlyBeingProcessed[2, 1]);
				AssertType("Pre-condition: Cell is of type string.", typeof(string), report.WorkSheetCurrentlyBeingProcessed[2, 3]);

				using (MemoryStream stream = new MemoryStream())
				{
					report.Save(stream);

					using (ExcelInterface excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(stream);
						AssertEquals(1, excelInterface.WorkSheets.Count);

						ExcelWorkSheet workSheet = excelInterface.WorkSheets[0];
						AssertMultilineASCIIEquals("Before User has Overridden in Visualiser", @"
{B}-[05-Apr-09 00:00]

{B}-[07-Jun-09 00:00]
{B}-[End of TRichString]
{B}-[05-Apr-09 00:00]

{B}-[07-Jun-09 00:00]
{B}-[End of System.String]".Trim(), workSheet.ToString());
					}
				}

				TemplateToVisualiserComponentsConverter converter = new TemplateToVisualiserComponentsConverter(report, report.OverridingDataSet);
				AssertEquals(4, converter.Components.Count);

				VisualiserComponentTextBox richStringTextBox = converter.Components[0] as VisualiserComponentTextBox;
				AssertNotNull(richStringTextBox);
				AssertMultilineASCIIEquals("textBox.GetRenderedControlValue",
@"05-Apr-09 00:00

07-Jun-09 00:00", richStringTextBox.GetRenderedControlValueForTesting().ToString());
				AssertEquals("End of TRichString", converter.Components[1].GetRenderedControlValueForTesting());

				VisualiserComponentTextBox textBox = converter.Components[2] as VisualiserComponentTextBox;
				AssertNotNull(textBox);
				AssertMultilineASCIIEquals("textBox.GetRenderedControlValue",
@"05-Apr-09 00:00

07-Jun-09 00:00", textBox.GetRenderedControlValueForTesting().ToString());
				AssertEquals("End of System.String", converter.Components[3].GetRenderedControlValueForTesting());

				richStringTextBox.SimulateUserSettingValueForTesting(
@"04-Apr-09 00:00

06-Jun-09 00:00

");

				textBox.SimulateUserSettingValueForTesting(
@"04-Apr-09 00:00

06-Jun-09 00:00

");

				using (MemoryStream stream = new MemoryStream())
				{
					report.Save(stream);

					using (ExcelInterface excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(stream);
						AssertEquals(1, excelInterface.WorkSheets.Count);

						ExcelWorkSheet workSheet = excelInterface.WorkSheets[0];
						AssertMultilineASCIIEquals("After User has Overridden in Visualiser", @"
{B}-[04-Apr-09 00:00]

{B}-[06-Jun-09 00:00]
{B}-[End of TRichString]
{B}-[04-Apr-09 00:00]

{B}-[06-Jun-09 00:00]
{B}-[End of System.String]".Trim(), workSheet.ToString());
					}
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestShrinkToFitWorksWithMergedCells()
		{
			ExcelTemplateForUnitTesting excelTemplate = new ExcelTemplateForUnitTesting("ShrinkToFitMacroInMergedCells.xls", TestFilesSubFolder.DocumentTestFiles);

			using (Report report = new Report(Pack, excelTemplate, Guid.Empty, Enterprise.Core.Constants.DataContext.UnitTest))
			{
				report.PrepareForRender();

				using (MemoryStream stream = new MemoryStream())
				{
					report.Save(stream);

					using (ExcelInterface excel = new ExcelInterface())
					{
						excel.LoadExcelFile(stream);

						ExcelWorkSheet workSheet = excel.WorkSheets[0];
						Assert("Pre-condition: (int)workSheet.GetCellFormat(0, 6).Font.Size", (int)workSheet.GetCellFormat(0, 6).FontSize <= 11);
						Assert("Pre-condition: (int)workSheet.GetCellFormat(0, 10).Font.Size", (int)workSheet.GetCellFormat(0, 10).FontSize <= 12);
						Assert("Pre-condition: (int)workSheet.GetCellFormat(0, 14).Font.Size", (int)workSheet.GetCellFormat(0, 14).FontSize <= 11);

						Assert((int)workSheet.GetCellFormat(0, 1).FontSize <= 8);
					}
				}
			}
		}

		public void TestOverlengthCellDataGetsReportedAndTruncated_NonCustomizedReport()
		{
			AssertOverlengthCellDataGetsReportedAndTruncated(false);
		}

		public void TestOverlengthCellDataGetsReportedAndTruncated_CustomizedReport()
		{
			AssertOverlengthCellDataGetsReportedAndTruncated(true);
		}

		public void TestCurrencyMacro()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Germany);

			DummyBusinessObject bo = Factory.New<DummyBusinessObject>();
			bo.Z0_AnotherDecimal = 123.45m;
			bo.Z0_Code = "EUR";

			using (MemoryStream templateStream = new MemoryStream())
			{
				using (ExcelInterface creationExcelInterface = new ExcelInterface())
				{
					creationExcelInterface.NewExcelFile(1);
					ExcelWorkSheet workSheet = creationExcelInterface.WorkSheets[0];
					workSheet[0, 0] = "#config";
					workSheet[1, 0] = "Name=TemplateFromStream";
					workSheet[2, 0] = "PageStyle=Portrait";
					workSheet[3, 0] = "#SectionBody";
					workSheet[4, 1] = "<Currency(<Z0_AnotherDecimal>, <Z0_Code>)>";
					workSheet[5, 0] = "#EndOfReport";
					creationExcelInterface.SaveToStream(templateStream);
				}

				Pack.StmMenuCommand.SU_IsLocalDocument = true;
				ExcelTemplateWrappingStream excelTemplate = new ExcelTemplateWrappingStream("TemplateFromStream", templateStream);
				using (Report report = GetNewReport(BODocDataProvider.Get(bo), excelTemplate))
				using (MemoryStream outputStream = new MemoryStream())
				{
					report.Save(outputStream);
					using (ExcelInterface excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(outputStream);
						ExcelWorkSheet workSheet = excelInterface.WorkSheets[0];
						AssertMultilineASCIIEquals("Generated Results", @"
{B}-[123.45]
".Trim(), workSheet.ToString(new CellFormatterExposingFormulae()));
					}
				}
			}
		}

		public void TestMultilingualStringField()
		{
			using (var mockGrm = Res.GetLanguageInstance(Core.SharedConstants.Languages.German).UseMockData())
			{
				BusinessObject bo = Factory.New<DummyBusinessObjectWithMultilingualString>();

				mockGrm.Put("hello", new ResourceStringData("hello", "Guten Tag"));

				using (MemoryStream templateStream1 = new MemoryStream())
				using (MemoryStream templateStream2 = new MemoryStream())
				{
					using (ExcelInterface creationExcelInterface = new ExcelInterface())
					{
						creationExcelInterface.NewExcelFile(1);
						ExcelWorkSheet workSheet = creationExcelInterface.WorkSheets[0];
						workSheet[0, 0] = "#config";
						workSheet[1, 0] = "Name=TemplateFromStream";
						workSheet[2, 0] = "PageStyle=Portrait";
						workSheet[3, 0] = "#SectionBody";
						workSheet[4, 1] = "<TheMultilingualString>";
						workSheet[5, 0] = "#EndOfReport";
						creationExcelInterface.SaveToStream(templateStream1);
						creationExcelInterface.SaveToStream(templateStream2);
					}

					Pack.StmMenuCommand.SU_IsLocalDocument = true;
					ExcelTemplateWrappingStream excelTemplate = new ExcelTemplateWrappingStream("TemplateFromStream", templateStream1);
					using (Report report = GetNewReport(BODocDataProvider.Get(bo), excelTemplate))
					using (MemoryStream outputStream = new MemoryStream())
					{
						report.Save(outputStream);
						using (ExcelInterface xlInterface = new ExcelInterface())
						{
							xlInterface.LoadExcelFile(outputStream);
							ExcelWorkSheet workSheet = xlInterface.WorkSheets[0];
							AssertMultilineASCIIEquals("Generated Results", @"{B}-[Hello]".Trim(), workSheet.ToString(new CellFormatterExposingFormulae()));
						}
					}

					excelTemplate = new ExcelTemplateWrappingStream("TemplateFromStream", templateStream2);
					using (Report report = GetNewReport(BODocDataProvider.Get(bo), excelTemplate))
					using (MemoryStream outputStream = new MemoryStream())
					{
						report.Parent.Language = Core.SharedConstants.Languages.German;
						report.Save(outputStream);
						using (ExcelInterface xlInterface = new ExcelInterface())
						{
							xlInterface.LoadExcelFile(outputStream);
							ExcelWorkSheet workSheet = xlInterface.WorkSheets[0];
							AssertMultilineASCIIEquals("Generated Results", @"{B}-[Guten Tag]".Trim(), workSheet.ToString(new CellFormatterExposingFormulae()));
						}
					}
				}
			}
		}

		public void TestLanguageIsSetDuringDataPopulation()
		{
			using (Env.Instance.SetTemporaryUserContext(Env.Instance.CurrentUser.PK, Env.Instance.CurrentBranch.PK, Env.Instance.CurrentDepartment.PK))
			{
				AssertEquals("Precondition: ConsumptionLogCreated", false, Env.Licence.LanguagePackLookup[Core.SharedConstants.Languages.German].ConsumptionLogCreated);

				BusinessObject bo = Factory.New<DummyWithCachedWrapperCollection>();

				using (var mockResourceStrings = Res.GetLanguageInstance(Core.SharedConstants.Languages.German).UseMockData())
				using (MemoryStream templateStream = new MemoryStream())
				{
					mockResourceStrings.Put("ResourceStringResolvedInWrapperConstructor", new ResourceStringData("ResourceStringResolvedInWrapperConstructor", "Rotcurtsnoc Repparw Olleh"));

					using (ExcelInterface creationExcelInterface = new ExcelInterface())
					{
						creationExcelInterface.NewExcelFile(1);
						ExcelWorkSheet workSheet = creationExcelInterface.WorkSheets[0];
						workSheet[0, 0] = "#config";
						workSheet[1, 0] = "Name=TemplateFromStream";
						workSheet[2, 0] = "PageStyle=Portrait";
						workSheet[3, 0] = "#SectionBody:Data=MyLines";
						workSheet[4, 1] = "<MyLines.ResolvedResourceString>";
						workSheet[5, 0] = "#EndOfReport";
						creationExcelInterface.SaveToStream(templateStream);
					}

					Pack.StmMenuCommand.SU_IsLocalDocument = true;
					ExcelTemplateWrappingStream excelTemplate = new ExcelTemplateWrappingStream("TemplateFromStream", templateStream);
					using (Report report = GetNewReport(BODocDataProvider.Get(bo), excelTemplate))
					using (MemoryStream outputStream = new MemoryStream())
					{
						report.Parent.Language = Core.SharedConstants.Languages.German;
						report.Save(outputStream);
						using (ExcelInterface xlInterface = new ExcelInterface())
						{
							xlInterface.LoadExcelFile(outputStream);
							ExcelWorkSheet workSheet = xlInterface.WorkSheets[0];
							AssertMultilineASCIIEquals("Generated Results", @"{B}-[Rotcurtsnoc Repparw Olleh]".Trim(), workSheet.ToString(new CellFormatterExposingFormulae()));
						}
					}
				}

				AssertEquals("ConsumptionLogCreated", true, Env.Licence.LanguagePackLookup[Core.SharedConstants.Languages.German].DocBuilderLanguageCheckpoint.ConsumptionLogCreated);
			}
		}

		public void TestLanguageIsSetDuringPrepareForRender()
		{
			BusinessObject bo = Factory.New<DummyWithCachedWrapperCollection>();

			using (var mockResourceStrings = Res.GetLanguageInstance(Core.SharedConstants.Languages.German).UseMockData())
			using (MemoryStream templateStream = new MemoryStream())
			{
				mockResourceStrings.Put("ResourceStringResolvedInWrapperConstructor", new ResourceStringData("ResourceStringResolvedInWrapperConstructor", "Rotcurtsnoc Repparw Olleh"));

				using (ExcelInterface creationExcelInterface = new ExcelInterface())
				{
					creationExcelInterface.NewExcelFile(1);
					ExcelWorkSheet workSheet = creationExcelInterface.WorkSheets[0];
					workSheet[0, 0] = "#config";
					workSheet[1, 0] = "Name=TemplateFromStream";
					workSheet[2, 0] = "PageStyle=Portrait";
					workSheet[3, 0] = "#SectionBody:Data=MyLines";
					workSheet[4, 0] = "#If \"<MyLines.IsTrue>\" == \"Y\"";
					workSheet[5, 1] = "<MyLines.ResolvedResourceString>";
					workSheet[6, 0] = "#EndIf";
					workSheet[7, 0] = "#EndOfReport";
					creationExcelInterface.SaveToStream(templateStream);
				}

				Pack.StmMenuCommand.SU_IsLocalDocument = true;
				ExcelTemplateWrappingStream excelTemplate = new ExcelTemplateWrappingStream("TemplateFromStream", templateStream);
				using (Report report = GetNewReport(BODocDataProvider.Get(bo), excelTemplate))
				using (MemoryStream outputStream = new MemoryStream())
				{
					report.Parent.Language = Core.SharedConstants.Languages.German;

					report.PrepareForRender();

					report.Save(outputStream);
					using (ExcelInterface xlInterface = new ExcelInterface())
					{
						xlInterface.LoadExcelFile(outputStream);
						ExcelWorkSheet workSheet = xlInterface.WorkSheets[0];
						AssertMultilineASCIIEquals("Generated Results", @"{B}-[Rotcurtsnoc Repparw Olleh]".Trim(), workSheet.ToString(new CellFormatterExposingFormulae()));
					}
				}
			}
		}

		public void TestTotalMacroWith2BodySections()
		{
			DummyBusinessObject bo = Factory.New<DummyBusinessObject>();

			DummyChildBusinessObject child = bo.Collection.AddNew();
			child.Z0_Number = 1;
			child.Z0_ChildOnly = 3;

			DummyChildBusinessObject child2 = bo.Collection.AddNew();
			child2.Z0_Number = 5;
			child2.Z0_ChildOnly = 7;

			using (MemoryStream templateStream = new MemoryStream())
			{
				using (ExcelInterface creationExcelInterface = new ExcelInterface())
				{
					creationExcelInterface.NewExcelFile(1);
					ExcelWorkSheet workSheet = creationExcelInterface.WorkSheets[0];
					workSheet[0, 0] = "#config";
					workSheet[1, 0] = "Name=TemplateFromStream";
					workSheet[2, 0] = "PageStyle=Portrait";
					workSheet[3, 0] = "#SectionBody:Data=Collection";
					workSheet[4, 1] = "<Collection.Z0_Number>";
					workSheet[5, 0] = "#SectionFooter";

					workSheet[7, 0] = "#SectionBody:Data=Collection";
					workSheet[8, 2] = "<Collection.Z0_ChildOnly>";
					workSheet[9, 0] = "#SectionFooter";

					workSheet[11, 0] = "#PageFooter";
					workSheet[12, 1] = "<Total Collection.Z0_Number>";
					workSheet[13, 2] = "<Total Collection.Z0_ChildOnly>";
					workSheet[14, 0] = "#EndOfReport";
					creationExcelInterface.SaveToStream(templateStream);
				}

				ExcelTemplateWrappingStream excelTemplate = new ExcelTemplateWrappingStream("TemplateFromStream", templateStream);
				using (Report report = GetNewReport(BODocDataProvider.Get(bo), excelTemplate))
				using (MemoryStream outputStream = new MemoryStream())
				{
					report.Save(outputStream);
					using (ExcelInterface excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(outputStream);
						ExcelWorkSheet workSheet = excelInterface.WorkSheets[0];
						AssertMultilineASCIIEquals("Generated Results", @"
{B}-[1]
{B}-[5]

{C}-[3]
{C}-[7]

{B}-[=SUM(B1:B2)]
{C}-[=SUM(C4:C5)]
".Trim(), workSheet.ToString(new CellFormatterExposingFormulae()));
					}
				}
			}
		}

		public void TestFormatEndToEnd()
		{
			DummyBusinessObject bo = Factory.New<DummyBusinessObject>();
			bo.Z0_Code = "HEY";
			bo.Z0_Description = "A BIT LONGER";
			bo.Z0_VarCharMax = @"<Z0_Code> - LIFE IS SHORT PLAY HARD - <Z0_Description>";

			DummyChildBusinessObject child = bo.Collection.AddNew();
			child.Z0_Code = "BAD";
			child.Z0_Description = "A BIT HAIRIER";

			DummyChildBusinessObject child2 = bo.Collection.AddNew();
			child2.Z0_Code = "FAST";
			child2.Z0_Description = "A BIT STRETCHIER";

			using (MemoryStream templateStream = new MemoryStream())
			{
				using (ExcelInterface creationExcelInterface = new ExcelInterface())
				{
					creationExcelInterface.NewExcelFile(1);
					ExcelWorkSheet workSheet = creationExcelInterface.WorkSheets[0];
					workSheet[0, 0] = "#config";
					workSheet[1, 0] = "Name=TemplateFromStream";
					workSheet[2, 0] = "PageStyle=Portrait";
					workSheet[3, 0] = "#SectionBody";
					workSheet[4, 1] = @"<Format(""{Z0_Code} - {Z0_Description}"")>";
					workSheet[5, 1] = @"<Self.Format(""{Z0_Code} - {Z0_Description}"")>";
					workSheet[6, 1] = @"<Collection.Format(""{Z0_Code} - {Z0_Description}"", Comma)>";
					workSheet[7, 1] = @"<Collection[2].Format(""{Z0_Code} - {Z0_Description}"")>";
					workSheet[8, 1] = @"<Collection.Self.Format(""{Z0_Code} - {Z0_Description}"")>";
					workSheet[9, 0] = "#EndOfReport";
					creationExcelInterface.SaveToStream(templateStream);
				}

				ExcelTemplateWrappingStream excelTemplate = new ExcelTemplateWrappingStream("TemplateFromStream", templateStream);
				using (Report report = GetNewReport(BODocDataProvider.Get(bo), excelTemplate))
				using (MemoryStream outputStream = new MemoryStream())
				{
					report.Save(outputStream);
					using (ExcelInterface excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(outputStream);
						ExcelWorkSheet workSheet = excelInterface.WorkSheets[0];
						AssertMultilineASCIIEquals("Generated Results", @"
{B}-[HEY - A BIT LONGER]
{B}-[HEY - A BIT LONGER]
{B}-[BAD - A BIT HAIRIER, FAST - A BIT STRETCHIER]
{B}-[FAST - A BIT STRETCHIER]
{B}-[BAD - A BIT HAIRIER]
".Trim(), workSheet.ToString());
					}
				}
			}
		}

		public void TestEvaluateInnerContentMacro()
		{
			DummyBusinessObject bo = Factory.New<DummyBusinessObject>();
			bo.Z0_Code = "HEY";
			bo.Z0_Description = "A BIT LONGER";
			bo.Z0_VarCharMax = @"<Z0_Code> - LIFE IS SHORT PLAY HARD - <Z0_Description>";

			using (MemoryStream templateStream = new MemoryStream())
			{
				using (ExcelInterface creationExcelInterface = new ExcelInterface())
				{
					creationExcelInterface.NewExcelFile(1);
					ExcelWorkSheet workSheet = creationExcelInterface.WorkSheets[0];
					workSheet[0, 0] = "#config";
					workSheet[1, 0] = "Name=TemplateFromStream";
					workSheet[2, 0] = "PageStyle=Portrait";
					workSheet[3, 0] = "#SectionBody";
					workSheet[4, 1] = @"<EvaluateInnerContent(""<Z0_VarCharMax>"")>";
					workSheet[5, 1] = @"<AutoHeight><EvaluateInnerContent(""<Z0_VarCharMax>"")>";
					workSheet[6, 1] = "- THE END -";
					workSheet[7, 0] = "#EndOfReport";
					creationExcelInterface.SaveToStream(templateStream);
				}

				ExcelTemplateWrappingStream excelTemplate = new ExcelTemplateWrappingStream("TemplateFromStream", templateStream);
				using (Report report = GetNewReport(BODocDataProvider.Get(bo), excelTemplate))
				using (MemoryStream outputStream = new MemoryStream())
				{
					report.Save(outputStream);
					using (ExcelInterface excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(outputStream);
						ExcelWorkSheet workSheet = excelInterface.WorkSheets[0];
						AssertMultilineASCIIEquals("Generated Results", @"
{B}-[HEY - LIFE IS SHORT PLAY HARD - A BIT LONGER]
{B}-[HEY - LIFE]
{B}-[IS SHORT]
{B}-[PLAY]
{B}-[HARD - A]
{B}-[BIT]
{B}-[LONGER]
{B}-[- THE END -]
".Trim(), workSheet.ToString());
					}
				}
			}
		}

		public void TestTotalOnDecimalValuesWhereGroupByHeaderIsUsed()
		{
			DummyBusinessObject topLevelDataSource = Factory.New<DummyBusinessObject>();
			DummyChildBusinessObject child1 = topLevelDataSource.Collection.AddNew();
			child1.Z0_Code = "ME";
			child1.Z0_Description = "IF-EH-I WOULD WALK FIVE HUNDRED MILES THEN-EH-I WOULD WALK FIVE HUNDRED MORE";
			child1.Z0_AnotherDecimal = 1.11m;
			DummyChildBusinessObject child2 = topLevelDataSource.Collection.AddNew();
			child2.Z0_Code = "ME";
			child2.Z0_Description = "JUST TO BE THAT MAN WHO WALKED THOSE THOUSAND MILES";
			child2.Z0_AnotherDecimal = 2.22m;
			DummyChildBusinessObject child3 = topLevelDataSource.Collection.AddNew();
			child3.Z0_Code = "YOU";
			child3.Z0_Description = "JUST TO FALL DOWN AGAINST YOUR DOOR.";
			child3.Z0_AnotherDecimal = 3.32m;

			using (MemoryStream templateStream = new MemoryStream())
			{
				using (ExcelInterface creationExcelInterface = new ExcelInterface())
				{
					creationExcelInterface.NewExcelFile(1);
					ExcelWorkSheet workSheet = creationExcelInterface.WorkSheets[0];
					workSheet[0, 0] = "#config";
					workSheet[1, 0] = "Name=TemplateFromStream";
					workSheet[2, 0] = "PageStyle=Portrait";
					workSheet[3, 0] = "#PageHeader";
					workSheet[4, 1] = "Page Header";
					workSheet[5, 0] = "#SectionPageHeader";
					workSheet[6, 1] = "Section Page Header";
					workSheet[7, 1] = "";
					workSheet[8, 0] = "#SectionBody:Data=Collection";
					workSheet[9, 1] = "<AutoHeight><Collection.Z0_Description>";
					workSheet[9, 2] = "<Collection.Z0_AnotherDecimal>";
					workSheet[10, 0] = "#GroupBy:Collection.Z0_Code:GroupTitle";
					workSheet[11, 1] = "<Collection.Z0_Code>";
					workSheet[12, 0] = "#GroupBy:Collection.Z0_Code";
					workSheet[13, 1] = "- Total -";
					workSheet[13, 2] = "<Total Collection.Z0_AnotherDecimal> "; // Deliberate Space on end as this was what caused the failure out in the real world.
					workSheet[14, 1] = "";
					workSheet[15, 0] = "#SectionFooter";
					workSheet[16, 1] = "- THE END -";
					workSheet[17, 0] = "#EndOfReport";
					creationExcelInterface.SaveToStream(templateStream);
				}

				ExcelTemplateWrappingStream excelTemplate = new ExcelTemplateWrappingStream("TemplateFromStream", templateStream);
				using (Report report = GetNewReport(BODocDataProvider.Get(topLevelDataSource), excelTemplate))
				using (MemoryStream outputStream = new MemoryStream())
				{
					report.Save(outputStream);
					using (ExcelInterface excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(outputStream);
						ExcelWorkSheet workSheet = excelInterface.WorkSheets[0];
						AssertMultilineASCIIEquals("Generated Results", @"
{B}-[Page Header]
{B}-[Section Page Header]

{B}-[ME]
{B}-[IF-EH-I]   {C}-[1.11]
{B}-[WOULD]
{B}-[WALK FIVE]
{B}-[HUNDRED]
{B}-[MILES]
{B}-[THEN-EH-I]
{B}-[WOULD]
{B}-[WALK FIVE]
{B}-[HUNDRED]
{B}-[MORE]
{B}-[JUST TO]   {C}-[2.22]
{B}-[BE THAT]
{B}-[MAN WHO]
{B}-[WALKED]
{B}-[THOSE]
{B}-[THOUSAN]
{B}-[D MILES]
{B}-[- Total -]   {C}-[3.33]

{B}-[YOU]
{B}-[JUST TO]   {C}-[3.32]
{B}-[FALL]
{B}-[DOWN]
{B}-[AGAINST]
{B}-[YOUR]
{B}-[DOOR.]
{B}-[- Total -]   {C}-[3.32]

{B}-[- THE END -]
".Trim(), workSheet.ToString());
					}
				}
			}
		}

		public void TestSectionBodyDataCaseInSensitive()
		{
			DummyBusinessObject topLevelDataSource = Factory.New<DummyBusinessObject>();
			DummyChildBusinessObject child1 = topLevelDataSource.Collection.AddNew();
			child1.Z0_Code = "One";
			child1.Z0_Description = "One";
			DummyChildBusinessObject child2 = topLevelDataSource.Collection.AddNew();
			child2.Z0_Code = "Two";
			child2.Z0_Description = "Two";
			DummyChildBusinessObject child3 = topLevelDataSource.Collection.AddNew();
			child3.Z0_Code = "Three";
			child3.Z0_Description = "Three";

			using (MemoryStream templateStream = new MemoryStream())
			{
				using (ExcelInterface creationExcelInterface = new ExcelInterface())
				{
					creationExcelInterface.NewExcelFile(1);
					ExcelWorkSheet workSheet = creationExcelInterface.WorkSheets[0];
					workSheet[0, 0] = "#config";
					workSheet[1, 0] = "Name=TemplateFromStream";
					workSheet[2, 0] = "PageStyle=Portrait";
					workSheet[3, 0] = "DataContext=.DummyBusinessObject";
					workSheet[4, 0] = "#SectionBody:Data=Collection";
					workSheet[5, 1] = "<collection.Z0_Code>";
					workSheet[5, 2] = "<Collection.Z0_Description>";
					workSheet[6, 0] = "#EndOfReport";
					creationExcelInterface.SaveToStream(templateStream);
				}

				ExcelTemplateWrappingStream excelTemplate = new ExcelTemplateWrappingStream("TemplateFromStream", templateStream);
				using (Report report = GetNewReport(BODocDataProvider.Get(topLevelDataSource), excelTemplate))
				using (MemoryStream outputStream = new MemoryStream())
				{
					report.Save(outputStream);
					using (ExcelInterface excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(outputStream);
						ExcelWorkSheet workSheet = excelInterface.WorkSheets[0];
						AssertMultilineASCIIEquals("Generated Results", @"
{B}-[One]   {C}-[One]
{B}-[Two]   {C}-[Two]
{B}-[Three]   {C}-[Three]
".Trim(), workSheet.ToString());
					}
				}
			}
		}

		public void TestCountWorksOnBusinessObjectCollection()
		{
			DummyBusinessObject topLevelDataSource = Factory.New<DummyBusinessObject>();
			topLevelDataSource.Collection.AddNew();
			topLevelDataSource.Collection.AddNew();
			topLevelDataSource.Collection.AddNew();
			using (MemoryStream templateStream = new MemoryStream())
			{
				using (ExcelInterface creationExcelInterface = new ExcelInterface())
				{
					creationExcelInterface.NewExcelFile(1);
					ExcelWorkSheet workSheet = creationExcelInterface.WorkSheets[0];
					workSheet[0, 0] = "#config";
					workSheet[1, 0] = "Name=TemplateFromStream";
					workSheet[2, 0] = "PageStyle=Portrait";
					workSheet[3, 0] = "DataContext=.DummyBusinessObject";
					workSheet[4, 0] = "#SectionBody";
					workSheet[5, 1] = "<Collection.Count>";
					workSheet[6, 0] = "#EndOfReport";
					creationExcelInterface.SaveToStream(templateStream);
				}

				ExcelTemplateWrappingStream excelTemplate = new ExcelTemplateWrappingStream("TemplateFromStream", templateStream);
				using (Report report = GetNewReport(BODocDataProvider.Get(topLevelDataSource), excelTemplate))
				using (MemoryStream outputStream = new MemoryStream())
				{
					report.Save(outputStream);
					using (ExcelInterface excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(outputStream);
						ExcelWorkSheet workSheet = excelInterface.WorkSheets[0];
						AssertMultilineASCIIEquals("Generated Results", "{B}-[3]", workSheet.ToString());
					}
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestDateTimeAsString()
		{
			const string ExpectedOutputForDateTimeAsString = @"
{B}-[07-Nov-1972]
{B}-[18-Sep-1971]
{B}-[10-Feb-1989]
";

			DummyBusinessObject topLevelDataSource = Factory.New<DummyBusinessObject>();
			topLevelDataSource.Z0_Date = new ZDateTime(1972, 11, 7);

			DummyChildBusinessObject child = topLevelDataSource.Collection.AddNew();
			child.Z0_Date = ZDateTime.BrettsBirthday;

			child = topLevelDataSource.Collection.AddNew();
			child.Z0_Date = new ZDateTime(1989, 2, 10);

			ExcelTemplateForUnitTesting excelTemplate = new ExcelTemplateForUnitTesting("DateTimeAsStringTestTemplate.xls", TestFilesSubFolder.DocumentTestFiles);
			using (Report report = GetNewReport(BODocDataProvider.Get(topLevelDataSource), excelTemplate))
			using (MemoryStream outputStream = new MemoryStream())
			{
				report.Save(outputStream);
				using (ExcelInterface excelInterface = new ExcelInterface())
				{
					excelInterface.LoadExcelFile(outputStream);
					ExcelWorkSheet workSheet = excelInterface.WorkSheets[0];
					AssertWorksheetEquals(workSheet, ExpectedOutputForDateTimeAsString);
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGroupIndex()
		{
			const string ExpectedOutputForTestGroupIndex = @"
{B}-[Header]
{B}-[1]
{B}-[1]
{B}-[2]
{B}-[2]
{B}-[3]
{B}-[3]
{B}-[4]
{B}-[4]
";

			DummyBusinessObject topLevelDataSource = Factory.New<DummyBusinessObject>();

			DummyChildBusinessObject child1 = topLevelDataSource.Collection.AddNew();
			child1.Z0_VarCharMax = "One";
			child1.Z0_Description = "LARGE";

			DummyChildBusinessObject child2 = topLevelDataSource.Collection.AddNew();
			child2.Z0_VarCharMax = "Two";
			child2.Z0_Description = "MEDIUM";

			DummyChildBusinessObject child3 = topLevelDataSource.Collection.AddNew();
			child3.Z0_VarCharMax = "Three";
			child3.Z0_Description = "SMALL";

			DummyChildBusinessObject child4 = topLevelDataSource.Collection.AddNew();
			child4.Z0_VarCharMax = "Four";
			child4.Z0_Description = "LARGE";
			ExcelTemplateForUnitTesting excelTemplate = new ExcelTemplateForUnitTesting("GroupIndexTestTemplate.xls", TestFilesSubFolder.DocumentTestFiles);
			using (Report report = GetNewReport(BODocDataProvider.Get(topLevelDataSource), excelTemplate))
			using (MemoryStream outputStream = new MemoryStream())
			{
				report.Save(outputStream);
				using (ExcelInterface excelInterface = new ExcelInterface())
				{
					excelInterface.LoadExcelFile(outputStream);
					ExcelWorkSheet workSheet = excelInterface.WorkSheets[0];
					AssertWorksheetEquals(workSheet, ExpectedOutputForTestGroupIndex);
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestDataSourceFilter()
		{
			const string ExpectedOutputForTestDataSourceFilter = @"
{B}-[Header]
{B}-[One]
{B}-[Four]
{B}-[Four]
{B}-[One]
{B}-[Two]
{B}-[Three]
{B}-[Four]
";

			DummyBusinessObject topLevelDataSource = Factory.New<DummyBusinessObject>();

			DummyChildBusinessObject child1 = topLevelDataSource.Collection.AddNew();
			child1.Z0_VarCharMax = "One";
			child1.Z0_Description = "LARGE";

			DummyChildBusinessObject child2 = topLevelDataSource.Collection.AddNew();
			child2.Z0_VarCharMax = "Two";
			child2.Z0_Description = "MEDIUM";

			DummyChildBusinessObject child3 = topLevelDataSource.Collection.AddNew();
			child3.Z0_VarCharMax = "Three";
			child3.Z0_Description = "SMALL";

			DummyChildBusinessObject child4 = topLevelDataSource.Collection.AddNew();
			child4.Z0_VarCharMax = "Four";
			child4.Z0_Description = "LARGE";

			ExcelTemplateForUnitTesting excelTemplate = new ExcelTemplateForUnitTesting("DataSourceFilterTestTemplate.xls", TestFilesSubFolder.DocumentTestFiles);
			using (Report report = GetNewReport(BODocDataProvider.Get(topLevelDataSource), excelTemplate))
			using (MemoryStream outputStream = new MemoryStream())
			{
				report.Save(outputStream);
				using (ExcelInterface excelInterface = new ExcelInterface())
				{
					excelInterface.LoadExcelFile(outputStream);
					ExcelWorkSheet workSheet = excelInterface.WorkSheets[0];
					AssertWorksheetEquals(workSheet, ExpectedOutputForTestDataSourceFilter);
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGroupCount()
		{
			const string ExpectedOutputForTestGroupCount = @"
{B}-[1]
{B}-[1]
{B}-[1]
{B}-[1]
{B}-[1]
{B}-[1]
{B}-[1]
{B}-[1]
{B}-[1]
{B}-[1]
{B}-[1]
";

			DummyBusinessObject topLevelDataSource = Factory.New<DummyBusinessObject>();

			int index = 0;
			while (index < 15)
			{
				DummyChildBusinessObject child1 = topLevelDataSource.Collection.AddNew();
				child1.Z0_VarCharMax = index < 7 ? "One" : "Two";
				index++;
			}

			ExcelTemplateForUnitTesting excelTemplate = new ExcelTemplateForUnitTesting("GroupCountTestTemplate.xls", TestFilesSubFolder.DocumentTestFiles);
			using (Report report = GetNewReport(BODocDataProvider.Get(topLevelDataSource), excelTemplate))
			using (MemoryStream outputStream = new MemoryStream())
			{
				report.Save(outputStream);
				using (ExcelInterface excelInterface = new ExcelInterface())
				{
					excelInterface.LoadExcelFile(outputStream);
					ExcelWorkSheet workSheet = excelInterface.WorkSheets[0];
					AssertWorksheetEquals(workSheet, ExpectedOutputForTestGroupCount);
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestDummyBusinessObjectAsDataSourceWithDummyCollectionSectionBody()
		{
			const string ExpectedOutputFromDummyBusinessObjectAsDataSourceWithDummyCollectionSectionBody = @"
{B}-[test]
{B}-[Brett made me do it.]
{B}-[Not that I'm Complaining.]
{B}-[Eat a fur pie.]
{B}-[That's not a nostril hair....]
{B}-[That would be dumb.]
";

			DummyBusinessObject topLevelDataSource = Factory.New<DummyBusinessObject>();
			topLevelDataSource.Collection.AddNew().Z0_VarCharMax = "Eat a fur pie.";
			topLevelDataSource.Collection.AddNew().Z0_VarCharMax = "That's not a nostril hair....";
			ExcelTemplateForUnitTesting excelTemplate = new ExcelTemplateForUnitTesting("DummyBusinessObjectAsDataSourceWithDummyCollectionSectionBody.xls", TestFilesSubFolder.DocumentTestFiles);
			using (Report report = GetNewReport(BODocDataProvider.Get(topLevelDataSource), excelTemplate))
			using (MemoryStream outputStream = new MemoryStream())
			{
				report.Save(outputStream);
				using (ExcelInterface excelInterface = new ExcelInterface())
				{
					excelInterface.LoadExcelFile(outputStream);
					ExcelWorkSheet workSheet = excelInterface.WorkSheets[0];
					AssertWorksheetEquals(workSheet, ExpectedOutputFromDummyBusinessObjectAsDataSourceWithDummyCollectionSectionBody);
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGraphicalOverlayStaysInFirstColumnAndMovesLeftWhenFirstColumnWidthIsSetToZero()
		{
			using (ExcelInterface excelInterface = new ExcelInterface())
			{
				string testFileName = BaseSourcePath + @"Enterprise\Product\Documents\DocumentEngine\Testing\DocumentTestFiles\GraphicalOverlayInFirstColumn.xls";
				excelInterface.LoadExcelFile(testFileName);
				ExcelWorkSheet workSheet = excelInterface.WorkSheets[0];
				excelInterface.Xls.ActiveSheetByName = workSheet.SheetName;

				AssertEquals(1, excelInterface.Xls.ImageCount);

				TClientAnchor anchor = excelInterface.Xls.GetImageProperties(1).Anchor;
				AssertEquals(1, anchor.Col1);
				AssertEquals(16, anchor.Col2);

				int anchorCol1 = anchor.Col1;
				int anchorCol2 = anchor.Col2;
				int anchorRow1 = anchor.Row1;
				int anchorRow2 = anchor.Row2;

				int anchorDx1 = anchor.Dx1;
				int anchorDx2 = anchor.Dx2;
				int anchorDy1 = anchor.Dy1;
				int anchorDy2 = anchor.Dy2;

				excelInterface.Xls.SetColHidden(1, true);
				excelInterface.Xls.SetColWidth(1, 0);

				anchor = excelInterface.Xls.GetImageProperties(1).Anchor;
				AssertEquals(anchorCol1, anchor.Col1);
				AssertEquals(anchorCol2, anchor.Col2);
				AssertEquals(anchorRow1, anchor.Row1);
				AssertEquals(anchorRow2, anchor.Row2);

				AssertEquals(anchorDx1, anchor.Dx1);
				AssertEquals(anchorDx2, anchor.Dx2);
				AssertEquals(anchorDy1, anchor.Dy1);
				AssertEquals(anchorDy2, anchor.Dy2);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2007, 1, 1)]
		public void TestDocEngineHandlesGroupsProperly()
		{
			GlbStaff.CurrentUser.GS_FullName = "Developer";
			var bo = Factory.New<DummyBusinessObject>();
			var topLevelDataSource = new PackingHeader(bo);

			var excelTemplate = new ExcelTemplateForUnitTesting("TestGroupBySections.xls", TestFilesSubFolder.DocumentTestFiles);
			using (Report report = GetNewReport(BODocDataProvider.Get(topLevelDataSource), excelTemplate))
			using (var outputStream = new MemoryStream())
			{
				report.Save(outputStream);
				using (var excelInterface = new ExcelInterface())
				{
					excelInterface.LoadExcelFile(outputStream);
					ExcelWorkSheet workSheet = excelInterface.WorkSheets[0];
					AssertWorksheetEquals(workSheet, ExpectedDocEngineHandlesGroupsProperly);
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGroupingGetsRightHeadersAndFootersEvenWhenTheGroupByFieldIsNotShownInAHeader()
		{
			const string ExpectedGroupingGetsRightHeadersAndFootersEvenWhenTheGroupByFieldIsNotShownInAHeader = @"
{B}-[MyCod]
{B}-[MyDescription]
{B}-[LC1]
{B}-[LineText1]
{B}-[LineText2]
{B}-[LineNText1]
{B}-[LC3]
{B}-[LineText3]
{B}-[LineNText3]

{B}-[PickALength]
";

			DummyEnterpriseBusinessObject dummyBO = GetDummyBOWithChildrenForTesting();

			ExcelTemplateForUnitTesting excelTemplate = new ExcelTemplateForUnitTesting("VisualisationTesting.xls", TestFilesSubFolder.DocumentTestFiles);
			using (Report report = GetNewReport(BODocDataProvider.Get(dummyBO), excelTemplate))
			using (MemoryStream outputStream = new MemoryStream())
			{
				report.Save(outputStream);
				using (ExcelInterface excelInterface = new ExcelInterface())
				{
					excelInterface.LoadExcelFile(outputStream);
					ExcelWorkSheet workSheet = excelInterface.WorkSheets[0];
					AssertWorksheetEquals(workSheet, ExpectedGroupingGetsRightHeadersAndFootersEvenWhenTheGroupByFieldIsNotShownInAHeader);
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestFormatterFunctionsWorkViaVisualiserOverride()
		{
			const string ExpectedFormatterFunctionsWorkViaVisualiserOverride = @"
{B}-[MyCod]
{B}-[MyDescription]
{B}-[LC1]
{B}-[Bretts]
{B}-[Farts]
{B}-[LineNText1]
{B}-[LC3]
{B}-[Reek]
{B}-[LineNText3]

{B}-[PickALength]
";
			DummyBusinessObject dummyBO = GetDummyBOWithChildrenForTesting();
			ExcelTemplateForUnitTesting excelTemplate = new ExcelTemplateForUnitTesting("VisualisationTesting.xls", TestFilesSubFolder.DocumentTestFiles);
			using (Report report = GetNewReport(BODocDataProvider.Get(dummyBO), excelTemplate))
			{
				Pack.Add(report);
				report.PrepareForRender();
				VisualiserDataSet vDataSet = report.OverridingDataSet;
				TemplateToVisualiserComponentsConverter templateToVCConverter = new TemplateToVisualiserComponentsConverter(report, vDataSet);
				List<VisualiserComponent> templateVisualComponents = templateToVCConverter.Components;

				DataTable dataTable = vDataSet.Tables[VisualiserDataSet.GetTableName("Collection")];
				dataTable.Rows[0][DummyBusinessObject.Schema.Z0_VarCharMax] = "Bretts";
				dataTable.Rows[1][DummyBusinessObject.Schema.Z0_VarCharMax] = "Farts";
				dataTable.Rows[2][DummyBusinessObject.Schema.Z0_VarCharMax] = "Reek";
				report.Factory.Save();
				Pack.SaveVisualizerContentNote();

				using (MemoryStream outputStream = new MemoryStream())
				{
					report.Save(outputStream);
					using (ExcelInterface excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(outputStream);
						ExcelWorkSheet workSheet = excelInterface.WorkSheets[0];
						AssertWorksheetEquals(workSheet, ExpectedFormatterFunctionsWorkViaVisualiserOverride);
					}
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestSaveVisualizerContentNoteWithoutOtherBOs()
		{
			var dummyBO = Factory.New<DummyEnterpriseBusinessObject>();
			dummyBO.Z0_Code = "OCode";
			ExcelTemplateForUnitTesting excelTemplate = new ExcelTemplateForUnitTesting("VisualisationTesting.xls", TestFilesSubFolder.DocumentTestFiles);
			using (Report report = GetNewReport(BODocDataProvider.Get(dummyBO), excelTemplate))
			{
				Pack.Add(report);
				report.Factory.Save();
				dummyBO.Z0_Code = "NCode";
				Pack.SaveVisualizerContentNote();
				var newdummyBO = new BusinessObjectFactory().Load<DummyBusinessObject>(dummyBO.PK);
				Assert("The other BOs should not be saved by SaveVisualizerContentNote", !newdummyBO.Z0_Code.Equals("NCode"));
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestFormatterFunctionsInvalidSurrogatePair()
		{
			const string ExpectedFormatterFunctionsInvalidSurrogatePair = @"
{B}-[MyCod]
{B}-[MyDescription]
{B}-[LC1]
{B}-[TestInfo]
{B}-[Farts]
{B}-[LineNText1]
{B}-[LC3]
{B}-[Reek]
{B}-[LineNText3]

{B}-[PickALength]
";
			DummyBusinessObject dummyBO = GetDummyBOWithChildrenForTesting();
			ExcelTemplateForUnitTesting excelTemplate = new ExcelTemplateForUnitTesting("VisualisationTesting.xls", TestFilesSubFolder.DocumentTestFiles);
			using (Report report = GetNewReport(BODocDataProvider.Get(dummyBO), excelTemplate))
			{
				Pack.Add(report);
				report.PrepareForRender();
				VisualiserDataSet vDataSet = report.OverridingDataSet;
				TemplateToVisualiserComponentsConverter templateToVCConverter = new TemplateToVisualiserComponentsConverter(report, vDataSet);
				List<VisualiserComponent> templateVisualComponents = templateToVCConverter.Components;

				DataTable dataTable = vDataSet.Tables[VisualiserDataSet.GetTableName("Collection")];

				Random random = new Random();
				char lowChar, highChar;
				char[] charArray = new char[10];
				for (int i = 0; i < 10; ++i)
				{
					lowChar = Convert.ToChar(random.Next(0xD800, 0xDC00));
					highChar = Convert.ToChar(random.Next(0xD800, 0xDC00));
					charArray[i] = highChar;
					charArray[++i] = lowChar;
				}
				string invalidUnicodeString = new string(charArray);

				dataTable.Rows[0][DummyBusinessObject.Schema.Z0_VarCharMax] = "Test" + invalidUnicodeString + "Info";
				dataTable.Rows[1][DummyBusinessObject.Schema.Z0_VarCharMax] = "Farts";
				dataTable.Rows[2][DummyBusinessObject.Schema.Z0_VarCharMax] = "Reek";
				report.Factory.Save();
				AssertNoExceptionThrown("Should not throw an exception", () => Pack.SaveVisualizerContentNote());
				using (MemoryStream outputStream = new MemoryStream())
				{
					report.Save(outputStream);
					using (ExcelInterface excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(outputStream);
						ExcelWorkSheet workSheet = excelInterface.WorkSheets[0];
						AssertWorksheetEquals(workSheet, ExpectedFormatterFunctionsInvalidSurrogatePair);
					}
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestDocEngineWorksDirectFromBusinessObject()
		{
			DummyBusinessObject mainBO = Factory.New<DummyBusinessObject>();
			mainBO.Z0_VarCharMax = "Friggin";
			DummyChildBusinessObject childBO1 = mainBO.Collection.AddNew();
			childBO1.Z0_VarCharMax = "Ferrets";
			DummyChildBusinessObject childBO2 = mainBO.Collection.AddNew();
			childBO2.Z0_VarCharMax = "Again!!";

			ExcelTemplateForUnitTesting excelTemplate = new ExcelTemplateForUnitTesting("DummyBusinessObjectAsDataSource.xls", TestFilesSubFolder.DocumentTestFiles);
			using (Report report = GetNewReport(BODocDataProvider.Get(mainBO), excelTemplate))
			{
				using (MemoryStream outputStream = new MemoryStream())
				{
					report.Save(outputStream);
					AssertEquals("report.DataContext", DataContext.BusinessObject, report.DataContextValue.DataContext);
					AssertEquals("report.DataContext", ".DummyBusinessObject", report.DataContextValue.BusinessObjectDataContext);
					using (ExcelInterface excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(outputStream);
						AssertEquals("xlInterface.WorkSheets[0][0, 2]", "Friggin", excelInterface.WorkSheets[0][0, 2]);
						AssertEquals("xlInterface.WorkSheets[0][1, 2]", "Ferrets", excelInterface.WorkSheets[0][1, 2]);
						AssertEquals("xlInterface.WorkSheets[0][2, 2]", "Again!!", excelInterface.WorkSheets[0][2, 2]);
					}
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestNestedLoop()
		{
			var level0 = Factory.New<DummyBusinessObjectLevel0>();

			#region Prepare Data

			level0.Z0_VarCharMax = "DocumentHeader";
			level0.Level0Only = "Level0Only";
			var childLevel1R1 = level0.CollectionLevel1.AddNew();
			childLevel1R1.Z0_VarCharMax = "SectionBody Level1 Record1";
			childLevel1R1.Z0_Decimal = 30m;
			childLevel1R1.Level1Only = "Level1OnlyR1";
			childLevel1R1.GroupBy = "GroupBy1";

			var childLevel2R1L1R1 = childLevel1R1.CollectionLevel2.AddNew();
			childLevel2R1L1R1.Z0_VarCharMax = "Foreach Level2 Record1 L1R1 Varc";
			childLevel2R1L1R1.Z0_NVarCharMax = "Foreach Level2 Record1 L1R1 NVarc";
			var childLevel3R1L2R1L1R1 = childLevel2R1L1R1.CollectionLevel3.AddNew();
			childLevel3R1L2R1L1R1.Z0_VarCharMax = "Foreach Level3 Record1 L2R1L1R1 Varc";
			childLevel3R1L2R1L1R1.Z0_NVarCharMax = "Foreach Level3 Record1 L2R1L1R1 NVarc";
			var childLevel3R2L2R1L1R1 = childLevel2R1L1R1.CollectionLevel3.AddNew();
			childLevel3R2L2R1L1R1.Z0_VarCharMax = "Foreach Level3 Record2 L2R1L1R1 Varc";
			childLevel3R2L2R1L1R1.Z0_NVarCharMax = "Foreach Level3 Record2 L2R1L1R1 NVarc";

			var childLevel2R2L1R1 = childLevel1R1.CollectionLevel2.AddNew();
			childLevel2R2L1R1.Z0_VarCharMax = "Foreach Level2 Record2 L1R1 Varc";
			childLevel2R2L1R1.Z0_NVarCharMax = "Foreach Level2 Record2 L1R1 NVarc";
			var childLevel3R1L2R2L1R1 = childLevel2R2L1R1.CollectionLevel3.AddNew();
			childLevel3R1L2R2L1R1.Z0_VarCharMax = "Foreach Level3 Record1 L2R2L1R1 Varc";
			childLevel3R1L2R2L1R1.Z0_NVarCharMax = "Foreach Level3 Record1 L2R2L1R1 NVarc";
			var childLevel3R2L2R2L1R1 = childLevel2R2L1R1.CollectionLevel3.AddNew();
			childLevel3R2L2R2L1R1.Z0_VarCharMax = "Foreach Level3 Record2 L2R2L1R1 Varc";
			childLevel3R2L2R2L1R1.Z0_NVarCharMax = "Foreach Level3 Record2 L2R2L1R1 NVarc";

			var childLevel1R2 = level0.CollectionLevel1.AddNew();
			childLevel1R2.Z0_VarCharMax = "SectionBody Level1 Record2";
			childLevel1R2.Z0_Decimal = 40m;
			childLevel1R2.Level1Only = "Level1OnlyR2";
			childLevel1R2.GroupBy = "GroupBy1";
			var childLevel2R1L1R2 = childLevel1R2.CollectionLevel2.AddNew();
			childLevel2R1L1R2.Z0_VarCharMax = "Foreach Level2 Record1 L1R2 Varc";
			childLevel2R1L1R2.Z0_NVarCharMax = "Foreach Level2 Record1 L1R2 NVarc";
			var childLevel3R1L2L1R2 = childLevel2R1L1R2.CollectionLevel3.AddNew();
			childLevel3R1L2L1R2.Z0_VarCharMax = "Foreach Level3 Record1 L2L1R2 Varc";
			childLevel3R1L2L1R2.Z0_NVarCharMax = "Foreach Level3 Record1 L2L1R2 NVarc";

			var childLevel1R3 = level0.CollectionLevel1.AddNew();
			childLevel1R3.Z0_VarCharMax = "SectionBody Level1 Record3";
			childLevel1R3.Z0_Decimal = 10m;
			childLevel1R3.Level1Only = "Level1OnlyR3";
			childLevel1R3.GroupBy = "GroupBy2";
			var childLevel2R1L1R3 = childLevel1R3.CollectionLevel2.AddNew();
			childLevel2R1L1R3.Z0_VarCharMax = "Foreach Level2 Record1 L1R3 Varc";
			childLevel2R1L1R3.Z0_NVarCharMax = "Foreach Level2 Record1 L1R3 NVarc";
			var childLevel3R1L2L1R3 = childLevel2R1L1R3.CollectionLevel3.AddNew();
			childLevel3R1L2L1R3.Z0_VarCharMax = "Foreach Level3 Record1 L2L1R3 Varc";
			childLevel3R1L2L1R3.Z0_NVarCharMax = "Foreach Level3 Record1 L2L1R3 NVarc";

			#endregion

			AssertNestedLoopOutput(level0, "DummyBusinessObjectAsDataSourceForNestedLoop.xls", "NestedLoopTestResult.xlsx");
		}

		public void TestNestedLoopWithNullSubCollection()
		{
			var level0 = Factory.New<DummyBusinessObjectLevel0>();
			var childLevel1 = level0.CollectionLevel1.AddNew();
			childLevel1.Z0_VarCharMax = "Level1";

			var childLevel2 = childLevel1.CollectionLevel2.AddNew();
			childLevel2.Z0_VarCharMax = "Level2";

			var childLevel3 = childLevel2.CollectionLevel3.AddNew();
			childLevel3.Z0_VarCharMax = "Level3";

			var templateString = @"{A}-[#config]
{A}-[Name=TestNestedLoopWithNullSubCollection]
{A}-[DataContext=.DummyBusinessObject]
{A}-[#SectionBody:Data=CollectionLevel1]
{B}-[<CollectionLevel1.Z0_VarCharMax>]
{A}-[#BeginLoop:Data=CollectionLevel2]
{B}-[<CollectionLevel2.Z0_VarCharMax>]
{A}-[#BeginLoop:Data=CollectionLevel3]
{B}-[<CollectionLevel3.Z0_VarCharMax>] {C}-[<CollectionLevel3.Parent.Z0_VarCharMax>]
{A}-[#EndLoop]
{A}-[#EndLoop]
{A}-[#EndOfReport]";
			var excelTemplate = DocumentEngineTestHelper.CreateExcelTemplateFromString("NestedLoopWithNullSubCollection", "", templateString);
			using (var report = GetNewReport(BODocDataProvider.Get(level0), excelTemplate))
			{
				using (var outputStream = new MemoryStream())
				{
					report.Save(outputStream);
					AssertEquals("report.DataContext", DataContext.BusinessObject, report.DataContextValue.DataContext);
					AssertEquals("report.DataContext", ".DummyBusinessObject", report.DataContextValue.BusinessObjectDataContext);
					using (var excelInterface = new ExcelInterface())
					{
						var expectedExcelString = @"{B}-[Level1]
{B}-[Level2]
{B}-[Level3]";
						excelInterface.LoadExcelFile(outputStream);
						AssertEquals("DocumentOutputResult", expectedExcelString, excelInterface.WorkSheets[0].ToString());
					}
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestPageBreakForNestedLoop()
		{
			var level0 = Factory.New<DummyBusinessObjectLevel0>();
			for (int i = 1; i <= 20; i++)
			{
				var level1 = level0.CollectionLevel1.AddNew();
				level1.Z0_Number = i;
				level1.Z0_VarCharMax = $"VarChar SB {i}";
				level1.Z0_NVarCharMax = $"NVarChar SB {i}";
				level1.Z0_Description = $"Description SB {i}";

				var level2Count = i % 3 + 1;
				for (int j = 1; j <= level2Count; j++)
				{
					var level2 = level1.CollectionLevel2.AddNew();
					level2.Z0_VarCharMax = $"L2R{j} VarChar L1R{i}";
					level2.Z0_NVarCharMax = $"L2R{j} NVarChar L1R{i}";
				}
			}

			AssertNestedLoopOutput(level0, "TestPageBreakForNestedLoop.xls", "PageBreakForNestedLoopTestResult.xlsx");
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestSectionBodyHeaderWithPageBreakPagination()
		{
			const string ExpectedOutputFromSectionBodyHeaderWithPageBreakPagination = @"{B}-[test]
{B}-[This is the First Section Header.]
{C}-[Child Row 1]
{C}-[Child Row 2]
{C}-[Child Row 3]
{C}-[Child Row 4]

{B}-[*** END OF PAGE ***]
{B}-[*** START *** Second Section Header with :PageBreak.]

{B}-[*** END *** Second Section Header with :PageBreak.]
{C}-[Child Row 1]

{B}-[*** END OF PAGE ***]
{B}-[This is the Second Section Page Header - Should See This as should have paginated.]
{C}-[Child Row 2]
{C}-[Child Row 3]
{C}-[Child Row 4]

{B}-[*** END OF PAGE ***]";

			DummyBusinessObject topLevelDataSource = Factory.New<DummyBusinessObject>();
			topLevelDataSource.Collection.AddNew().Z0_VarCharMax = "Child Row 1";
			topLevelDataSource.Collection.AddNew().Z0_VarCharMax = "Child Row 2";
			topLevelDataSource.Collection.AddNew().Z0_VarCharMax = "Child Row 3";
			topLevelDataSource.Collection.AddNew().Z0_VarCharMax = "Child Row 4";
			ExcelTemplateForUnitTesting excelTemplate = new ExcelTemplateForUnitTesting("SectionBodyHeaderWithPageBreakPaginationTest.xls", TestFilesSubFolder.DocumentTestFiles);
			using (Report report = GetNewReport(BODocDataProvider.Get(topLevelDataSource), excelTemplate))
			using (MemoryStream outputStream = new MemoryStream())
			{
				report.Save(outputStream);
				using (ExcelInterface excelInterface = new ExcelInterface())
				{
					excelInterface.LoadExcelFile(outputStream);
					ExcelWorkSheet workSheet = excelInterface.WorkSheets[0];
					AssertWorksheetEquals(workSheet, ExpectedOutputFromSectionBodyHeaderWithPageBreakPagination);
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestMultipleFormatsInOneMergedCell()
		{
			const string ExpectedOutputForMultipleFormatsInOneMergedCell = @"{B}-[CheeseBiscuits + CheeseBiscuits]
{B}-[CheeseBiscuits + CheeseBiscuits]
{B}-[<B>CheeseBiscuits + CheeseBiscuits<\B>]
{B}-[CheeseBiscuits + CheeseBiscuits + CheeseBiscuits + CheeseBiscuits + Straight Text]
{B}-[CheeseBiscuits + CheeseBiscuits]
{B}-[<B>CheeseBiscuits + CheeseBiscuits<\B>]
{B}-[<B>CheeseBiscuits + CheeseBiscuits + CheeseBiscuits + CheeseBiscuits<\B>]";

			DummyBusinessObject topLevelDataSource = Factory.New<DummyBusinessObject>();
			topLevelDataSource.Z0_VarCharMax = "CheeseBiscuits";

			ExcelTemplateForUnitTesting excelTemplate = new ExcelTemplateForUnitTesting("MultipleFormatsInOneMergedCell.xls", TestFilesSubFolder.DocumentTestFiles);
			using (Report report = GetNewReport(BODocDataProvider.Get(topLevelDataSource), excelTemplate))
			using (MemoryStream outputStream = new MemoryStream())
			{
				report.Save(outputStream);
				using (ExcelInterface excelInterface = new ExcelInterface())
				{
					excelInterface.LoadExcelFile(outputStream);
					ExcelWorkSheet workSheet = excelInterface.WorkSheets[0];
					string actualOutput = workSheet.ToString(new BoldResponsiveCellFormatter());
					AssertMultilineASCIIEquals("Contents are not the same", ExpectedOutputForMultipleFormatsInOneMergedCell, actualOutput);
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestBoldResponsiveCellFormatter()
		{
			const string ExpectedBoldResponsiveCellFormatterResult = @"{A}-[#config]
{A}-[Name=Dummy for Dummies]
{A}-[PageStyle=Portrait]
{A}-[DataContext=.DummyBusinessObject]

{A}-[#SectionBody]
{B}-[<Z0_VarCharMax> + <Z0_VarCharMax>]
{B}-[<Z0_VarCharMax> + <Z0_VarCharMax>]
{B}-[<B><Z0_VarCharMax> + <Z0_VarCharMax><\B>]
{B}-[<Z0_VarCharMax> + <Z0_VarCharMax> + <Z0_VarCharMax> + <Z0_VarCharMax> + Straight Text]
{B}-[<Z0_VarCharMax> + <Z0_VarCharMax>]
{B}-[<B><Z0_VarCharMax> + <Z0_VarCharMax><\B>]
{B}-[<B><Z0_VarCharMax> + <Z0_VarCharMax> + <Z0_VarCharMax> + <Z0_VarCharMax><\B>]
{A}-[#EndOfReport]";

			ExcelTemplateForUnitTesting excelTemplate = new ExcelTemplateForUnitTesting("MultipleFormatsInOneMergedCell.xls", TestFilesSubFolder.DocumentTestFiles);
			using (ExcelInterface excelInterface = new ExcelInterface())
			{
				excelInterface.LoadExcelFile(excelTemplate.FullTemplateSourceLocation);
				ExcelWorkSheet workSheet = excelInterface.WorkSheets[0];
				ZString result = workSheet.ToString(new BoldResponsiveCellFormatter());
				AssertMultilineASCIIEquals("BoldResponsiveCellFormatter didn't format the spreadsheet as expected.", ExpectedBoldResponsiveCellFormatterResult, result);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGroupByWithPageBreak_KeepInSamePage()
		{
			DummyBusinessObject topLevelDataSource = Factory.New<DummyBusinessObject>();
			AddDummyChildrenForTestGroupByWithPageBreak(topLevelDataSource, 1, 4);
			AddDummyChildrenForTestGroupByWithPageBreak(topLevelDataSource, 2, 4);
			AddDummyChildrenForTestGroupByWithPageBreak(topLevelDataSource, 3, 4);
			AddDummyChildrenForTestGroupByWithPageBreak(topLevelDataSource, 4, 4);
			AddDummyChildrenForTestGroupByWithPageBreak(topLevelDataSource, 5, 4);

			TestGroupByWithPageBreak(topLevelDataSource, "GroupByKeepInSamePageTest.xls", ExpectedResultForTestGroupByWithPageBreak_KeepInSamePage, false);
			TestGroupByWithPageBreak(topLevelDataSource, "GroupByKeepInSamePageTest.xls", ExpectedResultForTestGroupByWithPageBreak_KeepInSamePage, true);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGroupByWithPageBreak_MustSplitIfCannotBeKeptInSamePage()
		{
			DummyBusinessObject topLevelDataSource = Factory.New<DummyBusinessObject>();
			AddDummyChildrenForTestGroupByWithPageBreak(topLevelDataSource, 11, 12);
			AddDummyChildrenForTestGroupByWithPageBreak(topLevelDataSource, 12, 16);
			AddDummyChildrenForTestGroupByWithPageBreak(topLevelDataSource, 13, 5);
			AddDummyChildrenForTestGroupByWithPageBreak(topLevelDataSource, 14, 5);
			AddDummyChildrenForTestGroupByWithPageBreak(topLevelDataSource, 15, 12);
			AddDummyChildrenForTestGroupByWithPageBreak(topLevelDataSource, 16, 70);
			AddDummyChildrenForTestGroupByWithPageBreak(topLevelDataSource, 21, 2);
			AddDummyChildrenForTestGroupByWithPageBreak(topLevelDataSource, 22, 2);
			AddDummyChildrenForTestGroupByWithPageBreak(topLevelDataSource, 23, 2);
			AddDummyChildrenForTestGroupByWithPageBreak(topLevelDataSource, 24, 2);
			AddDummyChildrenForTestGroupByWithPageBreak(topLevelDataSource, 25, 2);
			AddDummyChildrenForTestGroupByWithPageBreak(topLevelDataSource, 26, 3);

			TestGroupByWithPageBreak(topLevelDataSource, "GroupByKeepInSamePageTest.xls", ExpectedResultForTestGroupByWithPageBreak_MustSplitIfCannotBeKeptInSamePage, false);
			TestGroupByWithPageBreak(topLevelDataSource, "GroupByKeepInSamePageTest.xls", ExpectedResultForTestGroupByWithPageBreak_MustSplitIfCannotBeKeptInSamePage, true);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGroupByWithPageBreak_NestedGroupByKeepInSamePage()
		{
			DummyBusinessObject dataSource = Factory.New<DummyBusinessObject>();
			AddDummyChildrenForTestGroupByWithPageBreak(dataSource, 11, "A1", 2);
			AddDummyChildrenForTestGroupByWithPageBreak(dataSource, 11, "B1", 1);
			AddDummyChildrenForTestGroupByWithPageBreak(dataSource, 12, "A2", 1);
			AddDummyChildrenForTestGroupByWithPageBreak(dataSource, 12, "B2", 8);
			AddDummyChildrenForTestGroupByWithPageBreak(dataSource, 13, "A3", 3);
			AddDummyChildrenForTestGroupByWithPageBreak(dataSource, 13, "B3", 5);

			TestGroupByWithPageBreak(dataSource, "NestedGroupByKeepInSamePageTest.xls", ExpectedResultForTestGroupByWithPageBreak_NestedGroupByKeepInSamePage, false);
			TestGroupByWithPageBreak(dataSource, "NestedGroupByKeepInSamePageTest.xls", ExpectedResultForTestGroupByWithPageBreak_NestedGroupByKeepInSamePage, true);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestHPageBreakRemove()
		{
			var topLevelDataSource = Factory.New<DummyBusinessObject>();
			topLevelDataSource.Collection.AddNew().Z0_VarCharMax = "1000001";
			topLevelDataSource.Collection.AddNew().Z0_VarCharMax = "1000002";
			topLevelDataSource.Collection.AddNew().Z0_VarCharMax = "1000003";
			topLevelDataSource.Collection.AddNew().Z0_VarCharMax = "1000004";
			topLevelDataSource.Collection.AddNew().Z0_VarCharMax = "1000005";
			topLevelDataSource.Collection.AddNew().Z0_VarCharMax = "1000006";
			topLevelDataSource.Collection.AddNew().Z0_VarCharMax = "1000007";
			var excelTemplate = new ExcelTemplateForUnitTesting("HPageBreakRemoveTest.xls", TestFilesSubFolder.DocumentTestFiles);
			using (var report = GetNewReport(BODocDataProvider.Get(topLevelDataSource), excelTemplate))
			using (var outputStream = new MemoryStream())
			{
				report.Save(outputStream);
				using (var excelInterface = new ExcelInterface())
				{
					excelInterface.LoadExcelFile(outputStream);
					var workSheet = excelInterface.WorkSheets[0];
					var code1 = workSheet[2, 3].ToString();
					var code2 = workSheet[34, 3].ToString();
					var code3 = workSheet[66, 3].ToString();
					var code4 = workSheet[98, 3].ToString();
					var code5 = workSheet[130, 3].ToString();
					var code6 = workSheet[162, 3].ToString();
					var code7 = workSheet[194, 3].ToString();

					AssertContains("1000001", code1);
					AssertContains("1000002", code2);
					AssertContains("1000003", code3);
					AssertContains("1000004", code4);
					AssertContains("1000005", code5);
					AssertContains("1000006", code6);
					AssertContains("1000007", code7);
				}
			}
		}

		void TestGroupByWithPageBreak(DummyBusinessObject topLevelDataSource, string templateNameForTest, string expectedOutput, bool groupByHasMultipleLines)
		{
			ExcelTemplateForUnitTesting excelTemplate = new ExcelTemplateForUnitTesting(templateNameForTest, TestFilesSubFolder.DocumentTestFiles);
			using (Report report = GetNewReport(BODocDataProvider.Get(topLevelDataSource), excelTemplate))
			using (MemoryStream outputStream = new MemoryStream())
			{
				if (groupByHasMultipleLines)
				{
					report.WorkSheetCurrentlyBeingProcessed.InsertRows(20, 1);
					report.WorkSheetCurrentlyBeingProcessed[20, 4] = "<HideRowIf(1==1)>";
				}

				report.Save(outputStream);
				using (ExcelInterface excelInterface = new ExcelInterface())
				{
					excelInterface.LoadExcelFile(outputStream);
					ExcelWorkSheet workSheet = excelInterface.WorkSheets[0];
					string actualOutput = workSheet.ToString();
					AssertMultilineASCIIEquals("Contents are not the same", expectedOutput, actualOutput);
				}
			}
		}

		public void TestTooLargePageFooterNoException()
		{
			var pageString = new List<string>();
			var pageHeader = new List<string>();
			for (int i = 0; i < 50; i++)
			{
				pageHeader.Add("Test");
			}
			for (int i = 0; i < 300; i++)
			{
				pageString.Add("Test");
			}

			var template = @"{A}-[#Config]
{A}-[Name=TestIsDraft]
{A}-[#DocumentHeader]"
+ string.Join("\r\n", pageHeader) +
@"{A}-[#PageHeader:StartFromSecondPage]"
+ string.Join("\r\n", pageHeader) +
@"{A}-[#SectionBody]
{B}-[Test Document Section Body]
{A}-[#SectionHeader]
{B}-[Test Document Section Header]
{A}-[#SectionBody]
{B}-[Test Document Section Body]
{A}-[#FirstPageFooter]
{B}-[First Document Footer]
{A}-[#PageFooter]
{B}-[Test Document Footer]
{A}-['-#LastPageFooter]"
+ string.Join("\r\n", pageString) +
@"{A}-[#OnlyOnePageFooter]
{B}-[Only One Page Footer]"
+ string.Join("\r\n", pageString) +
@"{A}-[#LastPageFooter]
{B}-[Last Page Footer]
{B}-[Test Last Page Footer]
{B}-[Test Last Page Footer]
{B}-[Test Last Page Footer]
{B}-[Test Last Page Footer]
{A}-[#EndOfReport]";
			var dummy = Factory.New<DummyBusinessObject>();

			using (Stream templateStream = new MemoryStream())
			using (var documentPack = new DocumentPack())
			using (var report = DocumentEngineTestHelper.CreateReportFromExcelTemplateContents(documentPack, BODocDataProvider.Get(dummy), templateStream, template))
			using (var printTask = new PrintTask())
			{
				printTask.Add(documentPack);
				documentPack.Add(report);

				var deliveryInstructions = new DeliveryInstructions(documentPack);
				deliveryInstructions.IsDraft = true;
				deliveryInstructions.Destination = DeliveryInstructionDestination.TakenFromContact;
				deliveryInstructions.Recipients.RemoveAndDeleteAll();
				var recipient = deliveryInstructions.Recipients.AddNew();
				recipient.DeliveryMethod = Enterprise.Core.Constants.ContactNotifyModes.Print;
				recipient.AttachmentType = "XLSX";

				AssertNoExceptionThrown(() => printTask.Run(deliveryInstructions));
			}
		}

		public void TestAutoHeightMacroInPageHeaderUnBrokeableWillShowWarningMessageAndNoErrorReport()
		{
			ErrorReporter.Clear();

			var dummy = Factory.New<DummyBusinessObject>();
			for (var i = 0; i < 35; i++)
			{
				dummy.Z0_VarCharMax += "line\r\n";
			}

			var template = @"{A}-[#Config]
{A}-[Name=TestIsDraft]
{A}-[#DocumentHeader]
{B}-[Test Document Header]
{A}-[#PageHeader]
{B}-[<AutoHeight><Z0_VarCharMax>]
{A}-[#PageFooter]
{B}-[<AutoHeight><Z0_VarCharMax>];
{B}-[End of Test]
{A}-[#EndOfReport]";

			using (Stream templateStream = new MemoryStream())
			using (var documentPack = new DocumentPack())
			using (var report = DocumentEngineTestHelper.CreateReportFromExcelTemplateContents(documentPack, BODocDataProvider.Get(dummy), templateStream, template))
			using (var printTask = new PrintTask())
			{
				printTask.Add(documentPack);
				documentPack.Add(report);

				var deliveryInstructions = new DeliveryInstructions(documentPack);
				deliveryInstructions.IsDraft = true;
				deliveryInstructions.Destination = DeliveryInstructionDestination.TakenFromContact;
				deliveryInstructions.Recipients.RemoveAndDeleteAll();
				var recipient = deliveryInstructions.Recipients.AddNew();
				recipient.DeliveryMethod = Enterprise.Core.Constants.ContactNotifyModes.Print;
				recipient.AttachmentType = "XLSX";

				AssertExceptionThrown<DocumentEngineException>(() => printTask.Run(deliveryInstructions));
				AssertEquals("Rpt.Errors", "Severity: [Fatal Error (without error report)] Message: [The report could not be generated due to some area does not " +
					"fit on the page and is too large to print/preview. Error details : \r\nArea Enterprise.DocumentEngine.Areas.PageHeaderArea does not fit in a page " +
					"and is unbreakable!] Cell: [N/A]",
					report.ErrorManager.ToString("Severity: [{0}] Message: [{1}] Cell: [{2}]", false));
				AssertEquals(0, ErrorReporter.TotalErrorCount);
			}
		}

		public void TestAutoHeightMacroWhenHandleSpecificCharacter()
		{
			var factory = new BusinessObjectFactory();
			var dummyBizo = factory.New<DummyBusinessObject>();
			// This string ends with an exceptional character combination: U+034F, U+200C, U+034F. These characters are invisible.
			dummyBizo.Z0_Description = "airport 1no. Crate͏‌͏";
			var command = factory.NewWithValidTestData<ReportCommand>();

			using (var templateStream = new MemoryStream())
			using (var docPack = new DocumentPack(command))
			{
				using (var creationExcelInterface = new ExcelInterface())
				{
					creationExcelInterface.NewExcelFile(1);
					var workSheet = creationExcelInterface.WorkSheets[0];
					workSheet[0, 0] = "#config";
					workSheet[1, 0] = "Name=TemplateFromStream";
					workSheet[2, 0] = "PageStyle=Portrait";
					workSheet[3, 0] = "#SectionBody";
					workSheet[4, 1] = "<AutoHeight><Z0_Description>";
					workSheet[4, 2] = "<Z0_Description>";
					workSheet[4, 3] = dummyBizo.Z0_Description;
					workSheet[5, 0] = "#EndOfReport";
					creationExcelInterface.SaveToStream(templateStream);
				}

				var excelTemplate = new ExcelTemplateWrappingStream("TemplateFromStream", templateStream);
				using (var report = GetNewReport(BODocDataProvider.Get(dummyBizo), excelTemplate))
				using (var outputStream = new MemoryStream())
				{
					using (var excelInterface = new ExcelInterface())
					{
						((IReportForUnitTesting)report).GenerateRegardlessOfAnyErrors = true;
						report.Save(outputStream);

						excelInterface.LoadExcelFile(outputStream);
						var workSheet = excelInterface.WorkSheets[0];

						AssertMultilineASCIIEquals("Generated Results",
							@"{B}-[airport 1no.]   {C}-[airport 1no. Crate]   {D}-[airport 1no. Crate]
{B}-[Crate]",
							workSheet.ToString(new CellFormatterExposingFormulae()));
					}
				}
			}
		}

		public void TestDecimalMacro_NumberDecimalSeparatorIsFromCurrentCompanyCountryCulture()
		{
			var originalSeparator = DataRegistry.Instance.NumberDecimalSeparator;
			DataRegistry.Instance.NumberDecimalSeparator = ",,";

			try
			{
				var dummyBizo = Factory.New<DummyForTestDecimalMacro>();
				dummyBizo.Z0_AnotherDecimal = 123.45m;
				Factory.Save();

				using (var templateStream = new MemoryStream())
				{
					using (var creationExcelInterface = new ExcelInterface())
					{
						creationExcelInterface.NewExcelFile(1);
						var workSheet = creationExcelInterface.WorkSheets[0];
						workSheet[0, 0] = "#config";
						workSheet[1, 0] = "Name=TemplateFromStream";
						workSheet[2, 0] = "PageStyle=Portrait";
						workSheet[3, 0] = "#SectionBody";
						workSheet[4, 1] = "<Z0_AnotherDecimal>";
						workSheet[4, 2] = "<Z0_DecimalCore>";
						workSheet[5, 0] = "#EndOfReport";
						creationExcelInterface.SaveToStream(templateStream);
					}

					Pack.StmMenuCommand.SU_IsLocalDocument = true;
					var excelTemplate = new ExcelTemplateWrappingStream("TemplateFromStream", templateStream);
					using (var report = GetNewReport(BODocDataProvider.Get(dummyBizo), excelTemplate))
					using (var outputStream = new MemoryStream())
					{
						report.Save(outputStream);
						using (var excelInterface = new ExcelInterface())
						using (Culture.SetTemporarily(Culture.CurrentCompanyCountryCulture))
						{
							excelInterface.LoadExcelFile(outputStream);
							var workSheet = excelInterface.WorkSheets[0];
							AssertMultilineASCIIEquals("Generated Results", @"
{B}-[123,,45]   {C}-[123,,45]
".Trim(), workSheet.ToString(new CellFormatterExposingFormulae()));
						}
					}
				}
			}
			finally
			{
				DataRegistry.Instance.NumberDecimalSeparator = originalSeparator;
			}
		}

		class DummyForTestDecimalMacro : DummyBusinessObject
		{
			public DummyForTestDecimalMacro(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public ZString Z0_DecimalCore
			{
				get { return Z0_AnotherDecimal.ToString(); }
			}
		}

		void AddDummyChildrenForTestGroupByWithPageBreak(DummyBusinessObject dummy, int z0_Number, int childCount)
		{
			for (int i = 0; i < childCount; i++)
			{
				DummyChildBusinessObject dummyChild = dummy.Collection.AddNew();
				dummyChild.Z0_Number = z0_Number;
				dummyChild.Z0_NVarChar = string.Format("Element {0}-{1}", z0_Number, i + 1);
			}
		}

		void AddDummyChildrenForTestGroupByWithPageBreak(DummyBusinessObject dummy, int z0_Number, ZString z0_NVarCharMax, int childCount)
		{
			for (int i = 0; i < childCount; i++)
			{
				DummyChildBusinessObject dummyChild = dummy.Collection.AddNew();
				dummyChild.Z0_Number = z0_Number;
				dummyChild.Z0_NVarCharMax = z0_NVarCharMax;
				dummyChild.Z0_NVarChar = string.Format("Element {0}-{1}-{2}", z0_Number, z0_NVarCharMax, i + 1);
			}
		}

		StmTemplateBase CreateTemplate(string templateName, TestFilesSubFolder subFolderName)
		{
			var excelTemplate = new ExcelTemplateForUnitTesting(templateName, subFolderName);
			var result = Factory.New<StmTemplateBase>();
			result.SO_Template = excelTemplate.GetAsByteArray();
			result.SO_DataContext = nameof(DataContext.UnitTest);

			return result;
		}

		void AssertWorksheetEquals(ExcelWorkSheet workSheet, string expectedValue)
		{
			AssertMultilineASCIIEquals("workSheet: " + workSheet.SheetName, expectedValue.Trim(), workSheet.ToString());
		}

		DummyEnterpriseBusinessObject GetDummyBOWithChildrenForTesting()
		{
			DummyEnterpriseBusinessObject dummyBO = Factory.New<DummyEnterpriseBusinessObject>();
			dummyBO.Z0_Code = "MyCod";
			dummyBO.Z0_Description = "MyDescription";
			dummyBO.Z0_NVarChar = "PickALength";

			DummyChildBusinessObject dummyChildBO1 = dummyBO.Collection.AddNew();
			dummyChildBO1.Z0_Number = 10;
			dummyChildBO1.Z0_VarCharMax = "LineText1";
			dummyChildBO1.Z0_FK_Code = "LC1";
			dummyChildBO1.Z0_NVarCharMax = "LineNText1";

			DummyChildBusinessObject dummyChildBO2 = dummyBO.Collection.AddNew();
			dummyChildBO2.Z0_Number = 10;
			dummyChildBO2.Z0_VarCharMax = "LineText2";
			dummyChildBO2.Z0_FK_Code = "LC2";
			dummyChildBO2.Z0_NVarCharMax = "LineNText2";

			DummyChildBusinessObject dummyChildBO3 = dummyBO.Collection.AddNew();
			dummyChildBO3.Z0_Number = 20;
			dummyChildBO3.Z0_VarCharMax = "LineText3";
			dummyChildBO3.Z0_FK_Code = "LC3";
			dummyChildBO3.Z0_NVarCharMax = "LineNText3";
			return dummyBO;
		}

		static void Load2PageOptionalTemplateInto(MemoryStream templateStream)
		{
			using (ExcelInterface creationExcelInterface = new ExcelInterface())
			{
				creationExcelInterface.NewExcelFile(2);

				ExcelWorkSheet workSheet1 = creationExcelInterface.WorkSheets[0];
				workSheet1[0, 0] = "#config";
				workSheet1[1, 0] = "Name=TemplateFromStream";
				workSheet1[2, 0] = "PageStyle=Portrait";
				workSheet1[3, 0] = "#SectionBody";
				workSheet1[4, 1] = "First Worksheet";
				workSheet1[5, 0] = "#EndOfReport";

				ExcelWorkSheet workSheet2 = creationExcelInterface.WorkSheets[1];
				workSheet2[0, 0] = "#config";
				workSheet2[1, 0] = "HideSheetIf=\"<Z0_VarCharMax>\"==\"Hide\"";
				workSheet2[2, 0] = "#SectionBody";
				workSheet2[3, 1] = "Second Worksheet";
				workSheet2[4, 0] = "#EndOfReport";

				creationExcelInterface.SaveToStream(templateStream);
			}
		}

		void AssertOverlengthCellDataGetsReportedAndTruncated(bool isCustomisedReport)
		{
			var bo = Factory.New<DummyBusinessObject>();
			bo.Z0_VarCharMax = "Long Long String with 33,000 characters in it:-".PadRight(33000, 'X');
			Assert("Precondition: Z0_VarCharMax.Length > CellContentReplacer.MaximumStringLengthForOneCellInExcel", bo.Z0_VarCharMax.Length > FlxConsts.Max_StringLenInCell);

			using (var templateStream = new MemoryStream())
			{
				using (var creationExcelInterface = new ExcelInterface())
				{
					creationExcelInterface.NewExcelFile(1);
					ExcelWorkSheet workSheet = creationExcelInterface.WorkSheets[0];
					workSheet[0, 0] = "#config";
					workSheet[1, 0] = "Name=TemplateFromStream";
					workSheet[2, 0] = "PageStyle=Portrait";
					workSheet[3, 0] = "#SectionBody";
					workSheet[4, 1] = "<Z0_VarCharMax>";
					workSheet[5, 0] = "#EndOfReport";
					workSheet.SheetNameOverride = "Felix the Cat";
					workSheet.UpdateSheetName();
					creationExcelInterface.SaveToStream(templateStream);
				}

				var excelTemplate = new ExcelTemplateWrappingStream("TemplateFromStream", templateStream);
				using (Report report = GetNewReport(BODocDataProvider.Get(bo), excelTemplate))
				using (var outputStream = new MemoryStream())
				{
					((IReportForUnitTesting)report).GenerateRegardlessOfAnyErrors = true;
					SetCustomizationFlags(report, isCustomisedReport);
					report.Save(outputStream);

					using (var excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(outputStream);
						ExcelWorkSheet workSheet = excelInterface.WorkSheets[0];
						string cellValue = workSheet[0, 1] as string;
						AssertNotNull("workSheet[0, 1] as string", cellValue);
						AssertEquals("cellValue.Length should be truncated to CellContentReplacer.MaximumStringLengthForOneCellInExcel", FlxConsts.Max_StringLenInCell, cellValue.Length);
					}

					AssertEquals("Report.Errors", @"Severity: [Warning (without error report)] Message: [Macro <Z0_VarCharMax> returned 33000 characters which is more than the maximum allowable by Excel (" + FlxConsts.Max_StringLenInCell + "). Cell Content has been truncated to 32767 characters.] Cell: [B5] Sheetname: [Felix the Cat]",
											report.ErrorManager.ToString("Severity: [{0}] Message: [{1}] Cell: [{2}] Sheetname: [{3}]", false));
				}
			}
		}

		void SetCustomizationFlags(Report report, bool isCustomisedReport)
		{
			if (report.Template != null)
			{
				report.Template.ContainsCustomisedSections = isCustomisedReport;
			}

			if (report.MenuItem != null)
			{
				report.MenuItem.SU_IsSystemDefined = !isCustomisedReport;
			}

			if (report.StTemplate != null)
			{
				report.StTemplate.SO_IsSystemDefined = !isCustomisedReport;
			}

			if (report.Pivot != null)
			{
				report.Pivot.SI_IsSystemDefined = !isCustomisedReport;
				report.Pivot.Template.SO_IsSystemDefined = !isCustomisedReport;
			}
		}

		void AssertNestedLoopOutput(DummyBusinessObjectLevel0 topLevelDataSource, string templateExcel, string outputExcel)
		{
			var excelTemplate = new ExcelTemplateForUnitTesting(templateExcel, TestFilesSubFolder.DocumentTestFiles);
			using (var report = GetNewReport(BODocDataProvider.Get(topLevelDataSource), excelTemplate))
			{
				using (var outputStream = new MemoryStream())
				{
					report.Save(outputStream);
					AssertEquals("report.DataContext", DataContext.BusinessObject, report.DataContextValue.DataContext);
					AssertEquals("report.DataContext", ".DummyBusinessObject", report.DataContextValue.BusinessObjectDataContext);
					using (var excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(outputStream);

						var expectedTemplate = new ExcelTemplateForUnitTesting(outputExcel, TestFilesSubFolder.ExpectedDocuments);
						using (var expectExcelInterface = new ExcelInterface())
						{
							expectExcelInterface.LoadExcelFile(expectedTemplate.FullTemplateSourceLocation);
							AssertEquals(expectExcelInterface.WorkSheets[0].ToString(), excelInterface.WorkSheets[0].ToString());
						}
					}
				}
			}
		}

		Report GetNewReport(IBODocDataProvider topLevelDataSource, ExcelTemplate excelTemplate)
		{
			return new Report(Pack, excelTemplate, topLevelDataSource, "Test_" + excelTemplate.TemplateName, null, DocumentDirection.ANY, false);
		}

		sealed class PackingHeader : NonPersistentBusinessObject, IVisualizerNoteSupporter
		{
			internal PackingHeader(DummyBusinessObject parent)
			{
				Parent = parent;
			}

			internal readonly DummyBusinessObject Parent;

			public GlbBranch CurrentBranch
			{
				get { return GlbBranch.CurrentBranch; }
			}

			#region Header Fields
			public ZString MyReportName
			{
				get { return "Delivery Docket"; }
			}

			public ZString ExternalReference
			{
				get { return "46038 - 31021"; }
			}

			public ZString CustomerReference
			{
				get { return "032033"; }
			}

			public ZString DocketID
			{
				get { return "W00010223"; }
			}

			public ZDateTime RequiredDate
			{
				get { return new ZDateTime(2002, 2, 15); }
			}

			public ZString ClientName
			{
				get { return "DANAFLEX PACKAGING"; }
			}

			public ZString WarehouseName
			{
				get { return "EXPRESS LOGISTICS BRISBANE"; }
			}

			public ZString ConsigneeAddress
			{
				get
				{
					return
	@"TEYS BROS ROCKHAMPTON
LAKES CREEK ROAD
NORTH ROCKHAMPTON QLD 4700
AUSTRALIA";
				}
			}

			public ZString GoodsBillToAddress
			{
				get { return ""; }
			}

			public ZString SpecialInstructions
			{
				get { return ""; }
			}

			public ZString References
			{
				get { return ""; }
			}

			public ZString DGContact
			{
				get { return ""; }
			}

			public ZString EmergencyNumber
			{
				get { return ""; }
			}

			public ZString PrintDGDetails
			{
				get { return ""; }
			}
			#endregion

			#region Footer
			public ZString UnitsSent
			{
				get { return ""; }
			}

			public ZString PackagesSent
			{
				get { return ""; }
			}

			public ZString PalletsSent
			{
				get { return ""; }
			}

			public ZString TotalWeight
			{
				get { return ""; }
			}

			public ZString TotalWeightUnit
			{
				get { return ""; }
			}

			public ZString TotalCubic
			{
				get { return ""; }
			}

			public ZString TotalCubicUnit
			{
				get { return ""; }
			}

			public ZString TransportCo
			{
				get { return ""; }
			}

			public ZString ServiceLevel
			{
				get { return ""; }
			}
			#endregion

			#region Line Descriptions from Client
			public ZString IMPartAttrib1Name
			{
				get { return ""; }
			}

			public ZString IMPartAttrib2Name
			{
				get { return ""; }
			}

			public ZString IMPartAttrib3Name
			{
				get { return ""; }
			}

			public ZString CustomAttrib1
			{
				get { return "Pallet Number"; }
			}

			public ZString CustomAttrib2
			{
				get { return ""; }
			}

			public ZString CustomAttrib3
			{
				get { return ""; }
			}
			#endregion

			#region Packing Lines
			public PackingLineCollection PackingLines
			{
				get
				{
					if (fPackingLines == null)
					{
						fPackingLines = GetNewPackingLinesCollection();
					}
					return fPackingLines;
				}
			}
			PackingLineCollection fPackingLines;

			PackingLineCollection GetNewPackingLinesCollection()
			{
				PackingLineCollection result = new PackingLineCollection();
				result.Add(new PackingLine("X10511", "SK23 TEYS LOGO 300X860MM POR", 9900, 9900, "BAG", "26421"));
				result.Add(new PackingLine("X10512", "SK23 AI007 LEGEND 225X860MM POR", 2750, 2750, "UNT", "26422"));
				result.Add(new PackingLine("X10513", "SK23 TEYS LOGO 400X600MM POR", 10800, 10800, "UNT", "26423"));
				result.Add(new PackingLine("X10514", "SK23 TEYS LOGO 400X600MM POR", 10800, 10800, "UNT", "26424"));
				result.Add(new PackingLine("X10515", "SK23 TEYS LOGO 400X600MM POR", 10800, 10800, "UNT", "26425"));
				return result;
			}
			#endregion

			#region IVisualizerNoteSupporter members

			ZGuid IVisualizerNoteSupporter.PK => Parent.PK;

			ZGuid IVisualizerNoteSupporter.ChildBusinessObjectPK => ZGuid.Empty;

			string IVisualizerNoteSupporter.TableCode => DummyBizoSchema.Constants.Prefix;

			#endregion
		}

		sealed class PackingLine : NonPersistentBusinessObject
		{
			public PackingLine(ZString productCode, ZString productDescription, ZInt qtyOrdered, ZInt qtySent, ZString unitOfQuantity, ZString palletNumber)
				: base()
			{
				this.pk = ZGuid.NewZGuid();
				this.productCode = productCode;
				this.productDescription = productDescription;
				this.qtyOrdered = qtyOrdered;
				this.qtySent = qtySent;
				this.unitOfQuantity = unitOfQuantity;
				this.palletNumber = palletNumber;
			}
			readonly ZGuid pk;
			readonly ZString productCode;
			readonly ZString productDescription;
			readonly ZInt qtyOrdered;
			readonly ZInt qtySent;
			readonly ZString unitOfQuantity;
			readonly ZString palletNumber;

			#region Group Fields
			public ZGuid LinePK
			{
				get { return this.pk; }
			}

			public ZString ProductCode
			{
				get { return productCode; }
			}

			public ZString ProductDescription
			{
				get { return productDescription; }
			}

			public ZInt LineUnitsOrdered
			{
				get { return qtyOrdered; }
			}

			public ZInt LineUnitsMet
			{
				get { return qtySent; }
			}

			public ZString UnitsUQ
			{
				get { return unitOfQuantity; }
			}
			#endregion

			#region Body Fields
			public ZString PartAttrib1
			{
				get { return ""; }
			}

			public ZString AttributeUnits
			{
				get { return ""; }
			}

			public ZString PartAttrib2
			{
				get { return ""; }
			}

			public ZString PartAttrib3
			{
				get { return ""; }
			}

			public ZString CustomAttrib1
			{
				get { return palletNumber; }
			}

			public ZString CustomAttrib2
			{
				get { return ""; }
			}

			public ZString CustomAttrib3
			{
				get { return ""; }
			}

			public ZDateTime ExpiryDate
			{
				get { return ZDateTime.Empty; }
			}

			public ZDateTime PackingDate
			{
				get { return ZDateTime.Empty; }
			}

			public ZString HazMatString
			{
				get { return ""; }
			}
			#endregion
		}

		sealed class PackingLineCollection : NonPersistentBusinessObjectCollection<PackingLine>
		{
			protected override BusinessObject CreateNonPersistentBusinessObject()
			{
				throw new InvalidOperationException();
			}

			protected override bool AllowNewCore
			{
				get { return false; }
			}
		}

		sealed class Fred : DocumentWrapper
		{
			public ZString Name => "Fred";

			public ZString Job => "Eating Hot Dogs";
		}

		sealed class Bill : DocumentWrapper
		{
			public ZString Name => "Bill";

			public ZString UnemploymentBenefits => "Undeserved";
		}

		sealed class DummyBusinessObjectWithResCurrentLanguage : DummyBaseBusinessObject
		{
			public DummyBusinessObjectWithResCurrentLanguage(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{ }

			public ZString CurrentLanguage => Res.CurrentLanguage;

			public ZString ResDotGetStringProperty => Res._GetString(ResourceStringAssemblyIdAttribute.IgnoreResourceStringsAssemblyId, "StringForTestingKey", "Must do better");

			public ZString MultiLanguageRegistryItemCastOutToZString
			{
				get
				{
					switch (CurrentLanguage)
					{
						case Core.SharedConstants.Languages.German:
							return "Güten Tag";
						default:
							return "Hello";
					}
				}
			}
		}

		sealed class DummyBusinessObjectWithMultilingualString : DummyBaseBusinessObject
		{
			public DummyBusinessObjectWithMultilingualString(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{ }

			public MultilingualString TheMultilingualString
			{
				get { return ResString.GetMultilingualString("hello", "Hello"); }
			}
		}

		sealed class DummyWithCachedWrapperCollection : DummyBaseBusinessObject
		{
			public DummyWithCachedWrapperCollection(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{ }

			public DummyDocumentWrapperWithResolvedResourceStringCollection MyLines
			{
				get
				{
					if (myLines == null)
					{
						myLines = new DummyDocumentWrapperWithResolvedResourceStringCollection(Factory);
						myLines.Add(new DummyDocumentWrapperWithResolvedResourceString(Res.GetString("ResourceStringResolvedInWrapperConstructor", "Hello Wrapper Constructor")));
					}
					return myLines;
				}
			}

			DummyDocumentWrapperWithResolvedResourceStringCollection myLines;
		}

		sealed class DummyDocumentWrapperWithResolvedResourceString : DocumentWrapper
		{
			public DummyDocumentWrapperWithResolvedResourceString(ZString resolvedResourceString)
			{
				this.resolvedResourceString = resolvedResourceString;
			}

			public ZString ResolvedResourceString { get { return resolvedResourceString; } }

			readonly ZString resolvedResourceString;
		}

		sealed class DummyDocumentWrapperWithResolvedResourceStringCollection : DocumentWrapperCollection<DummyDocumentWrapperWithResolvedResourceString>
		{
			public DummyDocumentWrapperWithResolvedResourceStringCollection(BusinessObjectFactory factory)
				: base(factory)
			{ }

			public ZBool IsTrue { get { return ZBool.True; } }
		}

		const string ExpectedDocEngineHandlesGroupsProperly = @"
{C}-[Delivery Docket - Order No 46038 - 31021]
{AI}-[CUSTOMER REF:]   {AQ}-[032033]
{AI}-[DOCKET ID:]   {AQ}-[W00010223]
{AI}-[REQUIRED DATE:]   {AQ}-[37302]
{AI}-[PRINTED DATE:]   {AQ}-[39083]
{AI}-[PAGE:]   {AQ}-[1 of 1]

{C}-[ORDER DETAILS]

{C}-[CLIENT]   {AA}-[WAREHOUSE]
{C}-[DANAFLEX PACKAGING]   {AA}-[EXPRESS LOGISTICS BRISBANE]

{C}-[CONSIGNEE]   {AA}-[GOODS BILLED TO]
{C}-[TEYS BROS ROCKHAMPTON|>LAKES CREEK ROAD|>NORTH ROCKHAMPTON QLD 4700|>AUSTRALIA]

{C}-[PRODUCT]   {O}-[DESCRIPTION]   {AO}-[QTY|>ORDERED]   {AS}-[QTY|>SENT]   {AV}-[UQ]
{C}-[X10511]   {O}-[SK23 TEYS LOGO 300X860MM POR]   {AO}-[9900]   {AS}-[9900]   {AW}-[BAG]
{G}-[Pallet Number:]   {O}-[26421]

{C}-[X10512]   {O}-[SK23 AI007 LEGEND 225X860MM POR]   {AO}-[2750]   {AS}-[2750]   {AW}-[UNT]
{G}-[Pallet Number:]   {O}-[26422]

{C}-[X10513]   {O}-[SK23 TEYS LOGO 400X600MM POR]   {AO}-[10800]   {AS}-[10800]   {AW}-[UNT]
{G}-[Pallet Number:]   {O}-[26423]

{C}-[X10514]   {O}-[SK23 TEYS LOGO 400X600MM POR]   {AO}-[10800]   {AS}-[10800]   {AW}-[UNT]
{G}-[Pallet Number:]   {O}-[26424]

{C}-[X10515]   {O}-[SK23 TEYS LOGO 400X600MM POR]   {AO}-[10800]   {AS}-[10800]   {AW}-[UNT]
{G}-[Pallet Number:]   {O}-[26425]

{AK}-[TOTAL]   {AO}-[45050]   {AS}-[45050]

{C}-[Units:]   {S}-[Packed By:]   {AI}-[RECEIVED IN GOOD CONDITION]
{C}-[Packages:]   {S}-[Packed Date:]   {AI}-[Transport Co: ]
{C}-[Pallets:]   {S}-[Checked By:]   {AI}-[Service Level: ]
{C}-[Weight:]   {S}-[Checked Date:]   {AI}-[Receiver Name:]
{C}-[Cubic:]   {AI}-[Signature:]
{AI}-[Date:]

{AI}-[Printed By: Developer ]
";

		const string ExpectedResultForTestGroupByWithPageBreak_KeepInSamePage =
@"{B}-[DOCUMENT HEADER]
{B}-[Other Document Header]
{B}-[Other Document Body]
{B}-[This is the Section Header.]
{B}-[Group BY 1]
{C}-[Element 1-1]
{C}-[Element 1-2]
{C}-[Element 1-3]
{C}-[Element 1-4]
{B}-[Group BY 2]
{C}-[Element 2-1]
{C}-[Element 2-2]
{C}-[Element 2-3]
{C}-[Element 2-4]
{B}-[This is the Section Page Header]
{B}-[Group BY 3]
{C}-[Element 3-1]
{C}-[Element 3-2]
{C}-[Element 3-3]
{C}-[Element 3-4]
{B}-[Group BY 4]
{C}-[Element 4-1]
{C}-[Element 4-2]
{C}-[Element 4-3]
{C}-[Element 4-4]
{B}-[This is the Section Page Header]
{B}-[Group BY 5]
{C}-[Element 5-1]
{C}-[Element 5-2]
{C}-[Element 5-3]
{C}-[Element 5-4]";

		const string ExpectedResultForTestGroupByWithPageBreak_MustSplitIfCannotBeKeptInSamePage =
@"{B}-[DOCUMENT HEADER]
{B}-[Other Document Header]
{B}-[Other Document Body]
{B}-[This is the Section Header.]
{B}-[This is the Section Page Header]
{B}-[Group BY 11]
{C}-[Element 11-1]
{C}-[Element 11-2]
{C}-[Element 11-3]
{C}-[Element 11-4]
{C}-[Element 11-5]
{C}-[Element 11-6]
{C}-[Element 11-7]
{C}-[Element 11-8]
{C}-[Element 11-9]
{C}-[Element 11-10]
{C}-[Element 11-11]
{C}-[Element 11-12]
{B}-[This is the Section Page Header]
{B}-[Group BY 12]
{C}-[Element 12-1]
{C}-[Element 12-2]
{C}-[Element 12-3]
{C}-[Element 12-4]
{C}-[Element 12-5]
{C}-[Element 12-6]
{C}-[Element 12-7]
{C}-[Element 12-8]
{C}-[Element 12-9]
{C}-[Element 12-10]
{C}-[Element 12-11]
{C}-[Element 12-12]
{C}-[Element 12-13]
{B}-[This is the Section Page Header]
{C}-[Element 12-14]
{C}-[Element 12-15]
{C}-[Element 12-16]
{B}-[Group BY 13]
{C}-[Element 13-1]
{C}-[Element 13-2]
{C}-[Element 13-3]
{C}-[Element 13-4]
{C}-[Element 13-5]
{B}-[This is the Section Page Header]
{B}-[Group BY 14]
{C}-[Element 14-1]
{C}-[Element 14-2]
{C}-[Element 14-3]
{C}-[Element 14-4]
{C}-[Element 14-5]
{B}-[This is the Section Page Header]
{B}-[Group BY 15]
{C}-[Element 15-1]
{C}-[Element 15-2]
{C}-[Element 15-3]
{C}-[Element 15-4]
{C}-[Element 15-5]
{C}-[Element 15-6]
{C}-[Element 15-7]
{C}-[Element 15-8]
{C}-[Element 15-9]
{C}-[Element 15-10]
{C}-[Element 15-11]
{C}-[Element 15-12]
{B}-[This is the Section Page Header]
{B}-[Group BY 16]
{C}-[Element 16-1]
{C}-[Element 16-2]
{C}-[Element 16-3]
{C}-[Element 16-4]
{C}-[Element 16-5]
{C}-[Element 16-6]
{C}-[Element 16-7]
{C}-[Element 16-8]
{C}-[Element 16-9]
{C}-[Element 16-10]
{C}-[Element 16-11]
{C}-[Element 16-12]
{C}-[Element 16-13]
{B}-[This is the Section Page Header]
{C}-[Element 16-14]
{C}-[Element 16-15]
{C}-[Element 16-16]
{C}-[Element 16-17]
{C}-[Element 16-18]
{C}-[Element 16-19]
{C}-[Element 16-20]
{C}-[Element 16-21]
{C}-[Element 16-22]
{C}-[Element 16-23]
{C}-[Element 16-24]
{C}-[Element 16-25]
{C}-[Element 16-26]
{C}-[Element 16-27]
{B}-[This is the Section Page Header]
{C}-[Element 16-28]
{C}-[Element 16-29]
{C}-[Element 16-30]
{C}-[Element 16-31]
{C}-[Element 16-32]
{C}-[Element 16-33]
{C}-[Element 16-34]
{C}-[Element 16-35]
{C}-[Element 16-36]
{C}-[Element 16-37]
{C}-[Element 16-38]
{C}-[Element 16-39]
{C}-[Element 16-40]
{C}-[Element 16-41]
{B}-[This is the Section Page Header]
{C}-[Element 16-42]
{C}-[Element 16-43]
{C}-[Element 16-44]
{C}-[Element 16-45]
{C}-[Element 16-46]
{C}-[Element 16-47]
{C}-[Element 16-48]
{C}-[Element 16-49]
{C}-[Element 16-50]
{C}-[Element 16-51]
{C}-[Element 16-52]
{C}-[Element 16-53]
{C}-[Element 16-54]
{C}-[Element 16-55]
{B}-[This is the Section Page Header]
{C}-[Element 16-56]
{C}-[Element 16-57]
{C}-[Element 16-58]
{C}-[Element 16-59]
{C}-[Element 16-60]
{C}-[Element 16-61]
{C}-[Element 16-62]
{C}-[Element 16-63]
{C}-[Element 16-64]
{C}-[Element 16-65]
{C}-[Element 16-66]
{C}-[Element 16-67]
{C}-[Element 16-68]
{C}-[Element 16-69]
{B}-[This is the Section Page Header]
{C}-[Element 16-70]
{B}-[Group BY 21]
{C}-[Element 21-1]
{C}-[Element 21-2]
{B}-[Group BY 22]
{C}-[Element 22-1]
{C}-[Element 22-2]
{B}-[Group BY 23]
{C}-[Element 23-1]
{C}-[Element 23-2]
{B}-[Group BY 24]
{C}-[Element 24-1]
{C}-[Element 24-2]
{B}-[This is the Section Page Header]
{B}-[Group BY 25]
{C}-[Element 25-1]
{C}-[Element 25-2]
{B}-[Group BY 26]
{C}-[Element 26-1]
{C}-[Element 26-2]
{C}-[Element 26-3]";

		const string ExpectedResultForTestGroupByWithPageBreak_NestedGroupByKeepInSamePage =
@"{B}-[DOCUMENT HEADER]
{B}-[Other Document Header]
{B}-[Other Document Body]
{B}-[This is the Section Header.]
{B}-[Group BY 11]
{B}-[Group BY A1]
{C}-[Element 11-A1-1]
{C}-[Element 11-A1-2]
{B}-[Group BY B1]
{C}-[Element 11-B1-1]
{B}-[This is the Section Page Header]
{B}-[Group BY 12]
{B}-[Group BY A2]
{C}-[Element 12-A2-1]
{B}-[Group BY B2]
{C}-[Element 12-B2-1]
{C}-[Element 12-B2-2]
{C}-[Element 12-B2-3]
{C}-[Element 12-B2-4]
{C}-[Element 12-B2-5]
{C}-[Element 12-B2-6]
{C}-[Element 12-B2-7]
{C}-[Element 12-B2-8]
{B}-[This is the Section Page Header]
{B}-[Group BY 13]
{B}-[Group BY A3]
{C}-[Element 13-A3-1]
{C}-[Element 13-A3-2]
{C}-[Element 13-A3-3]
{B}-[Group BY B3]
{C}-[Element 13-B3-1]
{C}-[Element 13-B3-2]
{C}-[Element 13-B3-3]
{C}-[Element 13-B3-4]
{C}-[Element 13-B3-5]";
	}
}
