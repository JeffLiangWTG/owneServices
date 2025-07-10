using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using CargoWise.Data;
using Enterprise.DataTools.DbBackupAndRestore.Business;
using NUnit.Framework;
using static System.FormattableString;

namespace Enterprise.DataTools.DbBackupAndRestore.Testing.Restore
{
	public class TestDataHelpers : Assertion
	{
		/// <summary>
		/// Copy an embedded resource from the Restore/TestFiles folder to the given folder.
		/// The embedded resource is the file name only, e.g., "testProdCopy.bak".
		/// Returns the full path of the copy.
		/// </summary>
		public static string CopyTestFileResource(string toFolder, string fileName)
			=> CopyAndNameTestFileResource(toFolder, fileName, fileName);

		/// <summary>
		/// Get all files from the db backup file with the specified target directory
		/// </summary>
		public static DbFileInfoCollection GetAllFilesFromBackupWithTargetDirectory(DbRestoreManager restoreManager, string backupFilePath, string targetDirectory)
		{
			var dbFileInfos = restoreManager.GetBackupDbFileInfoCollection(Db.ServerName, backupFilePath, null, null, null, null);
			foreach (DbFileInfo dbFileInfo in dbFileInfos)
			{
				dbFileInfo.FolderPath = targetDirectory;
				dbFileInfo.FolderPathView = targetDirectory;
			}
			return dbFileInfos;
		}

		/// <summary>
		/// Same as CopyTestFileResource, but the copy can be given a new name.
		/// </summary>
		public static string CopyAndNameTestFileResource(string toFolder, string resourceFileName, string newFileName)
		{
			var filePath = Path.Combine(toFolder, newFileName);
			SaveResourceToFile("Enterprise.DataTools.DbBackupAndRestore.Business.Testing.Restore.TestFiles." + resourceFileName, filePath);
			return filePath;
		}

		/// <summary>
		/// Save an embedded resource in this assembly to the given file path.
		/// The resource name is the full name ({namespace}.{folder-names}.{file-name})
		/// </summary>
		public static void SaveResourceToFile(string resourceName, string filePath)
		{
			using (var fileStream = File.Create(filePath))
			{
				var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName);
				Assert("embedded resource should exist: " + resourceName, stream != null);
				stream.CopyTo(fileStream);
			}
		}

		public static Dictionary<string, bool> PreconditionRegistryItems(DbConnection connection, string testDbName, IEnumerable<string> existRegItemNames)
		{
			var random = new Random();
			var preConditionedItems = new Dictionary<string, bool>(lazyPreconditionedRegistryItems.Value);
			existRegItemNames.ToList().ForEach(x =>
			{
				if (preConditionedItems.ContainsKey(x))
				{
					preConditionedItems.Remove(x);
				}

				preConditionedItems.Add(x, random.Next(0, existRegItemNames.Count() - 1) % 2 == 0);
			});

			foreach (var item in preConditionedItems)
			{
				connection.ExecuteNonQuery($"DELETE {testDbName}..StmData WHERE SD_Name = '{item.Key}'");
				InsertStmData(connection, testDbName, item.Key, PreservedTestValue, item.Value);
			}

			return preConditionedItems;
		}

		public static void AssertPreservedRegistryItems(DbConnection connection, string testDbName, IEnumerable<KeyValuePair<string, bool>> preConditionedItems)
		{
			CombineAssertions("The registry items must be the same as set by PreconditionRegistryItems method", () =>
			{
				var registryItems = preConditionedItems.Where(x => x.Value).Select(x => x.Key);
				var registryValues = new List<(string Name, string Value)>();
				registryItems.ToList().ForEach(name =>
				{
					registryValues.Add((name, GetStmData(connection, testDbName, name)));
				});

				Assert(registryValues.All(x => x.Value == PreservedTestValue));
			});
		}

