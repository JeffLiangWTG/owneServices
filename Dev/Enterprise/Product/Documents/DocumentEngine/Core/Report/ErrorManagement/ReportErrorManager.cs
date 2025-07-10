using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.Environment;
using Enterprise.Integration.DocumentEngine;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using FlexCel.Core;

namespace Enterprise.DocumentEngine.ReportErrorManagement
{
	public class ReportErrorManager : IHaveReportProcessingErrorsForGUI
	{
		internal ReportErrorManager(Report report)
		{
			if (report == null)
			{
				throw new ArgumentNullException("Report report");
			}

			this.report = report;
			this.errors = new List<IReportProcessingError>();
		}

		readonly Report report;
		readonly List<IReportProcessingError> errors;

		internal void Add(IReportProcessingError error)
		{
			AdjustWarningLevelIfOverridden(error);
			AdjustWarningLevelForCustomisedReports(error);

			error.SetCurrentStatus(OuterContent, InnerMacro, CurrentCell);
			error.SetTemplatePath(TemplatePath);
			if (!IsErrorCheckingSuspendedFor(error))
			{
				var matchingError = errors.Find(x =>
					string.Equals(x.Message, error.Message, StringComparison.InvariantCultureIgnoreCase)
					&& x.OuterContent == error.OuterContent
					&& x.TemplatePath == error.TemplatePath
					&& x.SheetName == error.SheetName);

				if (matchingError != null)
				{
					matchingError.IncrementOccurrences();
				}
				else
				{
					errors.Add(error);
				}
			}
		}

		internal void AddRange(IEnumerable<IReportProcessingError> errorsToAdd)
		{
			if (errorsToAdd != null)
			{
				foreach (var error in errorsToAdd)
				{
					error.SetTemplatePath(TemplatePath);
					if (!IsErrorCheckingSuspendedFor(error))
					{
						// We don't add the outer and inner macro here as this is not one error for one cell, but multiple for a whole region.
						// Outer and Inner Macros would have to be added at a different level.
						AdjustWarningLevelForCustomisedReports(error);
						errors.Add(error);
					}
				}
			}
		}

		void AdjustWarningLevelForCustomisedReports(IReportProcessingError error)
		{
			if (report.ContainsAnyCustomisation)
			{
				if (error.Severity == ReportProcessingErrorSeverity.Warning)
				{
					error.Severity = ReportProcessingErrorSeverity.WarningWithoutErrorReport;
				}
				else if (error.Severity == ReportProcessingErrorSeverity.Error)
				{
					error.Severity = ReportProcessingErrorSeverity.ErrorWithoutErrorReport;
				}
			}
		}

		void AdjustWarningLevelIfOverridden(IReportProcessingError error)
		{
			if (overriddenSeverity != null)
			{
				error.Severity = (ReportProcessingErrorSeverity)overriddenSeverity;
			}
		}

		internal bool HasErrors
		{
			get { return errors.Count != 0; }
		}

		internal int ErrorsCount => errors.Count;
		IReportProcessingError[] IHaveReportProcessingErrorsForGUI.GetErrors()
		{
			return errors.ToArray();
		}

		internal void ClearErrors()
		{
			errors.Clear();
		}

		internal void ReportFatalException(Exception fatalException)
		{
			Exception exception = fatalException;

			if (exception is System.Reflection.TargetInvocationException && exception.InnerException != null)
			{
				exception = exception.InnerException;
			}

			lastFatalException = exception;
			Add(GetReportProcessingError(exception, ReportProcessingErrorSeverity.Fatal));
			if (!ExceptionShouldBeShownToTheUser(exception))
			{
				string errorMessage = ErrorMessageForUser;
				ClearErrors();
				throw new ReportProcessingException("UNHANDLED " + ErrorTitle, errorMessage, exception);
			}
		}

		bool ExceptionShouldBeShownToTheUser(Exception exception)
		{
			return ReportProcessingErrorSeverityExtensions.ExceptionTypesToBeShownToUser.Any(exceptionToBeShown => exceptionToBeShown.IsInstanceOfType(exception));
		}

