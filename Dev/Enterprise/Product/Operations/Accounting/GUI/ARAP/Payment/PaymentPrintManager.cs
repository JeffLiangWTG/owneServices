using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.GUI.ARAP.Payment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.ARAP.ReceiptPayment
{
	public partial class PaymentPrintManager : PaymentPrintManagerBase
	{
		public PaymentPrintManager(PaymentApprovalBase approval, ZString transactionType, BusinessObjectFactory factory)
			: this(approval.TransactionHeader?.PK.ToGuid() ?? Guid.Empty, transactionType, factory)
		{
			this.approval = approval;
			var payment = approval.TransactionHeader as IChequeNumberAutoAllocation;
			if (approval.IsPosted && payment != null)
			{
				if (payment.ChequeIsAutoPrinted)
				{
					SetChequeIsAutoPrinted();
				}

				optionsAvailable = ~PaymentPrintOptions.None;
			}
			else
			{
				optionsAvailable = PaymentPrintOptions.PaymentBatchListing | PaymentPrintOptions.PaymentVoucher;
			}
		}

		public PaymentPrintManager(Guid paymentPK, ZString transactionType, BusinessObjectFactory factory)
		{
			this.Factory = factory;
			this.TransactionType = transactionType;
			this.PaymentPK = paymentPK;
			this.optionsAvailable = ~PaymentPrintOptions.None;
		}

		protected readonly PaymentPrintOptions optionsAvailable;
		protected readonly PaymentApprovalBase approval;
		protected readonly Guid PaymentPK;
		protected readonly string TransactionType;
		protected readonly BusinessObjectFactory Factory;

		public virtual bool IsCheque
		{
			get { return PaymentPrinter.PaymentType == ZArchitecture.Core.ReceiptTypes.Cheque; }
		}

		protected IPaymentPrint PaymentPrinter
		{
			get
			{
				if (fPaymentPrinter == null)
				{
					fPaymentPrinter = GetIPaymentPrint();
				}
				return fPaymentPrinter;
			}
		}

		protected override ZString PaymentChequeOrReference => PaymentPrinter.PaymentChequeOrReference;
		protected override ZString PaymentOrganisationCode => PaymentPrinter.PaymentOrganisationCode;
		protected override ZString PaymentTypeCode => PaymentPrinter.PaymentTypeCode;

		protected virtual IPaymentPrint GetIPaymentPrint()
		{
			return approval != null
				? new PaymentDocumentsPrinter(approval, Factory)
				: new PaymentDocumentsPrinter(PaymentPK, Factory);
		}

		public virtual void Print()
		{
			var printOprions = GetPaymentPrintOptions(GetPaymentDocumentsPrintPopup);
			if (printOprions == default)
			{
				return;
			}

			if (printOprions.HasFlag(PaymentPrintOptions.PaymentVoucher))
			{
				PaymentPrinter.PrintPaymentVoucher();
			}

			if (printOprions.HasFlag(PaymentPrintOptions.RemittanceAdvice))
			{
				PaymentPrinter.PrintRemittanceAdvice();
			}

			if (printOprions.HasFlag(PaymentPrintOptions.Cheque) && IsCheque)
			{
				PaymentPrinter.PrintCheque();
			}

			if (printOprions.HasFlag(PaymentPrintOptions.PaymentBatchListing))
			{
				PaymentPrinter.PrintPaymentBatchListing();
			}
		}

		public void AutoPrintCheque(ZGuid printQueuePK)
		{
			PaymentPrinter.AutoPrintCheque(printQueuePK);
			ChequeIsAutoPrinted = ZBool.True;
		}

		public ZBool ChequeIsAutoPrinted;

		//ToDo: delete this method
		public void SetChequeIsAutoPrinted() => ChequeIsAutoPrinted = ZBool.True;

		#region Implementation

		PaymentDocumentsPrintPopup GetPaymentDocumentsPrintPopup()
		{
			PaymentDocumentsPrintPopup result = GetPaymentDocumentsPrintPopupCore();
			result.IsCheque = IsCheque;
			return result;
		}

		protected virtual PaymentDocumentsPrintPopup GetPaymentDocumentsPrintPopupCore()
		{
			var options = optionsAvailable;
			if (approval?.PaymentBatch == null)
			{
				options &= ~PaymentPrintOptions.PaymentBatchListing;
			}

			return new PaymentDocumentsPrintPopup(PaymentDescription, ChequeIsAutoPrinted, options);
		}

		protected IPaymentPrint fPaymentPrinter;
		protected PaymentPrintOptions PaymentPrintOption;
		protected bool fDisableChequePrinting;

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

		public class TestPaymentPrintManager : PaymentPrintManager
		{
			PaymentDocumentsPrintPopup lastPrintForm;

			public TestPaymentPrintManager(Guid pk, string transactionType, BusinessObjectFactory factory)
				: base(pk, transactionType, factory)
			{
				PaymentPk = pk;
			}

			readonly Guid PaymentPk;

			public PaymentDocumentsPrintPopup LastPrintForm
			{
				get { return lastPrintForm; }
			}

			protected override IPaymentPrint GetIPaymentPrint()
			{
				return new MockPaymentPrint(PaymentPk, Factory);
			}

			protected override PaymentDocumentsPrintPopup GetPaymentDocumentsPrintPopupCore()
			{
				lastPrintForm = new PaymentDocumentsPrintPopup.MockPaymentDocumentsPrintPopup();
				return lastPrintForm;
			}

			public IPaymentPrint PaymentPrinter_Exposed
			{
				get
				{
					return base.PaymentPrinter_ForTestOnly;
				}
			}
		}

		public class MockPaymentPrint : IPaymentPrint
		{
			public MockPaymentPrint()
			{
			}

			public MockPaymentPrint(Guid pK, BusinessObjectFactory factory)
			{
				PaymentPKPassedForAutoPrinting = pK;
			}

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
				IsChequePrinted = true;
			}

			public void PrintPaymentBatchListing()
			{
				IsPaymentBatchListingPrinted = true;
			}

			public void AutoPrintCheque(ZGuid printerPK)
			{
				IsChequeAutoPrinted = true;
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
			public bool IsChequeAutoPrinted;
			public Guid PaymentPKPassedForAutoPrinting;
			public ZGuid PrinterPKPassedForAutoPrinting;

			#endregion
		}

		#endregion
#endif
	}
}
