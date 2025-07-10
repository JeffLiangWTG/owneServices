using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.GUI.ARAP.Payment;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using BasePayment = Enterprise.Accounting.Business.ARAP.ReceiptPayment.Payment;

namespace Enterprise.Accounting.GUI.ARAP.ReceiptPayment
{
	/// <summary>
	/// Wrapper around Remittance Advice Printing
	/// </summary>
	public partial class PaymentDocumentsPrinter : IPaymentBatchPrint, IDocumentEvents
	{
		public PaymentDocumentsPrinter(PaymentApprovalBase selectedApproval, BusinessObjectFactory factory)
		{
			Factory = factory;
			Approval = ReloadApproval(selectedApproval);
			Payment = selectedApproval.TransactionHeader as TransactionHeader;
		}

		public PaymentDocumentsPrinter(Guid pK, BusinessObjectFactory factory)
		{
			Factory = factory;
			Payment = factory.Load<TransactionHeader>(pK);
		}

		public PaymentDocumentsPrinter(IEnumerable<TransactionHeader> paymentCollection, PaymentApprovalBase selectedApproval, BusinessObjectFactory factory)
		{
			this.Factory = factory;
			this.Approval = ReloadApproval(selectedApproval);
			this.PaymentCollection = paymentCollection;
		}

		PaymentApprovalWithAuthorisation ReloadApproval(PaymentApprovalBase selectedApproval)
		{
			if (selectedApproval != null)
			{
				var result = Factory.Load<PaymentApprovalBase>(selectedApproval.PK) as PaymentApprovalWithAuthorisation;
				if (result == null)
				{
					ErrorReporter.ReportOnce("Payment approval type not applicable for printing is used");
				}

				return result;
			}

			return null;
		}

		protected virtual string ChequeTemplate
		{
			get { return fChequeTemplate; }
			set { fChequeTemplate = value; }
		}

		protected void AutoPrint(ZGuid printerPK, string templateName, Core.Constants.DataContext dataContext)
		{
			if (Payment != null &&
				(Payment.AH_TransactionType == ZArchitecture.Core.TransactionTypes.Payment ||
				Payment.AH_TransactionType == ZArchitecture.Core.TransactionTypes.DirectPayment ||
				Payment.AH_TransactionType == ZArchitecture.Core.TransactionTypes.Transfer))
			{
				using (var printingUtil = new AccPrintingUtility(Factory, dataContext))
				{
					printingUtil.PrintDocument(Payment, templateName, AllowedDeliveryOptions.HardCopyOnly, printerPK, false);
				}
			}
		}

		protected void Print(string templateName, Core.Constants.DataContext dataContext)
		{
			if (Payment != null &&
				(Payment.AH_TransactionType == ZArchitecture.Core.TransactionTypes.Payment ||
				Payment.AH_TransactionType == ZArchitecture.Core.TransactionTypes.DirectPayment ||
				Payment.AH_TransactionType == ZArchitecture.Core.TransactionTypes.Transfer))
			{
				using (var printingUtil = new AccPrintingUtility(Factory, dataContext))
				{
					var printResult = printingUtil.PrintDocument(Payment, templateName, AllowedDeliveryOptions.All, ZGuid.Empty, true);
					RaiseDocumentPrinted(printResult);
				}
			}
		}

		protected void Print(AllowedDeliveryOptions options, string menuName)
		{
			if (Payment != null &&
				(Payment.AH_TransactionType == ZArchitecture.Core.TransactionTypes.Payment ||
				Payment.AH_TransactionType == ZArchitecture.Core.TransactionTypes.DirectPayment ||
				Payment.AH_TransactionType == ZArchitecture.Core.TransactionTypes.Transfer))
			{
				using (var printingUtil = new AccPrintingUtility(Factory))
				{
					var printResult = printingUtil.PrintDocument(Payment, menuName, options, ZGuid.Empty, true);
					RaiseDocumentPrinted(printResult);
				}
			}
		}

		protected void PrintVoucherFromApproval(AllowedDeliveryOptions options, string menuName)
		{
			if (Approval != null)
			{
				using (var printingUtil = new AccPrintingUtility(Factory))
				{
					var printResult = printingUtil.PrintDocument(Approval, menuName, options, ZGuid.Empty, true);
					RaiseDocumentPrinted(printResult);
				}
			}
		}

