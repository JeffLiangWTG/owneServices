using System;
using System.Data;
using CargoWise.Data;

namespace Enterprise.Build.Database.Script.Testing.Public.HRM
{
	 class HRMDataHelpers
	{
		public static void CreateReviewProcess(DbConnection conn, Guid pk, string name)
		{
			var query = $@"
INSERT INTO dbo.ReviewProcess
	(RPR_PK, RPR_Name, RPR_EffectiveDate, RPR_SubmissionDate, RPR_RX_NKCurrency, RPR_PrimaryHierarchy,
		RPR_SystemCreateTimeUtc, RPR_SystemCreateUser, RPR_SystemLastEditTimeUtc, RPR_SystemLastEditUser, RPR_GC_Company)
VALUES
	(@pk, @name, GETUTCDATE(), GETUTCDATE(), 'AUD', 'DRM',
		GETUTCDATE(), 'E', GETUTCDATE(), 'E', '03052ed3-2c64-49ac-97d8-c6079d5015b5')";

			_ = conn.ExecuteNonQuery(query, p =>
			{
				p.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				p.AddParameter("@name", SqlDbType.NVarChar, 256, name);
			});
		}

		public static void CreateReviewProcessNode(DbConnection conn, Guid pk, Guid? parent, Guid reviewProcess, Guid reviewer, string revCode)
		{
			var query = $@"
IF NOT EXISTS ( SELECT NULL FROM dbo.GlbStaff WHERE GS_Code = @code )
	INSERT INTO dbo.GlbStaff
		(GS_PK, GS_Code, GS_LoginName, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser)
	VALUES
		(@staffPK, @code, @code, GETUTCDATE(), 'E', GETUTCDATE(), 'E');

INSERT INTO dbo.ReviewProcessNode
	(RRN_PK, RRN_RRN_Parent, RRN_RPR_ReviewProcess, RRN_GS_Reviewer, RRN_SystemCreateTimeUtc, RRN_SystemCreateUser, RRN_SystemLastEditTimeUtc, RRN_SystemLastEditUser)
VALUES
	(@pk, @parent, @reviewProcess, @reviewer, GETUTCDATE(), 'E', GETUTCDATE(), 'E')";

			_ = conn.ExecuteNonQuery(query, p =>
			{
				p.AddParameter("@staffPK", SqlDbType.UniqueIdentifier, reviewer);
				p.AddParameter("@code", SqlDbType.VarChar, 3, revCode);
				p.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				p.AddParameter("@reviewProcess", SqlDbType.UniqueIdentifier, reviewProcess);
				p.AddParameter("@reviewer", SqlDbType.UniqueIdentifier, reviewer);
				if (parent.HasValue)
				{
					p.AddParameter("@parent", SqlDbType.UniqueIdentifier, parent.Value);
				}
				else
				{
					p.AddParameter("@parent", SqlDbType.UniqueIdentifier, DBNull.Value);
				}
			});
		}

		public static void CreateReviewProposal(DbConnection conn, Guid pk, Guid node, Guid reviewee, string revieweeCode)
		{
			var query = $@"
IF NOT EXISTS ( SELECT NULL FROM dbo.GlbStaff WHERE GS_Code = @revieweeCode )
	INSERT INTO dbo.GlbStaff
		(GS_PK, GS_Code, GS_LoginName, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser)
	VALUES
		(@revieweePK, @revieweeCode, @revieweeCode, GETUTCDATE(), 'E', GETUTCDATE(), 'E');

INSERT INTO hrm.ReviewProposal
	(RRP_PK, RRP_RRN_ReviewNode, RRP_GS_Staff, RRP_SystemCreateTimeUtc, RRP_SystemCreateUser, RRP_SystemLastEditTimeUtc, RRP_SystemLastEditUser)
VALUES
	(@proposalPK, @nodePK, @revieweePK, GETUTCDATE(), 'E', GETUTCDATE(), 'E');
";

			_ = conn.ExecuteNonQuery(query, p =>
			{
				p.AddParameter("@proposalPK", SqlDbType.UniqueIdentifier, pk);
				p.AddParameter("@nodePK", SqlDbType.UniqueIdentifier, node);
				p.AddParameter("@revieweePK", SqlDbType.UniqueIdentifier, reviewee);
				p.AddParameter("@revieweeCode", SqlDbType.VarChar, 3, revieweeCode);
			});
		}

		public static void CreateReviewProposalEntitlement(DbConnection conn, Guid pk, Guid proposal, string code, int value)
		{
			var query = $@"
INSERT INTO hrm.ReviewProposalEntitlement
	(RRE_PK, RRE_RRP_Proposal, RRE_EntitlementCode, RRE_Value, RRE_SystemCreateTimeUtc, RRE_SystemCreateUser, RRE_SystemLastEditTimeUtc, RRE_SystemLastEditUser)
VALUES
	(@pk, @proposalPK, @entitlementCode, @value, GETUTCDATE(), 'E', GETUTCDATE(), 'E')
";

			_ = conn.ExecuteNonQuery(query, p =>
			{
				p.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				p.AddParameter("@proposalPK", SqlDbType.UniqueIdentifier, proposal);
				p.AddParameter("@entitlementCode", SqlDbType.VarChar, 3, code);
				p.AddParameter("@value", SqlDbType.Money, value);
			});
		}

		public static void SetReviewProcessOverrideHierarchy(DbConnection conn, Guid pk, string overrideManagerType)
		{
			var query = $@"
UPDATE dbo.ReviewProcess
SET RPR_OverrideHierarchy = @overrideManagerType, RPR_SystemLastEditTimeUtc = GETUTCDATE(), RPR_SystemLastEditUser = 'E'
WHERE RPR_PK = @pk
";

			_ = conn.ExecuteNonQuery(query, p =>
			{
				p.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				p.AddParameter("overrideManagerType", SqlDbType.VarChar, 3, overrideManagerType);
			});
		}

		public static void CreateGlbPersonLanguage(DbConnection conn, Guid pk, Guid person, string language)
		{
			var query = $@"
INSERT INTO dbo.GlbPersonLanguage
	(G7_PK, G7_Language, G7_PER_Person, G7_SystemCreateTimeUtc, G7_SystemCreateUser, G7_SystemLastEditTimeUtc, G7_SystemLastEditUser)
VALUES
	(@pk, @language, @personPK, GETUTCDATE(), 'E', GETUTCDATE(), 'E')
";

			_ = conn.ExecuteNonQuery(query, p =>
			{
				p.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				p.AddParameter("@personPK", SqlDbType.UniqueIdentifier, person);
				p.AddParameter("@language", SqlDbType.VarChar, 3, language);
			});
		}
	}
}