		public static void SetDatabaseAsForTest(DbConnection connection, string testDbName)
		{
			UpdateStmData(connection, testDbName, FreightNotesHeaderLength, TestRegistrationKey);
		}

		public static void RecreateSysRegistrationKey(AdminConnection connection, string dbName, string hostedLocation)
		{
			connection.ExecuteNonQuery($"DELETE {dbName}..StmData WHERE SD_Name = '{LicenceBuilder.SysKeyRegItem}'");
			new LicenceBuilder().CreateSystemRegistrationKeyForNewTestSystem(connection, dbName, hostedLocation);
		}

		#region SQL Insert

		internal static void InsertStmData(DbConnection connection, string dbName, string name, string value, bool preserveTestValue = false)
		{
			const string insertStmData = @"
INSERT INTO [{0}].[dbo].[StmData]
(SD_Name, SD_BinaryValue, SD_PreserveTestValue)
VALUES ('{1}',convert(varbinary(max), N'{2}'), @PreserveTestValue)
";

			var insertCommand = string.Format(insertStmData, dbName, name, value);
			using (var command = connection.Command(insertCommand))
			{
				command.AddParameter("@PreserveTestValue", SqlDbType.Bit, preserveTestValue);
				command.ExecuteNonQuery();
			}
		}

		#endregion

		#region SQL Update

		static void UpdateStmData(DbConnection connection, string dbName, string name, string value)
		{
			const string updateStmData = @"
UPDATE [{0}].[dbo].[StmData]
SET SD_BinaryValue = convert(varbinary(max), N'{1}')
WHERE SD_Name = '{2}'
";

			var updateCommand = string.Format(updateStmData, dbName, value, name);
			connection.Command(updateCommand).ExecuteNonQuery();
		}

		#endregion

		#region SQL Get

