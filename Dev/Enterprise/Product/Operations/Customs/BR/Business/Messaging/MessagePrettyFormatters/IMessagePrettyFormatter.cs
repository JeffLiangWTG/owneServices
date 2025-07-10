using CargoWise.Types;

namespace Enterprise.Customs.BR.Business
{
	public interface IMessagePrettyFormatter
	{
		ZString GetFormattedMessageText();
	}
}
