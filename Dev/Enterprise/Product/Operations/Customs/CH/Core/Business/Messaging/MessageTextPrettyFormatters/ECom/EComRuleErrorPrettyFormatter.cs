using System;
using System.Linq;
using System.Net;
using CargoWise.Customs.CH.MessageContracts.MessageProviders.Edec.ECom;
using CargoWise.Types;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.CH.Business;

public class EComRuleErrorPrettyFormatter : IMessagePrettyFormatter
{
	public EComRuleErrorPrettyFormatter(IEComRuleErrorsResponseDetail responseDetail)
	{
		this.responseDetail = responseDetail;
	}

	readonly IEComRuleErrorsResponseDetail responseDetail;

	public ZString GetFormattedText()
	{
		if (responseDetail is null)
		{
			return FormattableString.Invariant($"<h2>{MessagePrettyFormatterHelper.MessageIsEmpty}</h2>");
		}

		var htmlBuilder = new ZStringBuilder();
		htmlBuilder.Append($"<h2>{RequestRejectedTitle}</h2>");
		htmlBuilder.Append($"<p>{WebUtility.HtmlEncode(MessagePrettyFormatterHelper.RejectionDateTimeLabel)}: {responseDetail.RejectionDateTime:yyyy-MM-dd HH:mm:ss}</p>");
		htmlBuilder.Append($"<p>{WebUtility.HtmlEncode(RequestNumberLabel)}: {WebUtility.HtmlEncode(responseDetail.RequestNumber)}</p>");

		htmlBuilder.Append($"<h3>{MessagePrettyFormatterHelper.ErrorsTitle}</h3>");
		var errorsTable = new HtmlTableCreator(ColumnTitles);
		foreach (var ruleError in responseDetail.RuleErrors)
		{
			errorsTable.WriteRow(ruleError.RuleName, MessagePrettyFormatterHelper.GetTextByLanguageCode(ruleError.Descriptions.Select(x => (x.Language, x.Text))));
		}

		htmlBuilder.Append(errorsTable.ToHtml());
		return htmlBuilder.ToString();
	}

	#region Labels

	static string RequestRejectedTitle => Res.GetString("2AD894C2-E36D-49C0-825F-0035DE9E0E27", "eCom Request has been rejected");
	static string RequestNumberLabel => Res.GetString("D1D395E8-461E-4442-8A45-9DF0B8ECD076", "Request Number");

	string[] ColumnTitles => new[] { RuleNameLabel, DescriptionLabel };
	static string RuleNameLabel => Res.GetString("CCF2DEF7-5504-4D6B-B3D0-EB23B33AE620", "Rule Name");
	static string DescriptionLabel => Res.GetString("89B7D259-476F-45EC-A3C0-9B35EF240D7E", "Description");

	#endregion
}
