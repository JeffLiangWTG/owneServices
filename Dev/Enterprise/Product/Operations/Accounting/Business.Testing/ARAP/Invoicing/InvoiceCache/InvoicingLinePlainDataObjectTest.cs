using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.Invoicing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	public class InvoicingLinePlainDataObjectTest : TestCaseWithFactory
	{
		public void TestCreate()
		{
			var invoicingLine = Factory.NewWithValidTestData<APInvoiceLine>();
			invoicingLine.AL_AC = ZGuid.Empty;
			AssertNull(invoicingLine.Job);
			var plainObject = InvoicingLinePlainDataObject.Create(invoicingLine);
			AssertEquals(invoicingLine.PK, plainObject.PK);
			AssertEquals(invoicingLine.Branch.PK, plainObject.BranchPK);
			AssertEquals(invoicingLine.Department.PK, plainObject.DepartmentPK);
			AssertEquals(invoicingLine.AL_RX_NKTransactionCurrency, plainObject.Currrency);
			AssertEquals(invoicingLine.AL_ExchangeRate, plainObject.ExchangeRate);
			AssertEquals(ZGuid.Empty, plainObject.ChargeCodePK);
			AssertEquals(ZGuid.Empty, plainObject.JobPK);
			AssertEquals(ZGuid.Empty, plainObject.RelatedJobPK);

			invoicingLine.AL_AC = TestObjectCreator.CC1.PK;
			var job = TestObjectCreator.CreateJob(TestObjectCreator.Creditor1, 0m, null, 0m);
			var charge = Factory.New<Charge>();
			var jobChargeTarget = Factory.New<JobChargeTarget>();
			jobChargeTarget.JRT_JR = charge.PK;
			jobChargeTarget.JRT_RelatedJobID = job.PK;
			invoicingLine.AL_JH = job.PK;
			invoicingLine.OriginalJobCharge = charge;

			plainObject = InvoicingLinePlainDataObject.Create(invoicingLine);
			AssertEquals(TestObjectCreator.CC1.PK, plainObject.ChargeCodePK);
			AssertEquals(job.PK, plainObject.JobPK);
			AssertEquals(job.PK, plainObject.RelatedJobPK);
		}

		TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;
	}
}
