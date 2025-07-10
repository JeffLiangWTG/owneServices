using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.ClientSharedComponents.DocWrappers.Testing;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.ATL.DocWrappers.Testing
{
	[TestedType(typeof(DocATLAPPayment))]
	public class DocATLAPPaymentTest : DocumentWrapperTestCase
	{
		public void TestChequeAmountAsString()
		{
			payment.AH_OSTotalAmount = 1111.11m;
			AssertEquals("blah", "1,111.11", wrapper.ChequeAmountAsString);
		}

		public void TestRemittanceAdviceHeading()
		{
			AssertContains("This test is stupid, but i have to put here because Doooris told me so", "DATE       CHEQUE     REFERENCE       DETAILS               TRANSACTION AMOUNTS", wrapper.RemittanceAdviceDetailsHeadings);
		}

		public void TestRemittanceAdviceDetails()
		{
			AssertEquals("Remittance Advice Details", ZString.Empty, wrapper.RemittanceAdviceDetails);
			testHelper.SetupInvoicesForPayment(payment, 22, testDate, groupName);
			Factory.Save();
			wrapper = DocATLAPPayment.New(payment, Factory);
			AssertContains("Actual Remittance Advice Details:" + System.Environment.NewLine + wrapper.RemittanceAdviceDetails, "02-Nov-07             INV inv10       AP INVOICE           AUD            10.00", wrapper.RemittanceAdviceDetails);
			var payment2 = Factory.NewWithValidTestData<APPayment>();
			payment2.AH_TransactionNum = "pay2";
			payment2.AH_GE = GlbDepartment.CurrentDepartment.PK;
			payment2.AH_GB = GlbBranch.CurrentBranch.PK;
			groupName = "group2";
			testHelper.SetupInvoicesForPayment(payment2, 22, testDate, groupName, "invForSecondPay");
			testHelper.AddMatchedInvoiceToPayment(payment2, "invForSecondPay23", 10M, testDate, groupName);
			payment2.AH_OSTotal += 10M;
			payment2.AH_InvoiceAmount += 10M;
			payment2.AH_OutstandingAmount = 10M;
			payment2.AH_FullyPaidDate = ZDateTime.Empty;
			Factory.Save();
			wrapper = DocATLAPPayment.New(payment2, Factory);
			AssertEquals("Remittance Advice Details", ZString.Empty, wrapper.RemittanceAdviceDetails);
		}

		#region SetUp && Overrides
		APPayment payment;
		DocATLAPPayment wrapper;
		DocAPPaymentSharedTestHelper testHelper;
		ZDateTime testDate;
		ZString groupName;
		protected override void SetUp()
		{
			testDate = new ZDateTime("02/11/07");
			groupName = "group1";
			testHelper = new DocAPPaymentSharedTestHelper(Factory);
			payment = Factory.NewWithValidTestData<APPayment>();
			payment.AH_TransactionNum = "pay1";
			payment.AH_GE = GlbDepartment.CurrentDepartment.PK;
			payment.AH_GB = GlbBranch.CurrentBranch.PK;
			wrapper = DocATLAPPayment.New(payment, Factory);
			base.SetUp();
		}

		#region Overrides
		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[] { DocATLAPPayment.New(payment, Factory) };
		}
		#endregion
		#endregion
	}
}
