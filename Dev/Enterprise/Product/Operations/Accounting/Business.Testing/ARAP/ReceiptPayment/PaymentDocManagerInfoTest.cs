using Enterprise.Accounting.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.ReceiptPayment.Testing
{
	[TestedType(typeof(Payment.PaymentDocManagerInfo))]
	public class PaymentDocManagerInfoTest : AccountingDocManagerInfoTest
	{
		public void TestRelatedObjectsContainPaymentApproval()
		{
			AssertNotNull(ARPayment.DocManagerInfo);
			AssertEquals("AR Payment doc should have 1 related object", 1, ARPayment.DocManagerInfo.RelatedObjects.Length);
			AssertEquals("AR Payment doc should have AR Payment approval as related object", ARPaymentApproval.PK, ARPayment.DocManagerInfo.RelatedObjects[0].PK);

			AssertNotNull(APPayment.DocManagerInfo);
			AssertEquals("AP Payment doc should have 1 related object", 1, APPayment.DocManagerInfo.RelatedObjects.Length);
			AssertEquals("AP Payment doc should have AP Payment approval as related object", APPaymentApproval.PK, APPayment.DocManagerInfo.RelatedObjects[0].PK);
		}

		protected override void SetUp()
		{
			base.SetUp();

			ARPayment = Factory.New<ARPayment>();
			AssertNotNull("ARPayment should not be null", ARPayment);
			ARPaymentApproval = Factory.New<AccPaymentApproval>();
			ARPaymentApproval.AV_Ledger = LedgerTypes.AccountsReceivable;
			ARPaymentApproval.AV_AH = ARPayment.PK;
			AssertNotNull("ARPaymentApproval should not be null", ARPaymentApproval);

			APPayment = Factory.New<APPayment>();
			AssertNotNull("ARPayment should not be null", APPayment);
			APPaymentApproval = Factory.New<AccPaymentApproval>();
			APPaymentApproval.AV_Ledger = LedgerTypes.AccountsPayable;
			APPaymentApproval.AV_AH = APPayment.PK;
			AssertNotNull("APPaymentApproval should not be null", APPaymentApproval);
		}

		AccPaymentApproval ARPaymentApproval;
		ARPayment ARPayment;
		AccPaymentApproval APPaymentApproval;
		APPayment APPayment;
	}
}
