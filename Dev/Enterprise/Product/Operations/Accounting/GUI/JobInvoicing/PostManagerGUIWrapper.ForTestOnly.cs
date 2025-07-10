#if DEBUG

using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.GUI.JobInvoicing
{
	public partial class PostManagerGUIWrapper
	{
		public JobInvoicingPostingOption PostingOption_ForTestOnly => PostingOption;

		public BusinessObjectFactory PlugInFactory_ForTestOnly => PlugInFactory;

		public void JobsOnHold_ForTestOnly(object sender, BasePostManager.OnJobOnHoldEventArgs e)
		{
			JobsOnHold(sender, e);
		}

		public IEnumerable<Job> Jobs_ForTestOnly
		{
			get { return Jobs; }
			set { Jobs = value; }
		}

		public IEnumerable<Job> OriginalJobs_ForTestOnly => OriginalJobs;

		internal BulkPostingDataCollector BulkPostingDataCollector_ForTestOnly
		{
			get { return bulkPostingDataCollector; }
			set { bulkPostingDataCollector = value; }
		}

		public List<ARAP.AutoAllocationAndPrinting.PaymentBatchChequeNumberAllocator.DummyPaymentBatchChequeNumberAllocator> Test_AllocatorsList_ForTestOnly => Test_AllocatorsList;

		public PostManagerValidation GetPostManagerValidation_ForTestOnly(IEnumerable<Job> jobs, JobInvoicingPostingOption postingOption, IEnumerable<Job> originalJobs)
		{
			return GetPostManagerValidation(jobs, postingOption, originalJobs);
		}

		public ChangeTransactionDatesNotificationSubscriberGuiHelper NotificationHelper_ForTestOnly
		{
			get { return notificationHelper; }
			set { notificationHelper = value; }
		}

		internal ParentInfo GetParentInfoForPostingAction_ForTestOnly() => GetParentInfoForPostingAction();

		public BasePostManager PostManager_ForTestOnly => PostManager;

		public bool AllowDeliveryDuringPreviewInvoice_ForTestOnly => AllowDeliveryDuringPreviewInvoice;

		public BusinessObjectFactory TransactionFactory_ForTestOnly
		{
			get { return TransactionFactory; }
			set { TransactionFactory = value; }
		}

		public ARAP.AutoAllocationAndPrinting.PaymentBatchChequeNumberAllocator.DummyPaymentBatchChequeNumberAllocator Test_Allocator_ForTestOnly
		{
			get { return Test_Allocator; }
			set { Test_Allocator = value; }
		}

		public BasePostManager GetNewPostManager_ForTestOnly()
		{
			return GetNewPostManager();
		}

		public IJobInvoicingPlugIn JobInvoicingPlugIn_ForTestOnly => JobInvoicingPlugIn;

		public TransactionHeader[] GetTransactionsForPrinting_ForTestOnly(TransactionCreatorHashtable transactions)
			=> GetTransactionsForPrinting(transactions);

		protected virtual (IEnumerable<InvoicingBase> arInvoices, InvoicingBase[] apInvoicesAndCreditNotes, APPayment[] apPayments, APInvoice[] apApprovalRequests)
			AlterTransactionsForPrinting_ForTestOnly(IEnumerable<InvoicingBase> arInvoices, InvoicingBase[] apInvoicesAndCreditNotes, APPayment[] apPayments, APInvoice[] apApprovalRequests)
		{
			return (arInvoices, apInvoicesAndCreditNotes, apPayments, apApprovalRequests);
		}
	}
}

#endif
