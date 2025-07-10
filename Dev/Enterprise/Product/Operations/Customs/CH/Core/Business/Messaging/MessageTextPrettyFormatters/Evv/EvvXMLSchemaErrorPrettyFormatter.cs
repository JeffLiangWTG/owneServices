using System;
using System.Net;
using CargoWise.Customs.CH.MessageContracts.MessageProviders.Edec.Evv;
using CargoWise.Types;

namespace Enterprise.Customs.CH.Business;

public class EvvXMLSchemaErrorPrettyFormatter : IMessagePrettyFormatter
{
	public EvvXMLSchemaErrorPrettyFormatter(IEvvXMLSchemaErrorsResponseProvider responseProvider)
	{
		this.responseProvider = responseProvider;
	}

	readonly IEvvXMLSchemaErrorsResponseProvider responseProvider;

	public ZString GetFormattedText()
	{
		if (responseProvider is null)
		{
			return FormattableString.Invariant($"<h2>{MessagePrettyFormatterHelper.MessageIsEmpty}</h2>");
		}

		var htmlBuilder = new ZStringBuilder();
		htmlBuilder.Append($"<h2>{RequestRejectedTitle}</h2>");
		htmlBuilder.Append($"<p>{WebUtility.HtmlEncode(MessagePrettyFormatterHelper.RejectionDateTimeLabel)}: {responseProvider.RejectionDateTime:yyyy-MM-dd HH:mm:ss}</p>");
		htmlBuilder.Append($"<h3>{MessagePrettyFormatterHelper.SchemaErrorsTitle}</h3>");
		htmlBuilder.Append($"<p>{responseProvider.ErrorMessage}</p>");
		return htmlBuilder.ToString();
	}

	#region Labels

	static string RequestRejectedTitle => WebUtility.HtmlEncode(Res.GetString("FB458100-2550-4F94-8E18-12C9AE124346", "EVV Request has been rejected"));

	#endregion
}
