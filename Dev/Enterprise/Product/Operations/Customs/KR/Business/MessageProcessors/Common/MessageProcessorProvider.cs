using System;
using System.Collections.Generic;
using System.Reflection;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	public class MessageProcessorProvider
	{
		public IMessageProcessor GetProcessor(string messageType, Assembly assembly)
		{
			List<Type> typeList = new List<Type>();
			foreach (var type in assembly.GetTypes())
			{
				if (typeof(IMessageProcessor).IsAssignableFrom(type) && !type.IsAbstract)
				{
					foreach (MessageTypeAttribute attribute in type.GetCustomAttributes(typeof(MessageTypeAttribute), false))
					{
						if (attribute.MessageType == messageType)
						{
							typeList.Add(type);
							if (typeList.Count > 1)
							{
								throw new InvalidOperationException("Two or more identical attributes exist.");
							}
						}
					}
				}
			}
			return typeList.Count == 1 ? (IMessageProcessor)Activator.CreateInstance(typeList[0]) : null;
		}

		public IMessageProcessor GetProcessor(string messageType)
		{
			return GetProcessor(messageType, GetType().Assembly);
		}
	}
}