		protected void AddDocument(AccPrintingUtility printingUtil, string menuName, IDocumentSupportable payment, bool useLegacyDocument, bool findInSystemDefinedDocumentOnly)
		{
			if (payment is TransactionHeader th && th.AH_TransactionType == ZArchitecture.Core.TransactionTypes.Payment)
			{
				printingUtil.AddPaymentDocumentToPack(menuName, payment, useLegacyDocument, findInSystemDefinedDocumentOnly);
			}
		}

		#region Payment Properties

		ZString IPaymentPrint.PaymentOrganisationCode
		{
			get
			{
				return (Payment != null && Payment.Header != null) ? Payment.Header.OH_Code : (Approval?.Header?.OH_Code ?? ZString.Empty);
			}
		}

		ZString IPaymentPrint.PaymentTypeCode
		{
			get
			{
				return Payment != null ? Payment.AH_ReceiptType : (Approval?.ChequeOrReference ?? ZString.Empty);
			}
		}

		ZString IPaymentPrint.PaymentChequeOrReference
		{
			get
			{
				return Payment != null ? Payment.AH_ChequeOrReference : (Approval?.AV_PaymentType ?? ZString.Empty);
			}
		}

		#endregion

		#region Implementation

		protected readonly BusinessObjectFactory Factory;
		protected readonly PaymentApprovalWithAuthorisation Approval;
		protected readonly TransactionHeader Payment;
		protected readonly IEnumerable<TransactionHeader> PaymentCollection;
		string fChequeTemplate;

		protected readonly static string RemittanceAdviceMenuName = (NoResString)"Remittance Advice"; // Hard Coded Document Menu Names
		protected readonly static string PaymentVoucherMenuName = (NoResString)"Payment Voucher"; // Hard Coded Document Menu Names
		protected readonly static string PaymentBatchListingMenuName = (NoResString)"Payment Batch Listing"; // Hard Coded Document Menu Names

		protected bool ShouldReprintCheque()
		{
			if (ReprintChequeAllowed)
			{
				return Globals.Message.Show(Res.GetString("ea609dc6-e7e9-49b4-8bf3-23440aeb0162", "This Check is already printed. Do you want to re-print it?"), Res.GetString("bba43bca-52b1-4650-ae4a-f86b4e1ce816", "Payment Print"),
					MessageBoxButtons.YesNo, System.Windows.Forms.MessageBoxIcon.Question) == DialogResult.Yes;
			}
			else
			{
				Globals.Message.ShowError(Res.GetString("21cbb972-cd25-43cd-be65-206043493e10", "This check is already printed."), Res.GetString("bba43bca-52b1-4650-ae4a-f86b4e1ce816", "Payment Print"));
				return false;
			}
		}

		protected virtual bool ReprintChequeAllowed
		{
			get { return Env.Security.ReprintReallocateCheque.IsAllowed; }
		}

		protected virtual bool HasPermissionToPrintCheque()
		{
			return Env.Security.PrintCheque.IsAllowed;
		}

		bool? fFormsAreNotAllowed;
		bool FormsAreNotAllowed
		{
			get
			{
				if (!fFormsAreNotAllowed.HasValue)
				{
					fFormsAreNotAllowed = ((IDbConnected)Factory).Connection.IsInTransactionOtherThanTransactionedTestCase;
				}
				return fFormsAreNotAllowed.Value;
			}
		}

		protected bool ValidateChequePrinting(TransactionHeader payment)
		{
			bool canPrint = false;

			if (payment != null)
			{
				string message = null;
				fFormsAreNotAllowed = null;
				if (payment.AH_IsCancelled)
				{
					message = Res.GetString("fc4ea6ea-9559-4ad2-9c1e-4d40554bbcf7", "This cheque is canceled.");
				}
				else if (!HasPermissionToPrintCheque())
				{
					message = Res.GetString("f6d51b34-c9e7-404d-a499-5846d3a95f14", "You do not have the permission to print Check. Please Contact System Administrator.");
				}
				else if (string.IsNullOrWhiteSpace(ChequeTemplate))
				{
					message = Res.GetString("39a8754a-6ff2-438a-ab7b-ea263407da1b", "Auto printing of check is not properly configured.");
				}
				else if (!AccPrintingUtility.CheckMenuItemExistsForChequeTemplate(ChequeTemplate, payment))
				{
					message = AccPrintingUtility.GetMissingChequeTemplateMenuErrorMessage(ChequeTemplate);
				}
				else if (payment.AH_InvoicePrinted)
				{
					if (FormsAreNotAllowed)
					{
						message = Res.GetString("21cbb972-cd25-43cd-be65-206043493e10", "This check is already printed.");
					}
					else
					{
						canPrint = ShouldReprintCheque();
					}
				}
#if DEBUG
				else if (Globals.IsTest && TestAutoAllocationAndPrintCheques(payment))
				{
					message = "Testing Auto Print Cheques failure.";
				}
#endif
				else
				{
					canPrint = true;
				}

				if (message != null)
				{
					var heading = Res.GetString("bba43bca-52b1-4650-ae4a-f86b4e1ce816", "Payment Print");
					if (FormsAreNotAllowed)
					{
						message = string.Join(" ", Res.GetString("64ef3a67-751d-4272-ab86-9a55c1fb91c8", "Auto cheque printing failed. Please check Auto cheque printing settings."), message);
						throw new ZCannotSaveException(message, heading);
					}

					Globals.Message.ShowError(message, heading);
				}
			}

			return canPrint;
		}

