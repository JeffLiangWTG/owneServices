using System;
using System.Data;
using CargoWise.Data;

namespace Enterprise.Build.Database.Script.Testing.Public.Recruiter
{
	public static class TalentDataHelpers
	{
		public static Guid CreateTalActivity(DbConnection connection, string name, string type, string source = "")
		{
			var pk = Guid.NewGuid();

			var sqlText = @"
INSERT INTO dbo.TalActivity (TAV_PK, TAV_ActivityName, TAV_ActivityType, TAV_Source, TAV_SystemCreateTimeUtc, TAV_SystemCreateUser, TAV_SystemLastEditTimeUtc, TAV_SystemLastEditUser)
VALUES (@PK, @name, @type, @source, GETUTCDATE(), 'E', GETUTCDATE(), 'E')
";

			using (var cmd = connection.Command(sqlText))
			{
				cmd.AddParameter("@PK", SqlDbType.UniqueIdentifier, pk);
				cmd.AddParameter("@name", SqlDbType.NVarChar, name);
				cmd.AddParameter("@type", SqlDbType.Char, type);
				cmd.AddParameter("@source", SqlDbType.Char, source);

				cmd.ExecuteNonQuery();
			}

			return pk;
		}

		public static Guid CreateJobCampaign(DbConnection connection)
		{
			var pk = Guid.NewGuid();

			var sqlText = @"
INSERT INTO dbo.HRRecruitmentJobCampaign (HV_PK, HV_SystemCreateTimeUtc, HV_SystemCreateUser, HV_SystemLastEditTimeUtc, HV_SystemLastEditUser)
VALUES (@PK, GETUTCDATE(), 'E', GETUTCDATE(), 'E')
";

			using (var cmd = connection.Command(sqlText))
			{
				cmd.AddParameter("@PK", SqlDbType.UniqueIdentifier, pk);

				cmd.ExecuteNonQuery();
			}

			return pk;
		}

		public static Guid CreateTalDefaultStage(DbConnection connection, Guid campaignPK, string name, byte sequence, string completionEvent)
		{
			var pk = Guid.NewGuid();

			var sqlText = @"
INSERT INTO dbo.TalDefaultStage (TDS_PK, TDS_HV_RecruitmentJobCampaign, TDS_Name, TDS_Sequence, TDS_SE_NKCompletionEvent, TDS_SystemCreateTimeUtc, TDS_SystemCreateUser, TDS_SystemLastEditTimeUtc, TDS_SystemLastEditUser)
VALUES (@PK, @campaignPK, @name, @sequence, @completionEvent, GETUTCDATE(), 'E', GETUTCDATE(), 'E')
";

			using (var cmd = connection.Command(sqlText))
			{
				cmd.AddParameter("@PK", SqlDbType.UniqueIdentifier, pk);
				cmd.AddParameter("@campaignPK", SqlDbType.UniqueIdentifier, campaignPK);
				cmd.AddParameter("@name", SqlDbType.NVarChar, name);
				cmd.AddParameter("@sequence", SqlDbType.TinyInt, sequence);
				cmd.AddParameter("@completionEvent", SqlDbType.VarChar, completionEvent);

				cmd.ExecuteNonQuery();
			}

			return pk;
		}

		public static Guid CreateTalDefaultActivity(DbConnection connection, Guid activityPK, Guid stagePK)
		{
			var pk = Guid.NewGuid();

			var sqlText = @"
INSERT INTO dbo.TalDefaultActivity (TDV_PK, TDV_TAV_Activity, TDV_TDS_DefaultStage, TDV_SystemCreateTimeUtc, TDV_SystemCreateUser, TDV_SystemLastEditTimeUtc, TDV_SystemLastEditUser)
VALUES (@PK, @activityPK, @stagePK, GETUTCDATE(), 'E', GETUTCDATE(), 'E')
";

			using (var cmd = connection.Command(sqlText))
			{
				cmd.AddParameter("@PK", SqlDbType.UniqueIdentifier, pk);
				cmd.AddParameter("@activityPK", SqlDbType.UniqueIdentifier, activityPK);
				cmd.AddParameter("@stagePK", SqlDbType.UniqueIdentifier, stagePK);

				cmd.ExecuteNonQuery();
			}

			return pk;
		}

