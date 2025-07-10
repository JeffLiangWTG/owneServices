using Newtonsoft.Json;

namespace CargoWise.Data.SqlProxy.Interface.Models;

[method: JsonConstructor]
public class SqlProxyDataReader(List<ResultSet> resultSets)
{
	[JsonRequired]
	public List<ResultSet> ResultSets { get; set; } = resultSets;
}