		protected AccChequeBook GetChequeBook(string chequeNo, ZGuid bankPK)
		{
			AccChequeBook result = null;

			ZDecimal chequeNoAsDecimal = ZDecimal.Zero;
			if (ZDecimal.TryParse(chequeNo, out chequeNoAsDecimal) && chequeNoAsDecimal.IsInteger)
			{
				ZQuery filter = new ZQuery(AccChequeBookSchema.AK_AB, bankPK);
				filter.AddToFilter(AccChequeBookSchema.AK_StartNo, SQLComparisonOperator.LessThanOrEqualTo, chequeNoAsDecimal);
				filter.AddToFilter(AccChequeBookSchema.AK_LastNo, SQLComparisonOperator.GreaterThanOrEqualTo, chequeNoAsDecimal);
				result = Factory.LoadTop1(typeof(AccChequeBook), filter) as AccChequeBook;
			}

			return result;
		}

		protected string GetTemplateNameForRemittanceAdvice(TransactionHeader pay)
		{
			string templateName = "";

			if (pay != null)
			{
				AccChequeBook chequeBook = GetChequeBook(pay.AH_ChequeOrReference, pay.AH_AB);

				if (!pay.AH_IsCancelled && chequeBook != null && chequeBook.AK_AutoPrintCheque && pay.BankAccount.AB_SO_ChequeTemplate.IsValid)
				{
					StmTemplate template = Factory.Load(typeof(StmTemplate), pay.BankAccount.AB_SO_ChequeTemplate) as StmTemplate;
					templateName = template.SO_Name;
				}
			}

			return templateName;
		}

		[SuppressMessage("Microsoft.Design", "CA1030", Justification = "This is a false positive - we do follow event model")]
		protected void RaiseDocumentPrintRequested()
		{
			if (DocumentPrintRequested != null)
			{
				DocumentPrintRequested(this, new DocumentCancelEventArgs(null));
			}
		}

		[SuppressMessage("Microsoft.Design", "CA1030", Justification = "This is a false positive - we do follow event model")]
		protected void RaiseDocumentPrePreviewed(DeliveryInstructionDestination instructionsDestination)
		{
			if (instructionsDestination != DeliveryInstructionDestination.UserCancelled)
			{
				DocumentPrePreviewed?.Invoke(this, new DocumentPrintedEventArgs(instructionsDestination, null));
			}
		}

		[SuppressMessage("Microsoft.Design", "CA1030", Justification = "This is a false positive - we do follow event model")]
		protected void RaiseDocumentPrePrinted(DeliveryInstructionDestination instructionsDestination)
		{
			if (instructionsDestination != DeliveryInstructionDestination.UserCancelled)
			{
				if (DocumentPrePrinted != null)
				{
					DocumentPrePrinted(this, new DocumentPrintedEventArgs(instructionsDestination, null));
				}
			}
		}

		[SuppressMessage("Microsoft.Design", "CA1030", Justification = "This is a false positive - we do follow event model")]
		protected void RaiseDocumentPrinted(DeliveryInstructionDestination instructionsDestination)
		{
			if (instructionsDestination != DeliveryInstructionDestination.UserCancelled)
			{
				if (DocumentPrinted != null)
				{
					DocumentPrinted(this, new DocumentPrintedEventArgs(instructionsDestination, null));
				}
			}
		}

		#endregion

		#region IPaymentPrint Members

