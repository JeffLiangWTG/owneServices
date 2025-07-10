using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Scheduler.GraphEngine.Test
{
	[UseSnapshotProtection]
	abstract class DummyGrEngineServiceTaskTestCase : TestCase
	{
		protected BusinessObjectFactory factory;

		public void AssertNotInQueue(params DummyBusinessObject[] dummyBusinessObjects)
		{
			RequireSaved(dummyBusinessObjects);
			var query = GraphEngineTestExtensions.GetNotInQueueQuery(DummyBizoSchema.PK, dummyBusinessObjects);
			AssertContainsExactElementsInAnyOrder("Expecting none of these items to be in the queue.", dummyBusinessObjects, factory.Load<DummyBusinessObject>(query));
		}

		public void AssertNotReadyToProcess(params DummyBusinessObject[] dummyBusinessObjects)
		{
			RequireSaved(dummyBusinessObjects);
			GraphEngineTestExtensions.AssertStatusInQueue<DummyBusinessObject>("Expected dummies to be in the queue but they weren't", factory, QueueStatusCodes.Codes.Blocked, DummyBizoSchema.PK, dummyBusinessObjects.Select(d => d.PK).ToArray());
		}

		public void AssertReadyToProcess(params DummyBusinessObject[] dummyBusinessObjects)
		{
			RequireSaved(dummyBusinessObjects);
			GraphEngineTestExtensions.AssertStatusInQueue<DummyBusinessObject>("Expected dummies to be in the queue but they weren't", factory, QueueStatusCodes.Codes.Queued, DummyBizoSchema.PK, dummyBusinessObjects.Select(d => d.PK).ToArray());
		}

		public void AssertStatusInQueue(string message, string code, params DummyBusinessObject[] dummyBusinessObjects)
		{
			RequireSaved(dummyBusinessObjects);
			GraphEngineTestExtensions.AssertStatusInQueue<DummyBusinessObject>(message, factory, code, DummyBizoSchema.PK, dummyBusinessObjects.Select(d => d.PK).ToArray());
		}

		public void AssertSharesChainId(params DummyBusinessObject[] dummyBusinessObjects)
		{
			var chainIDs = GetChainIds(dummyBusinessObjects.Select(s => s.PK).ToArray());
			AssertEquals("Should be the same number of ids loaded", dummyBusinessObjects.Length, chainIDs.Count);
			AssertEquals("Only one unique id", 1, chainIDs.Distinct().Count());
			AssertNotEquals("Chain ID should not be empty", Guid.Empty, chainIDs.First());
		}

		static IList<Guid> GetChainIds(ZGuid[] pks)
		{
			var result = new List<Guid>();
			using (var command = Db.Connection.Command("select SQS_ChainID from dbo.StmQueueState where SQS_ParentId in (select value from @pks)"))
			{
				command.AddTableValuedParameter("@pks", StmQueueStateSchema.SQS_ParentID, pks);
				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						result.Add(reader.GetGuid(0));
					}
				}
			}
			return result;
		}

		public int CountInQueue(params DummyBusinessObject[] dummyBusinessObjects)
		{
			RequireSaved(dummyBusinessObjects);
			return GraphEngineTestExtensions.CountInQueue(dummyBusinessObjects);
		}

		public int CountProcessed(params DummyBusinessObject[] dummyBusinessObjects)
		{
			RequireSaved(dummyBusinessObjects);
			return GraphEngineTestExtensions.CountInQueue(dummyBusinessObjects, QueueStatusCodes.Codes.Processed);
		}

		static void RequireSaved(DummyBusinessObject[] dummyBusinessObjects)
		{
			Assert("You need to save before calling this.", dummyBusinessObjects.Aggregate(true, (a, s) => a && s.IsInDatabase));
		}
	}
}
