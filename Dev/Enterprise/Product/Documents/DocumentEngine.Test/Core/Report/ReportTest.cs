using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using CargoWise.Application;
using CargoWise.Bi.Common;
using CargoWise.BuildTools;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.Data.Utils;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.DocumentEngine.Business;
using Enterprise.DocumentEngine.DataProviders;
using Enterprise.DocumentEngine.DeliveryMethods;
using Enterprise.DocumentEngine.DocBuilder;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentEngine.ReportErrorManagement;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.DocumentEngine.RuntimeOptions.Testing;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.DocumentEngine.Scheduler.Business.Testing;
using Enterprise.DocumentEngine.Testing.UtilityClasses;
using Enterprise.DocumentEngine.ValueReplacers;
using Enterprise.DocumentEngineCore;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.DocumentEngineIntegration;
using Enterprise.DocumentWrappersCore;
using Enterprise.Environment;
using Enterprise.ExcelTemplates;
using Enterprise.Integration;
using Enterprise.Integration.DocumentEngine;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.Scheduler.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using FlexCel.Core;
using FlexCel.XlsAdapter;
using Moq;
using Moq.Protected;
using NUnit.Framework;
using DataContext = Enterprise.Core.Constants.DataContext;

namespace Enterprise.DocumentEngine.Testing
{
	[TestedType(typeof(Report))]
	public sealed class ReportTest : NonPersistentBusinessObjectTestCase
	{
		public void TestNotificationEmailSubjectAndSignatureWhenEmptyReportContingencyTypeIsEML()
		{
			var emailFormat = new EmailFormat();
			emailFormat.EmailSubjectFields.RemoveAndDeleteAll();
			emailFormat.EmailSubjectFields.Add(new EmailSubjectField("1", Core.Constants.EmailFormat.EmailFieldCodes.DocumentName));
			emailFormat.EmailSignatureFields.RemoveAndDeleteAll();
			emailFormat.EmailSignatureFields.Add(new EmailSignatureField("1", Enterprise.Core.Constants.EmailFormat.EmailFieldCodes.BranchName));
			emailFormat.EmailSignatureFields.Add(new EmailSignatureField("2", Enterprise.Core.Constants.EmailFormat.EmailFieldCodes.UserName));
			DocumentsDataRegistry.Instance.EmailFormat.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, emailFormat);

			ReportCommand reportCommand = null;

			using (var templateStream = new MemoryStream())
			{
				var excelTemplate = CreateTestEmptyReportContingencyEMLTemplate(templateStream, false, false);
				reportCommand = DocumentEngineTestHelper.CreateReportCommandWithExcelTemplate("Test Report1", excelTemplate, Factory);
			}

			RunPrintTask(reportCommand);

			var printJob = Factory.LoadTop1<StmPrintJob>(new ZQuery(StmPrintJobSchema.SP_EmailAttachments, "Test Report1.XLSX"));
			AssertNull(printJob);
			var emails = Env.OutgoingMailManager.EmailsCreated;
			AssertEquals("One email should have been created", 1, emails.Count);
			AssertContains("Test Report1 not delivered", emails[0].Subject);
			AssertContains("The resulting document was empty and therefore has not been delivered.\r\n\r\nBN - AUBNE\r\nCargoWise Support", emails[0].Body);

			emailFormat.EmailSignatureFields.RemoveAndDeleteAll();
			DocumentsDataRegistry.Instance.EmailFormat.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, emailFormat);

			RunPrintTask(reportCommand);

