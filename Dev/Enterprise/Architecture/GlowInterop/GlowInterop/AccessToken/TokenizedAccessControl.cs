using System;
using System.Data;
using CargoWise.Data;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ZArchitecture.GlowInterop
{
	public sealed class TokenizedAccessControl : ITokenizedAccessControl
	{
		bool ITokenizedAccessControl.TryConsume(string accessToken, string accessTokenType, out AccessTokenInfo info)
		{
			using (var command = Db.Connection.Command("TryConsumeAccessToken"))
			{
				command.CommandType = CommandType.StoredProcedure;

				command.AddParameter("@Token", SqlDbType.VarChar, accessToken);
				command.AddParameter("@Type", SqlDbType.VarChar, accessTokenType);
				command.AddOutputParameter("@Scope", SqlDbType.VarChar, int.MaxValue, 0, 0, null);
				command.AddOutputParameter("@ParentId", SqlDbType.UniqueIdentifier, 0, 0, 0, null);
				command.AddOutputParameter("@ParentTableCode", SqlDbType.VarChar, 3, 0, 0, null);
				command.AddOutputParameter("@TCATResult", SqlDbType.Bit, 0, 0, 0, null);

				command.ExecuteNonQuery();
				var result = (bool)command.GetParameterValue("@TCATResult");

				if (!result)
				{
					info = default(AccessTokenInfo);
					return false;
				}

				info = new AccessTokenInfo(
						(string)command.GetParameterValue("@Scope"),
						(Guid)command.GetParameterValue("@ParentId"),
						(string)command.GetParameterValue("@ParentTableCode"));
				return true;
			}
		}

		bool ITokenizedAccessControl.TryPeek(string accessToken, string accessTokenType, out AccessTokenInfo info)
		{
			using (var command = Db.Connection.Command("TryPeekAccessToken"))
			{
				command.CommandType = CommandType.StoredProcedure;

				command.AddParameter("@Token", SqlDbType.VarChar, accessToken);
				command.AddParameter("@Type", SqlDbType.VarChar, accessTokenType);
				command.AddOutputParameter("@Scope", SqlDbType.VarChar, int.MaxValue, 0, 0, null);
				command.AddOutputParameter("@ParentId", SqlDbType.UniqueIdentifier, 0, 0, 0, null);
				command.AddOutputParameter("@ParentTableCode", SqlDbType.VarChar, 3, 0, 0, null);
				command.AddOutputParameter("@TPATResult", SqlDbType.Bit, 0, 0, 0, null);

				command.ExecuteNonQuery();
				var result = (bool)command.GetParameterValue("@TPATResult");

				if (!result)
				{
					info = default(AccessTokenInfo);
					return false;
				}

				info = new AccessTokenInfo(
						(string)command.GetParameterValue("@Scope"),
						(Guid)command.GetParameterValue("@ParentId"),
						(string)command.GetParameterValue("@ParentTableCode"));
				return true;
			}
		}

		bool ITokenizedAccessControl.TryCreate(string token, string type, bool isPermanent, DateTime? expiresAtUtc, int useCount, AccessTokenInfo info)
		{
			bool result;
			using (var command = Db.Connection.Command("CreateAccessToken"))
			{
				command.CommandType = CommandType.StoredProcedure;

				command.AddParameter("@Token", SqlDbType.VarChar, token);
				command.AddParameter("@Type", SqlDbType.VarChar, type);
				command.AddParameter("@Scope", SqlDbType.VarChar, info.Scope);
				command.AddParameter("@ParentId", SqlDbType.UniqueIdentifier, info.ParentId);
				command.AddParameter("@ParentTableCode", SqlDbType.VarChar, info.ParentTableCode);
				command.AddParameter("@IsPermanent", SqlDbType.Bit, isPermanent);
				command.AddParameter("@ExpiresAtUtc", SqlDbType.DateTime, (object)expiresAtUtc ?? DBNull.Value);
				command.AddParameter("@UseCount", SqlDbType.Int, useCount);
				command.AddParameter("@CreateUser", SqlDbType.VarChar, EnvProxy.Instance.CurrentUser.Initials);
				command.AddOutputParameter("@CATResult", SqlDbType.Bit, 0, 0, 0, DBNull.Value);

				command.ExecuteNonQuery();
				result = (bool)command.GetParameterValue("@CATResult");
			}

			return result;
		}
	}
}
