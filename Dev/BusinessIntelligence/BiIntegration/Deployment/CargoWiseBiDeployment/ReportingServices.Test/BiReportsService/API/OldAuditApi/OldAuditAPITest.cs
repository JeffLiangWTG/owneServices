using System;
using CargoWise.Bi.Common;
using CargoWise.Bi.Deployment.ReportingServices.API.OldAuditApi;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.Types;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace CargoWise.Bi.Deployment.ReportingServices.Testing
{
	public class OldAuditAPITest : TestCase
	{
		[UseSnapshotProtection]
		public void TestAuditServerValue()
		{
			try
			{
				using (var mainDbConnection = Db.NewAdminConnection())
				{
					BiServers.ClearBiServersCache();
					SystemDataRegistry.Instance.BiAuditAPI.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
					SystemDataRegistry.Instance.BiAuditServer.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "SomeAuditServer");
					AssertEquals("SomeAuditServer", OldAuditAPI.Instance.AuditServer);
				}
			}
			finally
			{
				BiServers.ClearBiServersCache();
			}
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.Audit })]
		public void TestBatchSize()
		{
			try
			{
				BiServers.ClearBiServersCache();
				SystemDataRegistry.Instance.BiAuditAPI.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				SystemDataRegistry.Instance.BiAuditServer.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Db.ServerName);
				using (var auditConnection = Db.NewAdminConnection(Db.AuditDatabaseName))
				{
					BiMasterState.SetParameter(auditConnection, "ASP_BATCH_SIZE", "4321");
				}
				AssertEquals(4321, OldAuditAPI.Instance.APIBatchSize);
			}
			finally
			{
				BiServers.ClearBiServersCache();
			}
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main, DatabaseType.Audit })]
		public void TestGetCdcHistorySummaryDetails()
		{
			try
			{
				using (var mainDbConnection = Db.NewAdminConnection())
				{
					SystemDataRegistry.Instance.BiAuditAPI.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
					SystemDataRegistry.Instance.BiAuditServer.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Db.ServerName);

					AssertNoExceptionThrown("Should not throw an exception when start and end dates are in the same month and year", () =>
					{
						OldAuditAPI.Instance.GetCdcHistorySummaryDetails(new ZDateTime(2021, 4, 15), new ZDateTime(2021, 4, 16));
					});

					AssertNoExceptionThrown("Should not throw an exception when start and end dates are in different months", () =>
					{
						OldAuditAPI.Instance.GetCdcHistorySummaryDetails(new ZDateTime(2021, 4, 15), new ZDateTime(2021, 5, 15));
					});

					AssertNoExceptionThrown("Should not throw an exception when start and end dates are in different years", () =>
					{
						OldAuditAPI.Instance.GetCdcHistorySummaryDetails(new ZDateTime(2021, 4, 15), new ZDateTime(2022, 4, 15));
					});
				}
			}
			finally
			{
				BiServers.ClearBiServersCache();
			}
		}

		[UseSnapshotProtection(new[] { DatabaseType.Main })]
		public void TestGetCdcHistorySummaryDetailsWithInvalidDateRange()
		{
			try
			{
				using (var mainDbConnection = Db.NewAdminConnection())
				{
					SystemDataRegistry.Instance.BiAuditAPI.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
					SystemDataRegistry.Instance.BiAuditServer.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Db.ServerName);

					AssertExceptionThrown<AuditAPIException>("Should throw exception when start and end dates are not in the same year",
						"to_time must be greater than from_time.",
						() =>
						{
							OldAuditAPI.Instance.GetCdcHistorySummaryDetails(utcFromDateTime: new ZDateTime(2021, 4, 15), utcToDateTime: new ZDateTime(2021, 4, 14));
						});
				}
			}
			finally
			{
				BiServers.ClearBiServersCache();
			}
		}
	}
}
