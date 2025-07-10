using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.Client.EDI.ScavengingImportServiceTask;
using Enterprise.Client.EDI.ScavengingImportServiceTask.Business;
using Enterprise.Client.EDI.ServiceTasks;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace ZClientEDI.Test.ServiceTasks
{
	[TestedType(typeof(ScavengingPurgeServiceTask))]
	class ScavengingPurgeServiceTaskTest : ServiceTaskTestCase<ScavengingPurgeServiceTask>
	{
		public void TestServiceTaskCanRunInAnyBranch()
		{
			AssertNotNull(GetHostedServiceAttributes().Single(x => x.CanRunInAnyBranch));
		}

		[TestDate(2014, 12, 08)]
		public void TestClientStatisticsXMLArchive()
		{
			AddClientStatisticsXMLArchive(new DateTime(2014, 6, 1));
			AddClientStatisticsXMLArchive(new DateTime(2014, 6, 2));
			AddClientStatisticsXMLArchive(new DateTime(2014, 6, 3));
			AddClientStatisticsXMLArchive(new DateTime(2014, 6, 4));
			AddClientStatisticsXMLArchive(new DateTime(2014, 6, 5));
			AddClientStatisticsXMLArchive(new DateTime(2014, 6, 6));
			AddClientStatisticsXMLArchive(new DateTime(2014, 6, 7));
			AddClientStatisticsXMLArchive(new DateTime(2014, 6, 8));
			AddClientStatisticsXMLArchive(new DateTime(2014, 6, 9));
			AssertEquals(9, GetTableRowCount("ClientStatisticsXMLArchive"));

			var task = new ScavengingPurgeServiceTask(5);
			InitialiseTaskSchedule(task);
			task.RunTask();

			AssertEquals(2, GetTableRowCount("ClientStatisticsXMLArchive"));
			AssertEquals(@"
Information|Starting purge.
Information|5 CSA (Client Statistics XML Archive) items have been purged.
Information|2 CSA (Client Statistics XML Archive) items have been purged.
Information|0 COC (Client Consol) items have been purged.
Information|0 OIH (Client Org. Import History) items have been purged.
Information|Finished purge.
".TrimStart(), task.ServiceLogger.ToString());
		}

		[TestDate(2014, 12, 08)]
		public void TestClientOrgConsol()
		{
			AddClientOrgConsol(new DateTime(2009, 12, 1), 1);
			AddClientOrgConsol(new DateTime(2009, 12, 2), 2);
			AddClientOrgConsol(new DateTime(2009, 12, 3), 3);
			AddClientOrgConsol(new DateTime(2009, 12, 4), 4);
			AddClientOrgConsol(new DateTime(2009, 12, 5), 5);
			AddClientOrgConsol(new DateTime(2009, 12, 6), 6);
			AddClientOrgConsol(new DateTime(2009, 12, 7), 7);
			AddClientOrgConsol(new DateTime(2009, 12, 8), 8);
			AddClientOrgConsol(new DateTime(2009, 12, 9), 9);
			AssertEquals(9, GetTableRowCount("ClientOrgConsol"));

			var task = new ScavengingPurgeServiceTask(5);
			InitialiseTaskSchedule(task);
			task.RunTask();

			AssertEquals(2, GetTableRowCount("ClientOrgConsol"));
			AssertEquals(@"
Information|Starting purge.
Information|0 CSA (Client Statistics XML Archive) items have been purged.
Information|5 COC (Client Consol) items have been purged.
Information|2 COC (Client Consol) items have been purged.
Information|0 OIH (Client Org. Import History) items have been purged.
Information|Finished purge.
".TrimStart(), task.ServiceLogger.ToString());
		}

		[TestDate(2018, 10, 10)]
		public void TestAddClientOrgImportHistoryWithTimeCheck()
		{
			var registrySetting = new ScavengingPurgeSettings();
			var oihItem = registrySetting.AddNew();
			oihItem.Code = "OIH";
			oihItem.Description = (NoResString)"Client Org. Import History";
			oihItem.PurgeTime = 6;
			oihItem.PurgeTimeUnit = ScavengingPurgeItem.TimeUnit.Month;
			using (EDIDataRegistry.Instance.ScavengingPurgeSettings.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, registrySetting))
			{
				var org = Factory.New<OrgHeader>();
				org.CreatePatternMatchOverrideForTest().OO_Relationship = EDIConstants.OrgPatternMatchOverrideRelationships.EHubClientID;
				var emptyQuery = new ZQuery() { FetchOnlyFromLocalCache = true };
				var service = (ScavengingImportService)new ScavengingImportContainer(Mock.Of<INotifications>()).ResolveService();

				org.OH_SystemLastEditTimeUtc = ZDateTime.Empty;
				service.AddClientOrgImportHistory(org, "");
				AssertNull("No ClientOrgImportHistory created because the time is empty", Factory.LoadTop1<ClientOrgImportHistory>(emptyQuery));

				org.OH_SystemLastEditTimeUtc = new ZDateTime(1940, 5, 5);
				service.AddClientOrgImportHistory(org, "");
				AssertNull("No ClientOrgImportHistory created because it's too old", Factory.LoadTop1<ClientOrgImportHistory>(emptyQuery));

				org.OH_SystemLastEditTimeUtc = new ZDateTime(2020, 10, 20);
				Assert(org.OH_SystemLastEditTimeUtc.IsInTheFutureDatePartOnly);
				service.AddClientOrgImportHistory(org, "");
				AssertNull("No ClientOrgImportHistory created because it's in the future", Factory.LoadTop1<ClientOrgImportHistory>(emptyQuery));

				org.OH_SystemLastEditTimeUtc = new ZDateTime(2018, 10, 5);
				service.AddClientOrgImportHistory(org, "");
				AssertNotNull("There is ClientOrgImportHistory created!", Factory.LoadTop1<ClientOrgImportHistory>(emptyQuery));
			}
		}

		[TestDate(2010, 9, 8)]
		[UseSnapshotProtection]
		public void TestClientOrgImportHistory()
		{
			Db.Connection.CommitTransaction();
			ScavengingPartitionHelperTest.CreatePartitionScheme();
			ScavengingPartitionHelperTest.PartitionTable("ClientOrgImportHistory", "O2_ImportedDate", "O2_PartitionBy", "NR_RC__O2_ImportedDate");
			ScavengingPartitionHelperTest.DropEverythingThatBlocksTruncate("ClientOrgImportHistory", "PK_UX__O2_PK", new[] { "NR_RX__O2_TargetOrgPK" });

			var org1 = CreateTestOrg(1);
			var org2 = CreateTestOrg(2);
			var org3 = CreateTestOrg(3);
			var org4 = CreateTestOrg(4);

			var firstImportHistoryForOrg1 = AddClientOrgImportHistory(org1, new DateTime(2010, 3, 27), true);
			var firstImportHistoryForOrg2 = AddClientOrgImportHistory(org2, new DateTime(2010, 3, 28), true);
			AddClientOrgImportHistory(org2, new DateTime(2010, 3, 29));
			AddClientOrgImportHistory(org2, new DateTime(2010, 3, 30));
			AddClientOrgImportHistory(org2, new DateTime(2010, 3, 31));
			var firstImportHistoryForOrg3 = AddClientOrgImportHistory(org3, new DateTime(2010, 3, 2), true);
			AddClientOrgImportHistory(org3, new DateTime(2010, 3, 3));
			AddClientOrgImportHistory(org3, new DateTime(2010, 3, 4));
			var firstImportHistoryForOrg4 = AddClientOrgImportHistory(org4, new DateTime(2010, 6, 5), true);
			AddClientOrgImportHistory(org4, new DateTime(2010, 3, 6));
			AddClientOrgImportHistory(org4, new DateTime(2010, 3, 7));
			var recentImportHistoryForOrg41 = AddClientOrgImportHistory(org4, new DateTime(2010, 6, 8));
			var recentImportHistoryForOrg42 = AddClientOrgImportHistory(org4, new DateTime(2010, 6, 9));

			AssertEquals(13, GetTableRowCount("ClientOrgImportHistory"));

			var task = new ScavengingPurgeServiceTask();
			InitialiseTaskSchedule(task);
			task.RunTask();

			AssertEquals(6, GetTableRowCount("ClientOrgImportHistory"));
			AssertEquals(@"
Information|Starting purge.
Information|0 CSA (Client Statistics XML Archive) items have been purged.
Information|0 COC (Client Consol) items have been purged.
Information|7 OIH (Client Org. Import History) items have been purged.
Information|Finished purge.
".TrimStart(), task.ServiceLogger.ToString());

			AssertNotNull(Factory.Load<ClientOrgImportHistory>(firstImportHistoryForOrg1));
			AssertNotNull(Factory.Load<ClientOrgImportHistory>(firstImportHistoryForOrg2));
			AssertNotNull(Factory.Load<ClientOrgImportHistory>(firstImportHistoryForOrg3));
			AssertNotNull(Factory.Load<ClientOrgImportHistory>(firstImportHistoryForOrg4));
			AssertNotNull(Factory.Load<ClientOrgImportHistory>(recentImportHistoryForOrg41));
			AssertNotNull(Factory.Load<ClientOrgImportHistory>(recentImportHistoryForOrg42));

			Db.Connection.BeginTransaction();
		}

		public void TestMinimumPeriod()
		{
			AssertEquals("1day", GetHostedServiceAttributes().Single().MinimumPeriod);
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();

		#region Implementation

		static void AddClientStatisticsXMLArchive(DateTime insertUTC)
		{
			const string sql = @"
INSERT INTO dbo.ClientStatisticsXMLArchive ([IMA_PK], [IMA_ClientID], [IMA_MessageTrackingID], [IMA_MessageType], [IMA_ApplicationCode], [IMA_InsertUTC], [IMA_Content])
VALUES (newid(), 'ABCDEFXYZ', newid(), 'http://www.cargowise.com/Schemas/Native', 'SCV', @insertUTC, 'Test')";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("insertUTC", SqlDbType.DateTime, insertUTC);
				command.ExecuteNonQuery();
			}
		}

		void AddClientOrgConsol(DateTime exportUTC, int index)
		{
			var org = Factory.New<OrgHeader>();
			org.OH_FullName = "Purge Test Org " + index;
			org.OH_Code = "P_TEST_" + index;
			Factory.Save();

			const string sql = @"
INSERT INTO dbo.ClientOrgConsol ([O7_PK], [O7_ExportUTC], [O7_OH], [O7_AgentType], [O7_TransMode], [O7_LoadPort], [O7_DischargePort], [O7_TotalWeight], [O7_TotalVolume], [O7_ShipCount], [O7_ClientKey], [O7_CreateUTC])
VALUES (newid(), @exportUTC, @orgPK, 'AGENT_TYPE', 'TRM', 'AUSYD', 'USCHI', 100, 1000, 5, 'ABCDEFXYZ', getdate())";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("exportUTC", SqlDbType.DateTime, exportUTC);
				command.AddParameter("orgPK", SqlDbType.UniqueIdentifier, org.PK.ToGuid());
				command.ExecuteNonQuery();
			}
		}

		Guid AddClientOrgImportHistory(OrgHeader org, DateTime insertUTC, bool isNew = false)
		{
			var sql = $@"
INSERT INTO dbo.ClientOrgImportHistory ([O2_PK], [O2_ClientKey], [O2_OrgCode], [O2_TargetOrgPK], [O2_ImportedDate], [O2_ActionType], [O2_MatchCount])
VALUES (@pk, 'ABCDEFXYZ', @orgCode, @orgPK, @insertUTC, '{(isNew ? "CreateNew" : "Merge (Native)")}', 1)";
			var pk = Guid.NewGuid();
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("orgCode", SqlDbType.NVarChar, org.OH_Code.ToString());
				command.AddParameter("orgPK", SqlDbType.UniqueIdentifier, org.PK.ToGuid());
				command.AddParameter("insertUTC", SqlDbType.DateTime, insertUTC);
				command.ExecuteNonQuery();
			}
			return pk;
		}
		OrgHeader CreateTestOrg(int index)
		{
			var org = Factory.New<OrgHeader>();
			org.OH_FullName = "Purge Test Org " + index;
			org.OH_Code = "P_TEST_" + index;
			Factory.Save();

			return org;
		}

		static int GetTableRowCount(string tableName)
		{
			using (var command = Db.Connection.Command(string.Format("select COUNT(*) from {0}", tableName)))
			{
				var rowCount = command.ExecuteScalar();
				return Convert.ToInt32(rowCount);
			}
		}

		#endregion // Implementation
	}
}
