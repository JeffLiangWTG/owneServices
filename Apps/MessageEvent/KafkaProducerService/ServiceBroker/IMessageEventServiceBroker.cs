using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml;

namespace CargoWise.eHub.MessageEvent.KafkaProducerService.ServiceBroker
{
	public interface IMessageEventServiceBroker : IDisposable
	{
		void BeginTransaction();
		void Commit();
		IAsyncEnumerable<XmlDocument> GetNextBatchMessagesAsync();
		void Rollback();
	}
}
