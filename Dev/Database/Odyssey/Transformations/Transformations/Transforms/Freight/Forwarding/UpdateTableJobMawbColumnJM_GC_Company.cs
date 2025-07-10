using System;
using CargoWise.Data;
using CargoWise.Database.ExtendedProperties;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Freight.Forwarding
{
	public class UpdateTableJobMawbColumnJM_GC_Company : DataTransformation
	{
		public override string UserDescription => "Update existing records of table [JobMawb] to set value of new column [JM_GC_Company]";

		const string TriggerName = "TG_JobMawb_UpdateJM_GC_CompanyOnUpdateOrInsert";

		const string TriggerSql = @"CREATE TRIGGER TG_JobMawb_UpdateJM_GC_CompanyOnUpdateOrInsert
									ON JobMawb
									AFTER UPDATE, INSERT
									AS
									BEGIN
										IF UPDATE(JM_GB)
										BEGIN
											UPDATE JM
											SET JM.JM_GC_Company = G.GB_GC,
												JM.JM_SystemLastEditTimeUtc = I.JM_SystemLastEditTimeUtc,
												JM.JM_SystemLastEditUser = I.JM_SystemLastEditUser
											FROM JobMawb JM
											INNER JOIN Inserted I ON JM.JM_PK = I.JM_PK
											LEFT JOIN GlbBranch G ON G.GB_PK = I.JM_GB
										END;
									END;";

		const string LastProcessedDTPropertyString = "UpdateTableJobMawbColumnJM_GC_Company_LastProcessedDT";

		const string UpdateChunkSql = @"UPDATE JM
										SET JM_GC_Company = G.GB_GC,
											JM_SystemLastEditTimeUtc = GETUTCDATE(),
											JM_SystemLastEditUser = '~BP'
										FROM dbo.JobMawb JM
										LEFT JOIN dbo.GlbBranch G ON G.GB_PK = JM.JM_GB
										WHERE JM_GC_Company IS NULL AND JM_GB IS NOT NULL AND JM_SystemCreateTimeUtc BETWEEN @LowerBound AND @UpperBound;";

		protected override void OnlinePreUpgradeTransform()
		{
			if (!DbObjectCreator.TableExists(Db.Connection, JobMawbSchema.Constants.TableName))
			{
				return;
			}

			DbObjectCreator.CreateColumnIfNotExists(Db.Connection, JobMawbSchema.Constants.TableName, JobMawbSchema.Constants.JM_GC_Company, "UNIQUEIDENTIFIER");

			if (!DbObjectCreator.TriggerExists(Db.Connection, JobMawbSchema.Constants.TableName, TriggerName))
			{
				Db.Connection.ExecuteNonQuery(TriggerSql);
			}

			var lastProcessedDTString = ExtProperty.Database.Select(Db.Connection, LastProcessedDTPropertyString);
			var lastProcessedDT = long.TryParse(lastProcessedDTString, out var parsedDT) ? new DateTime(parsedDT) : (DateTime?)null;

			foreach (var chunk in DateTimeChunker.GenerateChunks(10000, lastProcessedDT, JobMawbSchema.JM_SystemCreateTimeUtc))
			{
				using (var cmd = Db.Connection.Command(UpdateChunkSql))
				{
					cmd.AddParameter("@LowerBound", JobMawbSchema.JM_SystemCreateTimeUtc.SqlDbType, chunk.LowerBound);
					cmd.AddParameter("@UpperBound", JobMawbSchema.JM_SystemCreateTimeUtc.SqlDbType, chunk.UpperBound);

					cmd.ExecuteNonQuery();
				}

				ExtProperty.Database.Update(Db.Connection, LastProcessedDTPropertyString, chunk.UpperBound.Ticks.ToString());
			}

			ExtProperty.Database.Delete(Db.Connection, LastProcessedDTPropertyString);
		}
	}
}
