#define CODE_ANALYSIS

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
#if NETFRAMEWORK
using System.Runtime.Serialization;
#endif
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Invoicing.Printing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.GUI.ARAP.AutoAllocationAndPrinting;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.GUI.JobInvoicing
{
	public class GLOWInvoicingPostManagerGUIWrapper : PostManagerGUIWrapper
	{
		public GLOWInvoicingPostManagerGUIWrapper(Guid jobPK, JobInvoicingPostingOption postingOption, bool shouldPerformBackDating, DateTime postDate, DateTime invoiceDate, BusinessObjectFactory plugInFactory)
			: base(null, postingOption, plugInFactory, null)
		{
			Jobs = LoadJobsInNewFactory(plugInFactory.Load<Job>(jobPK));
			this.shouldPerformBackDating = shouldPerformBackDating;
			this.postDate = postDate;
			this.invoiceDate = invoiceDate;
		}

		readonly bool shouldPerformBackDating;
		readonly DateTime postDate;
		readonly DateTime invoiceDate;

		bool getBackPostingDataCalled;
		BackPostingData backPostingData;

		public BackPostingData GetBackPostingData()
		{
			backPostingData = new BackPostingData();
			getBackPostingDataCalled = true;
			Post();
			return backPostingData;
		}

		bool getPreviewInvoicesDataCalled;
		PreviewInvoicesData previewInvoicesData;

		public PreviewInvoicesData GetPreviewInvoicesData()
		{
			previewInvoicesData = new PreviewInvoicesData();
			getPreviewInvoicesDataCalled = true;
			Preview();
			return previewInvoicesData;
		}

		protected override BasePostManager GetNewPostManager()
		{
			return new InvoicingPostManager(Jobs.Any() ? Jobs.First() : null, APInvoiceApprovalGUIProvider);
		}

		protected override bool PostTransactionsCore(TransactionCreatorHashtable transactions)
		{
#if DEBUG
			if (Globals.IsTest && DoTestPostTransactions)
			{
				Test_Allocator = new PaymentBatchChequeNumberAllocator.DummyPaymentBatchChequeNumberAllocator(transactions, TransactionFactory);
				Test_Allocator.SetChequeBookToInactiveOnSaving = Test_DeactivateChequeBookOnAllocation;
				Test_AllocatorsList.Add(Test_Allocator);
				SavePostingFactoriesForTest(transactions);
			}
			else
			{
#endif
				TransactionFactory.SetContext(BusinessContext.BulkTransactionSaving);
				try
				{
					ZExceptionReporting.ProcessWithSaveExceptionHandling(() => BusinessObjectFactory.SaveTogether(GetAllTransactionParticipants(transactions)), null, true);
				}
				finally
				{
					TransactionFactory.RemoveContext(BusinessContext.BulkTransactionSaving);
				}
#if DEBUG
			}
#endif
			if (!IsPostedForBatchInvoicing)
			{
				CheckCreditLimitExceeded(transactions);
			}

			return true;
		}

		protected override void PreviewTransactionsCore(System.Collections.ArrayList transactions)
		{
			if (getPreviewInvoicesDataCalled)
			{
				List<PreviewInvoiceData> previewInvoiceDataList = new List<PreviewInvoiceData>();
				foreach (InvoicingBase invoice in PostManager.Poster.PostedInvoices)
				{
					PreviewInvoiceData previewInvoiceData = new PreviewInvoiceData();
					previewInvoiceData.Account = invoice.Header.OH_Code;
					previewInvoiceData.Description = invoice.AH_Desc;
					previewInvoiceData.Category = invoice.AH_TransactionCategory;
					previewInvoiceData.InvoiceType = invoice.TransactionCategoryDescription;
					previewInvoiceData.Currency = invoice.AH_RX_NKTransactionCurrency;
					previewInvoiceData.Amount = invoice.AH_OSTotal;

					try
					{
						using (var task = new InvoicePrintTask(new InvoicePrintTask.Configuration(invoice) { JobParent = invoice.Job?.Parent, IsProFormaInvoice = true }))
						using (var stream = task.RunToStreamNotExcelOnly())
						{
							previewInvoiceData.PDFDocument = stream.ToArray();
						}
					}
					catch (FileNotFoundException ex)
					{
						Globals.Message.ShowError(ex.Message);
					}

					previewInvoiceDataList.Add(previewInvoiceData);
				}

				previewInvoicesData.PreviewInvoices = previewInvoiceDataList;
			}
		}

		protected override bool PerformTransactionBackDating(TransactionCreatorHashtable transactions)
		{
			bool result = base.PerformTransactionBackDating(transactions);

			if (getBackPostingDataCalled)
			{
				backPostingData.InvoiceDate = NotificationHelper.TransactionDate.ToDateTime();
				backPostingData.PostDate = NotificationHelper.PostDate.ToDateTime();
				backPostingData.RevenueRecognitionDates = NotificationHelper.RevenueRecognitionDates.ToString();
				result = false;
			}

			return result;
		}

		protected override QueryUserMsgBoxEventArgs QueryUserToPerformTransactionBackDating()
		{
			if (shouldPerformBackDating)
			{
				IJobInvoicingPlugIn jobPlugin = JobParent as IJobInvoicingPlugIn;
				ChangeTransactionDatesBusinessObject changeTransactionDateBusinessObject = GetNewChangeTransactionDatesBusinessObject(jobPlugin != null ? jobPlugin.InvoicingSupporter.JobInvoicingSecurity : null, TransactionFactory);

				changeTransactionDateBusinessObject.RevenueRecognitionDates = NotificationHelper.RevenueRecognitionDates;
				changeTransactionDateBusinessObject.PostDate = postDate;
				changeTransactionDateBusinessObject.InvoiceDate = invoiceDate;

				NotificationHelper.SetChangeTransactionDateBusinessObject(changeTransactionDateBusinessObject);
			}

			return new QueryUserYesNoCancelEventArgs("", shouldPerformBackDating);
		}

		protected override void JobsOnHoldCore(string message)
		{
			throw new GlowErrorReportException(message);
		}

		protected override void NothingPostedHandlerCore(string message)
		{
			throw new GlowErrorReportException(message);
		}

		protected override void NotifyPostingWarningCore(string message)
		{
			//don't show GUI pop-up as in GLOW is should be implemented in different way.
		}

		protected override void NotifyPostValidationError(string error)
		{
			throw new GlowErrorReportException(error);
		}

		protected override bool ConfirmContinueWithPostValidationWarning(PostManagerNotification notification)
		{
			return true;
		}

		protected override string GetJobOnHoldMessage(IEnumerable<Job> jobs)
		{
			return Res.GetString("70cf0c3c-4ee7-4211-bddc-e31b6154ef11", "You cannot post because this job is on hold.\r\nTo post, change the job status from '{0}'.", JobHeaderStatus.WorkOnHold.Code);
		}

		protected override void job_ShouldUseImmediateRevenueRecognisedDate(object sender, Business.UserQueryEventArgs e)
		{
		}

		protected override void PromptForRequisitionDateAndStatus()
		{
		}

		[Serializable]
		public class GlowErrorReportException : Exception
		{
			public GlowErrorReportException()
			{
			}

			public GlowErrorReportException(string message)
				: base(message)
			{
			}

			public GlowErrorReportException(string message, Exception inner)
				: base(message, inner)
			{
			}

#if NETFRAMEWORK
			protected GlowErrorReportException(SerializationInfo info, StreamingContext context)
				: base(info, context)
			{
			}
#endif
		}
	}
}
