using System;
using CargoWise.Common;
using Enterprise.Environment;

namespace Enterprise.Messaging.Business.MessageProcessor.Testing
{
	static class BaseOutgoingMessageProcessorTestHelper
	{
		public static IDisposable SetMessagesPerInterchange(int count)
		{
			var originalValue = Env.Registry.MessagesPerInterchange;
			return new DisposableAction(() => Env.Registry.MessagesPerInterchange = count, () => Env.Registry.MessagesPerInterchange = originalValue);
		}
	}
}
