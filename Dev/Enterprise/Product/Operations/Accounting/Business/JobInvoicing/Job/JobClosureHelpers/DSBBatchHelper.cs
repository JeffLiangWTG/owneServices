using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public static class DSBBatchHelper
	{
		public static ZGuid GetBatchPkForJob(JobHeader job, DateTime runningTime, bool createWhenEmpty = true)
		{
			Argument.NotNull(job, nameof(job));

			var currentBatchPK = GetBatchPK(job.Factory, job.JH_GC, runningTime);
			if (!currentBatchPK.IsValid && createWhenEmpty)
			{
				IDbConnected conn = job.Factory;
				conn.Connection.RunLocked(
					key: "DSBBatchHelper_GetBatchPkForJob", //Check whether it uses Transaction
					max_tries: 2,
					process: (isFirstRun) =>
					{
#if DEBUG
						InjectMethod_GetBatchPkForJob_ForTestOnly?.Invoke();
#endif
						using (var tran = conn.Connection.BeginTransactionWithManager())
						{
							currentBatchPK = GetBatchPK(job.Factory, job.JH_GC, runningTime);
							if (!currentBatchPK.IsValid)
							{
								currentBatchPK = CreateNewBatch(job.JH_GC, runningTime);
							}
							tran.CommitTransaction();
						}
					}
				);
			}
			return currentBatchPK;
		}

		public static IEnumerable<(ZGuid AL_PK, ZGuid AL_JBB, ZDecimal AL_LineAmount)> GetTransactionLinesWithDSBChargeCodes(Job job)
		{
			Argument.NotNull(job, nameof(job));

			var collection = new DynamicBusinessObjectCollection(job.Factory);
			collection.Load("SELECT * FROM GetAllRelativeDsbJobLinesByJob(@jobPK)", new[] { ZSqlParameter.New("@jobPK", job.PK.ToGuid(), DsbJobCloseBatchSchema.PK) });

			return collection.Select(row => {
				var linePK = new ZGuid(row["AL_PK"]);
				var lineBatchPK = new ZGuid(row["AL_JBB"]);
				var lineAmount = new ZDecimal(row["AL_LineAmount"]);
				return (linePK, lineBatchPK, lineAmount);
			});
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		public static void UpdateLinesBatch(Guid[] linesPK, Guid batchPK)
		{
			var sqlText = @"
UPDATE dbo.accTransactionLines
SET
	AL_JBB = @AL_JBB,
	AL_SystemLastEditTimeUtc = GETUTCDATE(),
	AL_SystemLastEditUser = @SystemLastEditUser
WHERE
	AL_PK IN (SELECT Value FROM @tvp_LinesPK)
";
			IDbConnected conn = new BusinessObjectFactory();
			using (var cmd = conn.Connection.Command(sqlText))
			{
				cmd.AddParameter("@AL_JBB", System.Data.SqlDbType.UniqueIdentifier, batchPK);
				cmd.AddTableValuedParameter("@tvp_LinesPK", StmPrintQueueSchema.SQ_QueueName, linesPK);
				cmd.AddParameterBasedOnDbColumn("@SystemLastEditUser", GlbStaff.CurrentUser.GS_Code.ToString(), GlbStaffSchema.GS_Code);
				cmd.ExecuteNonQuery();
			}
		}

		public static bool GetIsExistDsbBatchByCharge(ZGuid chargePK)
		{
			var chargePkParam = ZSqlParameter.New("@ChargePK", chargePK, AccChargeCodeSchema.PK);
			return Db.Connection.Exists($@"
FROM 
	dbo.DsbJobCloseBatch 
	JOIN dbo.AccTransactionLines ON AL_JBB = JBB_PK
	JOIN dbo.AccChargeCode ON AL_AC = AC_PK
WHERE 
	AC_PK = @ChargePK",
			new Action<DbCommand>((x) => x.AddParameter(chargePkParam)));
		}

		static ZGuid GetBatchPK(BusinessObjectFactory factory, ZGuid companyPK, DateTime runningTime)
		{
			Argument.NotNull(factory, nameof(factory));

			var batchQuery = @"
SELECT TOP(1) JBB_PK
FROM dbo.DsbJobCloseBatch WITH (READCOMMITTEDLOCK)
WHERE
	JBB_GC = @companyPK
AND JBB_BatchStatus = 'OPN'
AND JBB_SystemCreateTimeUtc >= Convert(date, @runningTime)
AND JBB_SystemCreateTimeUtc < Convert(date, DATEADD(day , 1 , @runningTime))
ORDER BY JBB_BatchNumber DESC
";

			return factory.LoadScalarValue<ZGuid>(batchQuery,
				new ZSqlParameter[] {
					ZSqlParameter.New("@companyPK", companyPK.ToGuid(), DsbJobCloseBatchSchema.JBB_GC),
					ZSqlParameter.New("@runningTime", runningTime, DsbJobCloseBatchSchema.JBB_SystemCreateTimeUtc),
			});
		}

		static ZGuid CreateNewBatch(ZGuid companyPK, DateTime runningTime)
		{
			var factory = new BusinessObjectFactory();
			var batch = factory.New<DsbJobCloseBatch>();
			batch.JBB_BatchNumber = AccountingNumberFountainWrapperFactory.Instance.GetDisbursementJobCloseBatchNumber(companyPK.ToGuid()).GetNext(factory).ToString();
			batch.JBB_GC = companyPK;
			batch.JBB_SystemCreateTimeUtc = runningTime;
			batch.JBB_SystemCreateUser = Env.CurrentUser.Initials;
			batch.JBB_BatchStatus = "OPN";
			factory.Save();
			return batch.PK;
		}

#if DEBUG
		[ThreadStatic]
		public static Action InjectMethod_GetBatchPkForJob_ForTestOnly;
#endif
	}
}
