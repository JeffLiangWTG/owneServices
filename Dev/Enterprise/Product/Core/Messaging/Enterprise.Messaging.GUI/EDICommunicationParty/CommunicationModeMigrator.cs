using System;
using System.Data;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Messaging.GUI
{
	public static class CommunicationModeMigrator
	{
		const int BatchSize = 10000;

		public static int OrganizationCount(BusinessObjectFactory factory)
		{
			var query = $@"
SELECT COUNT(*) AS NumberOfElements
FROM {EDICommunicationsModeSchema.Constants.SqlSchemaName}.{EDICommunicationsModeSchema.Constants.TableName}
WHERE
	{EDICommunicationsModeSchema.Constants.EK_CommunicationsTransport} = '{EDICommunicationsModeCommunicationsTransportList.Codes.EAdaptorInterface}' AND
	{EDICommunicationsModeSchema.Constants.EK_CommsDirection} = '{EDICommunicationsModeCommsDirectionList.Codes.Transmit}';
";
			return (int)((IDbConnected)factory).Connection.ExecuteScalar(query);
		}

		public static bool Run(ILogger log, ZGuid? ecc_pk, BusinessObjectFactory factory)
		{
			bool queryExecuted = true;
			try {
				using (var manager = ((IDbConnected)factory).Connection.BeginTransactionWithManager())
				{
					string sqlCommand = $@"
		UPDATE TOP (@RowsToChange) {EDICommunicationsModeSchema.Constants.SqlSchemaName}.{EDICommunicationsModeSchema.Constants.TableName}
		SET {EDICommunicationsModeSchema.Constants.EK_ECC_CommunicationPartyConfig} = @pk
		WHERE 
			{EDICommunicationsModeSchema.Constants.EK_CommunicationsTransport} = '{EDICommunicationsModeCommunicationsTransportList.Codes.EAdaptorInterface}' AND
			{EDICommunicationsModeSchema.Constants.EK_CommsDirection} = '{EDICommunicationsModeCommsDirectionList.Codes.Transmit}' AND
			({EDICommunicationsModeSchema.Constants.EK_ECC_CommunicationPartyConfig} IS NULL OR @pk IS NULL OR {EDICommunicationsModeSchema.Constants.EK_ECC_CommunicationPartyConfig} <> @pk) AND
			NOT ({EDICommunicationsModeSchema.Constants.EK_ECC_CommunicationPartyConfig} IS NULL AND @pk IS NULL) 
		SELECT @@ROWCOUNT
		";
					var rowChangeCount = 0;
					var totalChangeCount = 0;
					log.Notify(Res.GetString("320B61D8-A50F-4D30-90A9-13BD849BB84C", "Start updating"), false);

					do
					{
						rowChangeCount = ((IDbConnected)factory).Connection.ExecuteScalar<int>(sqlCommand,
							cmd =>
							{
								cmd.AddParameter((NoResString)"@RowsToChange", SqlDbType.Int, BatchSize);
								cmd.AddParameter((NoResString)"@pk", SqlDbType.UniqueIdentifier, (ecc_pk == null ? DBNull.Value : ecc_pk?.XmlSerializedValue));
							});
						totalChangeCount += rowChangeCount; 
					}
					while (BatchSize == rowChangeCount);
					log.Notify(Res.GetString("33878851-F452-4C88-9254-B196CE259CA2", "Finish updating on {0} organizations", totalChangeCount), false);

					manager.CommitTransaction();
				}
			}
			catch (SqlException ex)
			{
				queryExecuted = false;
				RecordException(ex, log);
			}
			return queryExecuted;
		}

		static void RecordException(SqlException ex, ILogger log)
		{
			log.Notify(Res.GetString("373FF2FA-EC1D-4F78-A0DB-E078D73AB498", "Update failed : {0}", ex.Message), true);
		}
	}
}
