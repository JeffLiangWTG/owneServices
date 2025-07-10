using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentEngine.Business;
using Enterprise.DocumentEngine.DocBuilder;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.DocumentEngine.Testing.UtilityClasses;
using Enterprise.Environment;
using Enterprise.ExcelTemplates;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using FlexCel.Core;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.ReportErrorManagement.Testing
{
	sealed class ReportErrorManagerTest : TestCaseWithFactory
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestReportError_ShouldAdjustWarningLevelForAnyCustomisations()
		{
			var excelTemplate = new ExcelTemplateForUnitTesting("SimpleTest.xls", TestFilesSubFolder.ReportTestFiles);
			var menuItem = Factory.New<StmMenuItem>();
			menuItem.SU_IsSystemDefined = false;
			using (var documentPack = new DocumentPack(menuItem))
			using (var report = new Report(documentPack, excelTemplate))
			{
				Assert("Report should contain customisations (menu item is non-system defined)", report.ContainsAnyCustomisation);

				var errorManager = new ReportErrorManager(report);
				errorManager.Add(new ReportProcessingError("Ignorable error", ReportProcessingErrorSeverity.Warning));

				var error = ((IHaveReportProcessingErrorsForGUI)errorManager).GetErrors()[0];
				AssertEquals("Should have changed error severity to not report errors", ReportProcessingErrorSeverity.WarningWithoutErrorReport, error.Severity);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestReportError_ShouldAdjustErrorLevelForAnyCustomisations()
		{
			var excelTemplate = new ExcelTemplateForUnitTesting("SimpleTest.xls", TestFilesSubFolder.ReportTestFiles);
			var menuItem = Factory.New<StmMenuItem>();
			menuItem.SU_IsSystemDefined = false;
			using (var documentPack = new DocumentPack(menuItem))
			using (var report = new Report(documentPack, excelTemplate))
			{
				Assert("Report should contain customisations (menu item is non-system defined)", report.ContainsAnyCustomisation);

				var errorManager = new ReportErrorManager(report);
				errorManager.Add(new ReportProcessingError("Ignorable error", ReportProcessingErrorSeverity.Error));

				var error = ((IHaveReportProcessingErrorsForGUI)errorManager).GetErrors()[0];
				AssertEquals("Should have changed error severity to not report errors", ReportProcessingErrorSeverity.ErrorWithoutErrorReport, error.Severity);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestReportError_ForSystemDefinedReport_ShouldNotAdjustWarningLevel()
		{
			var excelTemplate = new ExcelTemplateForUnitTesting("SimpleTest.xls", TestFilesSubFolder.ReportTestFiles);
			var menuItem = Factory.New<StmMenuItem>();
			menuItem.SU_IsSystemDefined = true;
			using (var documentPack = new DocumentPack(menuItem))
			using (var report = new Report(documentPack, excelTemplate))
			{
				AssertEquals("Report should NOT contain customisations", false, report.ContainsAnyCustomisation);

				var errorManager = new ReportErrorManager(report);
				errorManager.Add(new ReportProcessingError("Non-ignorable error", ReportProcessingErrorSeverity.Warning));

				var error = ((IHaveReportProcessingErrorsForGUI)errorManager).GetErrors()[0];
				AssertEquals("Should NOT have changed error severity", ReportProcessingErrorSeverity.Warning, error.Severity);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestReportErrorsShouldNotSendToEnterpriseBatchProcessor()
		{
			var excelTemplate = new ExcelTemplateForUnitTesting("SimpleTest.xls", TestFilesSubFolder.ReportTestFiles);
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_EmailAddress = "unit.test@cargowise.com";

			using (var documentPack = new DocumentPack())
			using (var report = new Report(documentPack, excelTemplate))
			{
				var scheduleTask = Factory.NewWithValidTestData<ReportScheduleTask>();
				scheduleTask.S5_ScheduleDescription = "My Scheduled Report";

#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				var log = scheduleTask.Logs.AddNew(Events.EditedARecord, ZDateTimeOffset.Now.AddDays(1));
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				log.SL_GS_NKUser = staff.GS_Code;

#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				var bpLog = scheduleTask.Logs.AddNew(Events.EditedARecord, ZDateTimeOffset.Now.AddDays(2));
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				bpLog.SL_GS_NKUser = User.ServiceUserCode;

				Factory.Save();

				report.StTemplate = Factory.NewWithValidTestData<StmTemplateBase>();
				report.StTemplate.SO_Template = excelTemplate.GetAsByteArray();
				report.SetScheduleTask(scheduleTask);
				report.ErrorManager.Add(new ReportFilterValidationError("My Error", ReportProcessingErrorSeverity.Error));
				report.ErrorManager.ReportErrors();

				AssertEquals("Emails to Client", 1, Env.OutgoingMailManager.EmailsCreated.Count);
				EmailDef email = Env.OutgoingMailManager.EmailsCreated[0];
				AssertEquals("Email.Recipients should match", "postmaster@sample.org", email.Recipients.RecipientsAsDelimitedString());
				AssertEquals("Email.CCRecipients should match", "unit.test@cargowise.com", email.CCRecipients.RecipientsAsDelimitedString());
				AssertEquals("Error Running Scheduled Report [My Scheduled Report]", email.Subject);

				Env.OutgoingMailManager.EmailsCreated.Clear();
				ErrorReporter.Clear();
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestReportError_ShouldNotThrowInsertPermissionDeniedException()
		{
			var excelTemplate = new ExcelTemplateForUnitTesting("SimpleTest.xls", TestFilesSubFolder.ReportTestFiles);
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_EmailAddress = "unit.test@cargowise.com";
			staff.GS_IsActive = false;

			using (var documentPack = new DocumentPack())
			using (var report = new Report(documentPack, excelTemplate))
			{
				report.StTemplate = Factory.NewWithValidTestData<StmTemplateBase>();
				report.StTemplate.SO_Template = excelTemplate.GetAsByteArray();
				report.ErrorManager.Add(new ReportFilterValidationError("My Error: " + DbCommand.ExecuteAsReaderFlagComments, ReportProcessingErrorSeverity.Error));
				report.ErrorManager.ReportErrors();

				AssertEquals("Emails to Client", 1, Env.OutgoingMailManager.EmailsCreated.Count);

				var body = Env.OutgoingMailManager.EmailsCreated[0].Body;
				var executeAsReaderFlag = DbCommand.ExecuteAsReaderFlagComments.Replace("\n", "").Trim();
				AssertNotContains(executeAsReaderFlag, body);
				AssertContains(DbCommand.ExecuteAsReaderFlagMask, body);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestReportErrorsShouldNotSendToInactiveUser()
		{
			var excelTemplate = new ExcelTemplateForUnitTesting("SimpleTest.xls", TestFilesSubFolder.ReportTestFiles);
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_EmailAddress = "unit.test@cargowise.com";
			staff.GS_IsActive = false;

			using (var documentPack = new DocumentPack())
			using (var report = new Report(documentPack, excelTemplate))
			{
				var scheduleTask = Factory.NewWithValidTestData<ReportScheduleTask>();
				scheduleTask.S5_ScheduleDescription = "My Scheduled Report";

#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				var log = scheduleTask.Logs.AddNew(Events.EditedARecord, ZDateTimeOffset.Now.AddDays(1));
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				log.SL_GS_NKUser = staff.GS_Code;

				Factory.Save();

				report.StTemplate = Factory.NewWithValidTestData<StmTemplateBase>();
				report.StTemplate.SO_Template = excelTemplate.GetAsByteArray();
				report.SetScheduleTask(scheduleTask);
				report.ErrorManager.Add(new ReportFilterValidationError("My Error", ReportProcessingErrorSeverity.Error));
				report.ErrorManager.ReportErrors();

				AssertEquals("Emails to Client", 1, Env.OutgoingMailManager.EmailsCreated.Count);
				EmailDef email = Env.OutgoingMailManager.EmailsCreated[0];
				AssertEquals("Email.Recipients should match", "postmaster@sample.org", email.Recipients.RecipientsAsDelimitedString());
				AssertEquals("Email.CCRecipients should match", "", email.CCRecipients.RecipientsAsDelimitedString());
				AssertEquals("Error Running Scheduled Report [My Scheduled Report]", email.Subject);

				Env.OutgoingMailManager.EmailsCreated.Clear();
				ErrorReporter.Clear();
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestSqlExceptionKey()
		{
			using (Report report = GetNewReportWithMenuItemAndStmTemplateSetup())
			{
				report.Parent.IsRunFromMenusCustomisationForm = true;
				AssertEquals("Pre-Condition: report.errorManager.HasErrors is false", false, report.ErrorManager.HasErrors);
				report.ErrorManager.ReportFatalException(new SQLExecutionException("tableName", "commandText", SqlExceptionBuilder.CreateSqlException(512,
					"Subquery returned more than 1 value. This is not permitted when the subquery follows =, !=, <, <= , >, >= or when the subquery is used as an expression.")));
				report.ErrorManager.ReportErrors();
				AssertEquals("Rpt.Errors", "Severity: [Fatal] Message: [Error loading table [tableName]. Error: [Subquery returned more than 1 value. This is not permitted when the subquery follows =, !=, <, <= , >, >= or when the subquery is used as an expression.] occurred running SQL: [commandText]. SqlException: Msg 512, Level 0, State 255, Line 0, Subquery returned more than 1 value. This is not permitted when the subquery follows =, !=, <, <= , >, >= or when the subquery is used as an expression.] Cell: [N/A]"
					, report.ErrorManager.ToString("Severity: [{0}] Message: [{1}] Cell: [{2}]", false));
				Assert(report.ErrorManager.GetKey(), report.ErrorManager.GetKey().Contains(
					@"Severity: [Fatal] Message: [Error loading table [tableName]. Error: [Subquery returned more than 1 value. This is not permitted when the subquery follows =, !=, <, <= , >, >= or when the subquery is used as an expression.] occurred running SQL: [(snipped)]. SqlException: Msg #, Level #, State #, Line #, Subquery returned more than 1 value. This is not permitted when the subquery follows =, !=, <, <= , >, >= or when the subquery is used as an expression.] Sheetname: [(unknown)] Cell Content: []"
					));
				AssertNull(ErrorReporter.LastExceptionReported);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestDoNotReportErrorIfRunFromMenusCustomisationForm()
		{
			using (Report report = GetNewReportWithMenuItemAndStmTemplateSetup())
			{
				report.Parent.IsRunFromMenusCustomisationForm = true;
				AssertEquals("Pre-Condition: report.errorManager.HasErrors is false", false, report.ErrorManager.HasErrors);
				report.ErrorManager.ReportFatalException(new DocumentEngineException("Template definition error"));
				report.ErrorManager.ReportErrors();
				AssertEquals("Rpt.Errors", "Severity: [Fatal] Message: [Template definition error] Cell: [N/A]", report.ErrorManager.ToString("Severity: [{0}] Message: [{1}] Cell: [{2}]", false));
				AssertNull(ErrorReporter.LastExceptionReported);
			}

			using (Report report = new Report(null, null))
			{
				try
				{
					AssertEquals("Pre-Condition: report.errorManager.HasErrors is false", false, report.ErrorManager.HasErrors);
					report.ErrorManager.ReportFatalException(new DocumentEngineException("Template definition error"));
					report.ErrorManager.ReportErrors();
					AssertEquals("Rpt.Errors", "Severity: [Fatal] Message: [Template definition error] Cell: [N/A]", report.ErrorManager.ToString("Severity: [{0}] Message: [{1}] Cell: [{2}]", false));
					AssertNotNull(ErrorReporter.LastExceptionReported);
				}
				finally
				{
					ErrorReporter.Clear();
				}
			}
		}

		public void TestOutermostCellContentIsTheOneThatGetsRecorded()
		{
			ReportErrorManager errorManager = new ReportErrorManager(new Report(null, null));
			using (errorManager.EvaluatingOuterContent("<Outer.Content>"))
			{
				ReportProcessingError newError = new ReportProcessingError("Random Error Message 1", ReportProcessingErrorSeverity.Error);
				errorManager.Add(newError);
				AssertEquals("error.OuterContent for basic macro", "<Outer.Content>", newError.OuterContent);
			}

			using (errorManager.EvaluatingOuterContent("<Outer.Content>"))
			using (errorManager.EvaluatingOuterContent("<Inner.Content>"))
			{
				ReportProcessingError newError = new ReportProcessingError("Random Error Message 2", ReportProcessingErrorSeverity.Error);
				errorManager.Add(newError);
				AssertEquals("error.OuterContent for basic macro", "<Outer.Content>", newError.OuterContent);
			}

			ReportProcessingError error = new ReportProcessingError("Random Error Message 3", ReportProcessingErrorSeverity.Error);
			errorManager.Add(error);
			AssertEquals("error.OuterContent for basic macro", null, error.OuterContent);

			using (errorManager.EvaluatingOuterContent("<Outer.Content>"))
			{
				error = new ReportProcessingError("Random Error Message 4", ReportProcessingErrorSeverity.Error);
				errorManager.Add(error);
				AssertEquals("error.OuterContent for basic macro", "<Outer.Content>", error.OuterContent);

				using (errorManager.EvaluatingOuterContent("<Inner.Content>"))
				{
					error = new ReportProcessingError("Random Error Message 5", ReportProcessingErrorSeverity.Error);
					errorManager.Add(error);
					AssertEquals("error.OuterContent for basic macro", "<Outer.Content>", error.OuterContent);

					using (errorManager.EvaluatingOuterContent("<Inner-Inner.Content>"))
					{
						error = new ReportProcessingError("Random Error Message 6", ReportProcessingErrorSeverity.Error);
						errorManager.Add(error);
						AssertEquals("error.OuterContent for basic macro", "<Outer.Content>", error.OuterContent);
					}

					error = new ReportProcessingError("Random Error Message 7", ReportProcessingErrorSeverity.Error);
					errorManager.Add(error);
					AssertEquals("error.OuterContent for basic macro", "<Outer.Content>", error.OuterContent);
				}

				error = new ReportProcessingError("Random Error Message 8", ReportProcessingErrorSeverity.Error);
				errorManager.Add(error);
				AssertEquals("error.OuterContent for basic macro", "<Outer.Content>", error.OuterContent);
			}

			error = new ReportProcessingError("Random Error Message 9", ReportProcessingErrorSeverity.Error);
			errorManager.Add(error);
			AssertEquals("error.OuterContent for basic macro", null, error.OuterContent);
		}

		public void TestOuterCellContentHandlesExcelFunctionsAndTruncatesLongContentTo250Characters()
		{
			ReportErrorManager errorManager = new ReportErrorManager(new Report(null, null));
			using (errorManager.EvaluatingOuterContent("<test.SessionId>"))
			{
				ReportProcessingError error = new ReportProcessingError("Random Error Message 1", ReportProcessingErrorSeverity.Error);
				errorManager.Add(error);
				AssertEquals("error.OuterContent for basic macro", "<test.SessionId>", error.OuterContent);
			}

			#region string reallyLongCellText
			string reallyLongCellText = @"<IF(""<ETA.FromDate>""=="""","""",""Filters: ETA <ETA> "")>
<IF(""<Shipment Transport>""=="""","""",""Shipment Transport: <Shipment Transport> "")>
<IF(""<Declaration Transport>""=="""","""",""Declaration Transport: <Declaration Transport> "")>
<IF(""<Vessel/Journey>""=="""","""",""Vessel: <VesselData.Code> "")>
<IF(""<Flight/Voyage>""=="""","""",""Flight/Voyage: <Flight/Voyage> "")>
<IF(""<Discharge Port>""=="""","""",""Discharge Port: <Discharge Port> "")>
<IF(""<Destination>""=="""","""",""Destination: <Destination> "")>
<IF(""<Importer/C'nee>""=="""","""",""Consignee/Importer: <Importer/C'nee> "")>
<IF(""<Exporter/Consignor>""=="""","""",""Consignor: <Exporter/Consignor> "")>
<IF(""<Cartage Company>""=="""","""",""Cartage Company: <CartageData.Name> "")>
<IF(""<Shipments & Declarations>""==""All"","""",""Job Type: <Shipments & Declarations> "")>
<IF(""<Delivery Status>""==""OPN"","""",""Delivery Status: <Delivery Status>"")>";
			#endregion

			using (errorManager.EvaluatingOuterContent(reallyLongCellText))
			{
				ReportProcessingError error = new ReportProcessingError("Random Error Message 2", ReportProcessingErrorSeverity.Error);
				errorManager.Add(error);
				AssertEquals("OuterContent length should be restricted to 250 characters", 250, error.OuterContent.Length);
				AssertMultilineASCIIEquals("error.OuterContent for looooong macro", @"<IF(""<ETA.FromDate>""=="""","""",""Filters: ETA <ETA> "")>|><IF(""<Shipment Transport>""=="""","""",""Shipment Transport: <Shipment Transport> "")>|><IF(""<Declaration Transport>""=="""","""",""Declaration Transport:  ... ""==""OPN"","""",""Delivery Status: <Delivery Status>"")>", error.OuterContent);
			}

			using (errorManager.EvaluatingOuterContent(new TFormula(@"=IF(""<Z0_FieldDoesNotExistInAFitOfBlueCheese>""=""Fred"",""Yes"",""<Z0_VarCharMax>"")")))
			{
				ReportProcessingError error = new ReportProcessingError("Random Error Message 3", ReportProcessingErrorSeverity.Error);
				errorManager.Add(error);
				AssertEquals("error.OuterContent for cell containing formula", @"=IF(""<Z0_FieldDoesNotExistInAFitOfBlueCheese>""=""Fred"",""Yes"",""<Z0_VarCharMax>"")", error.OuterContent);
			}
		}

		public void TestDuplicateErrorsIncrementTheCountOnTheOriginalErrorInsteadOfAddingANewError()
		{
			ReportErrorManager errorManager = new ReportErrorManager(new Report(null, null));
			AssertEquals("Pre-Condition: errorManager.HasErrors is false", false, errorManager.HasErrors);
			using (errorManager.EvaluatingOuterContent("<hello world>"))
			{
				errorManager.Add(new ReportProcessingError("This is a test message 1", new CellReference("Sheet1", 4, 2), ReportProcessingErrorSeverity.Error));
				errorManager.Add(new ReportProcessingError("This is a test message 1", new CellReference("Sheet1", 4, 3), ReportProcessingErrorSeverity.Error));
				errorManager.Add(new ReportProcessingError("This is a test message 1", new CellReference("Sheet1", 4, 4), ReportProcessingErrorSeverity.Error));
				AssertEquals("Report.Errors", @"
Severity: [Error] Message: [This is a test message 1] Cell: [C5] Sheetname: [Sheet1] Cell Content: [<hello world>] Occurences: [3]
".Trim(), errorManager.ToString("Severity: [{0}] Message: [{1}] Cell: [{2}] Sheetname: [{3}] Cell Content: [{5}] Occurences: [{6}]", false));
				errorManager.ClearErrors();
			}

			using (errorManager.EvaluatingOuterContent("<hello world>"))
			{
				errorManager.Add(new ReportProcessingError("This is a test message 1", new CellReference("Sheet1", 4, 2), ReportProcessingErrorSeverity.Error));
				errorManager.Add(new ReportProcessingError("This is a test message 2", new CellReference("Sheet2", 4, 3), ReportProcessingErrorSeverity.Error));
				errorManager.Add(new ReportProcessingError("This is a test message 1", new CellReference("Sheet1", 4, 4), ReportProcessingErrorSeverity.Error));
				AssertEquals("Report.Errors", @"
Severity: [Error] Message: [This is a test message 1] Cell: [C5] Sheetname: [Sheet1] Cell Content: [<hello world>] Occurences: [2]
Severity: [Error] Message: [This is a test message 2] Cell: [D5] Sheetname: [Sheet2] Cell Content: [<hello world>] Occurences: [1]
".Trim(), errorManager.ToString("Severity: [{0}] Message: [{1}] Cell: [{2}] Sheetname: [{3}] Cell Content: [{5}] Occurences: [{6}]", false));
				errorManager.ClearErrors();
			}

			using (errorManager.EvaluatingOuterContent("<hello world>"))
			{
				errorManager.Add(new ReportProcessingError("This is a test message 1", new CellReference("Sheet1", 4, 2), ReportProcessingErrorSeverity.Error));
			}
			using (errorManager.EvaluatingOuterContent("<world hello>"))
			{
				using (errorManager.EvaluatingOuterContent("<eats small shorts>"))
				{
					errorManager.Add(new ReportProcessingError("This is a test message 1", new CellReference("Sheet1", 4, 3), ReportProcessingErrorSeverity.Error));
					errorManager.Add(new ReportProcessingError("This is a test message 1", new CellReference("Sheet1", 4, 4), ReportProcessingErrorSeverity.Error));
				}
			}
			AssertEquals("Report.Errors", @"
Severity: [Error] Message: [This is a test message 1] Cell: [C5] Sheetname: [Sheet1] Cell Content: [<hello world>] Occurences: [1]
Severity: [Error] Message: [This is a test message 1] Cell: [D5] Sheetname: [Sheet1] Cell Content: [<world hello>] Occurences: [2]
".Trim(), errorManager.ToString("Severity: [{0}] Message: [{1}] Cell: [{2}] Sheetname: [{3}] Cell Content: [{5}] Occurences: [{6}]", false));
			errorManager.ClearErrors();
		}

		public void TestSuspendErrorReporting()
		{
			ReportErrorManager errorManager = new ReportErrorManager(new Report(null, null));
			errorManager.Add(new ReportProcessingError("Test Message 1", ReportProcessingErrorSeverity.Error));
			AssertEquals("Report.Errors", "Test Message 1", errorManager.ToString("{1}", false));
			errorManager.ClearErrors();

			using (var suspender = errorManager.GetErrorCheckingSuspender())
			{
				AssertEquals("suspender.HadError", false, suspender.HadError);
				AssertEquals("suspender.HidError", false, suspender.HidError);
				errorManager.Add(new ReportProcessingError("Test Message 2", ReportProcessingErrorSeverity.Error));
				AssertEquals("Report.Errors", ReportErrorManager.HasNoErrors, errorManager.ToString("{1}", false));
				AssertEquals("suspender.HadError", true, suspender.HadError);
				AssertEquals("suspender.HidError", true, suspender.HidError);
			}

			using (var suspender = errorManager.GetErrorCheckingSuspender(x => true))
			{
				AssertEquals("suspender.HadError", false, suspender.HadError);
				AssertEquals("suspender.HidError", false, suspender.HidError);
				errorManager.Add(new ReportProcessingError("Test Message 2", ReportProcessingErrorSeverity.Error));
				AssertEquals("Report.Errors", ReportErrorManager.HasNoErrors, errorManager.ToString("{1}", false));
				AssertEquals("suspender.HadError", true, suspender.HadError);
				AssertEquals("suspender.HidError", true, suspender.HidError);
			}

			using (var suspender = errorManager.GetErrorCheckingSuspender(x => false))
			{
				AssertEquals("suspender.HadError", false, suspender.HadError);
				AssertEquals("suspender.HidError", false, suspender.HidError);
				errorManager.Add(new ReportProcessingError("Test Message 2", ReportProcessingErrorSeverity.Error));
				AssertEquals("Report.Errors", "Test Message 2", errorManager.ToString("{1}", false));
				AssertEquals("suspender.HadError", true, suspender.HadError);
				AssertEquals("suspender.HidError", false, suspender.HidError);
				errorManager.ClearErrors();
			}

			using (var suspender1 = errorManager.GetErrorCheckingSuspender(x => false))
			using (var suspender2 = errorManager.GetErrorCheckingSuspender(x => true))
			{
				AssertEquals("suspender1.HadError", false, suspender1.HadError);
				AssertEquals("suspender2.HadError", false, suspender2.HadError);
				AssertEquals("suspender1.HidError", false, suspender1.HidError);
				AssertEquals("suspender2.HidError", false, suspender2.HidError);
				errorManager.Add(new ReportProcessingError("Test Message 2", ReportProcessingErrorSeverity.Error));
				AssertEquals("suspender1.HadError", true, suspender1.HadError);
				AssertEquals("suspender2.HadError", true, suspender2.HadError);
				AssertEquals("suspender1.HidError", false, suspender1.HidError);
				AssertEquals("suspender2.HidError", true, suspender2.HidError);
				AssertEquals("Report.Errors", ReportErrorManager.HasNoErrors, errorManager.ToString("{1}", false));
			}

			using (var suspender1 = errorManager.GetErrorCheckingSuspender(x => true))
			using (var suspender2 = errorManager.GetErrorCheckingSuspender(x => false))
			{
				AssertEquals("suspender1.HadError", false, suspender1.HadError);
				AssertEquals("suspender2.HadError", false, suspender2.HadError);
				AssertEquals("suspender1.HidError", false, suspender1.HidError);
				AssertEquals("suspender2.HidError", false, suspender2.HidError);
				errorManager.Add(new ReportProcessingError("Test Message 2", ReportProcessingErrorSeverity.Error));
				AssertEquals("suspender1.HadError", true, suspender1.HadError);
				AssertEquals("suspender2.HadError", true, suspender2.HadError);
				AssertEquals("suspender1.HidError", true, suspender1.HidError);
				AssertEquals("suspender2.HidError", false, suspender2.HidError);
				AssertEquals("Report.Errors", ReportErrorManager.HasNoErrors, errorManager.ToString("{1}", false));
			}

			using (var suspender1 = errorManager.GetErrorCheckingSuspender(x => true))
			using (var suspender2 = errorManager.GetErrorCheckingSuspender(x => true))
			{
				AssertEquals("suspender1.HadError", false, suspender1.HadError);
				AssertEquals("suspender2.HadError", false, suspender2.HadError);
				AssertEquals("suspender1.HidError", false, suspender1.HidError);
				AssertEquals("suspender2.HidError", false, suspender2.HidError);
				errorManager.Add(new ReportProcessingError("Test Message 2", ReportProcessingErrorSeverity.Error));
				AssertEquals("suspender1.HadError", true, suspender1.HadError);
				AssertEquals("suspender2.HadError", true, suspender2.HadError);
				AssertEquals("suspender1.HidError", true, suspender1.HidError);
				AssertEquals("suspender2.HidError", true, suspender2.HidError);
				AssertEquals("Report.Errors", ReportErrorManager.HasNoErrors, errorManager.ToString("{1}", false));
			}

			using (var suspender1 = errorManager.GetErrorCheckingSuspender(x => false))
			using (var suspender2 = errorManager.GetErrorCheckingSuspender(x => false))
			{
				AssertEquals("suspender1.HadError", false, suspender1.HadError);
				AssertEquals("suspender2.HadError", false, suspender2.HadError);
				AssertEquals("suspender1.HidError", false, suspender1.HidError);
				AssertEquals("suspender2.HidError", false, suspender2.HidError);
				errorManager.Add(new ReportProcessingError("Test Message 2", ReportProcessingErrorSeverity.Error));
				AssertEquals("suspender1.HadError", true, suspender1.HadError);
				AssertEquals("suspender2.HadError", true, suspender2.HadError);
				AssertEquals("suspender1.HidError", false, suspender1.HidError);
				AssertEquals("suspender2.HidError", false, suspender2.HidError);
				AssertEquals("Report.Errors", "Test Message 2", errorManager.ToString("{1}", false));
				errorManager.ClearErrors();
			}

			errorManager.Add(new ReportProcessingError("Test Message 3", ReportProcessingErrorSeverity.Error));
			AssertEquals("Report.Errors", "Test Message 3", errorManager.ToString("{1}", false));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestReportExceptionReturnsCorrectly()
		{
			using (var report = GetNewReportWithMenuItemAndStmTemplateSetup())
			{
				AssertNoExceptionThrown(delegate
				{ report.ErrorManager.ReportFatalException(new CloneAreaException("Test")); });
				AssertNoExceptionThrown(delegate
				{ report.ErrorManager.ReportFatalException(new DocumentEngineException("Test")); });
				AssertNoExceptionThrown(delegate
				{ report.ErrorManager.ReportFatalException(new DocumentEngineTooManyRowsException()); });
				AssertNoExceptionThrown(delegate
				{ report.ErrorManager.ReportFatalException(new DocumentEngineTooManyColumnsException()); });
				AssertNoExceptionThrown(delegate
				{ report.ErrorManager.ReportFatalException(new DocumentEngineTooManyRowsForThisFileFormatException()); });
				AssertNoExceptionThrown(delegate
				{ report.ErrorManager.ReportFatalException(new DocumentEngineTooManyColumnsForThisFileFormatException()); });
				AssertNoExceptionThrown(delegate
				{ report.ErrorManager.ReportFatalException(new ExcelInterfaceException(ExcelInterfaceExceptionType.ErrorSettingCellValue, "Test")); });
				AssertNoExceptionThrown(delegate
				{ report.ErrorManager.ReportFatalException(new ExpressionEvaluationException("Test", null)); });
				AssertNoExceptionThrown(delegate
				{ report.ErrorManager.ReportFatalException(new FormulaProviderException("Test")); });
				AssertNoExceptionThrown(delegate
				{ report.ErrorManager.ReportFatalException(new FormulaProviderNotReadyException("Test")); });
				AssertNoExceptionThrown(delegate
				{ report.ErrorManager.ReportFatalException(new InvalidGroupByColumnException("Test", "Uh-huh.")); });
				AssertNoExceptionThrown(delegate
				{ report.ErrorManager.ReportFatalException(new MaxConcurrentReportConnectionsExceeded("Test","Test")); });
				AssertNoExceptionThrown(delegate
				{ report.ErrorManager.ReportFatalException(new SQLExecutionException("Dummy", "Test", new Exception())); });
				AssertNoExceptionThrown(delegate
				{ report.ErrorManager.ReportFatalException(new TemplateDefinitionException("Test", new CellReference())); });
				AssertNoExceptionThrown(delegate
				{ report.ErrorManager.ReportFatalException(new FlexCel.Core.FlexCelCoreException()); });

				AssertExceptionThrown(typeof(ReportProcessingException), delegate
				{ report.ErrorManager.ReportFatalException(new ArgumentException("Test")); });
				AssertExceptionThrown(typeof(ReportProcessingException), delegate
				{ report.ErrorManager.ReportFatalException(new IOException("Test")); });
				AssertExceptionThrown(typeof(ReportProcessingException), delegate
				{ report.ErrorManager.ReportFatalException(new InvalidOperationException("Test")); });
				AssertExceptionThrown(typeof(ReportProcessingException), delegate
				{ report.ErrorManager.ReportFatalException(new Exception()); });
			}
		}

		public void TestConstructor()
		{
			ReportErrorManager errorManager = new ReportErrorManager(new Report(null, null));
			AssertEquals("errorManager.HasErrors is false", false, errorManager.HasErrors);
		}

		public void TestAddError()
		{
			ReportErrorManager errorManager = new ReportErrorManager(new Report(null, null));
			AssertEquals("Pre-Condition: errorManager.HasErrors is false", false, errorManager.HasErrors);
			errorManager.Add(new ReportProcessingError("This is a test message 1", new CellReference("Sheet1", 4, 2), ReportProcessingErrorSeverity.Error));
			errorManager.Add(new ReportProcessingError("This is a test message 2", new CellReference("Sheet2", 4, 3), ReportProcessingErrorSeverity.Error));
			errorManager.Add(new ReportProcessingError("This is a test message 3", new CellReference("Sheet3", 4, 4), ReportProcessingErrorSeverity.Error));
			AssertEquals("Report.Errors", @"
Severity: [Error] Message: [This is a test message 1] Cell: [C5] Sheetname: [Sheet1]
Severity: [Error] Message: [This is a test message 2] Cell: [D5] Sheetname: [Sheet2]
Severity: [Error] Message: [This is a test message 3] Cell: [E5] Sheetname: [Sheet3]
".Trim(), errorManager.ToString("Severity: [{0}] Message: [{1}] Cell: [{2}] Sheetname: [{3}]", false));
		}

		public void TestAddErrorRange()
		{
			ReportErrorManager errorManager = new ReportErrorManager(new Report(null, null));
			AssertEquals("Pre-Condition: errorManager.HasErrors is false", false, errorManager.HasErrors);
			errorManager.AddRange(new List<IReportProcessingError> {
				new ReportProcessingError("This is a test message 1", new CellReference("Sheet1", 4, 2), ReportProcessingErrorSeverity.Error),
				new ReportProcessingError("This is a test message 2", new CellReference("Sheet2", 4, 3), ReportProcessingErrorSeverity.Error),
				new ReportProcessingError("This is a test message 3", new CellReference("Sheet3", 4, 4), ReportProcessingErrorSeverity.Error)
			});
			AssertEquals("Report.Errors", @"
Severity: [Error] Message: [This is a test message 1] Cell: [C5] Sheetname: [Sheet1]
Severity: [Error] Message: [This is a test message 2] Cell: [D5] Sheetname: [Sheet2]
Severity: [Error] Message: [This is a test message 3] Cell: [E5] Sheetname: [Sheet3]
".Trim(), errorManager.ToString("Severity: [{0}] Message: [{1}] Cell: [{2}] Sheetname: [{3}]", false));
		}

		public void TestClearErrors()
		{
			ReportErrorManager errorManager = new ReportErrorManager(new Report(null, null));
			AssertEquals("Pre-Condition: errorManager.HasErrors is false", false, errorManager.HasErrors);
			errorManager.Add(new ReportProcessingError("This is a test message 1", new CellReference("Sheet1", 4, 2), ReportProcessingErrorSeverity.Error));
			AssertEquals("errorManager.HasErrors is false", true, errorManager.HasErrors);
			errorManager.ClearErrors();
			AssertEquals("errorManager.HasErrors is false", false, errorManager.HasErrors);
		}

		public void TestErrorsCount()
		{
			ReportErrorManager errorManager = new ReportErrorManager(new Report(null, null));
			AssertEquals("Pre-Condition: errorManager.HasErrors is false", false, errorManager.HasErrors);
			errorManager.Add(new ReportProcessingError("This is a test message 1", new CellReference("Sheet1", 4, 2), ReportProcessingErrorSeverity.Error));
			AssertEquals("Rpt.Errors", "Severity: [Error] Message: [This is a test message 1] Cell: [C5]", errorManager.ToString("Severity: [{0}] Message: [{1}] Cell: [{2}]", false));
		}

		public void TestHasErrors()
		{
			ReportErrorManager errorManager = new ReportErrorManager(new Report(null, null));
			AssertEquals("Pre-Condition: errorManager.HasErrors is false", false, errorManager.HasErrors);
			errorManager.Add(new ReportProcessingError("This is a test message 1", new CellReference("Sheet1", 4, 2), ReportProcessingErrorSeverity.Error));
			AssertEquals("errorManager.HasErrors", true, errorManager.HasErrors);
			errorManager.ClearErrors();
			AssertEquals("errorManager.HasErrors", false, errorManager.HasErrors);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestReportErrorSystemDefinedWithExceptionAsError()
		{
			using (var report = GetNewReportWithMenuItemAndStmTemplateSetup())
			{
				AssertEquals("Pre-Condition: report.errorManager.HasErrors is false", false, report.ErrorManager.HasErrors);
				report.ErrorManager.ReportFatalException(new DocumentEngineException("Template definition error"));
				report.ErrorManager.ReportErrors();
				AssertEquals("Rpt.Errors", "Severity: [Fatal] Message: [Template definition error] Cell: [N/A]", report.ErrorManager.ToString("Severity: [{0}] Message: [{1}] Cell: [{2}]", false));

				AssertNotNull(ErrorReporter.LastExceptionReported);
				try
				{
					AssertType("LastExceptionReported should be an ReportProcessingException", typeof(ReportProcessingException), ErrorReporter.LastExceptionReported);
					AssertMultilineASCIIEquals("ErrorReporter.LastExceptionReported.Message", string.Format(@"Error Generating Report [TestingOnly]

Report Information:

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
Severity: [Fatal] Message: [Template definition error] Cell: [N/A] Sheetname: [(unknown)] Cell Content: [] Occurences: [1]
".Trim(), report.MenuItem.PK, report.StTemplate.PK), ErrorReporter.LastExceptionReported.Message);

					AssertNotNull("ErrorReporter.LastExceptionReported.InnerException", ErrorReporter.LastExceptionReported.InnerException);
					AssertEquals("ErrorReporter.LastExceptionReported.InnerException.GetType()", typeof(DocumentEngineException), ErrorReporter.LastExceptionReported.InnerException.GetType());
					AssertEquals("ErrorReporter.LastExceptionReported.InnerException.Message", "Template definition error", ErrorReporter.LastExceptionReported.InnerException.Message);

					AssertMultilineASCIIEquals("ErrorReporter.LastKeyReported", string.Format(@"Error Generating Report [TestingOnly]
MenuItem PK: [{0}]  BusinessContext: [Custom]  Name/Path: [Misc/Delivery Order]  Filter: [CTY=GB]  IsSystemDefined: [Y]  IsClientSpecific: [Y]
Template PK: [{1}]  Name: [DA893]  DataContext: [Shipping]  ExcelFilePath: [abc.xls]  IsSystemDefined: [Y]  IsClientSpecific: [N]
--------------- Errors Found ---------------
Severity: [Fatal] Message: [Template definition error] Sheetname: [(unknown)] Cell Content: []
".Trim(), report.MenuItem.PK, report.StTemplate.PK), ErrorReporter.LastKeyReported);
				}
				finally
				{
					ErrorReporter.Clear();
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestReportErrorSystemDefinedWithWarningAsError()
		{
			using (Report report = GetNewReportWithMenuItemAndStmTemplateSetup())
			{
				AssertEquals("Pre-Condition: report.errorManager.HasErrors is false", false, report.ErrorManager.HasErrors);
				using (report.ErrorManager.EvaluatingOuterContent("<BigBuggerMacro>"))
				{
					report.ErrorManager.Add(new ReportProcessingError("This is a test message 6", new CellReference("Sheet1", 7, 2), ReportProcessingErrorSeverity.Warning));
				}
				AssertEquals("Precontition: Rpt.Errors", "Severity: [Warning] Message: [This is a test message 6] Cell: [C8]", report.ErrorManager.ToString("Severity: [{0}] Message: [{1}] Cell: [{2}]", false));

				report.ErrorManager.ReportErrors();
				AssertNotNull(ErrorReporter.LastExceptionReported);
				try
				{
					AssertType("LastExceptionReported should be an ReportProcessingException", typeof(ReportProcessingException), ErrorReporter.LastExceptionReported);
					AssertMultilineASCIIEquals("ErrorReporter.LastExceptionReported.Message", string.Format(@"Error Generating Report [TestingOnly]

Report Information:

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
Severity: [Warning] Message: [This is a test message 6] Cell: [C8] Sheetname: [Sheet1] Cell Content: [<BigBuggerMacro>] Occurences: [1]
".Trim(), report.MenuItem.PK, report.StTemplate.PK), ErrorReporter.LastExceptionReported.Message);

					AssertNull("ErrorReporter.LastExceptionReported.InnerException", ErrorReporter.LastExceptionReported.InnerException);

					AssertMultilineASCIIEquals("ErrorReporter.LastKeyReported", string.Format(@"Error Generating Report [TestingOnly]
MenuItem PK: [{0}]  BusinessContext: [Custom]  Name/Path: [Misc/Delivery Order]  Filter: [CTY=GB]  IsSystemDefined: [Y]  IsClientSpecific: [Y]
Template PK: [{1}]  Name: [DA893]  DataContext: [Shipping]  ExcelFilePath: [abc.xls]  IsSystemDefined: [Y]  IsClientSpecific: [N]
--------------- Errors Found ---------------
Severity: [Warning] Message: [This is a test message 6] Sheetname: [Sheet1] Cell Content: [<BigBuggerMacro>]
".Trim(), report.MenuItem.PK, report.StTemplate.PK), ErrorReporter.LastKeyReported);
				}
				finally
				{
					ErrorReporter.Clear();
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestReportErrorSystemDefinedWithWarningAsError_ErrStringConstantInFormulaTooLong()
		{
			using (var report = GetNewReportWithMenuItemAndStmTemplateSetup())
			{
				AssertEquals("Pre-Condition: report.errorManager.HasErrors is false", false, report.ErrorManager.HasErrors);
				using (report.ErrorManager.EvaluatingOuterContent("<OverLengthMacro>"))
				{
					report.ErrorManager.Add(new ReportProcessingError("Over Length Macro", ReportProcessingErrorSeverity.Warning, new ExcelInterfaceException(ExcelInterfaceExceptionType.ErrorSettingCellValue, "123", new FlexCelCoreException("456", FlxErr.ErrStringConstantInFormulaTooLong))));
				}
				AssertEquals("Precontition: Rpt.Errors", "Severity: [Warning] Message: [Over Length Macro] Cell: [N/A]", report.ErrorManager.ToString("Severity: [{0}] Message: [{1}] Cell: [{2}]", false));

				report.ErrorManager.ReportErrors();
				AssertNull(ErrorReporter.LastExceptionReported);
				ErrorReporter.Clear();
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestShowWarningSystemDefinedWithNotReportedWarning()
		{
			using (Report report = GetNewReportWithMenuItemAndStmTemplateSetup())
			{
				AssertEquals("Pre-Condition: report.errorManager.HasErrors is false", false, report.ErrorManager.HasErrors);
				using (report.ErrorManager.EvaluatingOuterContent("<BigBuggerMacro>"))
				{
					report.ErrorManager.Add(new TemplateGenerationError("Template definition error", ReportProcessingErrorSeverity.WarningWithoutErrorReport).ToReportProcessingError());
				}
				AssertEquals("Precontition: Rpt.Errors", "Severity: [Warning (without error report)] Message: [Template definition error] Cell: [N/A] Sheetname: [(unknown)]", report.ErrorManager.ToString("Severity: [{0}] Message: [{1}] Cell: [{2}] Sheetname: [{3}]", false));

				report.ErrorManager.ReportErrors();

				AssertNull("LastExceptionReported should be null", ErrorReporter.LastExceptionReported);

				AssertEquals("No email is sent to the client", 0, Env.OutgoingMailManager.EmailsCreated.Count);

				AssertEquals("report.errorManager.HasWarningsOnly is true", true, report.ErrorManager.HasWarningsOnly);
				ErrorReporter.Clear();
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestShowFatalNotReportedError_ShouldNotReport()
		{
			using (var report = GetNewReportWithMenuItemAndStmTemplateSetup())
			{
				AssertEquals("Pre-Condition: report.errorManager.HasErrors is false", false, report.ErrorManager.HasErrors);
				using (report.ErrorManager.EvaluatingOuterContent("<BigBuggerMacro>"))
				{
					report.ErrorManager.Add(new TemplateGenerationError("Template definition error", ReportProcessingErrorSeverity.FatalWithoutErrorReport).ToReportProcessingError());
				}
				AssertEquals("Pre-Condition: Rpt.Errors", "Severity: [Fatal Error (without error report)] Message: [Template definition error] Cell: [N/A] Sheetname: [(unknown)]", report.ErrorManager.ToString("Severity: [{0}] Message: [{1}] Cell: [{2}] Sheetname: [{3}]", false));

				report.ErrorManager.ReportErrors();

				AssertNull("LastExceptionReported should be null", ErrorReporter.LastExceptionReported);
				AssertEquals("No email is sent to the client", 0, Env.OutgoingMailManager.EmailsCreated.Count);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestShowErrorNotReportedError_ShouldNotReport()
		{
			using (var report = GetNewReportWithMenuItemAndStmTemplateSetup())
			{
				AssertEquals("Pre-Condition: report.errorManager.HasErrors is false", false, report.ErrorManager.HasErrors);
				using (report.ErrorManager.EvaluatingOuterContent("<BigBuggerMacro>"))
				{
					report.ErrorManager.Add(new TemplateGenerationError("Template definition error", ReportProcessingErrorSeverity.ErrorWithoutErrorReport).ToReportProcessingError());
				}
				AssertEquals("Pre-Condition: Rpt.Errors", "Severity: [Error (without error report)] Message: [Template definition error] Cell: [N/A] Sheetname: [(unknown)]", report.ErrorManager.ToString("Severity: [{0}] Message: [{1}] Cell: [{2}] Sheetname: [{3}]", false));

				report.ErrorManager.ReportErrors();

				AssertNull("LastExceptionReported should be null", ErrorReporter.LastExceptionReported);
				AssertEquals("No email is sent to the client", 0, Env.OutgoingMailManager.EmailsCreated.Count);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestReportExceptionSystemDefinedUnhandledExceptionGetsWrappedAndRethrown()
		{
			using (Report report = GetNewReportWithMenuItemAndStmTemplateSetup())
			{
				try
				{
					report.ErrorManager.ReportFatalException(new IOException("some unknown exception"));
				}
				catch (ReportProcessingException exception)
				{
					AssertEquals("report.ErrorManager.HasErrors should be false", false, report.ErrorManager.HasErrors);

					AssertEquals(string.Format(@"UNHANDLED Error Generating Report [TestingOnly]

Report Information:

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
Severity: [Fatal] Message: [IOException: some unknown exception] Cell: [N/A] Sheetname: [(unknown)] Cell Content: [] Occurences: [1]
", report.MenuItem.PK, report.StTemplate.PK).Trim(), exception.Message);
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestReportExceptionNonSystemDefinedErrorDoesNotGetReportedAsAnException()
		{
			using (Report report = GetNewReportWithMenuItemAndStmTemplateSetup())
			{
				report.StTemplate.SO_IsSystemDefined = false;
				report.ErrorManager.ReportFatalException(new DocumentEngineException("Template definition error"));
				report.ErrorManager.ReportErrors();
				AssertEquals("Rpt.Errors", @"
Severity: [Fatal Error (without error report)] Message: [Template definition error] Cell: [N/A]
".Trim(), report.ErrorManager.ToString("Severity: [{0}] Message: [{1}] Cell: [{2}]", false));

				AssertNull("LastExceptionReported should be null", ErrorReporter.LastExceptionReported);
				ErrorReporter.Clear();

				AssertEquals("Emails to Client", 1, Env.OutgoingMailManager.EmailsCreated.Count);
				EmailDef email = Env.OutgoingMailManager.EmailsCreated[0];
				AssertEquals("email.Recipients", "postmaster@sample.org", email.Recipients.RecipientsAsDelimitedString());
				AssertEquals("Error Generating Report [TestingOnly]", email.Subject);
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
Severity: [Fatal Error (without error report)] Message: [Template definition error] Cell: [N/A] Sheetname: [(unknown)] Cell Content: [] Occurences: [1]
", report.MenuItem.PK, report.StTemplate.PK).Trim(), email.Body.Trim());
				Env.OutgoingMailManager.EmailsCreated.Clear();
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestReportExceptionNonSystemDefinedUnhandledErrorDoesGetReportedAsAnException()
		{
			using (Report report = GetNewReportWithMenuItemAndStmTemplateSetup())
			{
				report.StTemplate.SO_IsSystemDefined = false;
				try
				{
					report.ErrorManager.ReportFatalException(new IOException("some unknown exception"));
				}
				catch (ReportProcessingException exception)
				{
					AssertEquals("report.ErrorManager.HasErrors should be false", false, report.ErrorManager.HasErrors);
					AssertEquals(string.Format(@"UNHANDLED Error Generating Report [TestingOnly]

Report Information:

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
Severity: [Fatal Error (without error report)] Message: [IOException: some unknown exception] Cell: [N/A] Sheetname: [(unknown)] Cell Content: [] Occurences: [1]
", report.MenuItem.PK, report.StTemplate.PK).Trim(), exception.Message);
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestReportExceptionNonSystemDefinedErrorTargetInvocationExceptionUserException()
		{
			using (Report report = GetNewReportWithMenuItemAndStmTemplateSetup())
			{
				report.StTemplate.SO_IsSystemDefined = false;
				report.ErrorManager.ReportFatalException(
					new System.Reflection.TargetInvocationException("TargetInvocationException"
						, new SQLExecutionException("SQLExecutionException", "sql", new Exception("Exception"))));

				report.ErrorManager.ReportErrors();
				AssertEquals("Rpt.Errors", @"
Severity: [Fatal Error (without error report)] Message: [Error loading table [SQLExecutionException]. System.Exception: [Exception] occurred running SQL: [sql]. ] Cell: [N/A]
".Trim(), report.ErrorManager.ToString("Severity: [{0}] Message: [{1}] Cell: [{2}]", false));

				AssertNull("LastExceptionReported should be null", ErrorReporter.LastExceptionReported);
				ErrorReporter.Clear();

				AssertEquals("Emails to Client", 1, Env.OutgoingMailManager.EmailsCreated.Count);
				EmailDef email = Env.OutgoingMailManager.EmailsCreated[0];
				AssertEquals("email.Recipients", "postmaster@sample.org", email.Recipients.RecipientsAsDelimitedString());
				AssertEquals("Error Generating Report [TestingOnly]", email.Subject);
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
Severity: [Fatal Error (without error report)] Message: [Error loading table [SQLExecutionException]. System.Exception: [Exception] occurred running SQL: [sql]. ] Cell: [N/A] Sheetname: [(unknown)] Cell Content: [] Occurences: [1]
", report.MenuItem.PK, report.StTemplate.PK).Trim(), email.Body.Trim());
				Env.OutgoingMailManager.EmailsCreated.Clear();
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestReportExceptionNonSystemDefinedErrorTargetInvocationExceptionNonUserException()
		{
			using (Report report = GetNewReportWithMenuItemAndStmTemplateSetup())
			{
				report.StTemplate.SO_IsSystemDefined = false;
				try
				{
					report.ErrorManager.ReportFatalException(new System.Reflection.TargetInvocationException("TargetInvocationException",
						new IOException("some unknown exception")));
				}
				catch (ReportProcessingException exception)
				{
					AssertEquals("report.ErrorManager.HasErrors should be false", false, report.ErrorManager.HasErrors);
					AssertEquals(string.Format(@"UNHANDLED Error Generating Report [TestingOnly]

Report Information:

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
Severity: [Fatal Error (without error report)] Message: [IOException: some unknown exception] Cell: [N/A] Sheetname: [(unknown)] Cell Content: [] Occurences: [1]
", report.MenuItem.PK, report.StTemplate.PK).Trim(), exception.Message);
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestReportExceptionNonSystemDefinedSendEmailsToCorrectLocalRecipients()
		{
			using (Report report = GetNewReportWithMenuItemAndStmTemplateWhichHasBeenEditedSetup())
			{
				report.StTemplate.SO_IsSystemDefined = false;
				AssertEquals("Precondition - Emails to Client", 0, Env.OutgoingMailManager.EmailsCreated.Count);
				report.ErrorManager.ReportFatalException(new DocumentEngineException("Template definition error"));

				report.ErrorManager.ReportErrors();
				AssertEquals("Emails to Client", 1, Env.OutgoingMailManager.EmailsCreated.Count);
				EmailDef email = Env.OutgoingMailManager.EmailsCreated[0];
				AssertEquals("Email.Recipients should match", "pizza@heaven.com.au", email.Recipients.RecipientsAsDelimitedString());
				AssertEquals("Email.CCRecipients should match", "templateAuthor@sample.org", email.CCRecipients.RecipientsAsDelimitedString());
				AssertEquals("Error Generating Report [TestingOnly]", email.Subject);
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
Severity: [Fatal Error (without error report)] Message: [Template definition error] Cell: [N/A] Sheetname: [(unknown)] Cell Content: [] Occurences: [1]
", report.MenuItem.PK, report.StTemplate.PK).Trim(), email.Body.Trim());
				Env.OutgoingMailManager.EmailsCreated.Clear();
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestAddError_WhenTemplateContainsCustomisedSections_ShouldChangeErrorToNotReported()
		{
			using (var report = GetNewReportWithMenuItemAndStmTemplateWhichHasBeenEditedSetup())
			{
				var manager = new ReportErrorManager(report);
				var error1 = new ReportProcessingError("Blah", ReportProcessingErrorSeverity.Warning);
				var error2 = new ReportProcessingError("Blah", ReportProcessingErrorSeverity.Warning);
				var error3 = new ReportProcessingError("Blah", ReportProcessingErrorSeverity.Warning);

				report.Template.ContainsCustomisedSections = true;
				manager.Add(error1);
				manager.AddRange(new[] { error2 });

				report.Template.ContainsCustomisedSections = false;
				manager.Add(error3);

				AssertEquals(ReportProcessingErrorSeverity.WarningWithoutErrorReport, error1.Severity);
				AssertEquals(ReportProcessingErrorSeverity.WarningWithoutErrorReport, error2.Severity);
				AssertEquals(ReportProcessingErrorSeverity.Warning, error3.Severity);
			}
		}

		public void TestDuplicateFilters()
		{
			var mainSheet = new KeyValuePair<string, string>("Main Sheet",
@"{A}-[#Config]
{A}-[#EndOfReport]");

			var filterSheet = new KeyValuePair<string, string>("Filter",
@"{A}-[FilterA]
{B}-[Type]
{C}-[ExactText]
{B}-[Field]
{C}-[XX_Field]
{B}-[DefaultValue]
{C}-[Hello]
{A}-[FilterA]
{B}-[Type]
{C}-[ExactText]
{B}-[Field]
{C}-[XX_Field]
{B}-[DefaultValue]
{C}-[Hello]
{A}-[#end]");

			var template = Factory.New<StmTemplateBase>();
			ConfigurableTemplateTestHelper.SetWorkSheets(template, mainSheet, filterSheet);

			ReportCommand command = Factory.New<ReportCommand>();
			DocumentPack pack = new DocumentPack(command);
			var excelTemplate = new ExcelTemplateReadFromByteArray("TemplateFromStream", "", template.SO_Template);
			using (var report = new Report(pack, excelTemplate))
			{
				Assert("There shouldn't be an error before the report load.", !report.ErrorManager.HasErrors);

				report.PrepareForRender();

				Assert("There should be an error about filter", report.ErrorManager.HasErrors);
				var errors = (report.ErrorManager as IHaveReportProcessingErrorsForGUI).GetErrors();
				AssertEquals(1, errors.Length);
				AssertEquals("Error Building Filters from Tree: There are filters using the same name [FilterA], which should be fixed by updating the template.", errors[0].Message);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestAddNonReportedSqlException_ShouldAdjustSeverity()
		{
			ReportSqlErrorAndAssertSeverity(9001, ReportProcessingErrorSeverity.FatalWithoutErrorReport);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestAddReportedSqlException_ShouldNotAdjustSeverity()
		{
			ReportSqlErrorAndAssertSeverity(8152, ReportProcessingErrorSeverity.Fatal);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestInvalidGroupByColumnException_ShouldNotAdjustSeverity()
		{
			using (var report = GetNewReportWithMenuItemAndStmTemplateSetup())
			{
				AssertInvalidGroupByColumnException(report, ReportProcessingErrorSeverity.Fatal);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestInvalidGroupByColumnException_ShouldAdjustSeverity()
		{
			using (var report = GetNewReportWithMenuItemAndStmTemplateSetup())
			{
				var wrapper = new DummyDocWrapper();
				((IReportForUnitTesting)report).SetBusinessObjectForTesting(wrapper);
				report.VisualizerContentNote.DD_DocumentData = ZBlob.FromAscii("abc");
				report.Factory.Save();
				report.VisualizerContentNote.Factory.Save();

				AssertInvalidGroupByColumnException(report, ReportProcessingErrorSeverity.FatalWithoutErrorReport);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestReportErrors_WhenReportContainsCustomsations_ShouldNotSendErrorReports()
		{
			using (var report = GetNewReportWithMenuItemAndStmTemplateSetup())
			{
				report.MenuItem.SU_IsSystemDefined = false;
				Assert(report.ContainsAnyCustomisation);

				var manager = new ReportErrorManager(report);
				manager.Add(new ReportProcessingError("", ReportProcessingErrorSeverity.Fatal));
				manager.ReportErrors();

				AssertEquals(0, Env.OutgoingMailManager.EmailsCreated.Count);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestReportErrorsShouldSendToEmailDestinationOverride()
		{
			var testEmail = "teste.email@cargowise.com";
			var emailDestinationOverride = "email.destination.override@cargowise.com";
			var excelTemplate = new ExcelTemplateForUnitTesting("SimpleTest.xls", TestFilesSubFolder.ReportTestFiles);

			Env.Registry.EmailDestinationOverride = emailDestinationOverride;

			using (var documentPack = new DocumentPack())
			using (var report = new Report(documentPack, excelTemplate))
			{
				var scheduleTask = Factory.NewWithValidTestData<ReportScheduleTask>();
				scheduleTask.S5_ScheduleDescription = "My Scheduled Report";

				Factory.Save();

				using (Env.CurrentUser.SetUserEmailAddressInTESTINGOnly(testEmail))
				{
					report.StTemplate = Factory.NewWithValidTestData<StmTemplateBase>();
					report.StTemplate.SO_Template = excelTemplate.GetAsByteArray();
					report.SetScheduleTask(scheduleTask);
					report.ErrorManager.Add(new ReportFilterValidationError("My Error", ReportProcessingErrorSeverity.Error));
					report.ErrorManager.ReportErrors();

					var emails = Factory.Load<MailManager.Business.MailItem>(new ZQuery());
					AssertEquals("There should be one email created", 1, emails.Length);
					AssertEquals("There should be one recipient", 1, emails[0].MailRecipients.Count);
					AssertEquals(emailDestinationOverride, emails[0].MailRecipients[0].EmailAddress);
					AssertEquals("Override message", true, emails[0].MI_Body.Contains(string.Format("(This message was redirected to {0} as it was sent from a non-production system. Originally the email was addressed to {1}.)", emailDestinationOverride, testEmail)));
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestNoScheduledReportHasBeenMarkedInactiveInEmail()
		{
			var testEmail = "teste.email@cargowise.com";
			var emailDestinationOverride = "email.destination.override@cargowise.com";
			var excelTemplate = new ExcelTemplateForUnitTesting("SimpleTest.xls", TestFilesSubFolder.ReportTestFiles);

			Env.Registry.EmailDestinationOverride = emailDestinationOverride;

			using (var documentPack = new DocumentPack())
			using (var report = new Report(documentPack, excelTemplate))
			{
				var scheduleTask = Factory.NewWithValidTestData<ReportScheduleTask>();
				scheduleTask.S5_ScheduleDescription = "My Scheduled Report";

				Factory.Save();

				using (Env.CurrentUser.SetUserEmailAddressInTESTINGOnly(testEmail))
				{
					report.StTemplate = Factory.NewWithValidTestData<StmTemplateBase>();
					report.StTemplate.SO_Template = excelTemplate.GetAsByteArray();
					report.SetScheduleTask(scheduleTask);
					report.ErrorManager.Add(new ReportProcessingError("Error Message", ReportProcessingErrorSeverity.FatalWithoutErrorReport, new ReportSQLTimeoutException()));
					report.ErrorManager.ReportErrors();

					var emails = Factory.Load<MailManager.Business.MailItem>(new ZQuery());
					AssertEquals("There should be one email created", 1, emails.Length);
					AssertNotContains("This scheduled report has been marked inactive until the following problems have been resolved:-", emails[0].MI_Body);
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestScheduledReportOutputMessageCorrectly()
		{
			var testEmail = "teste.email@cargowise.com";
			var emailDestinationOverride = "email.destination.override@cargowise.com";
			var excelTemplate = new ExcelTemplateForUnitTesting("SimpleTest.xls", TestFilesSubFolder.ReportTestFiles);

			Env.Registry.EmailDestinationOverride = emailDestinationOverride;

			using (var documentPack = new DocumentPack())
			using (var report = new Report(documentPack, excelTemplate))
			{
				var scheduleTask = Factory.NewWithValidTestData<ReportScheduleTask>();
				scheduleTask.S5_ScheduleDescription = "My Scheduled Report";

				Factory.Save();

				using (Env.CurrentUser.SetUserEmailAddressInTESTINGOnly(testEmail))
				{
					report.StTemplate = Factory.NewWithValidTestData<StmTemplateBase>();
					report.StTemplate.SO_Template = excelTemplate.GetAsByteArray();
					report.SetScheduleTask(scheduleTask);
					report.ErrorManager.Add(new ReportProcessingError("Error Message", ReportProcessingErrorSeverity.Error, new IOException()));
					report.ErrorManager.ReportErrors();

					var emails = Factory.Load<MailManager.Business.MailItem>(new ZQuery(MailDBItemsSchema.MI_Subject, "Error Running Scheduled Report [My Scheduled Report]"));
					AssertEquals("There should be one email created", 1, emails.Length);
					AssertEquals("This scheduled report has been marked inactive.", false, scheduleTask.S5_IsActive);
					AssertContains("This scheduled report has been marked inactive until the following problems have been resolved:-", emails[0].MI_Body);
				}
			}

			using (var documentPack = new DocumentPack())
			using (var report = new Report(documentPack, excelTemplate))
			{
				var scheduleTask = Factory.NewWithValidTestData<ReportScheduleTask>();
				scheduleTask.S5_ScheduleDescription = "My Scheduled Report";

				Factory.Save();

				using (Env.CurrentUser.SetUserEmailAddressInTESTINGOnly(testEmail))
				{
					report.StTemplate = Factory.NewWithValidTestData<StmTemplateBase>();
					report.StTemplate.SO_Template = excelTemplate.GetAsByteArray();
					report.SetScheduleTask(scheduleTask);
					report.ErrorManager.Add(new ReportProcessingError("Error Message", ReportProcessingErrorSeverity.Warning, new IOException()));
					report.ErrorManager.ReportErrors();

					var emails = Factory.Load<MailManager.Business.MailItem>(new ZQuery(MailDBItemsSchema.MI_Subject, "Error Running Scheduled Report [My Scheduled Report]")).OrderByDescending(s => s.MI_SystemCreateTimeUtc).ToArray();
					AssertEquals("There should be two email created", 2, emails.Length);
					AssertEquals("This scheduled report has been marked active.", true, scheduleTask.S5_IsActive);
					AssertNotContains("This scheduled report has been marked inactive until the following problems have been resolved:-", emails[0].MI_Body);
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestReportErrorsShouldIgnoreEmailHasNoFromAddressExceptionWhenSendingEmail()
		{
			var excelTemplate = new ExcelTemplateForUnitTesting("SimpleTest.xls", TestFilesSubFolder.ReportTestFiles);
			Env.Registry.MailboxEmailAddress = "";
			using (var documentPack = new DocumentPack())
			using (var report = new Report(documentPack, excelTemplate))
			{
				var scheduleTask = Factory.NewWithValidTestData<ReportScheduleTask>();
				scheduleTask.S5_ScheduleDescription = "My Scheduled Report";
				Factory.Save();

				report.StTemplate = Factory.NewWithValidTestData<StmTemplateBase>();
				report.StTemplate.SO_Template = excelTemplate.GetAsByteArray();
				report.SetScheduleTask(scheduleTask);
				report.ErrorManager.Add(new ReportFilterValidationError("My Error", ReportProcessingErrorSeverity.Error));
				report.ErrorManager.ReportErrors();

				Assert(report.ErrorManager.ToString().Contains("Could not send the report error to related user via email because the 'from address' was empty"));
				Assert(ErrorReporter.LastKeyReported.IsNullOrEmpty());
			}
		}

		protected override void SetUp()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			GlbGroup postMasterGroup = factory.Load<GlbGroup>(Core.Constants.Groups.PostMastersGroupPK);
			GlbStaff postMaster = postMasterGroup.Staff.AddNew();
			postMaster.GS_EmailAddress = "postmaster@sample.org";
			postMaster.GS_Code = "_O_";
			postMaster.GS_LoginName = "postmastersample";

			GlbStaff templateAuthor = factory.New<GlbStaff>();
			templateAuthor.GS_EmailAddress = "templateAuthor@sample.org";
			templateAuthor.GS_Code = "_X_";
			templateAuthor.GS_LoginName = "templateAuthorSample";
			factory.Save();
		}

		protected override void TearDown()
		{
			userEmailOverride?.Dispose();
			base.TearDown();
		}

		Report GetNewReportWithMenuItemAndStmTemplateSetup()
		{
			Report report = SetupMenuItemAndTemplate();
			userEmailOverride = Env.CurrentUser.SetUserEmailAddressInTESTINGOnly("");
			return report;
		}

		Report GetNewReportWithMenuItemAndStmTemplateWhichHasBeenEditedSetup()
		{
			var report = SetupMenuItemAndTemplate();

#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			var stmALog1 = report.StTemplate.Logs.AddNew(Events.AddedARecordToTheSystem, ZDateTimeOffset.Now);
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			stmALog1.SL_GS_NKUser = "_X_";

#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			var stmALog2 = report.StTemplate.Logs.AddNew(Events.EditedARecord, ZDateTimeOffset.Now);
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			stmALog2.SL_GS_NKUser = "_X_";

			userEmailOverride = Env.CurrentUser.SetUserEmailAddressInTESTINGOnly("pizza@heaven.com.au");
			return report;
		}

		Report SetupMenuItemAndTemplate()
		{
			ReportCommand command = Factory.New<ReportCommand>();
			ExcelTemplateForUnitTesting excelTemplate = new ExcelTemplateForUnitTesting("Test.xls", TestFilesSubFolder.DocumentTestFiles);
			DocumentPack pack = new DocumentPack(command);

			Report report = new Report(pack, excelTemplate, "TestingOnly", null, false);

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

			var pivot = Factory.New<StmMenuTemplatePivot>();
			pivot.SI_SO = report.StTemplate.PK;
			pivot.SI_SU = report.MenuItem.PK;
			pivot.SI_IsSystemDefined = true;

			return report;
		}

		void ReportSqlErrorAndAssertSeverity(int sqlErrorNumber, ReportProcessingErrorSeverity expectedSeverity)
		{
			using (var report = GetNewReportWithMenuItemAndStmTemplateSetup())
			{
				var sqlError = SqlExceptionBuilder.CreateSqlError(sqlErrorNumber, 1, 1, "", "Report me!", "", 1);
				var exception = SqlExceptionBuilder.CreateSqlException(SqlExceptionBuilder.CreateSqlErrorCollection(sqlError));

				var manager = new ReportErrorManager(report);
				manager.ReportFatalException(exception);
				var errors = ((IHaveReportProcessingErrorsForGUI)manager).GetErrors();

				AssertEquals(1, errors.Length);
				AssertEquals(expectedSeverity, errors[0].Severity);
			}
		}

		void AssertInvalidGroupByColumnException(Report report, ReportProcessingErrorSeverity expectedSeverity)
		{
			var exception = new InvalidGroupByColumnException("bla", "bouh!");

			var manager = new ReportErrorManager(report);
			manager.ReportFatalException(exception);
			var errors = ((IHaveReportProcessingErrorsForGUI)manager).GetErrors();

			AssertEquals(1, errors.Length);
			AssertEquals(expectedSeverity, errors[0].Severity);
		}

		IDisposable userEmailOverride;
	}
}
