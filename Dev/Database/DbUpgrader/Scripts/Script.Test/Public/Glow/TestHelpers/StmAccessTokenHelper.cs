using System;
using System.Data;
using System.Globalization;
using CargoWise.Data;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Build.Database.Script.Public.Glow.TestHelpers;

static class StmAccessTokenHelper
{
	public static Guid CreateStmAccessToken(string token, string type, DateTime? expiry, int remainingUseCount, string scope, Guid parentID, string parentTableCode, bool isPermanent = false)
	{
		var pk = Guid.NewGuid();
		var queryTemplate = @"
INSERT INTO [{0}]
([{1}], [{2}], [{3}], [{4}], [{5}], [{6}], [{7}], [{8}], [{9}], [{10}], [{11}], [{12}], [{13}])
VALUES
(@PK, @Token, @ExpiresAtUtc, @IsPermanent, @RemainingUseCount, @Type, @Scope, @ParentID, @ParentTableCode, SYSUTCDATETIME(), '~BP', SYSUTCDATETIME(), '~BP')";

		var query = string.Format(
			CultureInfo.InvariantCulture,
			queryTemplate,
			StmAccessTokenSchema.Constants.TableName,
			StmAccessTokenSchema.Constants.PK,
			StmAccessTokenSchema.Constants.SAT_Token,
			StmAccessTokenSchema.Constants.SAT_ExpiresAt,
			StmAccessTokenSchema.Constants.SAT_IsPermanentToken,
			StmAccessTokenSchema.Constants.SAT_RemainingUseCount,
			StmAccessTokenSchema.Constants.SAT_Type,
			StmAccessTokenSchema.Constants.SAT_Scope,
			StmAccessTokenSchema.Constants.SAT_ParentId,
			StmAccessTokenSchema.Constants.SAT_ParentTableCode,
			StmAccessTokenSchema.Constants.SAT_SystemCreateTimeUtc,
			StmAccessTokenSchema.Constants.SAT_SystemCreateUser,
			StmAccessTokenSchema.Constants.SAT_SystemLastEditTimeUtc,
			StmAccessTokenSchema.Constants.SAT_SystemLastEditUser);

		using var command = Db.Connection.Command(query);
		command.AddParameter("@PK", SqlDbType.UniqueIdentifier, pk);
		command.AddParameter("@Token", SqlDbType.VarChar, token);
		command.AddParameter("@ExpiresAtUtc", SqlDbType.DateTime, (object)expiry ?? DBNull.Value);
		command.AddParameter("@IsPermanent", SqlDbType.Bit, isPermanent);
		command.AddParameter("@RemainingUseCount", SqlDbType.Int, remainingUseCount);
		command.AddParameter("@Type", SqlDbType.VarChar, type);
		command.AddParameter("@Scope", SqlDbType.VarChar, scope);
		command.AddParameter("@ParentID", SqlDbType.UniqueIdentifier, parentID);
		command.AddParameter("@ParentTableCode", SqlDbType.VarChar, parentTableCode);

		command.ExecuteNonQuery();

		return pk;
	}

	public static Guid CreateStmAccessToken(string token, string type, DateTime? expiry, int remainingUseCount, string scope, bool isPermanent = false)
		=> CreateStmAccessToken(token, type, expiry, remainingUseCount, scope ?? "ScopetyData", Guid.Empty, "GS", isPermanent);

	public static Guid CreateStmAccessToken(string token, string type, DateTime? expiry, int remainingUseCount, bool isPermanent = false)
		=> CreateStmAccessToken(token, type, expiry, remainingUseCount, scope: null, isPermanent);
}
