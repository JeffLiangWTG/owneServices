using System;
using System.Collections.Concurrent;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.MailManager.Integration;

namespace Enterprise.MailManager.MessageProcessor
{
	[Serializable]
	public class MessageProcessorFactory : IMessageProcessorFactory
	{
		public MessageProcessorFactory(string serviceTaskCode, IMessageFilterConfig[] messageFilterConfigs)
		{
			_messageFilterConfigs = Array.FindAll(messageFilterConfigs, config => config.ServiceTaskCode.Equals(serviceTaskCode, StringComparison.OrdinalIgnoreCase));
			_processors = new ConcurrentDictionary<Type, object>();
		}

		public IMessageProcessorContext GetContext(ILogger logger)
		{
			return new MessageProcessorContext(logger);
		}

		public IMessageProcessor<T> GetProcessor<T>(string tableName)
		{
			var type = typeof(T);
			var result = _processors.GetOrAdd(type, (key) =>
			{
				var filterConfigsForTableAndTask = Array.FindAll(_messageFilterConfigs, config => config.TableName.Equals(tableName, StringComparison.OrdinalIgnoreCase));
				return new MessageProcessor<T>(filterConfigsForTableAndTask);
			});

			return (IMessageProcessor<T>)result;
		}

		public IMessageProcessor<T> GetProcessor<T>()
			=> GetProcessor<T>(BusinessObjectFactory.GetTableSchemaFromType(typeof(T)).TableName);

		readonly IMessageFilterConfig[] _messageFilterConfigs;
		[NonSerialized]
		readonly ConcurrentDictionary<Type, object> _processors;
	}
}

