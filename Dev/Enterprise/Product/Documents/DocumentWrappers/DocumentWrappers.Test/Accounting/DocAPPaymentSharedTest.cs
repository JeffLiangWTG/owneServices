using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentWrappers.Testing.Accounting
{
	sealed class DocAPPaymentSharedTest : TestCaseWithFactory
	{
		public void TestRemittanceAdviceHeading()
		{
			AssertContains("Testing Heading", "DATE       CHEQUE     REFERENCE       DETAILS               TRANSACTION AMOUNTS", wrapper.RemittanceAdviceDetailsHeadings);
		}

		public void TestRemittanceAdviceDetails()
		{
			AssertEquals("Remittance Advice Details", ZString.Empty, wrapper.RemittanceAdviceDetails);

			helper.SetupInvoicesForPayment(payment, 22, testDate, groupName);
			Factory.Save();
			wrapper = DocAPPayment.New(payment, Factory);
			AssertContains("Remittance Advice Details", "02-Nov-07             INV inv1        AP INVOICE                       1.00 ERN", wrapper.RemittanceAdviceDetails);

			var payment2 = Factory.NewWithValidTestData<APPayment>();
			payment2.AH_TransactionNum = "pay2";
			payment2.AH_GE = GlbDepartment.CurrentDepartment.PK;
			payment2.AH_GB = GlbBranch.CurrentBranch.PK;
			groupName = "group2";
			helper.SetupInvoicesForPayment(payment2, 22, testDate, groupName, "invForSecondPay");
			helper.AddMatchedInvoiceToPayment(payment2, "invForSecondPay23", 10M, testDate, groupName);
			helper.CreateMatchLink(payment2, 10M, groupName);
			Factory.Save();
			wrapper = DocAPPayment.New(payment2, Factory);
			AssertEquals("Remittance Advice Details", ZString.Empty, wrapper.RemittanceAdviceDetails);
		}

		public void TestShowTransactionLines()
		{
			AssertEquals("ShowTransactionLines", ZBool.True, wrapper.ShowTransactionLines);

			helper.SetupInvoicesForPayment(payment, 22, testDate, groupName);
			wrapper = DocAPPayment.New(payment, Factory);
			AssertEquals("ShowTransactionLines", ZBool.True, wrapper.ShowTransactionLines);

			helper.AddMatchedInvoiceToPayment(payment, "inv23", 10M, testDate, groupName);
			helper.CreateMatchLink(payment, 10M, groupName);
			Factory.Save();
			wrapper = DocAPPayment.New(payment, Factory);
			AssertEquals("ShowTransactionLines", ZBool.False, wrapper.ShowTransactionLines);
		}

		#region SetUp && Overrides
		APPayment payment;
		DocAPPayment wrapper;
		DocAPPaymentTestHelper helper;
		ZDateTime testDate;
		ZString groupName;

		protected override void SetUp()
		{
			Factory.GetDocWrapperContextManager().UpdateDocWrapperContextFromReportConstants(null);
			testDate = new ZDateTime("02/11/07");
			groupName = "group1";
			helper = new DocAPPaymentTestHelper(Factory);
			payment = Factory.NewWithValidTestData<APPayment>();
			payment.AH_TransactionNum = "pay1";
			payment.AH_GE = GlbDepartment.CurrentDepartment.PK;
			payment.AH_GB = GlbBranch.CurrentBranch.PK;
			wrapper = DocAPPayment.New(payment, Factory);
			base.SetUp();
		}

		#endregion
	}
}
