using CargoWise.Types;
using Enterprise.eHubMessaging.Business;

namespace Enterprise.Client.EDI.ServiceTasks.Tests
{
	public class EDISystemXmlMessageProcessorForTest : EDISystemXmlMessageProcessor
	{
		public IMessageAction GetMessageAction_Exposed(ZString messageType, ZString messageSubType)
		{
			return base.GetMessageAction(messageType, messageSubType, null);
		}
	}
}
