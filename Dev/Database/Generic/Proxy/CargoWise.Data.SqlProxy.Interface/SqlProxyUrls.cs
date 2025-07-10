namespace CargoWise.Data.SqlProxy.Interface;

public static class SqlProxyUrls
{
	// BaseUrl is a dummy base URL for the GlowLoader API, so, because we are using a named pipe client, the address is not used
	public const string BaseAddress = "http://localhost";

	public const string Scalar = "/api/Sql/Scalar";
	public const string Reader = "/api/Sql/Reader";
	public const string NonQuery = "/api/Sql/NonQuery";
	public const string BulkCopy = "/api/Sql/BulkCopy";
	public const string BeginTransaction = "/api/Sql/BeginTran";
	public const string CommitTransaction = "/api/Sql/Commit/{transactionId}";
	public const string RollbackTransaction = "/api/Sql/Rollback/{transactionId}";

	public static string? ListeningPort { get; set; }

	public static string BaseUrl => FormattableString.Invariant($"{BaseAddress}:{ListeningPort}");

	public static string ScalarUrl(string? baseUrl = null)
		=> new Uri(BaseUri(baseUrl), Scalar).ToString();

	public static string ReaderUrl(string? baseUrl = null)
		=> new Uri(BaseUri(baseUrl), Reader).ToString();

	public static string NonQueryUrl(string? baseUrl = null)
		=> new Uri(BaseUri(baseUrl), NonQuery).ToString();

	public static string BulkCopyUrl(string? baseUrl = null)
		=> new Uri(BaseUri(baseUrl), BulkCopy).ToString();

	public static string BeginTransUrl(string? baseUrl = null)
		=> new Uri(BaseUri(baseUrl), BeginTransaction).ToString();

	public static string CommitTransUrl(Guid transactionId, string? baseUrl = null)
		=> new Uri(BaseUri(baseUrl), CommitTransaction.Replace("{transactionId}", transactionId.ToString("D"))).ToString();

	public static string RollbackTransUrl(Guid transactionId, string? baseUrl = null)
		=> new Uri(BaseUri(baseUrl), RollbackTransaction.Replace("{transactionId}", transactionId.ToString("D"))).ToString();

	static Uri BaseUri(string? baseUrl) => new Uri(baseUrl ?? BaseUrl, UriKind.Absolute);
}
