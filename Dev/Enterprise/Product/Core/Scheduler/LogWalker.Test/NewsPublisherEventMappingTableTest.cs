using System;
using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.LogWalker.Test
{
	public class NewsPublisherEventMappingTableTest : TestCaseWithFactory
	{
		void DeleteRows() => Db.Connection.ExecuteNonQuery($"DELETE FROM dbo.StmNewsPublisherEventMapping");

		void DeleteTableKey() => Db.Connection.ExecuteNonQuery("DELETE FROM dbo.StmData WHERE SD_Name = 'NewsPublisherMappingTableKey'");

		int CountRows() => Db.Connection.ExecuteScalar<int>($"SELECT COUNT(*) FROM dbo.StmNewsPublisherEventMapping");

		int ExpectedNumerOfRows(LogSubscriber[] subscribers = null, bool includeEventPublishers = true)
		{
			subscribers = subscribers ?? new SubscriberProvider().GetAllSubscribersWithValidationAndAppendingToNotificationLog(new LoggerForLogWalkerTest());
			var rows = 0;

			foreach (var subscriber in subscribers)
			{
				foreach (string eventCode in subscriber.EventTypes.Where(et => et != Events.WorkflowTriggerEventCode))
				{
					foreach (string tableName in subscriber.TableNames)
					{
						rows++;
					}
				}
			}

			if (!includeEventPublishers)
			{
				return rows;
			}

			foreach (var publisher in WorkflowDescriptors.Instance.Values.OfType<IEventPublisher>())
			{
				foreach (var tableName in publisher.GetPublisherTableNames())
				{
					rows++;
				}
			}

			return rows;
		}

		public void TestProcedurePopulatesTableAndTableVersion()
		{
			DeleteTableKey();
			DeleteRows();

			var connection = Db.Connection;

			var (tableVersion, dynamicRowsKey) = NewsPublisherEventMappingTable.GetTableVersionAndDynamicRowsKey(connection);
			AssertEquals("Precondition", "-1", tableVersion);
			AssertEquals("Precondition", "-1", dynamicRowsKey);

			var subscribers = new SubscriberProvider().GetAllSubscribersWithValidationAndAppendingToNotificationLog(new LoggerForLogWalkerTest());
			NewsPublisherEventMappingTable.SynchroniseEventMappingTable(connection, subscribers);

			(tableVersion, dynamicRowsKey) = NewsPublisherEventMappingTable.GetTableVersionAndDynamicRowsKey(connection);
			AssertEquals(new EnterpriseInformationRetriever().VersionNumber, tableVersion);
			AssertNotEquals("-1", dynamicRowsKey);

			AssertEquals(ExpectedNumerOfRows(), CountRows());

			DeleteRows();
			NewsPublisherEventMappingTable.SynchroniseEventMappingTable(connection, subscribers);
			AssertEquals("The table version and dynamic rows key is upto date so now rows are inserted", 0, CountRows());
		}

		public void TestRepopulateTableWhenRegistryItemDisabled()
		{
			var connection = Db.Connection;
			DeleteRows();

			var subscribers = new SubscriberProvider().GetAllSubscribersWithValidationAndAppendingToNotificationLog(new LoggerForLogWalkerTest());

			SystemDataRegistry.Instance.LogWalkerEventMappingTable.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			NewsPublisherEventMappingTable.SynchroniseEventMappingTable(connection, subscribers);

			var rowCount1 = CountRows();
			AssertEquals(ExpectedNumerOfRows(), rowCount1);

			DeleteRows();
			NewsPublisherEventMappingTable.SynchroniseEventMappingTable(connection, subscribers);
			var rowCount2 = CountRows();
			AssertEquals(rowCount1, rowCount2);
		}

		public void TestDynamicSubscribersUpdateMappingTableWhenValuesChange()
		{
			var connection = Db.Connection;
			//Initially populate table to set table key
			NewsPublisherEventMappingTable.SynchroniseEventMappingTable(connection, Array.Empty<LogSubscriber>());
			DeleteRows();

			var subscriber = new MockSubscriber(eventTypes: new[] { Events.CustomisableEvent00Code, Events.CustomisableEvent01Code }, tableNames: new[] { ProcessTasksSchema.Constants.TableName }, name: "SUB1");
			var subscribers = new[] { subscriber };

			NewsPublisherEventMappingTable.SynchroniseEventMappingTable(connection, subscribers);
			var expectedRows = ExpectedNumerOfRows(subscribers, false);
			AssertEquals(expectedRows, CountRows());

			subscriber = new MockSubscriber(eventTypes: new[] { Events.CustomisableEvent00Code }, tableNames: new[] { ProcessTasksSchema.Constants.TableName }, name: "SUB1");
			subscribers = new[] { subscriber };
			NewsPublisherEventMappingTable.SynchroniseEventMappingTable(connection, subscribers);

			AssertEquals("Expecting the mapping to be updates as the number of events subscribed to has changed", expectedRows - 1, CountRows());

			var commandsBefore = connection.ExecutedCommandCount;
			NewsPublisherEventMappingTable.SynchroniseEventMappingTable(connection, subscribers);
			AssertEquals("Expecting 1 DB hit to check the current key of the mapping table", commandsBefore + 1, connection.ExecutedCommandCount);
		}
	}
}
