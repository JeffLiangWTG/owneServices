namespace Enterprise.Customs.CH.Business;

public interface ICHNctsMessagePrettyFormatterProvider
{
	IMessagePrettyFormatter GetFormatter(CHEDIMessage message);
}