		void IPaymentPrint.PrintPaymentVoucher()
		{
			if (Payment != null)
			{
				Print(AllowedDeliveryOptions.All, PaymentVoucherMenuName);
			}
			else
			{
				PrintVoucherFromApproval(AllowedDeliveryOptions.All, PaymentVoucherMenuName);
			}
		}

		void IPaymentPrint.PrintRemittanceAdvice()
		{
			Print(AllowedDeliveryOptions.All, RemittanceAdviceMenuName);
		}

		void IPaymentPrint.PrintCheque()
		{
			PrintCheque(ZGuid.Empty);
		}

		void IPaymentPrint.PrintPaymentBatchListing()
		{
			if (Approval?.PaymentBatch != null)
			{
				using (var printingUtil = new AccPrintingUtility(Factory))
				{
					var printResult = printingUtil.PrintDocument(Approval, PaymentApprovalDocumentSupporter.PaymentBatchListingMenuName, AllowedDeliveryOptions.All, ZGuid.Empty, true);
					RaiseDocumentPrinted(printResult);
				}
			}
		}

		void IPaymentPrint.AutoPrintCheque(ZGuid printerPK)
		{
			PrintCheque(printerPK);
		}

		void PrintCheque(ZGuid printerPK)
		{
			if (Payment != null)
			{
				ChequeTemplate = GetTemplateNameForRemittanceAdvice(Payment);
				Payment.DocumentSupporter.Initialise(this);
#if DEBUG
				if (Globals.IsTest && TestAutoAllocationAndPrintCheques(Payment))
				{
					ValidateChequePrinting(Payment);
				}
#endif
				if (!printerPK.IsValid)
				{
					if (ValidateChequePrinting(Payment))
					{
						Print(ChequeTemplate, Core.Constants.DataContext.Cheques);
					}
				}
				else
				{
					AutoPrint(printerPK, ChequeTemplate, Core.Constants.DataContext.Cheques);
				}
			}
		}

		string IPaymentPrint.PaymentType
		{
			get
			{
				if (Payment != null)
				{
					return (string)Payment.AH_ReceiptType;
				}
				else if (PaymentCollection != null && PaymentCollection.Any())
				{
					return PaymentCollection.First().AH_ReceiptType;
				}
				else if (Approval != null)
				{
					return Approval.AV_PaymentType;
				}

				return "";
			}
		}

		#endregion

		#region IPaymentBatchPrint Members

		void IPaymentBatchPrint.PrintDocumentsForPaymentBatch(ZBool printPaymentVouchers, ZBool printRemittanceAdvices, ZBool printCheques, ZBool printPaymentBatchListing)
		{
			PrintDocumentsForPaymentBatch(printPaymentVouchers, printRemittanceAdvices, printCheques, printPaymentBatchListing, ZGuid.Empty);
		}

		void IPaymentBatchPrint.AutoPrintCheques(ZGuid printerPK)
		{
			PrintDocumentsForPaymentBatch(ZBool.False, ZBool.False, ZBool.True, ZBool.False, printerPK);
		}

