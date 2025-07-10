using System;
using CargoWise.Data;

namespace Enterprise.Build.Database.Script.TestFramework
{
	public class FreightSailingTestHelper
	{
		public FreightSailingTestHelper(DbConnection connection)
		{
			testConnection = connection;
		}

		public Guid SetupBasicLinkedTransportAndReturnPKOf(JobTypes jobTypePKToReturn)
		{
			var jobVoyagePK = Guid.NewGuid();
			var jobVoyOriginPK = Guid.NewGuid();
			var jobVoyDestPK = Guid.NewGuid();
			var jobSailingPK = Guid.NewGuid();
			var jobConsolTransportPK = Guid.NewGuid();

			var documentaryCutOff = DateTime.Today;
			var vgmCutOff = documentaryCutOff.AddDays(1);
			var receivalCommences = documentaryCutOff.AddDays(2);
			var cutOff = documentaryCutOff.AddDays(3);

			var availabilityDate = documentaryCutOff.AddDays(4);
			var storageDate = documentaryCutOff.AddDays(5);

			var depotReceivalCommences = documentaryCutOff.AddDays(6);
			var depotCutOff = documentaryCutOff.AddDays(7);
			var depotAvailabilityDate = documentaryCutOff.AddDays(8);
			var depotStorageDate = documentaryCutOff.AddDays(9);

			var insertSql = $@"
				INSERT INTO dbo.JobVoyage
					(JV_PK, JV_RV_NKVessel, JV_VoyageFlight, JV_IsChartered, JV_IsCargoOnly, JV_SystemCreateTimeUtc, JV_SystemCreateUser, JV_SystemLastEditTimeUtc, JV_SystemLastEditUser)
				VALUES
					('{jobVoyagePK}', 'a', 'b', '0', '0', GetUtcDate(), '~BP', GetUtcDate(), '~BP')

				---------------------------------
				INSERT INTO dbo.JobVoyOrigin
					(JA_PK, JA_JV, JA_DocumentaryCutoff, JA_VGMCutOff, JA_ReceivalCommences, JA_CutOff, JA_SystemCreateTimeUtc, JA_SystemCreateUser, JA_SystemLastEditTimeUtc, JA_SystemLastEditUser)
				VALUES
					('{jobVoyOriginPK}', '{jobVoyagePK}', '{documentaryCutOff:s}', '{vgmCutOff:s}', '{receivalCommences:s}', '{cutOff:s}', GetUtcDate(), '~BP', GetUtcDate(), '~BP')

				---------------------------------
				INSERT INTO dbo.JobVoyDestination
					(JB_PK, JB_JV, JB_AvailabilityDate, JB_StorageDate, JB_SystemCreateTimeUtc, JB_SystemCreateUser, JB_SystemLastEditTimeUtc, JB_SystemLastEditUser)
				VALUES
					('{jobVoyDestPK}', '{jobVoyagePK}', '{availabilityDate:s}', '{storageDate:s}', GetUtcDate(), '~BP', GetUtcDate(), '~BP')

				---------------------------------
				INSERT INTO dbo.JobSailing
					(JX_PK, JX_JA, JX_JB, JX_DepotReceivalCommences, JX_DepotCutOff, JX_DepotAvailabilityDate, JX_DepotStorageDate, JX_SystemCreateTimeUtc, JX_SystemCreateUser, JX_SystemLastEditTimeUtc, JX_SystemLastEditUser)
				VALUES
					('{jobSailingPK}', '{jobVoyOriginPK}', '{jobVoyDestPK}', '{depotReceivalCommences:s}', '{depotCutOff:s}', '{depotAvailabilityDate:s}', '{depotStorageDate:s}', GetUtcDate(), '~BP', GetUtcDate(), '~BP')

				---------------------------------
				INSERT INTO dbo.JobConsolTransport
					(JW_PK, JW_JX, JW_SystemCreateTimeUtc, JW_SystemCreateUser, JW_SystemLastEditTimeUtc, JW_SystemLastEditUser)
				VALUES
					('{jobConsolTransportPK}', '{jobSailingPK}', GetUtcDate(), '~BP', GetUtcDate(), '~BP')";

			testConnection.ExecuteNonQuery(insertSql);

			switch (jobTypePKToReturn)
			{
				case JobTypes.Voyage:
					return jobVoyagePK;
				case JobTypes.Origin:
					return jobVoyOriginPK;
				case JobTypes.Destination:
					return jobVoyDestPK;
				case JobTypes.Sailing:
					return jobSailingPK;
				default:
					return jobConsolTransportPK;
			}
		}

		public enum JobTypes { Voyage, Origin, Destination, Sailing, ConsolTransport }

		readonly DbConnection testConnection;
	}
}
