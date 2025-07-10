using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.HotCheque;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.CashBook.DirectPayment;
using Enterprise.Accounting.Business.CashBook.DirectReceipt;
using Enterprise.Accounting.Business.CashBook.Transfer;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.DocumentTests.RunDocuments
{
	sealed class TransactionHeaderRunDocsTest : BaseRunDocumentsTest
	{
		public TransactionHeaderRunDocsTest() { }

		[ExpectNoExceptions]
		public void TestRemittanceAdvice()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Remittance Advice");
			fBusinessContext = BusinessContext.APTransaction;
			RunDocumentWithAllSections = ZBool.False;
			RunDocument();

			BusinessObjectForTest = GetDirectPayment();
			RunDocument();

			BusinessObjectForTest = GetCashBookTransfer();
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestPaymentVoucher()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Payment Voucher");
			fBusinessContext = BusinessContext.APTransaction;
			RunDocumentWithAllSections = ZBool.False;
			RunDocument();

			BusinessObjectForTest = GetDirectPayment();
			RunDocument();

			BusinessObjectForTest = GetCashBookTransfer();
			RunDocument();
		}

		#region Cheques

		[ExpectNoExceptions]
		public void TestCheque01()
		{
			CreateDocumentMenuAndSetPivot("Cheque01 Test", "HKHSBC", nameof(Core.Constants.DataContext.Cheques));
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Cheque01 Test");
			RunDocumentWithAllSections = ZBool.False;
			RunDocument();

			BusinessObjectForTest = GetDirectPayment();
			RunDocument();

			BusinessObjectForTest = GetHotCheque();
			RunDocument();
		}

		// CHEQUE TEMPLATE 2 HAS BEEN REMOVED - BK

		[ExpectNoExceptions]
		public void TestCheque03()
		{
			CreateDocumentMenuAndSetPivot("Cheque03 Test", "Singapore", nameof(Core.Constants.DataContext.Cheques));
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Cheque03 Test");
			RunDocumentWithAllSections = ZBool.False;
			RunDocument();

			BusinessObjectForTest = GetDirectPayment();
			RunDocument();

			BusinessObjectForTest = GetHotCheque();
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestCheque04()
		{
			CreateDocumentMenuAndSetPivot("Cheque04 Test", "Standard", nameof(Core.Constants.DataContext.Cheques));
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Cheque04 Test");
			RunDocumentWithAllSections = ZBool.False;
			RunDocument();

			BusinessObjectForTest = GetDirectPayment();
			RunDocument();

			BusinessObjectForTest = GetHotCheque();
			RunDocument();
		}

		#endregion

		[ExpectNoExceptions]
		public void TestReceiptMatchingForAccounting()
		{
			CreateDocumentMenuAndSetPivot("Receipt Matching Test", "Receipt Matching", nameof(Core.Constants.DataContext.TransactionHeader));
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Receipt Matching Test");
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestReceiptJournal()
		{
			BusinessObjectForTest = Factory.New<ARReceipt>();
			CreateDocumentMenuAndSetPivot("Receipt Journal", "Receipt Matching", nameof(Core.Constants.DataContext.TransactionHeader));
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Receipt Journal");
			RunDocumentWithAllSections = ZBool.False;
			RunDocument();

			BusinessObjectForTest = Factory.New<DirectReceipt>();
			RunDocument();
		}

		#region Implementation
		BusinessObject BusinessObjectForTest;
		public override BusinessObject GetBusinessObject
		{
			get { return BusinessObjectForTest; }
		}

		ZQuery fFilterForMenuItem;
		public override ZQuery FilterForMenuItem
		{
			get { return fFilterForMenuItem; }
			set { fFilterForMenuItem = value; }
		}

		BusinessContext fBusinessContext;
		public override BusinessContext BusinessContext
		{
			get { return fBusinessContext; }
		}

		protected override void SetUp()
		{
			BusinessObjectForTest = Factory.New<APPayment>();
			fBusinessContext = BusinessContext.Test;
			base.SetUp();
		}

		DirectPayment GetDirectPayment()
		{
			var dPY = Factory.New<DirectPayment>();
			DirectPaymentLine line = (DirectPaymentLine)dPY.Lines.AddNew();
			line.AL_RX_NKTransactionCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			return dPY;
		}

		AccHotCheque GetHotCheque()
		{
			return Factory.New<AccHotCheque>();
		}

		BankTransferRow GetCashBookTransfer()
		{
			return Factory.New<BankTransferFromRow>();
		}
		#endregion
	}
}
