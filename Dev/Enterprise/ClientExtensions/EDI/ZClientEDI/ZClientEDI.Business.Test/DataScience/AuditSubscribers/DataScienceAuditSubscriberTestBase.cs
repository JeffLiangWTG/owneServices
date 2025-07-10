using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Bi.Common;
using CargoWise.Bi.Common.Testing;
using CargoWise.Bi.ConfigLoader;
using Enterprise.AuditDataServices.Subscription.Common;
using Enterprise.AuditDataServices.Subscription.Testing;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core.Encryption;

namespace Enterprise.Client.EDI.DataScience.Business.AuditSubscribers.Tests
{
	public abstract class DataScienceAuditSubscriberTestBase<SubscriberType> : ActualDataChangesAuditSubscriberTest
		where SubscriberType : DataScienceSubscriberToKafkaBase
	{
		protected abstract IEnumerable<string> IgnoredColumnNames { get; }

		protected SubscriberType SubscriberUnderTest => (SubscriberType)TestDataChangeSubscriber;

		protected override DataTable GetTestDataTable() => null;

		public void TestTwoWayEncoderKeyAndStandardIVHaveNotChanged()
		{
			var encoder = TwoWayEncoder.NewWithStandardInitialisationVector();
			AssertEquals("7t5QPdYYkosmnLJlJZB1fA==", encoder.Encrypt("a"));
		}

		public virtual void TestEveryCdcEnabledColumnIsEitherSubscribedXorIgnored()
		{
			var subscriber = SubscriberUnderTest;
			var subscribedColumns = new HashSet<string>(subscriber.ColumnInfos.Select(c => c.ColumnName));
			var ignoredColumns = new HashSet<string>(IgnoredColumnNames);
			CombineAssertions(
				$"One or more new columns in the table {subscriber.Table.TableName} (required by {typeof(SubscriberType).Name}) are now tracked by AuditDataServices. " +
				$"Please ask the Data Science team if they are interested in tracking this new column. " +
				$"If not, add it to {nameof(IgnoredColumnNames)} property in the test case.",
				() =>
				{
					foreach (var cdcEnabledColumn in GetAllCdcEnabledColumns(subscriber))
					{
						var isColumnSubscribed = subscribedColumns.Contains(cdcEnabledColumn);
						var isColumnIgnored = ignoredColumns.Contains(cdcEnabledColumn);
						Assert($"Column {subscriber.Table.TableName}.{cdcEnabledColumn} not exclusively subscribed or ignored.", isColumnSubscribed != isColumnIgnored);
					}
				});
		}

		public virtual void TestEverySubscribedColumnIsCdcEnabled()
		{
			var subscriber = SubscriberUnderTest;
			var cdcEnabledColumns = new HashSet<string>(GetAllCdcEnabledColumns(subscriber));
			CombineAssertions(
				$"One or more new columns in subscriber {typeof(SubscriberType).Name} are no longer CDC-enabled. ",
				() =>
				{
					foreach (var subscribedColumn in subscriber.ColumnInfos)
					{
						var isColumnCdcEnabled = cdcEnabledColumns.Contains(subscribedColumn.ColumnName);
						Assert($"Column {subscriber.Table.TableName}.{subscribedColumn.ColumnName} is subscribed but not CDC-enabled", isColumnCdcEnabled);
					}
				});
		}

		public virtual void TestEveryIgnoredColumnIsCdcEnabled()
		{
			var subscriber = SubscriberUnderTest;
			var cdcEnabledColumns = new HashSet<string>(GetAllCdcEnabledColumns(subscriber));

			CombineAssertions(
				$"One or more ignored columns in subscriber {typeof(SubscriberType).Name} are no longer CDC-enabled. ",
				() =>
				{
					foreach (var ignoredColumn in IgnoredColumnNames)
					{
						var isColumnCdcEnabled = cdcEnabledColumns.Contains(ignoredColumn);
						Assert($"Column {subscriber.Table.TableName}.{ignoredColumn} is explicitly ignored but not CDC-enabled", isColumnCdcEnabled);
					}

					Assert(true); // So NUnit won't butch about an empty test when no column is ignored.
				});
		}

		protected static List<string> GetAllCdcEnabledColumns(SubscriberType subscriber) => BiAutomationConfigLoader.Instance.ConfigData.CdcTableConfig
			.Single(tableConfigRow => tableConfigRow.SourceSchema == subscriber.Table.SqlSchemaName && tableConfigRow.SourceTable == subscriber.Table.TableName)
			.GetCdcColumnConfigRows()
			.Where(columnConfigRow => columnConfigRow.ColumnInAudit)
			.Select(columnConfigRow => columnConfigRow.SourceColumn)
			.ToList();

		public virtual void TestSpecificColumnsSetToNullForQueryTypeAuditSubscriberWrapper()
		{
			var logger = new LoggerForTest();
			using var connection = AuditTestHelper.GetAuditConnection();
			using (BiTemporaryMasterState.SetParameterTemporaryValue(connection, BiConstants.LastMaxLsnProcessed, "0x00"))
			{
				var subscriberWrapper = SubscriberUnderTest.GetWrapper(connection, logger) as QueryTypeAuditSubscriberWrapper;
				Assert(subscriberWrapper != null);
				Assert(subscriberWrapper.SpecificColumns == null);
			}
		}
	}
}
