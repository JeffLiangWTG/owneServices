using System.Linq;
using CargoWise.Customs.CH.MessageContracts.MessageProviders.Edec.GoodsDeclarations;
using CargoWise.Types;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.CH.Business;

public class RuleErrorPrettyFormatter : IMessagePrettyFormatter
{
	public RuleErrorPrettyFormatter(IRuleErrorResponseDetail responseDetail)
	{
		this.responseDetail = responseDetail;
	}

	readonly IRuleErrorResponseDetail responseDetail;

	public ZString GetFormattedText()
	{
		var htmlTable = new HtmlTableCreator(ColumnTitles);

		foreach (var ruleError in responseDetail.RuleErrors)
		{
			var descriptionText = MessagePrettyFormatterHelper.GetTextByLanguageCode(ruleError.Descriptions.Select(x => (x.Language, x.Text)));

			var lineNumber = string.Empty;
			if (ruleError.Reference?.StartsWith(TraderItemIdPrefix) ?? false)
			{
				lineNumber = ruleError.Reference.Substring(TraderItemIdPrefix.Length);
			}

			htmlTable.WriteRow(new[]
			{
					ruleError.RuleName,
					descriptionText,
					lineNumber,
				});
		}

		return htmlTable.ToHtml();
	}

	string[] ColumnTitles => new[]
	{
			Res.GetString("0B6C5C23-2045-463F-BB1E-AEA5C1250B20", "Error code") ,
			Res.GetString("99B982C8-042E-4006-9C21-E0D3825A3CE7", "Error description"),
			Res.GetString("533636E3-53A5-42F6-9956-2764A36F4EB7", "Entry line #"),
		};

	const string TraderItemIdPrefix = "traderItemID:";
}
