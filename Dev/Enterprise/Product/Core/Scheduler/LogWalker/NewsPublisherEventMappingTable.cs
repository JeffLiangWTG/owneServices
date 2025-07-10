using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.Schema;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.LogWalker
{
	public static class NewsPublisherEventMappingTable
	{
		public static void SynchroniseEventMappingTable(DbConnection connection, IEnumerable<LogSubscriber> subscribers)
		{
			var currentVersion = new EnterpriseInformationRetriever().VersionNumber;

			var (tableVersion, dynamicRowsKey) = GetTableVersionAndDynamicRowsKey(connection);

			var repopulateMappingTable = !tableVersion.Equals(currentVersion) || !SystemDataRegistry.Instance.LogWalkerEventMappingTable.Value;

			var subscribersToUpdate = repopulateMappingTable ? subscribers : subscribers.Where(s => s.HasDynamicProperties || s.IsClientSpecificSubscriber);

			var (key, rowsToInsert) = CreateSubscriberEventMappingDataTable(subscribersToUpdate, repopulateMappingTable);

			if (repopulateMappingTable || !dynamicRowsKey.Equals(key))
			{
				var deleteStatement = repopulateMappingTable
					? "DELETE FROM [StmNewsPublisherEventMapping]"
					: "DELETE FROM [StmNewsPublisherEventMapping] WHERE NPE_IsSubscriberDynamic = 0";

				var insertStatement = rowsToInsert.Any()
					? EventMappingRowValue.GetInsertQuery(rowsToInsert)
					: "";

				var updateKeyStatement = $"UPDATE dbo.StmData SET SD_BinaryValue = convert(varbinary(max), N'{currentVersion}:{key}') WHERE SD_Name = 'NewsPublisherMappingTableKey';";

				var fullStatement = $@"
{deleteStatement}

{insertStatement}

{updateKeyStatement}
";

				connection.ExecuteNonQuery(fullStatement);
			}
		}

		static (string key, IEnumerable<EventMappingRowValue>) CreateSubscriberEventMappingDataTable(IEnumerable<LogSubscriber> subscribers, bool repopulateFullTable)
		{
			var (key, rows) = GetSubscriberRowValues(subscribers);

			if (repopulateFullTable)
			{
				var eventPublisherRows = GetIEventPublisherRowValues();
				rows = rows.Concat(eventPublisherRows);
			}

			return (key, rows);
		}

		static (string key, IEnumerable<EventMappingRowValue>) GetSubscriberRowValues(IEnumerable<LogSubscriber> subscribers)
		{
			var result = new List<EventMappingRowValue>();
			var key = new StringBuilder();
			foreach (var subscriber in subscribers)
			{
				var requiresKey = subscriber.HasDynamicProperties || subscriber.IsClientSpecificSubscriber;
				var subscriberKey = requiresKey ? new StringBuilder().Append(subscriber.Name) : null;
				foreach (var eventCode in subscriber.EventTypes.Where(et => et != Events.WorkflowTriggerEventCode))
				{
					subscriberKey?.Append(eventCode);
					foreach (var tableName in subscriber.TableNames)
					{
						subscriberKey?.Append(tableName);
						var tablePrefix = GetTablePrefix(tableName);
						result.Add(new EventMappingRowValue()
						{
							SubscriberName = subscriber.Name,
							EventCode = eventCode,
							TableName = tableName,
							TablePrefix = tablePrefix,
							IsConstant = !subscriber.IsClientSpecificSubscriber && !subscriber.HasDynamicProperties
						});
					}
				}

				if (requiresKey)
				{
					key.Append(subscriberKey);
				}
			}
			return (key.ToString(), result);
		}

		static IEnumerable<EventMappingRowValue> GetIEventPublisherRowValues()
		{
			foreach (var publisher in WorkflowDescriptors.Instance.Values.OfType<IEventPublisher>())
			{
				foreach (var tableName in publisher.GetPublisherTableNames())
				{
					var tablePrefix = GetTablePrefix(tableName);
					yield return new EventMappingRowValue()
					{
						SubscriberName = "IEventPublisher",
						EventCode = "000",
						TableName = tableName,
						TablePrefix = tablePrefix,
						IsConstant = true
					};
				}
			}
		}

		static string GetTablePrefix(string tableName)
		{
			string tablePrefix = ObjectFactory.Get<IApplicationSchemaResolver>().GetColumnNamePrefix(tableName);
			if (tablePrefix == null)
			{
				string message = string.Format("Table prefix not found for table [{0}].", tableName);
				throw new ApplicationException(message);
			}

			return tablePrefix;
		}

#if DEBUG
		public
#endif
		static (string version, string key) GetTableVersionAndDynamicRowsKey(DbConnection connection)
		{
			var dbKey = connection.ExecuteScalar("SELECT CONVERT(nvarchar(max), SD_BinaryValue), * FROM dbo.StmData WHERE SD_Name = 'NewsPublisherMappingTableKey'");

			if (dbKey == null)
			{
				dbKey = "-1:-1";
				connection.ExecuteNonQuery($"INSERT INTO dbo.StmData (SD_PK, SD_Name, SD_BinaryValue) VALUES (newid(), 'NewsPublisherMappingTableKey', convert(varbinary(max), N'{dbKey}'))");
			}

			var result = ((string)dbKey).Split(':');
			if (result.Length == 2)
			{
				return (result[0], result[1]);
			}
			else
			{
				return (string.Empty, string.Empty);
			}
		}

#if DEBUG

		public static void UpdateIEventPublisherMappingsForTest()
		{
			var rowsToInsert = GetIEventPublisherRowValues();
			var updateQuery = $@"
DELETE FROM [StmNewsPublisherEventMapping] WHERE NPE_SubscriberName = 'IEventPublisher';

{EventMappingRowValue.GetInsertQuery(rowsToInsert)}";

			Db.Connection.ExecuteNonQuery(updateQuery);
		}
#endif
	}

	class EventMappingRowValue
	{
		public string SubscriberName { get; set; }
		public string EventCode { get; set; }
		public string TableName { get; set; }
		public string TablePrefix { get; set; }
		public bool IsConstant { get; set; }

		public static string GetInsertQuery(IEnumerable<EventMappingRowValue> rows)
		{
			return $"INSERT INTO [StmNewsPublisherEventMapping] (NPE_SubscriberName, NPE_EventCode, NPE_TableName, NPE_TablePrefix, NPE_IsSubscriberDynamic, NPE_SystemCreateTimeUtc, NPE_SystemCreateUser, NPE_SystemLastEditTimeUtc, NPE_SystemLastEditUser) VALUES {string.Join(",", rows)}";
		}

		public override string ToString() => $"(N'{SubscriberName}', N'{EventCode}', N'{TableName}', N'{TablePrefix}', {(IsConstant ? 1 : 0)}, GETUTCDATE(), '~BP', GETUTCDATE(), '~BP')";
	}
}
