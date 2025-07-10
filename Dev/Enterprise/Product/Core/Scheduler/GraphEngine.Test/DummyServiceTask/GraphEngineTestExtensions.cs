using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Scheduler.GraphEngine.Test
{
	public static class GraphEngineTestExtensions
	{
		public static void AssertStatusInQueue<TBizo>(string message, BusinessObjectFactory factory, ZString status, SchemaPKColumn pkColumn, ZGuid[] bizoPks)
			where TBizo : BusinessObject
		{
			var query = new ZDBOnlyQuery(typeof(TBizo));
			query.AddToFilter(pkColumn, bizoPks);
			query.ReLoadExistingRows = true;

			var parameters = new ZSqlParameterCollection();
			parameters.Add("@status", status, StmQueueStateSchema.SQS_Status);

			query.AddFilterAndZSQLParameterCollection(
				$@"{pkColumn.Name} in (select {StmQueueStateSchema.Constants.SQS_ParentID} 
																							from {StmQueueStateSchema.Constants.SqlSchemaName}.{StmQueueStateSchema.Constants.TableName}
																							where {StmQueueStateSchema.Constants.SQS_Status} = @status)",
				parameters);

			var actualBusinessObjects = factory.Load<TBizo>(query);

			Assertion.AssertContainsExactElementsInAnyOrder(message, pk => factory.Load<TBizo>(pk)?.ToString() ?? pk.ToString(), bizoPks, actualBusinessObjects.Select(a => a.PK));
		}

		public static ZDBOnlyQuery GetNotInQueueQuery<TBizo>(SchemaPKColumn pkColumn, TBizo[] bizos)
			where TBizo : BusinessObject
		{
			var query = new ZDBOnlyQuery(typeof(TBizo));
			query.AddToFilter(pkColumn, bizos.Select(d => d.PK));
			query.ReLoadExistingRows = true;

			var parameters = new ZSqlParameterCollection();
			query.AddFilterAndZSQLParameterCollection(
				$@"{pkColumn.Name} not in (select {StmQueueStateSchema.Constants.SQS_ParentID} 
																									from {StmQueueStateSchema.Constants.SqlSchemaName}.{StmQueueStateSchema.Constants.TableName}
																									where {StmQueueStateSchema.Constants.SQS_ParentID} is not null)",
	parameters);
			return query;
		}

		public static int CountInQueue<TBizo>(TBizo[] bizos, string status = null)
			where TBizo : BusinessObject
		{
			var sql = $"select count(*) from {StmQueueStateSchema.Constants.SqlSchemaName}.{StmQueueStateSchema.Constants.TableName} where {StmQueueStateSchema.Constants.SQS_ParentID} in (select VALUE from @Bleh)";
			if (status != null)
			{
				sql = sql + " and SQS_Status = @Status";
			}

			using (var command = Db.Connection.Command(sql))
			{
				command.AddTableValuedParameter("@Bleh", StmQueueStateSchema.SQS_ParentID, bizos.Select(b => b.PK.ToString()));
				if (status != null)
				{
					command.AddParameterBasedOnDbColumn("@Status", status, StmQueueStateSchema.SQS_Status);
				}

				return (int)command.ExecuteScalar();
			}
		}

		public static GrEngineEnqueuer<DummyBusinessObject, StmQueueState>.LoadEntitiesResult Enqueue(this GrEngineEnqueuer<DummyBusinessObject, StmQueueState> enqueuer, BusinessObjectFactory factory, INotifications notifications = null)
		{
			var query = new Lazy<ZQuery>(() => new ZQuery());
			var result =  enqueuer.Enqueue(factory, query, notifications);
			foreach (var entity in result.NewEntities.Union(result.OldEntities))
			{
				Assertion.AssertEquals("Entity state must be reset after processing", entity.HasChanges, false);
			}

			return result;
		}
	}
}
