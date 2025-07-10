using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Providers.Common;
using Enterprise.Client.EDI.BorderWise;
using Enterprise.Integration;
using ServiceManager.Integration.ServiceTasks.CW;

[assembly: HostedService(
	BorderWiseDirectSyncServiceTask.Code,
	"User Management Portal Direct Data Sync",
	"CSP",
	typeof(BorderWiseDirectSyncServiceTask),
	CanRunInAnyBranch = true,
	MinimumPeriod = "15Minutes",
	DefaultScheduleRunEvery = "1day",
	DefaultScheduleStartAtLocal = "7hours",
	ActiveByDefault = true
	)]

namespace Enterprise.Client.EDI.BorderWise
{
	public class BorderWiseDirectSyncServiceTask : ServiceProviderImpl
	{
		public const string Code = "BDS";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Reference", "CW1065:Encrypt SQL Connection Rule", Justification = "Baseline")]
		public override void RunTask(CancellationToken youMustReactToThisToken)
		{
			var umpConnectionString = EDIDataRegistry.Instance.BorderWiseUmpDatabaseConnectionString.Value;

			if (string.IsNullOrEmpty(umpConnectionString))
			{
				var errorMessage = FormattableString.Invariant($"No connection string has been specified in registry item {EDIDataRegistry.Instance.BorderWiseUmpDatabaseConnectionString.Caption}.");
				ServiceLogger.Log(LogType.Warning, errorMessage);
				return;
			}

			var sqlConnectionStringBuilder = new SqlConnectionStringBuilder(umpConnectionString);

			var encrypt = SqlTlsSetting.ShouldEncryptSqlConnection(sqlConnectionStringBuilder.DataSource);
			sqlConnectionStringBuilder["Encrypt"] = encrypt;
			sqlConnectionStringBuilder["TrustServerCertificate"] = !encrypt;

			CreateLinkedServer(sqlConnectionStringBuilder);

			var updatedRecords = new DictionaryOfLists<int, Guid>();
			var performanceStats = new Dictionary<string, long>();

			var syncEvents = Enum.GetValues(typeof(EntitySyncEvents));
			
			var stopwatch = new Stopwatch();

			foreach (var syncEvent in syncEvents)
			{
				var operationKey = (int)syncEvent;
				
				var sql = CreateSqlStatement(sqlConnectionStringBuilder, operationKey);

				if (string.IsNullOrEmpty(sql))
				{
					ServiceLogger.Log(LogType.Warning, $"No sql found for operation key: {operationKey}");
					continue;
				}

				stopwatch.Restart();

				Db.Connection.ExecuteReader(sql, reader =>
				{
					var updatedPK = reader.GetGuid(0);
					var operation = reader.GetInt32(1);

					updatedRecords.AddValues(operation, updatedPK);
				});

				stopwatch.Stop();
				performanceStats.Add(syncEvent.ToString(), stopwatch.ElapsedMilliseconds);
			}

			if (updatedRecords.Count != 0)
			{
				var updatedEntityMessage = new StringBuilder();

				AddMessage((int)EntitySyncEvents.ContactsDisabled, "Contacts disabled");
				AddMessage((int)EntitySyncEvents.ContactsUpdated, "Contacts updated");
				AddMessage((int)EntitySyncEvents.OrganizationsDisabled, "Organizations disabled");
				AddMessage((int)EntitySyncEvents.OrganizationsUpdated, "Organizations updated");
				AddMessage((int)EntitySyncEvents.AddressesUpdated, "Addresses updated");
				AddMessage((int)EntitySyncEvents.NewOrganizationsNotSynced, "New organizations not synced");
				AddMessage((int)EntitySyncEvents.NewContactsNotSynced, "New contacts not synced");
				AddMessage((int)EntitySyncEvents.OrganizationMasterOrgUpdated, "Organizations master org updated");
				AddMessage((int)EntitySyncEvents.ExistingContactNotDeleted, "Existing contacts not deleted updated");

				void AddMessage(int operation, string messagePrefix)
				{
					updatedEntityMessage.Append(messagePrefix);
					updatedEntityMessage.Append(": ");
					updatedEntityMessage.Append(updatedRecords.ContainsKey(operation) ? string.Join(", ", updatedRecords[operation].OrderBy(pk => pk)) : string.Empty);
					updatedEntityMessage.AppendLine();
				}

				ServiceLogger.Log(LogType.Information, updatedEntityMessage.ToString());
			}

			var message = new StringBuilder();
			foreach (var performanceStat in performanceStats)
			{
				message.Append(FormattableString.Invariant($"Operation {performanceStat.Key} took {performanceStat.Value}ms"));
				message.AppendLine();
			}
			message.Append(FormattableString.Invariant($"Sync complete with {updatedRecords.Count} updates."));

			ServiceLogger.Log(LogType.Information, message.ToString());
		}

