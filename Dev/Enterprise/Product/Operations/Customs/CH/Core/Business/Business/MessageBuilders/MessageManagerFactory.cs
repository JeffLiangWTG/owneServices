namespace Enterprise.Customs.CH.Business;

public static class MessageManagerFactory
{
	public static IMessageManager CreateNew(IMessageSendingObject messageSendingObject)
	{
		return messageSendingObject switch
		{
			ImportDeclarationMessageSendingObject impObjectToSend => new ImportDeclarationMessageManager(impObjectToSend),

			ExportDeclarationMessageSendingObject exportObjectToSend => exportObjectToSend.MessageType.ToString() switch
			{
				PassarMessageTypeList.Codes.NC016 => new NC016MessageManager(exportObjectToSend),
				PassarMessageTypeList.Codes.NC123 => new NC123MessageManager(exportObjectToSend),
				PassarMessageTypeList.Codes.NE013 => new NE013MessageManager(exportObjectToSend),
				PassarMessageTypeList.Codes.NE014 => new NE014MessageManager(exportObjectToSend),
				PassarMessageTypeList.Codes.NE015 => new NE015MessageManager(exportObjectToSend),
				PassarMessageTypeList.Codes.NE069 => new NE069MessageManager(exportObjectToSend),
				PassarMessageTypeList.Codes.NE130 => new NE130MessageManager(exportObjectToSend),
				_ => null
			},

			SupportingDocSendingObject supportingDocSendingObject => new EbdMessageManager(supportingDocSendingObject),

			EComplaintMessageSendingObject complaintSendingObject => new EComplaintMessageManager(complaintSendingObject),

			EvvRequestSendingObject evvSendingObject => new EvvMessageManager(evvSendingObject),

			_ => null
		};
	}
}
