using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Bi.Common;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using Enterprise.DbUpgrader.Resource.Version;
using Enterprise.Upgrades;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Moq;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Startup.Testing
{
	[UseSnapshotProtection]
	sealed class UpgradeManagerConclusionTest : TestCase
	{
		[RequiresSTA]
		public void TestFailedToDropExceptionIsLoggedAndReportedWhenModernSecurityIsOff()
		{
			// Arrange
			EnvProxy.Instance.Registry.UseModernSqlSecuritySystem = false;

			var loginName = $"EnterpriseDbUser_{Db.DatabaseName}_{nameof(UpgradeManagerConclusionTest)}";
			var expectedLogMessage = $"Failed to drop login \"{loginName}\". Server: [{Db.ServerName}]";
			var createLoginScript = $@"
declare @sql nvarchar(max)

SET @sql = N'use [master]
CREATE LOGIN [{loginName}] WITH PASSWORD = '''', CHECK_POLICY = OFF;
DROP DATABASE IF EXISTS [30E86203-9D24-4BEB-B053-AE10AE29567D];
CREATE DATABASE [30E86203-9D24-4BEB-B053-AE10AE29567D];'

EXECUTE sp_executesql @sql

SET @sql = N'
USE [30E86203-9D24-4BEB-B053-AE10AE29567D]
EXECUTE sp_changedbowner [{loginName}]'

EXECUTE sp_executesql @sql
";
			var dropLoginScript = $@"
DROP DATABASE IF EXISTS [30E86203-9D24-4BEB-B053-AE10AE29567D]

if (EXISTS (SELECT NULL FROM sys.server_principals WHERE name = '{loginName}'))
begin
	DROP LOGIN {loginName.QuoteName()};
end
";

			var dbReferenceVersion = new VersionLabel(3680, 3864); // see schemaVersionForBiServersRegistrySplit and schemaVersionForAuditServerRegistrySplit
			var dummyVersion = new VersionLabel(0, 0);
			var versionChangeInfo = Mock.Of<IVersionChangeInfo>(x =>
				!x.IsRequired_ClientDocuments &&
				!x.IsRequired_Data &&
				!x.IsRequired_Schema &&
				!x.IsRequired_Script &&
				!x.IsRequired_Transformation &&
				x.DbReferenceVersion_Schema == dbReferenceVersion &&
				x.DbReferenceVersion_Data == dummyVersion &&
				x.DbReferenceVersion_Script == dummyVersion &&
				x.DbReferenceVersion_Transformation == dummyVersion &&
				x.DbReferenceVersion_Clr == dummyVersion);

			var upgradeInfo = new UpgradeInfo(Guid.NewGuid(), ReleaseInfo.Instance.VersionNumber.ToVersion());
			PackageHelper.InsertUpgradePackage(upgradeInfo.PK, upgradeInfo.Version, "RDY");
			var upgradeManagerMock = new Mock<UpgradeManager>(versionChangeInfo, upgradeInfo) { CallBase = true };
			upgradeManagerMock.Setup(x => x.IsHosted).Returns(false);
			var upgrader = upgradeManagerMock.Object;

			var upgradeLogs = new List<string>();
			upgrader.UpgradeEvent += (_, message) => upgradeLogs.Add(message);
			upgrader.OnErrorWithRetry += message =>
			{
				upgradeLogs.Add(message);
				return true;
			};

			try
			{
				SoftwareUpgrader.OverridableInstance.Value = Mock.Of<SoftwareUpgrader>();
				using (var adminConnection = Db.NewAdminConnection())
				{
					adminConnection.ExecuteNonQuery(createLoginScript);
				}

				// Act
				using (BiServers.TemporarilySetAuditServerToNull())
				using (BiServers.TemporarilySetDataWarehouseServerToNull())
				{
					upgrader.Run();
				}

				// Assert
				CombineAssertions(string.Join(System.Environment.NewLine, upgradeLogs), () =>
				{
					AssertEquals($"ErrorReporter.LastMessageReported： {ErrorReporter.LastMessageReported}", expectedLogMessage, ErrorReporter.LastMessageReported);
					AssertEquals(1, ErrorReporter.TotalErrorCount);
					AssertEquals(1, upgradeLogs.Count(x => x.StartsWith(expectedLogMessage)));
				});
			}
			finally
			{
				Db.NewAdminConnection().ExecuteNonQuery(dropLoginScript);
				SoftwareUpgrader.OverridableInstance.ResetValue();
				ErrorReporter.Clear();
			}
		}

		[UseSnapshotProtection]
		public void TestFailedToDropExceptionIsLoggedAndReportedWhenModernSecurityIsOn()
		{
			// Arrange
			EnvProxy.Instance.Registry.UseModernSqlSecuritySystem = true;
			var loginName = $"EnterpriseDbUser_{Db.DatabaseName}_{nameof(UpgradeManagerConclusionTest)}";
			var expectedLogMessage1 = $"Login '{loginName}' owns one or more database(s). Change the owner of the database(s) before dropping the login.";

			var createLoginScript = $@"
declare @sql nvarchar(max)

SET @sql = N'use [master]
CREATE LOGIN [{loginName}] WITH PASSWORD = '''', CHECK_POLICY = OFF;
DROP DATABASE IF EXISTS [30E86203-9D24-4BEB-B053-AE10AE29567D];
CREATE DATABASE [30E86203-9D24-4BEB-B053-AE10AE29567D];'

EXECUTE sp_executesql @sql

SET @sql = N'
USE [30E86203-9D24-4BEB-B053-AE10AE29567D]
EXECUTE sp_changedbowner [{loginName}]'

EXECUTE sp_executesql @sql
";
			var dropLoginScript = $@"
DROP DATABASE IF EXISTS [30E86203-9D24-4BEB-B053-AE10AE29567D]

if (EXISTS (SELECT NULL FROM sys.server_principals WHERE name = '{loginName}'))
begin
	DROP LOGIN {loginName.QuoteName()};
end
";

			var dbReferenceVersion = new VersionLabel(3680, 3864); // see schemaVersionForBiServersRegistrySplit and schemaVersionForAuditServerRegistrySplit
			var dummyVersion = new VersionLabel(0, 0);
			var versionChangeInfo = Mock.Of<IVersionChangeInfo>(x =>
				!x.IsRequired_ClientDocuments &&
				!x.IsRequired_Data &&
				!x.IsRequired_Schema &&
				!x.IsRequired_Script &&
				!x.IsRequired_Transformation &&
				x.DbReferenceVersion_Schema == dbReferenceVersion &&
				x.DbReferenceVersion_Data == dummyVersion &&
				x.DbReferenceVersion_Script == dummyVersion &&
				x.DbReferenceVersion_Transformation == dummyVersion &&
				x.DbReferenceVersion_Clr == dummyVersion);

			var upgradeInfo = new UpgradeInfo(Guid.NewGuid(), ReleaseInfo.Instance.VersionNumber.ToVersion());
			PackageHelper.InsertUpgradePackage(upgradeInfo.PK, upgradeInfo.Version, "RDY");
			var upgradeManagerMock = new Mock<UpgradeManager>(versionChangeInfo, upgradeInfo) { CallBase = true };
			upgradeManagerMock.Setup(x => x.IsHosted).Returns(false);
			var upgrader = upgradeManagerMock.Object;

			var upgradeLogs = new List<string>();
			upgrader.UpgradeEvent += (_, message) => upgradeLogs.Add(message);
			upgrader.OnErrorWithRetry += message =>
			{
				upgradeLogs.Add(message);
				return true;
			};

			try
			{
				SoftwareUpgrader.OverridableInstance.Value = Mock.Of<SoftwareUpgrader>();
				using (var adminConnection = Db.NewAdminConnection())
				{
					adminConnection.ExecuteNonQuery(createLoginScript);
				}

				// Act
				using (BiServers.TemporarilySetAuditServerToNull())
				using (BiServers.TemporarilySetDataWarehouseServerToNull())
				{
					upgrader.Run();
				}

				// Assert
				CombineAssertions(string.Join(System.Environment.NewLine, upgradeLogs), () =>
				{
					Assert(
						$"ErrorReporter.LastMessageReported '{ErrorReporter.LastMessageReported}' should contain '{expectedLogMessage1}'",
						ErrorReporter.LastMessageReported.Contains(expectedLogMessage1));

					AssertEquals(1, upgradeLogs.Count(x => x.Contains(expectedLogMessage1)));
				});
			}
			finally
			{
				Db.NewAdminConnection().ExecuteNonQuery(dropLoginScript);
				SoftwareUpgrader.OverridableInstance.ResetValue();
				ErrorReporter.Clear();
			}
		}
	}
}