		public static Guid CreateJobApplicant(DbConnection connection)
		{
			var pk = Guid.NewGuid();

			var sqlText = @"
INSERT INTO dbo.GlbPerson (PER_PK, PER_FullName, PER_EmailAddress, PER_SystemCreateTimeUtc, PER_SystemCreateUser, PER_SystemLastEditTimeUtc, PER_SystemLastEditUser)
VALUES ('896a8895-aea3-4076-a4c0-05781907fe4c', 'Britney', 'britney@gmail.com', GETUTCDATE(), 'E', GETUTCDATE(), 'E')

INSERT INTO dbo.HRJobApplicant (HA_PK, HA_PER, HA_SystemCreateTimeUtc, HA_SystemCreateUser, HA_SystemLastEditTimeUtc, HA_SystemLastEditUser)
VALUES (@PK, '896a8895-aea3-4076-a4c0-05781907fe4c', GETUTCDATE(), 'E', GETUTCDATE(), 'E')
";

			using (var cmd = connection.Command(sqlText))
			{
				cmd.AddParameter("@PK", SqlDbType.UniqueIdentifier, pk);

				cmd.ExecuteNonQuery();
			}

			return pk;
		}

		public static Guid CreateJobApplication(DbConnection connection, Guid campaignPK, Guid applicantPK)
		{
			var pk = Guid.NewGuid();

			var sqlText = @"
INSERT INTO dbo.HRJobApplication (HP_PK, HP_HV, HP_HA, HP_ApplicationNumber, HP_SystemCreateTimeUtc, HP_SystemCreateUser, HP_SystemLastEditTimeUtc, HP_SystemLastEditUser)
VALUES (@PK, @campaignPK, @applicantPK, '1', GETUTCDATE(), 'E', GETUTCDATE(), 'E')
";

			using (var cmd = connection.Command(sqlText))
			{
				cmd.AddParameter("@PK", SqlDbType.UniqueIdentifier, pk);
				cmd.AddParameter("@campaignPK", SqlDbType.UniqueIdentifier, campaignPK);
				cmd.AddParameter("@applicantPK", SqlDbType.UniqueIdentifier, applicantPK);

				cmd.ExecuteNonQuery();
			}

			return pk;
		}

		public static Guid CreateTalApplicationStage(DbConnection connection, Guid applicationPK, string name, byte sequence, string completionEvent)
		{
			var pk = Guid.NewGuid();

			var sqlText = @"
INSERT INTO dbo.TalApplicationStage (TPS_PK, TPS_HP_JobApplication, TPS_Name, TPS_Sequence, TPS_SE_NKCompletionEvent, TPS_SystemCreateTimeUtc, TPS_SystemCreateUser, TPS_SystemLastEditTimeUtc, TPS_SystemLastEditUser)
VALUES (@PK, @applicationPK, @name, @sequence, @completionEvent, GETUTCDATE(), 'E', GETUTCDATE(), 'E')
";

			using (var cmd = connection.Command(sqlText))
			{
				cmd.AddParameter("@PK", SqlDbType.UniqueIdentifier, pk);
				cmd.AddParameter("@applicationPK", SqlDbType.UniqueIdentifier, applicationPK);
				cmd.AddParameter("@name", SqlDbType.NVarChar, name);
				cmd.AddParameter("@sequence", SqlDbType.TinyInt, sequence);
				cmd.AddParameter("@completionEvent", SqlDbType.VarChar, completionEvent);

				cmd.ExecuteNonQuery();
			}

			return pk;
		}

		public static Guid CreateTalApplicationActivity(DbConnection connection, Guid activityPK, Guid stagePK)
		{
			var pk = Guid.NewGuid();

			var sqlText = @"
INSERT INTO dbo.TalApplicationActivity (TPA_PK, TPA_TAV_Activity, TPA_TPS_ApplicationStage, TPA_SystemCreateTimeUtc, TPA_SystemCreateUser, TPA_SystemLastEditTimeUtc, TPA_SystemLastEditUser)
VALUES (@PK, @activityPK, @stagePK, GETUTCDATE(), 'E', GETUTCDATE(), 'E')
";

			using (var cmd = connection.Command(sqlText))
			{
				cmd.AddParameter("@PK", SqlDbType.UniqueIdentifier, pk);
				cmd.AddParameter("@activityPK", SqlDbType.UniqueIdentifier, activityPK);
				cmd.AddParameter("@stagePK", SqlDbType.UniqueIdentifier, stagePK);

				cmd.ExecuteNonQuery();
			}

			return pk;
		}
	}
}
