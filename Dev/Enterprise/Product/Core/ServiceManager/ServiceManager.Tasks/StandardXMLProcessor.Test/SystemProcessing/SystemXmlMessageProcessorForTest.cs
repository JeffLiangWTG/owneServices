using System;
using CargoWise.Types;
using Enterprise.eHubMessaging.Business;

namespace Enterprise.ServiceManager.Tasks.StandardXMLProcessor.Tests
{
	sealed class SystemXmlMessageProcessorForTest : SystemXmlMessageProcessor
	{
		public void AddSupportedMessage_Exposed(string code, string objectFactoryTypeId)
		{
			base.AddSupportedMessage(code, objectFactoryTypeId);
		}

		public void AddSupportedMessage_Exposed(string code, Type actionType)
		{
			base.AddSupportedMessage(code, actionType);
		}

		public IMessageAction GetMessageAction_Exposed(ZString messageType, ZString messageSubType)
		{
			return base.GetMessageAction(messageType, messageSubType);
		}
	}
}
