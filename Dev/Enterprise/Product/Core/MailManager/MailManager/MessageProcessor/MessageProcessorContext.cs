using System;
using System.Collections.Generic;
using Enterprise.Integration;
using Enterprise.MailManager.Integration;

namespace Enterprise.MailManager.MessageProcessor
{
	class MessageProcessorContext : IMessageProcessorContext
	{
		public MessageProcessorContext(ILogger logger)
		{
			Logger = logger;
		}

		public ILogger Logger { get; private set; }

		public T GetFilterInstance<T>() where T : new()
		{
			object result;
			if (!_instances.TryGetValue(typeof(T), out result))
			{
				result = new T();
				_instances.Add(typeof(T), result);
			}

			return (T)result;
		}

		readonly Dictionary<Type, object> _instances = new Dictionary<Type, object>();
	}
}

