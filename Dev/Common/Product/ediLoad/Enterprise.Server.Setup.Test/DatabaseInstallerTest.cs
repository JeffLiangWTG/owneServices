using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.DataProtection.Administration.SqlServer;
using CargoWise.Loader.Common;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace Enterprise.Server.Setup.Testing
{
	class DatabaseInstallerTest : TestCase
	{
		CargowiseSetupSqlContextManager sqlContextManager;
		protected override void MasterSetUp()
		{
			base.MasterSetUp();
			Application.ConfigureApplicationServices();
		}
		protected override void SetUp()
		{
			base.SetUp();
			sqlContextManager = (CargowiseSetupSqlContextManager)Application.ServiceProvider.GetRequiredService<IProtectedDataAdministrationSqlExecutionContextManager>();
		}

		public void TestNeedsToInstall()
		{
			SetupConfiguration config = new SetupConfiguration();
			DatabaseInstaller dbInstaller = new DatabaseInstaller(new Installation(config), new InstallationSettings(config));
			Assert("Part of setup so it should always return true", dbInstaller.NeedsToInstall());
		}

		[ExpectNoExceptions]
		public void TestEndToEnd()
		{
			SetupConfiguration config = new SetupConfiguration();
			config.InstallationSettings.DbName = "DatabaseInstallerTest" + Guid.NewGuid().ToString();
			config.InstallationSettings.UseDefaultSQLServerMachineName = false;
			var dbInstaller = new DatabaseInstaller_ForTest(new Installation(config), config.InstallationSettings);
			try
			{
				var installResult = dbInstaller.InstallExcludingDependenciesForTest();
				AssertEquals(installResult.Message, InstallationResultStatus.OK, installResult.Status);
				AssertCreateMainDatabase(config.InstallationSettings.DbName);
			}
			finally
			{
				dbInstaller.DropInstalledDB();
			}
		}

		public void TestOtherSysAdminUsers()
		{
			// Arrange
			SetupConfiguration config = new SetupConfiguration();
			config.InstallationSettings.UseDefaultSQLServerMachineName = false;
			var dbInstaller = new DatabaseInstaller_ForTest(new Installation(config), config.InstallationSettings);

			var specialPrincipals = new[]
			{
				"public",
				"sysadmin",
				"securityadmin",
				"serveradmin",
				"setupadmin",
				"processadmin",
				"diskadmin",
				"dbcreator",
				"bulkadmin",
				"##MS_SQLResourceSigningCertificate##",
				"##MS_SQLReplicationSigningCertificate##",
				"##MS_SQLAuthenticatorCertificate##",
				"##MS_PolicySigningCertificate##",
				"##MS_SmoExtendedSigningCertificate##",
				"##MS_AgentSigningCertificate##",
				"##MS_PolicyTsqlExecutionLogin##",
				"##MS_PolicyEventProcessingLogin##"
			};

			sqlContextManager.OpenConnection(Db.ServerName);
			try
			{
				// Act
				var logins = dbInstaller.GetOtherSysAdminLoginNames();

				// Assert
				Assert("Should not contain special principals", !logins.Intersect(specialPrincipals).Any());
			}
			finally
			{
				sqlContextManager.Disconnect();
			}
		}

		void AssertCreateMainDatabase(string dbName)
		{
			var expectedNumberOfColumns = 49;
			var expectedListOfInitialColumns = GetExpectedListOfColumns();
			AssertEquals("Expected number of columns on the empty database:", expectedNumberOfColumns, expectedListOfInitialColumns.Count);

			var actualListOfInitialColumns = GetActualListOfColumns(dbName);
			AssertEquals("Actual number of columns on the empty database:", expectedNumberOfColumns, actualListOfInitialColumns.Count);

			foreach (var col in actualListOfInitialColumns)
			{
				AssertEquals("Column " + col.ColumnName + " exists", true, ColumnExistOnExpectedList(col, expectedListOfInitialColumns));
			}
		}

		bool ColumnExistOnExpectedList(DbColumn col, List<DbColumn> listofColumns)
		{
			return listofColumns.Exists(x =>
			x.TableName == col.TableName &&
			x.ColumnName == col.ColumnName &&
			x.ColumnId == col.ColumnId &&
			x.ColumnType == col.ColumnType &&
			x.ColumnLength == col.ColumnLength
			);
		}

		List<DbColumn> GetActualListOfColumns(string dbName)
		{
			var list = new List<DbColumn>();

			var sqlScript = string.Format(@"

Select OBJECT_NAME(object_id) TableName, name, column_id, user_type_id, max_length
From [{0}].sys.columns
Where object_id in (Select object_id From [{0}].sys.tables Where name in ('StmData', 'GlbStaff', 'StmUpgrade'))
", dbName);
			using (var connection = Db.NewAdminConnection(dbName))
			{
				var data = DataUtils.GetDataTableFromQuery(connection, sqlScript);

				foreach (DataRow row in data.Rows)
				{
					var rowItemArray = row.ItemArray;
					var v1 = (string)rowItemArray.GetValue(0); //TableName
					var v2 = (string)rowItemArray.GetValue(1); //Column Name
					var v3 = (int)rowItemArray.GetValue(2);        //Column Id
					var v4 = (int)rowItemArray.GetValue(3);        //Column Type
					var v5 = (short)rowItemArray.GetValue(4);      //Column Length

					list.Add(new DbColumn(v1, v2, v3, v4, v5));
				}
			}
			return list;
		}

		List<DbColumn> GetExpectedListOfColumns()
		{
			var list = new List<DbColumn>();

			list.Add(new DbColumn("StmData", "SD_PK", 1, 36, 16));
			list.Add(new DbColumn("StmData", "SD_Name", 2, 167, 300));
			list.Add(new DbColumn("StmData", "SD_Owner", 3, 36, 16));
			list.Add(new DbColumn("StmData", "SD_DepartmentGuid", 4, 36, 16));
			list.Add(new DbColumn("StmData", "SD_Type", 5, 175, 3));
			list.Add(new DbColumn("StmData", "SD_BinaryValue", 6, 165, -1));
			list.Add(new DbColumn("StmData", "SD_GuidValue", 7, 36, 16));
			list.Add(new DbColumn("StmData", "SD_PreserveTestValue", 8, 104, 1));
			list.Add(new DbColumn("StmData", "SD_IsLogged", 9, 104, 1));
			list.Add(new DbColumn("StmData", "SD_IsCancelled", 10, 104, 1));
			list.Add(new DbColumn("StmData", "SD_SystemCreateTimeUtc", 11, 58, 4));
			list.Add(new DbColumn("StmData", "SD_SystemCreateUser", 12, 167, 3));
			list.Add(new DbColumn("StmData", "SD_SystemLastEditTimeUtc", 13, 58, 4));
			list.Add(new DbColumn("StmData", "SD_SystemLastEditUser", 14, 167, 3));

			list.Add(new DbColumn("GlbStaff", "GS_PK", 1, 36, 16));
			list.Add(new DbColumn("GlbStaff", "GS_Code", 2, 175, 3));
			list.Add(new DbColumn("GlbStaff", "GS_IsActive", 3, 175, 1));
			list.Add(new DbColumn("GlbStaff", "GS_LoginName", 4, 167, 35));
			list.Add(new DbColumn("GlbStaff", "GS_IsController", 5, 175, 1));
			list.Add(new DbColumn("GlbStaff", "GS_IsSystemAccount", 6, 175, 1));
			list.Add(new DbColumn("GlbStaff", "GS_ChangePasswordAtNextLogin", 7, 175, 1));
			list.Add(new DbColumn("GlbStaff", "GS_IsOperational", 8, 104, 1));
			list.Add(new DbColumn("GlbStaff", "GS_PasswordHash", 9, 165, 20));
			list.Add(new DbColumn("GlbStaff", "GS_PasswordHashIterations", 10, 56, 4));
			list.Add(new DbColumn("GlbStaff", "GS_PasswordSalt", 11, 165, 16));

			list.Add(new DbColumn("StmUpgrade", "SZ_PK", 1, 36, 16));
			list.Add(new DbColumn("StmUpgrade", "SZ_Type", 2, 167, 3));
			list.Add(new DbColumn("StmUpgrade", "SZ_ExeVersionDate", 3, 58, 4));
			list.Add(new DbColumn("StmUpgrade", "SZ_MajorVersion", 4, 56, 4));
			list.Add(new DbColumn("StmUpgrade", "SZ_MinorVersion", 5, 56, 4));
			list.Add(new DbColumn("StmUpgrade", "SZ_Release", 6, 56, 4));
			list.Add(new DbColumn("StmUpgrade", "SZ_Patch", 7, 56, 4));
			list.Add(new DbColumn("StmUpgrade", "SZ_MajorDataVersion", 8, 56, 4));
			list.Add(new DbColumn("StmUpgrade", "SZ_MinorDataVersion", 9, 56, 4));
			list.Add(new DbColumn("StmUpgrade", "SZ_WorkingVersion", 10, 175, 1));
			list.Add(new DbColumn("StmUpgrade", "SZ_UpgradeNotes", 11, 167, -1));
			list.Add(new DbColumn("StmUpgrade", "SZ_UpgradeData_Compressed", 12, 165, -1));
			list.Add(new DbColumn("StmUpgrade", "SZ_SplitCount", 13, 52, 2));
			list.Add(new DbColumn("StmUpgrade", "SZ_SplitLastItem", 14, 175, 1));
			list.Add(new DbColumn("StmUpgrade", "SZ_Reference", 15, 167, 128));
			list.Add(new DbColumn("StmUpgrade", "SZ_Cancelled", 16, 175, 1));
			list.Add(new DbColumn("StmUpgrade", "SZ_CancelledReason", 17, 167, 128));
			list.Add(new DbColumn("StmUpgrade", "SZ_Status", 18, 167, 3));
			list.Add(new DbColumn("StmUpgrade", "SZ_StatusTime", 19, 58, 4));
			list.Add(new DbColumn("StmUpgrade", "SZ_StatusComment", 20, 167, 128));
			list.Add(new DbColumn("StmUpgrade", "SZ_SystemCreateTimeUtc", 21, 58, 4));
			list.Add(new DbColumn("StmUpgrade", "SZ_SystemCreateUser", 22, 167, 3));
			list.Add(new DbColumn("StmUpgrade", "SZ_SystemLastEditTimeUtc", 23, 58, 4));
			list.Add(new DbColumn("StmUpgrade", "SZ_SystemLastEditUser", 24, 167, 3));

			return list;
		}
		/*
		 -- StmData
		CREATE TABLE [{0}].dbo.StmData
		(
			SD_PK                uniqueidentifier NOT NULL DEFAULT NEWID(),
			SD_Name              varchar(300)         NULL,
			SD_Owner             uniqueidentifier     NULL,
			SD_DepartmentGuid    uniqueidentifier     NULL,
			SD_Type              char(3)              NULL,
			SD_BinaryValue       varbinary(max)       NULL,
			SD_GuidValue         uniqueidentifier     NULL,
			SD_PreserveTestValue bit                  NULL,
			SD_IsLogged			 bit                  NULL,
		)

		-- GlbStaff - 1 Valid Login
		CREATE TABLE [{0}].dbo.GlbStaff
		(
			GS_PK                        uniqueidentifier NULL,
			GS_Code                      char(3)          NULL,
			GS_IsActive                  char(1)          NULL,
			GS_LoginName                 varchar(35)      NULL,
			GS_Password                  varchar(128)     NULL,
			GS_IsController              char(1)          NULL,
			GS_IsSystemAccount           char(1)          NULL,
			GS_ChangePasswordAtNextLogin char(1)          NULL,
			GS_IsOperational             bit              NULL,
		)

		INSERT [{0}].dbo.GlbStaff (GS_PK, GS_Code, GS_LoginName, GS_Password, GS_IsActive, GS_IsController, GS_IsSystemAccount, GS_ChangePasswordAtNextLogin, GS_IsOperational) VALUES
			('4D001790-4A73-43FB-9393-3786FB8BCF84', 'X', 'sysadmin', 'w/QI+E3lJ8Q6m5k19/atROkmSd2WoLsI9D+YHyefEYg=', 'N', 'N', 'Y', 'Y', 0)

		-- StmUpgrade
		CREATE TABLE [{0}].dbo.StmUpgrade
		(
			SZ_PK                     uniqueidentifier NOT NULL,
			SZ_Type                   varchar(3)       NOT NULL,
			SZ_ExeVersionDate         smalldatetime        NULL,
			SZ_MajorVersion           int              NOT NULL,
			SZ_MinorVersion           int              NOT NULL,
			SZ_Release                int              NOT NULL,
			SZ_Patch                  int              NOT NULL,
			SZ_MajorDataVersion       int              NOT NULL,
			SZ_MinorDataVersion       int              NOT NULL,
			SZ_WorkingVersion         char(1)          NOT NULL,
			SZ_UpgradeNotes           varchar(max)     NOT NULL,
			SZ_UpgradeData_Compressed varbinary(max)       NULL,
			SZ_SplitCount             smallint         NOT NULL,
			SZ_SplitLastItem          char(1)          NOT NULL,
			SZ_Reference              varchar(128)     NOT NULL,
			SZ_Cancelled              char(1)          NOT NULL,
			SZ_CancelledReason        varchar(128)     NOT NULL,
			SZ_Status                 varchar(3)       NOT NULL,
			SZ_StatusTime             smalldatetime        NULL,
			SZ_StatusComment          varchar(128)     NOT NULL,
			SZ_SystemCreateTimeUtc    smalldatetime        NULL,
			SZ_SystemCreateUser		  varchar(3)	   NOT NULL,
			SZ_SystemLastEditTimeUtc  smalldatetime		   NULL,
			SZ_SystemLastEditUser	  varchar(3)	   NOT NULL,
		)

		ALTER TABLE [{0}].dbo.StmUpgrade ADD
			CONSTRAINT [DF_SZ_Type]             DEFAULT 'EDP' FOR SZ_Type,
			CONSTRAINT [DF_SZ_MajorVersion]     DEFAULT 0     FOR SZ_MajorVersion,
			CONSTRAINT [DF_SZ_MinorVersion]     DEFAULT 0     FOR SZ_MinorVersion,
			CONSTRAINT [DF_SZ_Release]          DEFAULT 0     FOR SZ_Release,
			CONSTRAINT [DF_SZ_Patch]            DEFAULT 0     FOR SZ_Patch,
			CONSTRAINT [DF_SZ_MajorDataVersion] DEFAULT 0     FOR SZ_MajorDataVersion,
			CONSTRAINT [DF_SZ_MinorDataVersion] DEFAULT 0     FOR SZ_MinorDataVersion,
			CONSTRAINT [DF_SZ_WorkingVersion]   DEFAULT 'N'   FOR SZ_WorkingVersion,
			CONSTRAINT [DF_SZ_UpgradeNotes]     DEFAULT ''    FOR SZ_UpgradeNotes,
			CONSTRAINT [DF_SZ_SplitCount]       DEFAULT 0     FOR SZ_SplitCount,
			CONSTRAINT [DF_SZ_SplitLastItem]    DEFAULT 'N'   FOR SZ_SplitLastItem,
			CONSTRAINT [DF_SZ_Reference]        DEFAULT ''    FOR SZ_Reference,
			CONSTRAINT [DF_SZ_Cancelled]        DEFAULT 'N'   FOR SZ_Cancelled,
			CONSTRAINT [DF_SZ_CancelledReason]  DEFAULT ''    FOR SZ_CancelledReason,
			CONSTRAINT [DF_SZ_Status]           DEFAULT ''    FOR SZ_Status,
			CONSTRAINT [DF_SZ_StatusComment]    DEFAULT ''    FOR SZ_StatusComment

		 */
	}

	class DbColumn
	{
		public DbColumn(string tableName, string columnName, int columnId, int columnType, int columnLength)
		{
			this.TableName = tableName;
			this.ColumnName = ColumnName;
			this.ColumnId = columnId;
			this.ColumnType = columnType;
			this.ColumnLength = columnLength;
		}

		public string TableName { get; internal set; }
		public string ColumnName { get; internal set; }
		public int ColumnId { get; internal set; }
		public int ColumnType { get; internal set; }
		public int ColumnLength { get; internal set; }
	}

	class DatabaseInstaller_ForTest : DatabaseInstaller
	{
		public DatabaseInstaller_ForTest(Installation installation, InstallationSettings installationSettings)
			: base(installation, installationSettings) { }

		protected override string[] GetOtherApplicationDatabases()
		{
			return new List<string>().ToArray();
		}

		protected override string CreateDBSQL()
		{
			return string.Format(
			@"USE master
			CREATE DATABASE [{0}]", installationSettings.DbName);
		}

		public void DropInstalledDB()
		{
			string sql = string.Format(
			@"USE master
			IF EXISTS
			(
				SELECT * FROM sys.databases
				WHERE name = '{0}'
			)
			DROP DATABASE [{0}]", installationSettings.DbName);

			using (var connection = TestConnectionProvider.OpenNewConnection("master"))
			{
#pragma warning disable CW1116 // ZQuery (for simple queries), DynamicBusinessObjectCollection (for more complex queries) or CargoWise.Data.Db.Connection (for commands and other cases) implements required functionality and should always be used.
				using (var cmd = new SqlCommand(sql, connection))
#pragma warning restore CW1116
				{
					cmd.ExecuteNonQuery();
				}
			}
		}
		protected override void RestoreDatabases()
		{
			//Not tested.
		}

		protected override void RevokeSysadminFromOtherLogins()
		{
			// Do not revoke sysadmins on test machines
		}

		public InstallationSettings installationSettings_ForTest
		{
			get { return installationSettings; }
		}
	}
}
