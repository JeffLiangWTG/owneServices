using System;
using System.Linq;
using System.Net;
using CargoWise.Customs.CH.MessageContracts.MessageProviders.Edec.Evv;
using CargoWise.Types;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.CH.Business;

public class EvvRuleErrorPrettyFormatter : IMessagePrettyFormatter
{
	public EvvRuleErrorPrettyFormatter(IEvvRuleErrorsResponseProvider responseProvider)
	{
		this.responseProvider = responseProvider;
	}

	readonly IEvvRuleErrorsResponseProvider responseProvider;

	public ZString GetFormattedText()
	{
		if (responseProvider is null)
		{
			return FormattableString.Invariant($"<h2>{MessagePrettyFormatterHelper.MessageIsEmpty}</h2>");
		}

		var htmlBuilder = new ZStringBuilder();
		htmlBuilder.Append($"<h2>{RequestRejectedTitle}</h2>");
		htmlBuilder.Append($"<p>{WebUtility.HtmlEncode(MessagePrettyFormatterHelper.RejectionDateTimeLabel)}: {responseProvider.RejectionDateTime:yyyy-MM-dd HH:mm:ss}</p>");

		htmlBuilder.Append($"<h3>{MessagePrettyFormatterHelper.ErrorsTitle}</h3>");
		var errorsTable = new HtmlTableCreator(ColumnTitles);
		foreach (var ruleError in responseProvider.RuleErrors)
		{
			errorsTable.WriteRow(WebUtility.HtmlEncode(ruleError.RuleName), WebUtility.HtmlEncode(MessagePrettyFormatterHelper.GetTextByLanguageCode(ruleError.Descriptions.Select(x => (x.Language, x.Text)))));
		}

		htmlBuilder.Append(errorsTable.ToHtml());
		return htmlBuilder.ToString();
	}

	#region Labels

	static string RequestRejectedTitle => WebUtility.HtmlEncode(Res.GetString("B6301F19-9E95-43B0-BC80-3A45E5279AB3", "EVV Request has been rejected"));

	string[] ColumnTitles => new[] { RuleNameLabel, DescriptionLabel };
	static string RuleNameLabel => WebUtility.HtmlEncode(Res.GetString("F5A46BDE-F809-445F-A318-C1D9E5BE8961", "Code"));
	static string DescriptionLabel => WebUtility.HtmlEncode(Res.GetString("18298CEB-D7DA-4CA5-9313-A39D2C0AF031", "Description"));

	#endregion
}
