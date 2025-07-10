using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Testing.ScriptTests
{
	public class GetDsbJobAmountTest : ScriptTest
	{
		[TestDate(2020, 6, 30)]
		public void TestGetDsbJobInfo()
		{
			var result = RunScript(batch1.PK);
			var jobInfo1 = result.Single(x => x.JobPK == job1.PK);
			var jobInfo2 = result.Single(x => x.JobPK == job2.PK);

			AssertEquals(100m, jobInfo1.DisbursementBalance);
			AssertEquals("Job1 should be closed in batch", false, jobInfo1.HasUnBatchedLine);

			AssertEquals(-150m, jobInfo2.DisbursementBalance);
			AssertEquals("Job2 should be closed in batch", false, jobInfo2.HasUnBatchedLine);
		}

		[TestDate(2020, 6, 30)]
		public void TestGetDsbJobInfoWithMultipleBranch()
		{
			var batch2 = TestObjectCreator.CreateDsbJobCloseBatch("B002");
			line2j2.AL_JBB = batch2.PK;
			Factory.Save();

			var result = RunScript(batch1.PK);
			var jobInfo1 = result.Single(x => x.JobPK == job1.PK);
			var jobInfo2 = result.Single(x => x.JobPK == job2.PK);

			AssertEquals("Job1 should be closed in batch1 with two lines", 100m, jobInfo1.DisbursementBalance);
			AssertEquals(false, jobInfo1.HasUnBatchedLine);

			AssertEquals("Job2 should be closed in batch1 with one line", -200m, jobInfo2.DisbursementBalance);
			AssertEquals(false, jobInfo2.HasUnBatchedLine);

			result = RunScript(batch2.PK);
			jobInfo2 = result.Single(x => x.JobPK == job2.PK);

			AssertEquals("Job2 should be closed again in batch2 with the other line", 50m, jobInfo2.DisbursementBalance);
			AssertEquals(false, jobInfo2.HasUnBatchedLine);
		}

		[TestDate(2020, 6, 30)]
		public void TestGetDsbJobWithChargeCodeOverride()
		{
			var batch2 = TestObjectCreator.CreateDsbJobCloseBatch("B002");
			line2j2.AL_JBB = batch2.PK;
			Factory.Save();

			charge2Rev.JR_ChargeType = Core.Constants.ChargeType.Margin;
			Factory.Save();

			AssertEquals("Pre-condition", line2j2.PK, charge2Rev.JR_AL_ARLine);
			AssertEquals(Core.Constants.ChargeType.Disbursement, line2j2.ChargeCode.AC_ChargeType);
			AssertNotEquals(Core.Constants.ChargeType.Disbursement, charge2Rev.JR_ChargeType);

			var result = RunScript(batch1.PK);

			var jobInfo1 = result.Single(x => x.JobPK == job1.PK);
			var jobInfo2 = result.Single(x => x.JobPK == job2.PK);

			AssertEquals("Job1 is not affected by overrided charge type", 100m, jobInfo1.DisbursementBalance);
			AssertEquals(false, jobInfo1.HasUnBatchedLine);

			AssertEquals("Job2 has a line in other batch but with non DSB job charge", -200m, jobInfo2.DisbursementBalance);
			AssertEquals(false, jobInfo2.HasUnBatchedLine);
		}

		[TestDate(2020, 6, 30)]
		public void TestGetDsbJobWithLineNotInCurrentBatch()
		{
			line2j2.AL_JBB = ZGuid.Empty;
			Factory.Save();

			var result = RunScript(batch1.PK);

			var jobInfo1 = result.Single(x => x.JobPK == job1.PK);
			var jobInfo2 = result.Single(x => x.JobPK == job2.PK);

			AssertEquals(100m, jobInfo1.DisbursementBalance);
			AssertEquals(false, jobInfo1.HasUnBatchedLine);

			AssertEquals("Job2 should only calculate balance of lines in current batch", -200m, jobInfo2.DisbursementBalance);
			AssertEquals(true, jobInfo2.HasUnBatchedLine);
		}

		[TestDate(2020, 6, 30)]
		public void TestGetDsbJobWithLineNotInFinalClosingBatch()
		{
			var batch2 = TestObjectCreator.CreateDsbJobCloseBatch("B002");
			batch2.JBB_BatchStatus = AccountingConstants.DsbJobBatchStatus.RequireApproval;
			line2j2.AL_JBB = batch2.PK;
			Factory.Save();

			var result = RunScript(batch1.PK);

			var jobInfo1 = result.Single(x => x.JobPK == job1.PK);
			AssertEquals(false, jobInfo1.HasOtherUnClosedBatch);

			var jobInfo2 = result.Single(x => x.JobPK == job2.PK);
			AssertEquals(true, jobInfo2.HasOtherUnClosedBatch);
		}

		protected override void SetUp()
		{
			base.SetUp();

			TestObjectCreator.PrepareDsbJobCloseBatchEnvironment(
				out job1, out job2, out batch1,
				out _, out _, out _, out line2j2,
				out _, out _,
				out _, out _, out _, out charge2Rev);
		}

		#region Implementation

		IEnumerable<(ZGuid JobPK, ZDecimal DisbursementBalance, ZBool HasUnBatchedLine, ZBool HasOtherUnClosedBatch)> RunScript(ZGuid batchPK)
		{
			var collection = new DynamicBusinessObjectCollection(Factory);
			collection.Load("SELECT * FROM GetDsbJobAmount(@BatchPK)", new[] { ZSqlParameter.New("@BatchPK", batchPK, DsbJobCloseBatchSchema.PK) });

			return collection.Select(row =>
			{
				var jobPK = new ZGuid(row["JobPK"]);
				var disbursementBalance = new ZDecimal(row["DisbursementBalance"]);
				var hasUnBatchedLine = new ZBool(row["HasUnBatchedLine"]);
				var hasOtherUnClosedBatch = new ZBool(row["HasOtherUnClosedBatch"]);
				return (jobPK, disbursementBalance, hasUnBatchedLine, hasOtherUnClosedBatch);
			}).ToArray();
		}

		Job job1, job2;
		DsbJobCloseBatch batch1;
		InvoiceLine line2j2;
		Charge charge2Rev;

		#endregion
	}
}
