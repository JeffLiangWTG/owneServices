using System;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public interface IJCSJobProvider
	{
		PickedJob? LoadJobPKFromQueue();
		bool CanTriggerQueuePopulation { get; }
		bool CanContinue(int jobCount);
		bool HasCounterReachedEndOfQueue();
		void UpdateRowNumberInRegistry();
		void Reset();
	}

	public class JCSJobProvider : IJCSJobProvider
	{
		internal JCSJobProvider()
		{
			LastRowNumber = AccountingConfigurationRegistry.Instance.NumberOfJobsProcessedForAutoClosingInTheCurrentBatch.Value;
		}

		public PickedJob? LoadJobPKFromQueue() => GetAJobFromQueue();

		public bool CanTriggerQueuePopulation => true;

		public bool CanContinue(int jobCount)
		{
			var unboundedBatchSize = AccountingConfigurationRegistry.Instance.AutoJobClosureProcessUnboundedBatchSize.Value;
			var batchSize = AccountingConfigurationRegistry.Instance.AutoJobClosureProcessBatchSize.Value;
			return unboundedBatchSize || jobCount < batchSize;
		}

		public void UpdateRowNumberInRegistry()
		{
			AccountingConfigurationRegistry.Instance.NumberOfJobsProcessedForAutoClosingInTheCurrentBatch.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, LastRowNumber);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "SQL query")]
		public bool HasCounterReachedEndOfQueue()
		{
			var maxRowNumber = Db.Connection.ExecuteScalar<int>("SELECT ISNULL(MAX(JHC_RowNumber), 0) FROM JobToCloseQueue");
			return LastRowNumber >= maxRowNumber;
		}

		public void Reset() => LastRowNumber = 0;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		PickedJob? GetAJobFromQueue()
		{
			var currentRowNumber = LastRowNumber + 1;
			try
			{
				PickedJob? job = null;
				using (var cmd = Db.Connection.Command("SELECT JHC_JH, JHC_RowNumber FROM dbo.JobToCloseQueue WHERE JHC_RowNumber = @rowNumber"))
				{
					cmd.AddParameter("@rowNumber", System.Data.SqlDbType.BigInt, currentRowNumber);
					using (var reader = cmd.ExecuteReader())
					{
						if (reader.Read())
						{
							var jobPKObj = reader["JHC_JH"];
							var rowNumberObj = reader["JHC_RowNumber"];
							if (jobPKObj != DBNull.Value && rowNumberObj != DBNull.Value)
							{
								job = new PickedJob((int)rowNumberObj, (Guid)jobPKObj);
							}
						}
					}
				}
				return job;
			}
			finally
			{
				LastRowNumber = currentRowNumber;
			}
		}

		int LastRowNumber { get; set; }
	}

	public struct PickedJob
	{
		public PickedJob(int rowNumber, ZGuid pk)
		{
			RowNumber = rowNumber;
			PK = pk;
		}

		public ZGuid PK { get; }

		public int RowNumber { get; }
	}
}