		string ErrorTitle
		{
			get
			{
				if (report.ScheduleTask != null)
				{
					return Res.GetString("90cfaf56-31ad-4d7e-b9e4-d6eb0c250676", "Error Running Scheduled Report [{0}]", report.ScheduleTask.S5_ScheduleDescription);
				}
				else
				{
					string reportName = null;
					try
					{
						reportName = report.Name;
					}
					catch { } // If for any reason the Report cannot return a name, we should still be able to continue and report a proper error.
					return Res.GetString("5208b75f-bd17-4bbb-ae0a-d6c12f7a900f", "Error Generating {0} [{1}]", report.Style.ToString(), (string.IsNullOrEmpty(reportName) ? (NoResString)"unknown" : reportName));
				}
			}
		}

		string ErrorMessageForUser
		{
			get
			{
				return GetErrorMessage(false);
			}
		}

		string ErrorMessageForErrorReport
		{
			get
			{
				return GetErrorMessage(true);
			}
		}

		string GetErrorMessage(bool includeExceptionDetails)
		{
			if (report.ScheduleTask != null)
			{
				return GetReportScheduleTaskErrorMessage(report.ScheduleTask, includeExceptionDetails);
			}
			else
			{
				return report.ToString() + GetErrorsFound(includeExceptionDetails);
			}
		}

		string GetErrorsFound(bool includeExceptionDetails)
		{
			return (NoResString)@"

Errors Found
---------------
" + this.ToString((NoResString)"Severity: [{0}] Message: [{1}] Cell: [{2}] Sheetname: [{3}] Cell Content: [{5}] Occurences: [{6}]{7}{8}", false, includeExceptionDetails);
		}

		string GetReportScheduleTaskErrorMessage(ReportScheduleTask reportScheduleTask, bool includeExceptionDetails)
		{
			var errorMessageBuilder = new ZStringBuilder(Res.GetString("ffa2fecf-fc92-48c0-9bec-32d0c3fc852a", "Scheduled Report Information:"));

			errorMessageBuilder.Append(string.Empty);
			errorMessageBuilder.Append(GetReportScheduleTaskDetailsString(reportScheduleTask));

			if (errors.Any(error => !(error.Exception is ReportSQLTimeoutException)) && !HasWarningsOnly)
			{
				errorMessageBuilder.Append(Res.GetString("6d504e32-8841-45c5-ac07-cab89ccc82fb", "This scheduled report has been marked inactive until the following problems have been resolved:-"));
			}

			errorMessageBuilder.Append(GetErrorsFound(includeExceptionDetails));

			return errorMessageBuilder.ToStringWithNewLineBetweenAppends();
		}

		string GetReportScheduleTaskDetailsString(ReportScheduleTask reportScheduleTask)
		{
			var builder = new StringBuilder();
			var leadingWhitespace = "   ";

			var factory = reportScheduleTask.Factory;
			var menuItem = factory.Load<StmMenuItem>(reportScheduleTask.S5_ParentID);
			var menuItemUniqueCode = menuItem != null ? menuItem.MenuItemUniqueCode : ZString.Empty;
			var branch = factory.Load<GlbBranch>(reportScheduleTask.S5_GB);
			var branchCode = branch != null ? branch.GB_Code : ZString.Empty;

			builder.Append(leadingWhitespace);
			builder.AppendLine(Res.GetString("eb69a102-934c-49c7-ae1c-287ca7cc5592", "Description = [{0}]", reportScheduleTask.S5_ScheduleDescription));
			builder.Append(leadingWhitespace);
			builder.AppendLine(Res.GetString("14fe4ae1-a7b1-4bef-baf6-f63e8a2e045b", "Start Date = [{0}]", reportScheduleTask.Recurrence.StartDateLocal));
			builder.Append(leadingWhitespace);
			builder.AppendLine(Res.GetString("03144504-60d8-49e6-8561-129c030a1d93", "End Date = [{0}]", reportScheduleTask.Recurrence.EndDateLocal));
			builder.Append(leadingWhitespace);
			builder.AppendLine(Res.GetString("a44022f5-3c19-4c2e-98bc-9c3dc78c4df7", "Report = [{0}]", menuItemUniqueCode));
			builder.Append(leadingWhitespace);
			builder.AppendLine(Res.GetString("09923398-9786-4a1a-952b-92978a7c5262", "Branch = [{0}]", branchCode));

			return builder.ToString();
		}

