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
	public class GetAllRelativeDsbJobLinesByJobTest : ScriptTest
	{
		[TestDate(2020, 6, 30)]
		public void TestGetDsbJobLines()
		{
			var batch2 = TestObjectCreator.CreateDsbJobCloseBatch("B002");
			line1j1.AL_JBB = ZGuid.Empty;
			line2j2.AL_JBB = batch2.PK;
			Factory.Save();

			var result1 = RunScript(job1.PK);
			AssertEquals(2, result1.Count());

			AssertLine(result1.Single(x => x.LinePK == line1j1.PK), ZGuid.Empty, -100m);
			AssertLine(result1.Single(x => x.LinePK == line2j1.PK), batch1.PK, 200m);

			var result2 = RunScript(job2.PK);
			AssertEquals(2, result2.Count());
			AssertLine(result2.Single(x => x.LinePK == line1j2.PK), batch1.PK, -200m);
			AssertLine(result2.Single(x => x.LinePK == line2j2.PK), batch2.PK, 50m);
		}

		[TestDate(2020, 6, 30)]
		public void TestGetDsbJobLinesWithJobChargeTypeOverride()
		{
			line2j2.AL_JBB = ZGuid.Empty;
			Factory.Save();

			charge2Rev.JR_ChargeType = Core.Constants.ChargeType.Margin;
			Factory.Save();

			AssertEquals("Pre-condition", line2j2.PK, charge2Rev.JR_AL_ARLine);
			AssertEquals(Core.Constants.ChargeType.Disbursement, line2j2.ChargeCode.AC_ChargeType);
			AssertNotEquals(Core.Constants.ChargeType.Disbursement, charge2Rev.JR_ChargeType);

			var result1 = RunScript(job1.PK);
			AssertEquals(2, result1.Count());

			AssertLine(result1.Single(x => x.LinePK == line1j1.PK), batch1.PK, -100m);
			AssertLine(result1.Single(x => x.LinePK == line2j1.PK), batch1.PK, 200m);

			var result2 = RunScript(job2.PK);
			AssertEquals(1, result2.Count());
			AssertLine(result2.Single(x => x.LinePK == line1j2.PK), batch1.PK, -200m);
		}

		[TestDate(2020, 6, 30)]
		public void TestGetDsbJobLinesWithInvalidShortfallSurplusAccount()
		{
			chargeCode1.AC_AG_DisbursementShortfallAccount = ZGuid.Empty;
			chargeCode1.AC_AG_DisbursementSurplusAccount = ZGuid.Empty;
			Factory.Save();

			var result1 = RunScript(job1.PK);
			AssertEquals(0, result1.Count());

			var result2 = RunScript(job2.PK);
			AssertEquals(2, result2.Count());
			AssertLine(result2.Single(x => x.LinePK == line1j2.PK), batch1.PK, -200m);
			AssertLine(result2.Single(x => x.LinePK == line2j2.PK), batch1.PK, 50m);
		}

		protected override void SetUp()
		{
			base.SetUp();

			TestObjectCreator.PrepareDsbJobCloseBatchEnvironment(
				out job1, out job2, out batch1,
				out line1j1, out line1j2, out line2j1, out line2j2,
				out chargeCode1, out _,
				out _, out _, out _, out charge2Rev);
		}

		#region Implementation

		IEnumerable<LineInfo> RunScript(ZGuid jobPK)
		{
			var collection = new DynamicBusinessObjectCollection(Factory);
			collection.Load("SELECT * FROM GetAllRelativeDsbJobLinesByJob(@JobPK)", new[] { ZSqlParameter.New("@JobPK", jobPK.ToGuid(), DsbJobCloseBatchSchema.PK) });

			var result = new List<LineInfo>();

			foreach (DynamicBusinessObject row in collection)
			{
				var linePK = new ZGuid(row["AL_PK"]);
				var lineBatchPK = new ZGuid(row["AL_JBB"]);
				var lineAmount = new ZDecimal(row["AL_LineAmount"]);
				result.Add(new LineInfo() { LinePK = linePK, BatchPK = lineBatchPK, LineAmount = lineAmount });
			}

			return result;
		}

		void AssertLine(LineInfo lineInfo, ZGuid expectedBatchPK, ZDecimal expectedAmount)
		{
			AssertEquals(expectedBatchPK, lineInfo.BatchPK);
			AssertEquals(expectedAmount, lineInfo.LineAmount);
		}

		struct LineInfo
		{
			public ZGuid LinePK;
			public ZGuid BatchPK;
			public ZDecimal LineAmount;
		}

		Job job1, job2;
		DsbJobCloseBatch batch1;
		InvoiceLine line1j1, line1j2, line2j1, line2j2;
		AccChargeCode chargeCode1;
		Charge charge2Rev;

		#endregion
	}
}
