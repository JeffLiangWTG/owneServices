using System.Data;
using System.Linq;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.PaymentApproval.Testing
{
	[TestedType(typeof(AccPaymentBatch))]
	public abstract class AccPaymentBatchTest : EnterpriseBusinessObjectTestCase
	{
		public void TestFountain()
		{
			var batch1 = GetFirstCreatedAccPaymentBatch();
			batch1.APB_AB = TestObjectCreator.AUDBankAccount.PK;

			var batch2 = Factory.New<AccPaymentBatch>();
			batch2.APB_AB = TestObjectCreator.AUDBankAccount.PK;

			var batch3 = Factory.New<AccPaymentBatch>();
			batch3.APB_AB = TestObjectCreator.AUDBankAccount.PK;
			Factory.Save();

			AssertContainsExactElementsInAnyOrder(new[] { "00001000", "00001001", "00001002" }, new[] { batch1, batch2, batch3 }.Select(x => x.APB_BatchNumber));

			var batch4 = Factory.New<AccPaymentBatch>();
			batch4.APB_AB = TestObjectCreator.AUDBankAccount.PK;

			var batch5 = Factory.New<AccPaymentBatch>();
			batch5.APB_AB = TestObjectCreator.AUDBankAccount.PK;

			var batch6 = Factory.New<AccPaymentBatch>();
			batch6.APB_AB = TestObjectCreator.AUDBankAccount.PK;

			Factory.Save();

			AssertContainsExactElementsInAnyOrder(new[] { "00001003", "00001004", "00001005" }, new[] { batch4, batch5, batch6 }.Select(x => x.APB_BatchNumber));
		}

		public void TestABP_Payment()
		{
			var accPaymentBatch = Factory.NewWithValidTestData<AccPaymentBatch>();
			accPaymentBatch.APB_PaymentType = "TRN";
			AssertEquals("TRN", accPaymentBatch.APB_PaymentType);

			accPaymentBatch.APB_PaymentType = ZString.Empty;
			AssertEquals(ZString.Empty, accPaymentBatch.APB_PaymentType);
		}

		public void TestDefaultValue()
		{
			var batch = Factory.New<AccPaymentBatch>();

			CombineAssertions("Default value asserting", () => {
				AssertEquals(nameof(AccPaymentBatch.APB_GC), GlbCompany.CurrentCompany.PK, batch.APB_GC);
				AssertEquals(nameof(AccPaymentBatch.APB_GB), GlbBranch.CurrentBranch.PK, batch.APB_GB);
				AssertEquals(nameof(AccPaymentBatch.APB_PaymentDate), ZDateTime.Today, batch.APB_PaymentDate);
				AssertEquals(nameof(AccPaymentBatch.APB_PostDate), ZDateTime.Today, batch.APB_PostDate);
				AssertEquals(nameof(AccPaymentBatch.APB_Status), Core.Constants.AccPaymentBatchStatus.Working, batch.APB_Status);
				AssertEquals(nameof(AccPaymentBatch.APB_PaymentType), AccountingConfigurationRegistry.Instance.DefaultPaymentType.Value, batch.APB_PaymentType);
			});
		}

		public void TestLocalAmountTotal()
		{
			var accPaymentBatch = Factory.NewWithValidTestData<AccPaymentBatch>();

			var approval1 = Factory.NewWithValidTestData<APPaymentApprovalWithoutAuthorisation>();
			approval1.AV_APB_PaymentBatch = accPaymentBatch.PK;
			approval1.AV_Amount = 10m;
			approval1.AV_PayExRate = 0.5;

			var approval2 = Factory.NewWithValidTestData<APPaymentApprovalWithoutAuthorisation>();
			approval2.AV_APB_PaymentBatch = accPaymentBatch.PK;
			approval2.AV_Amount = 20m;
			approval2.AV_PayExRate = 0.2m;

			accPaymentBatch.ClearPaymentApprovalCollection_ForTestOnly();

			AssertEquals(120m, accPaymentBatch.LocalAmountTotal);
		}

		public void TestUpdateBatchStatusToCompletedWhenAllApprovalsHavePostedStatus()
		{
			var approval1 = Factory.NewWithValidTestData<APPaymentApprovalWithoutAuthorisation>();
			approval1.AV_Status = PaymentApprovalStatus.FullyApproved;

			var approval2 = Factory.NewWithValidTestData<APPaymentApprovalWithoutAuthorisation>();
			approval2.AV_Status = PaymentApprovalStatus.Posted;

			var accPaymentBatch1 = Factory.NewWithValidTestData<AccPaymentBatch>();
			approval1.AV_APB_PaymentBatch = accPaymentBatch1.PK;
			approval2.AV_APB_PaymentBatch = accPaymentBatch1.PK;

			accPaymentBatch1.ClearPaymentApprovalCollection_ForTestOnly();
			AssertEquals("Precondition : batch has working status.", Core.Constants.AccPaymentBatchStatus.Working, accPaymentBatch1.APB_Status);

			Factory.Save();
			AssertEquals("Cannot update batch status because approval 1 is still in FullyApproved status.", Core.Constants.AccPaymentBatchStatus.Working, accPaymentBatch1.APB_Status);

			var approval3 = Factory.NewWithValidTestData<APPaymentApprovalWithoutAuthorisation>();
			approval3.AV_Status = PaymentApprovalStatus.Posted;

			var approval4 = Factory.NewWithValidTestData<APPaymentApprovalWithoutAuthorisation>();
			approval4.AV_Status = PaymentApprovalStatus.Posted;

			var accPaymentBatch2 = Factory.NewWithValidTestData<AccPaymentBatch>();
			approval3.AV_APB_PaymentBatch = accPaymentBatch2.PK;
			approval4.AV_APB_PaymentBatch = accPaymentBatch2.PK;

			accPaymentBatch2.ClearPaymentApprovalCollection_ForTestOnly();
			AssertEquals("Precondition : batch has working status.", Core.Constants.AccPaymentBatchStatus.Working, accPaymentBatch2.APB_Status);

			Factory.Save();
			AssertEquals("Should update batch status to Completed because all approvals are posted.", Core.Constants.AccPaymentBatchStatus.Completed, accPaymentBatch2.APB_Status);

			approval3.AV_Status = PaymentApprovalStatus.Cancelled;
			approval4.AV_Status = PaymentApprovalStatus.Cancelled;
			Factory.Save();
			AssertEquals("Batch status should not be changed once it is set to Completed.", Core.Constants.AccPaymentBatchStatus.Completed, accPaymentBatch2.APB_Status);
		}

		public void TestUpdateBatchStatusToCompletedWhenSomeApprovalsHavePostedStatusAndOtherApprovalsHaveCancelledStatus()
		{
			var approval1 = Factory.NewWithValidTestData<APPaymentApprovalWithoutAuthorisation>();
			approval1.AV_Status = PaymentApprovalStatus.FullyApproved;

			var approval2 = Factory.NewWithValidTestData<APPaymentApprovalWithoutAuthorisation>();
			approval2.AV_Status = PaymentApprovalStatus.Cancelled;

			var approval3 = Factory.NewWithValidTestData<APPaymentApprovalWithoutAuthorisation>();
			approval3.AV_Status = PaymentApprovalStatus.Posted;

			var accPaymentBatch1 = Factory.NewWithValidTestData<AccPaymentBatch>();
			approval1.AV_APB_PaymentBatch = accPaymentBatch1.PK;
			approval2.AV_APB_PaymentBatch = accPaymentBatch1.PK;
			approval3.AV_APB_PaymentBatch = accPaymentBatch1.PK;

			accPaymentBatch1.ClearPaymentApprovalCollection_ForTestOnly();
			AssertEquals("Precondition : batch has working status.", Core.Constants.AccPaymentBatchStatus.Working, accPaymentBatch1.APB_Status);

			Factory.Save();
			AssertEquals("Cannot update batch status because approval 1 is still in FullyApproved status.", Core.Constants.AccPaymentBatchStatus.Working, accPaymentBatch1.APB_Status);

			var approval4 = Factory.NewWithValidTestData<APPaymentApprovalWithoutAuthorisation>();
			approval4.AV_Status = PaymentApprovalStatus.Posted;

			var approval5 = Factory.NewWithValidTestData<APPaymentApprovalWithoutAuthorisation>();
			approval5.AV_Status = PaymentApprovalStatus.Cancelled;

			var approval6 = Factory.NewWithValidTestData<APPaymentApprovalWithoutAuthorisation>();
			approval6.AV_Status = PaymentApprovalStatus.Posted;

			var accPaymentBatch2 = Factory.NewWithValidTestData<AccPaymentBatch>();
			approval4.AV_APB_PaymentBatch = accPaymentBatch2.PK;
			approval5.AV_APB_PaymentBatch = accPaymentBatch2.PK;
			approval6.AV_APB_PaymentBatch = accPaymentBatch2.PK;

			accPaymentBatch2.ClearPaymentApprovalCollection_ForTestOnly();
			AssertEquals("Precondition : batch has working status.", Core.Constants.AccPaymentBatchStatus.Working, accPaymentBatch2.APB_Status);

			Factory.Save();
			AssertEquals("Should update batch status to Completed because two approvals are posted and one approval is cancelled.", Core.Constants.AccPaymentBatchStatus.Completed, accPaymentBatch2.APB_Status);

			approval4.AV_Status = PaymentApprovalStatus.Cancelled;
			approval5.AV_Status = PaymentApprovalStatus.Cancelled;
			approval6.AV_Status = PaymentApprovalStatus.Cancelled;
			Factory.Save();
			AssertEquals("Batch status should not be changed once it is set to Completed.", Core.Constants.AccPaymentBatchStatus.Completed, accPaymentBatch2.APB_Status);
		}

		public void TestUpdateBatchStatusToCancelledWhenAllApprovalsHaveCancelledStatus()
		{
			var approval1 = Factory.NewWithValidTestData<APPaymentApprovalWithoutAuthorisation>();
			approval1.AV_Status = PaymentApprovalStatus.FullyApproved;

			var approval2 = Factory.NewWithValidTestData<APPaymentApprovalWithoutAuthorisation>();
			approval2.AV_Status = PaymentApprovalStatus.Cancelled;

			var accPaymentBatch1 = Factory.NewWithValidTestData<AccPaymentBatch>();
			approval1.AV_APB_PaymentBatch = accPaymentBatch1.PK;
			approval2.AV_APB_PaymentBatch = accPaymentBatch1.PK;

			accPaymentBatch1.ClearPaymentApprovalCollection_ForTestOnly();
			AssertEquals("Precondition : batch has working status.", Core.Constants.AccPaymentBatchStatus.Working, accPaymentBatch1.APB_Status);

			Factory.Save();
			AssertEquals("Cannot update batch status because approval 1 is still in FullyApproved status.", Core.Constants.AccPaymentBatchStatus.Working, accPaymentBatch1.APB_Status);

			var approval3 = Factory.NewWithValidTestData<APPaymentApprovalWithoutAuthorisation>();
			approval3.AV_Status = PaymentApprovalStatus.Cancelled;

			var approval4 = Factory.NewWithValidTestData<APPaymentApprovalWithoutAuthorisation>();
			approval4.AV_Status = PaymentApprovalStatus.Cancelled;

			var accPaymentBatch2 = Factory.NewWithValidTestData<AccPaymentBatch>();
			approval3.AV_APB_PaymentBatch = accPaymentBatch2.PK;
			approval4.AV_APB_PaymentBatch = accPaymentBatch2.PK;

			accPaymentBatch2.ClearPaymentApprovalCollection_ForTestOnly();
			AssertEquals("Precondition : batch has working status.", Core.Constants.AccPaymentBatchStatus.Working, accPaymentBatch2.APB_Status);

			Factory.Save();
			AssertEquals("Should update batch status to Cancelled because all approvals are cancelled.", Core.Constants.AccPaymentBatchStatus.Cancelled, accPaymentBatch2.APB_Status);

			approval3.AV_Status = PaymentApprovalStatus.Posted;
			approval4.AV_Status = PaymentApprovalStatus.Posted;
			Factory.Save();
			AssertEquals("Batch status should not be changed once it is set to Cancelled.", Core.Constants.AccPaymentBatchStatus.Cancelled, accPaymentBatch2.APB_Status);
		}

		public void TestMatchEPaymentRecipients()
		{
			var request = Factory.NewWithValidTestData<AccEPaymentBeneficiaryRequest>();
			request.ABR_GC_Company = GlbCompany.CurrentCompany.PK;
			request.ABR_Status = EPaymentStatusCodes.BeneficiaryRequest.Received;
			request.ABR_SystemCreateTimeUtc = new ZDateTime(2021, 8, 18);
			request.ABR_LastResponseReceivedUtc = new ZDateTime(2021, 8, 18);
			request.ABR_SystemCreateUser = TestObjectCreator.Staff.GS_Code;
			Factory.Save();

			var accPaymentBatch = Factory.NewWithValidTestData<AccPaymentBatch>();
			AssertNotNull(accPaymentBatch.MatchEPaymentRecipients);
			AssertEquals(request.PK, accPaymentBatch.MatchEPaymentRecipients.CurrentRequest.PK);
			AssertEquals(request.PK, accPaymentBatch.MatchEPaymentRecipients.LastReceivedRequest.PK);
		}

		protected virtual AccPaymentBatch GetFirstCreatedAccPaymentBatch() => Factory.New<AccPaymentBatch>();

		TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;
	}
}
