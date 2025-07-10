using CargoWise.Data;

namespace Enterprise.Integration
{
	public interface IDbRestoreSubscriber
	{
		string ReadableName { get; }

		string Run(DbConnection connection);
	}
}
