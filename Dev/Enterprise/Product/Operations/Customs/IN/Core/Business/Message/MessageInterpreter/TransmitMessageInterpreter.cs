using CargoWise.Types;

namespace Enterprise.Customs.IN.Business;

sealed class TransmitMessageInterpreter : BaseMessageInterpreter
{
	public TransmitMessageInterpreter(EDIMessage message) : base(message)
	{
	}

	protected override ZString FormatMessageText() => HtmlEncode(Message.EM_MessageText);
}
