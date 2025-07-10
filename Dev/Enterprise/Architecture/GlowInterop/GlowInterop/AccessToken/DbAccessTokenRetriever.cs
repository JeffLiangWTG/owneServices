using System;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Data;
using CargoWise.Definitions.Authentication;

namespace Enterprise.ZArchitecture.GlowInterop;

public sealed class DbAccessTokenRetriever : CargoWise.SystemToSystemTrust.IDbAccessTokenRetriever
{
	[SuppressMessage("CargoWiseOne", "CW1107:Do Not Use Db.Connection Methods", Justification = "Need to access DB stored procedure shared with Glow")]
	public Task<string> GetDbAccessTokenAsync(CancellationToken cancellationToken)
	{
		using var command = Db.Connection.Command("GetCurrentActiveAccessToken");
		command.CommandType = CommandType.StoredProcedure;

		command.AddParameter("@Type", SqlDbType.VarChar, AccessTokenTypes.ApiDbToken);
		command.AddOutputParameter("@AccessToken", SqlDbType.VarChar, -1, 0, 0, null);

		command.ExecuteNonQuery();
		var parameterValue = command.GetParameterValue("@AccessToken");
		var result = parameterValue == DBNull.Value ? null : (string)parameterValue;

		return Task.FromResult(result);
	}
}
