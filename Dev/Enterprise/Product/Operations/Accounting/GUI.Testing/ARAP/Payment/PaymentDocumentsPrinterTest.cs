using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.GUI.ARAP.Payment;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.ARAP.ReceiptPayment.Testing
{
	public class PaymentDocumentsPrinterTest : TestCaseWithFactory
	{
		#region Implementation

		AccChequeBook CreateChequeBookAndBankWithChequeTemplateForAPayment(APPayment payment, string templateName)
		{
			AccBankAccount bankAccount = Factory.NewWithValidTestData<AccBankAccount>();
			bankAccount.AB_ChequeNumDigits = 1;
			StmTemplate chequeTemplate = Factory.LoadTop1<StmTemplate>(new ZQuery(StmTemplateSchema.SO_Name, templateName));
			bankAccount.AB_SO_ChequeTemplate = chequeTemplate.PK;
			AccChequeBook chequeBook = Factory.NewWithValidTestData<AccChequeBook>();
			chequeBook.AK_AutoPrintCheque = ZBool.True;
			chequeBook.AK_AB = bankAccount.PK;
			chequeBook.AK_StartNo = 2;
			chequeBook.AK_LastNo = 5;
			chequeBook.AK_CurrentNo = 3;

			payment.AH_AB = bankAccount.PK;
			payment.AH_ChequeOrReference = "3";
			Factory.Save();

			return chequeBook;
		}

		public class MockPaymentDocumentsPrinter : PaymentDocumentsPrinter
		{
			public MockPaymentDocumentsPrinter(Guid pK, BusinessObjectFactory factory)
				: base(pK, factory)
			{
			}

			public MockPaymentDocumentsPrinter(IEnumerable<TransactionHeader> transactions, PaymentApprovalBase approval, BusinessObjectFactory factory)
				: base(transactions, approval, factory)
			{
			}

			public AccChequeBook GetChequeBookExposed(string chequeNo, ZGuid bankPK)
			{
				return base.GetChequeBook(chequeNo, bankPK);
			}

			public string GetTemplateNameForRemittanceAdviceExposed(TransactionHeader payment)
			{
				return base.GetTemplateNameForRemittanceAdvice(payment);
			}

			protected override bool HasPermissionToPrintCheque()
			{
				return PermissionToPrintCheque;
			}

			protected override bool ReprintChequeAllowed
			{
				get { return fReprintChequeAllowed; }
			}

			public void SetReprintChequeAllowed(bool allowed)
			{
				fReprintChequeAllowed = allowed;
			}

			bool fReprintChequeAllowed;

			protected bool fPermissionToPrintCheque;
			public bool PermissionToPrintCheque
			{
				get { return fPermissionToPrintCheque; }
				set { fPermissionToPrintCheque = value; }
			}
		}

		#endregion

		[ExpectNoExceptions]
		public void TestPrintDocumentsForPaymentBatchWithChequeOptionCanPopulatesRecipient()
		{
			AccBankAccount bank = Factory.NewWithValidTestData(typeof(AccBankAccount)) as AccBankAccount;

			APPayment aPPaymentToTest = Factory.NewWithValidTestData(typeof(APPayment)) as APPayment;
			CreateChequeBookAndBankWithChequeTemplateForAPayment(aPPaymentToTest, "Singapore");
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			aPPaymentToTest.AH_OH = org.PK;

			var payments = new List<TransactionHeader>();
			payments.Add(aPPaymentToTest);
			PaymentDocumentsPrinter printer = new PaymentDocumentsPrinter(payments, null, Factory);

			AssertEquals("Precondition - Recipient PK should be empty", true, printer.HasEmptyRecipient_ForTestOnly);
			printer.PrintDocumentsForPaymentBatch_ForTestOnly(false, false, true, false, ZGuid.Empty);
			AssertEquals("Postcondition - Recipient PK should not be empty", false, printer.HasEmptyRecipient_ForTestOnly);
		}

		[ExpectNoExceptions]
		public void TestRemittanceAdviceCanPrintWhenUsingChineseLanguage()
		{
			AccBankAccount bank = Factory.NewWithValidTestData(typeof(AccBankAccount)) as AccBankAccount;

			StmTemplate invoiceTemplate = Factory.NewWithValidTestData(typeof(StmTemplate)) as StmTemplate;
			StmTemplate remittanceAdvice = Factory.NewWithValidTestData(typeof(StmTemplate)) as StmTemplate;

			invoiceTemplate.SO_Name = "AR Invoice";
			remittanceAdvice.SO_Name = "Remittance Advice";

			APPayment aPPaymentToTest = Factory.NewWithValidTestData(typeof(APPayment)) as APPayment;
			aPPaymentToTest.AH_AB = bank.PK;
			aPPaymentToTest.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.Cheque;
			aPPaymentToTest.AH_ChequeOrReference = "250";

			var payments = new List<TransactionHeader>();
			payments.Add(aPPaymentToTest);
			PaymentDocumentsPrinter printer = new PaymentDocumentsPrinter(payments, null, Factory);

			var staffWithChineseLanguage = Factory.NewWithValidTestData(typeof(GlbStaff)) as GlbStaff;
			staffWithChineseLanguage.GS_LoginName = "staffWithChineseLanguage";
			staffWithChineseLanguage.GS_WorkingLanguage = Enterprise.Core.Constants.Languages.ChineseSimplified;
			Factory.Save();

			using (Env.SetTemporaryUserContext(new UserContext(staffWithChineseLanguage.GS_LoginName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK)))
			{
				AssertEquals(Enterprise.Core.Constants.Languages.ChineseSimplified, Env.CurrentUser.Language);
				AssertEquals(Enterprise.Core.Constants.Languages.ChineseSimplified, ObjectFactory.Get<IResourceStrings>().CurrentLanguage);
				printer.PrintDocumentsForPaymentBatch_ForTestOnly(true, true, false, false, ZGuid.Empty);
				((IPaymentPrint)printer).PrintPaymentVoucher();
				((IPaymentPrint)printer).PrintRemittanceAdvice();
			}

			AssertEquals(Enterprise.Core.Constants.Languages.English, Env.CurrentUser.Language);
			AssertEquals(Enterprise.Core.Constants.Languages.English, ObjectFactory.Get<IResourceStrings>().CurrentLanguage);
			printer.PrintDocumentsForPaymentBatch_ForTestOnly(true, true, false, false, ZGuid.Empty);
			((IPaymentPrint)printer).PrintPaymentVoucher();
			((IPaymentPrint)printer).PrintRemittanceAdvice();
		}

		[ExpectNoExceptions]
		public void TestRemittanceAdvicePrintOKWithDocBuilderRegistryOnAndOff()
		{
			AccBankAccount bank = Factory.NewWithValidTestData(typeof(AccBankAccount)) as AccBankAccount;

			StmTemplate invoiceTemplate = Factory.NewWithValidTestData(typeof(StmTemplate)) as StmTemplate;
			StmTemplate remittanceAdvice = Factory.NewWithValidTestData(typeof(StmTemplate)) as StmTemplate;

			invoiceTemplate.SO_Name = "AR Invoice";
			remittanceAdvice.SO_Name = "Remittance Advice";

			APPayment aPPaymentToTest = Factory.NewWithValidTestData(typeof(APPayment)) as APPayment;
			aPPaymentToTest.AH_AB = bank.PK;
			aPPaymentToTest.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.Cheque;
			aPPaymentToTest.AH_ChequeOrReference = "250";

			var payments = new List<TransactionHeader>();
			payments.Add(aPPaymentToTest);
			PaymentDocumentsPrinter printer = new PaymentDocumentsPrinter(payments, null, Factory);

			DocumentsDataRegistry.Instance.UseNewDocBuilderRemittanceAdvice.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			DocumentsDataRegistry.Instance.UseNewDocBuilderPaymentVoucher.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			printer.PrintDocumentsForPaymentBatch_ForTestOnly(true, true, false, false, ZGuid.Empty);
			((IPaymentPrint)printer).PrintPaymentVoucher();
			((IPaymentPrint)printer).PrintRemittanceAdvice();

			DocumentsDataRegistry.Instance.UseNewDocBuilderRemittanceAdvice.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			DocumentsDataRegistry.Instance.UseNewDocBuilderPaymentVoucher.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			printer.PrintDocumentsForPaymentBatch_ForTestOnly(true, true, false, false, ZGuid.Empty);
			((IPaymentPrint)printer).PrintPaymentVoucher();
			((IPaymentPrint)printer).PrintRemittanceAdvice();
		}

		public void TestGetTemplateName()
		{
			AccBankAccount bank = Factory.NewWithValidTestData(typeof(AccBankAccount)) as AccBankAccount;
			AccBankAccount bank2 = Factory.NewWithValidTestData(typeof(AccBankAccount)) as AccBankAccount;
			AccChequeBook chequeBook1 = Factory.NewWithValidTestData(typeof(AccChequeBook)) as AccChequeBook;
			AccChequeBook chequeBook2 = Factory.NewWithValidTestData(typeof(AccChequeBook)) as AccChequeBook;
			AccChequeBook chequeBook3 = Factory.NewWithValidTestData(typeof(AccChequeBook)) as AccChequeBook;

			StmTemplate invoiceTemplate = Factory.NewWithValidTestData(typeof(StmTemplate)) as StmTemplate;
			StmTemplate chequeTemplate1 = Factory.NewWithValidTestData(typeof(StmTemplate)) as StmTemplate;
			StmTemplate chequeTemplate2 = Factory.NewWithValidTestData(typeof(StmTemplate)) as StmTemplate;
			StmTemplate remittanceAdvice = Factory.NewWithValidTestData(typeof(StmTemplate)) as StmTemplate;
			Factory.Save();

			chequeBook1.AK_AB = bank.PK;
			chequeBook2.AK_AB = bank.PK;
			chequeBook3.AK_AB = bank2.PK;

			chequeBook1.AK_StartNo = 1;
			chequeBook1.AK_LastNo = 500;
			chequeBook1.AK_AutoPrintCheque = true;

			chequeBook2.AK_StartNo = 1000;
			chequeBook2.AK_LastNo = 1500;
			chequeBook2.AK_AutoPrintCheque = true;

			chequeBook3.AK_StartNo = 1;
			chequeBook3.AK_LastNo = 500;
			chequeBook3.AK_AutoPrintCheque = true;

			invoiceTemplate.SO_Name = "AR Invoice";
			chequeTemplate1.SO_Name = "Cheque01";
			chequeTemplate2.SO_Name = "Cheque02";
			remittanceAdvice.SO_Name = "Remittance Advice";

			bank.AB_SO_ChequeTemplate = chequeTemplate1.PK;
			bank2.AB_SO_ChequeTemplate = chequeTemplate2.PK;

			APPayment aPPaymentToTest = Factory.NewWithValidTestData(typeof(APPayment)) as APPayment;
			aPPaymentToTest.AH_AB = bank.PK;
			aPPaymentToTest.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.Cheque;
			aPPaymentToTest.AH_ChequeOrReference = "250";

			MockPaymentDocumentsPrinter testPrintWrapper = new MockPaymentDocumentsPrinter(aPPaymentToTest.PK.ToGuid(), Factory);

			string templateName = testPrintWrapper.GetTemplateNameForRemittanceAdviceExposed(aPPaymentToTest);
			AssertEquals(chequeTemplate1.SO_Name, templateName);

			aPPaymentToTest.AH_ChequeOrReference = "1200";
			templateName = testPrintWrapper.GetTemplateNameForRemittanceAdviceExposed(aPPaymentToTest);
			AssertEquals(chequeTemplate1.SO_Name, templateName);

			chequeBook2.AK_AutoPrintCheque = false;
			templateName = testPrintWrapper.GetTemplateNameForRemittanceAdviceExposed(aPPaymentToTest);
			AssertEquals("", templateName);

			aPPaymentToTest.AH_AB = bank2.PK;
			aPPaymentToTest.AH_ChequeOrReference = "250";
			templateName = testPrintWrapper.GetTemplateNameForRemittanceAdviceExposed(aPPaymentToTest);
			AssertEquals(chequeTemplate2.SO_Name, templateName);

			bank2.AB_SO_ChequeTemplate = ZGuid.Empty;
			templateName = testPrintWrapper.GetTemplateNameForRemittanceAdviceExposed(aPPaymentToTest);
			AssertEquals("", templateName);
		}

		public void TestGetChequeBook()
		{
			AccBankAccount bank = Factory.NewWithValidTestData(typeof(AccBankAccount)) as AccBankAccount;
			AccBankAccount bank2 = Factory.NewWithValidTestData(typeof(AccBankAccount)) as AccBankAccount;

			AccChequeBook chequeBook1 = Factory.NewWithValidTestData(typeof(AccChequeBook)) as AccChequeBook;
			chequeBook1.AK_AB = bank.PK;
			chequeBook1.AK_StartNo = 1;
			chequeBook1.AK_LastNo = 500;

			AccChequeBook chequeBook2 = Factory.NewWithValidTestData(typeof(AccChequeBook)) as AccChequeBook;
			chequeBook2.AK_AB = bank.PK;
			chequeBook2.AK_StartNo = 1000;
			chequeBook2.AK_LastNo = 1500;

			AccChequeBook chequeBook3 = Factory.NewWithValidTestData(typeof(AccChequeBook)) as AccChequeBook;
			chequeBook3.AK_AB = bank2.PK;
			chequeBook3.AK_StartNo = 1;
			chequeBook3.AK_LastNo = 500;

			APPayment aPPaymentToTest = Factory.NewWithValidTestData(typeof(APPayment)) as APPayment;
			aPPaymentToTest.AH_AB = bank.PK;
			aPPaymentToTest.AH_ChequeOrReference = "250";
			aPPaymentToTest.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.Cheque;

			MockPaymentDocumentsPrinter testPrintWrapper = new MockPaymentDocumentsPrinter(aPPaymentToTest.PK.ToGuid(), Factory);
			AccChequeBook result = testPrintWrapper.GetChequeBookExposed("12", bank.PK);

			AssertEquals(chequeBook1.PK, result.PK);

			result = testPrintWrapper.GetChequeBookExposed("700", bank.PK);
			AssertNull(result);

			result = testPrintWrapper.GetChequeBookExposed("150", bank2.PK);
			AssertEquals(chequeBook3.PK, result.PK);

			result = testPrintWrapper.GetChequeBookExposed("1200", bank.PK);
			AssertEquals(chequeBook2.PK, result.PK);

			result = testPrintWrapper.GetChequeBookExposed("250.5", bank.PK);
			AssertNull(result);
		}

		public void TestShouldReprintCheque()
		{
			APPayment payment = Factory.New<APPayment>();
			payment.AH_InvoicePrinted = true;

			MockPaymentDocumentsPrinter testRemittanceAdvicePrint = new MockPaymentDocumentsPrinter(payment.PK.ToGuid(), Factory);
			testRemittanceAdvicePrint.PermissionToPrintCheque = true;
			testRemittanceAdvicePrint.SetReprintChequeAllowed(true);
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			Assert("Should be allowed to reprint cheque", testRemittanceAdvicePrint.ShouldReprintCheque_ForTestOnly());

			UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
			Assert("Should not reprint cheque", !testRemittanceAdvicePrint.ShouldReprintCheque_ForTestOnly());

			testRemittanceAdvicePrint.SetReprintChequeAllowed(false);
			Assert("Should not be allowed to reprint cheque", !testRemittanceAdvicePrint.ShouldReprintCheque_ForTestOnly());
			Assert("Error should be shown to user", UnitTestUserNotification.Instance.LastMessage.WasError);
		}

		public void TestValidateChequePrinting()
		{
			APPayment payment = Factory.New<APPayment>();
			payment.AH_IsCancelled = false;
			payment.AH_InvoicePrinted = false;

			MockPaymentDocumentsPrinter testRemittanceAdvicePrint = new MockPaymentDocumentsPrinter(payment.PK.ToGuid(), Factory);
			testRemittanceAdvicePrint.SetReprintChequeAllowed(true);
			testRemittanceAdvicePrint.PermissionToPrintCheque = true;
			testRemittanceAdvicePrint.ChequeTemplate_ForTestOnly = "STANDARD";
			bool chequePrintingAllowed = testRemittanceAdvicePrint.ValidateChequePrinting_ForTestOnly(payment);
			Assert("Should be able to print cheque", chequePrintingAllowed);
			Assert("No Error should be shown to user", UnitTestUserNotification.Instance.LastMessage.WasNone);
		}

		[ExpectNoExceptions()]
		public void TestValidateChequePrintingWhenNotAble()
		{
			CreateClientCheckTemplateAndMenu("Client Cheque Template 2", ZString.Empty);
			CreateClientCheckTemplateAndMenu("Client Cheque Template 3", "Check");

			APPayment payment = Factory.New<APPayment>();
			payment.AH_IsCancelled = true;
			payment.AH_InvoicePrinted = false;
			MockPaymentDocumentsPrinter testRemittanceAdvicePrint = new MockPaymentDocumentsPrinter(payment.PK.ToGuid(), Factory);
			bool chequePrintingAllowed = testRemittanceAdvicePrint.ValidateChequePrinting_ForTestOnly(payment);
			Assert("Should not be able to print cheque because it is cancelled", !chequePrintingAllowed);
			Assert("Error should be shown to user", UnitTestUserNotification.Instance.LastMessage.WasError);
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

			payment = Factory.New<APPayment>();
			payment.AH_IsCancelled = false;
			payment.AH_InvoicePrinted = false;

			testRemittanceAdvicePrint = new MockPaymentDocumentsPrinter(payment.PK.ToGuid(), Factory);
			testRemittanceAdvicePrint.SetReprintChequeAllowed(false);
			testRemittanceAdvicePrint.PermissionToPrintCheque = false;
			testRemittanceAdvicePrint.ChequeTemplate_ForTestOnly = "TESTTEMPLATE";
			chequePrintingAllowed = testRemittanceAdvicePrint.ValidateChequePrinting_ForTestOnly(payment);
			Assert("Should not be able to print cheque because user doesn't have permission", !chequePrintingAllowed);
			Assert("Error should be shown to user", UnitTestUserNotification.Instance.LastMessage.WasError);
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

			payment = Factory.New<APPayment>();
			payment.AH_IsCancelled = false;
			payment.AH_InvoicePrinted = false;

			testRemittanceAdvicePrint = new MockPaymentDocumentsPrinter(payment.PK.ToGuid(), Factory);
			testRemittanceAdvicePrint.SetReprintChequeAllowed(false);
			testRemittanceAdvicePrint.PermissionToPrintCheque = true;
			testRemittanceAdvicePrint.ChequeTemplate_ForTestOnly = "";
			chequePrintingAllowed = testRemittanceAdvicePrint.ValidateChequePrinting_ForTestOnly(payment);
			Assert("Should not be able to print cheque because template is not setup correctly", !chequePrintingAllowed);
			Assert("Error should be shown to user", UnitTestUserNotification.Instance.LastMessage.WasError);
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

			payment = Factory.New<APPayment>();
			payment.AH_IsCancelled = false;
			payment.AH_InvoicePrinted = false;

			testRemittanceAdvicePrint = new MockPaymentDocumentsPrinter(payment.PK.ToGuid(), Factory);
			testRemittanceAdvicePrint.SetReprintChequeAllowed(false);
			testRemittanceAdvicePrint.PermissionToPrintCheque = true;
			testRemittanceAdvicePrint.ChequeTemplate_ForTestOnly = "Client Cheque Template";
			chequePrintingAllowed = testRemittanceAdvicePrint.ValidateChequePrinting_ForTestOnly(payment);
			Assert("Should not be able to print cheque because Document menu is missing for cheque template", !chequePrintingAllowed);
			Assert("Error should be shown to user", UnitTestUserNotification.Instance.LastMessage.WasError);
			AssertEquals("Error should be shown to user", "Please make sure 'Client Cheque Template' Document menu with Empty menu path value exists for cheque template Client Cheque Template. If not, please create one through the Document Menu Customization screen", UnitTestUserNotification.Instance.LastMessage.Text);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

			testRemittanceAdvicePrint.ChequeTemplate_ForTestOnly = "Client Cheque Template 2";
			chequePrintingAllowed = testRemittanceAdvicePrint.ValidateChequePrinting_ForTestOnly(payment);
			Assert("Should  be able to print cheque because Document menu is not missing anymore", chequePrintingAllowed);

			//Now change ChequeTemplate
			testRemittanceAdvicePrint.ChequeTemplate_ForTestOnly = "Client Cheque Template 3";
			chequePrintingAllowed = testRemittanceAdvicePrint.ValidateChequePrinting_ForTestOnly(payment);
			Assert("Should not be able to print cheque because Document menu is missing for cheque template", !chequePrintingAllowed);
			Assert("Error should be shown to user", UnitTestUserNotification.Instance.LastMessage.WasError);
			AssertEquals("Error should be shown to user", "Please make sure 'Client Cheque Template 3' Document menu with Empty menu path value exists for cheque template Client Cheque Template 3. If not, please create one through the Document Menu Customization screen", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		void CreateClientCheckTemplateAndMenu(string templateName, string menuPath)
		{
			StmTemplate clientChequeTemplate = Factory.NewWithValidTestData<StmTemplate>();
			clientChequeTemplate.SO_Name = templateName;
			clientChequeTemplate.SO_DataContext = nameof(Core.Constants.DataContext.Cheques);
			clientChequeTemplate.SO_IsSystemDefined = true;
			clientChequeTemplate.SO_IsClientSpecific = false;

			StmMenuItem clientChequeMenu = Factory.NewWithValidTestData<StmMenuItem>();
			clientChequeMenu.SU_IsSystemDefined = true;
			clientChequeMenu.SU_IsClientSpecific = false;
			clientChequeMenu.SU_MenuName = templateName;
			clientChequeMenu.SU_BusinessContext = nameof(BusinessContext.APTransaction);
			clientChequeMenu.SU_MenuPath = menuPath;
			clientChequeMenu.SU_MenuType = "DOC";
			clientChequeMenu.SU_GS_NKStaffCode = "";

			StmMenuTemplatePivot newStmMenuTemplatePivot = Factory.New<StmMenuTemplatePivot>();
			newStmMenuTemplatePivot.SI_IsSystemDefined = clientChequeTemplate.SO_IsSystemDefined;
			newStmMenuTemplatePivot.SI_IsClientSpecific = clientChequeTemplate.SO_IsClientSpecific;
			newStmMenuTemplatePivot.SI_SU = clientChequeMenu.PK;
			newStmMenuTemplatePivot.SI_SO = clientChequeTemplate.PK;
			newStmMenuTemplatePivot.SI_DocumentTitle = clientChequeTemplate.SO_Name;
			newStmMenuTemplatePivot.SI_MenuTemplateFilter = null;

			Factory.Save();
		}

		public void TestIPaymentPrint_PaymentType()
		{
			APPayment payment = Factory.NewWithValidTestData<APPayment>();
			payment.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.Cash;
			MockPaymentDocumentsPrinter testRemittanceAdvicePrint = new MockPaymentDocumentsPrinter(payment.PK.ToGuid(), Factory);
			AssertEquals("Should return valid PaymentType", ZArchitecture.Core.ReceiptTypes.Cash, ((IPaymentPrint)testRemittanceAdvicePrint).PaymentType);

			payment.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.Cheque;
			APPayment payment2 = Factory.NewWithValidTestData<APPayment>();
			payment2.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.Cash;
			var transactions = new List<TransactionHeader>();
			transactions.Add(payment);
			transactions.Add(payment2);
			testRemittanceAdvicePrint = new MockPaymentDocumentsPrinter(transactions, null, Factory);
			AssertEquals("Should return valid PaymentType", ZArchitecture.Core.ReceiptTypes.Cheque, ((IPaymentPrint)testRemittanceAdvicePrint).PaymentType);

			testRemittanceAdvicePrint = new MockPaymentDocumentsPrinter(Guid.Empty, Factory);
			AssertEquals("Should return empty PaymentType", ZString.Empty, ((IPaymentPrint)testRemittanceAdvicePrint).PaymentType);
		}

		[ExpectNoExceptions()]
		public void TestPrintDocumentsForPaymentBatch()
		{
			APPayment payment = Factory.NewWithValidTestData<APPayment>();
			CreateChequeBookAndBankWithChequeTemplateForAPayment(payment, "STANDARD");
			APPayment payment2 = Factory.NewWithValidTestData<APPayment>();
			CreateChequeBookAndBankWithChequeTemplateForAPayment(payment2, "STANDARD");
			APPayment payment3 = Factory.NewWithValidTestData<APPayment>();
			CreateChequeBookAndBankWithChequeTemplateForAPayment(payment3, "STANDARD");

			var paymentCollection = new List<TransactionHeader>();
			paymentCollection.Add(payment);
			paymentCollection.Add(payment2);
			paymentCollection.Add(payment3);
			MockPaymentDocumentsPrinter testRemittanceAdvicePrint = new MockPaymentDocumentsPrinter(paymentCollection, null, Factory);
			((IPaymentBatchPrint)testRemittanceAdvicePrint).PrintDocumentsForPaymentBatch(ZBool.True, ZBool.True, ZBool.True, ZBool.False);
		}

		[ExpectNoExceptions()]
		public void TestPrintingChequesForPaymentBatchWithDifferentChequeTemplates()
		{
			APPayment payment = Factory.NewWithValidTestData<APPayment>();
			CreateChequeBookAndBankWithChequeTemplateForAPayment(payment, "USStandard");
			APPayment payment2 = Factory.NewWithValidTestData<APPayment>();
			CreateChequeBookAndBankWithChequeTemplateForAPayment(payment2, "USStandard");
			APPayment payment3 = Factory.NewWithValidTestData<APPayment>();
			CreateChequeBookAndBankWithChequeTemplateForAPayment(payment3, "UKStandard");

			var paymentCollection = new List<TransactionHeader>();
			paymentCollection.Add(payment);
			paymentCollection.Add(payment2);
			paymentCollection.Add(payment3);
			MockPaymentDocumentsPrinter testRemittanceAdvicePrint = new MockPaymentDocumentsPrinter(paymentCollection, null, Factory);
			testRemittanceAdvicePrint.PermissionToPrintCheque = true;
			((IPaymentBatchPrint)testRemittanceAdvicePrint).PrintDocumentsForPaymentBatch(ZBool.True, ZBool.True, ZBool.True, ZBool.False);
			AssertEquals("There should be only 1 print task created for cheques", 1, testRemittanceAdvicePrint.CountOfPaymentDocumentPacksCreated_ForTestOnly);
		}

		[ExpectNoExceptions()]
		public void TestAutoPrintDocumentsForPaymentBatch()
		{
			BusinessObject printQueue = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Enterprise.Integration.DocumentEngine.IStmPrintQueue)));
			APPayment payment = Factory.NewWithValidTestData<APPayment>();
			CreateChequeBookAndBankWithChequeTemplateForAPayment(payment, "STANDARD");
			APPayment payment2 = Factory.NewWithValidTestData<APPayment>();
			CreateChequeBookAndBankWithChequeTemplateForAPayment(payment2, "STANDARD");
			APPayment payment3 = Factory.NewWithValidTestData<APPayment>();
			CreateChequeBookAndBankWithChequeTemplateForAPayment(payment3, "STANDARD");

			Assert("flag should be set to false", !((IChequeNumberAutoAllocation)payment).ChequeIsAutoPrinted);
			Assert("flag should be set to false", !((IChequeNumberAutoAllocation)payment2).ChequeIsAutoPrinted);
			Assert("flag should be set to false", !((IChequeNumberAutoAllocation)payment3).ChequeIsAutoPrinted);

			var paymentCollection = new List<TransactionHeader>();
			paymentCollection.Add(payment);
			paymentCollection.Add(payment2);
			paymentCollection.Add(payment3);
			MockPaymentDocumentsPrinter testRemittanceAdvicePrint = new MockPaymentDocumentsPrinter(paymentCollection, null, Factory);
			testRemittanceAdvicePrint.PermissionToPrintCheque = true;
			((IPaymentBatchPrint)testRemittanceAdvicePrint).AutoPrintCheques(printQueue.PK);

			Assert("flag should be set to true", ((IChequeNumberAutoAllocation)payment).ChequeIsAutoPrinted);
			Assert("flag should be set to true", ((IChequeNumberAutoAllocation)payment2).ChequeIsAutoPrinted);
			Assert("flag should be set to true", ((IChequeNumberAutoAllocation)payment3).ChequeIsAutoPrinted);
		}

		[ExpectNoExceptions()]
		public void TestPrintingChequesForPaymentBatchPromptErrorIfCheckIsNotAutoPrint()
		{
			APPayment payment = Factory.NewWithValidTestData<APPayment>();
			AccChequeBook chequeBook = CreateChequeBookAndBankWithChequeTemplateForAPayment(payment, "USStandard");
			chequeBook.AK_AutoPrintCheque = ZBool.False;
			var paymentCollection = new List<TransactionHeader>();
			paymentCollection.Add(payment);
			MockPaymentDocumentsPrinter testRemittanceAdvicePrint = new MockPaymentDocumentsPrinter(paymentCollection, null, Factory);
			testRemittanceAdvicePrint.PermissionToPrintCheque = true;
			((IPaymentBatchPrint)testRemittanceAdvicePrint).PrintDocumentsForPaymentBatch(ZBool.False, ZBool.False, ZBool.True, ZBool.False);
			Assert("Error should be shown to user", UnitTestUserNotification.Instance.LastMessage.WasError);
			AssertEquals("Auto printing of check is not properly configured.", UnitTestUserNotification.Instance.LastMessage.Text);
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			AssertEquals("There should be only 0 print task created for cheques", 0, testRemittanceAdvicePrint.CountOfPaymentDocumentPacksCreated_ForTestOnly);
		}

		public void TestAutoAllocationAndPrintChequesErrorInTransaction()
		{
			var printQueue = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(Enterprise.Integration.DocumentEngine.IStmPrintQueue)));
			var payments = new List<TransactionHeader>();
			var payment = Factory.NewWithValidTestData<APPayment>();
			var chequeBook = CreateChequeBookAndBankWithChequeTemplateForAPayment(payment, "STANDARD");
			chequeBook.AK_Desc = "TestAutoAllocationAndPrintCheques";
			payments.Add(payment);
			var testRemittanceAdvicePrint = new MockPaymentDocumentsPrinter(payments, null, Factory);
			testRemittanceAdvicePrint.PermissionToPrintCheque = true;

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			((IPaymentBatchPrint)testRemittanceAdvicePrint).AutoPrintCheques(printQueue.PK);
			AssertEquals("Testing Auto Print Cheques failure.", UnitTestUserNotification.Instance.LastMessage.Text);

			var connection = ((IDbConnected)Factory).Connection;
			using (new DisposableAction(() => connection.BeginTransaction(), () => connection.RollbackTransaction()))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				var e = AssertExceptionThrown<ZCannotSaveException>("throws", () => ((IPaymentBatchPrint)testRemittanceAdvicePrint).AutoPrintCheques(printQueue.PK));
				AssertEquals("Auto cheque printing failed. Please check Auto cheque printing settings. Testing Auto Print Cheques failure.", e.Message);
			}
			Assert("No message shown to user.", string.IsNullOrEmpty(UnitTestUserNotification.Instance.LastMessage.Text));
		}
	}
}
