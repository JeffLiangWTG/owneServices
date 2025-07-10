using System.Data;
using CargoWise.Data;
using CargoWise.Definitions.Authentication;
using Enterprise.ZArchitecture.Environment;

namespace CargoWise.ServiceManager.Next.Launcher;

public class AccessTokenService : IAccessTokenService
{
	public bool RotateToken(TimeSpan validity, TimeSpan overlap)
	{
		return AttemptRotateToken(validity, overlap);
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWise", "CW1107:UseBusinessObjectFactory", Justification = "Used stored procedure shared with Glow")]
	public bool CheckTokenValidity(string token)
	{
		using var disposableDbConnection = Db.DisposableActionForDbConnection();
		using var command = Db.Connection.Command("TryPeekAccessToken");
		command.CommandType = CommandType.StoredProcedure;

		command.AddParameter("@Token", SqlDbType.VarChar, token);
		command.AddParameter("@Type", SqlDbType.VarChar, AccessTokenTypes.ApiDbToken);
		command.AddOutputParameter("@Scope", SqlDbType.VarChar, int.MaxValue, 0, 0, null);
		command.AddOutputParameter("@ParentId", SqlDbType.UniqueIdentifier, 0, 0, 0, null);
		command.AddOutputParameter("@ParentTableCode", SqlDbType.VarChar, 3, 0, 0, null);
		command.AddOutputParameter("@TPATResult", SqlDbType.Bit, 0, 0, 0, null);

		command.ExecuteNonQuery();
		var result = (bool)command.GetParameterValue("@TPATResult");
		return result;
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWise", "CW1107:UseBusinessObjectFactory", Justification = "Having both methods as scripts decreases risk of desync")]
	bool AttemptRotateToken(TimeSpan validity, TimeSpan overlap)
	{
		using var disposableDbConnection = Db.DisposableActionForDbConnection();
		using var command = Db.Connection.Command("CreateAccessTokenWhenRequired");
		command.CommandType = CommandType.StoredProcedure;

		command.AddParameter("@Type", SqlDbType.VarChar, AccessTokenTypes.ApiDbToken);
		command.AddParameter("@Scope", SqlDbType.VarChar, "");
		command.AddParameter("@ParentId", SqlDbType.UniqueIdentifier, Guid.Empty);
		command.AddParameter("@ParentTableCode", SqlDbType.VarChar, "SEC");
		command.AddParameter("@IsPermanent", SqlDbType.Bit, 0);
		command.AddParameter("@OverlapInSeconds", SqlDbType.Int, (int)overlap.TotalSeconds);
		command.AddParameter("@ValidityInSeconds", SqlDbType.Int, (int)validity.TotalSeconds);
		command.AddParameter("@UseCount", SqlDbType.Int, -1);
		command.AddParameter("@CreateUser", SqlDbType.VarChar, User.ServiceUserCode);
		command.AddOutputParameter("@CATResult", SqlDbType.Bit, 0, 0, 0, null);

		command.ExecuteNonQuery();
		var result = (bool)command.GetParameterValue("@CATResult");
		return result;
	}
}
