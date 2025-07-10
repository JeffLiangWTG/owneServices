using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using CargoWise.Data;
using NUnit.Framework;

namespace Enterprise.Client.EDI.ServiceTasks.ExternalMonitoringAlert.Testing
{
	class AlertRuleAndCapabilityManagerTest : TransactionedTestCase
	{
		protected override void TearDown()
		{
			var retryCount = 0;
			const int maxRetries = 3;
			while (retryCount < maxRetries)
			{
				try
				{
					AdoTestUtils.DropDbIfExists(adminConnection, mockWiseGridReportingDB);
					break;
				}
				catch (SqlException ex) when (ex.Number == 1222)
				{
					retryCount++;
					Thread.Sleep(5000);
				}
			}
			adminConnection?.Dispose();
			base.TearDown();
		}

		protected override void SetUp()
		{
			base.SetUp();
			adminConnection = Db.NewAdminConnection();
			var retryCount = 0;
			const int maxRetries = 3;
			while (retryCount < maxRetries)
			{
				try
				{
					AdoTestUtils.CreateDbIfNotExists(adminConnection, mockWiseGridReportingDB);
					break;
				}
				catch (SqlException ex) when (ex.Number == 1222)
				{
					retryCount++;
					Thread.Sleep(5000);
				}
			}

			InsertIntoAlertCapabilityTable(mockWiseGridReportingDB);
		}

		public void TestLoadAlertCapabilities()
		{
			using (((ICurrentDbControl)adminConnection).UseDatabase(mockWiseGridReportingDB))
			{
				var alertRuleAndCapabilityManager = new AlertRuleAndCapabilityManager(adminConnection);
				var alertCapabilities = alertRuleAndCapabilityManager.LoadAlertCapabilities(out var capabilityCounts);

				AssertEquals(AlertCapability_PK, alertCapabilities.First().PK);
				AssertEquals(AlertCapability_Capability, alertCapabilities.First().Capability);
				AssertEquals(AlertCapability_Product, alertCapabilities.First().Product);
				AssertEquals(AlertCapability_Module, alertCapabilities.First().Module);
				AssertEquals(AlertCapability_ProgramArea, alertCapabilities.First().ProgramArea);
				AssertEquals(AlertCapability_Priority, alertCapabilities.First().Priority);
				AssertEquals(AlertCapability_ChangeType, alertCapabilities.First().ChangeType);
				AssertEquals(AlertCapability_CapabilityId, alertCapabilities.First().CapabilityId);
				AssertEquals(AlertCapability_MaxTargets, alertCapabilities.First().MaxTargets);
			}
		}
		public static void InsertIntoAlertCapabilityTable(string dbName)
		{
			using (((ICurrentDbControl)adminConnection).UseDatabase(mockWiseGridReportingDB))
			{
				var sqlText = string.Format(CultureInfo.InvariantCulture, @"CREATE TABLE [dbo].[AlertCapability](
[AC_PK] [uniqueidentifier] NOT NULL,
[AC_Capability] [nvarchar](10) NOT NULL,
[AC_Product] [nvarchar](10) NOT NULL,
[AC_Module] [nvarchar](10) NOT NULL,
[AC_CapabilityId] [uniqueidentifier] NULL,
[AC_ProgramArea] [nvarchar](10) NULL,
[AC_MaxTargets] [int] NULL,
[AC_Priority] [nvarchar](10) NULL,
[AC_ChangeType] [nvarchar](10) NULL);
INSERT INTO [dbo].[AlertCapability] VALUES('{0}','{1}','{2}','{3}','{4}','{5}',{6},'{7}','{8}')",
				AlertCapability_PK,
				AlertCapability_Capability,
				AlertCapability_Product,
				AlertCapability_Module,
				AlertCapability_CapabilityId,
				AlertCapability_ProgramArea,
				AlertCapability_MaxTargets,
				AlertCapability_Priority,
				AlertCapability_ChangeType
				);
				using (var cmd = adminConnection.Command(sqlText))
				{
					cmd.ExecuteNonQuery();
				}
			}
		}

		internal static readonly Guid AlertCapability_PK = Guid.NewGuid();
		internal static readonly Guid AlertCapability_CapabilityId = Guid.NewGuid();
		internal const string AlertCapability_Capability = "DBH";
		internal const string AlertCapability_Product = "ENT";
		internal const string AlertCapability_Module = "OCH";
		internal const string AlertCapability_ProgramArea = "ARC";
		internal const string AlertCapability_Priority = "High";
		internal const string AlertCapability_ChangeType = "Test";
		internal const int AlertCapability_MaxTargets = 5;
		internal static AdminConnection adminConnection;
		internal static readonly string mockWiseGridReportingDB = "MockWiseGridReportingDB";
	}
}
