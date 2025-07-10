
using Enterprise.Customs.Business;

namespace Enterprise.Customs.BR.Business
{
	public static class MessageManagerCreator
	{
		public static BaseMessageManager CreateNew(BaseMessageSendingObject messageSendingObject)
		{
			BaseMessageManager messageManager = null;

			if (messageSendingObject is ExportDeclarationMessageSendingObject expObjectToSend)
			{
				messageManager = new ExportDeclarationMessageManager(expObjectToSend);
			}
			else if (messageSendingObject is DuimpMessageSendingObject duimpHeaderObjectToSend)
			{
				messageManager = new DuimpMessageManager(duimpHeaderObjectToSend);
			}
			else if (messageSendingObject is ImportSiscomexMessageSendingObject iswObjectToSend)
			{
				messageManager = new ImportSiscomexDeclarationMessageManager(iswObjectToSend);
			}
			else if (messageSendingObject is ImportLicenseMessageSendingObject licObjectToSend)
			{
				messageManager = new ImportLicenseMessageManager(licObjectToSend);
			}
			else if (messageSendingObject is LPCODeclarationMessageSendingObject lpcoObjectToSend)
			{
				messageManager = new LPCODeclarationMessageManager(lpcoObjectToSend);
			}
			else if (messageSendingObject is SubscriptionMessageSendingObject subObjectToSend)
			{
				messageManager = new SubscriptionMessageManager(subObjectToSend);
			}
			else if (messageSendingObject is ForeignOperatorMessageSendingObject foreignObjectToSend)
			{
				messageManager = new ForeignOperatorMessageManager(foreignObjectToSend);
			}
			else if (messageSendingObject is LPCOMessageSendingObject permitObjectToSend)
			{
				messageManager = new LPCOMessageManager(permitObjectToSend);
			}
			else if (messageSendingObject is GoodsCatalogMessageSendingObject catalogObjectToSend)
			{
				messageManager = new GoodsCatalogMessageManager(catalogObjectToSend);
			}

			return messageManager;
		}
	}
}
