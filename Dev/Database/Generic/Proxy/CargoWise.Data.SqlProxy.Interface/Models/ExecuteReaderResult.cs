using Newtonsoft.Json;

namespace CargoWise.Data.SqlProxy.Interface.Models;

[method: JsonConstructor]
public class ExecuteReaderResult(SqlProxyDataReader proxyDataReader)
{
	public SqlProxyDataReader ProxyDataReader { get; set; } = proxyDataReader;

	public SqlParameterDTO[]? Parameters { get; set; }
}
