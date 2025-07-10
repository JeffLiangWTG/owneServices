namespace Enterprise.Customs.Common.MessageBuilders
{
	public interface IEDIFACTMessageAttacheeAndEDIFACTMessageStatusCalculatorProvider : IEDIFACTMessageAttachee
	{
		EDIFACTMessageStatusCalculator GetCalculator(string country);
	}
}
