using System;
using System.Collections.Generic;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.CA.Business.MessageProcessors
{
	class MessageProcessorProvider
	{
		public CustomsMessageProcessor GetProcessor(string applicationIdentifier, LoggingInformation logger)
		{
			CustomsMessageProcessor result = null;
			Type processorType;
			if (GetApplicationIdentifierMessageProcessorDictionary().TryGetValue(applicationIdentifier, out processorType))
			{
				result = (CustomsMessageProcessor)Activator.CreateInstance(processorType, new object[] { logger });
			}
			return result;
		}

		public CustomsMessageProcessor GetProcessor(EDIMessage ediMessage, LoggingInformation logger)
		{
			CustomsMessageProcessor result = null;
			Type processorType;
			if (GetEDIMessageTypeMessageProcessorDictionary().TryGetValue(ediMessage.GetType(), out processorType))
			{
				result = (CustomsMessageProcessor)Activator.CreateInstance(processorType, new object[] { logger });
			}
			return result;
		}

		#region Implementation

		Dictionary<Type, Type> GetEDIMessageTypeMessageProcessorDictionary()
		{
			return new Dictionary<Type, Type>
					{
						{ typeof(B3Message), typeof(B3ResponseMessageProcessor) },
						{ typeof(SyntaxErrorMessage), typeof(ImportSyntaxErrorResponseMessageProcessor) },
						{ typeof(QueryMessage), typeof(QueryResponseMessageProcessor) },
						{ typeof(K84Message), typeof(K84ReportMessageProcessor) },
						{ typeof(TCPMessage), typeof(TCPResponseMessageProcessor) },
						{ typeof(RSFMessage), typeof(RSFResponseMessageProcessor) }
					};
		}

		Dictionary<string, Type> GetApplicationIdentifierMessageProcessorDictionary()
		{
			var dictionary = new Dictionary<string, Type>();

			foreach (Type type in GetType().Assembly.GetTypes())
			{
				if (type.IsSubclassOf(typeof(MessageProcessor)) && !type.IsAbstract)
				{
					foreach (ApplicationIdentifierAttribute attribute in type.GetCustomAttributes(typeof(ApplicationIdentifierAttribute), false))
					{
						dictionary.Add(attribute.ApplicationIdentifier, type);
					}
				}
			}
			return dictionary;
		}

		#endregion
	}
}
