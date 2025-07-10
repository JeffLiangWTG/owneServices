using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Data;
using Enterprise.Registry.Business.eServices;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Scheduler.GraphEngine.Test
{
	internal class StmQueueStateFactoryTest : TransactionedTestCase
	{
		public void TestUpdateIsDoneInBatches()
		{
			foreach (var recordCount in new[] { 2, 3, 5, 10 })
			{
				foreach (var batchSize in new[] { 1, 2, 3, 4 })
				{
					using (eAdaptorRegistry.Instance.StmQueueStateUpdateBatchSize.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, batchSize))
					{
						var factory = new StmQueueStateFactory(() => new GrEngineLogOptions());
						var records = PrepareTestRecords(recordCount);

						var executedCommandsBeforeUpdate = Db.Connection.ExecutedCommandCount;

						factory.Update(records);

						AssertEquals($"Wrong command count for record count: {recordCount} and batch size: {batchSize}", (recordCount + batchSize - 1) / batchSize, Db.Connection.ExecutedCommandCount - executedCommandsBeforeUpdate);
						Assert("All StmQueueState records should be updated", records.All(r => !r.StatusChanged && r.Status == QueueStatusCodes.Codes.Queued));

						foreach (var record in records)
						{
							AssertEquals($"Record with PK {record.Identifier} was not updated", QueueStatusCodes.Codes.Queued, ReadRecord(record.Identifier).Status);
						}
					}
				}
			}
		}

		static List<StmQueueState> PrepareTestRecords(int numberOfRecords)
		{
			var result = new List<StmQueueState>();
			for (var index = 0; index < numberOfRecords; index++)
			{
				var pk = Guid.NewGuid();
				Db.Connection.ExecuteNonQuery($@"INSERT INTO [dbo].[StmQueueState]({StmQueueStateSchema.Constants.PK},{StmQueueStateSchema.Constants.SQS_ServiceTaskCode},{StmQueueStateSchema.Constants.SQS_ParentID},{StmQueueStateSchema.Constants.SQS_ParentTableCode},{StmQueueStateSchema.Constants.SQS_Keys},{StmQueueStateSchema.Constants.SQS_Status},{StmQueueStateSchema.Constants.SQS_SystemCreateTimeUtc},{StmQueueStateSchema.Constants.SQS_SystemCreateUser},{StmQueueStateSchema.Constants.SQS_ParentMessageNumber},{StmQueueStateSchema.Constants.SQS_SystemLastEditUser},{StmQueueStateSchema.Constants.SQS_ChainID})
												VALUES(@PK,'UMI',NEWID(),'EM','','PKE',GETDATE(),'~AD','{index + 1}','~AD','00000000-0000-0000-0000-000000000000')",
					command => command.AddParameter("PK", SqlDbType.UniqueIdentifier, pk));

				var record = ReadRecord(pk);
				record.UpdateStatus(QueueStatusCodes.Codes.Queued);

				result.Add(record);
			}

			return result;
		}

		static StmQueueState ReadRecord(Guid pk)
		{
			var readCommand = Db.Connection.Command($"SELECT {StmQueueState.Columns().Select(c => c.Name).Aggregate((a, b) => a + ", " + b)} FROM [dbo].[StmQueueState] WHERE [SQS_PK] = @PK");
			readCommand.AddParameter("PK", SqlDbType.UniqueIdentifier, pk);

			using (var reader = readCommand.ExecuteReader())
			{
				reader.Read();
				return new StmQueueState(reader);
			}
		}
	}
}
