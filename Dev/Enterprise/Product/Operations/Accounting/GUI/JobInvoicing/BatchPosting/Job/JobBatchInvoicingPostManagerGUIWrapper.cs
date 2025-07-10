using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Invoicing.Printing;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.GUI.ARAP.ReceiptPayment;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.GUI.JobInvoicing.BatchPosting
{
	public partial class JobBatchInvoicingPostManagerGUIWrapper : BaseBatchInvoicingPostManagerGUIWrapper
	{
		public JobBatchInvoicingPostManagerGUIWrapper(JobInvoicingPostingOption postingOption,
				IJobInvoicingPlugIn[] jobCollection,
				ZString selectedPostingOptionName,
				ZString postedObjectName)
				: base(postingOption, selectedPostingOptionName, postedObjectName)
		{
			List<Job> jobs = new List<Job>();

			foreach (IJobInvoicingPlugIn plugIn in jobCollection)
			{
				jobs.Add(new Job.Loader(PlugInFactory, plugIn).Load(true, setJobDefaults: false));
			}

			this.JobCollection = jobs.ToArray();
		}

		public JobBatchInvoicingPostManagerGUIWrapper(JobInvoicingPostingOption postingOption,
			Job[] jobCollection,
			ZString selectedPostingOptionName,
			ZString postedObjectName)
			: base(postingOption, selectedPostingOptionName, postedObjectName)
		{
			this.JobCollection = jobCollection;
		}

		readonly Job[] JobCollection;

		#region Post

		protected override BasePostManager GetNewPostManager()
		{
			return new InvoicingPostManager(Jobs.Any() ? Jobs.First() : null, APInvoiceApprovalGUIProvider);
		}

		public override void Post()
		{
			foreach (Job job in JobCollection)
			{
				CurrentJob = job;
				JobParent = CurrentJob != null ? CurrentJob.Parent : null;
				RenewTransactionFactory();
				Jobs = LoadJobsInNewFactory(CurrentJob);
				IsPostedForBatchInvoicing = true;
				PostSingleObject(Jobs);
				if (ShouldStopPostingOnNextObject)
				{
					break;
				}
			}

			CheckCreditLimitExceeded(bulkPostingDataCollector);
			RaiseOnPostingFinished();
			OverrideTransactionDescription(bulkPostingDataCollector.AllPostedInvoicePKs.ToArray());
			PrintInvoices(null);
			PromptForRequisitionDateAndStatus(bulkPostingDataCollector);
		}

		internal static void CheckCreditLimitExceeded(BulkPostingDataCollector bulkPostingDataCollector)
		{
			var invoiceFactory = new BusinessObjectFactory();
			var aRPostedInvoices = invoiceFactory.Load<InvoicingBase>(new ZQuery(AccTransactionHeaderSchema.PK, bulkPostingDataCollector.AllPostedInvoicePKs));
			var aPPostedInvoices = invoiceFactory.Load<InvoicingBase>(new ZQuery(AccTransactionHeaderSchema.PK, bulkPostingDataCollector.AllAPInvoicesAndCreditNotesExcludingUAInvoicesAndUACreditNotePKs));
			IEnumerable<InvoicingBase> invoices = aRPostedInvoices.Union(aPPostedInvoices);
			InvoicingBase.CheckCreditLimitExceeded(invoices);
		}

		protected override void PrintInvoices(TransactionCreatorHashtable transactions)
		{
			BatchPrintJobInvoicing(bulkPostingDataCollector);
		}

		internal static void BatchPrintJobInvoicing(BulkPostingDataCollector bulkPostingDataCollector)
		{
			var classAInvoices = new List<InvoicingBase>();
			var normalInvoices = new List<InvoicingBase>();
			var selfBillingInvoices = new List<InvoicingBase>();
			var costConfirmationDocuments = new List<InvoicingBase>();
			var remittanceAdvices = new List<APPayment>();

			var printingFactory = new BusinessObjectFactory();
			var allPostedInvoices = printingFactory.Load<InvoicingBase>(new ZQuery(AccTransactionHeaderSchema.PK, bulkPostingDataCollector.AllPostedInvoicePKs));
			foreach (ZGuid pk in bulkPostingDataCollector.AllPostedInvoicePKs)
			{
				var aRTransaction = allPostedInvoices.FirstOrDefault(x => x.PK == pk);
				if (aRTransaction != null)
				{
					if (aRTransaction.IsGovtTaxInvoice)
					{
						if (AccountingConfigurationRegistry.Instance.InvoicePrintingOption.Value != GovtTaxInvoicePrintTask.EnterpriseInvoice)
						{
							classAInvoices.Add(aRTransaction);
						}
						else
						{
							normalInvoices.Add(aRTransaction);  // this uses the classAInvoice print task but the actual template is not class A
						}
					}
					else
					{
						normalInvoices.Add(aRTransaction);
					}
				}
			}

			var allAPInvoicesAndCreditNotesExcludingUAInvoicesAndUACreditNotes = printingFactory.Load<InvoicingBase>(new ZQuery(AccTransactionHeaderSchema.PK, bulkPostingDataCollector.AllAPInvoicesAndCreditNotesExcludingUAInvoicesAndUACreditNotePKs));
			foreach (ZGuid pk in bulkPostingDataCollector.AllAPInvoicesAndCreditNotesExcludingUAInvoicesAndUACreditNotePKs)
			{
				var aPTransaction = allAPInvoicesAndCreditNotesExcludingUAInvoicesAndUACreditNotes.FirstOrDefault(x => x.PK == pk);
				if (aPTransaction != null)
				{
					if (aPTransaction.IsSelfBillingInvoice)
					{
						selfBillingInvoices.Add(aPTransaction);
					}
					if (AccountingConfigurationRegistry.Instance.PrintOptionWhenAPInvoicePosted.Value)
					{
						costConfirmationDocuments.Add(aPTransaction);
					}
				}
			}

			var allAPPayments = printingFactory.Load<APPayment>(new ZQuery(AccTransactionHeaderSchema.PK, bulkPostingDataCollector.AllAPPaymentPKs));
			foreach (ZGuid pk in bulkPostingDataCollector.AllAPPaymentPKs)
			{
				var aPPayment = allAPPayments.FirstOrDefault(x => x.PK == pk);
				if (aPPayment != null)
				{
					remittanceAdvices.Add(aPPayment);
				}
			}

			PrintClassAInvoices(classAInvoices);
			PrintNormalInvoices(normalInvoices);
			PrintSelfBillingInvoices(selfBillingInvoices);
			PrintCostConfirmationDocuments(costConfirmationDocuments);
			PrintRemittanceAdvices(remittanceAdvices);
		}

		static void PrintClassAInvoices(IEnumerable<InvoicingBase> classAInvoices)
		{
			if (classAInvoices != null && classAInvoices.Any())
			{
				(new GovtTaxInvoicePrinter()).PrintGovtTaxInvoices(classAInvoices.ToArray());
			}
		}

		static void PrintRemittanceAdvices(IEnumerable<APPayment> remittanceAdvices)
		{
			if (remittanceAdvices != null && remittanceAdvices.Any())
			{
				PaymentBatchPrintManager batchPrintManager = new PaymentBatchPrintManager(remittanceAdvices, null, TransactionTypes.Payment, new BusinessObjectFactory());
				batchPrintManager.Print();
			}
		}

		static void PrintCostConfirmationDocuments(IEnumerable<InvoicingBase> costConfirmationDocuments)
		{
			if (costConfirmationDocuments != null && costConfirmationDocuments.Any())
			{
				if (Globals.Message.Show(Res.GetString("8ff0418e-93b7-4b45-a832-5992c841cda6", "Do you want to print Cost Confirmation Documents?"),
								Res.GetString("be0a1743-bac9-42a4-b172-00fd16a5681e", "Print Cost Confirmation Document"),
								MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
				{
					InvoicePrintHelper.PrintCostConfirmationDocument(costConfirmationDocuments.ToArray());
				}
			}
		}

		static void PrintSelfBillingInvoices(IEnumerable<InvoicingBase> selfBillingInvoices)
		{
			if (selfBillingInvoices != null && selfBillingInvoices.Any())
			{
				ZStringBuilder stringBuilder = new ZStringBuilder();
				foreach (InvoicingBase selfBillingInvoice in selfBillingInvoices)
				{
					stringBuilder.Append((selfBillingInvoice is APCreditNote ? Res.GetString("d4e58a9f-206b-45d7-a923-d008faa70ae0", "Credit Note") + " " : Res.GetString("d07f2f24-a100-4dcb-837b-e1811c331cc8", "Invoice") + " ") + selfBillingInvoice.InvoiceNumber);
				}
				ZString question = Res.GetString("833fd2a0-0242-4ef6-9683-42e1b25f38a2", "Do you want to print the following Self Billing {0}?\r\n{1}",
														(selfBillingInvoices.Count() == 1) ? Res.GetString("d07f2f24-a100-4dcb-837b-e1811c331cc8", "Invoice") : Res.GetString("5b8000a4-f824-43f6-8f25-17cefba4413d", "Invoices"), stringBuilder.ToStringWithNewLineBetweenAppends());
				if (Globals.Message.Show(question, Res.GetString("11185f71-2b2f-49b8-8e94-077769f2e9b8", "Print Self Billing Invoice"),
						MessageBoxButtons.YesNo, MessageBoxIcon.Question, DialogResult.Yes) == DialogResult.Yes)
				{
					InvoicePrintHelper.PrintSelfBillingInvoice(selfBillingInvoices.ToArray());
				}
			}
		}

		static void PrintNormalInvoices(IEnumerable<InvoicingBase> normalInvoices)
		{
			if (normalInvoices != null && normalInvoices.Any())
			{
				ZStringBuilder stringBuilder = new ZStringBuilder();
				foreach (InvoicingBase normalInvoice in normalInvoices)
				{
					stringBuilder.Append((normalInvoice.AH_TransactionType == TransactionTypes.CreditNote ? Res.GetString("8675631f-3317-4710-bff6-065885705a64", "credit note") + " " : Res.GetString("0108e772-d68d-4536-bab9-32c2f2e0a2f6", "invoice") + " ") + normalInvoice.InvoiceNumber);
				}
				ZString question = Res.GetString("08bd91b5-3081-44d4-8b2b-212465b786af", "Do you want to print the following {0}?\r\n{1}",
														(normalInvoices.Count() == 1) ? Res.GetString("0108e772-d68d-4536-bab9-32c2f2e0a2f6", "invoice") : Res.GetString("345a4ad1-20fb-42f9-b67b-90b02be306bf", "invoices"), stringBuilder.ToStringWithNewLineBetweenAppends());
				if (Globals.Message.Show(question, Res.GetString("3fc0fa9e-bc4f-4ac8-bae2-0dd2e09d1024", "Print AR Invoice"),
						MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
				{
					try
					{
						new InvoicePrintTask(new InvoicePrintTask.Configuration(normalInvoices.ToArray())).Run();
					}
					catch (Exception ex) when (ex is ReprintingInvoiceException)
					{
						Globals.Message.ShowError(AccountingConstants.ReprintingInvoiceMessage);
					}
				}
			}
		}

		protected override PostManagerValidation GetPostManagerValidation(IEnumerable<Job> jobs, JobInvoicingPostingOption postingOption, IEnumerable<Job> originalJobs)
		{
			return new PostManagerValidation(jobs, postingOption, originalJobs, isBulkPosting: true);
		}

		Job CurrentJob;

		#endregion

		#region Implementation

		protected override ZString CurrentObjectCode
		{
			get { return CurrentJob != null ? CurrentJob.JH_JobNum : ZString.Empty; }
		}

		public override ZInt NumberOfObjectsToPost
		{
			get { return JobCollection.Length; }
		}

		protected override void NothingPostedHandlerCore(string message)
		{
			if (Jobs.Any() && !Jobs.First().IsWorkOnHold)
			{
				base.NothingPostedHandlerCore(message);
			}
		}

		protected override string GetJobOnHoldMessage(IEnumerable<Job> jobs)
		{
			return Res.GetString("4a63fb72-8617-4173-afe4-b3c26cdc7d0a", "You cannot post because this job is on hold.\r\nTo post, change the job status from '{0}'.", JobHeaderStatus.WorkOnHold.Code);
		}

		protected override ZString DefaultPostedObjectName
		{
			get { return Res.GetString("Accounting|JobBatchInvoicingPostManager|DefaultPostedObjectName", "Job"); }
		}

		#endregion
	}
}
