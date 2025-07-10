namespace CargoWise.Data
{
	public interface IDedicatedSqlServerInstanceDefinition
	{
		bool IsDedicated(string serverInstanceName);
	}
}
