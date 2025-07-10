using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Testing.ScriptTests
{
	public class GetAllRelativeDsbJobLinesTest : ScriptTest
	{
		[TestDate(2020, 6, 30)]
		public void TestGetDsbJobLines()
		{
			var batch2 = TestObjectCreator.CreateDsbJobCloseBatch("B002");
			line11.AL_JBB = ZGuid.Empty;
			line22.AL_JBB = batch2.PK;
			Factory.Save();

			var result = RunScript(batch1.PK);

			AssertEquals(4, result.Count());

			AssertLine(result.Single(x => x.LinePK == line11.PK), job1.PK, ZGuid.Empty, -100m);
			AssertLine(result.Single(x => x.LinePK == line12.PK), job2.PK, batch1.PK, -200m);
			AssertLine(result.Single(x => x.LinePK == line21.PK), job1.PK, batch1.PK, 200m);
			AssertLine(result.Single(x => x.LinePK == line22.PK), job2.PK, batch2.PK, 50m);
		}

		[TestDate(2020, 6, 30)]
		public void TestGetDsbJobLinesWithJobChargeTypeOverride()
		{
			line22.AL_JBB = ZGuid.Empty;
			Factory.Save();

			charge2Rev.JR_ChargeType = Core.Constants.ChargeType.Margin;
			Factory.Save();

			AssertEquals("Pre-condition", line22.PK, charge2Rev.JR_AL_ARLine);
			AssertEquals(Core.Constants.ChargeType.Disbursement, line22.ChargeCode.AC_ChargeType);
			AssertNotEquals(Core.Constants.ChargeType.Disbursement, charge2Rev.JR_ChargeType);

			var result = RunScript(batch1.PK);

			AssertEquals(3, result.Count());

			AssertLine(result.Single(x => x.LinePK == line11.PK), job1.PK, batch1.PK, -100m);
			AssertLine(result.Single(x => x.LinePK == line12.PK), job2.PK, batch1.PK, -200m);
			AssertLine(result.Single(x => x.LinePK == line21.PK), job1.PK, batch1.PK, 200m);
		}

		[TestDate(2020, 6, 30)]
		public void TestGetDsbJobLinesWithInvalidShortfallSurplusAccount()
		{
			chargeCode1.AC_AG_DisbursementShortfallAccount = ZGuid.Empty;
			chargeCode1.AC_AG_DisbursementSurplusAccount = ZGuid.Empty;
			Factory.Save();

			var result = RunScript(batch1.PK);

			AssertEquals(2, result.Count());

			AssertLine(result.Single(x => x.LinePK == line12.PK), job2.PK, batch1.PK, -200m);
			AssertLine(result.Single(x => x.LinePK == line22.PK), job2.PK, batch1.PK, 50m);
		}

		protected override void SetUp()
		{
			base.SetUp();

			TestObjectCreator.PrepareDsbJobCloseBatchEnvironment(
				out job1, out job2, out batch1,
				out line11, out line12, out line21, out line22,
				out chargeCode1, out _,
				out _, out _, out _, out charge2Rev);
		}

		#region Implementation

		IEnumerable<LineInfo> RunScript(ZGuid batchPK)
		{
			var collection = new DynamicBusinessObjectCollection(Factory);
			collection.Load("SELECT * FROM GetAllRelativeDsbJobLines(@BatchPK)", new[] { ZSqlParameter.New("@BatchPK", batchPK, DsbJobCloseBatchSchema.PK) });

			var result = new List<LineInfo>();

			foreach (DynamicBusinessObject row in collection)
			{
				var jobPK = new ZGuid(row["AL_JH"]);
				var linePK = new ZGuid(row["AL_PK"]);
				var lineBatchPK = new ZGuid(row["AL_JBB"]);
				var lineAmount = new ZDecimal(row["AL_LineAmount"]);
				result.Add(new LineInfo() { JobPK = jobPK, LinePK = linePK, BatchPK = lineBatchPK, LineAmount = lineAmount });
			}

			return result;
		}

		void AssertLine(LineInfo lineInfo, ZGuid expectedJobPK, ZGuid expectedBatchPK, ZDecimal expectedAmount)
		{
			AssertEquals(expectedJobPK, lineInfo.JobPK);
			AssertEquals(expectedBatchPK, lineInfo.BatchPK);
			AssertEquals(expectedAmount, lineInfo.LineAmount);
		}

		struct LineInfo
		{
			public ZGuid LinePK;
			public ZGuid JobPK;
			public ZGuid BatchPK;
			public ZDecimal LineAmount;
		}

		Job job1, job2;
		DsbJobCloseBatch batch1;
		InvoiceLine line11, line12, line21, line22;
		AccChargeCode chargeCode1;
		Charge charge2Rev;

		#endregion
	}
}
