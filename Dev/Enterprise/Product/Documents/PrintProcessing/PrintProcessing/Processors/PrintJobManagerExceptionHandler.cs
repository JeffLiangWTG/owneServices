using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.DocumentEngineCore.Exceptions;
using Enterprise.DocumentScanning.Integration;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using FlexCel.Core;
using FlexCel.XlsAdapter;

namespace Enterprise.PrintProcessing
{
	internal class PrintJobManagerExceptionHandler
	{
		public PrintJobManagerExceptionHandler(PrintJobManager.ProgressDelegate logProgress)
		{
			LogProgress = logProgress;
		}

		readonly PrintJobManager.ProgressDelegate LogProgress;

		/// <summary>
		/// Will notify us of a failure to deliver after MaxRetryAttempts at delivering the job. 
		/// If the print job belongs to a batch of jobs that should be merged, then all print 
		/// jobs in that same merged job will have the failure rate incremented. 
		/// </summary>
		internal void HandlePrintException(StmPrintJobMergedCollection mergedPrintJobs, Exception exception)
		{
			mergedPrintJobs.IncrementRetryAttempts();

			if (mergedPrintJobs.RetryAttempts >= PrintJobManager.MaxRetryAttempts)
			{
				var errorHeader = Res.GetString("962c63c5-8b9e-4c79-82fd-dabab0b7a2bf"
					, "Error processing print job - giving up after {0} attempts (affecting a total of {1} print job(s))"
					, PrintJobManager.MaxRetryAttempts
					, mergedPrintJobs.Count);

				var exceptionParser = new ExceptionParser(exception);

				var messageBody = errorHeader + "\r\n\r\n" + exceptionParser.MessageForUser + "\r\n\r\n" + GetJobDetails(mergedPrintJobs, false);
				var logType = exceptionParser.IsKnownException ? TraceEventType.Warning : TraceEventType.Error;
				LogProgress(logType, messageBody);

				if (!exceptionParser.IsKnownException)
				{
					ErrorReporter.ReportOnce(exceptionParser.MessageForCargoWise + "\r\n\r\n" + GetJobDetails(mergedPrintJobs, true), exception);
				}

				var failReason = exceptionParser.MessageForUser;
				if (string.IsNullOrEmpty(failReason))
				{
					failReason = errorHeader;
				}
				mergedPrintJobs.SetFailureReason(failReason);

				EmailExceptionReportToLocalUser(mergedPrintJobs, messageBody);
			}
			else
			{
				var message =
					Res.GetString("351f6cb1-3c8b-43e6-acba-3d3e2af065ef",
						"Error processing print job on {0} attempt - {1} affected print jobs will be reprocessed later again",
						mergedPrintJobs.RetryAttempts, mergedPrintJobs.Count) + "\r\n\r\n" +
					new ExceptionParser(exception).MessageForUser + "\r\n\r\n" +
					GetJobDetails(mergedPrintJobs, false).ToString();
				LogProgress(TraceEventType.Warning, message);
			}
		}

