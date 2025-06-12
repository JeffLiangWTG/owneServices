using System.Data;
using System.Data.Common;
using System.Threading.Tasks;

namespace CargoWise.eHub.MessageEvent.KafkaProducerService.ServiceBroker
{
	public interface ISqlCommandWrapper : IDbCommand
	{
		Task<DbDataReader> ExecuteReaderAsync();
	}
}
