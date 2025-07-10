using Newtonsoft.Json;

namespace CargoWise.Data.SqlProxy.Interface.Models;

[method: JsonConstructor]
public class BeginTransactionResult(Guid transactionId)
{
	[JsonRequired]
	public Guid TransactionId { get; set; } = transactionId;
}
