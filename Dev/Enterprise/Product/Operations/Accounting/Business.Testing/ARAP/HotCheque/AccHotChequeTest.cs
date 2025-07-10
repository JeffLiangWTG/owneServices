using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.CashBook.DirectPayment;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.HotCheque.Testing
{
	[TestedType(typeof(AccHotCheque))]
	public class AccHotChequeTest : EnterpriseBusinessObjectTestCase
	{
		public void TestIEdocsParsingSupportProvider()
		{
			var bo = Factory.NewWithValidTestData<AccHotCheque>();
			var eDocsParsingSupport = bo as IEDocsParsingSupport;
			AssertNotNull("IEDocsParsingSupport must be implemented", eDocsParsingSupport);
			Assert(eDocsParsingSupport.DenySendForParsing(new Guid(), "PIN", "testfile.pdf"));
		}

		public void TestZDecimalsHaveCorrectDecimalPlacesAccHotCheque()
		{
			AccHotCheque testCheque = Factory.New<AccHotCheque>();
			AccBankAccount testBank = Factory.New(typeof(AccBankAccount)) as AccBankAccount;
			RefCurrency currency = Factory.LoadTop1<RefCurrency>(new ZQuery());
			testBank.AB_RX_NKAccountCurrency = currency.RX_Code;
			AccChequeBook testBook = Factory.New(typeof(AccChequeBook)) as AccChequeBook;

			testBook.AK_AB = testBank.PK;
			testCheque.AQ_AK = testBook.PK;

			var amountList = new List<string>
			{
				nameof(testCheque.AQ_Amount)
			};

			var tester = new DecimalPlacesAttributeTester(testCheque);
			tester.CheckNonLocalCurrency(amountList, nameof(testCheque.AmountDecimals), nameof(testCheque.ChequeBook.BankAccount.AB_RX_NKAccountCurrency), testCheque.ChequeBook.BankAccount);
		}

		public void TestSetDefaultValues()
		{
			Assert(Cheque.AQ_ChequeDate != ZDateTime.Empty);
			AssertEquals(ZArchitecture.Core.ActualOrMaxIndicator.Actual, Cheque.AQ_ActualOrMaxIndicator);
		}

		public void TestOnLoaded()
		{
			AccHotCheque testCheque = Factory.New<AccHotCheque>();
			testCheque.OnLoaded();

			AccHotChequeTestHelper.AssertReadOnly("Fields should be editable, becuase system specs state that users can open and edit hot cheques.", testCheque, false);
		}

		public void TestAQ_OH()
		{
			OrgHeader header = Factory.LoadTop1(typeof(OrgHeader), new ZQuery()) as OrgHeader;
			header.OH_FullName = "!!!";
			Cheque.AQ_OH = header.PK;

			AssertEquals(Cheque.AQ_ChequePayee, header.OH_FullName);
		}
		public void TestAQ_AK()
		{
			AccChequeBook testBookWithAutoAllocation = GetAutoPrintChequeBook();
			AccChequeBook testBook = Factory.New(typeof(AccChequeBook)) as AccChequeBook;
			testBook.AK_CurrentNo = 2;
			Cheque.AQ_AK = testBook.PK;
			AssertEquals(Cheque.AQ_ChequeNumber, "2");
			Assert(testBook.AK_CurrentNo == 3);

			testBookWithAutoAllocation.AK_CurrentNo = 1;
			Cheque.AQ_AK = testBookWithAutoAllocation.PK;
			Assert("Cheque number should be cleared", Cheque.AQ_ChequeNumber.IsEmpty);
			AssertEquals("Cheque book current NO should remain old", 1m, testBookWithAutoAllocation.AK_CurrentNo);
			Assert("Cheque number field should become read only", Cheque.AQ_ChequeNumberInfo.ReadOnly);

			Cheque.AQ_AK = testBook.PK;
			AssertEquals(Cheque.AQ_ChequeNumber, "3");
			Assert(testBook.AK_CurrentNo == 4);
			Assert("Cheque number field should become editable again", !Cheque.AQ_ChequeNumberInfo.ReadOnly);
		}

		public void TestAQ_Calc_ActualAmountIndicator()
		{
			Cheque.AQ_ActualOrMaxIndicator = ZArchitecture.Core.ActualOrMaxIndicator.Actual;
			Assert(Cheque.AQ_Calc_ActualAmountIndicator);
			Cheque.AQ_ActualOrMaxIndicator = ZArchitecture.Core.ActualOrMaxIndicator.Max;
			Assert(!Cheque.AQ_Calc_ActualAmountIndicator);

			Cheque.AQ_Calc_ActualAmountIndicator = true;
			AssertEquals(Cheque.AQ_ActualOrMaxIndicator, ZArchitecture.Core.ActualOrMaxIndicator.Actual);
			Cheque.AQ_Calc_ActualAmountIndicator = false;
			AssertEquals(Cheque.AQ_ActualOrMaxIndicator, ZArchitecture.Core.ActualOrMaxIndicator.Max);
		}

		public void TestAQ_Calc_MaximumAmountIndicator()
		{
			Cheque.AQ_ActualOrMaxIndicator = ZArchitecture.Core.ActualOrMaxIndicator.Actual;
			Assert(!Cheque.AQ_Calc_MaximumAmountIndicator);
			Cheque.AQ_ActualOrMaxIndicator = ZArchitecture.Core.ActualOrMaxIndicator.Max;
			Assert(Cheque.AQ_Calc_MaximumAmountIndicator);

			Cheque.AQ_Calc_MaximumAmountIndicator = true;
			AssertEquals(Cheque.AQ_ActualOrMaxIndicator, ZArchitecture.Core.ActualOrMaxIndicator.Max);
			Cheque.AQ_Calc_MaximumAmountIndicator = false;
			AssertEquals(Cheque.AQ_ActualOrMaxIndicator, ZArchitecture.Core.ActualOrMaxIndicator.Actual);
		}

		public void TestAQ_Calc_RX()
		{
			AccBankAccount testBank = Factory.New(typeof(AccBankAccount)) as AccBankAccount;
			RefCurrency currency = Factory.LoadTop1<RefCurrency>(new ZQuery());
			testBank.AB_RX_NKAccountCurrency = currency.RX_Code;

			AccChequeBook testBook = Factory.New(typeof(AccChequeBook)) as AccChequeBook;
			testBook.AK_AB = testBank.PK;

			Cheque.AQ_AK = testBook.PK;
			AssertEquals(Cheque.AQ_Calc_RX_NK, testBank.AB_RX_NKAccountCurrency);
		}

		public void TestAQ_Calc_ChequeStatus()
		{
			Cheque.AQ_Cancelled = true;
			AssertEquals(Cheque.AQ_Calc_ChequeStatus, AccHotCheque.CANCELLED);
			Cheque.AQ_Cancelled = false;
			Cheque.AQ_AH = ZGuid.NewZGuid();
			AssertEquals(Cheque.AQ_Calc_ChequeStatus, AccHotCheque.POSTED);
			Cheque.AQ_AH = ZGuid.Empty;
			AssertEquals(Cheque.AQ_Calc_ChequeStatus, AccHotCheque.ACTIVE);
		}

		public void TestCreatingUser()
		{
			Factory.Save();
			AssertEquals("Creating User", GlbStaff.CurrentUser.GS_FullName, Cheque.CreatingUser);
		}

		public void TestCreatingUserOnNew()
		{
			AccHotCheque cheque2 = Factory.New<AccHotCheque>();
			AssertEquals("Creating User", ZString.Empty, cheque2.CreatingUser);
		}

		public void TestCreateDate()
		{
			Factory.Save();
			AssertEquals("Created Date", ZDateTime.Now.Date, Cheque.CreatedDate.Date);
		}

		public void TestCreateDateOnNew()
		{
			AccHotCheque cheque2 = Factory.New<AccHotCheque>();
			AssertEquals("Created Date", ZDateTime.Empty, cheque2.CreatedDate);
		}

		public void TestCreatingUserReadOnly()
		{
			Assert("Creating User should always be readonly", Cheque.CreatingUserInfo.ReadOnly);
		}

		public void TestCreatedDateReadOnly()
		{
			Assert("Created Date should always be readonly", Cheque.CreatedDateInfo.ReadOnly);
		}

		public void TestCreditors()
		{
			AssertNotNull(Cheque.Creditors);
		}

		public void TestAccChequeBooks()
		{
			AssertNotNull(Cheque.AccChequeBooks);
		}

		public void TestCurrencies()
		{
			AssertNotNull(Cheque.Currencies);
		}

		public void TestJobHeaders()
		{
			AssertNotNull(Cheque.JobHeaders);
		}
		public void TestTransactionHeaders()
		{
			AssertNotNull(Cheque.TransactionHeaders);
		}

		public void TestChequeNumberInUse()
		{
			PrepareForTestChequeNumberInUse();

			AccHotCheque testCheque2 = Factory.NewWithValidTestData<AccHotCheque>();
			testCheque2.AQ_AK = fTestChequeBook.PK;

			testCheque2.AQ_ChequeNumber = "10001";
			AssertHasErrors("JobCharge with this cheque number", testCheque2.AQ_ChequeNumberInfo);
			testCheque2.AQ_ChequeNumber = "10002";
			AssertNoErrors("Payment Approval with this cheque number", testCheque2.AQ_ChequeNumberInfo);
			testCheque2.AQ_ChequeNumber = "10003";
			AssertHasErrors("Hot Cheque with this cheque number", testCheque2.AQ_ChequeNumberInfo);
			testCheque2.AQ_ChequeNumber = "10004";
			AssertHasErrors("AP Payment with this cheque number", testCheque2.AQ_ChequeNumberInfo);
			testCheque2.AQ_ChequeNumber = "10005";
			AssertHasErrors("Direct Payment with this cheque number", testCheque2.AQ_ChequeNumberInfo);
			testCheque2.AQ_ChequeNumber = "10006";
			AssertNoErrors("Reversed Payment with this cheque number", testCheque2.AQ_ChequeNumberInfo);
			testCheque2.AQ_ChequeNumber = "10007";
			AssertNoErrors("Nothing using this cheque number", testCheque2.AQ_ChequeNumberInfo);
		}

		public void TestCancelledChequeNumber()
		{
			APPayment testPayment = Factory.NewWithValidTestData<APPayment>();
			testPayment.AH_AB = fTestBank.PK;
			testPayment.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.Cheque;
			testPayment.ChequeBook = fTestChequeBook.PK;
			testPayment.AH_ChequeOrReference = "12345";
			AssertEquals("Valid cheque number", false, testPayment.AH_ChequeOrReferenceInfo.HasErrors());

			testPayment.AH_IsCancelled = true;
			((IMatching)testPayment).CurrentMatchGroup.AddNew().AP_AH = testPayment.PK;
			TestObjectCreator.SetupMatchLinkMatchDate(testPayment);
			Factory.Save();

			AccHotCheque testCheque1 = Factory.NewWithValidTestData<AccHotCheque>();
			testCheque1.AQ_AK = fTestChequeBook.PK;
			testCheque1.AQ_ChequeNumber = "12345";
			AssertEquals("Cancelled Cheque number", false, testCheque1.AQ_ChequeNumberInfo.HasErrors());

			//TestCheque1.AQ_ChequeNumber = "11111";

			APPayment testPayment2 = Factory.NewWithValidTestData<APPayment>();
			testPayment2.AH_AB = fTestBank.PK;
			testPayment2.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.Cheque;
			testPayment2.ChequeBook = fTestChequeBook.PK;
			testPayment2.AH_ChequeOrReference = "12345";
			testPayment2.AH_IsCancelled = false;
			Factory.Save();

			testCheque1 = Factory.NewWithValidTestData<AccHotCheque>();
			testCheque1.AQ_AK = fTestChequeBook.PK;
			testCheque1.AQ_ChequeNumber = "12345";
			AssertEquals("Used Cheque number", true, testCheque1.AQ_ChequeNumberInfo.HasErrors());
		}

		public void TestValidateMiddleChequeNumber()
		{
			AccHotCheque testCheque1 = Factory.NewWithValidTestData<AccHotCheque>();
			testCheque1.AQ_AK = fTestChequeBook.PK;
			testCheque1.AQ_ChequeNumber = "10005";
			AssertEquals("Valid cheque number", false, testCheque1.AQ_ChequeNumberInfo.HasErrors());
			Factory.Save();

			AccHotCheque testCheque2 = Factory.NewWithValidTestData<AccHotCheque>();
			testCheque2.AQ_AK = fTestChequeBook.PK;
			testCheque2.AQ_ChequeNumber = "10007";
			AssertEquals("Valid cheque number", false, testCheque2.AQ_ChequeNumberInfo.HasErrors());
			Factory.Save();

			AccHotCheque testCheque3 = Factory.NewWithValidTestData<AccHotCheque>();
			testCheque3.AQ_AK = fTestChequeBook.PK;
			testCheque3.AQ_ChequeNumber = "10006";
			AssertEquals("Valid cheque number", false, testCheque3.AQ_ChequeNumberInfo.HasErrors());
			Factory.Save();

			AccHotCheque testCheque4 = Factory.NewWithValidTestData<AccHotCheque>();
			testCheque4.AQ_AK = fTestChequeBook.PK;
			testCheque4.AQ_ChequeNumber = "10007";
			AssertEquals("Invalid cheque number", true, testCheque4.AQ_ChequeNumberInfo.HasErrors());
		}

		public void TestDocManagerCode()
		{
			AssertEquals("Code should be CHQ. Any change to the IDocManagerSupport interface must also be changed in document scanning lookup", "CHQ", ((IDocManagerSupport)Cheque).DocManagerInfo.DocManagerCode);
		}

		public void TestCalc_ChequeNumberIsAutoAllocatedLabel()
		{
			AccChequeBook testBookWithAutoAllocation = GetAutoPrintChequeBook();
			Cheque = Factory.NewWithValidTestData<AccHotCheque>();
			Cheque.AQ_AK = fTestChequeBook.PK;
			Assert("Cheque book not IsAutoPrint", Cheque.Calc_ChequeNumberIsAutoAllocatedLabel.IsEmpty);

			Cheque.AQ_AK = testBookWithAutoAllocation.PK;
			AssertEquals("Cheque book IsAutoPrint", AccountingConstants.ChequeLabelConstants.ChequeNumberIsAutoAllocatedLabel, Cheque.Calc_ChequeNumberIsAutoAllocatedLabel);

			Factory.Save();
			Assert("Cheque should be in database", Cheque.IsInDatabase);
			Assert("Label should be empty as cheque is in database now", Cheque.Calc_ChequeNumberIsAutoAllocatedLabel.IsEmpty);
		}

		public void TestCalc_ChequeIsAutoPrintedLabel()
		{
			AccChequeBook testBookWithAutoAllocation = GetAutoPrintChequeBook();
			Cheque = Factory.NewWithValidTestData<AccHotCheque>();
			Cheque.AQ_AK = fTestChequeBook.PK;
			Assert("Cheque book not IsAutoPrint", Cheque.Calc_ChequeIsAutoPrintedLabel.IsEmpty);
			Assert("IsAutoAllocationEnabled should return False", !((IChequeNumberAutoAllocation)Cheque).IsAutoAllocationEnabled);

			Cheque.AQ_AK = testBookWithAutoAllocation.PK;
			AssertEquals("Cheque book IsAutoPrint", AccountingConstants.ChequeLabelConstants.ChequeAutoPrintedLabel, Cheque.Calc_ChequeIsAutoPrintedLabel);
			Assert("IsAutoAllocationEnabled should return True", ((IChequeNumberAutoAllocation)Cheque).IsAutoAllocationEnabled);

			Factory.Save();
			Assert("Cheque should be in database", Cheque.IsInDatabase);
			Assert("Label should be empty as cheque is in database now", Cheque.Calc_ChequeIsAutoPrintedLabel.IsEmpty);
		}

		public void TestChequeBookCollecitonDoesNotContainInActiveRecords()
		{
			AccBankAccount testBank = Factory.NewWithValidTestData<AccBankAccount>();
			testBank.AB_GB = GlbBranch.CurrentBranch.PK;
			AccChequeBook testBook1 = Factory.NewWithValidTestData<AccChequeBook>();
			testBook1.AK_AB = testBank.PK;
			AccChequeBook testBook2 = Factory.NewWithValidTestData<AccChequeBook>();
			testBook2.AK_AB = testBank.PK;

			testBook1.AK_IsActive = ZBool.False;
			testBook2.AK_IsActive = ZBool.True;
			Factory.Save();
			Cheque.AccChequeBooks.Load();
			Assert(Cheque.AccChequeBooks.Contains(testBook2));
			Assert(!Cheque.AccChequeBooks.Contains(testBook1));
		}

		public void TestFieldsAreReadOnlyOnPrintedCheque()
		{
			AccChequeBook testBookWithAutoAllocation = GetAutoPrintChequeBook();
			Cheque = Factory.NewWithValidTestData<AccHotCheque>();

			foreach (bool printed in new bool[] { false, true })
			{
				Cheque.AQ_Printed = printed;
				AssertEquals("ReadOnly: AQ_ChequeDate", printed, Cheque.AQ_ChequeDateInfo.ReadOnly);
				AssertEquals("ReadOnly: AQ_OH", printed, Cheque.AQ_OHInfo.ReadOnly);
				AssertEquals("ReadOnly: AQ_AK", printed, Cheque.AQ_AKInfo.ReadOnly);
				AssertEquals("ReadOnly: AQ_ChequeNumber", printed, Cheque.AQ_ChequeNumberInfo.ReadOnly);
				AssertEquals("ReadOnly: AQ_ChequePayee", printed, Cheque.AQ_ChequePayeeInfo.ReadOnly);
				AssertEquals("ReadOnly: AQ_Amount", printed, Cheque.AQ_AmountInfo.ReadOnly);
				AssertEquals("ReadOnly: AQ_GS_NKResponsibleStaff", printed, Cheque.AQ_GS_NKResponsibleStaffInfo.ReadOnly);
			}
		}

		#region IChequeNumberAutoAllocation Members Tests

		public void TestIChequeNumberAutoAllocation_ChequeBookPK()
		{
			AssertNull("Should return empty cheque book", ((IChequeNumberAutoAllocation)Cheque).ChequeBook);
			Cheque.AQ_AK = fTestChequeBook.PK;
			AssertEquals("Should return fTestChequeBook PK", fTestChequeBook, ((IChequeNumberAutoAllocation)Cheque).ChequeBook);
		}

		public void TestIChequeNumberAutoAllocation_AssignChequeNumber()
		{
			Assert("AQ_ChequeNumber should be empty", Cheque.AQ_ChequeNumber.IsEmpty);
			((IChequeNumberAutoAllocation)Cheque).AssignChequeNumber("123");
			AssertEquals("AQ_ChequeNumber should be set", "123", Cheque.AQ_ChequeNumber);
		}

		public void TestIChequeNumberAutoAllocation_IsAllocationPerformed()
		{
			Assert("Should return False by defualt", !((IChequeNumberAutoAllocation)Cheque).IsAllocationPerformed);
			((IChequeNumberAutoAllocation)Cheque).AssignChequeNumber("123");
			Assert("Should", ((IChequeNumberAutoAllocation)Cheque).IsAllocationPerformed);
		}

		public void TestIChequeNumberAutoAllocation_Printing_ObjectPK()
		{
			AssertEquals(Cheque.PK, ((IChequeNumberAutoAllocation)Cheque).Printing_ObjectPK);
		}

		public void TestIChequeNumberAutoAllocation_Printing_PrinterPK()
		{
			BusinessObject stmPrintQueue = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Enterprise.Integration.DocumentEngine.IStmPrintQueue)));
			fTestChequeBook.AK_SQ = stmPrintQueue.PK;
			Factory.Save();
			AssertEquals("Should return empty cheque book", ZGuid.Empty, ((IChequeNumberAutoAllocation)Cheque).Printing_PrinterPK);
			Cheque.AQ_AK = fTestChequeBook.PK;
			AssertEquals("Should return fTestChequeBook PK", stmPrintQueue.PK, ((IChequeNumberAutoAllocation)Cheque).Printing_PrinterPK);
		}

		public void TestIChequeNumberAutoAllocation_ChequeIsAutoPrinted()
		{
			Assert("Default value", !((IChequeNumberAutoAllocation)Cheque).ChequeIsAutoPrinted);
			((IChequeNumberAutoAllocation)Cheque).ChequeIsAutoPrinted = ZBool.True;
			Assert("Value should be changed", ((IChequeNumberAutoAllocation)Cheque).ChequeIsAutoPrinted);
		}

		public void TestIChequeNumberAutoAllocation_AllocationOrPrintingFailed()
		{
			fTestChequeBook.AK_IsActive = ZBool.False;
			fTestChequeBook.Factory.Save();
			fTestChequeBook.AK_IsActive = ZBool.True;
			Cheque.AQ_AK = fTestChequeBook.PK;
			Cheque.AQ_ChequeNumber = "BLAH!";
			((IChequeNumberAutoAllocation)Cheque).AllocationOrPrintingFailed();
			Assert("ChequeNumber should be reset", Cheque.AQ_ChequeNumber.IsEmpty);
			Assert("Cheque book should be reloaded", !fTestChequeBook.AK_IsActive);
		}

		#endregion

		#region Implementation

		AccHotCheque Cheque;
		AccBankAccount fTestBank;
		AccChequeBook fTestChequeBook;

		protected override void SetUp()
		{
			base.SetUp();
			Cheque = Factory.New<AccHotCheque>();
			Cheque.AQ_Printed = false;

			fTestBank = Factory.NewWithValidTestData<AccBankAccount>();
			fTestBank.AB_ChequeNumDigits = 5;
			fTestBank.AB_AG = Factory.LoadTop1(typeof(AccGLHeader), new ZQuery()).PK;
			fTestBank.AB_GB = Factory.LoadTop1(typeof(GlbBranch), new ZQuery()).PK;

			fTestChequeBook = Factory.NewWithValidTestData<AccChequeBook>();
			fTestChequeBook.AK_AB = fTestBank.PK;
			fTestChequeBook.AK_StartNo = 10000;
			fTestChequeBook.AK_LastNo = 99999;

			Factory.Save();
		}

		void PrepareForTestChequeNumberInUse()
		{
			TestObjectCreator creator = new TestObjectCreator(Factory);
			Job testJob = creator.CreateJob(creator.LocalClient, 0, creator.Agent, 0);

			Charge charge = creator.CreateCharge(testJob, creator.CC1, "Charge 1", creator.AUD, 1m, creator.Creditor1, creator.AUD, 1m, creator.Agent);
			charge.JR_RX_NKCostCurrency = creator.AUD.RX_Code;
			charge.JR_OSCostExRate = 1m;
			charge.JR_APInvoiceNum = "5";
			charge.JR_APInvoiceDate = ZDateTime.Now.AddDays(1);
			charge.JR_PaymentDate = ZDateTime.Now.AddDays(2);
			charge.JR_PaymentType = ZArchitecture.Core.ReceiptTypes.Cheque;
			charge.JR_AB = fTestBank.PK;
			charge.BankAccount.AB_ChequeNumDigits = 5;
			charge.JR_AK = fTestChequeBook.PK;
			charge.JR_ChequeNo = "10001";
			Assert("Cheque number is not used yet", !charge.JR_ChequeNoInfo.HasErrors());

			APPaymentApprovalWithAuthorisation paymentApproval = Factory.New<APPaymentApprovalWithAuthorisation>();
			paymentApproval.AV_OH = creator.ABIGAS.PK;
			paymentApproval.AV_AB = fTestBank.PK;
			paymentApproval.AV_PaymentType = ZArchitecture.Core.ReceiptTypes.Cheque;
			paymentApproval.AV_AK = fTestChequeBook.PK;
			paymentApproval.AV_ChequeOrReference = "10002";
			Assert("Cheque number is not used yet", !paymentApproval.AV_ChequeOrReferenceInfo.HasErrors());

			AccHotCheque hotCheque = Factory.NewWithValidTestData<AccHotCheque>();
			hotCheque.AQ_AK = fTestChequeBook.PK;
			hotCheque.AQ_ChequeNumber = "10003";
			Assert("Cheque number is not used yet", !hotCheque.AQ_ChequeNumberInfo.HasErrors());

			Payment payment = Factory.NewWithValidTestData<APPayment>();
			payment.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.Cheque;
			payment.AH_AB = fTestBank.PK;
			payment.ChequeBook = fTestChequeBook.PK;
			payment.AH_ChequeOrReference = "10004";
			Assert("Cheque number is not used yet", !payment.AH_ChequeOrReferenceInfo.HasErrors());

			DirectPayment directPayment = Factory.NewWithValidTestData<DirectPayment>();
			directPayment.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.Cheque;
			directPayment.AH_AB = fTestBank.PK;
			directPayment.ChequeBookPK = fTestChequeBook.PK;
			directPayment.AH_ChequeOrReference = "10005";
			Assert("Cheque number is not used yet", !directPayment.AH_ChequeOrReferenceInfo.HasErrors());

			Payment reversedPayment = Factory.NewWithValidTestData<APPayment>();
			reversedPayment.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.Cheque;
			reversedPayment.AH_AB = fTestBank.PK;
			reversedPayment.ChequeBook = fTestChequeBook.PK;
			reversedPayment.AH_ChequeOrReference = "10006";
			reversedPayment.AH_IsCancelled = true;
			((IMatching)reversedPayment).CurrentMatchGroup.AddNew().AP_AH = reversedPayment.PK;
			TestObjectCreator.SetupMatchLinkMatchDate(reversedPayment);
			Factory.Save();
		}

		AccChequeBook GetAutoPrintChequeBook()
		{
			AccBankAccount bankAccount = Factory.NewWithValidTestData<AccBankAccount>();
			bankAccount.AB_ChequeNumDigits = 1;
			bankAccount.AB_SO_ChequeTemplate = TestObjectCreator.StandardTemplatePK;
			AccChequeBook chequeBook = Factory.NewWithValidTestData<AccChequeBook>();
			chequeBook.AK_AutoPrintCheque = ZBool.True;
			chequeBook.AK_AB = bankAccount.PK;
			Assert("Cheque Book should be AutoPrint", chequeBook.IsAutoPrint);
			Factory.Save();
			return chequeBook;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.New(typeof(AccHotCheque));
		}

		protected override bool CanPersistedObjectBeDeleted
		{
			get { return false; }
		}

		#endregion
	}
}
