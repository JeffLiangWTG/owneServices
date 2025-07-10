using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.Accounting
{
	[TestedType(typeof(DocPaymentApprovalMatchLink))]
	sealed class DocPaymentApprovalMatchLinkTestCase : DocumentWrapperTestCase
	{
		public void TestProperties()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();

			APPaymentApprovalWithAuthorisation approval = factory.NewWithValidTestData<APPaymentApprovalWithAuthorisation>();
			PaymentApprovalItem approvalItem = factory.NewWithValidTestData<PaymentApprovalItem>();
			APInvoice invoice = factory.NewWithValidTestData<APInvoice>();

			approval.AV_Amount = 200M;
			approval.AV_Discount = -300M;
			invoice.AH_TransactionNum = "00001001";
			invoice.AH_OSExTaxAmount = 100M;
			invoice.AH_GB = GlbBranch.CurrentBranch.PK;
			invoice.AH_GE = GlbDepartment.CurrentDepartment.PK;
			approvalItem.A2_PaymentThisRun = 100M;
			approvalItem.A2_AH = invoice.PK;
			approvalItem.A2_AV = approval.PK;

			factory.Save();

			DocPaymentApproval approvalWrapper = DocPaymentApproval.New(approval, factory);

			AssertNotEquals("Precondition: TransactionType", "", approvalWrapper.TransactionType);
			AssertNotEquals("Precondition: OSAmount", 0, approvalWrapper.OSAmount);
			AssertNotEquals("Precondition: Amount", 0, approvalWrapper.Amount);
			AssertNotEquals("Precondition: CreatorName", "", approvalWrapper.PreparedBy);

			AssertEquals("Precondition: CreatorName", 2, approvalWrapper.Payments.Count);

			AssertNotEquals("Precondition: TransactionType", "", approvalWrapper.Payments[0].TransactionType);
			AssertNotEquals("Precondition: OSAmount", 0, approvalWrapper.Payments[0].OSAmount);
			AssertNotEquals("Precondition: Amount", 0, approvalWrapper.Payments[0].Amount);
			AssertNotEquals("Precondition: CreatorName", "", approvalWrapper.Payments[0].PreparedBy);

			AssertNotEquals("Precondition: TransactionType", "", approvalWrapper.Payments[1].TransactionType);
			AssertNotEquals("Precondition: OSAmount", 0, approvalWrapper.Payments[1].OSAmount);
			AssertNotEquals("Precondition: Amount", 0, approvalWrapper.Payments[1].Amount);
			AssertNotEquals("Precondition: CreatorName", "", approvalWrapper.Payments[1].PreparedBy);

			AssertEquals("TransactionType", approvalWrapper.TransactionType, approvalWrapper.MatchLink.TransactionType);
			AssertEquals("OSAmount", approvalWrapper.OSAmount, approvalWrapper.MatchLink.OSAmount);
			AssertEquals("Amount", approvalWrapper.Amount, approvalWrapper.MatchLink.Amount);
			AssertEquals("MatchAmount", approvalWrapper.Amount, approvalWrapper.MatchLink.MatchAmount);
			AssertEquals("InvertedOSAmount", approvalWrapper.OSAmount, approvalWrapper.MatchLink.InvertedOSAmount);
			AssertEquals("CreatorName", approvalWrapper.PreparedBy, approvalWrapper.MatchLink.CreatorName);

			AssertEquals("TransactionType", approvalWrapper.Payments[0].TransactionType, approvalWrapper.Payments[0].MatchLink.TransactionType);
			AssertEquals("OSAmount", approvalWrapper.Payments[0].OSAmount, approvalWrapper.Payments[0].MatchLink.OSAmount);
			AssertEquals("Amount", -approvalWrapper.Payments[0].Amount, approvalWrapper.Payments[0].MatchLink.Amount);
			AssertEquals("MatchAmount", approvalWrapper.Payments[0].Amount, approvalWrapper.Payments[0].MatchLink.MatchAmount);
			AssertEquals("InvertedOSAmount", -approvalWrapper.Payments[0].OSAmount, approvalWrapper.Payments[0].MatchLink.InvertedOSAmount);
			AssertEquals("CreatorName", approvalWrapper.Payments[0].PreparedBy, approvalWrapper.Payments[0].MatchLink.CreatorName);

			AssertEquals("TransactionType", approvalWrapper.Payments[1].TransactionType, approvalWrapper.Payments[1].MatchLink.TransactionType);
			AssertEquals("OSAmount", approvalWrapper.Payments[1].OSAmount, approvalWrapper.Payments[1].MatchLink.OSAmount);
			AssertEquals("Amount", -approvalWrapper.Payments[1].Amount, approvalWrapper.Payments[1].MatchLink.Amount);
			AssertEquals("MatchAmount", approvalWrapper.Payments[1].Amount, approvalWrapper.Payments[1].MatchLink.MatchAmount);
			AssertEquals("InvertedOSAmount", -approvalWrapper.Payments[1].OSAmount, approvalWrapper.Payments[1].MatchLink.InvertedOSAmount);
			AssertEquals("CreatorName", approvalWrapper.Payments[1].PreparedBy, approvalWrapper.Payments[1].MatchLink.CreatorName);
		}

		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[]
			{
				DocPaymentApprovalMatchLink.New(PaymentApprovalWrapper, Factory)
			};
		}

		protected override void SetUp()
		{
			APPaymentApprovalWithAuthorisation approval = Factory.New<APPaymentApprovalWithAuthorisation>();
			PaymentApprovalWrapper = DocPaymentApproval.New(approval, Factory);
			base.SetUp();
		}

		DocPaymentApproval PaymentApprovalWrapper;
	}
}