		void PrintDocumentsForPaymentBatch(ZBool printPaymentVouchers, ZBool printRemittanceAdvices, ZBool printCheques, ZBool printPaymentBatchListing, ZGuid printerPK)
		{
			if (printPaymentBatchListing)
			{
				(this as IPaymentPrint).PrintPaymentBatchListing();
			}

			if (printPaymentVouchers)
			{
				using (var printingUtil = new AccPrintingUtility(Factory))
				{
					if (PaymentCollection.Any())
					{
						foreach (TransactionHeader payment in PaymentCollection)
						{
							AddDocument(printingUtil, PaymentVoucherMenuName, payment, useLegacyDocument: !DocumentsDataRegistry.Instance.UseNewDocBuilderPaymentVoucher.Value, findInSystemDefinedDocumentOnly: true);
						}

						printingUtil.PrintDocumentPacksForPaymentCollection(AllowedDeliveryOptions.All);
					}
					else if (Approval != null)
					{
						PrintVoucherFromApproval(AllowedDeliveryOptions.All, PaymentVoucherMenuName);
					}
				}
			}

			if (printRemittanceAdvices)
			{
				using (var printingUtil = new AccPrintingUtility(Factory))
				{
					foreach (TransactionHeader payment in PaymentCollection)
					{
						AddDocument(printingUtil, RemittanceAdviceMenuName, payment, useLegacyDocument: !DocumentsDataRegistry.Instance.UseNewDocBuilderRemittanceAdvice.Value, findInSystemDefinedDocumentOnly: true);
					}
					printingUtil.PrintDocumentPacksForPaymentCollection(AllowedDeliveryOptions.All);
				}
			}

			if (printCheques)
			{
				using (var printingUtil = new AccPrintingUtility(Factory))
				{
					foreach (TransactionHeader payment in PaymentCollection)
					{
						BasePayment paymentObj = payment as BasePayment;
						if (!((IChequeNumberAutoAllocation)paymentObj).ChequeIsAutoPrinted)
						{
							string chequeMenuName = GetTemplateNameForRemittanceAdvice(payment);
							ChequeTemplate = chequeMenuName;
							if (ValidateChequePrinting(payment))
							{
								AddDocument(printingUtil, chequeMenuName, payment, useLegacyDocument: false, findInSystemDefinedDocumentOnly: false);

								((IChequeNumberAutoAllocation)paymentObj).ChequeIsAutoPrinted = ZBool.True;
							}
						}
					}

					if (printingUtil.PaymentDocumentPacks.Count > 1)
					{
						PrintTask mergedPrintTask = new PrintTask();
						mergedPrintTask.DeliveryInstructionsDefaultPK = printingUtil.MenuPKForAPTransactionDeliveryInstructionHolder;

						foreach (DocumentPrintSet printSet in printingUtil.PaymentDocumentPacks.Values)
						{
							mergedPrintTask.AddRange(printSet.GetDocumentPacks());
						}
						printingUtil.PaymentDocumentPacks.Clear();
						printingUtil.PaymentDocumentPacks.Add(nameof(Core.Constants.DataContext.Cheques), mergedPrintTask);
					}

					if (printerPK != ZGuid.Empty)
					{
						printingUtil.AutoPrintDocumentPacksForPaymentCollection(printerPK);
					}
					else
					{
						printingUtil.PrintDocumentPacksForPaymentCollection(AllowedDeliveryOptions.All);
#if DEBUG
						if (Globals.IsTest)
						{
							CountOfPaymentDocumentPacksCreated = printingUtil.GetCountOfPaymentDocumentPacks();
							HasEmptyRecipient = printingUtil.DeliveryInstructionsPassedForPrintingHasEmptyRecipient();
						}
#endif
					}
				}
			}
		}

		#endregion

		#region IDocumentEvents Members

		public event DocumentCancelEventHandler DocumentPrintRequested;
		public event DocumentPrintedEventHandler DocumentPrePreviewed;
		public event DocumentPrintedEventHandler DocumentPrePrinted;
		public event DocumentPrintedEventHandler DocumentPrinted;

		#endregion

		#region TestCase
#if DEBUG
		int CountOfPaymentDocumentPacksCreated;
		bool HasEmptyRecipient = true;

		static bool IsChequeBookForTestAutoAllocationAndPrintCheques(AccChequeBook chequeBook)
		{
			return (chequeBook != null && chequeBook.AK_Desc == "TestAutoAllocationAndPrintCheques");
		}

		static bool TestAutoAllocationAndPrintCheques(TransactionHeader payment)
		{
			var allocation = payment as IChequeNumberAutoAllocation;
			return (allocation != null && IsChequeBookForTestAutoAllocationAndPrintCheques(allocation.ChequeBook));
		}

		public static void CheckAndThrowTestAutoAllocationAndPrintChequesFailure(AccChequeBook chequeBook)
		{
			if (IsChequeBookForTestAutoAllocationAndPrintCheques(chequeBook))
			{
				throw new ZCannotSaveException("Auto cheque printing failed. Please check Auto cheque printing settings. Testing Auto Print Cheques failure.", "TestAutoAllocationAndPrintCheques");
			}
		}

		public static bool PerformTestAutoAllocationAndPrintChequesFailure(Action action)
		{
			using (new DisposableAction(() => Globals.SetIsUnitTestingProductionFunctionality(true), () => Globals.SetIsUnitTestingProductionFunctionality(false)))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				action();
				string message = UnitTestUserNotification.Instance.LastMessage.Text;
				return message != null && message.StartsWith("Auto cheque printing failed. Please check Auto cheque printing settings. Testing Auto Print Cheques failure.", StringComparison.Ordinal);
			}
		}
#endif
		#endregion
	}
}
