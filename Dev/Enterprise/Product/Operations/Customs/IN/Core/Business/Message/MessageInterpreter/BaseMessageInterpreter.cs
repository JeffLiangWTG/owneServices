using System.Net;
using CargoWise.Common;
using CargoWise.Types;

namespace Enterprise.Customs.IN.Business;

abstract class BaseMessageInterpreter : IMessageInterpreter
{
	protected BaseMessageInterpreter(EDIMessage message)
	{
		Message = Argument.NotNull(message, nameof(message));
	}

	protected EDIMessage Message { get; }

	ZString IMessageInterpreter.GetMessageInterpretation()
	{
		var result = new ZStringBuilder();
		var text = Message.EM_MessageText;
		if (!text.IsEmpty)
		{
			result.Append(DefaultStyle);
			result.Append(FormatMessageText());
		}
		return result.ToString();
	}

	protected abstract ZString FormatMessageText();

	protected string HtmlEncode(ZString data)
	{
		return WebUtility.HtmlEncode(data).Replace("\x001d", "&harr;").Replace("\r\n", "<br>");
	}

	protected const string DefaultStyle = @"<style>
	body, p, td 
	{
		font-family: Microsoft Sans Serif; 
		font-size: 8.25pt;
		margin:4pt;
	}
</style>";
}
