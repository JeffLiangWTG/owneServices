using CargoWise.Types;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMREdiMessageFunctions
	{
		public CMREdiMessageFunctions()
		{
		}

		public ZBool DoMessagesContainAnyCARSTs(EDIMessageCollection messages)
		{
			ZBool result = false;
			foreach (EDIMessage message in messages)
			{
				if (message.EM_MessageType == CMRMessage.CMRMessageTypes.CARST)
				{
					result = true;
					break;
				}
			}
			return result;
		}
	}
}
