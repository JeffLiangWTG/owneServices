using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.AccQueryClaims;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.DataTransfer.eNett_Integration;
using Enterprise.Accounting.DataTransfer.eNett_Integration.Testing;
using Enterprise.Accounting.GUI.ARAP.PaymentApproval;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Encryption;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(APPaymentProcessingModule))]
	public class APPaymentProcessingModuleTest : PaymentProcessingModuleTest
	{
		protected override string AR_AP => "AP";

		protected override SecurityCheckpoint FirstApprovalCheckPoint
		{
			get { return Env.Security.APPaymentProcessingFirstApproval; }
		}

		protected override SecurityCheckpoint SecondApprovalCheckPoint
		{
			get { return Env.Security.APPaymentProcessingSecondApproval; }
		}

		protected override SecurityCheckpoint ThirdApprovalCheckPoint
		{
			get { return Env.Security.APPaymentProcessingThirdApproval; }
		}

		protected override SecurityCheckpoint CancelApprovalCheckPoint
		{
			get { return Env.Security.APPaymentProcessingCancelApproval; }
		}

		protected override SecurityCheckpoint PostCheckPoint
		{
			get { return Env.Security.APPaymentProcessingPost; }
		}

		protected override SecurityCheckpoint NewCashPaymentCheckPoint
		{
			get { return Env.Security.NewPayablesPaymentCash; }
		}

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.APPaymentProcessing;
		}

		protected override PaymentApprovalWithAuthorisation GetNewPaymentApproval()
		{
			return GetNewPaymentApproval(Factory);
		}

		protected override PaymentApprovalWithAuthorisation GetNewPaymentApproval(BusinessObjectFactory factory)
		{
			APPaymentApprovalWithAuthorisation approval = factory.New<APPaymentApprovalWithAuthorisation>();
			approval.AV_OH = TestOrgHeader.PK;
			approval.AV_Amount = 10000m;
			return approval;
		}

		protected override PaymentProcessingModule GetNewModule()
		{
			return new APPaymentProcessingModuleForTest();
		}

		protected override void AddTestObjects(IBusinessObjectCollection collection)
		{
			APPaymentApprovalWithAuthorisation approval1 = Factory.New<APPaymentApprovalWithAuthorisation>();
			approval1.AV_OH = TestOrgHeader.PK;
			collection.Add(approval1);

			APPaymentApprovalWithAuthorisation approval2 = Factory.New<APPaymentApprovalWithAuthorisation>();
			approval2.AV_OH = TestOrgHeader.PK;
			collection.Add(approval2);

			APPaymentApprovalWithAuthorisation approval3 = Factory.New<APPaymentApprovalWithAuthorisation>();
			approval3.AV_OH = TestOrgHeader.PK;
			collection.Add(approval3);

			APPaymentApprovalWithAuthorisation approval4 = Factory.New<APPaymentApprovalWithAuthorisation>();
			approval4.AV_OH = TestOrgHeader.PK;
			collection.Add(approval4);
		}

		protected override void SelectBusinessObjects(PaymentProcessingModule module, BusinessObject[] businessObjects)
		{
			APPaymentProcessingModuleForTest moduleAsAP = module as APPaymentProcessingModuleForTest;
			moduleAsAP.SetfSelectedBusinessObjects(businessObjects);
		}

		public void TestPostPaymentApprovalsViaENettCreditCard()
		{
			new AccountingPeriodTestHelper().SetupPeriods();

			bool originalEnableCreditCardPaymentsViaComPay = AccountingConfigurationRegistry.Instance.EnableCreditCardPaymentsViaComPay.Value;
			AccountingConfigurationRegistry.Instance.EnableCreditCardPaymentsViaComPay.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			Factory.Save();

			var header = Factory.NewWithValidTestData<AccGLHeader>();
			header.AG_AccountNum = "ZZAUDAcc";
			var bankAccount = TestObjectCreator.CreateBankAccount("ZZHSBCAUD", "HSBC AUD ACCT", "HSBC", "AUD", TestObjectCreator.AUD, "123456", "12345678", header);
			bankAccount.AB_DebitCreditCardExpiry = "0699";
			bankAccount.AB_DebitCreditCardName = "MR JOHN SMITH";
			var encoder = new TwoWayEncoder(bankAccount.PK.ToGuid());
			bankAccount.AB_DebitCreditCardNumber = encoder.Encrypt("1234567812345678");
			bankAccount.AB_AccountType = AccountTypeCodeDescriptionPairList.Codes.CCD;
			bankAccount.AB_AccountNum = "**** **** ***4 5678";

			OrgHeader orgHeader = TestOrgHeader;
			OrgCusCode cusCode = orgHeader.CustomsCodes.AddNew();
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.eNettRegistrationNumber;
			cusCode.OK_CustomsRegNo = "123456";
			cusCode.OK_RN_NKCodeCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			AssertEquals("eNettRegistrationNumber", "123456", orgHeader.ENettRegistrationNumber);

			PaymentApprovalWithAuthorisation approval1 = GetNewPaymentApprovalReadyToPost("00001002", "0002", 1000M);
			approval1.AV_Status = PaymentApprovalStatus.AwaitingApproval;
			approval1.AV_OH = orgHeader.PK;

			PaymentApprovalWithAuthorisation approval2 = GetNewPaymentApprovalReadyToPost("00001003", "0003", 1000M);
			approval2.AV_Status = PaymentApprovalStatus.FullyApproved;
			approval2.AV_PaymentType = ReceiptTypes.eNettCreditCard;
			approval2.AV_AB = bankAccount.PK;
			approval2.AV_ChequeOrReference = "0003";
			approval2.AV_OH = orgHeader.PK;

			PaymentApprovalWithAuthorisation approval3 = GetNewPaymentApprovalReadyToPost("00001004", "0004", 1000M);
			approval3.AV_Status = PaymentApprovalStatus.Posted;
			approval3.AV_OH = orgHeader.PK;

			PaymentApprovalWithAuthorisation approval4 = GetNewPaymentApprovalReadyToPost("00001005", "0005", 1000M);
			approval4.AV_Status = PaymentApprovalStatus.FullyApproved;
			approval4.AV_PaymentType = ReceiptTypes.eNettCreditCard;
			approval4.AV_AB = bankAccount.PK;
			approval4.AV_ChequeOrReference = "0005";
			approval4.AV_OH = orgHeader.PK;

			Factory.Save();

			approval1 = Factory.Load<PaymentApprovalWithAuthorisation>(approval1.PK);
			approval2 = Factory.Load<PaymentApprovalWithAuthorisation>(approval2.PK);
			approval3 = Factory.Load<PaymentApprovalWithAuthorisation>(approval3.PK);
			approval4 = Factory.Load<PaymentApprovalWithAuthorisation>(approval4.PK);

			BusinessObject[] businessObjects = new BusinessObject[4];
			businessObjects[0] = approval1;
			businessObjects[1] = approval2;
			businessObjects[2] = approval3;
			businessObjects[3] = approval4;

			Assert("Precondition: Approval1 should have no errors. Errors:" + approval1.Notifications.ToUniqueMessageListString(), !approval1.HasErrors);
			Assert("Precondition: Approval1 should have no errors. Errors: ", !approval1.PaymentMatchingBaseObject.HasErrors);

			Assert("Precondition: Approval2 should have no errors. Errors: " + approval2.Notifications.ToUniqueMessageListString(), !approval2.HasErrors);
			Assert("Precondition: Approval2 should have no errors. Errors: ", !approval2.PaymentMatchingBaseObject.HasErrors);

			Assert("Precondition: Approval3 should have no errors. Errors: " + approval3.Notifications.ToUniqueMessageListString(), !approval3.HasErrors);
			Assert("Precondition: Approval3 Matching should have no errors. Errors: ", !approval3.PaymentMatchingBaseObject.HasErrors);

			Assert("Precondition: Approval4 should have no errors. Errors: " + approval4.Notifications.ToUniqueMessageListString(), !approval4.HasErrors);
			Assert("Precondition: Approval4 should have no errors. Errors: ", !approval4.PaymentMatchingBaseObject.HasErrors);

			using (PaymentProcessingModule module = GetNewModule())
			{
				SelectBusinessObjects(module, businessObjects);

				PostCheckPoint.IsAllowed = true;
				eNettWebServiceWrapper.UseRealWebService_ForTesting = false;
				MockENettWebService.Instance.SetupForTesting("CARGOWISE");

				int initialInvokedCount = MockENettWebService.Instance.CountProcessCreditCardWasInvoked;
				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((form) =>
					{
						if (form.GetType() == typeof(CreditCardSecurityCodeForm))
						{
							var securityCodeBizo = (PaymentCreditCardSecurityCode)((ZForm)form).BusinessEntity;
							securityCodeBizo.CardSecurityCode = "333";
							securityCodeBizo.Continue = true;
						}
					});
				module.PostPaymentApprovals(null, new EventArgs());
				AssertEquals("Approval1 Status", PaymentApprovalStatus.AwaitingApproval, approval1.AV_Status);
				AssertEquals("Approval2 Status", PaymentApprovalStatus.Posted, approval2.AV_Status);
				AssertEquals("Approval3 Status", PaymentApprovalStatus.Posted, approval3.AV_Status);
				AssertEquals("Approval4 Status", PaymentApprovalStatus.Posted, approval4.AV_Status);
				AssertNotNull("Approval2 Payment", approval2.TransactionHeader);
				AssertNotNull("Approval4 Payment", approval4.TransactionHeader);
				AssertEquals("There should be 2 payments passed for printing", 2, module.PaymentProcessingGUIHelper.Test_PaymentsPassedForPrinting.Count());
				Assert("Payments passed for printing should contain Approval2's payment", module.PaymentProcessingGUIHelper.Test_PaymentsPassedForPrinting.Any(x => x.PK == approval2.TransactionHeader.PK));
				Assert("Payments passed for printing should contain Approval4's payment", module.PaymentProcessingGUIHelper.Test_PaymentsPassedForPrinting.Any(x => x.PK == approval4.TransactionHeader.PK));
				AssertEquals("ProcessCreditCard should have been invoked twice", 2, MockENettWebService.Instance.CountProcessCreditCardWasInvoked - initialInvokedCount);

				AccountingConfigurationRegistry.Instance.EnableCreditCardPaymentsViaComPay.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, originalEnableCreditCardPaymentsViaComPay);
			}
		}

		public void TestPostPaymentApprovalsWithInvoiceAttachedToOpenClaim()
		{
			var invoiceCreationFactory = new BusinessObjectFactory();
			var testInvoiceCreator = new TestObjectCreator(invoiceCreationFactory);

			var invoice1 = (APInvoice)testInvoiceCreator.CreateInvoiceWithLine(typeof(APInvoice), "00001100", testInvoiceCreator.AUD, 1m, 100m, 0m, 100m, 0m, TestOrgHeader, testInvoiceCreator.CC1.PK);
			testInvoiceCreator.AttachJobToAPLine(invoice1.Lines[0]);
			invoice1.Lines[0].AL_AT = testInvoiceCreator.GSTFREE1.PK;
			testInvoiceCreator.AttachChargeToAPLine(invoice1.Lines[0]);
			invoiceCreationFactory.Save();
			invoice1 = Factory.Load<APInvoice>(invoice1.PK);

			var invoice2 = (APInvoice)testInvoiceCreator.CreateInvoiceWithLine(typeof(APInvoice), "00001101", testInvoiceCreator.AUD, 1m, 200m, 0m, 200m, 0m, TestOrgHeader, testInvoiceCreator.CC1.PK);
			testInvoiceCreator.AttachJobToAPLine(invoice2.Lines[0]);
			invoice2.Lines[0].AL_AT = testInvoiceCreator.GSTFREE1.PK;
			testInvoiceCreator.AttachChargeToAPLine(invoice2.Lines[0]);
			invoiceCreationFactory.Save();
			invoice2 = Factory.Load<APInvoice>(invoice2.PK);

			new AccountingPeriodTestHelper(Factory).SetupPeriods();

			var bankAccount = TestObjectCreator.AUDBankAccount;
			bankAccount.AB_RX_NKAccountCurrency = "AUD";
			var chequeBook = TestObjectCreator.AUDChequeBook;

			var testPaymentApproval = GetNewPaymentApproval();
			testPaymentApproval.AV_OH = ZGuid.Empty;
			testPaymentApproval.AV_OH = TestOrgHeader.PK;
			testPaymentApproval.AV_AB = bankAccount.PK;
			testPaymentApproval.AV_AK = chequeBook.PK;
			testPaymentApproval.ExchangeRate.Currency = "AUD";
			testPaymentApproval.AV_ChequeOrReference = chequeBook.AK_CurrentNo.ToString();
			testPaymentApproval.AV_Amount = 300m;

			testPaymentApproval.PaymentMatchingBaseObject.MoveAllFromUnmatchToMatch();
			testPaymentApproval.PaymentMatchingBaseObject.MatchAndClearTransactions();

			var claim = Factory.New<APAccQueryClaim>();
			claim.AY_OH_Debtor = invoice1.AH_OH;
			claim.AY_OC = Factory.NewWithValidTestData<OrgContact>().PK;
			claim.AY_QueryClaimReference = "ref";
			claim.AY_QueryClaimStatus = QueryClaimStatusCodeList.Codes.QCStatus1Open;
			claim.AY_AH = invoice1.PK;
			Factory.Save();

			BusinessObject[] businessObjects = new BusinessObject[1];
			businessObjects[0] = testPaymentApproval;

			using (PaymentProcessingModule module = GetNewModule())
			{
				SelectBusinessObjects(module, businessObjects);
				PostCheckPoint.IsAllowed = true;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				module.PostPaymentApprovals(null, new EventArgs());
				AssertContains("The following Payments cannot be processed:", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertContains(testPaymentApproval.AV_ChequeOrReference, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertContains("One or more invoices attached to this approval is linked to an open claim", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestPostPaymentApprovalsWithInvoiceAttachedToAllowToMatchOpenClaim()
		{
			var newFactory = new BusinessObjectFactory();
			var newObjectCreator = new TestObjectCreator(newFactory);

			var invoice1 = (APInvoice)newObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "00001100", newObjectCreator.AUD, 1m, 100m, 0m, 100m, 0m, TestOrgHeader, newObjectCreator.CC1.PK);
			newObjectCreator.AttachJobToAPLine(invoice1.Lines[0]);
			invoice1.Lines[0].AL_AT = newObjectCreator.GSTFREE1.PK;
			newObjectCreator.AttachChargeToAPLine(invoice1.Lines[0]);

			newFactory.Save();

			invoice1 = Factory.Load<APInvoice>(invoice1.PK);

			new AccountingPeriodTestHelper(Factory).SetupPeriods();

			var bankAccount = TestObjectCreator.AUDBankAccount;
			bankAccount.AB_RX_NKAccountCurrency = "AUD";
			var chequeBook = TestObjectCreator.AUDChequeBook;

			var paymentApproval = GetNewPaymentApproval();
			paymentApproval.AV_OH = ZGuid.Empty;
			paymentApproval.AV_OH = TestOrgHeader.PK;
			paymentApproval.AV_AB = bankAccount.PK;
			paymentApproval.AV_AK = chequeBook.PK;
			paymentApproval.ExchangeRate.Currency = "AUD";
			paymentApproval.AV_ChequeOrReference = chequeBook.AK_CurrentNo.ToString();
			paymentApproval.AV_Amount = 100m;

			paymentApproval.PaymentMatchingBaseObject.MoveAllFromUnmatchToMatch();
			paymentApproval.PaymentMatchingBaseObject.MatchAndClearTransactions();

			var claim = Factory.New<APAccQueryClaim>();
			claim.AY_OH_Debtor = invoice1.AH_OH;
			claim.AY_OC = Factory.NewWithValidTestData<OrgContact>().PK;
			claim.AY_QueryClaimReference = "ref";
			claim.AY_QueryClaimStatus = QueryClaimStatusCodeList.Codes.QCStatus1Open;
			claim.AY_AH = invoice1.PK;
			claim.AY_HoldOption = HoldOptionType.Codes.ALM;

			Factory.Save();

			using (PaymentProcessingModule module = GetNewModule())
			{
				SelectBusinessObjects(module, new BusinessObject[1] { paymentApproval });
				PostCheckPoint.IsAllowed = true;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				AssertNull("Approval Payment", paymentApproval.TransactionHeader);

				module.PostPaymentApprovals(null, new EventArgs());

				AssertNotNull("Approval Payment", paymentApproval.TransactionHeader);
				var matchedObjects = paymentApproval.MatchingBaseObject.MatchedTransactions.Where(x => x is TransactionHeader && ((TransactionHeader)x).AH_TransactionType == TransactionTypes.Invoice);
				AssertEquals("Matched correct invoice", true, matchedObjects.Count() == 1 && ((TransactionHeader)matchedObjects.First()).PK == invoice1.PK);
				AssertEquals("Hold option allows the match", true, ((TransactionHeader)matchedObjects.First()).OpenQueryClaim.IsAllowMatch);
				AssertContains("No errors", "The following Payments will be processed", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("There should not be 1 payment passed for printing", 1, module.PaymentProcessingGUIHelper.Test_PaymentsPassedForPrinting.Count());
			}
		}

		protected class APPaymentProcessingModuleForTest : APPaymentProcessingModule
		{
			protected override BusinessObject[] SelectedBusinessObjects
			{
				get { return fSelectedBusinessObjects ?? base.SelectedBusinessObjects; }
			}

			BusinessObject[] fSelectedBusinessObjects;

			public void SetfSelectedBusinessObjects(BusinessObject[] newfSelectedBusinessObjects)
			{
				fSelectedBusinessObjects = newfSelectedBusinessObjects;
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			MockENettWebService.ClearInstance();
		}

		protected override void TearDown()
		{
			base.TearDown();
			MockENettWebService.ClearInstance();
		}
	}
}
