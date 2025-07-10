using Newtonsoft.Json;

namespace CargoWise.Data.SqlProxy.Interface.Models;

[method: JsonConstructor]
public class ExecuteBulkCopyResult(int rowsCopied)
{
	[JsonRequired]
	public int RowsCopied { get; set; } = rowsCopied;
}
