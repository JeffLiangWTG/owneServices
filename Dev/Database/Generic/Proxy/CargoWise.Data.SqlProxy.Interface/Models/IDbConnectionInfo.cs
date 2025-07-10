namespace CargoWise.Data.SqlProxy.Interface.Models;

public interface IDbConnectionInfo
{
	public string? ServerName { get; }

	public string? Database { get; }

	public string? UserName { get; }

	public string? Password { get; }

	public string? SuffixedApplicationName { get; }

	public int ConnectTimeout { get; }
}
