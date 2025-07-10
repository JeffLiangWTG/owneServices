using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.GUI.ARAP.Payment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.ARAP.ReceiptPayment
{
	public partial class PaymentBatchPrintManager : PaymentPrintManagerBase
	{
		public PaymentBatchPrintManager(IEnumerable<TransactionHeader> paymentCollection, PaymentApprovalBase approval, ZString transactionType, BusinessObjectFactory factory)
		{
			this.Factory = factory;
			this.TransactionType = transactionType;
			this.PaymentCollection = paymentCollection;
			this.Approval = approval;
		}

		protected readonly IEnumerable<TransactionHeader> PaymentCollection;
		protected readonly PaymentApprovalBase Approval;
		protected readonly string TransactionType;
		protected readonly BusinessObjectFactory Factory;

		public bool IsCheque
		{
			get { return PaymentPrinter.PaymentType == ZArchitecture.Core.ReceiptTypes.Cheque; }
		}

		protected IPaymentBatchPrint PaymentPrinter
		{
			get
			{
				if (fPaymentPrinter == null)
				{
					fPaymentPrinter = GetIPaymentBatchPrint();
				}
				return fPaymentPrinter;
			}
		}

		protected virtual IPaymentBatchPrint GetIPaymentBatchPrint()
		{
			return new PaymentDocumentsPrinter(PaymentCollection, Approval, Factory);
		}

		protected override ZString PaymentChequeOrReference => PaymentPrinter.PaymentChequeOrReference;
		protected override ZString PaymentOrganisationCode => PaymentPrinter.PaymentOrganisationCode;
		protected override ZString PaymentTypeCode => PaymentPrinter.PaymentTypeCode;

		public void Print()
		{
			ChequeIsAutoPrinted = CheckIfAllChequesAreAutoPrintedAlready(PaymentCollection);

			var printOprions = base.GetPaymentPrintOptions(() => PaymentPrintForm);
			if (printOprions == default)
			{
				return;
			}

			var shouldPrintPaymentVoucher = printOprions.HasFlag(PaymentPrintOptions.PaymentVoucher);
			var shouldPrintRemittance = printOprions.HasFlag(PaymentPrintOptions.RemittanceAdvice);
			var shouldPrintCheque = printOprions.HasFlag(PaymentPrintOptions.Cheque);
			var shouldPrintPaymentBatchListing = printOprions.HasFlag(PaymentPrintOptions.PaymentBatchListing);

			PaymentPrinter.PrintDocumentsForPaymentBatch(shouldPrintPaymentVoucher, shouldPrintRemittance, shouldPrintCheque && IsCheque, shouldPrintPaymentBatchListing);
		}

		public void PrintChequesDirectly(ZGuid printQueuePK)
		{
			PaymentPrinter.AutoPrintCheques(printQueuePK);
			ChequeIsAutoPrinted = ZBool.True;
		}
		public ZBool ChequeIsAutoPrinted;

		#region Implementation

		bool CheckIfAllChequesAreAutoPrintedAlready(IEnumerable<TransactionHeader> paymentCollection)
		{
			foreach (Business.ARAP.ReceiptPayment.Payment payment in paymentCollection)
			{
				if (!((IChequeNumberAutoAllocation)payment).ChequeIsAutoPrinted)
				{
					return false;
				}
			}
			return paymentCollection.Any();
		}

		protected PaymentDocumentsPrintPopup PaymentPrintForm
		{
			get
			{
				var optionsAvailable = PaymentPrintOptions.PaymentVoucher;

				if (Approval != null)
				{
					optionsAvailable |= PaymentPrintOptions.PaymentBatchListing;
				}

				if (PaymentCollection.Any())
				{
					optionsAvailable |= PaymentPrintOptions.RemittanceAdvice;
					optionsAvailable |= PaymentPrintOptions.Cheque;
				}

				fPaymentPrinterForm = GetPaymentDocumentsPrintPopup(optionsAvailable);
				fPaymentPrinterForm.IsCheque = IsCheque;
				return fPaymentPrinterForm;
			}
		}

		protected virtual PaymentDocumentsPrintPopup GetPaymentDocumentsPrintPopup(PaymentPrintOptions optionsAvailable)
		{
			return new PaymentDocumentsPrintPopup(PaymentDescription, ChequeIsAutoPrinted, optionsAvailable);
		}

		protected IPaymentBatchPrint fPaymentPrinter;
		protected PaymentDocumentsPrintPopup fPaymentPrinterForm;
		protected Guid fPKToPrint;
		protected bool fDisableChequePrinting;

		protected bool IsTransactionCancelled()
		{
			AccTransactionHeader transaction = new BusinessObjectFactory().Load<AccTransactionHeader>(fPKToPrint);
			return transaction.AH_IsCancelled;
		}

		#endregion

#if DEBUG
		#region Test Classes

		internal class TestPaymentPrintParentForm : ZForm
		{
			public override string FormCaption
			{
				get { return "Test Payment Print Parent"; }
			}
		}

		public class TestPaymentPrintManager : PaymentBatchPrintManager
		{
			public TestPaymentPrintManager(IEnumerable<TransactionHeader> paymentCollection, PaymentApprovalBase approval, string transactionType, BusinessObjectFactory factory)
				: base(paymentCollection, approval, transactionType, factory)
			{
				Payments = paymentCollection;
			}

			readonly IEnumerable<TransactionHeader> Payments;

			public PaymentDocumentsPrintPopup RemittancePrintForm_Exposed
			{
				get { return fPaymentPrinterForm; }
			}

			public PaymentDocumentsPrintPopup PaymentPrintForm_Exposed
			{
				get { return base.PaymentPrintForm; }
			}

			protected override PaymentDocumentsPrintPopup GetPaymentDocumentsPrintPopup(PaymentPrintOptions optionsAvailable)
			{
				return new PaymentDocumentsPrintPopup.MockPaymentDocumentsPrintPopup(PaymentDescription, ChequeIsAutoPrinted, ~PaymentPrintOptions.None);
			}

			protected override IPaymentBatchPrint GetIPaymentBatchPrint()
			{
				return new MockPaymentPrint(Payments, Factory);
			}
		}

		public class MockPaymentPrint : IPaymentBatchPrint
		{
			public MockPaymentPrint()
			{
			}

			public MockPaymentPrint(IEnumerable<TransactionHeader> paymentCollection, BusinessObjectFactory factory)
			{
				PaymentCollectionPassedForAutoPrinting = paymentCollection;
			}

			#region IPaymentBatchPrint Members

			void IPaymentBatchPrint.PrintDocumentsForPaymentBatch(ZBool printPaymentVouchers, ZBool printRemittanceAdvices, ZBool printCheques, ZBool printPaymentBatchListing)
			{
				if (printPaymentVouchers)
				{
					IsPaymentVoucherPrinted = true;
				}
				if (printRemittanceAdvices)
				{
					IsRemittanceAdvicePrinted = true;
				}
				if (printCheques)
				{
					IsChequePrinted = true;
				}
				if (printPaymentBatchListing)
				{
					IsPaymentBatchListingPrinted = true;
				}
			}

			#endregion

			#region IPaymentPrint Members

			public void PrintPaymentVoucher()
			{
				IsPaymentVoucherPrinted = true;
			}

			public void PrintRemittanceAdvice()
			{
				IsRemittanceAdvicePrinted = true;
			}

			public void PrintCheque()
			{
			}

			public void PrintPaymentBatchListing()
			{
				IsPaymentBatchListingPrinted = true;
			}

			public void AutoPrintCheque(ZGuid printer)
			{
				IsAutoChequePrinted = true;
			}

			public void AutoPrintCheques(ZGuid printerPK)
			{
				foreach (TransactionHeader transaction in PaymentCollectionPassedForAutoPrinting)
				{
					if (transaction is IChequeNumberAutoAllocation)
					{
						PaymentDocumentsPrinter.CheckAndThrowTestAutoAllocationAndPrintChequesFailure(((IChequeNumberAutoAllocation)transaction).ChequeBook);
						((IChequeNumberAutoAllocation)transaction).ChequeIsAutoPrinted = ZBool.True;
					}
				}
				IsAutoChequePrinted = true;
				PrinterPKPassedForAutoPrinting = printerPK;
			}

			public string PaymentType
			{
				get { return "CHQ"; }
			}

			public ZString PaymentTypeCode => "CHQ";

			public ZString PaymentChequeOrReference => "123456";

			public ZString PaymentOrganisationCode => "AALSHI";

			public void Reset()
			{
				IsPaymentVoucherPrinted = false;
				IsRemittanceAdvicePrinted = false;
				IsChequePrinted = false;
				IsPaymentBatchListingPrinted = false;
			}

			public bool IsPaymentBatchListingPrinted;
			public bool IsPaymentVoucherPrinted;
			public bool IsRemittanceAdvicePrinted;
			public bool IsChequePrinted;
			public bool IsAutoChequePrinted;
			public IEnumerable<TransactionHeader> PaymentCollectionPassedForAutoPrinting;
			public ZGuid PrinterPKPassedForAutoPrinting;

			#endregion
		}

		#endregion
#endif
	}
}
