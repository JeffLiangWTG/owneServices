using System;
using System.Reflection;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.MessageProcessors;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.MessageProcessors.Testing
{
	sealed class MessageProcessorProviderReflectionTest : TestCase
	{
		public void TestProvidesCorrectMessageProcessorsForEachApplicationIdentifierAttribute()
		{
			var logger = new LoggingInformation();
			var provider = new MessageProcessorProvider();

			foreach (Type type in Assembly.Load("Enterprise.Customs.CA.Business").GetTypes())
			{
				if (type.IsSubclassOf(typeof(MessageProcessor)) && !type.IsAbstract)
				{
					object[] attributes = type.GetCustomAttributes(typeof(ApplicationIdentifierAttribute), false);

					foreach (ApplicationIdentifierAttribute attribute in attributes)
					{
						AssertEquals(string.Format("Message Processor for '{0}' Application Identifier", attribute.ApplicationIdentifier), type, provider.GetProcessor(attribute.ApplicationIdentifier, logger).GetType());
					}
				}
			}
		}
	}
}