			printJob = Factory.LoadTop1<StmPrintJob>(new ZQuery(StmPrintJobSchema.SP_EmailAttachments, "Test Report1.XLSX"));
			AssertNull(printJob);
			emails = Env.OutgoingMailManager.EmailsCreated;
			AssertEquals("new email should have been created", 2, emails.Count);
			AssertContains("Test Report1 not delivered", emails[1].Subject);
			AssertContains("The resulting document was empty and therefore has not been delivered.\r\n\r\nEagle Datamation International\nBN - AUBNE", emails[1].Body);
		}

		public void TestDoNotClearRenderedTemplateIfDocContactWasNotAccessedInTemplate()
		{
			var template = DocumentEngineTestHelper.CreateExcelTemplateFromString("Test", string.Empty,
				@"{A}-[#Config]
{A}-[Name=Test]
{A}-[Data:ReportData=select Top 10 * from dbo.GlbStaff]
{A}-[#SectionBody:Data=ReportData]
{B}-[<ReportData.GS_AutoVersion>]]
{A}-[#SectionFooter]
{B}-[<Total ReportData.GS_AutoVersion>]
{A}-[#EndOfReport]");

			using (var report = new Report(Pack, template))
			{
				using (var outputStream = new MemoryStream())
				{
					Report.RenderedWorkSheetsForTesting = new List<ExcelWorkSheet>();
					var contact1 = new DocDeliveryContact(new BusinessObjectFactory())
					{
						Name = "Test Contact 1",
						DeliveryMethod = Enterprise.Core.Constants.ContactNotifyModes.Email,
						Email = "test1@cargowise.com",
						AttachmentType = AttachmentTypeList.Codes.Xls
					};
					((IReportForUnitTesting)report).DeliveryContact = contact1;

					report.Save(outputStream);
					Assert(report.IsGenerated);
					AssertEquals(1, Report.RenderedWorkSheetsForTesting.Count);

					var contact2 = new DocDeliveryContact(new BusinessObjectFactory())
					{
						Name = "Test Contact 2",
						DeliveryMethod = Enterprise.Core.Constants.ContactNotifyModes.Email,
						Email = "test2@cargowise.com",
						AttachmentType = AttachmentTypeList.Codes.Xls
					};
					((IReportForUnitTesting)report).DeliveryContact = contact2;

					report.Save(outputStream);
					Assert(report.IsGenerated);
					AssertEquals("the report should be cached as DeliveryContact is only used to file type.", 1, Report.RenderedWorkSheetsForTesting.Count);

					Report.RenderedWorkSheetsForTesting = null;
				}
			}

			var template1 = DocumentEngineTestHelper.CreateExcelTemplateFromString("Test", string.Empty,
				@"{A}-[#Config]
{A}-[Name=Test]
{A}-[Data:ReportData=select Top 10 * from dbo.GlbStaff]
{A}-[#PageHeader]
{B}-[<RecipientEmailAddress>]
{A}-[#SectionBody:Data=ReportData]
{B}-[<ReportData.GS_AutoVersion>]
{A}-[#SectionFooter]
{A}-[<Total ReportData.GS_AutoVersion>]
{A}-[#EndOfReport]");

			using (var report = new Report(Pack, template1))
			{
				using (var outputStream = new MemoryStream())
				{
					Report.RenderedWorkSheetsForTesting = new List<ExcelWorkSheet>();
					var contact1 = new DocDeliveryContact(new BusinessObjectFactory())
					{
						Name = "Test Contact 1",
						DeliveryMethod = Enterprise.Core.Constants.ContactNotifyModes.Email,
						Email = "test1@cargowise.com",
						AttachmentType = AttachmentTypeList.Codes.Xls
					};
					((IReportForUnitTesting)report).DeliveryContact = contact1;

					report.Save(outputStream);
					Assert(report.IsGenerated);
					AssertEquals(1, Report.RenderedWorkSheetsForTesting.Count);

					var contact2 = new DocDeliveryContact(new BusinessObjectFactory())
					{
						Name = "Test Contact 2",
						DeliveryMethod = Enterprise.Core.Constants.ContactNotifyModes.Email,
						Email = "test2@cargowise.com",
						AttachmentType = AttachmentTypeList.Codes.Xls
					};
					((IReportForUnitTesting)report).DeliveryContact = contact2;

					report.Save(outputStream);
					Assert(report.IsGenerated);
					AssertEquals("report should not be cached as <RecipientEmailAddress> is recipient specified.", 2, Report.RenderedWorkSheetsForTesting.Count);

					Report.RenderedWorkSheetsForTesting = null;
				}
			}
		}

		public void TestReportLanguage_WhenTranslateLegacyDocumentAndForcedLanguageExist_ShouldReturnForcedLanguage()
		{
			var excelTemplate = DocumentEngineTestHelper.CreateExcelTemplateFromString(
				"Test", string.Empty,
				@"{A}-[#Config]
{A}-[Name=TestTemplate]
{A}-[TRANSLATELEGACYDOCUMENT]
{A}-[ForcedLanguage=ZH-CN]
{A}-[#EndOfReport]");

			var dataSource = Factory.New<DummyBusinessObject>();
			using (var report = new Report(new DocumentPack(), excelTemplate, BODocDataProvider.Get(dataSource), "Test_" + excelTemplate.TemplateName, null, DocumentDirection.ANY, false))
			{
				var documentTemplate = Factory.New<StmTemplateBase>();
				documentTemplate.SO_Name = "Report";
				report.StTemplate = documentTemplate;

				report.PrepareForRender();

				AssertEquals(Core.Constants.Languages.ChineseSimplified, report.Language);
			}
		}

		public void TestIDeliverableWithDocDeliveryPrintDetails()
		{
			var menuItem = Factory.New<StmMenuItem>();
			menuItem.SU_IsSystemDefined = true;
			menuItem.SU_MenuName = "TestReportFieldNotFoundErrorsForFullyQualifiedMacros";
			menuItem.SU_BusinessContext = "Shipment";
			var dummy = Factory.New<DummyDocumentSupportable>();
			dummy.Collection.AddNew("AAA", "AAA Description");

			var excelTemplate = DocumentEngineTestHelper.CreateExcelTemplateFromString(
			"Test", string.Empty,
			@"{A}-[#Config]
{A}-[Name=TestTemplate]
{A}-[#EndOfReport]");

			var stmPrintQueue = Factory.NewWithValidTestData<StmPrintQueue>();
			stmPrintQueue.SQ_DisplayName = "Printer 1";
			stmPrintQueue.SQ_ServerName = "TEST";

			using (var documentPack = new DocumentPack(menuItem))
			using (var report = new Report(documentPack, excelTemplate))
			{
				var iDeliverable = report as IDeliverable;
				AssertNotNull(iDeliverable);
				AssertEquals("should be empty default.", null, iDeliverable.PrinterDetails.PrintQueue);
				AssertEquals("should be 1 default.", 1, iDeliverable.PrinterDetails.NumberOfCopies);

				report.PrinterDetails.PrintQueuePK = ZGuid.Invalid;
				report.PrinterDetails.NumberOfCopies = 2;

				AssertEquals("should be 2.", (ZShort)2, iDeliverable.PrinterDetails.NumberOfCopies);
				AssertEquals("should be null as it's invalid printer name.", null, report.PrinterDetails.PrintQueue);
				Assert("should has error.", report.PrinterDetails.PrintQueuePKInfo.HasError("Enter a valid Printer."));

				report.PrinterDetails.PrintQueuePK = stmPrintQueue.PK;
				AssertEquals(stmPrintQueue, report.PrinterDetails.PrintQueue);
				Assert("should have no error.", !report.PrinterDetails.PrintQueuePKInfo.HasErrors());
			}
		}

		public void TestSpecificPageRangesDoesNotCacheResult()
		{
			var template = DocumentEngineTestHelper.CreateExcelTemplateFromString("Test", string.Empty,
				@"{A}-[#Config]
{A}-[Name=Test]
{A}-[#SectionBody:Data=Collection]
{B}-[<Collection.Z0_Description>]
{B}-[<Collection.CollectionOwnProperty>]
{A}-[#EndOfReport]");
			var menuItem = Factory.New<StmMenuItem>();
			menuItem.SU_IsSystemDefined = true;
			menuItem.SU_MenuName = "TestSpecificPageRangesDoesNotCacheResult";
			menuItem.SU_BusinessContext = ".DummyBusinessObject";
			var dummy = Factory.New<DummyDocumentSupportable>();
			dummy.Collection.AddNew("AAA", "AAA Description");
			dummy.Collection.AddNew("BBB", "BBB Description");
			dummy.Collection.AddNew("CCC", "CCC Description");

			using (var documentPack = new DocumentPack(menuItem))
			using (var report = new Report(documentPack, template, BODocDataProvider.Get(dummy), null, null, null, DocumentDirection.ANY, false))
			{
				var stTemplate = Factory.New<StmTemplateBase>();
				stTemplate.SO_Name = "WhatEver";
				stTemplate.SO_ExcelTemplatePath = "WhatEver.xls";
				stTemplate.SO_IsSystemDefined = true;
				report.StTemplate = stTemplate;

				AssertEquals(true, report.CanSpecifyPageRanges);

				using (var stream1 = new MemoryStream())
				using (var stream2 = new MemoryStream())
				using (var stream3 = new MemoryStream())
				using (var stream4 = new MemoryStream())
				{
					report.Save(stream1);
					report.PageRangesSpecified = true;
					report.UpdateSpecifiedDataRowSource(new int[2] { 1, 3 });
					report.Save(stream2);
					report.UpdateSpecifiedDataRowSource(new int[2] { 1, 2 });
					report.Save(stream3);
					report.Save(stream4);

					AssertNotEquals(stream1.ToArray(), stream2.ToArray());
					AssertNotEquals(stream2.ToArray(), stream3.ToArray());
					AssertEquals(stream3.ToArray(), stream4.ToArray());
				}
			}
		}

		public void TestReportDataSourceWhenEnableSpecificPageRangesPrintingOption()
		{
			var content = new Dictionary<string, string>();
			content.Add("Sheet1",
	@"{A}-[#Config]
{A}-[Name=Test]
{A}-[Data:ReportData=SELECT Z0_Description FROM dbo.DummyBizo WHERE <Filter> = 'TEST']
{A}-[#SectionBody:Data=ReportData]
{B}-[<ReportData.Z0_Description>]
{A}-[#EndOfReport]");

			content.Add("Filter",
				@"{A}-[Filter] {B}-[Type] {C}-[Text]
{A}-[#End]");

			var template = DocumentEngineTestHelper.CreateExcelTemplateFromString("Test", string.Empty, content);

			using (var report = new Report(new DocumentPack(), template))
			{
				AssertEquals(false, report.CanSpecifyPageRanges);
				AssertEquals(0, report.ErrorManager.ErrorsCount);
			}
		}

		public void TestCanUseSpecificPageRangesWithMultiSectionBodyArea()
		{
			var template = DocumentEngineTestHelper.CreateExcelTemplateFromString("Test", string.Empty,
				@"{A}-[#Config]
{A}-[Name=Test]
{A}-[#SectionBody:Data=Collection]
{B}-[<Collection.ZStringProperty>]
{B}-[<Collection.CollectionOwnProperty>]
{A}-[#EndOfReport]");
			var menuItem = Factory.New<StmMenuItem>();
			menuItem.SU_IsSystemDefined = true;
			menuItem.SU_MenuName = "TestCanUseSpecificPageRangesWithMultiSectionBodyArea";
			menuItem.SU_BusinessContext = "Shipment";
			var dummy = Factory.New<DummyDocumentSupportable>();
			dummy.Collection.AddNew("AAA", "AAA Description");
			dummy.Collection.AddNew("BBB", "BBB Description");

			using (var documentPack = new DocumentPack(menuItem))
			using (var report = new Report(documentPack, template, BODocDataProvider.Get(dummy), null, null, null, DocumentDirection.ANY, false))
			{
				AssertEquals(true, report.CanSpecifyPageRanges);
				AssertEquals("datasource is not null when there is only 1 sectionbody area in the report.", 2, report.DataRowSourceRowCountIfPageRangesSpecifiable);
			}
		}

		public void TestCanUseSpecificPageRangesWithSingleSectionBodyArea()
		{
			var template = DocumentEngineTestHelper.CreateExcelTemplateFromString("Test", string.Empty,
				@"{A}-[#Config]
{A}-[Name=Test]
{A}-[#SectionBody:Data=Collection]
{B}-[<Collection.ZStringProperty>]
{B}-[<Collection.CollectionOwnProperty>]
{A}-[#SectionBody:Data=Collection]
{B}-[<Collection.ZStringProperty>]
{B}-[<Collection.CollectionOwnProperty>]
{A}-[#EndOfReport]");
			var menuItem = Factory.New<StmMenuItem>();
			menuItem.SU_IsSystemDefined = true;
			menuItem.SU_MenuName = "TestCanUseSpecificPageRangesWithSingleSectionBodyArea";
			menuItem.SU_BusinessContext = "Shipment";
			var dummy = Factory.New<DummyDocumentSupportable>();
			dummy.Collection.AddNew("AAA", "AAA Description");
			dummy.Collection.AddNew("BBB", "BBB Description");

			using (var documentPack = new DocumentPack(menuItem))
			using (var report = new Report(documentPack, template, BODocDataProvider.Get(dummy), null, null, null, DocumentDirection.ANY, false))
			{
				AssertEquals(false, report.CanSpecifyPageRanges);
				AssertEquals("datasource is null when there are more than 1 sectionbody areas in the report.", 0, report.DataRowSourceRowCountIfPageRangesSpecifiable);
			}
		}

		[GuiTest]
		public void TestReportIndexOutOfRangeExceptionWontBeThrown()
		{
			var postMasterGroup = Factory.Load<GlbGroup>(Core.Constants.Groups.PostMastersGroupPK);
			var postMaster = postMasterGroup.Staff.AddNew();
			postMaster.GS_EmailAddress = "justin@email.com";
			postMaster.GS_Code = "___";
			postMaster.GS_LoginName = "jnc";
			Factory.Save();

			var template = DocumentEngineTestHelper.CreateExcelTemplateFromString("Test", string.Empty,
				@"{A}-[#Config]
{A}-[Name=Test]
{A}-[#SectionBody:Data=Collection]
{B}-[<Collection.ZStringArray[100]>]
{A}-[#EndOfReport]");
			var menuItem = Factory.New<StmMenuItem>();
			menuItem.SU_IsSystemDefined = true;
			menuItem.SU_MenuName = "TestReportIndexOutOfRangeExceptionWontBeThrown";
			menuItem.SU_BusinessContext = "Shipment";
			var dummy = Factory.New<DummyDocumentSupportable>();
			var child1 = dummy.Collection.AddNew("AAA", "AAA Description");
			child1.ZStringArray = new ZString[] { "Value1", "Value2" };
			var child2 = dummy.Collection.AddNew("BBB", "BBB Description");
			child2.ZStringArray = new ZString[] { "Value3", "Value4" };

			var stTemplate = Factory.New<StmTemplateBase>();
			stTemplate.SO_Name = "WhatEver";
			stTemplate.SO_ExcelTemplatePath = "WhatEver.xls";
			var messageContained = "Error getting value for field 'ZStringArray[100]': Index was outside the bounds of the array.";

			using (var documentPack = new DocumentPack(menuItem))
			{
				using (var report = new Report(documentPack, template, BODocDataProvider.Get(dummy), null, null, null, DocumentDirection.ANY, false))
				{
					((IReportForUnitTesting)report).StopErrorsThrowingAnException = true;
					stTemplate.SO_IsSystemDefined = false;
					report.StTemplate = stTemplate;

					using (var stream = new MemoryStream())
					{
						AssertNoExceptionThrown(() => report.Save(stream));
						AssertEquals("Emails to Client", 1, Env.OutgoingMailManager.EmailsCreated.Count);
						var email = Env.OutgoingMailManager.EmailsCreated[0];
						AssertContains(messageContained, email.Body);
						AssertNullOrEmpty(ErrorReporter.LastMessageReported);
					}
				}

				Env.OutgoingMailManager.EmailsCreated.Clear();

				using (var report = new Report(documentPack, template, BODocDataProvider.Get(dummy), null, null, null, DocumentDirection.ANY, false))
				{
					((IReportForUnitTesting)report).StopErrorsThrowingAnException = true;
					stTemplate.SO_IsSystemDefined = true;
					report.StTemplate = stTemplate;

					using (var stream = new MemoryStream())
					{
						report.Save(stream);
						AssertEquals("Emails to Client", 0, Env.OutgoingMailManager.EmailsCreated.Count);
						AssertContains(messageContained, ErrorReporter.LastMessageReported);

						ErrorReporter.Instance.Clear();
					}
				}
			}
		}

		public void TestEmailSubjectHasAngleBrackets()
		{
			using (Stream templateStream = new MemoryStream())
			{
				DocumentEngineTestHelper.GenerateTemplateStreamFromString(templateStream,
@"{A}-[#Config]
{A}-[Name=TestEmailSubjectMacros]
{A}-[EmailSubject=<CompanyName>]
{A}-[#EndOfReport]");

				var excelTemplate = new ExcelTemplateWrappingStream("Test", templateStream);

				using (new TemporaryValueSetter<string>(value => GlbCompany.CurrentCompany.GC_Name = value, GlbCompany.CurrentCompany.GC_Name, "<AAA>"))
				using (var report = new Report(new DocumentPack(), excelTemplate))
				{
					report.PrepareForRender();

					using (var outputStream = new MemoryStream())
					{
						report.Save(outputStream);
						using (var excelInterface = new ExcelInterface())
						{
							excelInterface.LoadExcelFile(outputStream);
							AssertEquals($"<AAA> - {GlbBranch.CurrentBranch.GB_BranchName} - <AAA>",
								report.EmailSubject);
						}
					}
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestReportIsInactiveWithError()
		{
			var factory = new BusinessObjectFactory();

			var command = factory.NewWithValidTestData<ReportCommand>();
			var excelTemplate = new ExcelTemplateForUnitTesting("Test.xls", TestFilesSubFolder.ReportTestFiles);
			var scheduledReport = factory.NewWithValidTestData<ReportScheduleTask>();
			var printTaskUIProvider = new PrintTaskUIProviderForTesting();

			using (new PrintTaskUIProviderFactory.OverriderForTesting(printTaskUIProvider))
			using (var report = new Report(Pack, excelTemplate, Guid.Empty, Core.Constants.DataContext.UnitTest))
			{
				report.SetScheduleTask(scheduledReport);
				((IReportForUnitTesting)report).StopErrorsThrowingAnException = true;
				AssertEquals("Pre: Scheduled Report is valid.", true, report.ScheduleTask.S5_IsActive);
				report.ErrorManager.Add(new ReportProcessingError("Moo", ReportProcessingErrorSeverity.Warning));
				printTaskUIProvider.ResetShowErrors();
				((IReportForUnitTesting)report).ShowErrorsIfAny();
				AssertEquals("Scheduled Report is valid when warning detected.", true, report.ScheduleTask.S5_IsActive);
			}

			using (new PrintTaskUIProviderFactory.OverriderForTesting(printTaskUIProvider))
			using (var report = new Report(Pack, excelTemplate, Guid.Empty, Core.Constants.DataContext.UnitTest))
			{
				report.SetScheduleTask(scheduledReport);
				((IReportForUnitTesting)report).StopErrorsThrowingAnException = true;
				AssertEquals("Pre: Scheduled Report is valid.", true, report.ScheduleTask.S5_IsActive);
				report.ErrorManager.Add(new ReportProcessingError("Moo", ReportProcessingErrorSeverity.Error));
				printTaskUIProvider.ResetShowErrors();
				((IReportForUnitTesting)report).ShowErrorsIfAny();
				AssertEquals("Scheduled Report is invalid when error detected.", false, report.ScheduleTask.S5_IsActive);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestToStringWithScheduledTask()
		{
			var factory = new BusinessObjectFactory();

			var command = factory.NewWithValidTestData<ReportCommand>();
			command.SU_BusinessContext = "Custom";
			command.SU_MenuName = "Delivery Order";
			command.SU_MenuPath = "Misc/";
			command.SU_FilterList = "CTY=GB";
			command.SU_IsSystemDefined = true;
			command.SU_IsClientSpecific = true;
			var excelTemplate = new ExcelTemplateForUnitTesting("Test.xls", TestFilesSubFolder.ReportTestFiles);
			var scheduledReport = factory.NewWithValidTestData<ReportScheduleTask>();
			scheduledReport.S5_ParentID = command.PK;
			scheduledReport.S5_ScheduleDescription = "test description";
			var pack = new DocumentPack(command);
			using (var report = new Report(pack, excelTemplate))
			{
				report.SetScheduleTask(scheduledReport);

				AssertMultilineASCIIEquals("report.ToString()", string.Format(@"Report Information:

MenuItem:-
   BusinessContext = [Custom]
   Name with Path = [Misc/Delivery Order]
   Filter = [CTY=GB]
   PK = [{0}]
   IsSystemDefined = [Y]
   IsClientSpecific = [Y]

No StmTemplate Found on the Report.

Scheduled Task:-
   Description = [test description]", report.MenuItem.PK), report.ToString());
			}
		}

		[GuiTest]
		public void TestReportFieldNotFoundErrorsForFullyQualifiedMacros()
		{
			var template = DocumentEngineTestHelper.CreateExcelTemplateFromString("Test", string.Empty,
				@"{A}-[#Config]
{A}-[Name=Test]
{A}-[#SectionBody:Data=Collection]
{B}-[<Collection.ZStringProperty>]
{B}-[<Collection.CollectionOwnProperty>]
{A}-[#EndOfReport]");
			var menuItem = Factory.New<StmMenuItem>();
			menuItem.SU_IsSystemDefined = true;
			menuItem.SU_MenuName = "TestReportFieldNotFoundErrorsForFullyQualifiedMacros";
			menuItem.SU_BusinessContext = "Shipment";
			var dummy = Factory.New<DummyDocumentSupportable>();
			dummy.Collection.AddNew("AAA", "AAA Description");

			using (var documentPack = new DocumentPack(menuItem))
			using (var report = new Report(documentPack, template, BODocDataProvider.Get(dummy), null, null, null, DocumentDirection.ANY, false))
			{
				var stTemplate = Factory.New<StmTemplateBase>();
				stTemplate.SO_Name = "WhatEver";
				stTemplate.SO_ExcelTemplatePath = "WhatEver.xls";
				stTemplate.SO_IsSystemDefined = true;
				report.StTemplate = stTemplate;

				using (var stream = new MemoryStream())
				{
					report.Save(stream);
					AssertEquals("FieldNotFoundErrorsForFullyQualifiedMacros - TestReportFieldNotFoundErrorsForFullyQualifiedMacros - Shipment", ErrorReporter.LastKeyReported);
					Assert(ErrorReporter.LastMessageReported.Contains("<Collection.ZStringProperty>"));
					Assert(!ErrorReporter.LastMessageReported.Contains("<Collection.CollectionOwnProperty>"));
					ErrorReporter.Clear();
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestHPageBreakOverflow()
		{
			AssertHPageBreakOverflow("HPageBreakOverflow.xlsx");
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestHPageBreakOverflowWithPrintTitles()
		{
			AssertHPageBreakOverflow("HPageBreakOverflowWithPrintTitles.xlsx");
		}

		public void TestDeveloperExceptionThrownForDuplicateReportData()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			var excelTemplate = DocumentEngineTestHelper.CreateExcelTemplateFromString(
				"Test", string.Empty,
@"{A}-[#Config]
{A}-[Name=TestTemplate]
{A}-[Version=1.3]
{A}-[PageStyle=Portrait]
{A}-[Data:ReportData1=select '1']
{A}-[Data:ReportData2=select '1']
{A}-[#SectionBody:Data=ReportData1]
{B}-[<ReportData1.column1>]
{A}-[#SectionBody:Data=ReportData2]
{B}-[<ReportData2.column1>]
{A}-[#EndOfReport]");

			using (var documentPack = new DocumentPack())
			using (var report = new Report(documentPack, excelTemplate))
			using (var stream = new MemoryStream())
			{
				report.PrepareForRender();
				AssertExceptionThrown<DocumentEngineException>(@"Errors found generating template:-
Severity: [Warning (without error report)] Message: [The SQL data source below appears multiple times in the report 'TestTemplate'. To improve the report performance, please define this data source only once:select '1'.] Cell Content: []  Cell: [N/A] Sheetname: [(unknown)]


No MenuItem Found on the Report.

No StmTemplate Found on the Report.

Not Running from Scheduled Report.", () => report.Save(stream));
			}
		}

		public void TestRegistryTimeOutValue()
		{
			var excelTemplate = DocumentEngineTestHelper.CreateExcelTemplateFromString(
				"Test", string.Empty,
@"{A}-[#Config]
{A}-[Name=TestTemplate]
{A}-[Version=1.3]
{A}-[PageStyle=Portrait]
{A}-[Data:ReportData1=select '1']
{A}-[Data:ReportData2=select '2']
{A}-[#SectionBody:Data=ReportData1]
{B}-[<ReportData1.column1>]
{A}-[#SectionBody:Data=ReportData2]
{B}-[<ReportData2.column1>]
{A}-[#EndOfReport]");

			using (var documentPack = new DocumentPack())
			using (var report = new Report(documentPack, excelTemplate))
			{
				DocumentsDataRegistry.Instance.ReportDBCommandTimeOut.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 348);
				DocumentsDataRegistry.Instance.ReportPreviewTimeout.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 311);
				report.IsScheduledReport = true;

				report.PrepareForRender();
				AssertEquals("Value overriden by TimeOut to 348", 348.0, report.RemainingTimeForTimeout);
			}
		}

		public void TestPreviewRegistryTimeOutValue()
		{
			var excelTemplate = DocumentEngineTestHelper.CreateExcelTemplateFromString(
				"Test", string.Empty,
@"{A}-[#Config]
{A}-[Name=TestTemplate]
{A}-[Version=1.3]
{A}-[PageStyle=Portrait]
{A}-[Data:ReportData1=select '1']
{A}-[Data:ReportData2=select '2']
{A}-[#SectionBody:Data=ReportData1]
{B}-[<ReportData1.column1>]
{A}-[#SectionBody:Data=ReportData2]
{B}-[<ReportData2.column1>]
{A}-[#EndOfReport]");

			using (var documentPack = new DocumentPack())
			using (var report = new Report(documentPack, excelTemplate))
			{
				DocumentsDataRegistry.Instance.ReportDBCommandTimeOut.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 348);
				DocumentsDataRegistry.Instance.ReportPreviewTimeout.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 311);
				report.IsScheduledReport = false;

				report.PrepareForRender();
				AssertEquals("Value overriden by TimeOut to 311", 311.0, report.RemainingTimeForTimeout);
			}
		}

		public void TestConfigTimeOutValue()
		{
			var excelTemplate = DocumentEngineTestHelper.CreateExcelTemplateFromString(
					 "Test", string.Empty,
@"{A}-[#Config]
{A}-[Name=TestTemplate]
{A}-[Version=1.3]
{A}-[PageStyle=Portrait]
{A}-[SqlTimeout=30]
{A}-[Data:ReportData1=select '1']
{A}-[Data:ReportData2=select '2']
{A}-[#SectionBody:Data=ReportData1]
{B}-[<ReportData1.column1>]
{A}-[#SectionBody:Data=ReportData2]
{B}-[<ReportData2.column1>]
{A}-[#EndOfReport]");

			using (var documentPack = new DocumentPack())
			using (var report = new Report(documentPack, excelTemplate))
			{
				DocumentsDataRegistry.Instance.ReportDBCommandTimeOut.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 348);
				DocumentsDataRegistry.Instance.ReportPreviewTimeout.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 311);

				report.PrepareForRender();
				AssertEquals("Default timeout value is 30s", 30.0, report.RemainingTimeForTimeout);

				report.SetTableProviderTimeConsumed(new NativeSqlTableProvider(), 10);
				AssertEquals("Default timeout value is 20s", 20.0, report.RemainingTimeForTimeout);
			}
		}

		public void TestOverrideTimeOutValue()
		{
			var excelTemplate = DocumentEngineTestHelper.CreateExcelTemplateFromString(
					 "Test", string.Empty,
@"{A}-[#Config]
{A}-[Name=TestTemplate]
{A}-[Version=1.3]
{A}-[PageStyle=Portrait]
{A}-[SqlTimeout=30]
{A}-[Data:ReportData1=select '1']
{A}-[Data:ReportData2=select '2']
{A}-[#SectionBody:Data=ReportData1]
{B}-[<ReportData1.column1>]
{A}-[#SectionBody:Data=ReportData2]
{B}-[<ReportData2.column1>]
{A}-[#EndOfReport]");

			using (var documentPack = new DocumentPack())
			using (var report = new Report(documentPack, excelTemplate))
			{
				DocumentsDataRegistry.Instance.ReportDBCommandTimeOut.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 348);
				DocumentsDataRegistry.Instance.ReportPreviewTimeout.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 311);

				report.TimeOut = 100;
				report.PrepareForRender();
				AssertEquals("Value overriden by TimeOut to 100s", 100.0, report.RemainingTimeForTimeout);
			}
		}

		public void TestAccessingVisualizerContentNoteMultipleTimesDoesNotCreateMultipleFactories()
		{
			using (var report = new Report(Pack, null))
			{
				AssertNumberOfFactories(report, true);
			}

			using (var report = new Report(null, null))
			{
				AssertNumberOfFactories(report, false);
			}

			void AssertNumberOfFactories(Report report, bool extraFactory)
			{
				var initialCount = BusinessObjectFactory._NextInstance;
				var content = report.VisualizerContentNote;
				content = report.VisualizerContentNote;
				content = report.VisualizerContentNote;

				var currentCount = BusinessObjectFactory._NextInstance;
				AssertEquals("Only one new factory should have been created", initialCount + (extraFactory ? 1 : 0), currentCount);
			}
		}

		public void TestReportLanguage_TranslateLegacyDocument()
		{
			var excelTemplate = DocumentEngineTestHelper.CreateExcelTemplateFromString(
				"Test", string.Empty,
				@"{A}-[#Config]
{A}-[Name=TestTemplate]
{A}-[TranslateLegacyDocument]
{A}-[#EndOfReport]");

			using (var documentPack = new DocumentPack())
			using (var report = new Report(documentPack, excelTemplate))
			{
				documentPack.Language = Core.Constants.Languages.ChineseSimplified;
				report.PrepareForRender();
				AssertEquals("This legacy document should be translated", true, report.TranslateLegacyDocument);
				AssertNotEquals("This legacy document's language should not default to English", true, report.Language);
				AssertEquals(Core.Constants.Languages.ChineseSimplified, report.Language);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestReportNotRenderedFromCacheWithLanguangeSwitched()
		{
			var excelTemplate = new ExcelTemplateForUnitTesting("TestEmptyDocument.xls", TestFilesSubFolder.DocumentTestFiles);
			using (var report = new Report(Pack, excelTemplate))
			{
				using (var outputStream = new MemoryStream())
				{
					Pack.Language = Core.SharedConstants.Languages.EnglishAmerican;
					Report.RenderedWorkSheetsForTesting = new List<ExcelWorkSheet>();
					Assert(!report.IsGenerated);
					Assert(string.IsNullOrEmpty(report.RenderedLanguage));

					report.Save(outputStream);
					Assert(report.IsGenerated);
					Assert(report.RenderedLanguage == Core.SharedConstants.Languages.EnglishAmerican);
					AssertEquals(1, Report.RenderedWorkSheetsForTesting.Count);

					report.Save(outputStream);
					Assert(report.RenderedLanguage == Core.SharedConstants.Languages.EnglishAmerican);
					AssertEquals(1, Report.RenderedWorkSheetsForTesting.Count);

					Pack.Language = Core.SharedConstants.Languages.ChineseSimplified;
					report.Save(outputStream);
					Assert(report.RenderedLanguage == Core.SharedConstants.Languages.ChineseSimplified);
					AssertEquals(2, Report.RenderedWorkSheetsForTesting.Count);

					Report.RenderedWorkSheetsForTesting = null;
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestDocumentLanguage()
		{
			var excelTemplate = new ExcelTemplateForUnitTesting("TemplateWithTranslateTab.xls", TestFilesSubFolder.DocumentTestFiles);
			var dataSource = Factory.New<DummyBusinessObject>();
			var dataProviders = new DataProviderList(BODocDataProvider.Get(dataSource));

			using (var documentPack = new DocumentPack())
			using (var report = new Report(documentPack, excelTemplate, Guid.Empty, Enterprise.Core.Constants.DataContext.UnitTest))
			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.ChineseSimplified))
			{
				documentPack.Add(report);
				var reportTemplate = Factory.New<StmTemplateBase>();
				reportTemplate.SO_Name = "Report";
				report.StTemplate = reportTemplate;
				AssertEquals("Style", Report.Styles.Report, report.Style);
				AssertEquals("Language should be eqaul to current language for report", Core.SharedConstants.Languages.ChineseSimplified, report.Language);
			}

			using (var documentPack = new DocumentPack())
			using (var document = new Report(documentPack, excelTemplate, dataProviders, "Test", null, DocumentDirection.ANY, false))
			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.ChineseSimplified))
			{
				documentPack.Add(document);
				var documentTemplate = Factory.New<StmTemplateBase>();
				documentTemplate.SO_Name = Enterprise.Core.Constants.SectionRepositoryTemplateNames.System;
				document.StTemplate = documentTemplate;
				AssertEquals("Style", Report.Styles.Document, document.Style);
				AssertEquals("Language should be eqaul to defalut lanague EUS for DocBuiler document", Core.SharedConstants.Languages.EnglishAmerican, document.Language);

				documentTemplate.SO_Name = "LegacyDocument";
				AssertEquals("Language should be eqaul to EUS language for Legacy document", Core.SharedConstants.Languages.EnglishAmerican, document.Language);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestLanguageShouldBeConsistentWithRegistryForLegacyDocument()
		{
			var excelTemplate = new ExcelTemplateForUnitTesting("TemplateWithTranslateTab.xls", TestFilesSubFolder.DocumentTestFiles);
			var dataSource = Factory.New<DummyBusinessObject>();
			var dataProviders = new DataProviderList(BODocDataProvider.Get(dataSource));

			using (var documentPack = new DocumentPack())
			using (var document = new Report(documentPack, excelTemplate, dataProviders, "Test", null, DocumentDirection.ANY, false))
			using (RawDataRegistry.Instance.EnglishSpelling.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, Core.SharedConstants.Languages.EnglishBritish))
			{
				documentPack.Add(document);
				var documentTemplate = Factory.New<StmTemplateBase>();
				documentTemplate.SO_Name = "LegacyDocument";
				document.StTemplate = documentTemplate;
				AssertEquals(Core.SharedConstants.Languages.EnglishBritish, document.Language);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestErrorReportedWhenTemplateHasTranslationsSheet()
		{
			TestData.CreateJobTestTable();
			TestData.CreateHeaderTestTable();
			TestData.CreateLinesTestTable();
			var excelTemplate = new ExcelTemplateForUnitTesting("TemplateWithTranslateTab.xls", TestFilesSubFolder.DocumentTestFiles);
			using (Report.TemporarilyUseMainConnection())
			using (var report = new Report(Pack, excelTemplate, Guid.Empty, Enterprise.Core.Constants.DataContext.UnitTest))
			{
				using (var outputStream = new MemoryStream())
				{
					try
					{
						report.Save(outputStream);
					}
					catch (DocumentEngineException e)
					{
						Assert(e.Message.Contains("'Translations' sheet was obsoleted, please delete it."));
					}
				}
			}
		}

		[SnailTest]
		public void TestTooManyRowsExceptionHandled_Hits2003Limit_XLS_HasNotifications_UserDoesNotProceedsWithNewFormat()
		{
			UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.No);

			using (DocumentsDataRegistry.Instance.AutomaticallySwitchToXLSXIfRequired.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AssertTooManyRowsInDocumentExceptionHandled(nbBusinessObjectsToFail2003, AttachmentTypeList.Codes.Xls, AttachmentTypeList.Codes.Xls, AttachmentTypeList.Codes.Xls, false, false, null, @"Because of the large number of lines (or ""rows"") this report is returning from the database, you have exceeded the Microsoft imposed limit (65,535 rows) on this Excel 97-2003 based report.
You can generate it using Excel 2007 XLSX format, which will raise the rows limit to 1,048,575.
Otherwise, try filtering or narrowing the size of the report so that it is below the Excel 97-2003 limit of 65,535 rows.

Do you want to generate this report with Excel 2007 XLSX format?");
			}
		}

		[SnailTest]
		public void TestTooManyRowsExceptionHandled_Hits2003Limit_XLS_HasNotifications_UserProceedsWithNewFormat()
		{
			using (DocumentsDataRegistry.Instance.AutomaticallySwitchToXLSXIfRequired.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.Yes);

				AssertTooManyRowsInDocumentExceptionHandled(nbBusinessObjectsToFail2003, AttachmentTypeList.Codes.Xls, AttachmentTypeList.Codes.Xlsx, AttachmentTypeList.Codes.Xlsx, true, true, null, @"Because of the large number of lines (or ""rows"") this report is returning from the database, you have exceeded the Microsoft imposed limit (65,535 rows) on this Excel 97-2003 based report.
You can generate it using Excel 2007 XLSX format, which will raise the rows limit to 1,048,575.
Otherwise, try filtering or narrowing the size of the report so that it is below the Excel 97-2003 limit of 65,535 rows.

Do you want to generate this report with Excel 2007 XLSX format?");
			}
		}

		[SnailTest]
		public void TestTooManyRowsExceptionHandled_Hits2007Limit_XLS_HasNotifications_UserProceedsWithNewFormat_StillTooBig()
		{
			using (DocumentsDataRegistry.Instance.AutomaticallySwitchToXLSXIfRequired.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.Yes);

				AssertTooManyRowsInDocumentExceptionHandled(nbBusinessObjectsToFail2007, AttachmentTypeList.Codes.Xls, AttachmentTypeList.Codes.Xlsx, AttachmentTypeList.Codes.Xlsx, false, true, @"Because of the large number of lines (or ""rows"") this report is returning from the database, you have exceeded the Microsoft imposed limit (1,048,575 rows) on this Excel 2007 based report.
Try filtering or narrowing the size of the report so that it is below the Excel 2007 limit of 1,048,575 rows.", null);
			}
		}

		[SnailTest]
		public void TestTooManyRowsExceptionHandled_Hits2003Limit_XLS_AutomaticallySwitchToXLSXIfRequired_NoNotifications()
		{
			using (DocumentsDataRegistry.Instance.AutomaticallySwitchToXLSXIfRequired.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertTooManyRowsInDocumentExceptionHandled(nbBusinessObjectsToFail2003, AttachmentTypeList.Codes.Xls, AttachmentTypeList.Codes.Xlsx, AttachmentTypeList.Codes.Xlsx, true, true, null, null);
			}
		}

		[GuiTest, SnailTest]
		public void TestTooManyRowsExceptionHandled_Hits2007Limit_XLS_AutomaticallySwitchToXLSXIfRequired_HasNotifications_StillTooBig()
		{
			using (DocumentsDataRegistry.Instance.AutomaticallySwitchToXLSXIfRequired.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertTooManyRowsInDocumentExceptionHandled(nbBusinessObjectsToFail2007, AttachmentTypeList.Codes.Xls, AttachmentTypeList.Codes.Xlsx, AttachmentTypeList.Codes.Xlsx, false, true, @"Because of the large number of lines (or ""rows"") this report is returning from the database, you have exceeded the Microsoft imposed limit (1,048,575 rows) on this Excel 2007 based report.
Try filtering or narrowing the size of the report so that it is below the Excel 2007 limit of 1,048,575 rows.", null);
			}
		}

		[SnailTest]
		public void TestTooManyRowsExceptionHandled_Hits2003Limit_XLSX_NoNotificationsWhenReaching2003RowsLimit()
		{
			AssertTooManyRowsInDocumentExceptionHandled(nbBusinessObjectsToFail2003, AttachmentTypeList.Codes.Xlsx, AttachmentTypeList.Codes.Xlsx, AttachmentTypeList.Codes.Xlsx, true, false, null, null);
		}

		[GuiTest, SnailTest]
		public void TestTooManyRowsExceptionHandled_Hits2007Limit_XLSX_HasNotifications()
		{
			AssertTooManyRowsInDocumentExceptionHandled(nbBusinessObjectsToFail2007, AttachmentTypeList.Codes.Xlsx, AttachmentTypeList.Codes.Xlsx, AttachmentTypeList.Codes.Xlsx, false, false, @"Because of the large number of lines (or ""rows"") this report is returning from the database, you have exceeded the Microsoft imposed limit (1,048,575 rows) on this Excel 2007 based report.
Try filtering or narrowing the size of the report so that it is below the Excel 2007 limit of 1,048,575 rows.", null);
		}

		[SnailTest]
		public void TestTooManyRowsExceptionHandled_Hits2003Limit_PDF_AutomaticallySwitchToXLSXIfRequired_NoNotifications()
		{
			//AutomaticallySwitchToXLSXIfRequired set to false, but should still switch for PDF
			using (DocumentsDataRegistry.Instance.AutomaticallySwitchToXLSXIfRequired.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AssertTooManyRowsInDocumentExceptionHandled(nbBusinessObjectsToFail2003, AttachmentTypeList.Codes.Pdf, AttachmentTypeList.Codes.Pdf, AttachmentTypeList.Codes.Xlsx, true, true, null, null);
			}
		}

		[GuiTest, SnailTest]
		public void TestTooManyRowsExceptionHandled_Hits2007Limit_PDF_AutomaticallySwitchToXLSXIfRequired_NoNotifications_StillTooBig()
		{
			//AutomaticallySwitchToXLSXIfRequired set to false, but should still switch for PDF
			using (DocumentsDataRegistry.Instance.AutomaticallySwitchToXLSXIfRequired.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AssertTooManyRowsInDocumentExceptionHandled(nbBusinessObjectsToFail2007, AttachmentTypeList.Codes.Pdf, AttachmentTypeList.Codes.Pdf, AttachmentTypeList.Codes.Xlsx, false, true, @"Because of the large number of lines (or ""rows"") this report is returning from the database, you have exceeded the Microsoft imposed limit (1,048,575 rows) on this Excel 2007 based report.
Try filtering or narrowing the size of the report so that it is below the Excel 2007 limit of 1,048,575 rows.", null);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestPrintCopyTypeIsInitialised()
		{
			var excelTemplate = new ExcelTemplateForUnitTesting("GroupByWithPageBreak.xls", TestFilesSubFolder.ReportTestFiles);
			var stmMenuItem = Factory.New<DocumentCommand>();
			var businessObject = Factory.New<DummyBusinessObject>();
			var dataProviderList = new DataProviderList(BODocDataProvider.Get(businessObject));
			dataProviderList.PrintCopyType = PrintCopyType.PRN;
			using (var pack = new DocumentPack(stmMenuItem))
			using (var report = new Report(pack, excelTemplate, dataProviderList, "TestReport", null, DocumentDirection.ANY, false))
			{
				Assert(report.PrintCopyType == PrintCopyType.PRN);
			}
		}

		[SnailTest]
		public void TestTooManyRowsExceptionHandled_Hits2003Limit_CSV_AutomaticallySwitchToXLSXIfRequired_NoNotifications()
		{
			//AutomaticallySwitchToXLSXIfRequired set to false, but should still switch for CSV
			using (DocumentsDataRegistry.Instance.AutomaticallySwitchToXLSXIfRequired.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AssertTooManyRowsInReportExceptionHandled(nbDummiesToFail2003, true, true, null);
			}
		}

		[GuiTest, SnailTest]
		public void TestTooManyRowsExceptionHandled_Hits2007Limit_CSV_AutomaticallySwitchToXLSXIfRequired_NoNotifications_StillTooBig()
		{
			//AutomaticallySwitchToXLSXIfRequired set to false, but should still switch for CSV
			using (DocumentsDataRegistry.Instance.AutomaticallySwitchToXLSXIfRequired.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AssertTooManyRowsInReportExceptionHandled(nbDummiesToFail2007, false, true, @"Because of the large number of lines (or ""rows"") this report is returning from the database, you have exceeded the Microsoft imposed limit (1,048,575 rows) on this Excel 2007 based report.
Try filtering or narrowing the size of the report so that it is below the Excel 2007 limit of 1,048,575 rows.");
			}
		}

		public void TestTooManyColumnsExceptionHandled_Hits2003Limit_XLS_HasNotifications_UserDoesNotProceedsWithNewFormat()
		{
			UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.No);

			using (DocumentsDataRegistry.Instance.AutomaticallySwitchToXLSXIfRequired.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AssertTooManyColumnsExceptionHandled(AttachmentTypeList.Codes.Xls, AttachmentTypeList.Codes.Xls, AttachmentTypeList.Codes.Xls, false, false, @"Because of the large number of columns this report is returning, you have exceeded the Microsoft imposed limit (256 columns) on this Excel 97-2003 based report.
You can generate it using Excel 2007 XLSX format, which will raise the columns limit to 16,384.

Do you want to generate this report with Excel 2007 XLSX format?");
			}
		}

		public void TestTooManyColumnsExceptionHandled_Hits2003Limit_XLS_HasNotifications_UserProceedsWithNewFormat()
		{
			using (DocumentsDataRegistry.Instance.AutomaticallySwitchToXLSXIfRequired.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.Yes);

				AssertTooManyColumnsExceptionHandled(AttachmentTypeList.Codes.Xls, AttachmentTypeList.Codes.Xlsx, AttachmentTypeList.Codes.Xlsx, true, true, @"Because of the large number of columns this report is returning, you have exceeded the Microsoft imposed limit (256 columns) on this Excel 97-2003 based report.
You can generate it using Excel 2007 XLSX format, which will raise the columns limit to 16,384.

Do you want to generate this report with Excel 2007 XLSX format?");
			}
		}

		public void TestTooManyColumnsExceptionHandled_Hits2003Limit_XLS_AutomaticallySwitchToXLSXIfRequired_NoNotifications()
		{
			using (DocumentsDataRegistry.Instance.AutomaticallySwitchToXLSXIfRequired.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertTooManyColumnsExceptionHandled(AttachmentTypeList.Codes.Xls, AttachmentTypeList.Codes.Xlsx, AttachmentTypeList.Codes.Xlsx, true, true, null);
			}
		}

		public void TestTooManyColumnsExceptionHandled_Hits2003Limit_XLSX_NoNotificationsWhenReaching2003ColumnsLimit()
		{
			AssertTooManyColumnsExceptionHandled(AttachmentTypeList.Codes.Xlsx, AttachmentTypeList.Codes.Xlsx, AttachmentTypeList.Codes.Xlsx, true, false, null);
		}

		public void TestTooManyColumnsExceptionHandled_Hits2003Limit_PDF_AutomaticallySwitchToXLSXIfRequired_NoNotifications()
		{
			//AutomaticallySwitchToXLSXIfRequired set to false, but should still switch for PDF
			using (DocumentsDataRegistry.Instance.AutomaticallySwitchToXLSXIfRequired.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AssertTooManyColumnsExceptionHandled(AttachmentTypeList.Codes.Pdf, AttachmentTypeList.Codes.Pdf, AttachmentTypeList.Codes.Xlsx, true, true, null);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(2006, 1, 1)]
		public void TestSavingReportsDoesNotShowAnyFormsWhenThereAreErrors()
		{
			using (new PrintTaskUIProviderForTestingSuppressor()) // Stop the suppression of notifications to simulate production environment.
			{
				var originalIsWeb = Globals.IsWeb;

				try
				{
					var initialFormConstructedCount = TestingState.FormConstructedCount;

					Globals.IsWeb = true;

					var excelTemplate = new ExcelTemplateForUnitTesting("Test.xls", TestFilesSubFolder.DocumentTestFiles);
					using (var report = new Report(Pack, excelTemplate))
					{
						report.Renderer = GetReportRendererThatThrowsDocumentEngineException();

						using (var outputStream = new MemoryStream())
						{
							using (Report.TemporarilyStopErrorsThrowingAnException())
							{
								AssertNoExceptionThrown(() => report.Save(outputStream));
							}
							AssertEquals("report.ErrorManager.HasErrors is true", true, report.ErrorManager.HasErrors);
						}
					}

					AssertEquals("No new forms should be shown.", 0, TestingState.FormConstructedCount - initialFormConstructedCount);
				}
				finally
				{
					Globals.IsWeb = originalIsWeb;
					ErrorReporter.Clear();
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestUnknownExceptionIsThrown()
		{
			var excelTemplate = new ExcelTemplateForUnitTesting("Test.xls", TestFilesSubFolder.DocumentTestFiles);
			using (var report = new Report(Pack, excelTemplate))
			{
				var renderer = new Mock<IReportRenderer>();

				renderer.Setup(m => m.Render()).Callback((() => { throw new IOException("Unknown IO Exception"); }));

				report.Renderer = renderer.Object;

				using (var outputStream = new MemoryStream())
				{
					((IReportForUnitTesting)report).GenerateRegardlessOfAnyErrors = true;
					Assert(!report.IsGenerated);
					AssertExceptionThrown(typeof(ReportProcessingException), () => report.Save(outputStream));
					Assert(!report.IsGenerated);
					AssertEquals("report.ErrorManager.HasErrors should be false", false, report.ErrorManager.HasErrors);
				}

				renderer.VerifyAll();
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestKnownExceptionIsNotThrown()
		{
			var excelTemplate = new ExcelTemplateForUnitTesting("Test.xls", TestFilesSubFolder.DocumentTestFiles);
			using (var report = new Report(Pack, excelTemplate))
			{
				report.Renderer = GetReportRendererThatThrowsDocumentEngineException();

				using (var outputStream = new MemoryStream())
				{
					((IReportForUnitTesting)report).GenerateRegardlessOfAnyErrors = true;
					AssertNoExceptionThrown(() => report.Save(outputStream));
					AssertEquals("report.ErrorManager.HasErrors is true", true, report.ErrorManager.HasErrors);
					AssertEquals("Report.Errors", @"Severity: [Fatal Error (without error report)] Message: [This is a test DocumentEngineException] Cell: [N/A] Sheetname: [(unknown)]",
										report.ErrorManager.ToString("Severity: [{0}] Message: [{1}] Cell: [{2}] Sheetname: [{3}]", false));
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestSQLExecutionExceptionIsRecordedToErrorIfNotSuspeneded()
		{
			var mockSuspender = new Mock<IReportErrorHandler>();
			mockSuspender.Setup(x => x.ShouldSkipReportError(Moq.It.IsAny<Exception>())).Returns((false, string.Empty));

			var excelTemplate = new ExcelTemplateForUnitTesting("Test.xls", TestFilesSubFolder.DocumentTestFiles);
			using (ObjectFactory.Substitute(mockSuspender.Object))
			using (var report = new Report(Pack, excelTemplate))
			{
				report.Renderer = GetReportRendererThatThrowsSQLExecutionException("TableName", "SQL Comment", "SQL ERROR Message");

				using (var outputStream = new MemoryStream())
				{
					((IReportForUnitTesting)report).GenerateRegardlessOfAnyErrors = true;
					AssertNoExceptionThrown(() => report.Save(outputStream));
					AssertEquals("report.ErrorManager.HasErrors is true", true, report.ErrorManager.HasErrors);
					AssertEquals("Report.Errors",
						"Severity: [Fatal Error (without error report)] Message: [Error loading table [TableName]. System.Exception: [SQL ERROR Message] occurred running SQL: [SQL Comment]. ] Cell: [N/A] Sheetname: [(unknown)]",
						report.ErrorManager.ToString()
					);
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestSQLExecutionExceptionIsNotRecordedToErrorIfSuspeneded()
		{
			var mockSuspender = new Mock<IReportErrorHandler>();
			mockSuspender.Setup(x => x.ShouldSkipReportError(Moq.It.IsAny<Exception>())).Returns((true, "DisplayMessage"));

			var excelTemplate = new ExcelTemplateForUnitTesting("Test.xls", TestFilesSubFolder.DocumentTestFiles);
			using (ObjectFactory.Substitute(mockSuspender.Object))
			using (var report = new Report(Pack, excelTemplate))
			{
				report.Renderer = GetReportRendererThatThrowsSQLExecutionException("TableName", "SQL Comment", "SQL ERROR Message");

				using (var outputStream = new MemoryStream())
				{
					((IReportForUnitTesting)report).GenerateRegardlessOfAnyErrors = true;
					AssertNoExceptionThrown(() => report.Save(outputStream));
					AssertEquals("report.ErrorManager.HasErrors is true since Error has warnning exception.", true, report.ErrorManager.HasErrors);
					AssertEquals("Report.Errors",
						"Severity: [Fatal Error (without error report)] Message: [DisplayMessage] Cell: [N/A] Sheetname: [(unknown)]",
						report.ErrorManager.ToString()
					);
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestReportExceptionInCustomNotifications()
		{
			var excelTemplate = new ExcelTemplateForUnitTesting("Test.xls", TestFilesSubFolder.DocumentTestFiles);
			var stmMenuItem = Factory.New<DocumentCommand>();
			using (var outputStream = new MemoryStream())
			using (var pack = new DocumentPack(stmMenuItem))
			using (var report = new Report(pack, excelTemplate))
			using (var printTask = new PrintTask())
			{
				printTask.Add(pack);
				pack.Add(report);
				printTask.CustomNotifications = new NotificationBuffer();
				report.Renderer = GetReportRendererThatThrowsDocumentEngineException();

				var deliveryInstructions = new DeliveryInstructions(pack);
				deliveryInstructions.IsDraft = false;
				deliveryInstructions.Destination = DeliveryInstructionDestination.TakenFromContact;

				using (Report.TemporarilyStopErrorsThrowingAnException())
				{
					AssertNoExceptionThrown(() => printTask.Run(deliveryInstructions));
				}

				var generatedReport = printTask[0][0] as Report;
				AssertEquals(false, generatedReport.IsGenerated);
				AssertEquals("report.ErrorManager.HasErrors", true, generatedReport.ErrorManager.HasErrors);
				var expectedErrorMessage = @"Severity: [Fatal Error (without error report)] Message: [This is a test DocumentEngineException] Cell: [N/A] Sheetname: [(unknown)]";
				AssertEquals("Report.Errors", expectedErrorMessage, report.ErrorManager.ToString("Severity: [{0}] Message: [{1}] Cell: [{2}] Sheetname: [{3}]", false));
				AssertEquals("Report.Errors", expectedErrorMessage, ((NotificationBuffer)(printTask.CustomNotifications)).AsString.Trim());
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestRenderXLSXDisabledReportWithTooManyColumns()
		{
			var excelTemplate = new ExcelTemplateForUnitTesting("DisableXLSXExportTemplate.xls", TestFilesSubFolder.ReportTestFiles);
			using (var report = new Report(Pack, excelTemplate))
			{
				report.PrepareForRender();
				var renderer = new Mock<IReportRenderer>();

				renderer.Setup(m => m.Render())
					.Callback(new Action(() => { throw new ExcelLimitationForThisFileFormatException(ExcelLimitationsHelper.LimitationType.Column, ""); }));

				report.Renderer = renderer.Object;

				using (var outputStream = new MemoryStream())
				{
					((IReportForUnitTesting)report).GenerateRegardlessOfAnyErrors = true;
					Assert(!report.IsGenerated);
					report.Save(outputStream);
					Assert(!report.IsGenerated);
					AssertEquals("report.ErrorManager.HasErrors is true", true, report.ErrorManager.HasErrors);
					AssertEquals("Report.Errors", @"Severity: [Fatal Error (without error report)] Message: [Because of the large number of columns Report 'TestTemplate' is returning, it has exceeded the Microsoft imposed limit (256 columns) on this Excel 97-2003 based report.] Cell: [N/A] Sheetname: [(unknown)]",
										report.ErrorManager.ToString("Severity: [{0}] Message: [{1}] Cell: [{2}] Sheetname: [{3}]", false));
				}

				renderer.VerifyAll();
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestRenderXLSXDisabledReportWithTooManyRows()
		{
			var excelTemplate = new ExcelTemplateForUnitTesting("DisableXLSXExportTemplate.xls", TestFilesSubFolder.ReportTestFiles);
			using (var report = new Report(Pack, excelTemplate))
			{
				report.PrepareForRender();
				var renderer = new Mock<IReportRenderer>();

				renderer.Setup(m => m.Render())
					.Callback(new Action(() => { throw new ExcelLimitationForThisFileFormatException(ExcelLimitationsHelper.LimitationType.Row, ""); }));
				report.Renderer = renderer.Object;

				using (var outputStream = new MemoryStream())
				{
					((IReportForUnitTesting)report).GenerateRegardlessOfAnyErrors = true;
					Assert(!report.IsGenerated);
					report.Save(outputStream);
					Assert(!report.IsGenerated);
					AssertEquals("report.ErrorManager.HasErrors is true", true, report.ErrorManager.HasErrors);
					AssertEquals("Report.Errors", @"Severity: [Fatal Error (without error report)] Message: [Because of the large number of lines (or ""rows"") Report 'TestTemplate' is returning from the database, it has exceeded the Microsoft imposed limit (65,535 rows) on this Excel 97-2003 based report.
Try filtering or narrowing the size of the report so that it is below the Excel 97-2003 limit of 65,535 rows.] Cell: [N/A] Sheetname: [(unknown)]",
										report.ErrorManager.ToString("Severity: [{0}] Message: [{1}] Cell: [{2}] Sheetname: [{3}]", false));
				}

				renderer.VerifyAll();
			}
		}

		public void TestReportSQLTimeoutExceptionIsPropertlyHandled()
		{
			using (Db.Connection.TemporarySetLockTimeout(DbConnection.LockTimeout.SqlDefault))
			using (var anotherConnection = Db.NewExtraConnectionToMainDb())
			{
				anotherConnection.BeginTransaction();

				try
				{
					var excelTemplate = DocumentEngineTestHelper.CreateExcelTemplateFromString(
						"Test", string.Empty,
						@"{A}-[#Config]
{A}-[Name=TestTemplate]
{A}-[Version=1.3]
{A}-[PageStyle=Portrait]
{A}-[SqlTimeout=30]
{A}-[Data:ReportData=Select top 1 * from dbo.RefUnloco]
{A}-[#SectionBody:Data=ReportData]
{B}-[<ReportData.Number>]
{A}-[#EndOfReport]");

					using (var report = new Report(Pack, excelTemplate))
					using (var outputStream = new MemoryStream())
					{
						report.TimeOut = new ZInt(1);
						anotherConnection.ExecuteNonQuery("ALTER TABLE dbo.RefUnloco ADD RL_DummyColumn int null;");

						((IReportForUnitTesting)report).GenerateRegardlessOfAnyErrors = true;
						AssertNoExceptionThrown(() => report.Save(outputStream));
						Assert(!report.IsGenerated);
						AssertEquals("report.ErrorManager.HasErrors is true", true, report.ErrorManager.HasErrors);
						AssertEquals("Report.Errors", @"Severity: [Fatal Error (without error report)] Message: [During the running of this report, the query timed out.
This could be because the server is very busy or the report is complex and runs on a large data set and needs a longer query timeout.

1) Try running this report at a time that the system is not busy, or
2) Try increasing the query timeout for reports from:
   i) Query Timeout Override field with current value : 1, see (Change Report Filters -> Query Timeout Override field where default value : 0)
   ii) SQL Timeout with current value : 30, see (Report template config area that may be set as ""SqlTimeout=600"" or may not be defined where default value : -1)
   iii) Report Command Timeout with current value : 900, see (Documents -> Report Command Timeout where default value : 900)
   iv) Report Preview Timeout with current value : 300, see (Documents -> Report Preview Timeout where default value : 300)] Cell: [N/A] Sheetname: [(unknown)]",
							report.ErrorManager.ToString("Severity: [{0}] Message: [{1}] Cell: [{2}] Sheetname: [{3}]", false));
					}
				}
				finally
				{
					anotherConnection.RollbackTransaction();
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestMetadataHasChangedExceptionIsProperlyHandled()
		{
			var excelTemplate = new ExcelTemplateForUnitTesting("Test.xls", TestFilesSubFolder.DocumentTestFiles);
			using (var report = new Report(Pack, excelTemplate))
			{
				var renderer = new Mock<IReportRenderer>();

				renderer.Setup(m => m.Render())
					.Callback((() => { throw new MetadataHasChangedException(); }));

				report.Renderer = renderer.Object;

				using (var outputStream = new MemoryStream())
				{
					((IReportForUnitTesting)report).GenerateRegardlessOfAnyErrors = true;
					Assert(!report.IsGenerated);
					AssertNoExceptionThrown(() => report.Save(outputStream));
					Assert(!report.IsGenerated);
					AssertEquals("report.ErrorManager.HasErrors is true", true, report.ErrorManager.HasErrors);
					AssertEquals("Report.Errors", @"Severity: [Fatal Error (without error report)] Message: [The report could not be generated due to a transient conflict with the Index Update Service.

Please try again in a few minutes.] Cell: [N/A] Sheetname: [(unknown)]",
						report.ErrorManager.ToString("Severity: [{0}] Message: [{1}] Cell: [{2}] Sheetname: [{3}]", false));
				}

				renderer.VerifyAll();
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestSqlLockLostExceptionWillBeThrown()
		{
			var excelTemplate = new ExcelTemplateForUnitTesting("Test.xls", TestFilesSubFolder.DocumentTestFiles);
			using (var report = new Report(Pack, excelTemplate))
			{
				var renderer = new Mock<IReportRenderer>();

				renderer.Setup(m => m.Render())
					.Callback((() => { throw new SqlLockLostException(); }));
				report.Renderer = renderer.Object;

				using (var outputStream = new MemoryStream())
				{
					AssertExceptionThrown(typeof(SqlLockLostException), () => report.Save(outputStream));
				}

				renderer.VerifyAll();
			}
		}

		[GuiTest]
		public void TestErrorsInSystemDefinedTemplateIsReportedViaErrorReporterToCargoWise()
		{
			var command = Factory.New<ReportCommand>();
			var pack = new DocumentPack(command);
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
					workSheet[4, 1] = "<Hide RowIf(\"AB CD !=\")>";
					workSheet[4, 2] = "<Z0_Code>";
					workSheet[5, 0] = "#EndOfReport";
					workSheet.SheetNameOverride = "Felix the Cat";
					workSheet.UpdateSheetName();
					creationExcelInterface.SaveToStream(templateStream);
				}
				var excelTemplate = new ExcelTemplateWrappingStream("TemplateFromStream", templateStream);
				using (var report = new Report(pack, excelTemplate))
				{
					report.MenuItem.SU_BusinessContext = "Custom";
					report.MenuItem.SU_MenuName = "Delivery Order";
					report.MenuItem.SU_MenuPath = "Misc/";
					report.MenuItem.SU_FilterList = "CTY=GB";
					report.MenuItem.SU_IsSystemDefined = true;
					report.MenuItem.SU_IsClientSpecific = true;

					report.StTemplate = Factory.New<StmTemplate>();
					report.StTemplate.SO_Name = "DA893";
					report.StTemplate.SO_IsSystemDefined = true;
					report.StTemplate.SO_IsClientSpecific = false;
					report.StTemplate.SO_DataContext = "Shipping";
					report.StTemplate.SO_ExcelTemplatePath = @"abc.xls";

					using (var outputStream = new MemoryStream())
					{
						try
						{
							((IReportForUnitTesting)report).StopErrorsThrowingAnException = true;
							AssertNoExceptionThrown(() => report.Save(outputStream));
							AssertEquals("report.ErrorManager.HasErrors is true", true, report.ErrorManager.HasErrors);
							AssertEquals("report.ErrorManager.IsWarningOnly is true", true, report.ErrorManager.HasWarningsOnly);

							var stringsWhichShouldBeInReportedMessage = new[]
							{
								string.Format(@"Report Information:

MenuItem:-
   BusinessContext = [Custom]
   Name with Path = [Misc/Delivery Order]
   Filter = [CTY=GB]
   PK = [{0}]
   IsSystemDefined = [Y]
   IsClientSpecific = [Y]

Template:-
   Name = [DA893]
   DataContext = [Shipping]
   ExcelFilePath = [abc.xls]
   PK = [{1}]
   IsSystemDefined = [Y]
   IsClientSpecific = [N]

Not Running from Scheduled Report.

Errors Found
---------------
Severity: [Warning (without error report)] Message: [Field <Z0_Code> not found on DataSource.] Cell: [C5] Sheetname: [Felix the Cat] Cell Content: [<Z0_Code>] Occurences: [1]
Enterprise.DocumentEngine.Exceptions.FieldNotFoundException: Field <Z0_Code> not found on DataSource.
   at Enterprise.DocumentEngine.Exceptions.FieldNotFoundException.ReportFieldNotFound(String fieldIdentifier, Object dataSource)",
										report.MenuItem.PK, report.StTemplate.PK),
								@"Severity: [Warning] Message: [Error in HideRowIf Macro: Result of ""AB CD !="" is not a True/False expression. Input macro: [<Hide RowIf(""AB CD !="")>]] Cell: [B5] Sheetname: [Felix the Cat] Cell Content: [<Hide RowIf(""AB CD !="")>] Occurences: [1]"
							};

							var reportedError = ErrorReporter.LastMessageReported.Trim();
							foreach (var expectedString in stringsWhichShouldBeInReportedMessage)
							{
								var message = string.Format("Error (below) should contain string vvvvvvvvvv\r\n{0}\r\n^^^^^^^^^^^^^^^^^^^^\r\n\r\nFull Error vvvvvvvvvvvvvvvvvvvv\r\n{1}\r\n^^^^^^^^^^^^^^^^^^^^", expectedString, reportedError);
								Assert(message, reportedError.Contains(expectedString));
							}
						}
						finally
						{
							ErrorReporter.Clear();
						}
					}
				}
			}
		}

		public void TestSourceFile()
		{
			var factory = new BusinessObjectFactory();
			var command = factory.New<ReportCommand>();
			var pack = new DocumentPack(command);

			using (var report = new Report(pack, new DummyExcelTemplate("ExcelTemplate Name", "ExcelTemplate Location")))
			{
				report.StTemplate = factory.New<StmTemplate>();
				report.StTemplate.SO_Name = "StmTemplate Name";
				report.StTemplate.SO_ExcelTemplatePath = "StmTemplate Path";
				AssertEquals("report.SourceFile", "StmTemplate Path", report.SourceFile);

				report.StTemplate.SO_ExcelTemplatePath = "";
				AssertEquals("report.SourceFile", "ExcelTemplate Location", report.SourceFile);
			}

			using (var report = new Report(pack, new DummyExcelTemplate("ExcelTemplate Name", "")))
			{
				report.StTemplate = factory.New<StmTemplate>();
				report.StTemplate.SO_Name = "StmTemplate Name";
				AssertEquals("report.SourceFile", @"Template Name: StmTemplate Name", report.SourceFile);

				report.StTemplate.SO_Name = "";
				AssertEquals("report.SourceFile", @"ExcelTemplate Name: ExcelTemplate Name", report.SourceFile);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestToString()
		{
			var factory = new BusinessObjectFactory();
			var command = factory.New<ReportCommand>();

			var excelTemplate = new ExcelTemplateForUnitTesting("Test.xls", TestFilesSubFolder.DocumentTestFiles);
			var pack = new DocumentPack(command);
			using (var report = new Report(pack, excelTemplate))
			{
				report.MenuItem.SU_BusinessContext = "Custom";
				report.MenuItem.SU_MenuName = "Delivery Order";
				report.MenuItem.SU_MenuPath = "Misc/";
				report.MenuItem.SU_FilterList = "CTY=GB";
				report.MenuItem.SU_IsSystemDefined = true;
				report.MenuItem.SU_IsClientSpecific = true;

				report.StTemplate = factory.New<StmTemplate>();
				report.StTemplate.SO_Name = "DA893";
				report.StTemplate.SO_IsSystemDefined = true;
				report.StTemplate.SO_IsClientSpecific = false;
				report.StTemplate.SO_DataContext = "Shippment";
				report.StTemplate.SO_ExcelTemplatePath = @"abc.xls";

				AssertMultilineASCIIEquals("report.ToString()", string.Format(@"Report Information:

MenuItem:-
   BusinessContext = [Custom]
   Name with Path = [Misc/Delivery Order]
   Filter = [CTY=GB]
   PK = [{0}]
   IsSystemDefined = [Y]
   IsClientSpecific = [Y]

Template:-
   Name = [DA893]
   DataContext = [Shippment]
   ExcelFilePath = [abc.xls]
   PK = [{1}]
   IsSystemDefined = [Y]
   IsClientSpecific = [N]

Not Running from Scheduled Report.", report.MenuItem.PK, report.StTemplate.PK), report.ToString());
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[GuiTest]
		public void TestErrorThrownGetsReportedToTheLocalErrorManagerInsteadOfSendingAnExceptionBackToUs()
		{
			var factory = new BusinessObjectFactory();
			var command = factory.New<ReportCommand>();
			var excelTemplate = new ExcelTemplateForUnitTesting("Test.xls", TestFilesSubFolder.DocumentTestFiles);
			var pack = new DocumentPack(command);

			var postMasterGroup = factory.Load<GlbGroup>(Core.Constants.Groups.PostMastersGroupPK);
			var postMaster = postMasterGroup.Staff.AddNew();
			postMaster.GS_EmailAddress = "postmaster@sample.org";
			postMaster.GS_Code = "_O_";
			postMaster.GS_LoginName = "postmastersample";
			factory.Save();

			using (var report = new Report(pack, excelTemplate))
			{
				report.Renderer = GetReportRendererThatThrowsDocumentEngineException();

				((IReportForUnitTesting)report).StopErrorsThrowingAnException = true;
				report.MenuItem.SU_BusinessContext = "Custom";
				report.MenuItem.SU_MenuName = "Delivery Order";
				report.MenuItem.SU_MenuPath = "Misc/";
				report.MenuItem.SU_FilterList = "CTY=GB";
				report.MenuItem.SU_IsSystemDefined = true;
				report.MenuItem.SU_IsClientSpecific = true;

				report.StTemplate = Factory.New<StmTemplate>();
				report.StTemplate.SO_Name = "DA893";
				report.StTemplate.SO_IsSystemDefined = false;
				report.StTemplate.SO_IsClientSpecific = false;
				report.StTemplate.SO_DataContext = "Shipping";
				report.StTemplate.SO_ExcelTemplatePath = @"abc.xls";
				report.Save(new MemoryStream());

				AssertNull("LastExceptionReported should be null", ErrorReporter.LastExceptionReported);
				ErrorReporter.Clear();

				AssertEquals("Emails to Client", 1, Env.OutgoingMailManager.EmailsCreated.Count);
				var email = Env.OutgoingMailManager.EmailsCreated[0];
				AssertEquals("email.Recipients", "postmaster@sample.org", email.Recipients.RecipientsAsDelimitedString());
				AssertEquals("Error Generating Report [unknown]", email.Subject);
				AssertMultilineASCIIEquals("email.Body", string.Format(@"Report Information:

MenuItem:-
   BusinessContext = [Custom]
   Name with Path = [Misc/Delivery Order]
   Filter = [CTY=GB]
   PK = [{0}]
   IsSystemDefined = [Y]
   IsClientSpecific = [Y]

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
Severity: [Fatal Error (without error report)] Message: [This is a test DocumentEngineException] Cell: [N/A] Sheetname: [(unknown)] Cell Content: [] Occurences: [1]
", report.MenuItem.PK, report.StTemplate.PK).Trim(), email.Body.Trim());
			}
			Env.OutgoingMailManager.EmailsCreated.Clear();
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestReportErrorManager()
		{
			var excelTemplate = new ExcelTemplateForUnitTesting("TextFilter.xls", TestFilesSubFolder.ReportTestFiles);
			using (var report = new Report(Pack, excelTemplate))
			{
				var errorManager = report.ErrorManager;
				AssertNotNull("report.ErrorManager", errorManager);
				AssertEquals("report.ErrorManager is lazy loaded then kept on the report, you get the same object every time", errorManager, report.ErrorManager);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestReportIsPreparedForRenderIsResetWhenDisposed()
		{
			var excelTemplate = new ExcelTemplateForUnitTesting("TextFilter.xls", TestFilesSubFolder.ReportTestFiles);
			Report report;

			using (report = new Report(Pack, excelTemplate))
			{
				report.PrepareForRender();
				Assert("Pre-condition: Report is prepared for render.", report.IsPreparedForRenderForTesting);
			}

			Assert("Report should be not prepared for render after being disposed.", !report.IsPreparedForRenderForTesting);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestColumnLayout()
		{
			var excelTemplate = new ExcelTemplateForUnitTesting("TextFilter.xls", TestFilesSubFolder.ReportTestFiles);
			using (var report = new Report(Pack, excelTemplate))
			{
				AssertEquals("Precondition", 0, report.ManagerLoadHitCountForTest);

				report.SelectedColumnLayout = "Default Configuration: See Configuration tab to modify";
				AssertEquals("Layout was not loaded on changing selected one as filters were empty", 0, report.ManagerLoadHitCountForTest);
				AssertEquals("Layout list is empty as filters are empty", 0, report.ColumnLayoutList.Count);
				AssertEquals("IWebReport.IsOnlyCompanyDefaultLayout", false, ((IWebReport)report).IsOnlyCompanyDefaultLayout);

				report.PrepareForRender();
				report.SelectedColumnLayout = "Default Configuration: See Configuration tab to modify";
				AssertEquals("Layout was not loaded because value has not changed", 0, report.ManagerLoadHitCountForTest);
				AssertEquals("Eagle Datamation International", report.ColumnLayoutList[0].Description);
				AssertEquals("IWebReport.IsOnlyCompanyDefaultLayout", true, ((IWebReport)report).IsOnlyCompanyDefaultLayout);
				AssertEquals("Layout was loaded on changing selected one by creating ColumnLayoutList", 1, report.ManagerLoadHitCountForTest);
				AssertEquals("Eagle Datamation International", report.SelectedColumnLayoutInfo.Value);
			}
		}

		public void TestFormatType()
		{
			using (var report = new Report(null, null))
			{
				AssertEquals(OrgConstants.AttachmentType.PDF, report.FormatType_List[0].Code);
				AssertEquals(OrgConstants.AttachmentType.PDFA, report.FormatType_List[1].Code);
				AssertEquals(OrgConstants.AttachmentType.XLS, report.FormatType_List[2].Code);
				AssertEquals(OrgConstants.AttachmentType.XLSX, report.FormatType_List[3].Code);
				AssertEquals(OrgConstants.AttachmentType.TIF, report.FormatType_List[4].Code);
				AssertEquals("PDF - Adobe PDF File", report.FormatType_List[0].Description);
				AssertEquals("PDF/A-2 - Adobe PDF File Archive", report.FormatType_List[1].Description);
				AssertEquals("XLS - Microsoft Excel Spreadsheet", report.FormatType_List[2].Description);
				AssertEquals("XLSX - Microsoft Excel 2007 Spreadsheet", report.FormatType_List[3].Description);
				AssertEquals("TIF - TIFF Image File", report.FormatType_List[4].Description);
				AssertEquals(OrgConstants.AttachmentType.PDF, report.SelectedFormatType);
				report.SelectedFormatType = OrgConstants.AttachmentType.XLS;
				AssertEquals(OrgConstants.AttachmentType.XLS, report.SelectedFormatTypeInfo.Value);
			}
		}

		[ExpectNoExceptions]
		public void TestIsTooManyRowsException_97_2003()
		{
			using (var excelInterface = new ExcelInterface())
			{
				excelInterface.NewExcelFile(1);
				excelInterface.WorkSheets[0][0, 0] = "blah";
				excelInterface.WorkSheets[0][65000, 0] = "blah";
				excelInterface.WorkSheets[0].DuplicateRows(0, 5000, 6000, 1);
			}
		}

		public void TestIsTooManyRowsException_2007()
		{
			using (var excelInterface = new ExcelInterface())
			{
				excelInterface.NewExcelFile(1);
				excelInterface.WorkSheets[0][0, 0] = "blah";
				excelInterface.WorkSheets[0][1048000, 0] = "blah";
				try
				{
					excelInterface.WorkSheets[0].DuplicateRows(0, 5000, 6000, 1);
					Fail("Must throw an exception on last line!");
				}
				catch (FlexCelXlsAdapterException exception)
				{
					AssertEquals(true, Report.IsTooManyRowsException(exception));
				}
			}
		}

		[ExpectNoExceptions]
		public void TestIsTooManyRowsException_IsNotThrownWith2007WhenReaching2003RowsLimit()
		{
			using (var excelInterface = new ExcelInterface())
			{
				excelInterface.NewExcelFile(1);
				excelInterface.WorkSheets[0][0, 0] = "blah";
				excelInterface.WorkSheets[0][65000, 0] = "blah";
				excelInterface.WorkSheets[0].DuplicateRows(0, 5000, 6000, 1);
			}
		}

		[GuiTest]
		public void TestMaxReportConnectionsExceededDisplaysCorrectMessage()
		{
			var mutex = new ReportMutex();
			var report1 = Report.NewForTesting(new DocumentPack());

			try
			{
				SystemDataRegistry.Instance.ReportMaxConnections.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 1);
				mutex.Lock(report1);

				TestCaseHelper.ClearTable(StmPrintJobSchema.Constants.TableName);

				var pack = new DocumentPack();

				using (var report2 = new Report(pack, EmptyAndValidTemplate, Guid.NewGuid(), DataContext.None))
				{
					var printTask = new PrintTaskTest.MockPrintTask();
					pack.Add(report2);
					printTask.Add(pack);

					var taskSettings = new PrintTaskSettings(printTask);
					taskSettings.Destination = DeliveryInstructionDestination.Preview;

					report2.IsScheduledReport = false;
					Assert(!report2.IsGenerated);
					using (Report.TemporarilyStopErrorsThrowingAnException())
					{
						printTask.Run(taskSettings);
					}
					var generatedReport = (Report)printTask[0][0];
					Assert(!generatedReport.IsGenerated);
					Assert(generatedReport.ErrorManager.HasErrors);
					var expectedMessage = "Maximum Concurrent Report Limit Reached.\r\n\r\nThe following reports are currently running...\r\n\r\n" + mutex.GetFormattedCurrentLocks() + "\r\nPlease try again in a few minutes.";
					AssertEquals("Report.Errors", string.Format(@"Severity: [Fatal Error (without error report)] Message: [{0}] Cell: [N/A] Sheetname: [(unknown)]", expectedMessage),
								generatedReport.ErrorManager.ToString("Severity: [{0}] Message: [{1}] Cell: [{2}] Sheetname: [{3}]", false));

					report2.IsScheduledReport = true;
					Assert(!report2.IsGenerated);
					using (Report.TemporarilyStopErrorsThrowingAnException())
					{
						AssertExceptionThrown<MaxConcurrentReportConnectionsExceeded>(() => printTask.Run(taskSettings));
					}
					generatedReport = (Report)printTask[0][0];
					Assert(!generatedReport.IsGenerated);
					Assert(!generatedReport.ErrorManager.HasErrors);
				}
			}
			finally
			{
				ErrorReporter.Clear();
				mutex.Unlock(report1);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestMacroTranslatorExcludesUnUsedUDFFields()
		{
			var list = new UserControlProviderList();
			AddFieldToList(list, "Alpha", "AlphaValue");
			AddFieldToList(list, "Beta", "BetaValue");
			AddFieldToList(list, "Mohsen", "PinesForKateMoss");
			AddFieldToList(list, "Charlie", "EatsCats");
			AddFieldToList(list, "Henry", "WearsADress");
			AddFieldToList(list, "Zubin", "LikesGreenUnderwear");
			AddFieldToList(list, "Brett", "HazzaBiggun");
			AddFieldToList(list, "Brendon", "LovesBob");

			var excelTemplate = new ExcelTemplateForUnitTesting("UDF with defaults.xls", TestFilesSubFolder.ReportTestFiles);
			using (var report = new Report(Pack, excelTemplate, new DocumentWrapperForTesting("Main"), "ss", list, DocumentDirection.ANY, false))
			{
				report.PrepareForRender();
				AssertEquals("AlphaValue", report.MacroTranslator.GetValue("<Alpha>", Passes.FirstPass));
				AssertEquals("BetaValue", report.MacroTranslator.GetValue("<Beta>", Passes.FirstPass));
				AssertEquals("PinesForKateMoss", report.MacroTranslator.GetValue("<Mohsen>", Passes.FirstPass));
				AssertEquals("EatsCats", report.MacroTranslator.GetValue("<Charlie>", Passes.FirstPass));
				AssertNull(report.MacroTranslator.GetValueProvider(Passes.FirstPass, "<Henry>"));
				AssertNull(report.MacroTranslator.GetValueProvider(Passes.FirstPass, "<Zubin>"));
				AssertNull(report.MacroTranslator.GetValueProvider(Passes.FirstPass, "<Brett>"));
				AssertNull(report.MacroTranslator.GetValueProvider(Passes.FirstPass, "<Brendon>"));
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestNoRepetitiveMacroInProviderCache()
		{
			var excelTemplate = new ExcelTemplateForUnitTesting("UDF with defaults.xls", TestFilesSubFolder.ReportTestFiles);
			using (var report = new Report(Pack, excelTemplate, new DocumentWrapperForTesting("Main"), "ss", new UserControlProviderList(), DocumentDirection.ANY, false))
			{
				report.PrepareForRender();
				report.Analyser.Analyse();

				foreach (var constant in report.TemplateDefinedConstants)
				{
					AssertEquals($"Duplicate macros with name {constant} have been detected in ProviderCache. Names should be unique.", 1, report.MacroTranslator.providerCache.Providers.Count(provider => provider.Regex.IsMatch($"<{constant.Key}>")));
				}
			}
		}

		public void TestTemplateDefinedConstantNamesDoNotConflictWithValueProviders()
		{
			var providers = new ValueProviderCollector().ValueProviders;
			var type = typeof(DocumentEngineIntegration.Constants.TemplateDefined);

			foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Static))
			{
				Assert($"Template constant {field.GetRawConstantValue()} is matching an existing ValueProvider. Names should be unique",
					!providers.Providers.Any(provider => provider.Regex.IsMatch($"<{field.GetRawConstantValue()}>")));
			}
		}

		public void TestCopies()
		{
			using (var report = Report.NewForTesting(Pack))
			{
				AssertEquals("Default copies", 1, report.PrinterDetails.NumberOfCopies);
				report.PrinterDetails.NumberOfCopies = 69;
				AssertEquals("69 copies", 69, report.PrinterDetails.NumberOfCopies);
			}
		}

		public void TestGetDeliveryInfo_WithEmailOverride()
		{
			var emailSenderAddress = "SomeFakeEmail@FakeMail.fake";
			using (var report = new Report(Pack, NewStyleTemplate))
			{
				report.MenuItem.SU_EmailSenderOverride = emailSenderAddress;

				var wrapper = new DummyDocWrapper();
				((IReportForUnitTesting)report).SetBusinessObjectForTesting(wrapper);
				report.IsDeliveredByEmail = true;
				var info = ((IDeliverable)report).GetDeliveryInfo(false);

				AssertEquals(emailSenderAddress, info.EmailFromAddress);
			}
		}

		public void TestDeliveryInfo_WithParentImplementingEDocsPluginHostDecider()
		{
			using (var report = new Report(Pack, NewStyleTemplate))
			{
				var refUNLOCO = Factory.NewWithValidTestData<MockRefUNLOCO>();
				var wrapper = Factory.NewWithValidTestData<MockDummyBizo>();
				wrapper.RefUNLOCO = refUNLOCO;

				((IReportForUnitTesting)report).SetBusinessObjectForTesting(BODocDataProvider.GetDefault(wrapper));
				var info = ((IDeliverable)report).GetDeliveryInfo(false);
				AssertEquals("Parent PK", refUNLOCO.PK, info.ParentGuid);
				AssertEquals("Parent Table Name", refUNLOCO.TableName, info.ParentTableName);
			}
		}

		public void TestGetDeliveryInfo_IsDeliveredByEmail()
		{
			var menuItem = Factory.New<StmMenuItem>();
			using (var pack = new DocumentPack(menuItem))
			using (var report = new Report(pack, NewStyleTemplate))
			{
				report.IsDeliveredByEmail = false;
				var info = ((IDeliverable)report).GetDeliveryInfo(false);
				AssertEquals(ZString.Empty, info.EmailFromAddress);
				AssertEquals(ZString.Empty, info.EmailSignature);
			}
		}

		public void TestValidEmailOverrideShowsWarning()
		{
			const string warningString = "In order to utilize this function please ensure that SMTP relay is enabled for this address/domain on your SMTP server";

			using (var report = new Report(Pack, NewStyleTemplate))
			{
				AssertNoWarning(report.MenuItem.SU_EmailSenderOverrideInfo, warningString);

				report.MenuItem.SU_EmailSenderOverride = "totes@legit.email";

				AssertHasWarning(report.MenuItem.SU_EmailSenderOverrideInfo, warningString);
			}
		}

		public void TestInvalidEmailOverride()
		{
			using (var report = new Report(Pack, NewStyleTemplate))
			{
				Assert("Precondition: empty/null email sender override should not be an error", !report.MenuItem.HasErrors);

				report.MenuItem.SU_EmailSenderOverride = "ImNotValid";

				AssertHasError(report.MenuItem.SU_EmailSenderOverrideInfo, "That is not a valid email address");
			}
		}

		public void TestGetDeliveryInfo()
		{
			using (var report = new Report(Pack, NewStyleTemplate))
			{
				report.PrinterDetails.NumberOfCopies = 42;
				report.SheetNames.Add(new SheetName { StrictName = "Sheet1", EntireName = "Sheet1 Value" });
				var wrapper = new DummyDocWrapper();
				((IReportForUnitTesting)report).SetBusinessObjectForTesting(wrapper);
				var info = ((IDeliverable)report).GetDeliveryInfo(false);

				AssertEquals("Info's email subject should match report email subject", report.EmailSubject, info.EmailSubjectLine);
				AssertEquals("Info's trailing space should match report trailing space", report.TrailingSpace, info.TrailingSpace);
				var bizO = report.BODocDataProvider.ParentBusinessObject;
				AssertEquals("Report has a DocWrapper, so Parent Guid should exist", bizO.PK, info.ParentGuid);
				AssertEquals("Report has a DocWrapper, so Parent table should exist", bizO.TableName, info.ParentTableName);
				AssertEquals("Report has a DocWrapper, so DocType should exist", report.DocTypeCode, info.DocumentType);
				Assert("Info should be report format", info.DeliveryFormat == DeliveryInfo.DeliveryFormats.Report);
				AssertEquals("Copies", (short)42, info.Copies);

				AssertEquals(1, info.SheetNames.Count);
				AssertEquals("Sheet1", info.SheetNames.First().StrictName);
				AssertEquals("Sheet1 Value", info.SheetNames.First().EntireName);

				var dummySupport = new DummyParentDocManagerSupport(Factory)
				{
					ParentGuidForTesting = ZGuid.NewZGuid(),
					ParentTableNameForTesting = "Z@"
				};
				report.Parent.ForceBusinessObjectToLogAgainst(dummySupport);
				info = ((IDeliverable)report).GetDeliveryInfo(false);
				AssertEquals("Report has a DocWrapper, so Parent Guid should exist", dummySupport.ParentGuidForTesting, info.ParentGuid);
				AssertEquals("Report has a DocWrapper, so Parent table should exist", "Z@", info.ParentTableName);
				AssertEquals("Report has a DocWrapper, so RelatedBusinessContext should exist", "ZZZ", info.RelatedBusinessContext);
			}
		}

		public void TestGetDeliveryInfo_Culture()
		{
			var menuItem = Factory.New<StmMenuItem>();
			var pack = new DocumentPack(menuItem);
			using (var report = new Report(pack, NewStyleTemplate))
			{
				menuItem.SU_IsLocalDocument = true;
				var info = ((IDeliverable)report).GetDeliveryInfo(false);
				AssertEquals(true, info.IsLocalDocument);

				menuItem.SU_IsLocalDocument = false;
				info = ((IDeliverable)report).GetDeliveryInfo(false);
				AssertEquals(false, info.IsLocalDocument);

				menuItem.RenderCulture = new System.Globalization.CultureInfo("en-US");
				info = ((IDeliverable)report).GetDeliveryInfo(false);
				AssertEquals(true, info.IsLocalDocument);

				menuItem.RenderCulture = ZArchitecture.Core.Culture.Default;
				info = ((IDeliverable)report).GetDeliveryInfo(false);
				AssertEquals(false, info.IsLocalDocument);
			}
		}

		public void TestGetDeliveryInfo_FlexCelLineSpacing()
		{
			var menuItem = Factory.New<StmMenuItem>();
			var pack = new DocumentPack(menuItem);

			using (var report = new Report(pack, NewStyleTemplate))
			{
				var info = ((IDeliverable)report).GetDeliveryInfo(false);
				AssertEquals(1m, info.LineSpacing);

				menuItem.SU_FlexCelLineSpacing = 2m;
				info = ((IDeliverable)report).GetDeliveryInfo(false);
				AssertEquals(2m, info.LineSpacing);
			}
		}

		public void TestGetSupportedDeliveryMethodsCore()
		{
			var menuItem = Factory.New<StmMenuItem>();
			var pack = new DocumentPack(menuItem);
			using (var report = new Report(pack, NewStyleTemplate))
			{
				report.PrintCopyType = PrintCopyType.ALL;
				AssertContainsExactElementsInAnyOrder(Core.Constants.ContactNotifyModes.All, report.GetSupportedDeliveryMethods());

				report.PrintCopyType = PrintCopyType.PRN;
				AssertContainsExactElementsInAnyOrder(new string[] { Core.Constants.ContactNotifyModes.Print, Core.Constants.ContactNotifyModes.EPrint }, report.GetSupportedDeliveryMethods());

				report.PrintCopyType = PrintCopyType.EML;
				AssertContainsExactElementsInAnyOrder(new string[] { Core.Constants.ContactNotifyModes.Email }, report.GetSupportedDeliveryMethods());

				report.PrintCopyType = PrintCopyType.FAX;
				AssertContainsExactElementsInAnyOrder(new string[] { Core.Constants.ContactNotifyModes.Fax }, report.GetSupportedDeliveryMethods());
			}
		}

		public void TestGetSupportedDeliveryMethodDispiteOfPrintCopyType()
		{
			var menuItem = Factory.New<StmMenuItem>();
			var pack = new DocumentPack(menuItem);
			using (var report = new Report(pack, NewStyleTemplate))
			{
				report.PrintCopyType = PrintCopyType.ALL;
				AssertContainsExactElementsInAnyOrder(Core.Constants.ContactNotifyModes.All, report.GetSupportedDeliveryMethodDespiteOfPrintCopyType());

				report.PrintCopyType = PrintCopyType.PRN;
				AssertContainsExactElementsInAnyOrder(Core.Constants.ContactNotifyModes.All, report.GetSupportedDeliveryMethodDespiteOfPrintCopyType());
			}
		}

		public void TestSupportsDeliveryMethod()
		{
			var menuItem = Factory.New<StmMenuItem>();
			var pack = new DocumentPack(menuItem);
			using (var report = new Report(pack, NewStyleTemplate))
			{
				report.PrintCopyType = PrintCopyType.ALL;
				Assert(report.SupportsDeliveryMethod(Core.Constants.ContactNotifyModes.Email));
				Assert(report.SupportsDeliveryMethod(Core.Constants.ContactNotifyModes.Electronic));
				Assert(report.SupportsDeliveryMethod(Core.Constants.ContactNotifyModes.EPrint));
				Assert(report.SupportsDeliveryMethod(Core.Constants.ContactNotifyModes.Fax));
				Assert(report.SupportsDeliveryMethod(Core.Constants.ContactNotifyModes.Print));
				Assert(report.SupportsDeliveryMethod(Core.Constants.ContactNotifyModes.Ftp));
				Assert(report.SupportsDeliveryMethod(Core.Constants.ContactNotifyModes.DoNotDeliver));

				report.PrintCopyType = PrintCopyType.PRN;
				Assert(!report.SupportsDeliveryMethod(Core.Constants.ContactNotifyModes.Email));
				Assert(!report.SupportsDeliveryMethod(Core.Constants.ContactNotifyModes.Electronic));
				Assert(report.SupportsDeliveryMethod(Core.Constants.ContactNotifyModes.EPrint));
				Assert(!report.SupportsDeliveryMethod(Core.Constants.ContactNotifyModes.Fax));
				Assert(report.SupportsDeliveryMethod(Core.Constants.ContactNotifyModes.Print));
				Assert(!report.SupportsDeliveryMethod(Core.Constants.ContactNotifyModes.Ftp));
				Assert(!report.SupportsDeliveryMethod(Core.Constants.ContactNotifyModes.DoNotDeliver));

				report.PrintCopyType = PrintCopyType.EML;
				Assert(report.SupportsDeliveryMethod(Core.Constants.ContactNotifyModes.Email));
				Assert(!report.SupportsDeliveryMethod(Core.Constants.ContactNotifyModes.Electronic));
				Assert(!report.SupportsDeliveryMethod(Core.Constants.ContactNotifyModes.EPrint));
				Assert(!report.SupportsDeliveryMethod(Core.Constants.ContactNotifyModes.Fax));
				Assert(!report.SupportsDeliveryMethod(Core.Constants.ContactNotifyModes.Print));
				Assert(!report.SupportsDeliveryMethod(Core.Constants.ContactNotifyModes.Ftp));
				Assert(!report.SupportsDeliveryMethod(Core.Constants.ContactNotifyModes.DoNotDeliver));

				report.PrintCopyType = PrintCopyType.FAX;
				Assert(!report.SupportsDeliveryMethod(Core.Constants.ContactNotifyModes.Email));
				Assert(!report.SupportsDeliveryMethod(Core.Constants.ContactNotifyModes.Electronic));
				Assert(!report.SupportsDeliveryMethod(Core.Constants.ContactNotifyModes.EPrint));
				Assert(report.SupportsDeliveryMethod(Core.Constants.ContactNotifyModes.Fax));
				Assert(!report.SupportsDeliveryMethod(Core.Constants.ContactNotifyModes.Print));
				Assert(!report.SupportsDeliveryMethod(Core.Constants.ContactNotifyModes.Ftp));
				Assert(!report.SupportsDeliveryMethod(Core.Constants.ContactNotifyModes.DoNotDeliver));
			}
		}

		public void TestGetDeliveryInfo_ShouldSetMenuItemPK()
		{
			var menuItem = Factory.New<StmMenuItem>();
			var pack = new DocumentPack(menuItem);
			using (var report = new Report(pack, NewStyleTemplate))
			{
				var guid = ZGuid.NewZGuid();
				report.SourcePivotPK = guid;
				var info = report.GetDeliveryInfo(false);
				AssertEquals(guid, info.ParentPivotPK);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestShouldNotThrowException_WhenFilterIllegalWithTwoContacts()
		{
			var excelTemplate = new ExcelTemplateForUnitTesting(BaseSourcePath +
				@"Enterprise\Product\Documents\ExcelTemplates\Reports\Job Billing - Charges Not Yet Posted as AR or AP.xls", TestFilesSubFolder.ReportTestFiles);

			using (Globals.SetIsUserInteractiveForTest(false))
			using (Report.TemporarilyStopErrorsThrowingAnException())
			using (var documentPack = new DocumentPack())
			using (var report = new Report(documentPack, excelTemplate))
			{
				var instructions = new DeliveryInstructions(documentPack);
				instructions.Destination = DeliveryInstructionDestination.TakenFromContact;
				instructions.Recipients.RemoveAndDeleteAll();
				instructions.IsDraft = false;

				var contact1 = instructions.Recipients.AddNew();
				contact1.DeliveryMethod = Enterprise.Core.Constants.ContactNotifyModes.Email;
				contact1.AttachmentType = AttachmentTypeList.Codes.Xls;
				contact1.Email = "unit.test@cargowise.com";
				contact1.Name = "Test1";

				var contact2 = instructions.Recipients.AddNew();
				contact2.DeliveryMethod = Enterprise.Core.Constants.ContactNotifyModes.Email;
				contact2.AttachmentType = AttachmentTypeList.Codes.Xls;
				contact2.Email = "123@123.com";
				contact2.Name = "Test2";

				report.UpdateAndSynchroniseFilters();

				var filterfield = report.FilterCollection["Job Opened Date"] as DateRangeField;
				AssertNotNull(string.Format("There is no filter called '{0}' in this report", "Job Opened Date"), filterfield);

				filterfield.ValueLow = ZDateTime.Empty;
				filterfield.ValueHigh = ZDateTime.Empty;

				report.OptionalTemplateSheetCollection["Sell Not Yet Posted - Detail"].Selected = true;
				report.OptionalTemplateSheetCollection["Costs Not Yet Posted - Detail"].Selected = false;
				documentPack.Add(report);
				var printTask = new PrintTask();
				printTask.Add(documentPack);

				AssertNoExceptionThrown(() =>
				{
					printTask.Run(instructions);
				});
			}
		}

		public void TestGetDeliveryInfoWithBusinessObjectToLogAgainstAlreadySet()
		{
			BusinessObject bizOToLogAgainst = Factory.LoadTop1<RefUNLOCO>(new ZQuery());
			Pack.ForceBusinessObjectToLogAgainst(bizOToLogAgainst);
			using (var report = new Report(Pack, NewStyleTemplate))
			{
				var wrapper = new DummyDocWrapper();
				((IReportForUnitTesting)report).SetBusinessObjectForTesting(wrapper);
				report.CustomWatermarkText = (NoResString)"customText";

				var info = ((IDeliverable)report).GetDeliveryInfo(false);
				AssertEquals("Info's email subject should match report email subject.", report.EmailSubject, info.EmailSubjectLine);
				AssertEquals("Info's trailing space should match report trailing space.", report.TrailingSpace, info.TrailingSpace);
				AssertEquals("Report has business object to log against, so Parent Guid should exist.", bizOToLogAgainst.PK, info.ParentGuid);
				AssertEquals("Report has a DocWrapper, so Parent table should exist.", bizOToLogAgainst.TableName, info.ParentTableName);
				AssertEquals("Report has a DocWrapper, so DocType should exist.", report.DocTypeCode, info.DocumentType);
				AssertEquals("Info should be report format.", DeliveryInfo.DeliveryFormats.Report, info.DeliveryFormat);
				AssertEquals("RelatedBusinessContext should not be empty.", true, info.RelatedBusinessContext.Length > 0);
				AssertEquals("RelatedBusinessContext", ((IDocManagerSupport)bizOToLogAgainst).DocManagerInfo.DocManagerCode, info.RelatedBusinessContext);
				AssertEquals("customText", info.CustomWatermarkText);
			}
		}

		public void TestGetDeliveryInfo_WithBusinessObjectPk()
		{
			// Arrange
			using (var report = new Report(Pack, NewStyleTemplate))
			{
				var docDataProvider = new DummyDocWrapper();
				((IReportForUnitTesting)report).SetBusinessObjectForTesting(docDataProvider);
				// Act
				var deliveryInfo = ((IDeliverable)report).GetDeliveryInfo(false);
				// Assert
				AssertEquals(report.BODocDataProvider.BusinessObjectToLogAgainst.PK, deliveryInfo.BusinessObjectPk);
			}
		}

		public void TestGetDeliveryInfoDoesNotSetRelatedBusinessObjectIfShouldNotRecordDocument()
		{
			var mockUnloco = Factory.New<MockRefUNLOCO>();
			Pack.ForceBusinessObjectToLogAgainst(mockUnloco);

			using (var report = new Report(Pack, NewStyleTemplate))
			{
				var wrapper = new DummyDocWrapper();
				((IReportForUnitTesting)report).SetBusinessObjectForTesting(wrapper);
				var info = ((IDeliverable)report).GetDeliveryInfo(false);
				AssertEquals("RelatedBusinessContext", "", info.RelatedBusinessContext);
			}
		}

		public void TestName()
		{
			using (var report = new Report(Pack, NewStyleTemplate))
			{
				report.PrepareForRender();
				AssertEquals("IDeliverable.Name is the report name", report.Name, ((IDeliverable)report).Name);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestFileExtension()
		{
			var contact = new DocDeliveryContact(new BusinessObjectFactory());
			contact.Name = "Dexter";
			contact.CompanyName = "Miami Metro Police";
			contact.Address1 = "addr 1";
			contact.Address2 = "addr 2";
			contact.City = "Miami";
			contact.PostCode = "33010";
			contact.State = "FL";
			contact.UNLOCO = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "USMIA"));
			contact.DeliveryMethod = Enterprise.Core.Constants.ContactNotifyModes.Email;

			using (var report = new Report(Pack, NewStyleTemplate))
			{
				((IReportForUnitTesting)report).DeliveryContact = contact;

				using (DocumentsDataRegistry.Instance.ExcelDefaultRenderingFormat.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ExcelFileFormatOptionList.Codes.XLS))
				{
					contact.AttachmentType = AttachmentTypeList.Codes.Xls;
					AssertEquals("IDeliverable.FileExtension is XLS", "XLS", ((IDeliverable)report).FileExtension);

					contact.AttachmentType = AttachmentTypeList.Codes.Xlsx;
					AssertEquals("IDeliverable.FileExtension is XLSX", "XLSX", ((IDeliverable)report).FileExtension);

					contact.AttachmentType = AttachmentTypeList.Codes.Pdf;
					AssertEquals("IDeliverable.FileExtension is XLS", "XLS", ((IDeliverable)report).FileExtension);
				}

				using (DocumentsDataRegistry.Instance.ExcelDefaultRenderingFormat.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ExcelFileFormatOptionList.Codes.XLSX))
				{
					contact.AttachmentType = AttachmentTypeList.Codes.Xls;
					AssertEquals("IDeliverable.FileExtension is XLS", "XLS", ((IDeliverable)report).FileExtension);

					contact.AttachmentType = AttachmentTypeList.Codes.Xlsx;
					AssertEquals("IDeliverable.FileExtension is XLSX", "XLSX", ((IDeliverable)report).FileExtension);

					contact.AttachmentType = AttachmentTypeList.Codes.Pdf;
					AssertEquals("IDeliverable.FileExtension is XLSX", "XLSX", ((IDeliverable)report).FileExtension);
				}
			}

			var excelTemplate = new ExcelTemplateForUnitTesting("DisableXLSXExportTemplate.xls", TestFilesSubFolder.ReportTestFiles);
			using (var report = new Report(Pack, excelTemplate))
			{
				report.PrepareForRender();
				((IReportForUnitTesting)report).DeliveryContact = contact;

				using (DocumentsDataRegistry.Instance.ExcelDefaultRenderingFormat.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ExcelFileFormatOptionList.Codes.XLSX))
				{
					contact.AttachmentType = AttachmentTypeList.Codes.Xls;
					AssertEquals("IDeliverable.FileExtension is always XLS", "XLS", ((IDeliverable)report).FileExtension);

					contact.AttachmentType = AttachmentTypeList.Codes.Xlsx;
					AssertEquals("IDeliverable.FileExtension is always XLS", "XLS", ((IDeliverable)report).FileExtension);

					contact.AttachmentType = AttachmentTypeList.Codes.Pdf;
					AssertEquals("IDeliverable.FileExtension is always XLS", "XLS", ((IDeliverable)report).FileExtension);
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestFileExtensionForTemplateHaveBothXlsxEnableAndDisableSheet()
		{
			var contact = new DocDeliveryContact(new BusinessObjectFactory());
			contact.Name = "Dexter";
			contact.CompanyName = "Miami Metro Police";
			contact.Address1 = "addr 1";
			contact.Address2 = "addr 2";
			contact.City = "Miami";
			contact.PostCode = "33010";
			contact.State = "FL";
			contact.UNLOCO = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "USMIA"));
			contact.DeliveryMethod = Core.Constants.ContactNotifyModes.Email;

			var excelTemplate = new ExcelTemplateForUnitTesting("TemplateHaveBothXlsxEnableAndDisableSheet.xls", TestFilesSubFolder.ReportTestFiles);
			using (var report = new Report(Pack, excelTemplate))
			{
				report.PrepareForRender();
				((IReportForUnitTesting)report).DeliveryContact = contact;

				using (DocumentsDataRegistry.Instance.ExcelDefaultRenderingFormat.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ExcelFileFormatOptionList.Codes.XLSX))
				using (var stream = new MemoryStream())
				{
					contact.AttachmentType = AttachmentTypeList.Codes.Xlsx;
					report.OptionalTemplateSheetCollection["DisableXlsx1"].Selected = false;
					report.OptionalTemplateSheetCollection["DisableXlsx2"].Selected = true;
					report.OptionalTemplateSheetCollection["EnableXlsx"].Selected = true;
					report.Save(stream);
					AssertEquals("IDeliverable.FileExtension is XLS if at least one optional sheet having disableXlsxExport config is selected", "XLS", ((IDeliverable)report).FileExtension);

					stream.SetLength(0);
					stream.Position = 0;
					report.OptionalTemplateSheetCollection["DisableXlsx1"].Selected = false;
					report.OptionalTemplateSheetCollection["DisableXlsx2"].Selected = false;
					report.OptionalTemplateSheetCollection["EnableXlsx"].Selected = true;
					report.Save(stream);
					AssertEquals("IDeliverable.FileExtension is XLSX only the optional sheet without disableXlsxExport config is selected", "XLSX", ((IDeliverable)report).FileExtension);
				}
			}
		}

		public void TestSelectedFormatType()
		{
			using (var report = new Report(Pack, NewStyleTemplate))
			{
				AssertEquals("SelectedFormatType is PDF by default", "PDF", report.SelectedFormatType);
				report.SelectedFormatType = OrgConstants.AttachmentType.TIF;
				AssertEquals(OrgConstants.AttachmentType.TIF, report.SelectedFormatTypeInfo.Value);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestInvalidSlowServer()
		{
			var excelTemplate = new ExcelTemplateForUnitTesting("SimpleTest with invalid slow server.xls", TestFilesSubFolder.ReportTestFiles);
			using (var report = new Report(Pack, excelTemplate))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				using (var stream = new MemoryStream())
				{
					((IReportForUnitTesting)report).GenerateRegardlessOfAnyErrors = true;
					report.Save(stream);
					AssertEquals("Report.Errors", "Severity: [Warning (without error report)] Message: [sql2005 is not a recognized slow server] Cell: [A2]",
											report.ErrorManager.ToString("Severity: [{0}] Message: [{1}] Cell: [{2}]", false));
				}
			}
		}

		public void TestOrientationsList()
		{
			using (var report = new Report(null, null))
			{
				AssertNotNull("report.OrientationList", report.OrientationList);
				AssertEquals("report.OrientationList.GetType()", typeof(ReportOrientationTypeList), report.OrientationList.GetType());
			}
		}

		public void TestOrientationManager()
		{
			using (var report = Report.NewForTesting(Pack))
			{
				AssertEquals("Default orientation builder", ReportOrientationTypeList.Codes.Default, report.OrientationManager.Value);
				report.OrientationManager.Value = "XXX";
				AssertEquals("report.Orientation builder", "XXX", report.OrientationManager.Value);
			}
		}

		public void TestOrientation()
		{
			using (var report = Report.NewForTesting(Pack))
			{
				AssertEquals("Default orientation", ReportOrientationTypeList.Codes.Default, report.Orientation);
				report.Orientation = "YYY";
				AssertEquals("report.Orientation", "YYY", report.Orientation);
			}
		}

		public void TestIsEdwDataSource()
		{
			using (var report = Report.NewForTesting(Pack))
			{
				AssertEquals("Default IsEdwDataSource", false, report.IsEdwDataSource);
				report.IsEdwDataSource = true;
				AssertEquals("report.IsEdwDataSource", true, report.IsEdwDataSource);
			}
		}

		public void TestTrySwitchToEdwConnection()
		{
			var testServerName = "TestDataWarehouseServer";
			using (SystemDataRegistry.Instance.BiDataWarehouseServer.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, testServerName))
			using (var report = Report.NewForTesting(Pack))
			{
				BiServers.ClearBiServersCache();
				AssertEquals("Default IsEdwDataSource", false, report.IsEdwDataSource);

				using (report.TrySwitchConnection())
				{
					AssertNotEquals(testServerName, report.RunningConnection.ServerName);
				}

				report.IsEdwDataSource = true;
				AssertEquals("report.IsEdwDataSource", true, report.IsEdwDataSource);

				using (report.TrySwitchConnection(true))
				{
					AssertEquals(testServerName, report.RunningConnection.ServerName);
				}

				report.IsEdwDataSource = false;
				using (report.TrySwitchConnection(true))
				{
					AssertNotEquals(testServerName, report.RunningConnection.ServerName);
				}

				BiServers.ClearBiServersCache();
			}
		}

		public void TestEdwConnection()
		{
			using (var report = Report.NewForTesting(Pack))
			{
				AssertEquals(Db.Connection.CurrentDatabase, report.RunningConnection.CurrentDatabase);
				AssertEquals(Db.Connection.ServerName, report.RunningConnection.ServerName);

				report.IsEdwDataSource = true;
				using (report.TrySwitchConnection(true))
				{
					using (BiServers.TemporarilySetDataWarehouseServerToNull())
					{
						AssertNull(report.RunningConnection);
					}

					var testServerName = "TestDataWarehouseServer";
					using (SystemDataRegistry.Instance.BiDataWarehouseServer.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, testServerName))
					{
						BiServers.ClearBiServersCache();
						AssertEquals(testServerName, report.RunningConnection.ServerName);
					}
				}

				AssertEquals(Db.Connection.ServerName, report.RunningConnection.ServerName);
				BiServers.ClearBiServersCache();
			}
		}

		public void TestIsDisposedProperty()
		{
			using (var report = new Report(Pack, EmptyAndValidTemplate, Guid.Empty, Enterprise.Core.Constants.DataContext.UnitTest))
			{
				AssertEquals(false, report.IsDisposed);

				report.IsDisposed = true;
				AssertEquals(true, report.IsDisposed);
			}
		}

		public void TestNoExceptionThrownIfIsDisposed()
		{
			using (var report = new Report(Pack, EmptyAndValidTemplate, Guid.Empty, Enterprise.Core.Constants.DataContext.UnitTest))
			{
				var reportRender = new ReportRenderer(report);

				report.IsDisposed = false;
				AssertExceptionThrown<NullReferenceException>(() => reportRender.RenderCore());

				report.IsDisposed = true;
				AssertNoExceptionThrown(() => reportRender.RenderCore());
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1136:DoNotUseSystemRuntimeSerializationFormattersBinary", Justification = "WI00700503 - Pending migration")]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestOrientationJsonConverter()
		{
			var excelTemplate = new ExcelTemplateForUnitTesting("Test.xls", TestFilesSubFolder.ReportTestFiles);
			using (var report = new Report(Pack, excelTemplate))
			{
				report.PrepareForRender();
				report.Orientation = "ZZZ";

				var result = JsonConverterHelper.Serialize(report);
				using (var deserialisedReport = JsonConverterHelper.Deserialize<Report>(result))
				{
					AssertEquals("deserializedReport.Orientation", "ZZZ", deserialisedReport.Orientation);
				}
			}
		}

		public void TestIsEdwDataSourceJsonConverter()
		{
			using (var report = new Report(Pack, NewStyleTemplate))
			{
				report.IsEdwDataSource = true;

				var result = JsonConverterHelper.Serialize(report);
				using (var deserialisedReport = JsonConverterHelper.Deserialize<Report>(result))
				{
					AssertEquals("IsEdwDataSource", true, deserialisedReport.IsEdwDataSource);
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestFilterFieldJsonConverter()
		{
			var excelTemplate = new ExcelTemplateForUnitTesting("TextFilter.xls", TestFilesSubFolder.ReportTestFiles);
			using (var report = new Report(Pack, excelTemplate))
			{
				report.PrepareForRender();
				var textField = (TextField)report.FilterCollection[1];
				textField.ValueAsStringForSerialisation = "My Test Field Value";

				var result = JsonConverterHelper.Serialize(report);

				using (Env.SetTemporaryUserContext("CWPostMaster", GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
				{
					using (var deserialisedReport = JsonConverterHelper.Deserialize<Report>(result))
					{
						AssertEquals(1, deserialisedReport.FilterCollection.Count);
						var newTxtFilter = (TextField)deserialisedReport.FilterCollection[0];
						AssertEquals("My Test Field Value", newTxtFilter.ValueAsStringForSerialisation);
						AssertEquals("XX_Field", newTxtFilter.FieldName);
					}
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGroupByJsonConverter()
		{
			var excelTemplate = new ExcelTemplateForUnitTesting("ThreeGroupBys.xls", TestFilesSubFolder.ReportTestFiles);
			using (var report = new Report(Pack, excelTemplate))
			{
				report.PrepareForRender();
				report.GroupByCollection[0].Selected = false;
				report.GroupByCollection[2].Selected = false;
				report.GroupByCollection.BreakPageOverride = true;

				var groupBy = report.GroupByCollection[1];
				groupBy.DisplayName = "New Display Name";
				groupBy.FieldList = "b,a,c";
				groupBy.Selected = true;

				AssertEquals("Count", 3, report.GroupByCollection.Count);
				AssertEquals("Display name", "Alpha", report.GroupByCollection[0].DisplayName);
				AssertEquals("Field list", "a,b,c", report.GroupByCollection[0].FieldList);
				AssertEquals("Display name", "New Display Name", report.GroupByCollection[1].DisplayName);
				AssertEquals("Field list", "b,a,c", report.GroupByCollection[1].FieldList);
				AssertEquals("Display name", "Gamma", report.GroupByCollection[2].DisplayName);
				AssertEquals("Field list", "c,a,b", report.GroupByCollection[2].FieldList);
				AssertEquals("Selected Group Field list", "b,a,c", report.GroupByCollection.SelectedGroupBy.FieldList);
				AssertEquals("Break page override", true, report.GroupByCollection.BreakPageOverride);

				var result = JsonConverterHelper.Serialize(report);
				using (var deserialisedReport = JsonConverterHelper.Deserialize<Report>(result))
				{
					AssertEquals("Selected Group Field list", "b,a,c", deserialisedReport.GroupByCollection.SelectedGroupBy.FieldList);
					AssertEquals("Selected Group Display name", "New Display Name", deserialisedReport.GroupByCollection.SelectedGroupBy.DisplayName);
					AssertEquals("Break page override", true, deserialisedReport.GroupByCollection.BreakPageOverride);
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGroupByJsonConverter_NoGroupBy()
		{
			var excelTemplate = new ExcelTemplateForUnitTesting("ThreeGroupBys.xls", TestFilesSubFolder.ReportTestFiles);
			using (var report = new Report(Pack, excelTemplate))
			{
				report.PrepareForRender();
				report.GroupByCollection.Clear();

				var result = JsonConverterHelper.Serialize(report);
				using (var deserialisedReport = JsonConverterHelper.Deserialize<Report>(result))
				{
					AssertEquals("Should be no group bys", 0, deserialisedReport.GroupByCollection.Count);
					AssertEquals("Should be 'no selected group'", new GroupByCollection().SelectedGroupBy.DisplayName, deserialisedReport.GroupByCollection.SelectedGroupBy.DisplayName);
					AssertEquals("Should be 'no selected group'", new GroupByCollection().SelectedGroupBy.FieldList, deserialisedReport.GroupByCollection.SelectedGroupBy.FieldList);
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestSortOrderJsonConverter()
		{
			var excelTemplate = new ExcelTemplateForUnitTesting("ThreeSortOrders.xls", TestFilesSubFolder.ReportTestFiles);
			using (var report = new Report(Pack, excelTemplate))
			{
				report.PrepareForRender();
				report.SortOrderCollection[0].Selected = false;
				report.SortOrderCollection[1].Selected = false;
				var sortOrder = report.SortOrderCollection[2];
				sortOrder.DisplayName = "New Display Name";
				sortOrder.FieldList = "c, b, a";
				sortOrder.Selected = true;

				AssertEquals("Count", 3, report.SortOrderCollection.Count);
				AssertEquals("Display name", "Alpha", report.SortOrderCollection[0].DisplayName);
				AssertEquals("Field list", "a, b, c", report.SortOrderCollection[0].FieldList);
				AssertEquals("Display name", "Beta", report.SortOrderCollection[1].DisplayName);
				AssertEquals("Field list", "b, c, a", report.SortOrderCollection[1].FieldList);
				AssertEquals("Display name", "New Display Name", report.SortOrderCollection[2].DisplayName);
				AssertEquals("Field list", "c, b, a", report.SortOrderCollection[2].FieldList);
				AssertEquals("Selected Sort Field list", "c, b, a", report.SortOrderCollection.SelectedOrder.FieldList);

				var result = JsonConverterHelper.Serialize(report);
				using (var deserialisedReport = JsonConverterHelper.Deserialize<Report>(result))
				{
					AssertEquals("Selected Sort Display name", "New Display Name", deserialisedReport.SortOrderCollection.SelectedOrder.DisplayName);
					AssertEquals("Selected Sort Field list", "c, b, a", deserialisedReport.SortOrderCollection.SelectedOrder.FieldList);
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestSortOrderJsonConverter_NoSortOrder()
		{
			var excelTemplate = new ExcelTemplateForUnitTesting("ThreeSortOrders.xls", TestFilesSubFolder.ReportTestFiles);
			using (var report = new Report(Pack, excelTemplate))
			{
				report.PrepareForRender();
				report.SortOrderCollection.Clear();

				var result = JsonConverterHelper.Serialize(report);
				using (var deserialisedReport = JsonConverterHelper.Deserialize<Report>(result))
				{
					AssertEquals("Should be no sort orders", 0, deserialisedReport.SortOrderCollection.Count);
					AssertEquals("Should be 'no selected sort'", new SortOrderCollection().SelectedOrder.DisplayName, deserialisedReport.SortOrderCollection.SelectedOrder.DisplayName);
					AssertEquals("Should be 'no selected sort'", new SortOrderCollection().SelectedOrder.FieldList, deserialisedReport.SortOrderCollection.SelectedOrder.FieldList);
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestOptionalTemplateSheetJsonConverter()
		{
			var excelTemplate = new ExcelTemplateForUnitTesting("MultipleTemplatesWithOptionalTemplates.xls", TestFilesSubFolder.ReportTestFiles);
			using (var report = new Report(Pack, excelTemplate))
			{
				report.PrepareForRender();
				report.OptionalTemplateSheetCollection["Sheet1"].Selected = false;
				var optionalTemplateSheet = report.OptionalTemplateSheetCollection["Sheet3"];
				optionalTemplateSheet.Selected = true;
				optionalTemplateSheet.DisplayName = "New Display Name";
				optionalTemplateSheet.FieldName = "New Field Name";

				var result = JsonConverterHelper.Serialize(report);
				using (var deserialisedReport = JsonConverterHelper.Deserialize<Report>(result))
				{
					AssertEquals("Count", 2, deserialisedReport.OptionalTemplateSheetCollection.Count);
					AssertEquals("Collection Should contain sheet1", true, deserialisedReport.OptionalTemplateSheetCollection.Contains("Sheet1"));
					AssertEquals("Collection Should contain sheet3", true, deserialisedReport.OptionalTemplateSheetCollection.Contains("Sheet3"));
					AssertEquals("Selected Sheet", true, deserialisedReport.OptionalTemplateSheetCollection["Sheet3"].Selected);
					AssertEquals("Sheet 3 Display Name", "New Display Name", deserialisedReport.OptionalTemplateSheetCollection["Sheet3"].DisplayName);
					AssertEquals("Sheet 3 Field Name", "New Field Name", deserialisedReport.OptionalTemplateSheetCollection["Sheet3"].FieldName);
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestOptionalTemplateSheetJsonConverterDeserialization()
		{
			var excelTemplate = new ExcelTemplateForUnitTesting("MultipleTemplatesWithOptionalAndNoMandatoryTemplates.xls", TestFilesSubFolder.ReportTestFiles);
			using (var report = new Report(Pack, excelTemplate))
			{
				report.PrepareForRender();
				report.OptionalTemplateSheetCollection["Sheet1"].Selected = true;

				var result = JsonConverterHelper.Serialize(report);
				using (var deserialisedReport = JsonConverterHelper.Deserialize<Report>(result))
				{
					using (var newReport = new Report(Pack, excelTemplate))
					{
						newReport.DeserializedReport = deserialisedReport;
						newReport.PrepareForRender();
						AssertEquals("Count", 2, newReport.OptionalTemplateSheetCollection.Count);
						newReport.RunPreSaveValidation();
						AssertEquals("Shouldn't have errors.", false, newReport.HasErrors);
					}
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestJsonConverterDesrializedReportShouldBeSetBeforeAnalysis()
		{
			var excelTemplate = new ExcelTemplateForUnitTesting("MultipleTemplatesWithOptionalAndNoMandatoryTemplates.xls", TestFilesSubFolder.ReportTestFiles);
			using (var report = new Report(Pack, excelTemplate))
			{
				report.PrepareForRender();
				report.OptionalTemplateSheetCollection["Sheet1"].Selected = true;

				var result = JsonConverterHelper.Serialize(report);
				using (var deserialisedReport = JsonConverterHelper.Deserialize<Report>(result))
				{
					using (var newReport = new Report(Pack, excelTemplate))
					{
						newReport.PrepareForRender();
						Assert("Report is analyzed", newReport.IsAnalyzed);
						newReport.DeserializedReport = deserialisedReport;
						AssertEquals("An error report should be sent", "Deserialized report shouldn't be changed once report is analyzed as we may have several macros which referenced the saved SortOrder/GroupBy option.", ErrorReporter.LastMessageReported);
						ErrorReporter.Clear();
					}
				}
			}
		}

		public void TestColumnHeadingManagerJsonConverter()
		{
			Report deserialisedReport;

			using (var report = new Report(Pack, NewStyleTemplate))
			{
				var newColumns = new ColumnHeading[]
					{
						new ColumnHeading("1", "1", "1", 1, 1, 1, true),
						new ColumnHeading("2", "2", "4", 2, 2, 2, false)
					};
				report.ColumnHeadingManager.CurrentConfiguration.Worksheets.Add(new Worksheet(new ColumnHeadingCollection(newColumns), "Sheet1"));

				var result = JsonConverterHelper.Serialize(report);
				deserialisedReport = JsonConverterHelper.Deserialize<Report>(result);
			}

			using (deserialisedReport)
			using (var report = new Report(Pack, NewStyleTemplate))
			{
				report.ColumnHeadingManager.DefaultTemplateConfigurationManager.AddHeading("Sheet1", new ColumnHeading("1", "1", "1", 1, 1, 1, true));
				report.ColumnHeadingManager.DefaultTemplateConfigurationManager.AddHeading("Sheet1", new ColumnHeading("2", "2", "2", 4, 4, 4, true));
				report.ColumnHeadingManager.DefaultTemplateConfigurationManager.AddHeading("Sheet1", new ColumnHeading("3", "3", "3", 3, 3, 3, false));

				report.DeserializedReport = deserialisedReport;
				report.PrepareForRender();

				AssertEquals("ColumnHeadingManager.Headings.Count", 3, report.ColumnHeadingManager.CurrentConfiguration.Worksheets["Sheet1"].ColumnHeadings.Count);
				ColumnHeadingTest.AssertPropertyValues(report.ColumnHeadingManager.CurrentConfiguration.Worksheets["Sheet1"].ColumnHeadings[0], "1", "1", "1", 1, 1, 1, true);
				ColumnHeadingTest.AssertPropertyValues(report.ColumnHeadingManager.CurrentConfiguration.Worksheets["Sheet1"].ColumnHeadings[1], "2", "2", "4", 4, 2, 2, false);
				ColumnHeadingTest.AssertPropertyValues(report.ColumnHeadingManager.CurrentConfiguration.Worksheets["Sheet1"].ColumnHeadings[2], "3", "3", "3", 3, 3, 3, true);
			}
		}

		public void TestReportWithoutOverrideReportDbOptionFieldDeserializationJson()
		{
			var jsonSerializedReport =
				@"
				{
				  ""ReportName"": """",
				  ""FilterCollection"": null,
				  ""OptionalTemplateSheetCollection"": null,
				  ""SelectedSortOrder"": null,
				  ""SelectedGroupBy"": null,
				  ""BreakPageOverride"": false,
				  ""ColumnHeadingManager"": {
					""Headings"": ""\u003C?xml version=\u00221.0\u0022 encoding=\u0022utf-16\u0022?\u003E\r\n\u003CReportColumnSettings xmlns:xsd=\u0022http://www.w3.org/2001/XMLSchema\u0022 xmlns:xsi=\u0022http://www.w3.org/2001/XMLSchema-instance\u0022\u003E\r\n  \u003CVersion\u003E0\u003C/Version\u003E\r\n\u003C/ReportColumnSettings\u003E""
				  },
				  ""PageOrientation"": ""DEF"",
				  ""TimeOut"": 0,
				  ""IsEdwDataSource"": false,
				  ""ShouldUpdateSchedulableFilters"": true
				}
				";

			using (var deserialisedReport = JsonConverterHelper.Deserialize<Report>(jsonSerializedReport))
			{
				AssertEquals("OverrideReportDbOption", false, deserialisedReport.OverrideReportDbOption);
			}
		}

		public void TestOverrideReportDbOptionJsonConverter()
		{
			using (var report = new Report(Pack, NewStyleTemplate))
			{
				report.OverrideReportDbOption = true;
				var result = JsonConverterHelper.Serialize(report);
				using (var deserialisedReport = JsonConverterHelper.Deserialize<Report>(result))
				{
					AssertEquals("OverrideReportDbOption", true, deserialisedReport.OverrideReportDbOption);
				}
			}
		}

		public void TestEmailSubjectSetFromBranding()
		{
			var bO = Factory.New<DummyBusinessObject>();
			var wrapper = new DocImageSupportWrapperTestClass(bO, Factory);

			var contact = Factory.New<OrgHeader>();
			contact.MiscServ.OM_FWAgentCategory = "FWD";
			Pack.Organisation = contact;
			SetDocumentBranding();

			using (var report = new Report(Pack, NewStyleTemplate, "ReportName", ContactType.FreightAgent, false))
			{
				((IReportForUnitTesting)report).SetBusinessObjectForTesting(wrapper);
				AssertEquals("Email Subject", "", report.EmailSubject);
				report.PrepareForRender();
				report.SetEmailSubject();
				AssertEquals("Email Subject", true, report.EmailSubject.IndexOf("BrandNameFromAgent".ToUpper()) > -1);
			}
		}

		public void TestGetDeliveryInfoSetsEmailSignatureAndFromAddress()
		{
			var bO = Factory.New<DummyBusinessObject>();
			var wrapper = new DocImageSupportWrapperTestClass(bO, Factory);

			var contact = Factory.New<OrgHeader>();
			contact.MiscServ.OM_FWAgentCategory = "FWD";
			Pack.Organisation = contact;
			SetDocumentBranding();

			using (var report = new Report(Pack, NewStyleTemplate, "ReportName", ContactType.FreightAgent, false))
			{
				((IReportForUnitTesting)report).SetBusinessObjectForTesting(wrapper);
				report.PrepareForRender();
				report.IsDeliveredByEmail = true;
				var info = ((IDeliverable)report).GetDeliveryInfo(false);
				AssertEquals("Info.EmailFromAddress", "agent@edi.com.au", info.EmailFromAddress);

				var formatter = new EmailFormatter(GlbStaff.CurrentUser);
				formatter.UpdateCompanyNameToBrandName(wrapper.BrandName);
				string expectedString = formatter.GetEmailSignature(DocumentsDataRegistry.Instance.EmailFormat.GetValueWithoutFallback(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty).EmailSignatureFields);
				AssertEquals("Info.EmailSignature", expectedString, info.EmailSignature);
			}
		}

		public void TestGetEmailFromAddressWithNoCurrentUser()
		{
			using (Env.SetTemporaryUserContext(Guid.Empty, Guid.Empty, Guid.Empty))
			using (var report = new Report(Pack, NewStyleTemplate))
			{
				var wrapper = new DummyFromEmailWrapperWithImageSupportAndEmail();
				((IReportForUnitTesting)report).SetBusinessObjectForTesting(wrapper);
				AssertEquals("From email address", "CargoWise <PleaseDoNotReply@cargowise.com>", report.GetEmailFromAddress());
			}
		}

		public void TestIsXLSFileCreated()
		{
			TestData.CreateJobTestTable();
			TestData.CreateHeaderTestTable();
			TestData.CreateLinesTestTable();
			TestData.CreateDocEngineTestTable();

			using (Report.TemporarilyUseMainConnection())
			using (var report = new Report(Pack, NewStyleTemplate, TestData.GetFirstGuidInTestTable(), Enterprise.Core.Constants.DataContext.UnitTest))
			{
				using (var outputStream = new MemoryStream())
				{
					((IReportForUnitTesting)report).GenerateRegardlessOfAnyErrors = true;
					report.Save(outputStream);
					Assert("Output XLS file should exist!", outputStream.Length > 0);
				}
			}
		}

		public void TestUpdateSheetNameWhenPreRendering()
		{
			var content = new Dictionary<string, string>();
			content.Add("Sheet1",
	@"{A}-[#Config]
{A}-[SheetNameOverride=SheetNameOverride1]
{A}-[#EndOfReport]
");

			content.Add("Sheet2",
				@"{A}-[#Config]
{A}-[SheetNameOverride=SheetNameOverride2]
{A}-[#EndOfReport]
");
			var excelTemplate = DocumentEngineTestHelper.CreateExcelTemplateFromString("Test", string.Empty, content);

			using (var documentPack = new DocumentPack())
			using (var report = new Report(documentPack, excelTemplate))
			{
				report.PrepareForRender();
				CombineAssertions(() =>
				{
					AssertEquals(2, report.SheetNames.Count);
					AssertEquals("Sheet1", report.SheetNames[0].StrictName);
					AssertEquals("SheetNameOverride1", report.SheetNames[0].EntireName);
					AssertEquals("Sheet2", report.SheetNames[1].StrictName);
					AssertEquals("SheetNameOverride2", report.SheetNames[1].EntireName);
				});
			}
		}

		public void TestSortSheetNameDoesNotMatch()
		{
			var sheetNameField = typeof(Report).GetField("SortSheetNameRegEx", BindingFlags.NonPublic | BindingFlags.Static);
			AssertNotNull("Couldn't find regex field", sheetNameField);
			var sheetName = (Regex)sheetNameField.GetValue(null);
			AssertNotNull("Couldn't get regex value", sheetName);

			AssertNoMatch(sheetName, "");
			AssertNoMatch(sheetName, " ");
			AssertNoMatch(sheetName, "sorting ord");
			AssertNoMatch(sheetName, "filter and sorting");
		}

		public void TestOptionalTemplateSheetNamesMatch()
		{
			var sheetNameField = typeof(Report).GetField("OptionalTemplateSheetNameRegEx", BindingFlags.NonPublic | BindingFlags.Static);
			AssertNotNull("Couldn't find regex field", sheetNameField);
			var sheetName = (Regex)sheetNameField.GetValue(null);
			AssertNotNull("Couldn't get regex value", sheetName);
			AssertMatch(sheetName, "Optional Templates");
			AssertMatch(sheetName, "optional templates");
			AssertMatch(sheetName, "Optional templates");
		}

		public void TestSortSheetNameMatches()
		{
			var sheetNameField = typeof(Report).GetField("SortSheetNameRegEx", BindingFlags.NonPublic | BindingFlags.Static);
			AssertNotNull("Couldn't find regex field", sheetNameField);
			var sheetName = (Regex)sheetNameField.GetValue(null);
			AssertNotNull("Couldn't get regex value", sheetName);

			AssertMatch(sheetName, "sort");
			AssertMatch(sheetName, "sorting");
			AssertMatch(sheetName, " sort");
			AssertMatch(sheetName, "sorting ");
			AssertMatch(sheetName, "sort order");
			AssertMatch(sheetName, "sort orders");
			AssertMatch(sheetName, "sorting orders");
			AssertMatch(sheetName, "sorting order");
			AssertMatch(sheetName, "  sOrTinG   oRDErS   ");
			AssertMatch(sheetName, "  sortorders   ");
			AssertMatch(sheetName, "sortingorder");
		}

		public void TestConstantsSheetNameDoesNotMatch()
		{
			var constantsSheetNameRegExField = typeof(Report).GetField("ConstantsSheetNameRegEx", BindingFlags.NonPublic | BindingFlags.Static);
			AssertNotNull("Couldn't find regex field", constantsSheetNameRegExField);
			var constantsSheetNameRegExName = (Regex)constantsSheetNameRegExField.GetValue(null);
			AssertNotNull("Couldn't get regex value", constantsSheetNameRegExName);

			AssertNoMatch(constantsSheetNameRegExName, "");
			AssertNoMatch(constantsSheetNameRegExName, " ");
			AssertNoMatch(constantsSheetNameRegExName, "const ants");
		}

		public void TestConstantsSheetNameMatches()
		{
			var constantsSheetNameRegExField = typeof(Report).GetField("ConstantsSheetNameRegEx", BindingFlags.NonPublic | BindingFlags.Static);
			AssertNotNull("Couldn't find regex field", constantsSheetNameRegExField);
			var constantsSheetNameRegExName = (Regex)constantsSheetNameRegExField.GetValue(null);
			AssertNotNull("Couldn't get regex value", constantsSheetNameRegExName);

			AssertMatch(constantsSheetNameRegExName, "Constant");
			AssertMatch(constantsSheetNameRegExName, "Constants");
			AssertMatch(constantsSheetNameRegExName, " COnstant");
			AssertMatch(constantsSheetNameRegExName, "CONSTANTS ");
		}

		public void TestFlexCelScaleSheetNameDoesNotMatch()
		{
			var flexCelScaleSheetNameRegExField = typeof(Report).GetField("FlexCelScaleSheetNameRegEx", BindingFlags.NonPublic | BindingFlags.Static);
			AssertNotNull("Couldn't find regex field", Report.FlexCelScaleSheetNameRegEx);
			var flexCelScaleSheetNameRegExName = (Regex)flexCelScaleSheetNameRegExField.GetValue(null);
			AssertNotNull("Couldn't get regex value", flexCelScaleSheetNameRegExName);

			AssertNoMatch(flexCelScaleSheetNameRegExName, " #FlexCelScale");
			AssertNoMatch(flexCelScaleSheetNameRegExName, "#FlexCelScale ");
			AssertNoMatch(flexCelScaleSheetNameRegExName, "#FlexCel Scale");
			AssertNoMatch(flexCelScaleSheetNameRegExName, "FlexCelScale");
		}

		public void TestFlexCelScaleSheetNameMatches()
		{
			var flexCelScaleSheetNameRegExField = typeof(Report).GetField("FlexCelScaleSheetNameRegEx", BindingFlags.NonPublic | BindingFlags.Static);
			AssertNotNull("Couldn't find regex field", Report.FlexCelScaleSheetNameRegEx);
			var flexCelScaleSheetNameRegExName = (Regex)flexCelScaleSheetNameRegExField.GetValue(null);
			AssertNotNull("Couldn't get regex value", flexCelScaleSheetNameRegExName);

			AssertMatch(flexCelScaleSheetNameRegExName, "#FlexCelScale");
			AssertMatch(flexCelScaleSheetNameRegExName, "#Flexcelscale");
			AssertMatch(flexCelScaleSheetNameRegExName, "#FLEXCELSCALE");
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[TestDate(1991, 01, 05, 11, 02, 03, 123)]
		public void TestEmailSubject()
		{
			TestData.CreateJobTestTable();
			TestData.CreateHeaderTestTable();
			TestData.CreateLinesTestTable();
			TestData.CreateDocEngineTestTable();

			var excelTemplate = new ExcelTemplateForUnitTesting("EmailSubject.xls", TestFilesSubFolder.ReportTestFiles);
			using (Report.TemporarilyUseMainConnection())
			using (var report = new Report(Pack, excelTemplate, TestData.GetFirstGuidInTestTable(), Enterprise.Core.Constants.DataContext.UnitTest))
			{
				using (var outputStream = new MemoryStream())
				{
					((IReportForUnitTesting)report).GenerateRegardlessOfAnyErrors = true;
					report.Save(outputStream);
				}
				AssertEquals(GlbCompany.CurrentCompany.GC_Name + " - " + GlbBranch.CurrentBranch.GB_BranchName + " - Total Pages : 4, Consignor: Consignor \"test company                 , Now: Saturday, 05 January 1991 11:02:03", report.EmailSubject);
			}
		}

		public void TestEmailSubjectWithDots()
		{
			using var templateStream = new MemoryStream();
			DocumentEngineTestHelper.GenerateTemplateStreamFromString(templateStream,
@"{A}-[#Config]
{A}-[Name=TestEmailSubjectMacros]
{A}-[EmailSubject=ETG.001]


{A}-[#EndOfReport]");

			var excelTemplate = new ExcelTemplateWrappingStream("Test", templateStream);

			using var report = new Report(new DocumentPack(), excelTemplate);
			report.PrepareForRender();

			using var outputStream = new MemoryStream();
			report.Save(outputStream);
			using var excelInterface = new ExcelInterface();
			excelInterface.LoadExcelFile(outputStream);
			AssertEquals($"{GlbCompany.CurrentCompany.GC_Name} - {GlbBranch.CurrentBranch.GB_BranchName} - ETG.001", report.EmailSubject);
		}

		public void TestEmailSubjectFromDocWrapper()
		{
			using (var report = new Report(Pack, EmptyAndValidTemplate, new DummyDocWrapper(), "Some Document", null, DocumentDirection.ANY, false))
			{
				using (var outputStream = new MemoryStream())
				{
					report.Save(outputStream);
				}
				AssertEquals(GlbCompany.CurrentCompany.GC_Name + " - " + GlbBranch.CurrentBranch.GB_BranchName + " - Some Document - My ID", report.EmailSubject);
			}
		}

		public void TestEmailSubjectOverrideFromStmMenuDocumentConfig()
		{
			TestData.CreateJobTestTable();
			TestData.CreateHeaderTestTable();
			TestData.CreateLinesTestTable();
			TestData.CreateDocEngineTestTable();

			using (Report.TemporarilyUseMainConnection())
			using (var report = new Report(Pack, EmptyAndValidTemplate, new DummyDocWrapper(), "Some Document", null, DocumentDirection.ANY, false))
			{
				report.StTemplate = Factory.New<StmTemplate>();
				var contact = new DocDeliveryContact(Factory);
				contact.OrgHeaderPK = Factory.New<OrgHeader>().PK;

				var config = Factory.New<StmMenuDocumentConfig>();
				var pivot = Factory.New<StmMenuTemplatePivot>();
				pivot.SI_SU = report.MenuItem.PK;
				pivot.SI_SO = report.StTemplate.PK;
				config.S3_SI = pivot.PK;
				config.S3_GC = Env.CurrentCompany.PK;
				config.S3_OH = contact.OrgHeaderPK;
				config.S3_OverrideEmailSubject = "New Subject";

				using (var outputStream = new MemoryStream())
				{
					report.Save(contact, contact, outputStream);
				}
				Assert(report.EmailSubject.EndsWith("New Subject"));
			}
		}

		public void TestEmailSubjectReportNameLocalization()
		{
			using (var mockRes = Res.GetLanguageInstance(Core.SharedConstants.Languages.ChineseSimplified).UseMockData())
			{
				mockRes.Put(DocBuilderResourceStrings.ReportNameKeyPrefix + "Some Document", new ResourceStringData(DocBuilderResourceStrings.ReportNameKeyPrefix + "Some Document", "有些文档"));
				Pack.Language = Core.SharedConstants.Languages.ChineseSimplified;
				using (var report = new Report(Pack, EmptyAndValidTemplate, new DummyDocWrapper(), "Some Document", null, DocumentDirection.ANY, false))
				{
					report.StTemplate = Factory.New<StmTemplate>();
					report.StTemplate.SO_Name = Core.Constants.SectionRepositoryTemplateNames.System;
					var contact = new DocDeliveryContact(Factory);
					contact.OrgHeaderPK = Factory.New<OrgHeader>().PK;

					var config = Factory.New<StmMenuDocumentConfig>();
					var pivot = Factory.New<StmMenuTemplatePivot>();
					pivot.SI_SU = report.MenuItem.PK;
					pivot.SI_SO = report.StTemplate.PK;
					config.S3_SI = pivot.PK;
					config.S3_GC = Env.CurrentCompany.PK;
					config.S3_OH = contact.OrgHeaderPK;
					config.S3_OverrideEmailSubject = "<ReportName>";

					using (var outputStream = new MemoryStream())
					{
						report.Save(contact, contact, outputStream);
					}
					AssertEndsWith("EmailSubject", "有些文档", report.EmailSubject);
				}
			}
		}

		public void TestCorrectPivotIsSelectedForSettingDocumentName()
		{
			TestData.CreateJobTestTable();
			TestData.CreateHeaderTestTable();
			TestData.CreateLinesTestTable();
			TestData.CreateDocEngineTestTable();

			var client = Factory.NewWithValidTestData<OrgHeader>();
			var contact = new DocDeliveryContact(Factory);
			contact.OrgHeaderPK = client.PK;

			var command = Factory.NewWithValidTestData<DocumentCommand>();
			var templateRecord = StmTemplateBase.GetDocBuilderTemplate(Factory, DocBuilderTemplateType.System);

			var pivot = Factory.New<StmMenuTemplatePivot>();
			pivot.SI_SO = templateRecord.PK;
			pivot.SI_SU = command.PK;

			var config = Factory.New<StmMenuDocumentConfig>();
			config.S3_SI = pivot.PK;
			config.S3_GC = Env.CurrentCompany.PK;
			config.S3_OH = contact.OrgHeaderPK;
			config.S3_OverrideEmailSubject = "New Subject";

			var pivot2 = Factory.New<StmMenuTemplatePivot>();
			pivot2.SI_SO = templateRecord.PK;
			pivot2.SI_SU = command.PK;

			var config2 = Factory.New<StmMenuDocumentConfig>();
			config2.S3_SI = pivot2.PK;
			config2.S3_GC = Env.CurrentCompany.PK;
			config2.S3_OH = contact.OrgHeaderPK;
			config2.S3_OverrideEmailSubject = "New Subject2";

			var docSupportedBO = Factory.New<DummyBODocSupportable>();
			docSupportedBO.Z0_Guid = client.PK;
			docSupportedBO.Z0_Code = "Test";

			Factory.Save();

			using (Report.TemporarilyUseMainConnection())
			using (var pack = new DocumentPack(command, docSupportedBO, null, null))
			using (var outputStream = new MemoryStream())
			{
				var report = pack[1] as Report;
				report.Save(contact, contact, outputStream);
				var info = report.GetDeliveryInfo(false);
				Assert(report.EmailSubject.EndsWith("New Subject2"));
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestIsXLSFileCreatedWithMacroReplacementInSQL()
		{
			TestData.CreateJobTestTable();
			TestData.CreateHeaderTestTable();
			TestData.CreateLinesTestTable();
			TestData.CreateDocEngineTestTable();

			var excelTemplate = new ExcelTemplateForUnitTesting("ReplaceMacrosInSQL.xls", TestFilesSubFolder.ReportTestFiles);
			using (Report.TemporarilyUseMainConnection())
			using (var report = new Report(Pack, excelTemplate, TestData.GetFirstGuidInTestTable(), Enterprise.Core.Constants.DataContext.UnitTest))
			{
				using (var outputStream = new MemoryStream())
				{
					((IReportForUnitTesting)report).GenerateRegardlessOfAnyErrors = true;
					report.Save(outputStream);
					Assert("Output XLS file should exist!", outputStream.Length > 0);
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestIsRepairEstimateXLSFileCreated()
		{
			TestData.CreateJobTestTable();
			TestData.CreateHeaderTestTable();
			TestData.CreateLinesTestTable();

			var excelTemplate = new ExcelTemplateForUnitTesting("RepairEstimate.xls", TestFilesSubFolder.ReportTestFiles);
			using (Report.TemporarilyUseMainConnection())
			using (var report = new Report(Pack, excelTemplate, Guid.Empty, Enterprise.Core.Constants.DataContext.UnitTest))
			{
				report.PrepareForRender();
				AssertEquals("Report.Errors", string.Concat("Severity: [Error (without error report)] Message: [Every Section in a template must contain a #SectionBody.\r\nWorkSheetCurrentlyBeingProcessed:\r\n", report.WorkSheetCurrentlyBeingProcessed.ToString(), "] Cell: [A7] Sheetname: [Sheet1]"),
										report.ErrorManager.ToString("Severity: [{0}] Message: [{1}] Cell: [{2}] Sheetname: [{3}]", false));
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestFilterCollectionIsReadOnConstruction()
		{
			var excelTemplate = new ExcelTemplateForUnitTesting("TextFilter.xls", TestFilesSubFolder.ReportTestFiles);
			using (var report = new Report(Pack, excelTemplate))
			{
				report.PrepareForRender();
				AssertEquals("Should read filters", 2, report.FilterCollection.Count);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestSortOrderCollectionIsReadOnConstruction()
		{
			var excelTemplate = new ExcelTemplateForUnitTesting("ThreeSortOrders.xls", TestFilesSubFolder.ReportTestFiles);
			using (var report = new Report(Pack, excelTemplate))
			{
				report.PrepareForRender();
				AssertEquals("Should be registered as editable child", true, report.IsRegisteredEditableChildObject(report.SortOrderCollection));
				AssertEquals("Should read sort orders", 3, report.SortOrderCollection.Count);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[ExpectException(typeof(FileNotFoundException))]
		public void TestConstructorWithFileNotFound()
		{
			var excelTemplate = new ExcelTemplateForUnitTesting("NonexistentFileName", TestFilesSubFolder.ReportTestFiles);
			using (var report = new Report(Pack, excelTemplate))
			{
			}
		}

		[ExpectNoExceptions()]
		public void TestConstructorWithGuid()
		{
			TestData.CreateJobTestTable();
			TestData.CreateHeaderTestTable();
			TestData.CreateLinesTestTable();
			TestData.CreateDocEngineTestTable();

			using (Report.TemporarilyUseMainConnection())
			using (var report = new Report(Pack, NewStyleTemplate, Guid.NewGuid(), Enterprise.Core.Constants.DataContext.UnitTest))
			{
				using (var outputStream = new MemoryStream())
				{
					((IReportForUnitTesting)report).GenerateRegardlessOfAnyErrors = true;
					report.Save(outputStream);
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestConstructorWithUserControlProviderList()
		{
			var list = new UserControlProviderList();
			AddFieldToList(list, "Field1", "Value1");
			AddFieldToList(list, "Charlie", "EatsCats");
			AddFieldToList(list, "Beta", "ThanYou");

			var excelTemplate = new ExcelTemplateForUnitTesting("UDF with defaults.xls", TestFilesSubFolder.ReportTestFiles);
			using (var report = new Report(Pack, excelTemplate, new DocumentWrapperForTesting("Main"), "ss", list, DocumentDirection.ANY, false))
			{
				report.PrepareForRender();
				AssertEquals("Field on BO Only", "Main", report.MacroTranslator.GetValue("<TestString>", Passes.FirstPass));
				AssertEquals("Field on BO and UDF", "EatsCats", report.MacroTranslator.GetValue("<Charlie>", Passes.FirstPass));
				AssertEquals("Field on UDF Only", "ThanYou", report.MacroTranslator.GetValue("<Beta>", Passes.FirstPass));
				AssertEquals("Field on Neither", null, report.MacroTranslator.GetValueProvider(Passes.FirstPass, "<DogBreath>"));
				AssertEquals("Report must be running on report server by default", false, report.OverrideReportDbOption);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestDisableFixedValueCacheOnUDF()
		{
			var list = new UserControlProviderList();
			var field = new TextField(Factory);
			field.DisplayName = "JohnSnow";
			field.DefaultExpression = "<CompanyCode>";
			list.Add(field);

			GlbCompany.CurrentCompany.GC_Code = "EDI";
			var excelTemplate = new ExcelTemplateForUnitTesting("UDF with DisableFixedValueCache.xls", TestFilesSubFolder.ReportTestFiles);
			using (var report = new Report(Pack, excelTemplate, new DocumentWrapperForTesting("Main"), "ss", list, DocumentDirection.ANY, false))
			{
				report.PrepareForRender();
				Assert("Should read DisableFixedValueCache setting from config area", report.DisableFixedValueCache);
				AssertEquals("Should return current company code", "EDI", report.MacroTranslator.GetValue("<JohnSnow>", Passes.FirstPass));
			}

			GlbCompany.CurrentCompany.GC_Code = "XXX";
			using (var report = new Report(Pack, excelTemplate, new DocumentWrapperForTesting("Main"), "ss", list, DocumentDirection.ANY, false))
			{
				report.PrepareForRender();
				Assert("Should read DisableFixedValueCache setting from config area", report.DisableFixedValueCache);
				AssertEquals("Should return new company code", "XXX", report.MacroTranslator.GetValue("<JohnSnow>", Passes.FirstPass));
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestFixedValueProviderOnUDFExtendedMacro()
		{
			var list = new UserControlProviderList();
			var field = new MultipleChoice(Factory)
			{
				DisplayName = "Some Option",
				ZValue = "Val 1",
				IsOverriddenInDocData = true
			};
			field.List.AddPair("Val 1", "Option 1");
			field.List.AddPair("Val 2", "Option 2");
			field.List.AddPair("Val 3", "Option 3");
			field.List.AddPair("Val 4", "Option 4");
			list.Add(field);

			var excelTemplate = new ExcelTemplateForUnitTesting("UDF with MultipleChoice.xls", TestFilesSubFolder.ReportTestFiles);
			using (var report = new Report(Pack, excelTemplate, new DocumentWrapperForTesting("Main"), "ss", list, DocumentDirection.ANY, false))
			{
				report.PrepareForRender();
				AssertEquals("Option 1", report.MacroTranslator.GetValue("<Some Option.Description>", Passes.FirstPass));
			}

			field.ZValue = "Val 2";
			using (var report = new Report(Pack, excelTemplate, new DocumentWrapperForTesting("Main"), "ss", list, DocumentDirection.ANY, false))
			{
				report.PrepareForRender();
				AssertEquals("Option 2", report.MacroTranslator.GetValue("<Some Option.Description>", Passes.FirstPass));
			}

			field.ValueProviders.Add(new DelegateValueProvider(field.DisplayName + ".DummyProperty", GetDummyProperty));
			using (var report = new Report(Pack, excelTemplate, new DocumentWrapperForTesting("Main"), "ss", list, DocumentDirection.ANY, false))
			{
				report.PrepareForRender();
				AssertMultilineASCIIEquals($@"An error occurs when getting <Some Option.DummyProperty> value for field {{Some Option}}.
Report Information:

MenuItem:-
   BusinessContext = []
   Name with Path = []
   Filter = []
   PK = [{report.MenuItem.PK}]
   IsSystemDefined = [N]
   IsClientSpecific = [N]

No StmTemplate Found on the Report.

Not Running from Scheduled Report.", ErrorReporter.LastMessageReported);
			}

			ErrorReporter.Clear();

			object GetDummyProperty(string macro, Report report)
			{
				throw new InvalidOperationException("Boooooom!");
			}
		}

		public void TestIsXLSFileCreatedTwice()
		{
			TestData.CreateJobTestTable();
			TestData.CreateHeaderTestTable();
			TestData.CreateLinesTestTable();
			TestData.CreateDocEngineTestTable();

			using (Report.TemporarilyUseMainConnection())
			using (var report = new Report(Pack, NewStyleTemplate, TestData.GetFirstGuidInTestTable(), Enterprise.Core.Constants.DataContext.UnitTest))
			{
				((IReportForUnitTesting)report).GenerateRegardlessOfAnyErrors = true;
				using (var outputStream = new MemoryStream())
				{
					report.Save(outputStream);
					CheckOutputFile(outputStream);
				}

				using (var outputStream = new MemoryStream())
				{
					report.Save(outputStream);
					CheckOutputFile(outputStream);
				}
			}
		}

		public void TestTrailingSpaceNoName()
		{
			using (IReportForUnitTesting report = Report.NewForTesting(Pack))
			{
				report.fName = null;
				AssertEquals("null", 0, report.TrailingSpace);

				report.fName = "";
				AssertEquals("empty string", 0, report.TrailingSpace);
			}
		}

		public void TestTrailingSpaceWhenDocumentIsNotAWB()
		{
			using (IReportForUnitTesting report = Report.NewForTesting(Pack))
			{
				Env.Registry.Freight.AirWaybill.HAWBPaperType = Core.Constants.AWB.PaperTypes.Traxon;
				Env.Registry.Freight.AirWaybill.MAWBPaperType = Core.Constants.AWB.PaperTypes.Traxon;

				report.fName = "";
				AssertEquals("", 0, report.TrailingSpace);

				report.fName = "A";
				AssertEquals("", 0, report.TrailingSpace);

				report.fName = "1";
				AssertEquals("", 0, report.TrailingSpace);

				report.fName = "2";
				AssertEquals("", 0, report.TrailingSpace);

				report.fName = "Some Other Name";
				AssertEquals("", 0, report.TrailingSpace);
			}
		}

		public void TestHAWBPage1TrailingSpace()
		{
			using (IReportForUnitTesting report = Report.NewForTesting(Pack))
			{
				report.fName = "Neutral HAWB1";

				Env.Registry.Freight.AirWaybill.HAWBPaperType = Core.Constants.AWB.PaperTypes.Iata;
				AssertEquals("IATA", 0, report.TrailingSpace);

				Env.Registry.Freight.AirWaybill.HAWBPaperType = Core.Constants.AWB.PaperTypes.IataOld;
				AssertEquals("IATA Old", 0, report.TrailingSpace);

				Env.Registry.Freight.AirWaybill.HAWBPaperType = Core.Constants.AWB.PaperTypes.Traxon;
				AssertEquals("Traxon", 24, report.TrailingSpace);

				Env.Registry.Freight.AirWaybill.HAWBPaperType = Core.Constants.AWB.PaperTypes.Letter;
				AssertEquals("Letter", 0, report.TrailingSpace);
			}
		}

		public void TestHAWBPage2TrailingSpace()
		{
			using (IReportForUnitTesting report = Report.NewForTesting(Pack))
			{
				report.fName = "Neutral HAWB2";

				Env.Registry.Freight.AirWaybill.HAWBPaperType = Core.Constants.AWB.PaperTypes.Iata;
				AssertEquals("IATA", 48, report.TrailingSpace);

				Env.Registry.Freight.AirWaybill.HAWBPaperType = Core.Constants.AWB.PaperTypes.IataOld;
				AssertEquals("IATA Old", 48, report.TrailingSpace);

				Env.Registry.Freight.AirWaybill.HAWBPaperType = Core.Constants.AWB.PaperTypes.Traxon;
				AssertEquals("Traxon", 24, report.TrailingSpace);

				Env.Registry.Freight.AirWaybill.HAWBPaperType = Core.Constants.AWB.PaperTypes.Letter;
				AssertEquals("Letter", 0, report.TrailingSpace);
			}
		}

		public void TestMAWBPage1TrailingSpace()
		{
			using (IReportForUnitTesting report = Report.NewForTesting(Pack))
			{
				report.fName = "Neutral MAWB1";

				Env.Registry.Freight.AirWaybill.MAWBPaperType = Core.Constants.AWB.PaperTypes.Iata;
				AssertEquals("IATA", 0, report.TrailingSpace);

				Env.Registry.Freight.AirWaybill.MAWBPaperType = Core.Constants.AWB.PaperTypes.IataOld;
				AssertEquals("IATA Old", 0, report.TrailingSpace);

				Env.Registry.Freight.AirWaybill.MAWBPaperType = Core.Constants.AWB.PaperTypes.Traxon;
				AssertEquals("Traxon", 24, report.TrailingSpace);

				Env.Registry.Freight.AirWaybill.MAWBPaperType = Core.Constants.AWB.PaperTypes.Letter;
				AssertEquals("Letter", 0, report.TrailingSpace);
			}

			using (IReportForUnitTesting report = Report.NewForTesting(Pack))
			{
				report.fName = "Carrier MAWB1";

				Env.Registry.Freight.AirWaybill.MAWBPaperType = Core.Constants.AWB.PaperTypes.Iata;
				AssertEquals("IATA", 0, report.TrailingSpace);

				Env.Registry.Freight.AirWaybill.MAWBPaperType = Core.Constants.AWB.PaperTypes.IataOld;
				AssertEquals("IATA Old", 0, report.TrailingSpace);

				Env.Registry.Freight.AirWaybill.MAWBPaperType = Core.Constants.AWB.PaperTypes.Traxon;
				AssertEquals("Traxon", 24, report.TrailingSpace);

				Env.Registry.Freight.AirWaybill.MAWBPaperType = Core.Constants.AWB.PaperTypes.Letter;
				AssertEquals("Letter", 0, report.TrailingSpace);
			}
		}

		public void TestMAWBPage2TrailingSpace()
		{
			using (IReportForUnitTesting report = Report.NewForTesting(Pack))
			{
				report.fName = "Neutral MAWB2";

				Env.Registry.Freight.AirWaybill.MAWBPaperType = Core.Constants.AWB.PaperTypes.Iata;
				AssertEquals("IATA", 48, report.TrailingSpace);

				Env.Registry.Freight.AirWaybill.MAWBPaperType = Core.Constants.AWB.PaperTypes.IataOld;
				AssertEquals("IATA Old", 48, report.TrailingSpace);

				Env.Registry.Freight.AirWaybill.MAWBPaperType = Core.Constants.AWB.PaperTypes.Traxon;
				AssertEquals("Traxon", 24, report.TrailingSpace);

				Env.Registry.Freight.AirWaybill.MAWBPaperType = Core.Constants.AWB.PaperTypes.Letter;
				AssertEquals("Letter", 0, report.TrailingSpace);
			}

			using (IReportForUnitTesting report = Report.NewForTesting(Pack))
			{
				report.fName = "Carrier MAWB2";

				Env.Registry.Freight.AirWaybill.MAWBPaperType = Core.Constants.AWB.PaperTypes.Iata;
				AssertEquals("IATA", 48, report.TrailingSpace);

				Env.Registry.Freight.AirWaybill.MAWBPaperType = Core.Constants.AWB.PaperTypes.IataOld;
				AssertEquals("IATA Old", 48, report.TrailingSpace);

				Env.Registry.Freight.AirWaybill.MAWBPaperType = Core.Constants.AWB.PaperTypes.Traxon;
				AssertEquals("Traxon", 24, report.TrailingSpace);

				Env.Registry.Freight.AirWaybill.MAWBPaperType = Core.Constants.AWB.PaperTypes.Letter;
				AssertEquals("Letter", 0, report.TrailingSpace);
			}
		}

		public void TestCMRDocumentTrailingSpace()
		{
			using (IReportForUnitTesting report = Report.NewForTesting(Pack))
			{
				report.fName = "CMR International Consignement Note";
				AssertEquals(24, report.TrailingSpace);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestReadUserDefinedFields()
		{
			var excelTemplate = new ExcelTemplateForUnitTesting("UserDefinedField - Date.xls", TestFilesSubFolder.ReportTestFiles);
			using (var report = new Report(Pack, excelTemplate, Guid.NewGuid(), Enterprise.Core.Constants.DataContext.UnitTest))
			{
				AssertEquals(0, report.UserDefinedFieldList.Count);
				report.PrepareForRender();
				report.ReadUserDefinedFields();
				AssertEquals(1, report.UserDefinedFieldList.Count);
				AssertEquals("Unit test date", report.UserDefinedFieldList[0].DisplayName);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestMultiSheet1()
		{
			TestData.CreateJobTestTable();
			TestData.CreateHeaderTestTable();
			TestData.CreateLinesTestTable();
			TestData.CreateDocEngineTestTable();

			var excelTemplate = new ExcelTemplateForUnitTesting("NewStyleMulti-Sheet.xls", TestFilesSubFolder.ReportTestFiles);
			using (Report.TemporarilyUseMainConnection())
			using (var tempFile = TempFile.NewWithExtension("XLS"))
			using (var report = new Report(Pack, excelTemplate, TestData.GetFirstGuidInTestTable(), Enterprise.Core.Constants.DataContext.UnitTest))
			using (var outputStream = new MemoryStream())
			{
				((IReportForUnitTesting)report).GenerateRegardlessOfAnyErrors = true;
				report.Save(outputStream);
				using (var excelInterface = new ExcelInterface())
				{
					excelInterface.LoadExcelFile(outputStream);
					AssertEquals("1 of 5", excelInterface.WorkSheets[0][3, 20]);
					AssertEquals("1 of 5", excelInterface.WorkSheets[1][3, 20]);
					excelInterface.SaveToFile(tempFile.Filename);
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGroupByCollection()
		{
			var excelTemplate = new ExcelTemplateForUnitTesting("GroupByPage.xls", TestFilesSubFolder.ReportTestFiles);
			using (var report = new Report(Pack, excelTemplate, Guid.Empty, Enterprise.Core.Constants.DataContext.UnitTest))
			{
				report.PrepareForRender();
				AssertEquals("Should be registered as editable child", true, report.IsRegisteredEditableChildObject(report.GroupByCollection));
				AssertEquals(4, report.GroupByCollection.Count);
				AssertEquals("Only RL_HasAirport", report.GroupByCollection[0].DisplayName);
				AssertEquals("Lines.RL_HasAirport", report.GroupByCollection[0].FieldList);
				AssertEquals(false, report.GroupByCollection[0].Selected);

				AssertEquals("All", report.GroupByCollection[1].DisplayName);
				AssertEquals("Lines.RL_HasAirport,Lines.RL_RN_NKCountryCode", report.GroupByCollection[1].FieldList);
				AssertEquals(false, report.GroupByCollection[1].Selected);

				AssertEquals("None", report.GroupByCollection[2].DisplayName);
				AssertEquals("", report.GroupByCollection[2].FieldList);
				AssertEquals(true, report.GroupByCollection[2].Selected);

				AssertEquals("Country", report.GroupByCollection[3].DisplayName);
				AssertEquals("Lines.RL_RN_NKCountryCode", report.GroupByCollection[3].FieldList);
				AssertEquals(false, report.GroupByCollection[3].Selected);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGroupBySheet()
		{
			var excelTemplate = new ExcelTemplateForUnitTesting("GroupByPage.xls", TestFilesSubFolder.ReportTestFiles);
			using (var report = new Report(Pack, excelTemplate, Guid.Empty, Enterprise.Core.Constants.DataContext.UnitTest))
			{
				AssertEquals("GroupBys", report.GroupBySheet.SheetName);
			}
		}

		public void TestGroupBy_IsTemplateSheet()
		{
			AssertEquals(true, Report.IsTemplateSheet("Sheet1"));
			AssertEquals(false, Report.IsTemplateSheet("GroupBys"));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestHidesGroupBySheet()
		{
			var excelTemplate = new ExcelTemplateForUnitTesting("GroupByPage.xls", TestFilesSubFolder.ReportTestFiles);
			using (var report = new Report(Pack, excelTemplate, Guid.Empty, Enterprise.Core.Constants.DataContext.UnitTest))
			{
				using (var outputStream = new MemoryStream())
				{
					report.Save(outputStream);
					using (var xlInterface = new ExcelInterface())
					{
						xlInterface.LoadExcelFile(outputStream);

						AssertEquals(2, xlInterface.Xls.SheetCount);

						xlInterface.Xls.ActiveSheet = 1;
						AssertEquals("Sheet1", xlInterface.Xls.SheetName);
						AssertEquals(FlexCel.Core.TXlsSheetVisible.Visible, xlInterface.Xls.SheetVisible);

						xlInterface.Xls.ActiveSheet = 2;
						AssertEquals("GroupBys", xlInterface.Xls.SheetName);
						AssertEquals(FlexCel.Core.TXlsSheetVisible.VeryHidden, xlInterface.Xls.SheetVisible);
					}
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestRenderableSheetsCollection()
		{
			var excelTemplate = new ExcelTemplateForUnitTesting("MultipleTemplatesWithOptionalTemplates.xls", TestFilesSubFolder.ReportTestFiles);
			using (var report = new Report(Pack, excelTemplate, Guid.Empty, Enterprise.Core.Constants.DataContext.UnitTest))
			{
				AssertEquals("There should be three renderable sheets", 3, report.TemplateSheets.Count);
				AssertEquals("Should contain sheet1", true, report.TemplateSheets.Contains("Sheet1"));
				AssertEquals("Should contain sheet2", true, report.TemplateSheets.Contains("Sheet2"));
				AssertEquals("Should contain sheet3", true, report.TemplateSheets.Contains("Sheet3"));
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestDoesntRenderUnselectedTemplates()
		{
			var excelTemplate = new ExcelTemplateForUnitTesting("MultipleTemplatesForOptonalRenderTest.xls", TestFilesSubFolder.ReportTestFiles);
			using (var report = new Report(Pack, excelTemplate, Guid.Empty, Enterprise.Core.Constants.DataContext.UnitTest))
			{
				report.PrepareForRender();
				AssertEquals("Optional sheets contains two items", 2, report.OptionalTemplateSheetCollection.Count);
				AssertEquals("Contains sheet1", true, report.OptionalTemplateSheetCollection.Contains("Sheet1"));
				AssertEquals("Contains sheet3", true, report.OptionalTemplateSheetCollection.Contains("Sheet3"));

				report.OptionalTemplateSheetCollection["Sheet1"].Selected = true;
				report.OptionalTemplateSheetCollection["Sheet3"].Selected = true;

				using (var outputStream = new MemoryStream())
				{
					report.Save(outputStream);
					using (var excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(outputStream);
						AssertEquals("Sheet was rendered if currentcompany macro was replaced", true, CurrentCompanyMacroReplaced("Sheet1", excelInterface.Xls, 1, 2));
						AssertEquals("Sheet was rendered if currentcompany macro was replaced", true, CurrentCompanyMacroReplaced("Sheet3", excelInterface.Xls, 1, 2));
					}
				}

				report.OptionalTemplateSheetCollection["Sheet1"].Selected = false;
				using (var outputStream = new MemoryStream())
				{
					report.Save(outputStream);
					using (var excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(outputStream);
						AssertEquals("Sheet was not rendered if currentcompany macro was not replaced", false, CurrentCompanyMacroReplaced("Sheet1", excelInterface.Xls, 1, 2));
						AssertEquals("Sheet was rendered if currentcompany macro was replaced", true, CurrentCompanyMacroReplaced("Sheet3", excelInterface.Xls, 1, 2));
					}
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestReportReAnalysedIfNeccessary()
		{
			var excelTemplate = new ExcelTemplateForUnitTesting("MultipleOptionalTemplatesWithDifferentDataSources.xls", TestFilesSubFolder.ReportTestFiles);
			using (var report = new Report(Pack, excelTemplate, Guid.Empty, Enterprise.Core.Constants.DataContext.UnitTest))
			{
				report.PrepareForRender();
				AssertEquals("Optional sheets contains two items", 2, report.OptionalTemplateSheetCollection.Count);
				AssertEquals("Contains sheet1", true, report.OptionalTemplateSheetCollection.Contains("Sheet1"));
				AssertEquals("Contains sheet2", true, report.OptionalTemplateSheetCollection.Contains("Sheet2"));

				report.OptionalTemplateSheetCollection["Sheet1"].Selected = false;
				report.OptionalTemplateSheetCollection["Sheet2"].Selected = true;

				using (var outputStream = new MemoryStream())
				{
					report.Save(outputStream);
					using (var excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(outputStream);
						AssertEquals("Report correctly reanalysed if Sheet2Data.Sheet2Field Correctly replaced", true, MacroReplaced("Sheet2", excelInterface.Xls, 1, 2, "Sheet2"));
					}
				}
			}
		}

		[TestDate(2012, 8, 15, 15, 29, 0)]
		public void TestCacheResultForConsequentPrintings()
		{
			using (var templateStream = new MemoryStream())
			{
				using (var creationExcelInterface = new ExcelInterface())
				{
					creationExcelInterface.NewExcelFile(1);
					var workSheet = creationExcelInterface.WorkSheets[0];
					workSheet[0, 0] = "#config";
					workSheet[1, 0] = "Name=TemplateFromStream";
					workSheet[2, 0] = "PageStyle=Portrait";
					workSheet[3, 0] = "#DocumentHeader";
					workSheet[4, 1] = "<Now>";
					workSheet[5, 0] = "#EndOfReport";
					creationExcelInterface.SaveToStream(templateStream);
				}

				var excelTemplate = new ExcelTemplateReadFromByteArray("TemplateFromStream", "", templateStream.ToArray());
				using (var pack = new DocumentPack(Factory.New<DocumentCommand>()))
				using (var report = new Report(pack, excelTemplate, BODocDataProvider.Get(Factory.New<DummyBusinessObject>()), excelTemplate.TemplateName, null, DocumentDirection.ANY, false))
				{
					report.IsSkipUnRegisterDisposable = true;
					TestDateAttribute.Date = new DateTime(2012, 08, 15, 15, 01, 00);
					AssertCacheResultForConsequentPrintings("Initial print", report, 1, 2, new DateTime(2012, 08, 15, 15, 01, 00));

					TestDateAttribute.Date = new DateTime(2012, 08, 15, 15, 03, 00);
					AssertCacheResultForConsequentPrintings("Return cached result", report, 1, 2, new DateTime(2012, 08, 15, 15, 01, 00));

					TestDateAttribute.Date = new DateTime(2012, 08, 15, 16, 02, 00);
					AssertCacheResultForConsequentPrintings("Refreshed due to outdated cache", report, 1, 2, new DateTime(2012, 08, 15, 16, 02, 00));
					report.IsSkipUnRegisterDisposable = false;
				}
			}
		}

		public void TestRegenerateForNewRecipient()
		{
			using (var templateStream = new MemoryStream())
			{
				using (var creationExcelInterface = new ExcelInterface())
				{
					creationExcelInterface.NewExcelFile(1);
					var workSheet = creationExcelInterface.WorkSheets[0];
					workSheet[0, 0] = "#config";
					workSheet[1, 0] = "Name=TemplateFromStream";
					workSheet[2, 0] = "PageStyle=Portrait";
					workSheet[3, 0] = "#DocumentHeader";
					workSheet[4, 1] = "<RecipientName>";
					workSheet[5, 0] = "#EndOfReport";
					creationExcelInterface.SaveToStream(templateStream);
				}

				var excelTemplate = new ExcelTemplateReadFromByteArray("TemplateFromStream", "", templateStream.ToArray());
				using (var pack = new DocumentPack(Factory.New<DocumentCommand>()))
				using (var report = new Report(pack, excelTemplate, BODocDataProvider.Get(Factory.New<DummyBusinessObject>()), excelTemplate.TemplateName, null, DocumentDirection.ANY, false))
				{
					AssertRegenerateForNewRecipient(report, "Abcdef", 1, 2);
					AssertRegenerateForNewRecipient(report, "Qwerty", 1, 2);
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestAtLeastOneOptionalTemplateSelectedValidation()
		{
			var excelTemplate1 = new ExcelTemplateForUnitTesting("MultipleTemplatesWithOptionalAndNoMandatoryTemplates.xls", TestFilesSubFolder.ReportTestFiles);
			using (var report = new Report(Pack, excelTemplate1, Guid.Empty, Enterprise.Core.Constants.DataContext.UnitTest))
			{
				report.PrepareForRender();
				AssertEquals("Optional sheets contains two items", 2, report.OptionalTemplateSheetCollection.Count);
				AssertEquals("Contains sheet1", true, report.OptionalTemplateSheetCollection.Contains("Sheet1"));
				AssertEquals("Contains sheet3", true, report.OptionalTemplateSheetCollection.Contains("Sheet3"));

				report.OptionalTemplateSheetCollection["Sheet1"].Selected = true;
				report.OptionalTemplateSheetCollection["Sheet3"].Selected = false;

				report.RunPreSaveValidation();
				AssertEquals("Report should not have an error", false, report.HasErrors);
			}

			var excelTemplate2 = new ExcelTemplateForUnitTesting("MultipleTemplatesWithOptionalAndNoMandatoryTemplates.xls", TestFilesSubFolder.ReportTestFiles);
			using (var report = new Report(Pack, excelTemplate2, Guid.Empty, Enterprise.Core.Constants.DataContext.UnitTest))
			{
				report.PrepareForRender();
				AssertEquals("Optional sheets contains two items", 2, report.OptionalTemplateSheetCollection.Count);
				AssertEquals("Contains sheet1", true, report.OptionalTemplateSheetCollection.Contains("Sheet1"));
				AssertEquals("Contains sheet3", true, report.OptionalTemplateSheetCollection.Contains("Sheet3"));

				report.OptionalTemplateSheetCollection["Sheet1"].Selected = false;
				report.OptionalTemplateSheetCollection["Sheet3"].Selected = false;

				report.RunPreSaveValidation();
				AssertEquals("Report should have an error", true, report.HasErrors);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestHidesAllUnSelectedOptionalSheets()
		{
			var excelTemplate = new ExcelTemplateForUnitTesting("MultipleTemplatesWithOptionalTemplates.xls", TestFilesSubFolder.ReportTestFiles);
			using (var report = new Report(Pack, excelTemplate, Guid.Empty, Enterprise.Core.Constants.DataContext.UnitTest))
			{
				report.PrepareForRender();
				AssertEquals("Optional sheets contains two items", 2, report.OptionalTemplateSheetCollection.Count);
				AssertEquals("Contains sheet1", true, report.OptionalTemplateSheetCollection.Contains("Sheet1"));
				AssertEquals("Contains sheet3", true, report.OptionalTemplateSheetCollection.Contains("Sheet3"));

				report.OptionalTemplateSheetCollection["Sheet1"].Selected = true;
				report.OptionalTemplateSheetCollection["Sheet3"].Selected = false;

				using (var outputStream = new MemoryStream())
				{
					report.Save(outputStream);
					using (var excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(outputStream);
						AssertEquals("Resulting file contains more than one sheet", true, excelInterface.Xls.SheetCount > 0);
						AssertEquals("Sheet1 is optional but selected so it's visible", true, SheetVisible("Sheet1", excelInterface.Xls));
						AssertEquals("Sheet2 is not optional so it's visible", true, SheetVisible("Sheet2", excelInterface.Xls));
						AssertEquals("Sheet3 is optional but it's not selected so it's invisible", false, SheetVisible("Sheet3", excelInterface.Xls));
					}
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestDependentFilterSynchronizedAfterCloning()
		{
			var excelTemplate = new ExcelTemplateForUnitTesting("DependentFilter.xls", TestFilesSubFolder.ReportTestFiles);

			var reportCommand = DocumentEngineTestHelper.CreateReportCommandWithExcelTemplate("Test Dependent Filter Report", excelTemplate, Factory);
			using (var pack = new DocumentPack(reportCommand))
			{
				var report = pack[0] as Report;
				AssertNotNull(report);

				report.PrepareForRender();
				AssertEquals(3, report.FilterCollection.Count);

				var multiChoiceFilter = report.FilterCollection[1] as MultipleChoice;
				AssertNotNull(multiChoiceFilter);
				multiChoiceFilter.ZValue = "I2";

				var codeListMultipleChoiceFilter = report.FilterCollection[2] as CodeListMultipleChoice;
				AssertNotNull(codeListMultipleChoiceFilter);
				codeListMultipleChoiceFilter.ZValue = "I2_2";

				var clonedPack = pack.Clone();
				var clonedReport = clonedPack[0] as Report;
				AssertNotNull(clonedReport);
				AssertEquals(3, clonedReport.FilterCollection.Count);

				var clonedMultiChoiceFilter = clonedReport.FilterCollection[1] as MultipleChoice;
				AssertNotNull(clonedMultiChoiceFilter);
				AssertEquals("I2", clonedMultiChoiceFilter.ZValue);

				var clonedCodeListMultipleChoice = clonedReport.FilterCollection[2] as CodeListMultipleChoice;
				AssertNotNull(clonedCodeListMultipleChoice);
				AssertEquals("I2_2", clonedCodeListMultipleChoice.ZValue);
			}
		}

		public void TestSetScheduleTask()
		{
			using (var report = (Report)Pack.AddNew())
			{
				var dummyFilter1 = new DummyFilterField(false, "x", Factory);
				var dummyFilter2 = new DummyFilterField(false, "y", Factory);
				var mockSchedulableFilter1 = new MockSchedulableFilterField();
				var mockSchedulableFilter2 = new MockSchedulableFilterField();

				report.FilterCollection.Add(dummyFilter1);
				report.FilterCollection.Add(mockSchedulableFilter1);
				report.UserDefinedFieldList = new UserControlProviderList();
				report.UserDefinedFieldList.Add(dummyFilter2);
				report.UserDefinedFieldList.Add(mockSchedulableFilter2);

				var scheduleTask = Factory.NewWithValidTestData<ReportScheduleTask>();
				report.SetScheduleTask(scheduleTask);
				AssertEquals("ScheduleTask", scheduleTask, report.ScheduleTask);
				AssertEquals("mockSchedulableFilter1.LastScheduleTask", scheduleTask, mockSchedulableFilter1.LastScheduleTask);
				AssertEquals("mockSchedulableFilter2.LastScheduleTask", scheduleTask, mockSchedulableFilter2.LastScheduleTask);
			}

			var command = Factory.Load<ReportCommand>(ReportScheduleTaskTest.TestReportPK);
			var pack = new DocumentPack(command);
			using (var report = (Report)pack[0])
			{
				var scheduleTask = Factory.NewWithValidTestData<ReportScheduleTask>();
				report.SetScheduleTask(scheduleTask);
				report.PrepareForRender();
				var foundDateFilter = false;
				foreach (var filter in report.FilterCollection)
				{
					var dateFilter = filter as DateRangeField;
					if (dateFilter != null)
					{
						foundDateFilter = true;
						AssertEquals("LowSchedule.ScheduleTask", scheduleTask, dateFilter.LowSchedule.ScheduleTask);
					}
				}
				AssertEquals("At least one DateRangeField should be found.", true, foundDateFilter);
			}
		}

		[ExpectException(typeof(ArgumentNullException))]
		public void TestSetScheduleTaskThrowsIfArgumentIsNull()
		{
			using (var report = (Report)Pack.AddNew())
			{
				report.SetScheduleTask(null);
			}
		}

		[ExpectExceptionMessage(typeof(InvalidOperationException), "SetScheduleTask cannot be called more than once.")]
		public void TestSetScheduleTaskCannotBeCalledMoreThanOnce()
		{
			using (var report = (Report)Pack.AddNew())
			{
				report.SetScheduleTask(Factory.NewWithValidTestData<ReportScheduleTask>());
				report.SetScheduleTask(Factory.NewWithValidTestData<ReportScheduleTask>());
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestPrepareForRenderUpdatesSchedulableFilters()
		{
			var scheduleTask = Factory.NewWithValidTestData<ReportScheduleTask>();
			var excelTemplate = new ExcelTemplateForUnitTesting("DateFilter.xls", TestFilesSubFolder.ReportTestFiles);
			using (var report = new Report(Pack, excelTemplate))
			{
				report.SetScheduleTask(scheduleTask);
				report.PrepareForRender();
				var dateField = (DateField)report.FilterCollection[1];
				AssertEquals("FilterCollection[0].ValueSchedule.ScheduleTask", scheduleTask, dateField.Schedule.ScheduleTask);
			}
		}

		[ExpectNoExceptions]
		public void TestPrepareForRenderTwiceDoesNotCrashWhenSynchronisingWithDeserialisedReport()
		{
			var command = Factory.Load<ReportCommand>(new ZGuid("CFD5875F-1F58-4E39-A775-AB5FEFF9D5CC"));
			var otherPack = new DocumentPack(command);

			using (var deserialisedReport = (Report)Pack.AddNew())
			using (var report = (Report)otherPack[0])
			{
				var deserialisedField = new DateTimeOffsetRangeField(Factory);
				deserialisedField.DisplayName = "Date Range";
				deserialisedField.FieldName = "DateTimeOffsetField";
				deserialisedField.ValueLow = ZDateTimeOffset.Now;
				deserialisedField.ValueHigh = ZDateTimeOffset.Now;
				deserialisedReport.FilterCollection.Add(deserialisedField);
				report.DeserializedReport = deserialisedReport;

				report.PrepareForRender();
				Assert(report.FilterCollection.Contains(deserialisedField));

				((IReportForUnitTesting)report).IsPreparedForRender = false;
				AssertEquals("deserialisedField.IsValid", true, deserialisedField.IsValid);
				report.PrepareForRender();
				Assert(report.FilterCollection.Contains(deserialisedField));
			}
		}

		public void TestMustRunOnline()
		{
			var command = Factory.Load<ReportCommand>(new ZGuid("CFD5875F-1F58-4E39-A775-AB5FEFF9D5CC"));
			var testPack = new DocumentPack(command);

			using (var deserialisedReport = (Report)Pack.AddNew())
			using (var report = (Report)testPack[0])
			{
				AssertEquals("Default value should be false", false, report.MustRunOnline);
				command.SU_MustRunOnline = true;
				AssertEquals("Should return value of ReportCommand", true, report.MustRunOnline);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestFirstRenderableSheetName()
		{
			var excelTemplate1 = new ExcelTemplateForUnitTesting("MultipleTemplatesWithOptionalTemplates.xls", TestFilesSubFolder.ReportTestFiles);
			using (var report = new Report(Pack, excelTemplate1, Guid.Empty, Enterprise.Core.Constants.DataContext.UnitTest))
			{
				report.PrepareForRender();
				AssertEquals("First renderable sheet is first sheet", "Sheet1", report.FirstRenderableSheetName);
			}

			var excelTemplate2 = new ExcelTemplateForUnitTesting("MultipleTemplatesFirstRenderableAfterNonRenderables.xls", TestFilesSubFolder.ReportTestFiles);
			using (var report = new Report(Pack, excelTemplate2, Guid.Empty, Enterprise.Core.Constants.DataContext.UnitTest))
			{
				report.PrepareForRender();
				AssertEquals("First renderable sheet is Sheet2 which is after the optional templates sheet.", "Sheet2", report.FirstRenderableSheetName);
			}
		}

		public void TestFilterCollectionRegisteredAsEditable()
		{
			using (var report = Report.NewForTesting(Pack))
			{
				AssertEquals("Collection should be registered on report", true, report.IsRegisteredEditableChildObject(report.FilterCollection));
				var oldFilters = report.FilterCollection;
				var filters = new CollectionOfIFilter();
				report.FilterCollection = filters;
				AssertEquals("New Collection should be registered on report", true, report.IsRegisteredEditableChildObject(filters));
				AssertEquals("Old Collection should not be registered on report", false, report.IsRegisteredEditableChildObject(oldFilters));
			}
		}

		public void TestRequiredValidationOrgNoOrgOrg()
		{
			using (var report = Report.NewForTesting(Pack))
			{
				var provider = CollectionAndModuleIDBuilder.GetCollectionAndModuleID(Factory, CollectionProviderTypeCodeDescriptionList.Codes.Organisation);
				var lookup = new MultipleSelectionLookup(Factory);
				lookup.DisplayName = "TestLookupField";
				lookup.SetCollectionProvider(provider);
				lookup.Validators.Add(new ValidatorPack().RequiredFilter);

				var orgCode = Factory.LoadTop1<OrgHeader>(new ZQuery()).OH_Code;

				report.FilterCollection.Add(lookup);

				lookup.ValueAsStringForSerialisation = orgCode;
				report.RunPreSaveValidation();
				AssertEquals(false, report.HasErrors);

				((BusinessObjectCollection)lookup.BindToList).RemoveAll();
				report.RunPreSaveValidation();
				AssertHasRowError("Should have error", lookup, "'TestLookupField' should have data.");
				AssertEquals(true, report.HasErrors);

				lookup.ValueAsStringForSerialisation = orgCode;
				report.RunPreSaveValidation();
				AssertNoErrors("Shouldn't have any errors", lookup);
				AssertEquals(false, report.HasErrors);
			}
		}

		public void TestRequiredValidationNoOrgOrg()
		{
			using (var report = Report.NewForTesting(Pack))
			{
				var provider = CollectionAndModuleIDBuilder.GetCollectionAndModuleID(Factory, CollectionProviderTypeCodeDescriptionList.Codes.Organisation);
				var lookup = new MultipleSelectionLookup(Factory);
				lookup.DisplayName = "TestLookupField";
				lookup.SetCollectionProvider(provider);
				lookup.Validators.Add(new ValidatorPack().RequiredFilter);

				var orgCode = Factory.LoadTop1<OrgHeader>(new ZQuery()).OH_Code;

				report.FilterCollection.Add(lookup);

				report.RunPreSaveValidation();
				AssertHasRowError("Should have error", lookup, "'TestLookupField' should have data.");
				AssertEquals(true, report.HasErrors);

				lookup.ValueAsStringForSerialisation = orgCode;
				report.RunPreSaveValidation();
				AssertNoErrors("Shouldn't have any errors", lookup);
				AssertEquals(false, report.HasErrors);
			}
		}

		public void TestOptionalTemplateCollectionRegisteredAsEditable()
		{
			using (var report = Report.NewForTesting(Pack))
			{
				AssertEquals("Collection should be registered on report", true, report.IsRegisteredEditableChildObject(report.OptionalTemplateSheetCollection));
			}
		}

		public void TestUserDefinedFieldListRegisteredAsEditable()
		{
			using (var report = Report.NewForTesting(Pack))
			{
				var firstList = new UserControlProviderList();
				report.UserDefinedFieldList = firstList;
				AssertEquals("First list be registered on report", true, report.IsRegisteredEditableChildObject(firstList));

				var secondList = new UserControlProviderList();
				report.UserDefinedFieldList = secondList;
				AssertEquals("First List should be deregistered on report", false, report.IsRegisteredEditableChildObject(firstList));
				AssertEquals("Second List should be registered on report", true, report.IsRegisteredEditableChildObject(secondList));
			}
		}

		public void TestUpdateFilterCollectionOnReportFromDeserializedReport()
		{
			using (var deserialisedReport = Report.NewForTesting(Pack))
			using (var report = Report.NewForTesting(Pack))
			{
				report.DeserializedReport = deserialisedReport;

				var codeLookupField = new CodeLookupField(Factory);
				var codeListMulipleChoiceField = new CodeListMultipleChoice(Factory);
				var lookupField = new LookupField(Factory);
				var dateField1 = new DateField(Factory);
				var dateField2 = new DateField(Factory);
				var dateField3 = new DateField(Factory);
				var multiSelectField = new MultipleSelectionLookup(Factory);
				var columnConfigField = new ColumnConfigurationField(Factory);
				var optionsGroupField = new OptionGroup(Factory);
				var sendingAgentLookup = new MultipleSelectionLookup(Factory);
				var receivingAgentLookup = new MultipleSelectionLookup(Factory);

				codeLookupField.DisplayName = "1";
				codeListMulipleChoiceField.DisplayName = "2";
				lookupField.DisplayName = "Link";
				dateField1.DisplayName = "4";
				dateField2.DisplayName = "5";
				dateField3.DisplayName = "6";
				multiSelectField.DisplayName = "7";
				columnConfigField.DisplayName = "Columns";
				var columnConfigFieldManager = new ColumnConfigurationsManager(ZGuid.Empty, false);
				((IColumnHeadingManagerListener)columnConfigField).SetManager(columnConfigFieldManager);

				var collectionProvider = new RefUNLOCOCollectionProvider(Factory);
				codeLookupField.SetCollectionProvider(collectionProvider);

				var list = new CodeDescriptionPairList();
				list.AddPair("", "empty");
				codeListMulipleChoiceField.SetPairList(list);

				lookupField.SetCollectionProvider(new RefUNLOCOCollectionProvider(Factory));
				report.ColumnHeadingManager.AddLinkedField(lookupField);
				report.ColumnHeadingManager.SaveToFilterField = lookupField.DisplayName;

				multiSelectField.GroupName = "x";
				multiSelectField.GroupDescription = "y";
				multiSelectField.SetCollectionProvider(collectionProvider);

				optionsGroupField.AddOption("Invoice", "INV");
				optionsGroupField.DescriptionCodePairList[0].Value = true;

				var sendProvider = new SendingAgentCollectionProvider(Factory);
				sendProvider.Collection.Add(Factory.NewWithValidTestData<OrgHeader>());
				sendingAgentLookup.GroupName = "S";
				sendingAgentLookup.GroupDescription = "Send";
				sendingAgentLookup.DisplayName = "Sending Agent";
				sendingAgentLookup.SetCollectionProvider(sendProvider);

				Assert(!string.IsNullOrEmpty(sendingAgentLookup.ValueAsObject.ToString()));

				var receiveProvider = new ReceivingAgentCollectionProvider(Factory);
				receiveProvider.Collection.Add(Factory.NewWithValidTestData<OrgHeader>());
				receivingAgentLookup.GroupName = "R";
				receivingAgentLookup.GroupDescription = "Receive";
				receivingAgentLookup.DisplayName = "Receiving Agent";
				receivingAgentLookup.SetCollectionProvider(receiveProvider);

				Assert(!string.IsNullOrEmpty(receivingAgentLookup.ValueAsObject.ToString()));

				report.FilterCollection.Add(codeLookupField);
				report.FilterCollection.Add(codeListMulipleChoiceField);
				report.FilterCollection.Add(lookupField);
				report.FilterCollection.Add(dateField1);
				report.FilterCollection.Add(dateField2);
				report.FilterCollection.Add(dateField3);
				report.FilterCollection.Add(multiSelectField);
				report.FilterCollection.Add(columnConfigField);
				report.FilterCollection.Add(optionsGroupField);
				report.FilterCollection.Add(sendingAgentLookup);
				report.FilterCollection.Add(receivingAgentLookup);

				dateField2.ValueInfo.AddError("x");

				var deserialisedLookupField = new CodeLookupField(new CodeLookupFieldJsonData());
				var deserialisedCodeListMulipleChoiceField = new CodeListMultipleChoice(new CodeListMultipleChoiceJsonData());
				var deserialisedField3 = new LookupField(new LookupFieldJsonData());
				deserialisedField3.SetCollectionProvider(new RefUNLOCOCollectionProvider(Factory));
				var deserialisedField4 = new DateField(new DateFieldJsonData());
				var deserialisedField7 = new MultipleSelectionLookup(new MultipleSelectionLookupJsonData());
				var deserialsedColumnConfigField = JsonConverterHelper.Deserialize<ColumnConfigurationField>(JsonConverterHelper.Serialize(columnConfigField));

				deserialisedLookupField.DisplayName = "1";
				deserialisedCodeListMulipleChoiceField.DisplayName = "2";
				deserialisedField3.DisplayName = "Link";
				deserialisedField4.DisplayName = "4";
				deserialisedField7.DisplayName = "7";
				deserialsedColumnConfigField.DisplayName = "Columns";

				var guid = Factory.LoadTop1<RefUNLOCO>(new ZQuery()).PK;
				deserialisedField3.ZValue = guid;

				var newConfig = new CombinedConfigurationManager(report.ColumnHeadingManager, guid, "WTF", "Whatever", "Test");
				newConfig.Save();
				newConfig.Load();

				deserialsedColumnConfigField.Value = newConfig;

				var anotherCollectionProvider = new RefUNLOCOCollectionProvider(Factory);
				deserialisedField7.SetCollectionProvider(anotherCollectionProvider);
				deserialisedField7.ValueAsStringForSerialisation = "AUBNE, AUSYD";

				deserialisedReport.FilterCollection.Add(deserialisedLookupField);
				deserialisedReport.FilterCollection.Add(deserialisedCodeListMulipleChoiceField);
				deserialisedReport.FilterCollection.Add(deserialisedField3);
				deserialisedReport.FilterCollection.Add(deserialisedField4);
				deserialisedReport.FilterCollection.Add(deserialisedField7);
				deserialisedReport.FilterCollection.Add(deserialsedColumnConfigField);

				deserialisedField4.ValueInfo.AddError("x");

				var validator1 = new AtLeastOneFilterNotEmptyValidator();
				validator1.Filters.Add(codeLookupField);
				validator1.Filters.Add(codeListMulipleChoiceField);
				validator1.Filters.Add(dateField3);
				codeLookupField.Validators.Add(validator1);
				codeListMulipleChoiceField.Validators.Add(validator1);
				dateField3.Validators.Add(validator1);

				var validator2 = new RequiredFilterValidator();
				validator2.Filters.Add(codeListMulipleChoiceField);
				codeListMulipleChoiceField.Validators.Add(validator2);

				var validator3 = new RequiredFilterValidator();
				validator3.Filters.Add(lookupField);
				lookupField.Validators.Add(validator3);

				report.ColumnHeadingManager.SaveToFilterField = "Link";

				((IReportForUnitTesting)report).UpdateFilterCollectionOnReportFromDeserializedReport();

				AssertEquals("FilterCollection.Count", 11, report.FilterCollection.Count);
				AssertEquals("FilterCollection[0]", deserialisedLookupField, report.FilterCollection[0]);
				AssertEquals("FilterCollection[1]", deserialisedCodeListMulipleChoiceField, report.FilterCollection[1]);
				AssertEquals("FilterCollection[2]", deserialisedField3, report.FilterCollection[2]);
				AssertEquals("FilterCollection[3]", deserialisedField4, report.FilterCollection[3]);
				AssertEquals("FilterCollection[4]", dateField2, report.FilterCollection[4]);
				AssertEquals("FilterCollection[5]", dateField3, report.FilterCollection[5]);
				AssertEquals("FilterCollection[6]", deserialisedField7, report.FilterCollection[6]);
				AssertEquals("FilterCollection[7]", deserialsedColumnConfigField, report.FilterCollection[7]);

				AssertEquals("deserialsedColumnConfigField.Manager", columnConfigFieldManager, ((ColumnConfigurationField)report.FilterCollection[7]).Manager);
				AssertEquals("deserialisedLookupField.CollectionProvider", collectionProvider, deserialisedLookupField.CollectionProvider);
				AssertEquals("deserialisedCodeListMulipleChoiceField.List", list, deserialisedCodeListMulipleChoiceField.List);

				AssertEquals("deserialisedLookupField.Validators.Count", 1, deserialisedLookupField.Validators.Count);
				AssertEquals("deserialisedCodeListMulipleChoiceField.Validators.Count", 2, deserialisedCodeListMulipleChoiceField.Validators.Count);
				AssertEquals("deserialisedField3.Validators.Count", 1, deserialisedField3.Validators.Count);
				AssertEquals("deserialisedField4.Validators.Count", 0, deserialisedField4.Validators.Count);

				AssertEquals("field5.Validators.Count", 0, dateField2.Validators.Count);
				AssertEquals("field6.Validators.Count", 1, dateField3.Validators.Count);
				AssertEquals("deserialisedLookupField.Validators[0]", validator1, deserialisedLookupField.Validators[0]);
				AssertEquals("deserialisedCodeListMulipleChoiceField.Validators[0]", validator1, deserialisedCodeListMulipleChoiceField.Validators[0]);
				AssertEquals("deserialisedCodeListMulipleChoiceField.Validators[1]", validator2, deserialisedCodeListMulipleChoiceField.Validators[1]);
				AssertEquals("deserialisedField3.Validators[0]", validator3, deserialisedField3.Validators[0]);

				AssertEquals("field6.Validators[0]", validator1, dateField3.Validators[0]);

				AssertEquals("validator1.Filters.Count", 3, validator1.Filters.Count);
				AssertEquals("validator2.Filters.Count", 1, validator2.Filters.Count);
				AssertEquals("validator3.Filters.Count", 1, validator3.Filters.Count);

				AssertEquals("validator1.Filters[0]", deserialisedLookupField, validator1.Filters[0]);
				AssertEquals("validator1.Filters[1]", deserialisedCodeListMulipleChoiceField, validator1.Filters[1]);
				AssertEquals("validator1.Filters[2]", dateField3, validator1.Filters[2]);
				AssertEquals("validator2.Filters[0]", deserialisedCodeListMulipleChoiceField, validator2.Filters[0]);
				AssertEquals("validator3.Filters[0]", deserialisedField3, validator3.Filters[0]);
				AssertEquals("deserialisedField7.GroupName", "x", deserialisedField7.GroupName);
				AssertEquals("deserialisedField7.GroupDescription", "y", deserialisedField7.GroupDescription);

				AssertEquals("deserialisedField7.ValueAsString", "AUBNE, AUSYD", deserialisedField7.ValueAsStringForSerialisation);

				AssertNoErrors(deserialisedLookupField);
				AssertNoErrors(deserialisedCodeListMulipleChoiceField);
				AssertNoErrors(deserialisedField3);
				AssertNoErrors(deserialisedField4);
				AssertNoErrors(dateField2);
				AssertNoErrors(dateField3);
				AssertNoErrors(deserialsedColumnConfigField);

				Assert("Default value of option group should have been cleared if it is not in filter collection of deserialized report ", string.IsNullOrEmpty(((OptionGroup)report.FilterCollection[8]).ValueAsObject.ToString()));
				Assert("Default value of MultipleSelectionLookup field should have been cleared if it is not in filter collection of deserialized report ", string.IsNullOrEmpty(((MultipleSelectionLookup)report.FilterCollection[9]).ValueAsObject.ToString()));
				Assert("Default value of MultipleSelectionLookup field should have been cleared if it is not in filter collection of deserialized report ", string.IsNullOrEmpty(((MultipleSelectionLookup)report.FilterCollection[10]).ValueAsObject.ToString()));
			}
		}

		public void TestOverrideReportDbOptionSynchronisedWithDeserialisedReport()
		{
			using (var deserialisedReport = Report.NewForTesting(Pack))
			using (var report = Report.NewForTesting(Pack))
			{
				deserialisedReport.OverrideReportDbOption = true;
				report.DeserializedReport = deserialisedReport;
				AssertNotEquals("OverrideReportDbOption not synchronised", deserialisedReport.OverrideReportDbOption, report.OverrideReportDbOption);
				((IReportForUnitTesting)report).SynchroniseReportDataWithDeserialisedReport();
				AssertEquals("OverrideReportDbOption synchronised", deserialisedReport.OverrideReportDbOption, report.OverrideReportDbOption);
			}
		}

		public void TestTheCorrectionOfDuplicatedCodesInDeserialisedOptionGroup()
		{
			using (var report = Report.NewForTesting(Pack))
			{
				ErrorReporter.Clear();
				var replaceOptionsGroup = new OptionGroup(Factory);
				var deserialisedOptionGroup = new OptionGroup(Factory);

				replaceOptionsGroup.AddOption("Description for AAA", "AAA");
				replaceOptionsGroup.AddOption("Description for BBB", "BBB");
				replaceOptionsGroup.AddOption("Description for CCC", "CCC");
				replaceOptionsGroup.AddOption("Description for DDD", "DDD");

				deserialisedOptionGroup.AddOption("Description for AAA", "AAA", true);
				deserialisedOptionGroup.AddOption("Description for BBB", "BBB", true);
				deserialisedOptionGroup.AddOption("Description for DDD", "BBB", true);
				deserialisedOptionGroup.AddOption("Description for CCC", "CCC", true);
				deserialisedOptionGroup.AddOption("Description for EEE", "CCC", false);
				AssertNoExceptionThrown(() =>
				{
					deserialisedOptionGroup.UpdateOptions(replaceOptionsGroup);
				});
				AssertEquals("deserialisedOptionGroup should have 4 items", 4, deserialisedOptionGroup.DescriptionCodePairList.Count);

				AssertEquals("AAA", deserialisedOptionGroup.GetCodeFromDescription("Description for AAA"));
				AssertEquals("BBB", deserialisedOptionGroup.GetCodeFromDescription("Description for BBB"));
				AssertEquals("CCC", deserialisedOptionGroup.GetCodeFromDescription("Description for CCC"));
				AssertEquals("DDD", deserialisedOptionGroup.GetCodeFromDescription("Description for DDD"));

				foreach (var pair in deserialisedOptionGroup.DescriptionCodePairList)
				{
					switch (deserialisedOptionGroup.GetCodeFromDescription(pair.Description))
					{
						case "AAA":
						case "BBB":
						case "CCC":
						case "DDD":
							AssertEquals(true, pair.Value);
							break;
						default:
							Fail("There should not have others code.");
							break;
					}
				}

				ErrorReporter.Clear();
			}
		}

		public void TestOptionGroupFilterFieldSynchronisedWithDeserialisedReport()
		{
			using (var deserialisedReport = Report.NewForTesting(Pack))
			using (var report = Report.NewForTesting(Pack))
			{
				var optionGroup = new OptionGroup(Factory);
				var deserialisedOptionGroup = new OptionGroup(new OptionGroupJsonData());
				var deserialisedList = new CodeDescriptionPairList();
				var newList = new CodeDescriptionPairList();
				newList.AddPair("OP1", "Option 1");
				newList.AddPair("OP2", "Option 2");
				deserialisedList.AddPair("OP1", "Option 1");
				optionGroup.AddAllOptions(newList);
				deserialisedOptionGroup.AddAllOptions(deserialisedList);

				deserialisedOptionGroup.DescriptionCodePairList[0].Value = true;

				report.FilterCollection.Add(optionGroup);
				deserialisedReport.FilterCollection.Add(deserialisedOptionGroup);
				report.DeserializedReport = deserialisedReport;
				((IReportForUnitTesting)report).SynchroniseReportDataWithDeserialisedReport();
				Assert("The Report should have the only one OptionGroup Filter Field.", report.FilterCollection[0] is OptionGroup);
				AssertEquals("The OptionGroup Filter should have 2 Options.", 2, (report.FilterCollection[0] as OptionGroup).BindableBooleanItems.Count);
				AssertEquals("Option 2", (report.FilterCollection[0] as OptionGroup).BindableBooleanItems[0].Text);
				AssertEquals("Option 1", (report.FilterCollection[0] as OptionGroup).BindableBooleanItems[1].Text);
				AssertEquals("Option 2 should be false.", false, (report.FilterCollection[0] as OptionGroup).BindableBooleanItems[0].BoolValue);
				AssertEquals("Option 1 should be true.", true, (report.FilterCollection[0] as OptionGroup).BindableBooleanItems[1].BoolValue);
			}
		}

		public void TestUpdateGroupByCollectionOnReportFromDeserializedReport()
		{
			using (var deserialisedReport = Report.NewForTesting(Pack))
			using (var report = Report.NewForTesting(Pack))
			{
				deserialisedReport.GroupByCollection.BreakPageOverride = false;
				report.DeserializedReport = deserialisedReport;
				AssertEquals("UpdateBreakPageOverrideFromDeserializedReport", false, report.GroupByCollection.BreakPageOverride);

				deserialisedReport.GroupByCollection.BreakPageOverride = true;
				((IReportForUnitTesting)report).UpdateGroupByCollectionOnReportFromDeserializedReport();
				AssertEquals("UpdateBreakPageOverrideFromDeserializedReport", true, report.GroupByCollection.BreakPageOverride);

				deserialisedReport.GroupByCollection.BreakPageOverride = false;
				((IReportForUnitTesting)report).UpdateGroupByCollectionOnReportFromDeserializedReport();
				AssertEquals("UpdateBreakPageOverrideFromDeserializedReport", false, report.GroupByCollection.BreakPageOverride);
			}
		}

		public void TestUpdateCompanyForAccountingPeriodsRangeFieldFromDeserializedReport()
		{
			using (var deserialisedReport = Report.NewForTesting(Pack))
			using (var report = Report.NewForTesting(Pack))
			{
				var reportingBook = Factory.NewWithValidTestData<AccReportingBook>();
				reportingBook.ARB_GC_CompanyOfPeriod = GlbCompany.GetDemoCompany(Factory).PK;
				Factory.Save();

				var accountingPeriodRangeField = new AccountingPeriodsRangeField(Factory);
				accountingPeriodRangeField.DependencyValue = reportingBook.PK.ToString();
				accountingPeriodRangeField.DependentFilter = "Reporting Book";
				accountingPeriodRangeField.DisplayName = "ARB";
				accountingPeriodRangeField.FieldName = "Period";
				report.FilterCollection.Add(accountingPeriodRangeField);

				var deserializedField = new AccountingPeriodsRangeField(new AccountingPeriodsRangeFieldJsonData());
				deserializedField.DisplayName = "ARB";
				deserializedField.FieldName = "Period";

				deserialisedReport.FilterCollection.Add(deserializedField);
				report.DeserializedReport = deserialisedReport;

				((IReportForUnitTesting)report).UpdateFilterCollectionOnReportFromDeserializedReport();
				deserializedField.PeriodFrom = 202302;
				deserializedField.PeriodTo = 202304;

				AssertHasError(deserializedField.PeriodToInfo, @"There is no period set up for 202304 in DEM - Demo Company.
Please go to General Ledger >> Period Management to setup periods.");
			}
		}

		public void TestPeriodDateRangeFieldFromDeserializedReport()
		{
			using (var deserialisedReport = Report.NewForTesting(Pack))
			using (var report = Report.NewForTesting(Pack))
			{
				var reportingBook = Factory.NewWithValidTestData<AccReportingBook>();
				reportingBook.ARB_GC_CompanyOfPeriod = GlbCompany.GetDemoCompany(Factory).PK;
				Factory.Save();

				var periodRangeField = new PeriodDateRangeField(Factory);
				periodRangeField.DependencyValue = reportingBook.PK.ToString();
				periodRangeField.DependentFilter = "Reporting Book";
				periodRangeField.DisplayName = "ARB";
				periodRangeField.FieldName = "Period";
				report.FilterCollection.Add(periodRangeField);

				var deserializedField = new PeriodDateRangeField(new PeriodDateRangeFieldJsonData());
				deserializedField.DisplayName = "ARB";
				deserializedField.FieldName = "Period";

				deserialisedReport.FilterCollection.Add(deserializedField);
				report.DeserializedReport = deserialisedReport;

				((IReportForUnitTesting)report).UpdateFilterCollectionOnReportFromDeserializedReport();
				deserializedField.ValueLow = new ZDateTime(2023, 04, 05);
				deserializedField.ValueHigh = new ZDateTime(2023, 06, 05);

				AssertHasError(deserializedField.ValueLowInfo, @"The date entered is not in the accounting periods of DEM-Demo Company");
			}
		}

		public void TestSingleAccountingPeriodFieldFromDeserializedReport()
		{
			using (var deserialisedReport = Report.NewForTesting(Pack))
			using (var report = Report.NewForTesting(Pack))
			{
				var reportingBook = Factory.NewWithValidTestData<AccReportingBook>();
				reportingBook.ARB_GC_CompanyOfPeriod = GlbCompany.GetDemoCompany(Factory).PK;
				Factory.Save();

				var periodRangeField = new SingleAccountingPeriodField(Factory);
				periodRangeField.DependencyValue = reportingBook.PK.ToString();
				periodRangeField.DependentFilter = "Reporting Book";
				periodRangeField.DisplayName = "ARB";
				periodRangeField.FieldName = "Period";
				report.FilterCollection.Add(periodRangeField);

				var deserializedField = new SingleAccountingPeriodField(new SingleAccountingPeriodFieldJsonData());
				deserializedField.DisplayName = "ARB";
				deserializedField.FieldName = "Period";

				deserialisedReport.FilterCollection.Add(deserializedField);
				report.DeserializedReport = deserialisedReport;

				((IReportForUnitTesting)report).UpdateFilterCollectionOnReportFromDeserializedReport();
				deserializedField.SinglePeriod = 202304;

				AssertHasError(deserializedField.SinglePeriodInfo, @"There is no period set up for 202304 in DEM - Demo Company.
Please go to General Ledger >> Period Management to setup periods.");
			}
		}

		public void TestCodeListMultipleChoiceFromDeserializedReport()
		{
			using (var deserialisedReport = Report.NewForTesting(Pack))
			using (var report = Report.NewForTesting(Pack))
			{
				var originField = new CodeListMultipleChoice(Factory);
				originField.DisplayName = "ARB";
				originField.FieldName = "CodeList";

				var pairList = new CodeDescriptionPairList();
				pairList.AddPair("I1", "Item 1");
				pairList.AddPair("I2", "Item 2");
				originField.SetPairList(new ReadOnlyCodeDescriptionPairList(pairList.ToXMLByteArray()));
				originField.AllowInvalidCode = true;

				report.FilterCollection.Add(originField);

				var deserializedField = new CodeListMultipleChoice(new CodeListMultipleChoiceJsonData());
				deserializedField.DisplayName = "ARB";
				deserializedField.FieldName = "CodeList";

				deserialisedReport.FilterCollection.Add(deserializedField);
				report.DeserializedReport = deserialisedReport;

				AssertNull("No data", deserializedField.List);

				((IReportForUnitTesting)report).UpdateFilterCollectionOnReportFromDeserializedReport();

				AssertEquals("Has data now", 2, deserializedField.List.Count);
				Assert("Has data now: I1", deserializedField.List.IndexOfCode("I1") >= 0);
				Assert("Has data now: I2", deserializedField.List.IndexOfCode("I2") >= 0);
			}
		}

		public void TestLookupFieldFromDeserializedReport()
		{
			using (var deserialisedReport = Report.NewForTesting(Pack))
			using (var report = Report.NewForTesting(Pack))
			{
				var originField = new LookupField(Factory);
				originField.DisplayName = "ARB";
				originField.FieldName = "CodeList";

				var dependenceProvider = new DependenceCollectionProvider(Factory);
				dependenceProvider.List.Add("staff and resource", new StaffAndResourceCollectionProvider(Factory));
				dependenceProvider.List.Add("org", new OrgHeaderCollectionProvider(Factory));
				dependenceProvider.List.Add("gl", new AccGLHeaderCollectionProvider(Factory));

				originField.SetCollectionProvider(dependenceProvider);
				originField.DependencyValue = "org";

				report.FilterCollection.Add(originField);

				var deserializedField = new LookupField(new LookupFieldJsonData());
				deserializedField.DisplayName = "ARB";
				deserializedField.FieldName = "CodeList";

				deserialisedReport.FilterCollection.Add(deserializedField);
				var dependenceProvider2 = new DependenceCollectionProvider(Factory);
				dependenceProvider2.List.Add("staff and resource", new StaffAndResourceCollectionProvider(Factory));
				dependenceProvider2.List.Add("org", new OrgHeaderCollectionProvider(Factory));

				deserializedField.SetCollectionProvider(dependenceProvider2);
				report.DeserializedReport = deserialisedReport;

				AssertEquals("No data now", ModuleIDs.NotAssigned, deserializedField.CollectionProvider.ModuleID);

				((IReportForUnitTesting)report).UpdateFilterCollectionOnReportFromDeserializedReport();

				AssertEquals("Has data now", ModuleIDs.Organisation, deserializedField.CollectionProvider.ModuleID);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestIDocumentMembers()
		{
			var excelTemplate = new ExcelTemplateForUnitTesting("MultipleTemplatesWithOptionalTemplates.xls", TestFilesSubFolder.ReportTestFiles);
			using (var report = new Report(Pack, excelTemplate, Guid.Empty, Enterprise.Core.Constants.DataContext.UnitTest))
			{
				report.PrintCopyType = PrintCopyType.FAX;
				report.IncludedInPrint = true;

				IDocument document = report;
				AssertEquals("DocumentName", "", document.DocumentName);
				AssertEquals("DocumentDeliveryMethod", "FAX", document.DocumentDeliveryMethod);
				AssertEquals("DocumentDeliveryMethod", true, document.IncludeInPrint);

				report.PrintCopyType = PrintCopyType.PRN;
				report.IncludedInPrint = false;

				AssertEquals("DocumentDeliveryMethod", "PRN", document.DocumentDeliveryMethod);
				AssertEquals("DocumentDeliveryMethod", false, document.IncludeInPrint);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestIDeliveryEmailAttachmentMembers()
		{
			var excelTemplate = new ExcelTemplateForUnitTesting("NewStyleTemplate.xls", TestFilesSubFolder.ReportTestFiles);
			using (var report = new Report(Pack, excelTemplate))
			{
				report.PrepareForRender();
				IDeliveryEmailAttachment attachment = report;

				AssertEquals("IDeliveryEmailAttachment.FileName is the report name", report.Name, attachment.FileName);
				AssertEquals("IDeliveryEmailAttachment.FileSizeInBytes is the report template size", report.Template.GetAsTemplateStream().Length, attachment.FileSizeInBytes);

				report.IncludedInPrint = true;
				Assert(attachment.ShouldBeAttached);

				report.IncludedInPrint = false;
				Assert(!attachment.ShouldBeAttached);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[GuiTest]
		public void TestShowErrorsIfAny()
		{
			var excelTemplate = new ExcelTemplateForUnitTesting("MultipleTemplatesWithOptionalTemplates.xls", TestFilesSubFolder.ReportTestFiles);
			var printTaskUIProvider = new PrintTaskUIProviderForTesting();
			using (new PrintTaskUIProviderFactory.OverriderForTesting(printTaskUIProvider))
			using (var report = new Report(Pack, excelTemplate, Guid.Empty, Core.Constants.DataContext.UnitTest))
			{
				var notifications = new NotificationBuffer();
				report.ScheduleTaskNotifications = notifications;
				((IReportForUnitTesting)report).StopErrorsThrowingAnException = true;

				AssertEquals("Pre-Condition: report.ErrorManager.HasErrors is false", false, report.ErrorManager.HasErrors);
				AssertEquals("Globals.IsUserInteractive", true, Globals.IsUserInteractive);

				printTaskUIProvider.ResetShowErrors();
				((IReportForUnitTesting)report).ShowErrorsIfAny();
				AssertEquals("printTaskUIProvider.ShowErrorsCalled", false, printTaskUIProvider.ShowErrorsCalled);
				AssertEquals("ScheduleTaskNotifications.Events.Length", 0, notifications.Events.Length);

				report.ErrorManager.Add(new ReportProcessingError("Moo", ReportProcessingErrorSeverity.Warning));
				AssertEquals("Report.Errors", @"Severity: [Warning (without error report)] Message: [Moo] Cell: [N/A] Sheetname: [(unknown)]", report.ErrorManager.ToString("Severity: [{0}] Message: [{1}] Cell: [{2}] Sheetname: [{3}]", false));
				printTaskUIProvider.ResetShowErrors();
				((IReportForUnitTesting)report).ShowErrorsIfAny();
				Assert(printTaskUIProvider.CanFormBeCreatedDuringDbTransaction);

				printTaskUIProvider.ResetShowErrors();
				Assert(!printTaskUIProvider.CanFormBeCreatedDuringDbTransaction);
				((IReportForUnitTesting)report).ShowErrorsIfAny();
				AssertEquals("ScheduleTaskNotifications.Events.Length", 0, notifications.Events.Length);
				report.ErrorManager.ClearErrors();

				report.ErrorManager.Add(new ReportProcessingError("Moo", new CellReference("A", "B"), ReportProcessingErrorSeverity.Warning));
				report.ErrorManager.Add(new ReportProcessingError("Oink", new CellReference("C", "D"), ReportProcessingErrorSeverity.Warning));
				AssertEquals("Report.Errors", @"
Severity: [Warning (without error report)] Message: [Moo] Cell: [B] Sheetname: [A]
Severity: [Warning (without error report)] Message: [Oink] Cell: [D] Sheetname: [C]".Trim(), report.ErrorManager.ToString("Severity: [{0}] Message: [{1}] Cell: [{2}] Sheetname: [{3}]", false));

				try
				{
					Globals.IsUserInteractive = false;
					printTaskUIProvider.ResetShowErrors();
					((IReportForUnitTesting)report).ShowErrorsIfAny();
				}
				finally
				{
					Globals.IsUserInteractive = true;
				}

				AssertEquals("printTaskUIProvider.ShowErrorsCalled", false, printTaskUIProvider.ShowErrorsCalled);
				AssertEquals("ScheduleTaskNotifications.Events.Length", 1, notifications.Events.Length);
				AssertMultilineASCIIEquals("ScheduleTaskNotifications.Events[0].Message", string.Format(@"
Severity: [Warning (without error report)] Message: [Moo] Cell: [B] Sheetname: [A] TemplatePath: [{1}]
Severity: [Warning (without error report)] Message: [Oink] Cell: [D] Sheetname: [C] TemplatePath: [{1}]

Report Information:

MenuItem:-
   BusinessContext = []
   Name with Path = []
   Filter = []
   PK = [{0}]
   IsSystemDefined = [N]
   IsClientSpecific = [N]

No StmTemplate Found on the Report.

Not Running from Scheduled Report.

".Trim(), report.MenuItem.PK, excelTemplate.TemplateSourceLocation), notifications.Events[0].Message);
			}
			ErrorReporter.Clear();
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestUpdateAndSynchroniseFilters()
		{
			var excelTemplate = new ExcelTemplateForUnitTesting("TextFilter.xls", TestFilesSubFolder.ReportTestFiles);
			using (var report = new Report(Pack, excelTemplate))
			{
				AssertEquals("Prerequisite: shouldn't be PreparedForRender", false, ((IReportForUnitTesting)report).IsPreparedForRender);
				report.UpdateAndSynchroniseFilters();
				AssertEquals("Should read filters", 2, report.FilterCollection.Count);
				AssertEquals("Shouldn't be PreparedForRender", false, ((IReportForUnitTesting)report).IsPreparedForRender);
				AssertNull(((IReportForUnitTesting)report).XlInterfaceDirect);
			}
		}

		public void TestSetSheetName()
		{
			TestData.CreateLinesTestTable();
			using (Report.TemporarilyUseMainConnection())
			using (var report = new Report(new DocumentPack(), EmptyAndValidTemplate))
			{
				report.WorkSheetCurrentlyBeingProcessed[2, 0] = "SheetNameOverride=Hello World Report Loooooooooooooooooooooooooooooooong suffix";
				report.WorkSheetCurrentlyBeingProcessed[4, 0] = "Data:Test=##LinesTest";
				report.PrepareForRender();

				using (var outputStream = new MemoryStream())
				{
					report.Save(outputStream);

					AssertEquals(1, report.SheetNames.Count);
					AssertEquals("Hello World Report Looooooooooo", report.SheetNames.First().StrictName);
					AssertEquals("Hello World Report Loooooooooooooooooooooooooooooooong suffix", report.SheetNames.First().EntireName);

					using (var excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(outputStream);
						AssertEquals("Hello World Report Looooooooooo", excelInterface.Xls.ActiveSheetByName);
					}
				}
			}
		}

		public void TestUsesReportNameForSheetNameIfNotOverriden()
		{
			TestData.CreateJobTestTable();
			TestData.CreateHeaderTestTable();
			TestData.CreateLinesTestTable();
			TestData.CreateDocEngineTestTable();

			using (Report.TemporarilyUseMainConnection())
			using (var report = new Report(Pack, EmptyAndValidTemplate, new DummyDocWrapper(), "Some Document", null, DocumentDirection.ANY, false))
			{
				report.StTemplate = Factory.New<StmTemplate>();
				report.StTemplate.SO_Name = Enterprise.Core.Constants.SectionRepositoryTemplateNames.System;

				var contact = new DocDeliveryContact(Factory);
				contact.OrgHeaderPK = Factory.New<OrgHeader>().PK;

				var config = Factory.New<StmMenuDocumentConfig>();
				var pivot = Factory.New<StmMenuTemplatePivot>();
				pivot.SI_SU = report.MenuItem.PK;
				pivot.SI_SO = report.StTemplate.PK;
				config.S3_SI = pivot.PK;
				config.S3_GC = Env.CurrentCompany.PK;
				config.S3_OH = contact.OrgHeaderPK;

				using (var outputStream = new MemoryStream())
				{
					report.Save(contact, contact, outputStream);

					using (var excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(outputStream);

						AssertEquals(1, excelInterface.Xls.SheetCount);
						excelInterface.Xls.ActiveSheet = 1;
						AssertEquals("Some Document", excelInterface.Xls.SheetName);
					}
				}
			}
		}

		[TestDate(2010, 05, 24, 10, 10, 00)]
		public void TestMultipleNestedMacrosInEmailSubject()
		{
			using (Stream templateStream = new MemoryStream())
			{
				DocumentEngineTestHelper.GenerateTemplateStreamFromString(templateStream,
	@"{A}-[#Config]
{A}-[Name=TestEmailSubjectMacros]
{A}-[EmailSubject=Current Date: <DateTimeAsString('<Now>', 'dd-MMM-yy')> Current Time: <DateTimeAsString('<Now>', 'hh:mm')>]


{A}-[#EndOfReport]");

				var excelTemplate = new ExcelTemplateWrappingStream("Test", templateStream);

				using (new TemporaryValueSetter<string>(value => GlbCompany.CurrentCompany.GC_Name = value, GlbCompany.CurrentCompany.GC_Code, "Robert's Company"))
				using (new TemporaryValueSetter<string>(value => GlbBranch.CurrentBranch.GB_BranchName = value, GlbBranch.CurrentBranch.GB_BranchName, "Developer"))
				using (var report = new Report(new DocumentPack(), excelTemplate))
				{
					report.PrepareForRender();

					using (var outputStream = new MemoryStream())
					{
						report.Save(outputStream);
						using (var excelInterface = new ExcelInterface())
						{
							var expectedDate = new DateTime(2010, 05, 24, 10, 10, 00);

							excelInterface.LoadExcelFile(outputStream);
							AssertEquals("Robert's Company - Developer - Current Date: 24-May-10 Current Time: 10:10",
								report.EmailSubject);
						}
					}
				}
			}
		}

		[TestDate(2010, 05, 24, 10, 10, 00)]
		public void TestTranslateMacros()
		{
			var dummy = Factory.New<DummyBODocSupportable>();
			dummy.Z0_Code = "Test";

			using (new TemporaryValueSetter<string>(value => GlbCompany.CurrentCompany.GC_Name = value, GlbCompany.CurrentCompany.GC_Code, "Robert's Company"))
			using (var report = new Report(Pack, EmptyAndValidTemplate, BODocDataProvider.Get(dummy), "Some Document", null, DocumentDirection.ANY, false))
			{
				AssertEquals("Robert's Company", report.TranslateMacros("<CompanyName>"));
				AssertEquals("Test", report.TranslateMacros("<Z0_Code>"));
				AssertEquals("24-May-10", report.TranslateMacros("<DateTimeAsString('<Now>', 'dd-MMM-yy')>"));
			}
		}

		[TestDate(2010, 10, 14, 14, 18, 00)]
		public void TestNotFormattedNowInEmailSubject()
		{
			using (Stream templateStream = new MemoryStream())
			{
				DocumentEngineTestHelper.GenerateTemplateStreamFromString(templateStream,
	@"{A}-[#Config]
{A}-[Name=TestEmailSubjectMacros]
{A}-[EmailSubject=Current Date:<Now>]


{A}-[#EndOfReport]");

				var excelTemplate = new ExcelTemplateWrappingStream("Test", templateStream);

				using (new TemporaryValueSetter<string>(value => GlbCompany.CurrentCompany.GC_Name = value, GlbCompany.CurrentCompany.GC_Code, "Anton's Company"))
				using (new TemporaryValueSetter<string>(value => GlbBranch.CurrentBranch.GB_BranchName = value, GlbBranch.CurrentBranch.GB_BranchName, "Developer"))
				using (var report = new Report(new DocumentPack(), excelTemplate))
				{
					report.PrepareForRender();

					using (var outputStream = new MemoryStream())
					{
						report.Save(outputStream);
						using (var excelInterface = new ExcelInterface())
						{
							excelInterface.LoadExcelFile(outputStream);
							AssertEquals("Anton's Company - Developer - Current Date:Thursday, 14 October 2010 14:18:00",
								report.EmailSubject);
						}
					}
				}
			}
		}

		public void TestGetDeliveryInfoSetsFromEmailCustomField()
		{
			using (var report = new Report(Pack, NewStyleTemplate))
			{
				report.PrinterDetails.NumberOfCopies = 42;
				var wrapper = new DummyFromEmailWrapper();
				((IReportForUnitTesting)report).SetBusinessObjectForTesting(wrapper);
				report.IsDeliveredByEmail = true;
				var info = ((IDeliverable)report).GetDeliveryInfo(false);
				AssertEquals("From email address", "CargoWise <PleaseDoNotReply@cargowise.com>", info.EmailFromAddress);
			}
		}

		[ExpectNoExceptions]
		public void TestSetDocumentName_WhenAnalyserIsNull_ShouldNotThrow()
		{
			using (var report = new Report(Pack, NewStyleTemplate))
			{
				AssertNull(report.Analyser);
				report.SetDocumentName();
			}
		}

		public void TestContainsAnyCustomisation()
		{
			using (var report = new Report(Pack, NewStyleTemplate))
			{
				report.StTemplate = Factory.New<StmTemplateBase>();

				ResetCustomisationFlags(report);
				AssertShouldContainAnyCustomisationAndResetFlags(report, false);

				report.Template.ContainsCustomisedSections = true;
				AssertShouldContainAnyCustomisationAndResetFlags(report, true);

				report.MenuItem.SU_IsSystemDefined = false;
				AssertShouldContainAnyCustomisationAndResetFlags(report, true);

				report.StTemplate.SO_IsSystemDefined = false;
				AssertShouldContainAnyCustomisationAndResetFlags(report, true);

				var pivot = Factory.New<StmMenuTemplatePivot>();
				pivot.SI_SU = report.MenuItem.PK;
				pivot.SI_SO = report.StTemplate.PK;

				report.Pivot.SI_IsSystemDefined = false;
				AssertShouldContainAnyCustomisationAndResetFlags(report, true);

				report.Pivot.Template.SO_IsSystemDefined = false;
				AssertShouldContainAnyCustomisationAndResetFlags(report, true);
			}
		}

		public void TestContainsAnyCustomisationForRetriever()
		{
			var template = Factory.New<StmTemplate>();
			template.SO_Name = "Test";
			var dataContextForTesting = Enterprise.Core.Constants.DataContext.Shipment;
			template.SO_DataContext = dataContextForTesting.ToString();

			template.SO_Template = EmptyAndValidTemplate.GetAsByteArray();

			template.SO_IsSystemDefined = false;

			var result = ExcelTemplateRetriever.GetTemplate("Test", dataContextForTesting, Factory);

			using (var report = new Report(null, result))
			{
				AssertEquals(report.ContainsAnyCustomisation, true);
			}

			template.SO_IsSystemDefined = true;
			result = ExcelTemplateRetriever.GetTemplate("Test", dataContextForTesting, Factory);

			using (var report = new Report(null, result))
			{
				AssertEquals(report.ContainsAnyCustomisation, false);
			}
		}

		public void TestContainsDataRows()
		{
			CombineAssertions("ContainsDataRows should be correct ", () =>
			{
				AssertReportContainsDataRow(false, false, false);
				AssertReportContainsDataRow(true, false, true);
			});
		}

		void AssertReportContainsDataRow(bool fisrtPageHasData, bool secondPageHasData, bool excepectedResult)
		{
			using (var templateStream = new MemoryStream())
			{
				var excelTemplate = CreateTestEmptyReportContingencyEMLTemplate(templateStream, fisrtPageHasData, secondPageHasData);
				using (var report = new Report(new DocumentPack(), excelTemplate))
				{
					using (var stream = new MemoryStream())
					{
						report.Save(stream);
						AssertEquals(excepectedResult, report.ContainsDataRows);
					}
				}
			}
		}

		public void TestAttachShouldNotEmptyWhenEmptyReportContingencyIsEMLAndAttachIsXLSX()
		{
			ExcelTemplateWrappingStream excelTemplate = null;
			ReportCommand reportCommand = null;
			using (var templateStream = new MemoryStream())
			{
				excelTemplate = CreateTestEmptyReportContingencyEMLTemplate(templateStream, true, false);
				reportCommand = DocumentEngineTestHelper.CreateReportCommandWithExcelTemplate("Test Report", excelTemplate, Factory);
			}

			RunPrintTask(reportCommand);

			var printJob = Factory.LoadTop1<StmPrintJob>(new ZQuery(StmPrintJobSchema.SP_EmailAttachments, "Test Report.XLSX"));
			AssertNotNull(printJob);

			using (var templateStream = new MemoryStream())
			{
				excelTemplate = CreateTestEmptyReportContingencyEMLTemplate(templateStream, false, false);
				reportCommand = DocumentEngineTestHelper.CreateReportCommandWithExcelTemplate("Test Report2", excelTemplate, Factory);
			}

			RunPrintTask(reportCommand);

			printJob = Factory.LoadTop1<StmPrintJob>(new ZQuery(StmPrintJobSchema.SP_EmailAttachments, "Test Report2.XLSX"));
			AssertNull(printJob);
			var emails = Env.OutgoingMailManager.EmailsCreated;
			AssertEquals("One email should have been created", 1, emails.Count);
			AssertContains("The resulting document was empty and therefore has not been delivered.", emails[0].Body);
		}

		void RunPrintTask(ReportCommand reportCommand)
		{
			using (var printTask = new PrintTask(reportCommand))
			{
				var documentPack = new DocumentPack(reportCommand);

				var instructions = new DeliveryInstructions(documentPack);
				instructions.Destination = DeliveryInstructionDestination.TakenFromContact;
				instructions.Recipients.RemoveAndDeleteAll();
				var docContact = instructions.Recipients.AddNew();
				docContact.DeliveryMethod = Enterprise.Core.Constants.ContactNotifyModes.Email;
				docContact.AttachmentType = AttachmentTypeList.Codes.Xlsx;
				docContact.Email = "unit.test@cargowise.com";
				docContact.Name = "Test";
				docContact.EmptyReportContingency = new EmptyReportContingency(EmptyReportContingencyList.Codes.SendEmailNotification, docContact.Email);

				printTask.Add(documentPack);
				printTask.Run(instructions);
			}
		}

		ExcelTemplateWrappingStream CreateTestEmptyReportContingencyEMLTemplate(MemoryStream templateStream, bool firstPageHasDataLine, bool secondPageHasDataLine)
		{
			using (var creationExcelInterface = new ExcelInterface())
			{
				creationExcelInterface.NewExcelFile(2);
				var workSheet = creationExcelInterface.WorkSheets[0];
				workSheet.SheetNameOverride = "TEST1";
				workSheet[0, 0] = "#config";
				workSheet[1, 0] = "Name=RunningConnectionTemplate";
				workSheet[2, 0] = "data:test=SELECT @@SPID AS SessionId, SERVERPROPERTY('ServerName') AS ServerName, DB_NAME() AS DatabaseName";

				if (!firstPageHasDataLine)
				{
					workSheet[2, 0] += " where 1=0 ";
				}

				workSheet[3, 0] = "#SectionBody:data=test";
				workSheet[4, 1] = "<test.SessionId>";
				workSheet[4, 2] = "<test.ServerName>";
				workSheet[4, 3] = "<test.DatabaseName>";
				workSheet[5, 0] = "#endofreport";

				var workSheet1 = creationExcelInterface.WorkSheets[1];
				workSheet1.SheetNameOverride = "TEST2";
				workSheet1[0, 0] = "#config";
				workSheet1[1, 0] = "Name=RunningConnectionTemplate";
				workSheet1[2, 0] = "data:test=SELECT @@SPID AS SessionId, SERVERPROPERTY('ServerName') AS ServerName, DB_NAME() AS DatabaseName";

				if (!secondPageHasDataLine)
				{
					workSheet1[2, 0] += " where 1=0 ";
				}

				workSheet1[3, 0] = "#SectionBody:data=test";
				workSheet1[4, 1] = "<test.SessionId>";
				workSheet1[4, 2] = "<test.ServerName>";
				workSheet1[4, 3] = "<test.DatabaseName>";
				workSheet1[5, 0] = "#endofreport";
				creationExcelInterface.SaveToStream(templateStream);
			}

			var excelTemplate = new ExcelTemplateWrappingStream("RunningConnectionTemplate", templateStream);
			return excelTemplate;
		}

		void AssertShouldContainAnyCustomisationAndResetFlags(Report report, bool shouldContainCustomisations)
		{
			var message = string.Format("Report.ContainsAnyCustomisation should be " + shouldContainCustomisations);
			AssertEquals(message, shouldContainCustomisations, report.ContainsAnyCustomisation);
			ResetCustomisationFlags(report);
		}

		void ResetCustomisationFlags(Report report)
		{
			report.Template.ContainsCustomisedSections = false;
			report.MenuItem.SU_IsSystemDefined = true;
			report.StTemplate.SO_IsSystemDefined = true;
			if (report.Pivot != null)
			{
				report.Pivot.SI_IsSystemDefined = true;
				report.Pivot.Template.SO_IsSystemDefined = true;
			}
		}

		public void TestCanIncludeInPrint_ShouldDefaultToTrue()
		{
			IDocument report = new Report(new DocumentPack(), NewStyleTemplate);
			Assert(report.CanIncludeInPrint);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestIncludeInPrintShouldTriggerEmailDeliveryValidation()
		{
			var excelTemplate = new ExcelTemplateForUnitTesting("SimpleTest with big size.xls",
					TestFilesSubFolder.ReportTestFiles);
			var command = Factory.New<DocumentCommand>();
			var pack = new DocumentPack(command);
			var instructions = new DeliveryInstructions(pack);
			var testReport = new Report(pack, excelTemplate) { PrintCopyType = PrintCopyType.ALL };
			instructions.DeliverablesToBePrinted.Add(testReport);

			instructions.Recipients.RemoveAndDeleteAll();
			var contact = instructions.Recipients.AddNew();
			contact.DeliveryMethod = "EML";
			contact.AttachmentType = "PDF";
			contact.DeliveryAddress = "test@wtg.com";

			using (SystemDataRegistry.Instance.EmailAttachmentSizeLimitInMB.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 1))
			{
				testReport.IncludedInPrint = true;
				AssertHasError(contact.DeliveryMethodInfo,
					$"One or more eDoc files exceeds the 1MB attachment limit and cannot be sent: . The limit is defined in the Registry at {((IRegistryItemInternals)SystemDataRegistry.Instance.EmailAttachmentSizeLimitInMB).Location}.");

				testReport.IncludedInPrint = false;
				AssertNoErrors(contact.DeliveryMethodInfo);
			}

			//999 999 999 MB when converted to Bytes will overflow int.MaxValue. This makes sure we cater for the max value this registry supports
			using (SystemDataRegistry.Instance.EmailAttachmentSizeLimitInMB.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 999_999_999))
			{
				testReport.IncludedInPrint = true;
				AssertNoErrors(contact.DeliveryMethodInfo);
			}
		}

		public void TestCreateReport_ShouldNotCacheInFactory()
		{
			var reference = CreateReportWeakReference();
			GC.Collect();
			GC.WaitForFullGCComplete();
			AssertNull(reference.Target);
		}

		void RunTestSetEmailSubject(Report report, bool isScheduledReport, bool useScheduledTaskDescription, Action test, string scheduleDescription = "Test Schedule Task Description")
		{
			if (isScheduledReport)
			{
				var scheduleTask = Factory.NewWithValidTestData<ReportScheduleTask>();
				scheduleTask.S5_ScheduleDescription = scheduleDescription;
				report.SetScheduleTask(scheduleTask);
			}

			using (DocumentsDataRegistry.Instance.UseScheduledTaskDescriptionInEmailSubjectForScheduledReports.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, useScheduledTaskDescription))
			using (DocumentsDataRegistry.Instance.EmailFormat.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, RegistryEmailFormatForTesting))
			{
				report.SetEmailSubject();
				test();
			}
		}

		EmailFormat RegistryEmailFormatForTesting
		{
			get
			{
				var format = new EmailFormat();
				format.EmailSubjectFields.RemoveAndDeleteAll();
				format.EmailSubjectFields.Add(new EmailSubjectField("1", Core.Constants.EmailFormat.EmailFieldCodes.DocumentName));
				return format;
			}
		}

		public void TestSetEmailSubject_NotScheduledReport()
		{
			var report = new Report(new DocumentPack(), NewStyleTemplate);
			report.Name = "Document Name";
			report.SetDocumentName();

			RunTestSetEmailSubject(report, isScheduledReport: false, useScheduledTaskDescription: false, test: () =>
			{
				AssertNull(report.ScheduleTask);
				Assert(report.EmailSubject.Contains("Document Name"));
			});
		}

		public void TestSetEmailSubject_ScheduledReport_RegistryUseScheduledTaskDescriptionForEmailSubjectFalse()
		{
			var report = new Report(new DocumentPack(), NewStyleTemplate);
			report.Name = "Document Name";
			report.SetDocumentName();

			RunTestSetEmailSubject(report, isScheduledReport: true, useScheduledTaskDescription: false, test: () =>
			{
				AssertNotNull(report.ScheduleTask);
				Assert(report.EmailSubject.Contains(report.Name));
			});
		}

		[TestDate(2019, 1, 1)]
		public void TestSetEmailSubject_ScheduledReport_RegistryUseScheduledTaskDescriptionForEmailSubjectTrue()
		{
			var docpack = new DocumentPack();
			var report = new Report(docpack, NewStyleTemplate);
			report.Name = "Document Name";
			report.SetDocumentName();

			var deliveryInstructions = new DeliveryInstructions(docpack);
			deliveryInstructions.Recipients.RemoveAndDeleteAll();

			var recipient = deliveryInstructions.Recipients.AddNew();
			recipient.DeliveryMethod = Core.Constants.ContactNotifyModes.Email;
			recipient.AttachmentType = AttachmentTypeList.Codes.Pdf;
			recipient.Name = "Sango";
			recipient.DeliveryAddress = "Sango@test.com";

			var scheduleTask = Factory.NewWithValidTestData<ReportScheduleTask>();
			scheduleTask.S5_ScheduleDescription = "Test -<Recipients.S6_EmailToRecipientsAsString> <Now>";
			scheduleTask.PopulateDefaultsFromDeliveryInstructions(deliveryInstructions);
			Factory.Save();

			report.SetScheduleTask(scheduleTask);

			using (DocumentsDataRegistry.Instance.UseScheduledTaskDescriptionInEmailSubjectForScheduledReports.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (DocumentsDataRegistry.Instance.EmailFormat.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, RegistryEmailFormatForTesting))
			{
				report.SetEmailSubject();
				AssertNotNull(report.ScheduleTask);
				Assert(!report.EmailSubject.Contains("Document Name"));
				AssertEquals("Test -Sango@test.com Tuesday, 01 January 2019 00:00:00", report.EmailSubject);
			}
		}

		public void TestGetLocalizedSheetName()
		{
			using (var resourceStrings = Res.UseMockData())
			{
				resourceStrings.Put("ReportName|The Title", new ResourceStringData("", "Der Titel"));
				AssertEquals("Der Titel", Report.GetLocalizedSheetName("The Title"));
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestRegisterUsedUDFFieldsWithIsOverriddenInDocData()
		{
			var dateField = new DateField(Factory)
			{
				DisplayName = "Unit test date"
			};

			var userDefinedFieldValueList = new UserControlProviderList(dateField);

			using (var report = new Report(new DocumentPack(), new ExcelTemplateForUnitTesting("UserDefinedField - Date.xls", TestFilesSubFolder.ReportTestFiles), new DataProviderList(new DummyFromEmailWrapper()), "New Report 1", userDefinedFieldValueList, DocumentDirection.ANY, false))
			{
				report.PrepareForRender();
				AssertNotNull(report.MacroTranslator);
				AssertEquals(typeof(DelegateValueProvider), report.MacroTranslator.GetValueProvider(Passes.FirstPass, "<Unit test date>").GetType());
			}

			dateField.IsOverriddenInDocData = true;
			using (var report = new Report(new DocumentPack(), new ExcelTemplateForUnitTesting("UserDefinedField - Date.xls", TestFilesSubFolder.ReportTestFiles), new DataProviderList(new DummyFromEmailWrapper()), "New Report 1", userDefinedFieldValueList, DocumentDirection.ANY, false))
			{
				report.PrepareForRender();
				AssertNotNull(report.MacroTranslator);
				AssertEquals(typeof(FixedValueProvider), report.MacroTranslator.GetValueProvider(Passes.FirstPass, "<Unit test date>").GetType());
			}
		}

		WeakReference CreateReportWeakReference()
		{
			var report = new Report(new DocumentPack(Factory.New<StmMenuItem>()), NewStyleTemplate);
			return new WeakReference(report);
		}

		public void TestMacroNowDontChangeValueInsideReport()
		{
			using (var templateStream = new MemoryStream())
			{
				using (var creationExcelInterface = new ExcelInterface())
				{
					creationExcelInterface.NewExcelFile(1);
					var workSheet = creationExcelInterface.WorkSheets[0];
					workSheet[0, 0] = "#config";
					workSheet[1, 0] = "Name=TemplateFromStream";
					workSheet[2, 0] = "PageStyle=Portrait";
					workSheet[3, 0] = "#DocumentHeader";
					workSheet[4, 1] = "<Now>";
					workSheet[5, 0] = "Name=TemplateFromStream";
					workSheet[6, 0] = "PageStyle=Portrait";
					workSheet[7, 1] = "<Now>";
					workSheet[8, 0] = "#EndOfReport";
					creationExcelInterface.SaveToStream(templateStream);
				}

				var excelTemplate = new ExcelTemplateReadFromByteArray("TemplateFromStream", "", templateStream.ToArray());
				using (var pack = new DocumentPack(Factory.New<DocumentCommand>()))
				using (var report = new Report(pack, excelTemplate, BODocDataProvider.Get(Factory.New<DummyBusinessObject>()), excelTemplate.TemplateName, null, DocumentDirection.ANY, false))
				{
					using (var outputStream = new MemoryStream())
					{
						report.Save(outputStream);
						using (var excelInterface = new ExcelInterface())
						{
							excelInterface.LoadExcelFile(outputStream);
							var datetime1 = double.Parse(excelInterface.Xls.GetCellValue(1, 2).ToString());
							var date1 = DateTime.FromOADate(datetime1);

							var datetime2 = double.Parse(excelInterface.Xls.GetCellValue(4, 2).ToString());
							var date2 = DateTime.FromOADate(datetime2);

							AssertEquals("Initial print", date2, date1);
						}
					}
				}
			}
		}

		public void TestAllAvailableDeliveryModes()
		{
			using (var report = new Report(DocumentPack.EmptyPack, null))
			{
				AssertEquals("Report should support all available delivery modes", nameof(PrintCopyType.ALL), report.AllAvailableDeliveryModes);
			}
		}

		public void TestLanguages()
		{
			var testLanguage = Factory.New<IRefLocalLanguage>();
			testLanguage.RA_Code = "EN";
			testLanguage.RA_RN_NKCountryCode = "CN";
			testLanguage.RA_Description = "Test custom language";
			Factory.Save();

			try
			{
				var testLanguageCheckPoint = Env.Security.DocBuilderLanguages.ChildCheckPoints.FirstOrDefault(p => p.Code == "DocBuilderLanguage_" + testLanguage.FullLanguageCode)
					?? new SecurityCheckpoint("DocBuilderLanguage_" + testLanguage.FullLanguageCode, testLanguage.RA_DescriptionMultilingual, Env.Security.DocBuilderLanguages, Env.Security.SecurityInstance);
				if (!Env.Security.DocBuilderLanguagesLookup.ContainsKey(testLanguage.FullLanguageCode))
				{
					Env.Security.DocBuilderLanguagesLookup.Add(testLanguage.FullLanguageCode, testLanguageCheckPoint);
				}

				var allowedLanguages = WebDataRegistry.Instance.AllowedLanguages.Value;
				allowedLanguages.RemoveAll();

				var allowedItem = allowedLanguages.AddNew();
				allowedItem.Code = Core.Constants.Languages.German;

				allowedItem = allowedLanguages.AddNew();
				allowedItem.Code = Core.Constants.Languages.French;

				allowedItem = allowedLanguages.AddNew();
				allowedItem.Code = testLanguage.FullLanguageCode;

				WebDataRegistry.Instance.AllowedLanguages.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, allowedLanguages);

				var allLanguages = new CodeDescriptionPairList(OLookUpEditType.Language);

				using (var report = new Report(DocumentPack.EmptyPack, null))
				{
					var expected = new[] { Core.Constants.Languages.German, Core.Constants.Languages.French, testLanguage.FullLanguageCode.ToString() }.Select(code => allLanguages[code]);
					AssertContainsExactElementsInAnyOrder(expected, report.Languages);
				}

				using (var report = new Report(DocumentPack.EmptyPack, null))
				{
					allowedItem = allowedLanguages.AddNew();
					allowedItem.Code = Core.Constants.Languages.English;
					WebDataRegistry.Instance.AllowedLanguages.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, allowedLanguages);

					var expected = new[] { Core.Constants.Languages.EnglishAmerican, Core.Constants.Languages.EnglishBritish, Core.Constants.Languages.German, Core.Constants.Languages.French, testLanguage.FullLanguageCode.ToString() }.Select(code => allLanguages[code]);
					AssertContainsExactElementsInAnyOrder(expected, report.Languages);
				}
			}
			finally
			{
				Env.Security.DocBuilderLanguagesLookup.Remove(testLanguage.FullLanguageCode);
			}
		}

		public void TestDocType()
		{
			using (var report = new Report(DocumentPack.EmptyPack, null))
			{
				AssertNullOrEmpty(report.DocumentTypeCode);
				AssertNullOrEmpty(report.DocumentTypeDescription);
			}

			var refDocType = Factory.NewWithValidTestData<RefDocType>();
			refDocType.RT_DocType = "TST";
			refDocType.RT_Desc = "Test";

			var template = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "Test",
				@"{A}-[#Config]
{A}-[#EndOfReport]");

			var stmMenuItem = Factory.New<StmMenuItemBase>();
			var pivot = stmMenuItem.Documents.AddNew();
			pivot.SI_SU = stmMenuItem.PK;
			pivot.SI_SO = template.PK;
			pivot.SI_RT_DocType = refDocType.PK;

			using (var pack = new DocumentPack(stmMenuItem))
			using (var report = Report.NewForTesting(pack))
			{
				report.StTemplate = template;
				AssertEquals("DocumentTypeCode", "TST", report.DocumentTypeCode);
				AssertEquals("DocumentTypeDescription", "Test", report.DocumentTypeDescription);
			}
		}

		public void TestIdentifier()
		{
			var refDocType = Factory.NewWithValidTestData<RefDocType>();
			refDocType.RT_DocType = "TST";
			refDocType.RT_Desc = "Test";

			var template = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "Test",
		@"{A}-[#Config]
{A}-[#EndOfReport]");

			var stmMenuItem = Factory.New<StmMenuItemBase>();
			var pivot = stmMenuItem.Documents.AddNew();
			pivot.SI_SU = stmMenuItem.PK;
			pivot.SI_SO = template.PK;
			pivot.SI_RT_DocType = refDocType.PK;

			using (var pack = new DocumentPack(stmMenuItem))
			using (var report = new Report(pack, NewStyleTemplate))
			using (var md5 = MD5.Create())
			{
				var wrapper = new DummyFromEmailWrapperWithImageSupportAndEmail();
				((IReportForUnitTesting)report).SetBusinessObjectForTesting(wrapper);
				((IReportForUnitTesting)report).SetMenuTemplatePivotPK(pivot.PK);

				report.Name = "Test Report";
				report.StTemplate = template;

				var key = $"{pivot.PK}+{wrapper.BusinessObjectForPrintJob.PK}+Test Report+TST";

				var expectIdentifier = Convert.ToBase64String(md5.ComputeHash(Encoding.UTF8.GetBytes(key)));

				AssertEquals(expectIdentifier, report.Identifier);
			}
		}

		public void TestIdentifier_NonPersistentBusinessObjectWithISourceIdentifierProvider()
		{
			var sourceGuid = ZGuid.NewZGuid();
			var dummDummyNonPersistentBusinessObjectWithISourceIdentifierProvider1 = new DummyNonPersistentBusinessObjectWithISourceIdentifierProvider(sourceGuid);
			var dummDummyNonPersistentBusinessObjectWithISourceIdentifierProvider2 = new DummyNonPersistentBusinessObjectWithISourceIdentifierProvider(sourceGuid);

			using (var report1 = new Report(Pack, EmptyAndValidTemplate, BODocDataProvider.Get(dummDummyNonPersistentBusinessObjectWithISourceIdentifierProvider1), "Some Document", null, DocumentDirection.ANY, false))
			using (var report2 = new Report(Pack, EmptyAndValidTemplate, BODocDataProvider.Get(dummDummyNonPersistentBusinessObjectWithISourceIdentifierProvider2), "Some Document", null, DocumentDirection.ANY, false))
			{
				AssertEquals("Identifier should equal with ISourceIdentifierProvider.", report1.Identifier, report2.Identifier);
			}
		}

		public void TestIdentifier_NonPersistentBusinessObjectWithIParentDocManagerSupport()
		{
			var sourceGuid = ZGuid.NewZGuid();
			var dummyProvider1 = new DummyParentDocManagerSupport(Factory);
			var dummyProvider2 = new DummyParentDocManagerSupport(Factory);
			dummyProvider1.ParentGuidForTesting = sourceGuid;
			dummyProvider2.ParentGuidForTesting = sourceGuid;

			using (var report1 = new Report(Pack, EmptyAndValidTemplate, BODocDataProvider.Get(dummyProvider1), "Some Document", null, DocumentDirection.ANY, false))
			using (var report2 = new Report(Pack, EmptyAndValidTemplate, BODocDataProvider.Get(dummyProvider2), "Some Document", null, DocumentDirection.ANY, false))
			{
				AssertEquals("Identifier should equal with IParentDocManagerSupport.", report1.Identifier, report2.Identifier);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestInvalidCharactersExceptionShouldNotThrow()
		{
			var excelTemplate = new ExcelTemplateForUnitTesting("test.xls", TestFilesSubFolder.ReportTestFiles);
			using (var report = new Report(Pack, excelTemplate))
			{
				var renderer = new Mock<IReportRenderer>();

				renderer.Setup(m => m.Render()).Callback(delegate
				{
					throw new ArgumentException(
						"The surrogate pair (0xDA50, 0x542) is invalid. A high surrogate character (0xD800 - 0xDBFF) must always be paired with a low surrogate character (0xDC00 - 0xDFFF).");
				});

				report.Renderer = renderer.Object;

				using (var stream = new MemoryStream())
				{
					((IReportForUnitTesting)report).GenerateRegardlessOfAnyErrors = true;
					AssertNoExceptionThrown("Invalid surrogate character exception should not be thrown.", () => report.Save(stream));
					AssertEquals("report.ErrorManager.HasErrors should be true", true, report.ErrorManager.HasErrors);
				}

				renderer.VerifyAll();
			}
		}

		public void TestGeneratedFileCanBePasswordProtectedForModifying()
		{
			var content = new Dictionary<string, string>();
			content.Add("Sheet1",
	@"{A}-[#Config]
{A}-[SheetNameOverride=SheetNameOverride1]
{A}-[#EndOfReport]
");

			content.Add("Sheet2",
				@"{A}-[#Config]
{A}-[SheetNameOverride=SheetNameOverride2]
{A}-[#EndOfReport]
");
			var excelTemplate = DocumentEngineTestHelper.CreateExcelTemplateFromString("Test", string.Empty, content);

			using (Report.TemporarilyUseMainConnection())
			using (var report = new Report(Pack, excelTemplate, new DummyDocWrapper(), "Some Document", null, DocumentDirection.ANY, true))
			{
				var contact = new DocDeliveryContact(Factory);

				using (var outputStream = new MemoryStream())
				using (var excelInterface = new ExcelInterface())
				{
					report.Save(contact, contact, outputStream);
					excelInterface.LoadExcelFile(outputStream);

					AssertEquals(2, excelInterface.Xls.SheetCount);

					for (var i = 1; i <= excelInterface.Xls.SheetCount; i++)
					{
						excelInterface.Xls.ActiveSheet = i;
						AssertEquals(true, excelInterface.Xls.Protection.HasSheetPassword);
					}
				}
			}
		}

		public void TestFillDataTableThrowTimeoutExceptionShouldNotReportWhenRenderAndSave()
		{
			var template = DocumentEngineTestHelper.CreateExcelTemplateFromString("Test", string.Empty,
@"{A}-[#Config]
{A}-[Name=Test]
{A}-[Data:ReportData=select top(50000) * from dbo.orgheader join dbo.OrgContact on OC_PK != OH_PK join dbo.OrgAddress on OA_PK != OC_PK]
{A}-[#SectionBody:Data=ReportData]
{B}-[<ReportData.OH_PK>]]
{A}-[#EndOfReport]");
			var stTemplate = Factory.New<StmTemplateBase>();
			stTemplate.SO_Name = "WhatEver";
			stTemplate.SO_ExcelTemplatePath = "WhatEver.xls";
			stTemplate.SO_IsSystemDefined = true;

			var menuItem = Factory.New<StmMenuItem>();
			menuItem.SU_IsSystemDefined = true;
			menuItem.SU_MenuName = "TestReportFieldNotFoundErrorsForFullyQualifiedMacros";
			menuItem.SU_BusinessContext = "Shipment";

			using (var pack = new DocumentPack(menuItem))
			using (var stream = new MemoryStream())
			using (var report = new Report(pack, template))
			using (Report.TemporarilyStopErrorsThrowingAnException())
			{
				report.StTemplate = stTemplate;
				report.TimeOut = 1;

				report.Save(stream);
				AssertEquals(string.Empty, ErrorReporter.LastMessageReported);
			}
		}

		public void TestIdentifiablePKValid()
		{
			using (var report = new Report(Pack, null))
			{
				AssertEquals(ZGuid.Empty, report.IdentifiablePK);
				Assert("Invalid if IdentifiablePK is Empty", !report.IsIdentifiablePKValid);
			}

			var nonPersistentBizO = new NonPersistentDummy();
			using (var report = new Report(Pack, null, BODocDataProvider.Get(nonPersistentBizO), null, null, null, DocumentDirection.ANY, false))
			{
				Assert(nonPersistentBizO is NonPersistentBusinessObject);
				AssertEquals(nonPersistentBizO.PK, report.IdentifiablePK);
				Assert("Invalid if IdentifiablePK is Empty", !report.IsIdentifiablePKValid);
			}

			var dummy = Factory.NewWithValidTestData<DummyBusinessObject>();
			using (var report = new Report(Pack, null, BODocDataProvider.Get(dummy), null, null, null, DocumentDirection.ANY, false))
			{
				AssertEquals(dummy.PK, report.IdentifiablePK);
				Assert(!dummy.IsInDatabase);
				Assert("Invalid if not in Database", !report.IsIdentifiablePKValid);

				dummy.Factory.Save();
				Assert(dummy.IsInDatabase);
				Assert("Valid if in Database", report.IsIdentifiablePKValid);
			}

			var dummyWithISourceIdentifierProvider = new DummyNonPersistentBusinessObjectWithISourceIdentifierProvider(ZGuid.BrettsGuid);
			using (var report = new Report(Pack, null, BODocDataProvider.Get(dummyWithISourceIdentifierProvider), null, null, null, DocumentDirection.ANY, false))
			{
				AssertNotEquals(dummyWithISourceIdentifierProvider.PK, report.IdentifiablePK);
				Assert("Valid if in ISourceIdentifierProvider", report.IsIdentifiablePKValid);
			}
		}

		class NonPersistentDummy : NonPersistentBusinessObject { }

		#region Implementation

		IReportRenderer GetReportRendererThatThrowsDocumentEngineException()
		{
			var result = new Mock<IReportRenderer>();

			result.Setup(m => m.Render()).Callback((() =>
			{
				throw new DocumentEngineException("This is a test DocumentEngineException");
			}));

			return result.Object;
		}

		IReportRenderer GetReportRendererThatThrowsSQLExecutionException(string tableName, string commandText, string errorMessage)
		{
			var result = new Mock<IReportRenderer>();

			result.Setup(m => m.Render()).Callback((() =>
			{
				throw new SQLExecutionException(tableName, commandText, new Exception(errorMessage));
			}));

			return result.Object;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new Report(new DocumentPack(), null);
		}

		void AddFieldToList(UserControlProviderList list, string displayName, string value)
		{
			var field = new TextField(Factory);
			field.DisplayName = displayName;
			field.Value = value;
			list.Add(field);
		}

		DocumentPack Pack
		{
			get { return fPack ?? (fPack = new DocumentPack(Factory.New<StmMenuItem>())); }
		}
		DocumentPack fPack;

		void CheckOutputFile(Stream outputStream)
		{
			Assert("Output XLS file should exist!", outputStream.Length > 0);

			using (var excelInterface = new ExcelInterface())
			{
				excelInterface.LoadExcelFile(outputStream);

				AssertEquals("TestTemplate Header1                                 ", excelInterface.WorkSheets[0][1, 2].ToString());
				AssertEquals("Consignor \"test company                 ", excelInterface.WorkSheets[0][10, 2].ToString());
				AssertEquals("Unit test Line number 0                 ", excelInterface.WorkSheets[0][23, 2].ToString());
				AssertEquals("Unit test Line number 5                 ", excelInterface.WorkSheets[0][24, 2].ToString());
				AssertEquals("PageFooter ", excelInterface.WorkSheets[0][52, 2].ToString());
				AssertEquals("TestTemplate Header1                                 ", excelInterface.WorkSheets[0][53, 2].ToString());
				AssertEquals("Unit test Line number 135               ", excelInterface.WorkSheets[0][64, 2].ToString());
				AssertEquals("Unit test Line number 140               ", excelInterface.WorkSheets[0][65, 2].ToString());
			}
		}

		bool SheetVisible(string sheetName, ExcelFile xls)
		{
			SetActiveSheetByName(xls, sheetName);
			return xls.SheetVisible == FlexCel.Core.TXlsSheetVisible.Visible;
		}

		bool CurrentCompanyMacroReplaced(string sheetName, ExcelFile xls, int row, int col)
		{
			return MacroReplaced(sheetName, xls, row, col, GlbCompany.CurrentCompany.GC_Name);
		}

		bool MacroReplaced(string sheetName, ExcelFile xls, int row, int col, string expectedReplacement)
		{
			SetActiveSheetByName(xls, sheetName);
			var actualValue = (string)xls.GetCellValue(row, col);
			return actualValue == expectedReplacement;
		}

		void SetActiveSheetByName(ExcelFile xls, string sheetName)
		{
			for (var i = 1; i <= xls.SheetCount; i++)
			{
				xls.ActiveSheet = i;
				if (xls.SheetName == sheetName)
				{
					break;
				}
			}
			AssertEquals(sheetName + " should exist", true, xls.SheetName == sheetName);
		}

		void SetDocumentBranding()
		{
			DocumentsDataRegistry.Instance.EnableAgentBranding.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), System.Guid.Empty, System.Guid.Empty, true);

			var collection = new AgentDocumentBrandCollection();
			var element = (DocumentBrandingBusinessObject)collection.AddNew();
			element.CodeList.AddPair("FWD", "Desc");
			element.Code = "FWD";
			element.Description = (NoResString)"Desc";
			element.Image = new Bitmap(5, 5);
			element.BrandName = "BrandNameFromAgent";
			element.BrandEmailAddress = "agent@edi.com.au";
			DocumentsDataRegistry.Instance.AgentDocumentBrand.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), System.Guid.Empty, System.Guid.Empty, collection);
		}

		class DocImageSupportWrapperTestClass : DocBaseWrapperBaseWithImageSupport
		{
			public DocImageSupportWrapperTestClass(BusinessObject bizObj, BusinessObjectFactory factoryToWrap)
				: base(bizObj, factoryToWrap)
			{
			}
		}

		class MockRefUNLOCO : RefUNLOCO, IDocManagerSupport
		{
			DocManagerInfo docManagerInfo;

			public MockRefUNLOCO(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			#region IDocManagerSupport Members

			DocManagerInfo IDocManagerSupport.DocManagerInfo
			{
				get
				{
					if (docManagerInfo == null)
					{
						var mockInfo = new Mock<DocManagerInfo>(new object[] { this, "ABC" });
						mockInfo.Protected().Setup<bool>("ShouldRecordDocumentCore", ItExpr.IsAny<IStmMenuItem>()).Returns(false);
						docManagerInfo = mockInfo.Object;
					}
					return docManagerInfo;
				}
			}

			#endregion
		}

		class MockDummyBizo : DummyBusinessObject, IEDocsPluginHostDecider
		{
			public IBusiness HostBusinessEntity => RefUNLOCO;

			public MockDummyBizo(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public MockRefUNLOCO RefUNLOCO { get; set; }
		}

		class DummyNonPersistentBusinessObjectWithISourceIdentifierProvider : DummyNonPersistentBusinessObject, ISourceIdentifierProvider
		{
			readonly ZGuid sourceGuid;
			public DummyNonPersistentBusinessObjectWithISourceIdentifierProvider(ZGuid sourceGuid)
				: base()
			{
				this.sourceGuid = sourceGuid;
			}
			public ZGuid SourceIdentifier => sourceGuid;
		}

		class DummyExcelTemplate : ExcelTemplate
		{
			public DummyExcelTemplate(string templateName, string templateSourceLocation)
				: base(templateName, templateSourceLocation)
			{
			}

			protected override byte[] GetAsByteArrayInternal()
			{
				throw new NotImplementedException("It's a DUMMY Dude... DOH!");
			}

			protected override Stream GetAsTemplateStreamInternal()
			{
				throw new NotImplementedException("It's a DUMMY Dude... DOH!");
			}
		}

		class DummyFromEmailWrapper : DocumentWrapper
		{
			public DummyFromEmailWrapper()
				: base(new DummyFromEmailDocDataProvider(), new BusinessObjectFactory())
			{
			}

			public override string ToString()
			{
				return "Dummy Wrapper with From Email";
			}
		}

		class DummyFromEmailWrapperWithImageSupportAndEmail : DocBaseWrapperBaseWithImageSupport
		{
			public DummyFromEmailWrapperWithImageSupportAndEmail()
				: base(new DummyFromEmailDocDataProvider(), new BusinessObjectFactory())
			{
			}

			public override string ToString()
			{
				return "Dummy Wrapper with Image Support and Custom Field Email From Address";
			}
		}

		#endregion

		protected override void TearDown()
		{
			base.TearDown();
			embeddedResourceRetriever?.Dispose();
		}

		EmbeddedResourceRetriever embeddedResourceRetriever;

		ExcelTemplateForUnitTesting emptyAndValidTemplate;
		ExcelTemplateForUnitTesting EmptyAndValidTemplate
		{
			get
			{
				if (emptyAndValidTemplate == null)
				{
					embeddedResourceRetriever = new EmbeddedResourceRetriever();
					var tempFileName = embeddedResourceRetriever.SaveResourceToFile("Enterprise.DocumentEngine.Test.Testing.ReportTestFiles.EmptyAndValidTemplate.xls", "EmptyAndValidTemplate.xls");
					emptyAndValidTemplate = new ExcelTemplateForUnitTesting("EmptyAndValidTemplate.xls", Path.GetFullPath(tempFileName));
				}
				return emptyAndValidTemplate;
			}
		}

		ExcelTemplateForUnitTesting newStyleTemplate;
		ExcelTemplateForUnitTesting NewStyleTemplate
		{
			get
			{
				if (newStyleTemplate == null)
				{
					embeddedResourceRetriever = new EmbeddedResourceRetriever();
					var tempFileName = embeddedResourceRetriever.SaveResourceToFile("Enterprise.DocumentEngine.Test.Testing.ReportTestFiles.NewStyleTemplate.xls", "NewStyleTemplate.xls");
					newStyleTemplate = new ExcelTemplateForUnitTesting("NewStyleTemplate.xls", Path.GetFullPath(tempFileName));
				}
				return newStyleTemplate;
			}
		}

		void AssertRegenerateForNewRecipient(Report report, string recipientName, int row, int col)
		{
			using (var outputStream = new MemoryStream())
			{
				report.Save(new DocDeliveryContact(Factory) { Name = recipientName }, null, outputStream);
				using (var excelInterface = new ExcelInterface())
				{
					excelInterface.LoadExcelFile(outputStream);
					AssertEquals(recipientName, excelInterface.Xls.GetCellValue(row, col));
				}
			}
		}

		void AssertCacheResultForConsequentPrintings(string comment, Report report, int row, int col, DateTime expectedValue)
		{
			using (var outputStream = new MemoryStream())
			{
				report.Save(outputStream);
				using (var excelInterface = new ExcelInterface())
				{
					excelInterface.LoadExcelFile(outputStream);
					var datetime = double.Parse(excelInterface.Xls.GetCellValue(row, col).ToString());
					AssertEquals(comment, expectedValue, DateTime.FromOADate(datetime));
				}
			}
		}

		void AssertHPageBreakOverflow(string templateName)
		{
			var dummy = Factory.New<DummyBusinessObject>();
			for (var i = 0; i < 21; i++)
			{
				dummy.Collection.AddNew();
			}
			Factory.Save();

			var contact = new DocDeliveryContact(new BusinessObjectFactory())
			{
				Name = "Rick Grimes",
				DeliveryMethod = Enterprise.Core.Constants.ContactNotifyModes.Email,
				Email = "unit.test@cargowise.com",
				AttachmentType = AttachmentTypeList.Codes.Xlsx
			};

			var excelTemplate = new ExcelTemplateForUnitTesting(templateName, TestFilesSubFolder.ReportTestFiles);
			var stmTemplate = Factory.New<StmTemplateBase>();
			stmTemplate.SO_Name = "Test";
			stmTemplate.SO_Template = excelTemplate.GetAsByteArray();
			stmTemplate.SO_DataContext = nameof(DataContext.Shipment);

			var reportCommand = Factory.New<ReportCommand>();
			var pivot = reportCommand.Documents.AddNew();
			pivot.SI_SU = reportCommand.PK;
			pivot.SI_SO = stmTemplate.PK;

			using (Report.TemporarilyUseMainConnection())
			using (var pack = new DocumentPack(reportCommand))
			using (var report = Report.NewForTesting(pack))
			using (var printTask = new PrintTask())
			{
				printTask.Add(pack);

				var deliveryInstructions = new DeliveryInstructions(pack)
				{
					IsDraft = false,
					Destination = DeliveryInstructionDestination.TakenFromContact,
				};
				deliveryInstructions.Recipients.RemoveAndDeleteAll();
				deliveryInstructions.Recipients.Add(contact);

				printTask.Run(deliveryInstructions);

				var printJobs = new StmPrintJobCollection(Factory);
				printJobs.Load();
				AssertEquals("printJobs.Count", 1, printJobs.Count);

				using (var excelInterface = new ExcelInterface())
				using (var stream = new MemoryStream(printJobs[0].SP_CustomProperties))
				{
					excelInterface.LoadExcelFile(stream);
					AssertEquals("excelInterface.WorkSheets.Count", 13, excelInterface.WorkSheets.Count);

					AssertEquals("A document with a looooong name", excelInterface.WorkSheets[0].SheetName);
					AssertEquals("A document with a looooong nam3", excelInterface.WorkSheets[1].SheetName);
					AssertEquals("A document with a looooong nam4", excelInterface.WorkSheets[2].SheetName);
					AssertEquals("A document with a looooong nam5", excelInterface.WorkSheets[3].SheetName);
					AssertEquals("A document with a looooong nam6", excelInterface.WorkSheets[4].SheetName);
					AssertEquals("A document with a looooong nam8", excelInterface.WorkSheets[5].SheetName);
					AssertEquals("A document with a looooong nam9", excelInterface.WorkSheets[6].SheetName);
					AssertEquals("A document with a looooong na10", excelInterface.WorkSheets[7].SheetName);
					AssertEquals("A document with a looooong na11", excelInterface.WorkSheets[8].SheetName);
					AssertEquals("A document with a looooong na12", excelInterface.WorkSheets[9].SheetName);
					AssertEquals("A document with a looooong na13", excelInterface.WorkSheets[10].SheetName);
					AssertEquals("A document with a looooong nam2", excelInterface.WorkSheets[11].SheetName); //This is the hidden sheet from the template with a conflicting name
					AssertEquals("A document with a looooong nam7", excelInterface.WorkSheets[12].SheetName); //This is the visible sheet from the template with a conflicting name

					using (var excelInterfaceForExpectedDocument = new ExcelInterface())
					{
						var expectedFilePath = Path.Combine(BuildConstants.LocalEnterprisePath, string.Format(Culture.Invariant, @"Enterprise\Product\Documents\DocumentEngine\Testing\ExpectedDocuments\{0}", templateName));
						excelInterfaceForExpectedDocument.LoadExcelFile(expectedFilePath);
						for (var i = 0; i < 13; i++)
						{
							AssertEquals(string.Format(Culture.Invariant, "Worksheet {0} content is different from expected value", i), excelInterfaceForExpectedDocument.WorkSheets[i].ToString(true), excelInterface.WorkSheets[i].ToString(true));
						}
					}
				}
			}
		}

		void AssertTooManyRowsInDocumentExceptionHandled(int nbBusinessObjects, string attachmentType, string expectedEmailAttachmentFormat, string expectedBlobType, bool shouldHaveGeneratedAValidFile, bool isFormatSwitchExpected, string expectedErrorMessage, string expectedNotification)
		{
			//This will also fail if you have any rows in StmPrintJob before the test starts. You'll want to truncate StmPrintJob and run again before pursuing further

			try
			{
				var businessObject = Factory.New<DummyBusinessObject>();
				for (var i = 0; i < nbBusinessObjects; i++)
				{
					var child = businessObject.Collection.AddNew();
					child.Z0_Number = 1;
				}

				using (var templateStream = new MemoryStream())
				{
					var contact = new DocDeliveryContact(new BusinessObjectFactory());
					contact.Name = "Dexter";
					contact.CompanyName = "Miami Metro Police";
					contact.Address1 = "addr 1";
					contact.Address2 = "addr 2";
					contact.City = "Miami";
					contact.PostCode = "33010";
					contact.State = "FL";
					contact.UNLOCO = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "USMIA"));
					contact.DeliveryMethod = Enterprise.Core.Constants.ContactNotifyModes.Email;
					contact.Email = "unit.test@cargowise.com";
					contact.AttachmentType = attachmentType;

					using (var creationExcelInterface = new ExcelInterface())
					{
						creationExcelInterface.NewExcelFile(1);
						var workSheet = creationExcelInterface.WorkSheets[0];
						workSheet[0, 0] = "#config";
						workSheet[1, 0] = "Name=TemplateFromStream";
						workSheet[2, 0] = "PageStyle=Continuous";

						workSheet[3, 0] = "#SectionBody:Data=Collection";
						workSheet[4, 1] = "<Collection.Z0_Number>";
						workSheet[5, 1] = "<Collection.Z0_Number>";
						workSheet[6, 1] = "<Collection.Z0_Number>";
						workSheet[7, 1] = "<Collection.Z0_Number>";
						workSheet[8, 1] = "<Collection.Z0_Number>";
						workSheet[9, 1] = "<Collection.Z0_Number>";
						workSheet[10, 1] = "<Collection.Z0_Number>";
						workSheet[11, 1] = "<Collection.Z0_Number>";
						workSheet[12, 1] = "<Collection.Z0_Number>";
						workSheet[13, 1] = "<Collection.Z0_Number>";
						workSheet[14, 0] = "#SectionFooter";

						workSheet[15, 0] = "#PageFooter";
						workSheet[16, 0] = "#EndOfReport";
						creationExcelInterface.SaveToStream(templateStream, attachmentType);
					}

					var excelTemplate = new ExcelTemplateWrappingStream("TemplateFromStream", templateStream);
					var stmMenuItem = Factory.New<DocumentCommand>();
					using (var outputStream = new MemoryStream())
					using (var pack = new DocumentPack(stmMenuItem))
					using (var report = new Report(pack, excelTemplate, BODocDataProvider.Get(businessObject), "Test_" + excelTemplate.TemplateName, null, DocumentDirection.ANY, false))
					using (var printTask = new PrintTask())
					{
						printTask.Add(pack);
						pack.Add(report);

						var deliveryInstructions = new DeliveryInstructions(pack);
						deliveryInstructions.IsDraft = false;
						deliveryInstructions.Destination = DeliveryInstructionDestination.TakenFromContact;
						deliveryInstructions.Recipients.RemoveAndDeleteAll();
						deliveryInstructions.Recipients.Add(contact);

						Assert(!report.IsGenerated);

						using (Report.TemporarilyStopErrorsThrowingAnException())
						{
							AssertNoExceptionThrown(() => printTask.Run(deliveryInstructions));
						}

						var generatedReport = printTask[0][0] as Report;
						AssertEquals(shouldHaveGeneratedAValidFile, generatedReport.IsGenerated);
						AssertEquals("report.ErrorManager.HasErrors", !string.IsNullOrEmpty(expectedErrorMessage), generatedReport.ErrorManager.HasErrors);
						if (!string.IsNullOrEmpty(expectedErrorMessage))
						{
							AssertEquals("Report.Errors", string.Format(@"Severity: [Fatal Error (without error report)] Message: [{0}] Cell: [N/A] Sheetname: [(unknown)]", expectedErrorMessage),
								generatedReport.ErrorManager.ToString("Severity: [{0}] Message: [{1}] Cell: [{2}] Sheetname: [{3}]", false));
						}
						AssertEquals(expectedNotification, UnitTestUserNotification.Instance.LastMessage.Text);
						AssertEquals("DocDeliveryContact.AttachmentType should not be altered when format switching occurs", attachmentType, contact.AttachmentType);
					}

					if (shouldHaveGeneratedAValidFile)
					{
						var printJobs = new StmPrintJobCollection(Factory);
						printJobs.Load();
						AssertEquals("Pre-condition: printJobs.Length", 1, printJobs.Count);

						using (var excelInterface = new ExcelInterface())
						using (var stream = new MemoryStream(printJobs[0].SP_CustomProperties))
						{
							excelInterface.LoadExcelFile(stream);
							AssertEquals("excelInterface.WorkSheets.Count", 1, excelInterface.WorkSheets.Count);

							var workSheet = excelInterface.WorkSheets[0];

							var regex = new Regex(@"\{B\}-\[1\]\r?\n", RegexOptions.Multiline);
							AssertEquals(nbBusinessObjects * 10 - 1, regex.Matches(workSheet.ToString()).Count);
						}

						AssertEquals("Wrong SP_EmailAttachmentFormat", expectedEmailAttachmentFormat, printJobs[0].SP_EmailAttachmentFormat);
						AssertEquals("Wrong SP_EmailAttachments extension", expectedBlobType, printJobs[0].BlobType);
					}
				}
			}
			finally
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				ErrorReporter.Clear();
			}
		}

		void AssertTooManyRowsInReportExceptionHandled(int nbDummies, bool shouldHaveGeneratedAValidFile, bool isFormatSwitchExpected, string expectedErrorMessage)
		{
			try
			{
				var attachmentType = AttachmentTypeList.Codes.Csv;

				var businessObject = Factory.New<DummyBusinessObject>();
				businessObject.Z0_Number = 1;
				for (var i = 0; i < nbDummies; i++)
				{
					var child = businessObject.Collection.AddNew();
					child.Z0_Number = 1;
				}
				Factory.Save();

				var contact = new DocDeliveryContact(new BusinessObjectFactory());
				contact.Name = "Dexter";
				contact.CompanyName = "Miami Metro Police";
				contact.Address1 = "addr 1";
				contact.Address2 = "addr 2";
				contact.City = "Miami";
				contact.PostCode = "33010";
				contact.State = "FL";
				contact.UNLOCO = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "USMIA"));
				contact.DeliveryMethod = Enterprise.Core.Constants.ContactNotifyModes.Email;
				contact.Email = "unit.test@cargowise.com";
				contact.AttachmentType = attachmentType;

				var template = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "Test",
@"{A}-[#Config]
{A}-[Data:ReportData=Select DummyBizo1.Z0_Number as Number from dbo.DummyBizo as DummyBizo1 cross join dbo.DummyBizo as DummyBizo2 cross join dbo.DummyBizo as DummyBizo3]
{A}-[#SectionBody:Data=ReportData]
{B}-[<ReportData.Number>]
{A}-[#EndOfReport]");

				var reportCommand = Factory.New<ReportCommand>();
				var pivot = reportCommand.Documents.AddNew();
				pivot.SI_SU = reportCommand.PK;
				pivot.SI_SO = template.PK;

				using (Report.TemporarilyUseMainConnection())
				using (var pack = new DocumentPack(reportCommand))
				using (var report = Report.NewForTesting(pack))
				using (var printTask = new PrintTask())
				{
					printTask.Add(pack);

					var deliveryInstructions = new DeliveryInstructions(pack);
					deliveryInstructions.IsDraft = false;
					deliveryInstructions.Destination = DeliveryInstructionDestination.TakenFromContact;
					deliveryInstructions.Recipients.RemoveAndDeleteAll();
					deliveryInstructions.Recipients.Add(contact);

					Assert(!report.IsGenerated);

					using (Report.TemporarilyStopErrorsThrowingAnException())
					{
						AssertNoExceptionThrown(() => printTask.Run(deliveryInstructions));
					}

					var generatedReport = printTask[0][0] as Report;
					AssertEquals(shouldHaveGeneratedAValidFile, generatedReport.IsGenerated);
					AssertEquals("report.ErrorManager.HasErrors", !string.IsNullOrEmpty(expectedErrorMessage), generatedReport.ErrorManager.HasErrors);
					if (!string.IsNullOrEmpty(expectedErrorMessage))
					{
						AssertEquals("Report.Errors", string.Format(@"Severity: [Fatal Error (without error report)] Message: [{0}] Cell: [N/A] Sheetname: [(unknown)]", expectedErrorMessage),
							generatedReport.ErrorManager.ToString("Severity: [{0}] Message: [{1}] Cell: [{2}] Sheetname: [{3}]", false));
					}
					AssertEquals("DocDeliveryContact.AttachmentType should not be altered when format switching occurs", attachmentType, contact.AttachmentType);
				}

				if (shouldHaveGeneratedAValidFile)
				{
					var printJobs = new StmPrintJobCollection(Factory);
					printJobs.Load();
					AssertEquals("Pre-condition: printJobs.Length", 1, printJobs.Count);

					var textFromFile = printJobs[0].SP_CustomProperties.ToUTF8().ToString().TrimWithUnicodeWhitespace();
					var regex = new Regex(@"""1""\r?\n", RegexOptions.Multiline);
					//nbDummies + 1 is for number of child DummyBizo + 1 parent
					//Pow^3 is for the SQL Select on DummyBizo with 2 cross join on itself
					//- 1 is because the last line has no line break
					var expectedNumberOfLines = (int)Math.Pow(nbDummies + 1, 3) - 1;
					AssertEquals(expectedNumberOfLines, regex.Matches(textFromFile).Count);
					AssertEquals("Wrong SP_EmailAttachmentFormat", attachmentType, printJobs[0].SP_EmailAttachmentFormat);
					AssertEquals("Wrong SP_EmailAttachments extension", attachmentType, printJobs[0].BlobType);
				}
			}
			finally
			{
				ErrorReporter.Clear();
			}
		}

		void AssertTooManyColumnsExceptionHandled(string attachmentType, string expectedEmailAttachmentFormat, string expectedBlobType, bool shouldHaveGeneratedAValidFile, bool isFormatSwitchExpected, string expectedNotification)
		{
			try
			{
				var businessObject = Factory.New<DummyBusinessObject>();
				var child = businessObject.Collection.AddNew();
				child.Z0_Number = 1;

				using (var templateStream = new MemoryStream())
				{
					var contact = new DocDeliveryContact(new BusinessObjectFactory());
					contact.Name = "Dexter";
					contact.CompanyName = "Miami Metro Police";
					contact.Address1 = "addr 1";
					contact.Address2 = "addr 2";
					contact.City = "Miami";
					contact.PostCode = "33010";
					contact.State = "FL";
					contact.UNLOCO = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "USMIA"));
					contact.DeliveryMethod = Enterprise.Core.Constants.ContactNotifyModes.Email;
					contact.Email = "unit.test@cargowise.com";
					contact.AttachmentType = attachmentType;

					using (var creationExcelInterface = new ExcelInterface())
					{
						creationExcelInterface.NewExcelFile(1, TExcelFileFormat.v2007);
						var workSheet = creationExcelInterface.WorkSheets[0];
						workSheet[0, 0] = "#config";
						workSheet[1, 0] = "Name=TemplateFromStream";
						workSheet[2, 0] = "PageStyle=Portrait";

						workSheet[3, 0] = "#SectionBody:Data=Collection";
						workSheet[4, FlxConsts.Max_Columns97_2003 + 10] = "<Collection.Z0_Number>";
						workSheet[5, 0] = "#SectionFooter";

						workSheet[6, 0] = "#PageFooter";
						workSheet[7, 0] = "#EndOfReport";
						creationExcelInterface.SaveToStream(templateStream, ExcelFileFormatOptionList.Codes.XLSX);
					}

					var excelTemplate = new ExcelTemplateWrappingStream("TemplateFromStream", templateStream);
					var stmMenuItem = Factory.New<DocumentCommand>();
					using (var pack = new DocumentPack(stmMenuItem))
					using (var report = new Report(pack, excelTemplate, BODocDataProvider.Get(businessObject), "Test_" + excelTemplate.TemplateName, null, DocumentDirection.ANY, false))
					using (var printTask = new PrintTask())
					{
						printTask.Add(pack);
						pack.Add(report);

						var deliveryInstructions = new DeliveryInstructions(pack);
						deliveryInstructions.IsDraft = false;
						deliveryInstructions.Destination = DeliveryInstructionDestination.TakenFromContact;
						deliveryInstructions.Recipients.RemoveAndDeleteAll();
						deliveryInstructions.Recipients.Add(contact);

						Assert(!report.IsGenerated);
						AssertNoExceptionThrown(() => printTask.Run(deliveryInstructions));
						AssertEquals(shouldHaveGeneratedAValidFile, (printTask[0][0] as Report).IsGenerated);
						AssertEquals("report.ErrorManager.HasErrors is false", false, report.ErrorManager.HasErrors);
						AssertEquals(expectedNotification, UnitTestUserNotification.Instance.LastMessage.Text);
						AssertEquals("DocDeliveryContact.AttachmentType should not be altered when format switching occurs", attachmentType, contact.AttachmentType);
					}

					if (shouldHaveGeneratedAValidFile)
					{
						var printJobs = new StmPrintJobCollection(Factory);
						printJobs.Load();
						AssertEquals("Pre-condition: printJobs.Length", 1, printJobs.Count);

						using (var excelInterface = new ExcelInterface())
						using (var stream = new MemoryStream(printJobs[0].SP_CustomProperties))
						{
							excelInterface.LoadExcelFile(stream);
							AssertEquals("excelInterface.WorkSheets.Count", 1, excelInterface.WorkSheets.Count);
						}

						AssertEquals("Wrong SP_EmailAttachmentFormat", expectedEmailAttachmentFormat, printJobs[0].SP_EmailAttachmentFormat);
						AssertEquals("Wrong SP_EmailAttachments extension", expectedBlobType, printJobs[0].BlobType);
					}
				}
			}
			finally
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			}
		}

		internal static Dictionary<string, string> GetApplicationLoginModifiedDates()
		{
			var allApplicationLogins = Db.GetAllLoginNames(Db.DatabaseName);
			var loginModifyDates = new Dictionary<string, string>();
			foreach (var applicationLogin in allApplicationLogins)
			{
				var modifyDate = Db.Connection.ExecuteScalar($"SELECT modify_date FROM sys.database_principals where name = '{applicationLogin}'");
				loginModifyDates.Add(applicationLogin, modifyDate.ToString());
			}

			return loginModifyDates;
		}

		internal static ExcelTemplateWrappingStream CreateTestRunningConnectionTemplate(MemoryStream templateStream)
		{
			using (var creationExcelInterface = new ExcelInterface())
			{
				creationExcelInterface.NewExcelFile(1);
				var workSheet = creationExcelInterface.WorkSheets[0];
				workSheet[0, 0] = "#config";
				workSheet[1, 0] = "Name=RunningConnectionTemplate";
				workSheet[2, 0] = "data:test=SELECT @@SPID AS SessionId, SERVERPROPERTY('ServerName') AS ServerName, DB_NAME() AS DatabaseName";
				workSheet[3, 0] = "#SectionBody:data=test";
				workSheet[4, 1] = "<test.SessionId>";
				workSheet[4, 2] = "<test.ServerName>";
				workSheet[4, 3] = "<test.DatabaseName>";
				workSheet[5, 0] = "#endofreport";
				creationExcelInterface.SaveToStream(templateStream);
			}

			var excelTemplate = new ExcelTemplateWrappingStream("RunningConnectionTemplate", templateStream);
			return excelTemplate;
		}

		internal static ExcelTemplateWrappingStream CreateTestRunningConnectionTemplateWithUsername(MemoryStream templateStream)
		{
			using (var creationExcelInterface = new ExcelInterface())
			{
				creationExcelInterface.NewExcelFile(1);
				var workSheet = creationExcelInterface.WorkSheets[0];
				workSheet[0, 0] = "#config";
				workSheet[1, 0] = "Name=TestRunningConnectionTemplate_Impersonate";
				workSheet[2, 0] = "data:test=SELECT @@SPID AS SessionId, SERVERPROPERTY('ServerName') AS ServerName, DB_NAME() AS DatabaseName, current_user AS Username";
				workSheet[3, 0] = "#SectionBody:data=test";
				workSheet[4, 1] = "<test.SessionId>";
				workSheet[4, 2] = "<test.ServerName>";
				workSheet[4, 3] = "<test.DatabaseName>";
				workSheet[4, 4] = "<test.Username>";
				workSheet[5, 0] = "#endofreport";
				creationExcelInterface.SaveToStream(templateStream);
			}

			var excelTemplate = new ExcelTemplateWrappingStream("RunningConnectionTemplate_Impersonate", templateStream);
			return excelTemplate;
		}

		internal static ExcelTemplateWrappingStream CreateTestRunningConnectionTemplateWithDboTable(MemoryStream templateStream)
		{
			using (var creationExcelInterface = new ExcelInterface())
			{
				creationExcelInterface.NewExcelFile(1);
				var workSheet = creationExcelInterface.WorkSheets[0];
				workSheet[0, 0] = "#config";
				workSheet[1, 0] = "Name=TestRunningConnectionTemplate_Impersonate";
				workSheet[2, 0] = "data:test=SELECT COUNT (*) as TotalGlbStaffs FROM [dbo].[GlbStaff]";
				workSheet[3, 0] = "#SectionBody:data=test";
				workSheet[4, 1] = "<test.TotalGlbStaffs>";
				workSheet[5, 0] = "#endofreport";
				creationExcelInterface.SaveToStream(templateStream);
			}

			var excelTemplate = new ExcelTemplateWrappingStream("RunningConnectionTemplatee", templateStream);
			return excelTemplate;
		}

		internal static ExcelTemplateWrappingStream CreateTestRunningConnectionTemplateWithHRMTable(MemoryStream templateStream)
		{
			using (var creationExcelInterface = new ExcelInterface())
			{
				creationExcelInterface.NewExcelFile(1);
				var workSheet = creationExcelInterface.WorkSheets[0];
				workSheet[0, 0] = "#config";
				workSheet[1, 0] = "Name=TestRunningConnectionTemplate_Impersonate";
				workSheet[2, 0] = "data:test=SELECT COUNT (*) as TotalGlbStaffEntitlements FROM [hrm].[GlbStaffEntitlement]";
				workSheet[3, 0] = "#SectionBody:data=test";
				workSheet[4, 1] = "<test.TotalGlbStaffEntitlements>";
				workSheet[5, 0] = "#endofreport";
				creationExcelInterface.SaveToStream(templateStream);
			}

			var excelTemplate = new ExcelTemplateWrappingStream("RunningConnectionTemplate_Impersonate", templateStream);
			return excelTemplate;
		}

		internal static void AssertReportResults(MemoryStream outputStream, int expectedSpid, string expectedServerName, string expectedDbName)
		{
			var expectedOutput = "{B}-[" + expectedSpid.ToString() + "]   {C}-[" + expectedServerName + "]   {D}-[" + expectedDbName + "]";

			using (var xlInterface = new ExcelInterface())
			{
				xlInterface.LoadExcelFile(outputStream);
				var workSheet = xlInterface.WorkSheets[0];
				AssertMultilineASCIIEquals(
					"Expected Output: {B}-[SQL_SESSION_ID]   {C}-[SERVER_NAME]   {D}-[DATABASE_NAME]",
					expectedOutput, workSheet.ToString());
			}
		}

		internal static void AssertReportResults(MemoryStream outputStream, int expectedSpid, string expectedServerName, string expectedDbName, string expectedUsername)
		{
			var expectedOutput = "{B}-[" + expectedSpid.ToString() + "]   {C}-[" + expectedServerName + "]   {D}-[" + expectedDbName + "]   {E}-[" + expectedUsername + "]";

			using (var xlInterface = new ExcelInterface())
			{
				xlInterface.LoadExcelFile(outputStream);
				var workSheet = xlInterface.WorkSheets[0];
				AssertMultilineASCIIEquals(
					"Expected Output: {B}-[SQL_SESSION_ID]   {C}-[SERVER_NAME]   {D}-[DATABASE_NAME]   {E}-[Username]",
					expectedOutput, workSheet.ToString());
			}
		}

		const int nbBusinessObjectsToFail2003 = 7000;
		const int nbBusinessObjectsToFail2007 = 150000;
		const int nbDummiesToFail2003 = 40;
		const int nbDummiesToFail2007 = 103;

		sealed class DummyParentDocManagerSupport : NonPersistentBusinessObject, IParentDocManagerSupport
		{
			public DummyParentDocManagerSupport(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			public ZGuid ParentGuid => ParentGuidForTesting;

			public ZGuid ParentGuidForTesting;

			public ZString ParentTableName => ParentTableNameForTesting;

			public ZString ParentTableNameForTesting;

			DocManagerInfo docManagerInfo;
			public DocManagerInfo DocManagerInfo => docManagerInfo ?? (docManagerInfo = new DocManagerInfo(this, "ZZZ"));
		}

		sealed class PrintTaskUIProviderForTesting : IPrintTaskUIProvider
		{
			public bool ShowRuntimeOptionsUI(PrintTask printTask, AllowedDeliveryOptions deliveryOptions, DeliveryInstructions instructions, ISecurityCheckpoint modifyDocumentCheckPoint)
			{
				throw new NotImplementedException();
			}

			public bool ShowPrintTaskDeliveryUI(PrintTaskSettings taskSettings) => throw new NotImplementedException();

			public IProgressNotificationUI GetNewProgressNotificationUI(PrintTaskSettings taskSettings) => throw new NotImplementedException();

			public IProgressNotificationUI GetNewProgressNotificationUI(DeliveryInstructions instructions, int totalPacks)
			{
				throw new NotImplementedException();
			}

			public bool ShowDocDeliveryUI(DeliveryInstructions instructions, ISecurityCheckpoint modifyDocumentCheckPoint)
			{
				throw new NotImplementedException();
			}

			public bool ShowDocDeliveryUI(PrintTask printTask, DeliveryInstructions instructions, ISecurityCheckpoint modifyDocumentCheckPoint)
			{
				throw new NotImplementedException();
			}

			public void ShowPrinterSelectionUI(DeliveryInstructions deliveryInstructions) => throw new NotImplementedException();

			public void ShowPreview(Stream xlsStream, DeliveryInfo[] deliveryInfos, DocumentDelivery.IDeliverCapableForm parentForm)
			{
				throw new NotImplementedException();
			}

			public bool ShowErrorsCalled;
			public bool CanFormBeCreatedDuringDbTransaction;

			public bool ShowErrors(Report report)
			{
				ShowErrorsCalled = true;
				CanFormBeCreatedDuringDbTransaction = ObjectFactory.Get<INeedToShowMessage>().CanFormBeCreatedDuringDbTransaction(null);
				return true;
			}

			public void ResetShowErrors()
			{
				ShowErrorsCalled = false;
				CanFormBeCreatedDuringDbTransaction = false;
			}

			public void ShowWarning(string caption, string message) => throw new NotImplementedException();
		}
	}
}
