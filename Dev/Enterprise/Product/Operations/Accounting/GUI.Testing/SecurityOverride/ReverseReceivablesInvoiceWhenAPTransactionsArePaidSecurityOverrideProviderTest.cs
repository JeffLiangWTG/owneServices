using System.Windows.Forms;
using Enterprise.Accounting.Business.ARAP;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Reversing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.Testing
{
	public class ReverseReceivablesInvoiceWhenAPTransactionsArePaidSecurityOverrideProviderTest : InvoicingSecurityOverrideProviderTest
	{
		protected override InvoicingSecurityOverrideProvider GetSecurityProvider(bool showApprovalRequestButton = false, bool alwaysCreateApprovalRequest = false, bool keepLoginFormResultAfterFirstUserAnswer = false)
		{
			return new ReverseReceivablesInvoiceWhenAPTransactionsArePaidSecurityOverrideProvider(Factory.NewWithValidTestData<ARInvoice>());
		}

		protected override bool ShouldPromptForGranted
		{
			get { return true; }
		}

		[ExpectNoExceptions]
		public override void TestGetClosedJobs()
		{
			var job1 = Factory.NewJobWithValidTestDataForTesting<Job>();
			job1.JH_Status = JobHeaderStatus.Closed.Code;
			job1.JH_JobNum = "TESTJOB1";
			var job2 = Factory.NewJobWithValidTestDataForTesting<Job>();
			job2.JH_Status = JobHeaderStatus.Closed.Code;
			job2.JH_JobNum = "TESTJOB2";
			var job3 = Factory.NewJobWithValidTestDataForTesting<Job>();
			job3.JH_Status = JobHeaderStatus.Working.Code;
			job3.JH_JobNum = "TESTJOB3";

			var invoice1 = Factory.NewWithValidTestData<ARInvoice>();
			invoice1.AddRelatedJobsForReversing_ForTestOnly(job1);
			var wrapper1 = new IReversingImplicitlyImplementedWrapperForBinding(invoice1);

			var invoice2 = Factory.NewWithValidTestData<ARInvoice>();
			invoice2.AddRelatedJobsForReversing_ForTestOnly(job2);
			invoice2.AddRelatedJobsForReversing_ForTestOnly(job3);
			var wrapper2 = new IReversingImplicitlyImplementedWrapperForBinding(invoice2);

			var discount = Factory.NewWithValidTestData<APDiscount>();
			var wrapper3 = new IReversingImplicitlyImplementedWrapperForBinding(discount);

			var reversingProvider = new MultipleReversingProviderForHeader();
			reversingProvider.TransactionsAlreadyReversed.AddRange(wrapper1, wrapper2, wrapper3);
			var testProvider = new ReverseReceivablesInvoiceWhenAPTransactionsArePaidSecurityOverrideProvider(reversingProvider);
			AssertEquals("TESTJOB1, TESTJOB2", testProvider.GetClosedJobs_ForTestOnly());
		}

		public void TestGetClosedJobsForSingleInvoice()
		{
			var job1 = Factory.NewJobWithValidTestDataForTesting<Job>();
			job1.JH_Status = JobHeaderStatus.Closed.Code;
			job1.JH_JobNum = "TESTJOB1";
			var job2 = Factory.NewJobWithValidTestDataForTesting<Job>();
			job2.JH_Status = JobHeaderStatus.Working.Code;
			job2.JH_JobNum = "TESTJOB2";

			var invoice1 = Factory.NewWithValidTestData<ARInvoice>();
			invoice1.AddRelatedJobsForReversing_ForTestOnly(job1);
			invoice1.AddRelatedJobsForReversing_ForTestOnly(job2);
			var wrapper1 = new IReversingImplicitlyImplementedWrapperForBinding(invoice1);

			var reversingProvider = new MultipleReversingProviderForHeader();
			reversingProvider.TransactionsAlreadyReversed.Add(wrapper1);
			var testProvider = new ReverseReceivablesInvoiceWhenAPTransactionsArePaidSecurityOverrideProvider(invoice1);
			AssertEquals("TESTJOB1", testProvider.GetClosedJobs_ForTestOnly());
		}

		public void TestLoginPromtIsShownOnceWhenBothTypeOfInvoiceSecurityIsAllowed()
		{
			var testUser = Factory.NewWithValidTestData<GlbStaff>();
			testUser.GS_LoginName = "User1";
			testUser.StaffPlainTextPassword = "pass";
			testUser.GS_ChangePasswordAtNextLogin = false;

			var invoiceSecurity = Factory.New<GlbSecurity>();
			invoiceSecurity.GU_GS = testUser.PK;
			invoiceSecurity.GU_SecurityRight = Env.Security.ReverseReceivablesInvoiceWhenAPTransactionsArePaid.Code;
			invoiceSecurity.GU_SecurityItemIsAllowed = true;

			var selfBilledInvoiceSecurity = Factory.New<GlbSecurity>();
			selfBilledInvoiceSecurity.GU_GS = testUser.PK;
			selfBilledInvoiceSecurity.GU_SecurityRight = Env.Security.ReverseARSelfBilledInvoiceWhenAPArePaid.Code;
			selfBilledInvoiceSecurity.GU_SecurityItemIsAllowed = true;
			Factory.Save();

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;
			ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs(form =>
			{
				((LoginForm)form).DoLoginForTest(testUser.GS_LoginName, testUser.StaffPlainTextPassword);
				ZFormModaliser.ResultToReturnFromShowDialog = ZFormModaliser.ResultToReturnFromShowDialog != DialogResult.OK ? DialogResult.OK : DialogResult.Cancel;
			});

			var reversingProvider = new MultipleReversingProviderForHeader();
			var testProvider = new ReverseReceivablesInvoiceWhenAPTransactionsArePaidSecurityOverrideProvider(reversingProvider);
			var security = ((ISecurityOverrideProvider)testProvider).PromptForTemporaryAccess(Env.Security.ReverseReceivablesInvoiceWhenAPTransactionsArePaid);
			AssertEquals("The Login Form is shown", DialogResult.OK, ZFormModaliser.ResultToReturnFromShowDialog);
			AssertNotNull("Found security certificate", security);
			Assert("User is allowed", security.IsAllowed);

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;
			var securityForSlefBilling = ((ISecurityOverrideProvider)testProvider).PromptForTemporaryAccess(Env.Security.ReverseARSelfBilledInvoiceWhenAPArePaid);
			AssertEquals("The Login Form is not shown", DialogResult.Cancel, ZFormModaliser.ResultToReturnFromShowDialog);
			AssertNotNull("Found security certificate", securityForSlefBilling);
			Assert("User is allowed", securityForSlefBilling.IsAllowed);

			selfBilledInvoiceSecurity.GU_SecurityItemIsAllowed = false;
			Factory.Save();

			securityForSlefBilling = null;
			testProvider.UserSecurityOverride.SecurityInstance.CheckPointLookUpTable_ForTest.Clear();
			securityForSlefBilling = ((ISecurityOverrideProvider)testProvider).PromptForTemporaryAccess(Env.Security.ReverseARSelfBilledInvoiceWhenAPArePaid);
			AssertEquals("The Login Form is shown", DialogResult.OK, ZFormModaliser.ResultToReturnFromShowDialog);
			AssertNotNull("Found security certificate", securityForSlefBilling);
			Assert("User is not allowed", !securityForSlefBilling.IsAllowed);
		}
	}
}