		class ExceptionParser
		{
			[SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
			internal ExceptionParser(Exception exception)
			{
				string exceptionExplanation = null;
				if (exception is System.Drawing.Printing.InvalidPrinterException)
				{
					exceptionExplanation = Res.GetString("089514ca-2200-47a2-a433-63cf81e1b358", @"Printer selected is no longer valid or not currently available / on-line.");
				}
				else if (exception is System.ComponentModel.Win32Exception win32Exception)
				{
					exceptionExplanation = Res.GetString("089514ca-2200-47a2-a433-63cf81e1b359", @"Driver Communication Error sending job to selected Printer. {0}", win32Exception.Message);
				}
				else if ((exception is BitmapCreationException) && (!((BitmapCreationException)exception).IsDimensionOutsideRange))
				{
					exceptionExplanation = Res.GetString("089514ca-2200-47a2-a433-63cf81e1b357", @"There was an error rendering an image - Could not allocate resources to render a Bitmap for Document Production.");
				}
				else if (exception is EDocsOffLineException)
				{
					exceptionExplanation = Res.GetString("0821ed1a-f1d8-4311-83bd-84ae533df360", @"There is an error with the Database Server. Please inform your System Administrator of this problem.");
				}
				else if (exception is FlexCelXlsAdapterException && ((FlexCelXlsAdapterException)exception).ErrorCode == XlsErr.ErrFileIsNotSupported)
				{
					exceptionExplanation = Res.GetString("d1e97bbe-7be1-405c-aa5d-a8e7d172749f", @"The file is not on any on the formats supported by {0}.", "FlexCel");
				}
				else if (exception is ImageFormatException)
				{
					exceptionExplanation = Res.GetString("bf93467c-83ae-4602-8c98-44a630eaf9be", @"There is an error with image format.");
				}
				else if (exception is ExcelInterfaceException && ((ExcelInterfaceException)exception).Type == ExcelInterfaceExceptionType.ErrorFontNotFound)
				{
					exceptionExplanation = Res.GetString("12174591-fa1e-492c-9008-e3afffe855b9", @"A font was used in a template that is not installed on the machine running your Printing Service Tasks.

Please either install this font on the machine running your Printing Service Tasks or remove it from the template.");
				}
				else if (exception is ExcelInterfaceException && ((ExcelInterfaceException)exception).Type == ExcelInterfaceExceptionType.ErrorFontNotSupported)
				{
					exceptionExplanation = Res.GetString("e4901b3c-2f04-4f0b-a8ab-ef138363b530", @"A font was used in a template that is not installed or corrupted on the machine running your Printing Service Tasks.

Please either install this font on the machine running your Printing Service Tasks or remove it from the template.");
				}
				else if (exception is EmailNotCompleteException)
				{
					exceptionExplanation = Res.GetString("513f4bc6-4b06-469b-a2b5-7c8a90a29abd", @"A Print Task was created with an invalid Destination Email Address. Please verify the Email Address is valid.");
				}
				else if (exception.IsGdiPlusException())
				{
					exceptionExplanation = Res.GetString("4eefc374-09b0-4bde-a6e0-933ab943ac43", @"A generic error occurred in GDI+.

Microsoft GDI+ (graphical rendering engine used for drawing graphics and printing document) has had a generic failure rendering a document. 
This can be caused by a machine being low on memory, out of window handles, having a problem with a video driver, or some sort of memory corruption creating an unexpected un-handled machine state.

If these errors continue to occur, please try rebooting this machine, adding more memory to this machine, or moving this process to another machine.");
				}
				else if (exception is IOException || exception is UnauthorizedAccessException)
				{
					exceptionExplanation = Res.GetString("e2f0a2d9-402b-496a-87a1-5c8b341fd211", @"An IO error occurred. Please check user permissions, security settings, that the folder exists and that there are no network errors/mis-configurations.");
				}
				else if (exception is FlexCelCoreException && ((FlexCelCoreException)exception).ErrorCode == FlxErr.ErrFontNotSupported)
				{
					exceptionExplanation = Res.GetString("608b94ee-f8B1-42e6-af8C-63c6db665340", @"A font was used in a template that is not installed or corrupted on the machine running your Printing Service Tasks.

Please either install this font on the machine running your Printing Service Tasks or remove it from the template.");
				}
				else if (exception is ZSaveConcurrencyException)
				{
					exceptionExplanation = Res.GetString("6F1B1FB2-C271-4B32-8E5E-7DBF0F960729", @"While your Printing Service Tasks were running, another user had made changes to the job. Current task will abort due to concurrency errors.");
				}
				else if (exception is SqlException sqlException && sqlException.Number == 21)
				{
					exceptionExplanation = Res.GetString("7369def0-c046-42a4-85cd-138192f60abc", "There are some errors on your server. Please contact your system administrator to check your server windows event logs.");
				}
				else if (exception is InvalidDataException)
				{
					exceptionExplanation = Res.GetString("2A811987-852A-464E-99CE-7A03C98B0337", "There's some corrupted data when processing print jobs.");
				}
				else if (exception is ExternalStorageException ex)
				{
					exceptionExplanation = ex.UnableToAccessStorageFriendlyMessage;
				}

				if (!string.IsNullOrEmpty(exceptionExplanation))
				{
					MessageForUser = Res.GetString("3eb966d8-0acc-4a81-a0df-60bcf106419c"
						, @"{0}

Error Message is: {1}"
						, exceptionExplanation
						, exception.GetFullMessage());

					IsKnownException = true;
				}
				else
				{
					MessageForCargoWise = (NoResString)"Exception Processing Print Jobs: " + exception.GetFullMessage(); // it's only used for reporting errors back to cargowise
					MessageForUser = Res.GetString("089514ca-2200-47a2-a433-63cf81e1b356", @"An exception report has been sent back to CargoWise for further investigation.

Message: [{0}]
Type:    [{1}]
{3}: [{2}]"
						, MessageForCargoWise
						, exception.GetType().FullName
						, exception.GetHResult().ToString()
						, "HResult");
				}
			}

			internal readonly bool IsKnownException;
			internal readonly string MessageForUser;
			internal readonly string MessageForCargoWise;
		}

		void EmailExceptionReportToLocalUser(StmPrintJobMergedCollection mergedPrintJobs, ZString messageBody)
		{
			try
			{
				EmailDef failureEmail = new EmailDef();
				failureEmail.Subject = Res.GetString("a05d2d13-f148-4a5e-a977-a2e94df80957", "Error Processing Print Job(s)");
				failureEmail.Body = messageBody;

				ZString emailAddress = GetLocalEmailAddressIfAvailable(mergedPrintJobs);
				if (!emailAddress.IsEmpty && EmailAddressValidation.IsEmailAddressValid(emailAddress))
				{
					failureEmail.AddRecipientForUserCommunication(emailAddress);
					Env.OutgoingMailManager.CreateAndSave(failureEmail);
				}
				else
				{
					Env.OutgoingMailManager.CreateAndSave(failureEmail, Core.Constants.Groups.PostMastersGroupPK, GroupSourceLocator.GetFromRegistryItem(Env.Registry.RawRegistry.NotificationGroup));
				}
			}
			catch (Exception ex)
			{
				if (ex.IsCriticalException())
				{
					throw;
				}

				string emailingFailureFailureMessage = Res.GetString("8106dfab-7af4-4e6b-9804-e6afa0294740", "Error Sending Print Job Failure Email:- {0}", ex.Message);
				LogProgress(TraceEventType.Error, emailingFailureFailureMessage);

				if (!(ex is EmailSendFailedException))
				{
					ErrorReporter.ReportOnce(emailingFailureFailureMessage, ex);
				}
			}
		}

		static ZString GetLocalEmailAddressIfAvailable(StmPrintJobMergedCollection mergedPrintJobs)
		{
			ZString emailAddress = "";
			foreach (StmPrintJob printJob in mergedPrintJobs)
			{
				if (printJob.Staff != null && !printJob.Staff.GS_EmailAddress.IsEmpty)
				{
					emailAddress = printJob.Staff.GS_EmailAddress;
					break;
				}
			}

			if (emailAddress.IsEmpty)
			{
				emailAddress = Env.CurrentUser.EmailAddress;
			}

			return emailAddress;
		}

		internal static ZString GetJobDetails(StmPrintJobMergedCollection mergedPrintJobs, bool isReportingToCargowise)
		{
			ZStringBuilder result = new ZStringBuilder();
			int index = 1;
			int total = mergedPrintJobs.Count;
			foreach (StmPrintJob printJob in mergedPrintJobs)
			{
				ZString printJobsDetail = Res.GetString("a94df8cd-ac01-4050-bfb7-8069210db629", @"--- Print Job {0} of {1} --------------
Job Type: [{2}]
Document Name: [{3}]
No. of Copies: [{4}]
Fax Destination: [{5}]
Email To: [{6}]
Email Subject: [{7}]
Email Attachments: [{8}]",
				index++, total, printJob.SP_JobType, printJob.SP_DocumentName, printJob.SP_Copies, printJob.SP_FaxDestination, printJob.EmailToRecipients.Value, printJob.SP_EmailSubjectLine, printJob.SP_EmailAttachments);

				if (isReportingToCargowise)
				{
					printJobsDetail += "\r\n" + Res.GetString("8a2a9c43-a829-43cb-ad9d-429ab9c14ca6", "Parent Table: [{0}]\r\nParent PK: [{1}]", printJob.SP_ParentTableName, printJob.SP_ParentGuid);
				}
				else if (!printJob.SP_ParentTableName.IsEmpty)
				{
					printJobsDetail += "\r\n" + Res.GetString("213e06ee-b960-40a3-a972-73456da56a35", "Parent Table: [{0}]", printJob.SP_ParentTableName);
				}

				result.Append(printJobsDetail);
			}
			return result.ToStringWithDelimiterBetweenAppends("\r\n\r\n");
		}
	}
}