		public bool HasWarningsOnly
		{
			get
			{
				return !errors.Any(error => (error.Severity != ReportProcessingErrorSeverity.Warning && error.Severity != ReportProcessingErrorSeverity.WarningWithoutErrorReport));
			}
		}

		bool ContainsOnlyFilterValidationErrors
		{
			get
			{
				return !errors.Any(error => !(error is ReportFilterValidationError));
			}
		}

		bool ContainsOnlyNotReportedWarningsOrErrors
		{
			get
			{
				return errors.All(error => error.Severity == ReportProcessingErrorSeverity.WarningWithoutErrorReport
										 || error.Severity == ReportProcessingErrorSeverity.ErrorWithoutErrorReport
										 || error.Severity == ReportProcessingErrorSeverity.FatalWithoutErrorReport);
			}
		}

		bool ContainsOnlyErrFormula
		{
			get
			{
				return errors.All(error => (error.Exception?.InnerException is FlexCelCoreException ex)
										&& (ex.ErrorCode == FlxErr.ErrStringConstantInFormulaTooLong
										|| ex.ErrorCode == FlxErr.ErrFormulaTooLong
										|| ex.ErrorCode == FlxErr.ErrFormulaStart
										|| ex.ErrorCode == FlxErr.ErrFormulaInvalid));
			}
		}

		internal void ReportErrors()
		{
			if (ShouldReportError)
			{
				if (report.ScheduleTask != null && !HasWarningsOnly)
				{
					report.ScheduleTask.S5_IsActive = false;
				}
				if (report.ScheduleTask != null && ContainsOnlyFilterValidationErrors)
				{
					SendErrorMessageToLocalUserToDealWithTheIssue(report.ScheduleTask, ErrorTitle, ErrorMessageForUser);
				}
				else
				{
					StmTemplate template = report.StTemplate;
					if ((template != null && !template.SO_IsSystemDefined))
					{
						SendErrorMessageToLocalUserToDealWithTheIssue(template, ErrorTitle, ErrorMessageForUser);
					}
					else if (!ContainsOnlyNotReportedWarningsOrErrors && !report.ContainsAnyCustomisation && !ContainsOnlyErrFormula)
					{
						var reportProcessingException = new ReportProcessingException(ErrorTitle, ErrorMessageForUser, lastFatalException); // Using the message without stacktrace here to avoid duplication in Issue Manager
						ErrorReporter.ReportOnce(GetKey(), ErrorMessageForErrorReport, reportProcessingException);
					}
				}
			}
		}

		bool ShouldReportError
		{
			get
			{
				if (report.Parent == null || !report.Parent.IsRunFromMenusCustomisationForm)
				{
#if DEBUG
					bool isUsingDeveloperLogin = false;

					IUser currentUser = EnvProxy.Instance.CurrentUser;
					if (currentUser != null)
					{
						isUsingDeveloperLogin = currentUser.IsDeveloperLogin;
					}

					return !isUsingDeveloperLogin || !Globals.IsUserInteractive || Globals.IsTest;
#else
					return true;
#endif
				}
				else
				{
					return false;
				}
			}
		}

