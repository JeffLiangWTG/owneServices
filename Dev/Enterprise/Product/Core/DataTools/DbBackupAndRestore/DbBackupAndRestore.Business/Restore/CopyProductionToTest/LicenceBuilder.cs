using System;
using System.Data;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using CargoWise.Common;
using CargoWise.Data;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Encryption;

namespace Enterprise.DataTools.DbBackupAndRestore.Business
{
	public class LicenceBuilder
	{
		public void AdjustCompanyLicencesToFitTestSystem(DbConnection connection, string restoredDbName, string physicalServerId)
		{
			string sqlText = String.Format(@"
				SELECT SD_Owner, convert(nvarchar(max), SD_BinaryValue) EncryptedKey
				FROM [{0}]..StmData
				WHERE SD_Name = '{1}'
				AND SD_Owner is not null
				AND SD_DepartmentGuid is null",
				restoredDbName, LicenceRegItem);

			var licenceTable = DataUtils.GetDataTableFromQuery(connection, sqlText);

			foreach (DataRow licenceRow in licenceTable.Rows)
			{
				Guid companyPk = (Guid)licenceRow[0];
				string encryptedKey = licenceRow[1].ToString();

				if (!String.IsNullOrWhiteSpace(encryptedKey))
				{
					string testSystemEncryptedKey = null;

					try
					{
						testSystemEncryptedKey = GetTestSystemLicenceFromProductionValue(encryptedKey, physicalServerId);
					}
					catch (Exception ex) when (ex is FormatException)
					{
						testSystemEncryptedKey = null;
					}

					if (testSystemEncryptedKey != null)
					{
						sqlText = String.Format(@"
							UPDATE [{0}]..StmData SET SD_BinaryValue = convert(varbinary(max), N'{1}')
								WHERE SD_Name = '{2}'
								AND SD_Owner = '{3}'
								AND SD_DepartmentGuid is null;",
							restoredDbName, testSystemEncryptedKey, LicenceRegItem, companyPk.ToString());
						connection.ExecuteNonQuery(sqlText);
					}
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1061:DoNotUseDateTimeUtcNow", Justification = "Baseline")]
		public void CreateSystemRegistrationKeyForNewTestSystem(AdminConnection connection, string restoredDbName, string hostedLocation)
		{
			ISystemRegistrationKey systemKey = new SystemRegistrationKey(
				DateTime.UtcNow.AddMonths(1),
				connection.GetAdminLoginSid(),
				connection.ServerInstanceName,
				restoredDbName,
				DatabaseTypes.Codes.Test,
				DatabaseSecurityModePairList.Codes.Locked,
				hostedLocation, "", "", "", "", 0, 0, DateTime.UtcNow, "");

			string sqlText = String.Format(@"
				INSERT [{0}]..StmData (SD_PK, SD_Name, SD_Owner, SD_DepartmentGuid, SD_Type, SD_BinaryValue)
					VALUES (newid(), '{1}', null, null, 'STR', convert(varbinary(max), N'{2}'));",
				restoredDbName, SysKeyRegItem, systemKey.ToEncryptedKeyString());
			connection.ExecuteNonQuery(sqlText);
		}

		public string GetHostedLocationFromRestoredDatabaseBeforeClearingProductionData(DbConnection connection, string restoredDbName)
		{
			var productRegistrationKey = GetProductRegistrationKey(connection, restoredDbName);
			if (!string.IsNullOrWhiteSpace(productRegistrationKey?.HostedLocation))
			{
				return productRegistrationKey.HostedLocation;
			}

			var systemKey = GetSystemRegistrationKey(connection, restoredDbName);
			if (!string.IsNullOrWhiteSpace(systemKey?.HostedLocation))
			{
				return systemKey.HostedLocation;
			}

			return string.Empty;
		}

		public static ISystemRegistrationKey GetSystemRegistrationKey(DbConnection connection, string dbName)
		{
			string sqlText = string.Format(@"
IF EXISTS (SELECT null FROM sys.databases WHERE name = '{0}')
BEGIN
	IF EXISTS (SELECT null FROM [{0}].sys.tables WHERE name = 'StmData'
		AND schema_id = (SELECT TOP 1 schema_id from [{0}].sys.schemas WHERE name = 'dbo'))
	BEGIN
		SELECT TOP 1 convert(nvarchar(max), SD_BinaryValue)
		FROM [{0}]..StmData WHERE SD_Name = '{1}'
	END
END",
				dbName, SysKeyRegItem);

			object encryptedKey = connection.ExecuteScalar(sqlText);

			ISystemRegistrationKey result = null;

			if (encryptedKey != null && encryptedKey != DBNull.Value)
			{
				try
				{
					result = SystemRegistrationKey.NewFromEncryptedXmlKey(encryptedKey.ToString());
				}
				catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException)
				{
					result = null;
				}
			}

			return result;
		}

		public static RegistrationKey GetProductRegistrationKey(DbConnection connection, string dbName)
		{
			string sqlText = string.Format(@"
IF EXISTS (SELECT * FROM sys.databases WHERE name = '{0}')
BEGIN
	IF EXISTS (SELECT * FROM [{0}].sys.tables WHERE name = 'StmData'
		AND schema_id = (SELECT TOP 1 schema_id from [{0}].sys.schemas WHERE name = 'dbo'))
	BEGIN
		SELECT TOP 1 convert(nvarchar(max), SD_BinaryValue)
		FROM [{0}].dbo.StmData WHERE SD_Name = '{1}'
	END
END",
				dbName, ProductRegItem);

			object encryptedKey = connection.ExecuteScalar(sqlText);
			RegistrationKey result = null;
			if (encryptedKey != null && encryptedKey != DBNull.Value)
			{
				try
				{
					string xmlKey = TwoWayEncoder.NewWithStandardInitialisationVector().Decrypt((string)encryptedKey);
					var reader = new StringReader(xmlKey);
					var registrationKeySerializer = new XmlSerializer(typeof(RegistrationKey));
					result = (RegistrationKey)registrationKeySerializer.Deserialize(reader);
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
				}
			}

			return result;
		}

		public static TwoWayEncoder GetNewLicenceEncoder()
		{
			var encoder = new TwoWayEncoder(Enterprise.Core.Constants.LicenceConstants.EncryptionKey);
			return encoder;
		}

		public bool WasRestoredDatabaseHostedWithCargoWise(DbConnection connection, string dbName)
		{
			var hostedLocation = GetHostedLocationFromRestoredDatabaseBeforeClearingProductionData(connection, dbName);

			return !string.IsNullOrEmpty(hostedLocation) && !hostedLocation.Equals(Core.Constants.LicenceConstants.NotHostedWithCargoWise);
		}

		protected string GetTestSystemLicenceFromProductionValue(string encryptedKey, string physicalServerId)
		{
			var encoder = GetNewLicenceEncoder();

			string testSystemLicence = encoder.Decrypt(encryptedKey.Trim());

			testSystemLicence = PhysicalServerIDRegex.Replace(testSystemLicence, @"PhysicalServerID=""" + physicalServerId + @"""");

			return encoder.Encrypt(testSystemLicence);
		}

		internal static string GetPhysicalServerId(DbConnection connection, string tableName, string dbName)
		{
			return GetStmData(connection, dbName, tableName, "PhysicalServerID");
		}

		static string GetStmData(DbConnection connection, string dbName, string tableName, string name)
		{
			string selectStmData = String.Format(@"
				SELECT TOP 1 convert(nvarchar(max), SD_BinaryValue) as textvalue
				FROM [{0}].[dbo].[{1}]
				WHERE SD_Name = '{2}'",
				dbName, tableName, name);

			object o = connection.ExecuteScalar(selectStmData);
			string result = (o == null) ? null : o.ToString().Trim();

			return result;
		}

		static readonly Regex PhysicalServerIDRegex = new Regex(@"PhysicalServerID=['""]\w{0,3}['""]", RegexOptions.Compiled);

		internal const string LicenceRegItem = "FreightNotesLengthNew";
		internal const string SysKeyRegItem = "FreightNotesHeaderLength";
		internal const string ProductRegItem = "FreightNotesRegistration";
	}

	public class RegistrationKey
	{
		public int DatabaseNumber { get; set; }
		public string EnterpriseCode { get; set; }
		public string ServerCode { get; set; }
		public string Password { get; set; }
		public string DbType { get; set; }
		public string DbSecurityMode { get; set; }
		public string HostedLocation { get; set; }
	}
}
