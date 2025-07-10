using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Invoicing.Printing;
using Enterprise.Accounting.GUI;
using Enterprise.Accounting.GUI.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.DocumentEngine;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Module
{
	public partial class PeriodicInvoiceBulkPosterController : INotificationHandler
	{
		public PeriodicInvoiceBulkPosterController(PeriodicInvoiceBulkPoster poster, Form parentForm)
		{
			this.poster = poster;
			this.parentForm = parentForm;
		}

		public void StartPosting()
		{
#if DEBUG
			if (Globals.IsTest)
			{
				LastStackTraceInStartPosting_ForTestOnly = new StackTrace();
			}
#endif
			HookEvents();
			if (ParentForm == null)
			{
				ProgressForm.Show();
			}
			else
			{
				ProgressForm.ShowModalTo(ParentForm);
			}
#if DEBUG
			if (Globals.IsTest)
			{
				LastProgressFormForTestOnly = ProgressForm;
			}
#endif
			ProgressForm.SetStatusAndPercentComplete(Res.GetString("79ccf156-ef74-454e-88e8-2976f7f6021c", "Initializing Posting..."), 0);

			Action postAction = Poster.Post;

			if (!progressForm.IsDisposedOrHasDisposedParent())
			{
				ProgressForm.BeginInvoke(postAction); //BeginInvoke is used to allow to finalise Bulk Periodice Invoice form closing before posting start.
			}
		}

		void HookEvents()
		{
			Poster.PostingStarted += Poster_PostingStarted;
			Poster.PostingProgress += Poster_PostingProgress;
			Poster.PostingFinished += Poster_PostingFinished;
			ProgressForm.CancellingQuery += ProgressForm_CancellingQuery;
			Poster.NegativeComplianceFailedToCreate += Poster_NegativeComplianceFailedToCreate;
		}

		void UnHookEvents()
		{
			Poster.PostingStarted -= Poster_PostingStarted;
			Poster.PostingProgress -= Poster_PostingProgress;
			Poster.PostingFinished -= Poster_PostingFinished;
			ProgressForm.CancellingQuery -= ProgressForm_CancellingQuery;
			Poster.NegativeComplianceFailedToCreate -= Poster_NegativeComplianceFailedToCreate;
		}

		void Poster_NegativeComplianceFailedToCreate(object sender, EventArgs e)
		{
			foreach (InvoicingBase invoicingBase in sender as InvoicingBaseCollection)
			{
				if (invoicingBase is ARInvoice || invoicingBase is ARCreditNote || invoicingBase is APInvoice || invoicingBase is APCreditNote)
				{
					invoicingBase.OnNegativeCompliancesFailedToCreate += new EventHandler(NegativeCompliancesFailedToCreate);
				}
			}
		}

		void NegativeCompliancesFailedToCreate(object sender, EventArgs e)
		{
			Globals.Message.ShowWarning(AccountingConstants.GetComplianceDocumentNegativeMessage());
		}

		void Poster_PostingStarted(int numberOfInvoicesToPost)
		{
			NumberOfInvoicesToPost = numberOfInvoicesToPost;
			NumberOfInvoicesFailed = 0;
			StartTime = ZDateTime.Now;
			ProgressForm.ResetLog();
			ProgressForm.SetStatusAndPercentComplete(GetProgessStatusText(0), 0);
			RecordLogMessage(Res.GetString("138a7e02-1366-4bd1-8526-6121b1d951b5", "Posting is started."), false, false);
#if DEBUG
			if (Globals.IsTest)
			{
				LastProgressFormForTestOnly = ProgressForm;
			}
#endif
		}

		void Poster_PostingProgress(int numberOfInvoicesProcessed, string message, bool isFailed, Exception exception)
		{
			int percentComplete = (int)(100.0 * numberOfInvoicesProcessed / NumberOfInvoicesToPost);
			if (isFailed)
			{
				NumberOfInvoicesFailed++;
			}
			ProgressForm.SetStatusAndPercentComplete(GetProgessStatusText(numberOfInvoicesProcessed), percentComplete);

			if (exception != null)
			{
				HandleException(message, exception);
			}
			else if (!string.IsNullOrEmpty(message))
			{
				RecordLogMessage(message, isFailed);
			}
		}

		void Poster_PostingFinished(bool isCancelled, int numberOfInvoicesProcessed, ZGuid[] postedInvoicePKs, string errorMessage, Exception exception)
		{
			try
			{
				if (exception != null)
				{
					HandleException(errorMessage, exception);
				}
				else if (!string.IsNullOrEmpty(errorMessage))
				{
					RecordLogMessage(errorMessage, true);
				}
				else
				{
					string messagePrefix = isCancelled ? Res.GetString("a84c75eb-c6dd-4d1d-93cc-9ddd9a386a74", "Posting is canceled.") :
														 Res.GetString("b729ae85-2e10-453d-bb4b-2f3e8dfec020", "Posting is finished.");
					string statusText = GetProgessStatusText(numberOfInvoicesProcessed);
					ProgressForm.SetStatusAndPercentComplete(statusText, 100);
					RecordLogMessage(string.Format("{0} {1}{2}", messagePrefix, statusText, GetErrorsSummary()), false, false);

					if (postedInvoicePKs.Length > 0)
					{
						var doPrinting = GetUserChoice();
						if (doPrinting == YesNoYesAllNoAllMessageBoxResult.YesToAll)
						{
							var postedInvoices = new BusinessObjectFactory().Load<InvoicingBase>(new ZQuery(AccTransactionHeaderSchema.PK, postedInvoicePKs));
							List<InvoicingBase> enterpriseInvoices = new List<InvoicingBase>();
							List<InvoicingBase> governmentInvoices = new List<InvoicingBase>();
							string invoicePrintingOption = AccountingConfigurationRegistry.Instance.InvoicePrintingOption.Value;

							if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.China) // China always print Enterprise invoice here- see WI00045869
							{
								foreach (var postedInvoice in postedInvoices)
								{
									enterpriseInvoices.Add(postedInvoice);
								}
							}
							else
							{
								foreach (var postedInvoice in postedInvoices)
								{
									if (postedInvoice.IsGovtTaxInvoice &&
										(invoicePrintingOption == GovtTaxInvoicePrintTask.GovtTaxInvoice || invoicePrintingOption == GovtTaxInvoicePrintTask.BothGovtTaxAndEnterpriseInvoice))
									{
										governmentInvoices.Add(postedInvoice);
									}
									else // i.e. EnterpriseInvoice
									{
										enterpriseInvoices.Add(postedInvoice);
									}
								}
							}

							if (enterpriseInvoices.Count > 0)
							{
								var printTask = new InvoicePrintTask(new InvoicePrintTask.Configuration(enterpriseInvoices.ToArray()));
								printTask.Run();
#if DEBUG
								enterpriseInvoicesPrintTaskForTesting = printTask.GetTask();
#endif
							}

							if (governmentInvoices.Count > 0)
							{
								var printer = new GovtTaxInvoicePrinter(invoicePrintingOption);
								printer.PrintGovtTaxInvoices(governmentInvoices.ToArray());
#if DEBUG
								governmentInvoicesPrintTaskForTesting = printer.printeTaskForTesting;
#endif
							}
						}
						else if (doPrinting == YesNoYesAllNoAllMessageBoxResult.Yes)
						{
							InvoicePrinter printer = null;
							var postedInvoices = new BusinessObjectFactory().Load<InvoicingBase>(new ZQuery(AccTransactionHeaderSchema.PK, postedInvoicePKs));
							foreach (var postedInvoice in postedInvoices)
							{
								(printer ?? (printer = new InvoicePrinter())).PrintTransaction(postedInvoice, ParentForm, InvoicePrintContext.DontCare);
							}
						}
					}
				}
			}
			catch (Exception innerEx) when (!innerEx.IsCriticalException())
			{
				ExceptionReporter.Instance.ReportException(GetType().FullName, innerEx);
			}
			finally
			{
				UnHookEvents();
				ResetProgressForm();
			}
		}

		protected virtual YesNoYesAllNoAllMessageBoxResult GetUserChoice()
		{
			return YesNoYesAllMessageBox.Show(Res.GetString("7593e728-d35e-4b7d-b3f7-8cca065aaf70", "Do you want to print any of the posted transactions?"), MessageBoxCaption, DeliveryAllText);
		}

		void HandleException(string errorMessage, Exception exception)
		{
			try
			{
				RecordLogMessage(errorMessage, true);
				ZExceptionReporting.HandleSaveException(exception, NotificationHandler);
			}
			catch (ZSaveConcurrencyException resolvingException)
			{
				RecordLogMessage(Res.GetString("7851fb69-0e2e-44e9-ba31-1562ac422ba1",
					"There was an unresolved concurrency error preventing posting this Periodic Invoice. Please try to reopen it and Post again.{0}{1}",
					System.Environment.NewLine, resolvingException.Message), true, false, false);
			}
		}

		string GetProgessStatusText(int numberOfInvoicesProcessed)
		{
			int numberOfInvoicesPosted = numberOfInvoicesProcessed - NumberOfInvoicesFailed;
			string invoicesAreText = Res.GetString("856d3321-90b0-4669-8c63-5ba33609ffe7", "invoices are");
			string theyText = Res.GetString("e9872ea1-386f-4a21-8647-aea898c5934a", "they");
			if (numberOfInvoicesPosted == 1)
			{
				invoicesAreText = Res.GetString("253c72bf-5073-482b-9d7e-34f4323cb60e", "invoice is");
				theyText = Res.GetString("02a4ef9e-14fb-4e29-9069-2e4ac4163ab3", "it");
			}
			cancelQuestionText = Res.GetString("0d745f97-e226-4184-af75-0469127f785b",
@"You are about to cancel Bulk Periodic Invoice posting. {0} {1} posted already and {2} won’t be rolled back.
Do you want to cancel posting?",
			numberOfInvoicesPosted, invoicesAreText, theyText);
			return Res.GetString("822ed17e-5911-4729-aa68-18f597c67adb", "Posted {0}{3} of {1} periodic invoices.\r\nElapsed time: {2}",
				numberOfInvoicesPosted, NumberOfInvoicesToPost, ElapsedTimeFormatted,
				NumberOfInvoicesFailed > 0 ? Res.GetString("48d5bee3-47c9-41de-9702-151cf22219fc", ", failed {0}", NumberOfInvoicesFailed) : "");
		}

		void RecordLogMessage(string message, bool reportError, bool showIterationResult = true, bool showTime = true)
		{
			string iterationResult = string.Empty;
			string notificationFormat = "{0}";
			if (showIterationResult)
			{
				notificationFormat = "{1} - {0}";
				iterationResult = reportError ? Res.GetString("0fce2fa1-c5d3-4924-8431-f10df9c347a3", "ERROR") :
											 Res.GetString("8cddf01f-6242-4963-9c2e-904b08db09b0", "Success");
			}

			string logRecord = string.Format("{0}: {1}", ZDateTime.Now.ToLongTimeString(), string.Format(notificationFormat, message, iterationResult));
			if (reportError)
			{
				NotificationHandler.ReportError(logRecord, "");
			}
			else
			{
				NotificationHandler.ReportInformation(logRecord, "");
			}
		}

		string GetErrorsSummary()
		{
			return ErrorsList.IsEmpty ? string.Empty : ("\r\n\r\n" + Res.GetString("230e4acc-35fe-4ad7-b156-3847dc60ee35", "Errors:\r\n{0}", ErrorsList));
		}

		ZStringBuilder ErrorsList
		{
			get { return errorsList ?? (errorsList = new ZStringBuilder()); }
		}
		ZStringBuilder errorsList;

		PeriodicInvoiceBulkPoster Poster
		{
			get { return poster; }
		}
		readonly PeriodicInvoiceBulkPoster poster;

		Form ParentForm
		{
			get { return parentForm; }
		}
		readonly Form parentForm;

		int NumberOfInvoicesToPost { get; set; }

		int NumberOfInvoicesFailed { get; set; }

		ZDateTime StartTime { get; set; }

		ZString ElapsedTimeFormatted
		{
			get
			{
				var timeSpan = ZDateTime.Now - StartTime;
				return string.Format("{0:00}:{1:00}:{2:00}", (int)timeSpan.TotalHours, timeSpan.Minutes, timeSpan.Seconds);
			}
		}

		ProgressWithDetailesForm ProgressForm
		{
			get
			{
				if (progressForm == null)
				{
					progressForm = new ProgressWithDetailesForm();
				}
				return progressForm;
			}
		}
		ProgressWithDetailesForm progressForm;

		void ResetProgressForm()
		{
			ProgressForm.ActivateCloseButton();
			progressForm = null;
		}

		void ProgressForm_CancellingQuery(object sender, CancelEventArgs e)
		{
			var questionResult = Globals.Message.Show(CancelQuestionText, MessageBoxCaption, MessageBoxButtons.YesNo, DialogResult.No);
			if (questionResult == DialogResult.Yes)
			{
				Poster.CancelPosting();
			}
			else
			{
				e.Cancel = true;
			}
		}

		string CancelQuestionText
		{
			get { return cancelQuestionText ?? Res.GetString("983948c6-3cac-44d5-b792-67c06911b5d9", "You are about to cancel Bulk Periodic Invoice posting. Are you sure you want to cancel posting?"); }
		}
		string cancelQuestionText;

		string MessageBoxCaption
		{
			get { return Res.GetString("e338338a-0393-4ca0-9f77-19889913d7c5", "Bulk Invoice Posting"); }
		}

		string DeliveryAllText
		{
			get { return Res.GetString("813f1a90-f5ba-40c1-8585-30ce6835479c", "Deliver All"); }
		}

		INotificationHandler NotificationHandler
		{
			get { return this; }
		}

		#region INotificationHandler Members

		void INotificationHandler.ReportError(string message, string caption, string errorContext, Exception exception)
		{
			ProgressForm.AddLog(message);
			ErrorsList.AppendLine(message);
		}

		void INotificationHandler.ReportInformation(string message, string caption)
		{
			ProgressForm.AddLog(message);
		}

		#endregion

#if DEBUG
		internal ProgressForm LastProgressFormForTestOnly;
		internal StackTrace LastStackTraceInStartPosting_ForTestOnly;
		internal PeriodicInvoiceBulkPoster GetPoster_ForTestOnly()
		{
			return Poster;
		}
#endif

		#region Test
#if DEBUG

		PrintTask enterpriseInvoicesPrintTaskForTesting;
		PrintTask governmentInvoicesPrintTaskForTesting;
#endif
		#endregion
	}
}