		void SendErrorMessageToLocalUserToDealWithTheIssue(EnterpriseBusinessObject businessObject, string errorTitle, string errorMessage)
		{
			try
			{
				EmailDef email = new EmailDef();
				email.Subject = errorTitle;
				email.Body = DbCommand.SanitizeExecuteAsReaderFlags(errorMessage);

				var excludeEnterpriseBatchProcessorQuery = new ZQuery(StmALogSchema.SL_GS_NKUser, SQLComparisonOperator.NotEqual, User.ServiceUserCode);
				StmALog stmALog = businessObject.Logs.MostRecentLogByEventTime(Events.EditedARecord, excludeEnterpriseBatchProcessorQuery) ?? businessObject.Logs.MostRecentLogByEventTime(Events.AddedARecordToTheSystem, excludeEnterpriseBatchProcessorQuery);

				IGlbStaff user = stmALog != null ? stmALog.User : null;
				if (user != null && !user.GS_EmailAddress.IsEmpty)
				{
					GlbStaff staff = user as GlbStaff;
					if (staff == null || staff.GS_IsActive)
					{
						email.AddRecipientForUserCommunication(user.GS_EmailAddress, RecipientDef.RecipientTypes.CC);
					}
				}

				IUser currentUser = Env.CurrentUser;
				if (currentUser != null && currentUser.IsActive && !string.IsNullOrEmpty(currentUser.EmailAddress))
				{
					email.AddRecipientForUserCommunication(currentUser.EmailAddress, RecipientDef.RecipientTypes.TO);
					Env.OutgoingMailManager.CreateAndSave(email);
				}
				else
				{
					Env.OutgoingMailManager.CreateAndSave(email, Core.Constants.Groups.PostMastersGroupPK, GroupSourceLocator.GetFromRegistryItem(Env.Registry.RawRegistry.NotificationGroup));
				}
			}
			catch (EmailHasNoRecipientsException exception)
			{
				ErrorReporter.ReportOnce("Error sending report processing failure email (no recipients)", exception);
			}
			catch (EmailNotCompleteException exception)
			{
				if (exception is EmailHasNoFromAddressException)
				{
					Add(new ReportProcessingError(Res.GetString("DB45CBE4-2048-4092-9BB8-0170B9D2AC67", "Could not send the report error to related user via email because the 'from address' was empty. Message: {0}", exception.Message), ReportProcessingErrorSeverity.Error, exception));
				}
				else
				{
					ErrorReporter.ReportOnce("Error sending report processing failure email (email not complete)", exception);
				}
			}
		}

		IReportProcessingError GetReportProcessingError(Exception exception, ReportProcessingErrorSeverity severity)
		{
			severity = AdjustSeverityForReporting(exception, severity);
			var message = ExceptionShouldBeShownToTheUser(exception) ? exception.Message : exception.GetType().Name + ": " + exception.Message;
			var exceptionWithLocation = exception as TemplateDefinitionException;
			CellReference cell = exceptionWithLocation != null ? exceptionWithLocation.CellReference : CurrentCell;

			return new ReportProcessingError(message, cell, severity);
		}

		ReportProcessingErrorSeverity AdjustSeverityForReporting(Exception exception, ReportProcessingErrorSeverity severity)
		{
			var sqlException = exception.GetBaseException() as SqlException;
			var invalidGroupByColumnException = exception.GetBaseException() as InvalidGroupByColumnException;
			return report.ContainsAnyCustomisation
				|| (sqlException != null && !ZExceptionExtensions.IsSqlExceptionReportable(sqlException))
				|| (invalidGroupByColumnException != null && report.VisualizerContentNote != null && report.VisualizerContentNote.IsInDatabase)
				? severity.GetNonReportableEquivalent()
				: severity;
		}

		CellReference CurrentCell;

		internal IDisposable EvaluatingCell(CellReference cellReference)
		{
			return new DisposableRecorder<CellReference>(value => CurrentCell = value, cellReference, CurrentCell);
		}

		string TemplatePath
		{
			get { return report.SourceFile; }
		}

		public CargoWise.ComponentModel.INotificationType GetNotificationType()
		{
			var result = NotificationType.Information;
			foreach (var error in errors)
			{
				var errorNotification = error.Severity.ToNotificationType();
				if (errorNotification == NotificationType.Warning && result != NotificationType.Error) { result = NotificationType.Warning; }
				if (errorNotification == NotificationType.Error) { result = NotificationType.Error; }
			}
			return result;
		}

		public IEnumerable<IReportError> GetReportErrors()
		{
			foreach (var error in errors)
			{
				yield return new ReportError(error.Message, error.Severity.ToNotificationType());
			}
		}

