using CargoWise.Types;
using Enterprise.Customs.KR.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using static Enterprise.Customs.KR.Messaging.MessageFunctions;

namespace Enterprise.Customs.KR.GUI
{
	public static class IJobDeclarationMessageSendingObjectParentExtensionMethods
	{
		public static void SendMessage(this IJobDeclarationMessageSendingObjectParent wrapper, MessageFunctionCode messageFunctionCode, JobDeclaration declaration)
		{
			using (var sender = wrapper.GetMessageSender(messageFunctionCode))
			{
				if (sender != null)
				{
					var resultMessageCount = sender.Send();
					if (EDIMenuMethods.TryFactorySave(declaration))
					{
						Globals.Message.Show(ZString.Format(MessageSender.MessageSendSuccessful, resultMessageCount));
					}
				}
				else
				{
					Globals.Message.Show((NoResString)"No sender exists. More development to be done.");
				}
			}
		}
	}
}
