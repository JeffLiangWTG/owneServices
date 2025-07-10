namespace CargoWise.Data
{
	public interface IDbConnected
	{
		DbConnection Connection { get; }
	}
}
