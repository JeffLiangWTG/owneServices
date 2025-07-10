using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Bi.Common;
using CargoWise.Bi.ConfigLoader;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace CargoWise.Bi.Product.Manager.Business.Testing
{
	class AuditInformationTest : TransactionedTestCase
	{
		public void TestAuditTablesAreUnique()
		{
			var auditServer = BiServers.LoadAuditServerUsingCacheIfPossible(TestConnection);
			var auditInfo = new AuditInformation(auditServer, Db.AuditDatabaseName);
			auditInfo.RefreshInfo();

			var auditTablesFromConfiguration = BiAutomationConfigLoader.Instance.ConfigData.CdcTableConfig.Count(t => t.TableInAudit);

			var auditTableNames = auditInfo.AuditTables.Select(t => t.Name);
			var duplicatedTables = auditTableNames.Distinct().Where(t1 => auditTableNames.Count(t2 => t1 == t2) > 1);

			CombineAssertions(() =>
			{
				AssertEquals("Audit Table count:", auditTablesFromConfiguration, auditInfo.AuditTables.Count);
				Assert(string.Format(CultureInfo.InvariantCulture, "Duplicated entries:\r\n{0}", string.Join("\r\n", duplicatedTables)), !duplicatedTables.Any());
			});
		}

		public void TestDataLossInfoIsEmpty()
		{
			var auditServer = BiServers.LoadAuditServerUsingCacheIfPossible(TestConnection);
			var auditInfo = new AuditInformation(auditServer, Db.AuditDatabaseName);
			auditInfo.RefreshInfo();

			AssertEquals("Data Loss count:", 0, auditInfo.DataLoss.Count);
		}

		public void TestAuditSubscriberInfoIsEmpty()
		{
			var auditServer = BiServers.LoadAuditServerUsingCacheIfPossible(TestConnection);
			var auditInfo = new AuditInformation(auditServer, Db.AuditDatabaseName);
			auditInfo.RefreshInfo();

			AssertEquals("Audit subscriber count:", 0, auditInfo.Subscribers.Count);
		}

		[UseSnapshotProtection(Db.AuditDatabaseSuffix)]
		public void TestAuditSubscriberInfoWithLsnPeriod()
		{
			using (var connection = Db.NewAdminConnection(Db.AuditDatabaseName))
			{
				var auditInfo = new AuditInformation(connection.ServerName, Db.AuditDatabaseName);
				PrepareTestData(connection);

				auditInfo.RefreshInfo();

				CombineAssertions(() =>
				{
					AssertEquals("Audit subscriber count:", 2, auditInfo.Subscribers.Count);
					AssertContainsExactElementsInAnyOrder("PeriodHighWaterMark should be converted to string correctly.", new ZInt[] { 100, 0 }, auditInfo.Subscribers.Select(s => s.PeriodHighWatermark));
					AssertContainsExactElementsInAnyOrder("Description should be converted to string correctly.", new ZString[] { "Valid", ZString.Empty }, auditInfo.Subscribers.Select(s => s.Description));
				});
			}
		}

		void PrepareTestData(DbConnection connection)
		{
			var insertSql = $@"
--data for SubscriberControl
insert into [biadmin].[SubscriberControl] (SubscriberCode, Description, LsnHighWaterMark, SeqValHighWaterMark, PeriodHighWaterMark) values ('111', 'Valid', 1, 10, 100);
insert into [biadmin].[SubscriberControl] (SubscriberCode, Description, LsnHighWaterMark, SeqValHighWaterMark, PeriodHighWaterMark) values ('112', null, 1, 10, null);

--data for join table LsnTimeMapping
insert into [biadmin].[LsnTimeMapping] (StartLsn, TranEndTimeUtc) values (1, '2022-07-05 08:23:53.410');

";
			connection.ExecuteNonQuery(insertSql);
		}
	}
}