		/// <summary>
		/// Will return a formatted list of all errors found, or a "No Errors Found" message.
		/// </summary>
		/// <param name="format">A Format string handling the following parameters:
		/// {0}-Severity, 
		/// {1}-Message, 
		/// {2}-Cell Reference, 
		/// {3}-Sheet Name, 
		/// {4}-Template Path, 
		/// {5}-Outer Content (Original Cell Content / Macro), 
		/// {6}-Occurrences
		/// {7}-Optional Column Details
		/// {8}-Exception Details
		/// </param>
		/// <param name="includeReportInformation">If true, includes a preformatted header string identifying the Report / Document Menu Items / Template rows.</param>
		/// <param name="includeExceptionDetails">If true, includes the exception or stack trace which caused each error.</param>
		/// <returns></returns>
		public string ToString(string format, bool includeReportInformation, bool includeExceptionDetails = false)
		{
			string finalString;
			if (HasErrors)
			{
				var result = new List<string>();
				foreach (var error in errors)
				{
					var severity = error.Severity == ReportProcessingErrorSeverity.WarningWithoutErrorReport
								|| error.Severity == ReportProcessingErrorSeverity.FatalWithoutErrorReport
								|| error.Severity == ReportProcessingErrorSeverity.ErrorWithoutErrorReport
						? error.Severity.ToStringFormatted()
						: error.Severity.ToString();

					var exceptionDetails = includeExceptionDetails ? "\r\n" + ((object)error.Exception ?? "").ToString() : "";
					var message = string.Format(format,
						severity, //0
						error.Message, //1
						error.CellName, //2
						error.SheetName, //3
						error.TemplatePath, //4
						error.OuterContent, //5
						error.Occurrences, //6
						GetErrorMessageForOptionalColumnIfNeeded(error), //7
						exceptionDetails); //8

					result.Add(message);
				}
				result.Sort();
				finalString = string.Join("\r\n", result.ToArray());
			}
			else
			{
				finalString = HasNoErrors;
			}
			if (includeReportInformation)
			{
				finalString = finalString + "\r\n\r\n" + report.ToString();
			}
			return finalString;
		}