		static void CreateLinkedServer(SqlConnectionStringBuilder umpConnectionString)
		{
			if (Db.Connection.ServerName != umpConnectionString.DataSource)
			{
				var sql = FormattableString.Invariant($@"
IF NOT EXISTS(SELECT * FROM sys.servers WHERE [NAME] = '{umpConnectionString.DataSource}')
BEGIN
	exec sp_addlinkedserver '{umpConnectionString.DataSource}'
	exec sp_addlinkedsrvlogin '{umpConnectionString.DataSource}', 'false', '{Db.Connection.UserLogin}', '{umpConnectionString.UserID}', '{umpConnectionString.Password}'
END
");

				Db.Connection.ExecuteNonQuery(sql);
			}
		}

		static string CreateSqlStatement(SqlConnectionStringBuilder umpConnectionString, int operationKey)
		{
			var remoteServerPrefix = FormattableString.Invariant($"[{umpConnectionString.DataSource}].{umpConnectionString.InitialCatalog}.dbo");

			var declareTempTable = FormattableString.Invariant($@"
DECLARE @UpdatedRecords TABLE (PK uniqueidentifier, OperationNumber int);
");
			var selectTempTable = FormattableString.Invariant($@"
SELECT * FROM @UpdatedRecords
");

			string InsertUpdateRecords(string pkName, int operationNumber, bool isUmpPk = true) =>
				FormattableString.Invariant($@"
INSERT INTO @UpdatedRecords 
SELECT {(isUmpPk ? "ump" : "ediProd")}.{pkName}, {operationNumber}
");

			var deleteContactsUpdate = FormattableString.Invariant(
				$@"
UPDATE ump
SET
	OC_IsActive = 0,
	OC_IsDeleted = 1
");

			var deleteContactsCondition = FormattableString.Invariant($@"
FROM {remoteServerPrefix}.OrgContact ump
LEFT JOIN dbo.OrgContact ediProd ON ediProd.OC_PK = ump.EdiProdRecordId
WHERE 1=1
AND ump.OC_IsDeleted = 0
AND ump.EdiProdRecordId <> '00000000-0000-0000-0000-000000000000'
AND ediProd.OC_PK IS NULL
");

			var updateContactsUpdate = FormattableString.Invariant(
				$@"
UPDATE ump
	SET
	ump.OC_IsActive = ediProd.OC_IsActive,
	ump.OC_ContactName = ediProd.OC_ContactName,
	ump.OC_Language = ediProd.OC_Language,
	ump.OC_Title = ediProd.OC_Title,
	ump.OC_RN_NKNationality = ediProd.OC_RN_NKNationality,
	ump.OC_Gender = ediProd.OC_Gender,
	ump.OC_Phone = ediProd.OC_Phone,
	ump.OC_PhoneExtension = ediProd.OC_PhoneExtension,
	ump.OC_Mobile = ediProd.OC_Mobile,
	ump.OC_Birthday = ediProd.OC_Birthday,
	ump.OC_HasAccess = ediProd.OC_WebAccessEnabled,
	ump.OC_Email = ediProd.OC_Email,
	ump.OC_OH = umpOrgHeader.OH_PK,
	ump.OC_PasswordHash = CASE WHEN PER_PasswordHash IS NOT NULL THEN PER_PasswordHash ELSE ediProd.OC_PasswordHash END,
	ump.OC_PasswordHashIterations = CASE WHEN PER_PasswordHash IS NOT NULL THEN PER_PasswordHashIterations ELSE ediProd.OC_PasswordHashIterations END,
	ump.OC_PasswordSalt = CASE WHEN PER_PasswordHash IS NOT NULL THEN PER_PasswordSalt ELSE ediProd.OC_PasswordSalt END
");

			var updateContactsCondition = FormattableString.Invariant($@"
FROM {remoteServerPrefix}.OrgContact ump
JOIN dbo.OrgContact ediProd ON ediProd.OC_PK = ump.EdiProdRecordId
JOIN dbo.GlbPerson ON PER_PK = ediProd.OC_PER
JOIN {remoteServerPrefix}.OrgHeader umpOrgHeader ON umpOrgHeader.EdiProdRecordId = ediProd.OC_OH
WHERE 1=2
	OR ediProd.[OC_IsActive] <> ump.OC_IsActive
	OR ump.OC_ContactName <> ediProd.[OC_ContactName] COLLATE DATABASE_DEFAULT
	OR ump.OC_Language <> ediProd.[OC_Language] COLLATE DATABASE_DEFAULT
	OR ump.OC_Title <> ediProd.[OC_Title] COLLATE DATABASE_DEFAULT
	OR ump.OC_RN_NKNationality <> ediProd.[OC_RN_NKNationality] COLLATE DATABASE_DEFAULT
	OR ump.OC_Gender <> ediProd.[OC_Gender] COLLATE DATABASE_DEFAULT
	OR ump.OC_Phone <> ediProd.[OC_Phone] COLLATE DATABASE_DEFAULT
	OR ump.OC_PhoneExtension <> ediProd.[OC_PhoneExtension] COLLATE DATABASE_DEFAULT
	OR ump.OC_Mobile <> ediProd.[OC_Mobile] COLLATE DATABASE_DEFAULT
	OR ump.OC_Birthday <> ediProd.[OC_Birthday]
	OR umpOrgHeader.EdiProdRecordId <> ediProd.[OC_OH]
	OR ump.OC_Email <> ediProd.[OC_Email] COLLATE DATABASE_DEFAULT 
	OR ump.OC_PasswordHash <> case when PER_PasswordHash is not null then PER_PasswordHash else ediProd.[OC_PasswordHash] end
	OR ump.OC_PasswordHashIterations <> case when PER_PasswordHash is not null then PER_PasswordHashIterations else ediProd.[OC_PasswordHashIterations] end
	OR ump.OC_PasswordSalt <> case when PER_PasswordHash is not null then PER_PasswordSalt else ediProd.[OC_PasswordSalt] end
	OR ump.OC_HasAccess <> ediProd.OC_WebAccessEnabled
");

			var deleteOrgsUpdate = FormattableString.Invariant(
				$@"
UPDATE ump
SET
	OH_IsActive = 0
");

			var deleteOrgsCondition = FormattableString.Invariant($@"
FROM {remoteServerPrefix}.OrgHeader ump
LEFT JOIN dbo.OrgHeader ediProd ON ediProd.OH_PK = ump.EdiProdRecordId
WHERE 1=1
	AND ump.OH_IsActive = 1
	AND ediProd.OH_PK IS NULL
	AND EdiProdRecordId IS NOT NULL
");

			var updateOrgsUpdate = FormattableString.Invariant(
				$@"
UPDATE ump
SET
	ump.OH_Code = ediProd.OH_Code,
	ump.OH_IsActive = ediProd.OH_IsActive,
	ump.OH_FullName = ediProd.OH_FullName,
	ump.OH_Language = ediProd.OH_Language
");

			var updateOrgsCondition = FormattableString.Invariant($@"
FROM {remoteServerPrefix}.OrgHeader ump
JOIN dbo.OrgHeader ediProd ON ediProd.OH_PK = ump.EdiProdRecordId
WHERE 1=2
	OR ump.OH_IsActive <> ediProd.OH_IsActive
	OR ump.OH_Code <> ediProd.OH_Code COLLATE DATABASE_DEFAULT
	OR ump.OH_FullName <> ediProd.OH_FullName COLLATE DATABASE_DEFAULT
	OR ump.OH_Language <> ediProd.OH_Language COLLATE DATABASE_DEFAULT
");

			var updateOrgMasterOrgUpdate = FormattableString.Invariant(
				$@"
UPDATE ump
SET
	ump.OH_MasterOrgPK = CASE WHEN ediMasterOrgInUmp.OH_PK IS NULL OR ump.OH_PK = ediMasterOrgInUmp.OH_PK THEN NULL ELSE ediMasterOrgInUmp.OH_PK END
");

			var updateOrgMasterOrgCondition = FormattableString.Invariant($@"
FROM {remoteServerPrefix}.OrgHeader ump
LEFT JOIN (
SELECT oh.OH_PK, ediProd.OH_PK as OH_PK_Master
FROM dbo.OrgHeader oh
INNER JOIN dbo.LicenceCompany ON LC_OH = oh.OH_PK
INNER JOIN dbo.LicenceHeader ON LC_PK = LA_LC AND LA_IsActive = 1
INNER JOIN dbo.LicenceDatabase ON LD_PK = LA_LD AND LD_IsActive = 1 AND LD_Product = 'BOR'
INNER JOIN dbo.OrgHeader ediProd ON LD_OH_WebAccessOrg = ediProd.OH_PK
WHERE oh.OH_IsActive = 1
) as ediMasterOrg ON ediMasterOrg.OH_PK = ump.EdiProdRecordId
LEFT JOIN {remoteServerPrefix}.OrgHeader ediMasterOrgInUmp ON ediMasterOrgInUmp.EdiProdRecordId = ediMasterOrg.OH_PK_Master
WHERE 1=1
AND	(
		(
			ediMasterOrgInUmp.OH_PK IS NOT NULL
			AND
			(
				(ump.OH_PK <> ediMasterOrgInUmp.OH_PK AND (ump.OH_MasterOrgPK IS NULL OR ump.OH_MasterOrgPK <> ediMasterOrgInUmp.OH_PK))
				OR
				(ump.OH_PK = ediMasterOrgInUmp.OH_PK AND ump.OH_MasterOrgPK IS NOT NULL)
			) 
		)
		OR
		(
			ump.OH_MasterOrgPK IS NOT NULL
			AND ediMasterOrgInUmp.OH_PK IS NULL
		)
	)
");

			var updateAddressesUpdate = FormattableString.Invariant(
				$@"
UPDATE ump
SET
	ump.OA_IsActive = ediProd.OA_IsActive,
	ump.OA_Code = ediProd.OA_Code,
	ump.OA_Address1 = ediProd.OA_Address1,
	ump.OA_Address2 = ediProd.OA_Address2,
	ump.OA_State = ediProd.OA_State,
	ump.OA_PostCode = ediProd.OA_PostCode,
	ump.OA_Phone = ediProd.OA_Phone,
	ump.OA_Email = ediProd.OA_Email,
	ump.OA_City = ediProd.OA_City,
	ump.OA_CountryCode = ediProd.OA_RN_NKCountryCode,
	ump.OA_Language = ediProd.OA_Language,
	ump.OA_OH = umpOrgHeader.OH_PK
");

			var updateAddressesCondition = FormattableString.Invariant($@"
FROM {remoteServerPrefix}.OrgAddress ump
JOIN dbo.OrgAddress ediProd ON ediProd.OA_PK = ump.EdiProdRecordId
JOIN {remoteServerPrefix}.OrgHeader umpOrgHeader ON umpOrgHeader.EdiProdRecordId = ediProd.OA_OH
WHERE 1=2
	OR ump.OA_IsActive <> ediProd.OA_IsActive
	OR ump.OA_OH <> umpOrgHeader.OH_PK
	OR ump.OA_Code <> ediProd.OA_Code COLLATE DATABASE_DEFAULT
	OR ump.OA_Address1 <> ediProd.OA_Address1 COLLATE DATABASE_DEFAULT
	OR ump.OA_Address2 <> ediProd.OA_Address2 COLLATE DATABASE_DEFAULT
	OR ump.OA_State <> ediProd.OA_State COLLATE DATABASE_DEFAULT
	OR ump.OA_PostCode <> ediProd.OA_PostCode COLLATE DATABASE_DEFAULT
	OR ump.OA_Phone <> ediProd.OA_Phone COLLATE DATABASE_DEFAULT
	OR ump.OA_Email <> ediProd.OA_Email COLLATE DATABASE_DEFAULT
	OR ump.OA_City <> ediProd.OA_City COLLATE DATABASE_DEFAULT
	OR ump.OA_CountryCode <> ediProd.OA_RN_NKCountryCode COLLATE DATABASE_DEFAULT
	OR ump.OA_Language <> ediProd.OA_Language COLLATE DATABASE_DEFAULT
");

			var newOrgsCheckCondition = FormattableString.Invariant($@"
FROM dbo.OrgHeader ediProd
LEFT JOIN {remoteServerPrefix}.OrgHeader ump  ON ediProd.OH_PK = ump.EdiProdRecordId
WHERE 1=1
	AND ediProd.OH_IsActive = 1
	AND ump.OH_PK IS NULL
ORDER BY ediProd.OH_PK
");

			var newContactsCheckCondition = FormattableString.Invariant($@"
FROM dbo.OrgContact ediProd
LEFT JOIN {remoteServerPrefix}.OrgContact ump  ON ediProd.OC_PK = ump.EdiProdRecordId
WHERE 1=1
	AND ediProd.OC_IsActive = 1
	AND ump.OC_PK IS NULL
ORDER BY ediProd.OC_PK
");

			var existingContactNotDeletedCondition = FormattableString.Invariant($@"
FROM {remoteServerPrefix}.OrgContact ump
INNER JOIN dbo.OrgContact ediProd ON ediProd.OC_PK = ump.EdiProdRecordId
WHERE 1=1
	AND ump.OC_IsDeleted = 1
");

			var existingContactNotDeletedUpdate = FormattableString.Invariant($@"
UPDATE ump
SET	ump.OC_IsDeleted = 0
");
			var stringBuilder = new StringBuilder();
			stringBuilder.Append(declareTempTable);

			switch (operationKey)
			{
				case (int)EntitySyncEvents.ContactsDisabled: 
					stringBuilder.Append(InsertUpdateRecords("OC_PK", (int)EntitySyncEvents.ContactsDisabled));
					stringBuilder.Append(deleteContactsCondition);
					stringBuilder.Append(deleteContactsUpdate);
					stringBuilder.Append(deleteContactsCondition);
					break;
				case (int)EntitySyncEvents.ContactsUpdated:
					stringBuilder.Append(InsertUpdateRecords("OC_PK", (int)EntitySyncEvents.ContactsUpdated));
					stringBuilder.Append(updateContactsCondition);
					stringBuilder.Append(updateContactsUpdate);
					stringBuilder.Append(updateContactsCondition);
					break;
				case (int)EntitySyncEvents.OrganizationsDisabled:
					stringBuilder.Append(InsertUpdateRecords("OH_PK", (int)EntitySyncEvents.OrganizationsDisabled));
					stringBuilder.Append(deleteOrgsCondition);
					stringBuilder.Append(deleteOrgsUpdate);
					stringBuilder.Append(deleteOrgsCondition);
					break;
				case (int)EntitySyncEvents.OrganizationsUpdated:
					stringBuilder.Append(InsertUpdateRecords("OH_PK", (int)EntitySyncEvents.OrganizationsUpdated));
					stringBuilder.Append(updateOrgsCondition);
					stringBuilder.Append(updateOrgsUpdate);
					stringBuilder.Append(updateOrgsCondition);
					break;
				case (int)EntitySyncEvents.AddressesUpdated:
					stringBuilder.Append(InsertUpdateRecords("OA_PK", (int)EntitySyncEvents.AddressesUpdated));
					stringBuilder.Append(updateAddressesCondition);
					stringBuilder.Append(updateAddressesUpdate);
					stringBuilder.Append(updateAddressesCondition);
					break;
				case (int)EntitySyncEvents.OrganizationMasterOrgUpdated:
					stringBuilder.Append(InsertUpdateRecords("OH_PK", (int)EntitySyncEvents.OrganizationMasterOrgUpdated));
					stringBuilder.Append(updateOrgMasterOrgCondition);
					stringBuilder.Append(updateOrgMasterOrgUpdate);
					stringBuilder.Append(updateOrgMasterOrgCondition);
					break;
				case (int)EntitySyncEvents.NewOrganizationsNotSynced:
					stringBuilder.Append(InsertUpdateRecords("OH_PK", (int)EntitySyncEvents.NewOrganizationsNotSynced, isUmpPk: false));
					stringBuilder.Append(newOrgsCheckCondition);
					break;
				case (int)EntitySyncEvents.NewContactsNotSynced:
					stringBuilder.Append(InsertUpdateRecords("OC_PK", (int)EntitySyncEvents.NewContactsNotSynced, isUmpPk: false));
					stringBuilder.Append(newContactsCheckCondition);
					break;
				case (int)EntitySyncEvents.ExistingContactNotDeleted:
					stringBuilder.Append(InsertUpdateRecords("OC_PK", (int)EntitySyncEvents.ExistingContactNotDeleted));
					stringBuilder.Append(existingContactNotDeletedCondition);
					stringBuilder.Append(existingContactNotDeletedUpdate);
					stringBuilder.Append(existingContactNotDeletedCondition);
					break;
			}

			stringBuilder.Append(selectTempTable);

			return stringBuilder.ToString();
		}
	}
}
