namespace Enterprise.Customs.CA.Business
{
	public static class GOVCBRMessageProvider
	{
		public static GOVCBRMessageWrapper GetMessageWrapper(EDIMessage message)
		{
			GOVCBRMessageWrapper messageWrapper = null;
			if (message is ACIForwarderMessage aCIForwarderMessage && aCIForwarderMessage.GOVCBR != null)
			{
				messageWrapper = new D11BGOVCBRMessageWrapper(aCIForwarderMessage);
			}
			else if (message is UniversalEventMessage universalEventMessage && universalEventMessage.GOVCBR != null)
			{
				messageWrapper = new D13AGOVCBRMessageWrapper(universalEventMessage);
			}
			else
			{
				messageWrapper = new GOVCBRMessageWrapper(message);
			}

			return messageWrapper;
		}
	}
}
