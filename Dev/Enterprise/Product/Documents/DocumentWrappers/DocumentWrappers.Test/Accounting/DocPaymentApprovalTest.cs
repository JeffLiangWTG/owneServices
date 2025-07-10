using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers
{
	[TestedType(typeof(DocPaymentApproval))]
	sealed class DocPaymentApprovalTest : DocumentWrapperTestCase
	{
		public void TestReceiptTypeDescription()
		{
			Approval.AV_PaymentType = ZArchitecture.Core.ReceiptTypes.Cash;
			AssertEquals("Cash", ApprovalWrapper.ReceiptTypeDescription);

			Approval.AV_PaymentType = ZArchitecture.Core.ReceiptTypes.Cheque;
			AssertEquals("Cheque", ApprovalWrapper.ReceiptTypeDescription);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				Approval.AV_PaymentType = ZArchitecture.Core.ReceiptTypes.Cheque;
				AssertEquals("Check", ApprovalWrapper.ReceiptTypeDescription);
			}

			Approval.AV_PaymentType = ZArchitecture.Core.ReceiptTypes.CreditCard;
			AssertEquals("Credit Card", ApprovalWrapper.ReceiptTypeDescription);

			Approval.AV_PaymentType = ZArchitecture.Core.ReceiptTypes.DirectCredit;
			AssertEquals("Direct Credit", ApprovalWrapper.ReceiptTypeDescription);

			Approval.AV_PaymentType = ZArchitecture.Core.ReceiptTypes.DirectDebit;
			AssertEquals("Direct Debit", ApprovalWrapper.ReceiptTypeDescription);
		}

		public void TestReceiptType()
		{
			Approval.AV_PaymentType = ZArchitecture.Core.ReceiptTypes.CreditCard;
			AssertEquals("ReceiptType", Approval.AV_PaymentType, ApprovalWrapper.ReceiptType);
			AssertEquals("ReceiptType", Approval.AV_PaymentType, ApprovalWrapper.PaymentType);
		}

		public void TestChequeOrReference()
		{
			Approval.AV_ChequeOrReference = "00001001";
			AssertEquals("ChequeOrReference", Approval.AV_ChequeOrReference, ApprovalWrapper.ChequeOrReference);
		}

		public void TestOrganisation()
		{
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			OrgHeader testOrgHeader = newFactory.New<OrgHeader>();
			testOrgHeader.CompanyData.OB_IsCreditor = true;
			testOrgHeader.OH_Code = "ABCORG";
			newFactory.Save();

			Approval.AV_OH = testOrgHeader.PK;
			AssertEquals("Organisation", Approval.Header.OH_Code, ApprovalWrapper.Organisation.Code);
		}

		public void TestBankAccount()
		{
			AccBankAccount bankAccount = Factory.New<AccBankAccount>();
			bankAccount.AB_Code = "ABCBANK";

			Approval.AV_AB = bankAccount.PK;
			AssertEquals("BankAccount", Approval.BankAccount.AB_Code, ApprovalWrapper.BankAccount.Code);
		}

		public void TestTransactionType()
		{
			AssertEquals("TransactionType", "UNA", ApprovalWrapper.TransactionType);
		}

		public void TestInvoiceAndPaymentDate()
		{
			Approval.AV_PaymentDate = new ZDateTime(2006, 01, 01);
			Approval.AV_PostDate = new ZDateTime(2006, 01, 02);

			AssertEquals("InvoiceDate", Approval.AV_PaymentDate, ApprovalWrapper.InvoiceDate);
		}

		public void TestTransactionNumber()
		{
			APPayment payment = Factory.New<APPayment>();
			payment.AH_TransactionNum = "00001001";
			Approval.AV_AH = payment.PK;
			AssertEquals("TransactionNum", ZString.Empty, ApprovalWrapper.TransactionNumber);
		}

		public void TestDesc()
		{
			Approval.AV_PaymentComment = "Payment Comment";
			AssertEquals("Desc", Approval.AV_PaymentComment, ApprovalWrapper.Desc);
		}

		public void TestAmounts()
		{
			Approval.AV_Amount = 100M;
			Approval.AV_PayExRate = 0.5M;

			AssertEquals("OSAmount", Approval.AV_Amount, ApprovalWrapper.OSAmount);
			AssertEquals("OSTotalForRemittanceAdvice", Approval.AV_Amount, ApprovalWrapper.OSTotalForRemittanceAdvice);
			AssertEquals("Currency", Approval.AV_RX_NKPaymentCurrency, ApprovalWrapper.Currency.Code);
			AssertEquals("LocalAmount", Approval.AV_Calc_LocalAmount, ApprovalWrapper.Amount);
			AssertEquals("InvoiceAmountForPaymentVoucher", Approval.AV_Calc_LocalAmount, ApprovalWrapper.InvoiceAmountForPaymentVoucher);
			AssertEquals("RemittanceExchangeRate", Approval.AV_PayExRate, ApprovalWrapper.RemittanceExchangeRate);
		}

		public void TestExchangeRate()
		{
			Approval.AV_PayExRate = 123.456m;
			AssertEquals("Exchange Rate", 123.456m, ApprovalWrapper.ExchangeRate);
		}

		public void TestPaymentCurrency()
		{
			RefCurrency currency = Factory.NewWithValidTestData<RefCurrency>();
			currency.RX_Code = "ABC";
			Approval.AV_RX_NKPaymentCurrency = currency.RX_Code;
			AssertEquals("Currency", Approval.AV_RX_NKPaymentCurrency, ApprovalWrapper.Currency.Code);
		}

		public void TestApprovalStatus()
		{
			AssertEquals("Approval Status", Approval.AV_Calc_DescriptionOfAuthorisationRequired, ApprovalWrapper.ApprovalStatus);
		}

		public void TestAuthorisationDescription()
		{
			AssertEquals("FirstAuthorisationDescription", "First Authorization (" + Approval.Level1AuthorisationStatus + ")", ApprovalWrapper.FirstAuthorisationDescription);
			AssertEquals("SecondAuthorisationDescription", "Second Authorization (" + Approval.Level2AuthorisationStatus + ")", ApprovalWrapper.SecondAuthorisationDescription);
			AssertEquals("ThirdAuthorisationDescription", "Third Authorization (" + Approval.Level3AuthorisationStatus + ")", ApprovalWrapper.ThirdAuthorisationDescription);

			Approval.AV_GS_NKApproval1st = GlbStaff.CurrentUser.GS_Code;
			Approval.AV_GS_NKApproval2nd = GlbStaff.CurrentUser.GS_Code;
			Approval.AV_GS_NKApproval3rd = GlbStaff.CurrentUser.GS_Code;

			AssertEquals("FirstAuthorisation", Approval.Approval1st.GS_FullName, ApprovalWrapper.FirstAuthorisation);
			AssertEquals("SecondAuthorisation", Approval.Approval2nd.GS_FullName, ApprovalWrapper.SecondAuthorisation);
			AssertEquals("ThirdAuthorisation", Approval.Approval3rd.GS_FullName, ApprovalWrapper.ThirdAuthorisation);
		}

		public void TestShowNewAuthorisationFooter()
		{
			AssertEquals(true, ApprovalWrapper.ShowNewAuthorisationFooter);
		}

		public void TestCurrentCompanyReciprocal()
		{
			var header = DocGenericTransactionHeader.New(Approval, Factory);
			AssertEquals("payment approval's exchange rate decimal should always be 6M", 6M, header.CurrentCompanyReciprocal);
		}

		#region TestBarcode

		public void TestBarcode()
		{
			AssertEquals("", ApprovalWrapper.Barcode);
		}

		#endregion

		public override DocumentWrapper[] GetDocumentWrappers()
		{
			PaymentApprovalWithAuthorisation approval = Factory.New<APPaymentApprovalWithAuthorisation>();
			return new DocumentWrapper[] { DocPaymentApproval.New(approval, Factory) };
		}

		protected override void SetUp()
		{
			Approval = Factory.New<APPaymentApprovalWithAuthorisation>();
			ApprovalWrapper = DocPaymentApproval.New(Approval, Factory);
			base.SetUp();
		}

		PaymentApprovalWithAuthorisation Approval;
		DocPaymentApproval ApprovalWrapper;
	}
}