		string GetErrorMessageForOptionalColumnIfNeeded(IReportProcessingError error)
		{
			string result = string.Empty;

			if (error.Severity == ReportProcessingErrorSeverity.Warning && error.Message.Contains((NoResString)"is not found in Table"))
			{
				result = string.Format((NoResString)" Optional Column Indices: [{0}]", string.Join(",", report.Analyser.NonExistantOptionalColumns.Select(x => x.ToString())));

				var hiddenColumn = report.Analyser.Config.HideColumnExpressions.FirstOrDefault(pair => pair.Value.Contains(error.Message));
				if (hiddenColumn.Value != null)
				{
					int columnIndex = hiddenColumn.Key;
					ZBool optionalColumnContainsCell = report.Analyser.NonExistantOptionalColumns.Contains(columnIndex);
					result += string.Format((NoResString)" Cell Column Index: [{0}] Optional Column Contains Cell: [{1}]", columnIndex, optionalColumnContainsCell.ToYN());
				}
				else
				{
					result += (NoResString)" Optional Column Contains Cell: [N]";
				}
			}

			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		internal const string HasNoErrors = "ReportErrorManager has no errors";

		public override string ToString()
		{
			return ToString((NoResString)"Severity: [{0}] Message: [{1}] Cell: [{2}] Sheetname: [{3}]", false, false);
		}

		bool IsErrorCheckingSuspendedFor(IReportProcessingError error)
		{
			bool result = false;
			foreach (var suspender in errorCheckingSuspenders)
			{
				result |= suspender.ErrorShouldBeHidden(error);
			}
			return result;
		}

		internal IErrorCheckingSuspender GetErrorCheckingSuspender(Predicate<IReportProcessingError> errorShouldBeHidden)
		{
			return new ErrorCheckingSuspender(this, errorShouldBeHidden);
		}

		internal IErrorCheckingSuspender GetErrorCheckingSuspender()
		{
			return new ErrorCheckingSuspender(this, error => true);
		}

		internal interface IErrorCheckingSuspender : IDisposable
		{
			bool HadError { get; }
			bool HidError { get; }
		}

		readonly List<ErrorCheckingSuspender> errorCheckingSuspenders = new List<ErrorCheckingSuspender>();

		class ErrorCheckingSuspender : IErrorCheckingSuspender
		{
			public ErrorCheckingSuspender(ReportErrorManager parent, Predicate<IReportProcessingError> errorShouldBeHidden)
			{
				this.parent = parent;
				this.errorShouldBeHidden = errorShouldBeHidden;
				parent.errorCheckingSuspenders.Add(this);
			}
			readonly ReportErrorManager parent;
			readonly Predicate<IReportProcessingError> errorShouldBeHidden;

			internal bool ErrorShouldBeHidden(IReportProcessingError error)
			{
				hadError = true;
				bool result = errorShouldBeHidden(error);
				if (result)
				{
					hidError = true;
				}
				return result;
			}

			void IDisposable.Dispose()
			{
				parent.errorCheckingSuspenders.Remove(this);
			}

			bool IErrorCheckingSuspender.HadError
			{
				get { return hadError; }
			}
			bool hadError;

			bool IErrorCheckingSuspender.HidError
			{
				get { return hidError; }
			}
			bool hidError;
		}

		internal IDisposable ReportAllErrorsWithSeverity(ReportProcessingErrorSeverity? severity)
		{
			return new DisposableRecorder<ReportProcessingErrorSeverity?>(value => overriddenSeverity = value, severity, overriddenSeverity);
		}
		ReportProcessingErrorSeverity? overriddenSeverity;

		internal IDisposable EvaluatingOuterContent(object cellContent)
		{
			var cellContentToRecord = OuterContent ?? FormatOuterCellContentHandlingFormulas(cellContent).ShrinkToMaxLength(250);
			return new DisposableRecorder<string>(value => OuterContent = value, cellContentToRecord, OuterContent);
		}

		internal IDisposable EvaluatingInnerMacro(string macro)
		{
			return new DisposableRecorder<string>(value => InnerMacro = value, macro, InnerMacro);
		}

		internal string OuterContent { get; private set; }
		string InnerMacro;
		Exception lastFatalException;

		class DisposableRecorder<T> : IDisposable
		{
			internal DisposableRecorder(Void<T> setter, T valueToSet, T originalValue)
			{
				this.setValue = setter;
				this.originalValue = originalValue;

				setValue(valueToSet);
			}

			internal delegate void Void<R>(R parameter);

			readonly Void<T> setValue;
			readonly T originalValue;

			void IDisposable.Dispose()
			{
				setValue(originalValue);
			}
		}

		static string FormatOuterCellContentHandlingFormulas(object cellContent)
		{
			if (cellContent == null)
			{
				return string.Empty;
			}

			TFormula cellFormula = cellContent as TFormula;
			if (cellFormula != null)
			{
				cellContent = cellFormula.Text;
			}

			return cellContent.ToString().Replace("\r\n", "|>").Replace("\r", "|>").Replace("\n", "|>");
		}

		internal string GetKey()
		{
			string reportName = report.Name;
			string menuItemID = (NoResString)"(none found)";
			string templateID = (NoResString)"(none found)";
			IStmMenuItem menuItem = report.MenuItem;
			if (menuItem != null)
			{
				string menuNameWithPath = menuItem.SU_MenuPath + (string.IsNullOrEmpty(menuItem.SU_MenuPath) || menuItem.SU_MenuPath.EndsWith("/") ? "" : "/") + menuItem.SU_MenuName;
				menuItemID = string.Format((NoResString)"PK: [{0}]  BusinessContext: [{1}]  Name/Path: [{2}]  Filter: [{3}]  IsSystemDefined: [{4}]  IsClientSpecific: [{5}]"
					, menuItem.PK, menuItem.SU_BusinessContext, menuNameWithPath, menuItem.SU_FilterList, menuItem.SU_IsSystemDefined.ToYN(), menuItem.SU_IsClientSpecific.ToYN());
			}

			StmTemplate template = report.StTemplate;
			if (template != null)
			{
				templateID = string.Format((NoResString)@"PK: [{0}]  Name: [{1}]  DataContext: [{2}]  ExcelFilePath: [{3}]  IsSystemDefined: [{4}]  IsClientSpecific: [{5}]"
					, template.PK, template.SO_Name, template.SO_DataContext, template.SO_ExcelTemplatePath, template.SO_IsSystemDefined.ToYN(), template.SO_IsClientSpecific.ToYN());
			}

			string errorsFound = this.ToString("Severity: [{0}] Message: [{1}] Sheetname: [{3}] Cell Content: [{5}]", false, false);
			errorsFound = new Regex(
				@"occurred running SQL: \[(.*)\]. SqlException: Msg \d+, Level \d+, State \d+, Line \d+").Replace(errorsFound,
				@"occurred running SQL: [(snipped)]. SqlException: Msg #, Level #, State #, Line #");

			string keyString = (NoResString)@"Error Generating Report [{0}]
MenuItem {1}
Template {2}
--------------- Errors Found ---------------
{3}";

			return string.Format(keyString, reportName, menuItemID, templateID, errorsFound);
		}
	}
}
