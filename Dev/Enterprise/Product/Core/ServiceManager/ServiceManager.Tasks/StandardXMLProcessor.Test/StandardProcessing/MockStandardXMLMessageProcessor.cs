using System;
using CargoWise.Types;
using Enterprise.eHubMessaging.Business;

namespace Enterprise.ServiceManager.Tasks.StandardXMLProcessor.Tests
{
	public class MockStandardXMLMessageProcessor : StandardXMLMessageProcessor
	{
		readonly bool returnNull;
		readonly Exception exceptionToThrow;

		public MockStandardXMLMessageProcessor(bool returnNull, Exception exceptionToThrow)
			: base()
		{
			this.returnNull = returnNull;
			this.exceptionToThrow = exceptionToThrow;
		}

		internal override IMessageAction GetMessageAction(ZString messageType, ZString messageSubType)
		{
			if (returnNull)
			{
				return null;
			}

			if (exceptionToThrow != null)
			{
				throw exceptionToThrow;
			}

			return base.GetMessageAction(messageType, messageSubType);
		}
	}
}
