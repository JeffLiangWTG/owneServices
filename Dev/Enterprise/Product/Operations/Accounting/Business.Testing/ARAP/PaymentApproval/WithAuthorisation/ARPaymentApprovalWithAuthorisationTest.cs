using System;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.PaymentApproval.Testing
{
	[TestedType(typeof(ARPaymentApprovalWithAuthorisation))]
	public class ARPaymentApprovalWithAuthorisationTest : PaymentApprovalWithAuthorisationTest
	{
		public void TestIEdocsParsingSupportProvider()
		{
			var eDocsParsingSupport = TestPaymentApproval as IEDocsParsingSupport;
			AssertNotNull("IEDocsParsingSupport must be implemented", eDocsParsingSupport);
			Assert(eDocsParsingSupport.DenySendForParsing(new Guid(), "PIN", "testfile.pdf"));
		}

		public override void TestProcessEPaymentSecurity()
		{
			Env.Security.NewReceivablesPaymentProcessEPayment.IsAllowed = false;
			Assert(!Env.Security.NewReceivablesPaymentProcessEPayment.IsAllowed);
			Env.Security.NewReceivablesPaymentProcessEPayment.IsAllowed = true;
			Assert(Env.Security.NewReceivablesPaymentProcessEPayment.IsAllowed);
		}

		public void TestPaymentApprovalSecurity()
		{
			AssertPaymentApprovalSecurityCase(Env.Security.ARPaymentProcessingNewCheque, false, ReceiptTypes.Cheque, false);
			AssertPaymentApprovalSecurityCase(Env.Security.ARPaymentProcessingNewCash, false, ReceiptTypes.Cash, false);
			AssertPaymentApprovalSecurityCase(Env.Security.ARPaymentProcessingNewCreditCard, false, ReceiptTypes.CreditCard, false);
			AssertPaymentApprovalSecurityCase(Env.Security.ARPaymentProcessingNewDirectDebit, false, ReceiptTypes.DirectDebit, false);
			AssertPaymentApprovalSecurityCase(Env.Security.ARPaymentProcessingNewEFT, false, ReceiptTypes.EFT, false);
			AssertPaymentApprovalSecurityCase(Env.Security.ARPaymentProcessingNewSFT, false, ReceiptTypes.ScheduledEFT, false);
			AssertPaymentApprovalSecurityCase(Env.Security.ARPaymentProcessingNewCRQ, false, ReceiptTypes.CollectionRequest, false);

			AssertPaymentApprovalSecurityCase(Env.Security.ARPaymentProcessingNewCheque, true, ReceiptTypes.Cheque, true);
			AssertPaymentApprovalSecurityCase(Env.Security.ARPaymentProcessingNewCash, true, ReceiptTypes.Cash, true);
			AssertPaymentApprovalSecurityCase(Env.Security.ARPaymentProcessingNewCreditCard, true, ReceiptTypes.CreditCard, true);
			AssertPaymentApprovalSecurityCase(Env.Security.ARPaymentProcessingNewDirectDebit, true, ReceiptTypes.DirectDebit, true);
			AssertPaymentApprovalSecurityCase(Env.Security.ARPaymentProcessingNewEFT, true, ReceiptTypes.EFT, true);
			AssertPaymentApprovalSecurityCase(Env.Security.ARPaymentProcessingNewSFT, true, ReceiptTypes.ScheduledEFT, true);
			AssertPaymentApprovalSecurityCase(Env.Security.ARPaymentProcessingNewCRQ, true, ReceiptTypes.CollectionRequest, true);

			AssertPaymentApprovalSecurityCase(Env.Security.ARPaymentProcessingNewDirectDebit, false, ReceiptTypes.DirectCredit, true);
		}

		protected override ZString ExpectedDefaultLedger
		{
			get { return LedgerTypes.AccountsReceivable; }
		}

		protected override SecurityCheckpoint FirstApprovalCheckPoint
		{
			get { return Env.Security.ARPaymentProcessingFirstApproval; }
		}

		protected override SecurityCheckpoint SecondApprovalCheckPoint
		{
			get { return Env.Security.ARPaymentProcessingSecondApproval; }
		}

		protected override SecurityCheckpoint ThirdApprovalCheckPoint
		{
			get { return Env.Security.ARPaymentProcessingThirdApproval; }
		}

		protected override SecurityCheckpoint CancelApprovalCheckPoint
		{
			get { return Env.Security.ARPaymentProcessingCancelApproval; }
		}

		protected override SecurityCheckpoint CancelEPaymentCheckPoint
		{
			get { return Env.Security.ARPaymentProcessingCancelEPayment; }
		}

		protected override (string, string) GetExpectedCreatorNotification(ZGuid approvalPK)
		{
			var expectedSubject = "AR Payment Request Approved - ZOrg AUD 1000.00";
			var expectedBody = $@"<br/><p>The following AR Payment Request was approved:</p>
<p><a href=""edient:Command=ShowViewForm&LicenceCode=EDIEDIDAT&ControllerID=ARPaymentProcessing&BusinessEntityPK={approvalPK}&VersionNumber={new EnterpriseInformationRetriever().VersionNumber}&Hash=%2bMHt%2fmIG%2fGWzG8LrjocCRAo4YNtUsCOq2"">ZOrg - 14-May-20 - AR PAYMENT</a></p>
<p><span style=""font-weight:bold;"">Created on:</span> 14 May 2020 10:16</p>
<p><span style=""font-weight:bold;"">Payment Amount:</span> 1000.00 AUD</p>
<p><span style=""font-weight:bold;"">Organization:</span> ZOrg</p>";

			return (expectedSubject, expectedBody);
		}

		protected override void SetUp()
		{
			base.SetUp();
			TestPaymentApproval = (ARPaymentApprovalWithAuthorisation)Factory.NewWithValidTestData(GetExpectedBusinessObjectType());
		}

		ARPaymentApprovalWithAuthorisation TestPaymentApproval;
	}
}