		internal static Dictionary<string, string> GetStmData(DbConnection connection, string dbName)
		{
			var stmData = new Dictionary<string, string>();
			var selectCommand = Invariant($@"
SELECT SD_Name, CONVERT(nvarchar(max), SD_BinaryValue) SD_Value
FROM [{dbName}].[dbo].[StmData]
WHERE SD_Type NOT IN ('', 'BIN')
");
			using (var reader = connection.Command(selectCommand).ExecuteReader())
			{
				while (reader.Read())
				{
					stmData.Add((string)reader[0], reader[1].ToString());
				}
			}

			return stmData;
		}

		static string GetStmData(DbConnection connection, string dbName, string name)
		{
			var selectStmData = $@"
SELECT TOP 1 convert(nvarchar(max), SD_BinaryValue) as textvalue
FROM [{dbName}].[dbo].[StmData]
WHERE SD_Name = '{name}'
";

			return connection.Command(selectStmData).ExecuteScalar()?.ToString().Trim();
		}

		#endregion

		#region Test Data

		[SuppressMessage("CargoWiseOne", "CW1043:WordSpellingRule", Justification = "RegistryItemKeys")]
		[SuppressMessage("CargoWiseOne", "CW1021")]
		static readonly Lazy<Dictionary<string, bool>> lazyPreconditionedRegistryItems = new Lazy<Dictionary<string, bool>>(() =>
			new Dictionary<string, bool>
			{
				{ "ACDataImportDirectory", true },
				{ "ADConfig", true },
				{ "AIRLINEFHLMESSAGESRECIPIENTCONFIGURATION", true },
				{ "AIRLINEFWBMESSAGESRECIPIENTCONFIGURATION", true },
				{ "ARAPBalancesUpdateImportDirectoryItem", true },
				{ "AUCCompanyCertificateData", true },
				{ "AUSUPPRESSCONTRLACKNOWLEDGEMENTS", true },
				{ "AUSUPPRESSRESENDS", true },
				{ "BackupFilePath", true },
				{ "BiAuditServer", true },
				{ "BiDataWarehouseServer", true },
				{ "BIRDImportBackupDirectory", true },
				{ "BIRDImportDirectory", true },
				{ "CANSENDCARGOIMPMESSAGESTHROUGHEADAPTOR", true },
				{ "ConsolsDataImportDirectory", true },
				{ "CustomDomainCredentials", true },
				{ "DatabaseAuthenticationMode", true },
				{ "DataLoadingModuleOutputDirectory", true },
				{ "DATE2012_11NAMESPACEANDFORMATKICKSIN", true },
				{ "DocManagerDataFileSizeThresholdGb", true },
				{ "DocManagerDBDataFilePath", true },
				{ "DocManagerDBLogFilePath", true },
				{ "DomainCredentialsCollection", true },
				{ "DOTNET_VERSION|PC01", false },
				{ "DOTNET_VERSION|PC02", false },
				{ "DOTNET_VERSION|SRV68", false },
				{ "DOTNET_VERSION|SRV79", false },
				{ "EADAPTORINBOUNDAUTHENTICATIONS", true },
				{ "EADAPTOROUTBOUNDPASSWORD", true },
				{ "ediTariffInstallationDirectory", true },
				{ "EHUBERRORSNOTIFICATIONGROUP", true },
				{ "EHUBGATEWAYSERVERADDRESS", true },
				{ FreightNotesHeaderLength, false },
				{ "GLOWENTERPRISESERVICESROOTURI", true },
				{ "HASINTERFACECONNECTOR", true },
				{ "HASNATIVEXMLCONNECTOR", true },
				{ "ImporterSecurityFilingDataImportDirectory", true },
				{ "INBOUNDADAPTERSERVICEURL", true },
				{ "INBOUNDMESSAGEDISCARDEDNOTIFICATIONGROUP", true },
				{ "INBOUNDMESSAGEPROCESSEDOKNOTIFICATIONCONFIGURATION", true },
				{ "INBOUNDMESSAGEPROCESSEDWITHERRORSNOTIFICATIONCONFIGURATION", true },
				{ "INBOUNDMESSAGEPROCESSEDWITHWARNINGSNOTIFICATIONCONFIGURATION", true },
				{ "INBOUNDMESSAGEREJECTEDNOTIFICATIONGROUP", true },
				{ "MailboxEmailAddress", true },
				{ "MailboxUserName", true },
				{ "MailboxDisplayName", true },
				{ "OUTBOUNDADAPTERSERVICEURL", true },
				{ "PhysicalServerID", true },
				{ "Physical_System_MailServer", true },
				{ "ProductsXMLDataImportDirectory", true },
				{ "PURGESETTINGS", true },
				{ "ReportingDBServerName", true },
				{ "ReportingDbServerThreshold", false },
				{ "SCAVENGINGTASKSETTINGS", true },
				{ "SENDCAVIAEHUB", true },
				{ "SENDCBPVIAEHUB", true },
				{ "SENDHKISACEVIAEHUB", true },
				{ "SENDNZCUSCARVIAEHUB", true },
				{ "SENDNZCUSDECVIAEHUB", true },
				{ "ServiceManagerTaskABC", true },
				{ "ServiceManagerTaskABI", true },
				{ "ServiceManagerTaskACC", true },
				{ "ServiceTaskUnloadTimeoutInMinutes", true },
				{ "SystemLogBatchProcessHighWaterMark", false },
				{ "UNIVERSALXMLALWAYSINCLUDEJOBCOSTINGINUNIVERSALSHIPMENT", true },
				{ "UNIVERSALXMLENABLEVERBOSELOGGING", true },
				{ "UNIVERSALXMLUPDATECONSOLCONTAINERSDURINGAUTOMATICIMPORT", true },
				{ "UNIVERSALXMLUPDATECONSOLDURINGAUTOMATICIMPORT", true },
				{ "UNIVERSALXMLUPDATECONSOLROUTINGDURINGAUTOMATICIMPORT", true },
				{ "UNIVERSALXMLUPDATECONSOLSHIPMENSTDURINGAUTOMATICIMPORT", true },
				{ "UNIVERSALXMLUPDATESHIPMENTDURINGAUTOMATICIMPORT", true },
				{ "UNIVERSALXMLUSECOMBINEDREFERENCEANDPARTYIDMATCH", true },
				{ "USEBROKERAGEDATAFIRSTWHENEXPORTUNIVERSALXML", true },
				{ "UseDbBackupCompression", false },
				{ "USEDEFAULTINGOFDATAWHENIMPORTINGUNIVERSALXML", true },
				{ "UserLoginPrefix", true },
				{ "USERORGANISATIONALUNIT", true },
				{ "WarehouseIFSDataImportDirectory", true },
				{ "WebAdminUserLogin", true },
				{ "WebAdminUserPassword", true },
				{ "WebRootPath", true },
				{ "WebServicePassword", true },
				{ "WebServiceUsername", true },
				{ "WiseCloudAccessSecurityGroup", true },
				{ "WorkflowExceptionGenerationHWM", false },
				{ "WorkflowFieldChangeTriggerHWM", false },
				{ "XMLSERVICEVERBOSELOGGING", true },
				{ "XmlSchemaValidationStrict", true },
			});

		#endregion

		const string PreservedTestValue = nameof(PreservedTestValue);
		const string FreightNotesHeaderLength = nameof(FreightNotesHeaderLength);
		const string TestRegistrationKey = "GAPLcP4y8YaMhFHYeaeNGKCfLexsSSDCx2Ylb3nUg4rc0NQbGAIJGcufyV1XoKPWW0F0Vh5qhmKsbOTxJq4cqZz9jGdkVUFUX2nLLVhvCqgwOOV8/g2MNUjgCRrmXl4AJasEcWKOEuN2/17gtipST/hvRYPIB/WNV/5CgLeBiL2kGZKSLbZjyRQK7ZLdCPVypFxNI1QI5o1GHrSik5fLeRJd/rNV0c241toy5m1V5eGCq9LQJrQ9Lp1s0wD6gOmUnTek5vzVVFV9RYyLTUHfQ89oRk9B3PLIxBtOWvNoKkmsHx9UHrCNBCi6AyFNWavrwCYYGQLUmyeZpnasnnXccy38AoE+A9DpfE/SPgMEubvtVdxrhSbG8yqRj1bkoNXzQVlfFrE21Wwx4FTlzUO0J/bY7TAs054oWSOrP/gPzXg7JzV5zi0LuRwjO07TPegenFLHL5m/JtCx2CaA/o/A2anwJplm8bT3n4fp4XHvLvbfyKe82+JxEjkOPdnpVrCoV37fZ5hv7T3LJVhgpV3M5RjSufTPjykStO7u8uMl6g5Ez0WYYDSXLZlV8kTawl4s25lanNHs0jYxz2VjEQDymZQFMArxN3pH3S1sCA+jhRMAC+UK9B7NJGpA5ollhjv6oiPerSzfa5mV6I9pdtSCelNyZTavkGFSL/MfzXsOKcf3bZ3wjAiksCJf6YyA6YDkpSY152Q5aWaPmPTn10+blc0eCk2x+aNO/s4KnhV3kBIvJAipWrhs2Cc4GTZ7J5cz3cc1qyzOoNCo/+Le8gsFxFckNMCF3H2pqG43m5cRRsQFTdrGfL+ZXMG/8wcw6GR1MN45Mc6D99ADc9cKqgvfezUWObxmc74W5F1afH4X6WSI7VgGDDelY5yQFNbgJuJbn5+DLO7Npr9xw+y6wAnPnQMzyUj7dUFEuEA6yXyhEhBu7prllJ1JminQZK4UhqlnTAYiJZGdcwPkV4TN+mNyE7UzvLnjIBGJO96IfEpMd1fmVexEjq3nPEwoYBJKmZXUGaemTwKS5+il3HDLdFVXbw==";
	}
}
