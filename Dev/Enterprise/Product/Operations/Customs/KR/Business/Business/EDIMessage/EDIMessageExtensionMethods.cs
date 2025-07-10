namespace Enterprise.Customs.KR.Business
{
	public static class EDIMessageExtensionMethods
	{
		public static EDIMessage CloneIncludingInterpretation(this EDIMessage message, CusStatementHeader statementHeader)
		{
			var clonedMessage = (EDIMessage)message.Clone();
			clonedMessage.EM_Status = EDIMessage.Status.Received;
			clonedMessage.EM_LinkedObject = statementHeader;
			clonedMessage.EM_MessageInterpretation = message.EM_MessageInterpretation;

			return clonedMessage;
		}
	}
}
