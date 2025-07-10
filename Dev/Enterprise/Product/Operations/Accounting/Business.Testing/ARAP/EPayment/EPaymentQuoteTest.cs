using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.Accounting.Business.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.EPayment.Testing
{
	[TestedType(typeof(EPaymentQuote))]
	public class EPaymentQuoteTest : AccEPaymentQuoteTest
	{
		public void TestQuoteReferenceIsSetOnSaving()
		{
			SetupDataForTest();
			var paymentApproval = GetNewTestAPPaymentApproval(TestObjectCreator.AALSHI, TestBank, ZArchitecture.Core.ReceiptTypes.Cheque, TestCheques, "2", 200m);
			paymentApproval.AV_RX_NKPaymentCurrency = "USD";
			Factory.Save();

			var company1 = TestObjectCreator.CreateNewCompany("CC1");
			var branch1 = TestObjectCreator.CreateBranch("BB1", company1);
			var branch2 = TestObjectCreator.CreateBranch("BB2", company1);

			var company2 = TestObjectCreator.CreateNewCompany("CC2");
			var branch3 = TestObjectCreator.CreateBranch("BB3", company2);

			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch1.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var approval1 = CreateEPaymentQuote(paymentApproval);
				var approval2 = CreateEPaymentQuote(paymentApproval);
				Factory.Save();
				AssertContainsExactElementsInAnyOrder(new[] { "00001000", "00001001" }, new[] { approval1, approval2 }.Select(x => x.QU_InternalReference));
			}

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch2.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var approval3 = CreateEPaymentQuote(paymentApproval);
				var approval4 = CreateEPaymentQuote(paymentApproval);
				Factory.Save();
				AssertContainsExactElementsInAnyOrder(new[] { "00001002", "00001003" }, new[] { approval3, approval4 }.Select(x => x.QU_InternalReference));
			}

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch3.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var approval5 = CreateEPaymentQuote(paymentApproval);
				var approval6 = CreateEPaymentQuote(paymentApproval);
				Factory.Save();
				AssertContainsExactElementsInAnyOrder(new[] { "00001000", "00001001" }, new[] { approval5, approval6 }.Select(x => x.QU_InternalReference));
			}
		}

		EPaymentQuote CreateEPaymentQuote(PaymentApprovalWithAuthorisation paymentApproval)
		{
			var quote = Factory.New(GetExpectedBusinessObjectType()) as EPaymentQuote;
			quote.QU_AV = paymentApproval.PK;
			quote.QU_ProviderCode = "OFX";
			quote.QU_ToAmount = paymentApproval.AV_Amount;
			quote.QU_RX_NKToCurrency = paymentApproval.AV_RX_NKPaymentCurrency;
			quote.QU_RX_NKFromCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			quote.QU_GC = GlbCompany.CurrentCompany.PK;
			return quote;
		}

		PaymentApprovalWithAuthorisation GetNewTestAPPaymentApproval(OrgHeader org, AccBankAccount bank, ZString receiptType,
			AccChequeBook chequeBook, ZString chequeOrRef, ZDecimal amount)
		{
			APPaymentApprovalWithAuthorisation paymentApproval = Factory.New<APPaymentApprovalWithAuthorisation>();
			paymentApproval.AV_OH = org.PK;
			paymentApproval.AV_AB = bank.PK;
			paymentApproval.AV_PaymentType = receiptType;
			paymentApproval.AV_AK = chequeBook.PK;
			paymentApproval.AV_ChequeOrReference = chequeOrRef;
			paymentApproval.AV_Amount = amount;
			return paymentApproval;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.NewWithValidTestData<EPaymentQuote>();
		}

		AccountingPeriodTestHelper PeriodHelper;
		AccBankAccount TestBank;
		AccChequeBook TestCheques;
		OrgHeader TestOrg1;

		void SetupDataForTest()
		{
			PeriodHelper = new AccountingPeriodTestHelper(Factory);
			PeriodHelper.SetupPeriods();

			TestBank = Factory.NewWithValidTestData<AccBankAccount>();
			TestCheques = Factory.NewWithValidTestData<AccChequeBook>();
			TestCheques.AK_StartNo = 1;
			TestCheques.AK_LastNo = 100;
			TestCheques.AK_CurrentNo = 1;
			TestCheques.AK_AB = TestBank.PK;

			TestOrg1 = Factory.NewWithValidTestData<OrgHeader>();
			TestOrg1.CompanyData.OB_IsDebtor = true;
			TestOrg1.CompanyData.OB_IsCreditor = true;

			Factory.Save();
		}

		protected TestObjectCreator TestObjectCreator
		{
			get
			{
				if (fTestObjectCreator == null)
				{
					fTestObjectCreator = new TestObjectCreator(Factory);
				}

				return fTestObjectCreator;
			}
		}
		protected TestObjectCreator fTestObjectCreator;
	}
}
