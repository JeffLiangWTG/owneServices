namespace CargoWise.Data
{
	public interface IZSqlSaverConfiguration
	{
		bool ParameterizeInsertAndUpdateStatements { get; }
		int RowsToPostPerSqlStatement { get; }
	}
}
