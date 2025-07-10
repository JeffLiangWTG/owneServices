using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.NCTS.Business;

public class NctsEdiMessagePrettier<TDataProvider>
	where TDataProvider : class, INCTSPrettierData
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Constant strings")]
	protected const string Css = "<style>body, p, td { font-family: Verdana, Arial, Helvetica, sans-serif; font-size: 13px; } table .tdGoodsItemsTitle { border-bottom: solid 1px; width: 25%; }</style>";

	protected readonly EDIMessage message;
	protected readonly TDataProvider dataProvider;

	public NctsEdiMessagePrettier(EDIMessage message, TDataProvider dataProvider)
	{
		this.message = Argument.NotNull(message, nameof(message));
		this.dataProvider = Argument.NotNull(dataProvider, nameof(dataProvider));
	}

	public string MakeOutboundPrettyForInterpretation(NctsHeader header) => header.IsPhase5 ? MakeOutboundPrettyForPhase5Interpretation() : message.HumanReadableMessage.ToString();

	protected virtual string MakeOutboundPrettyForPhase5Interpretation()
	{
		var pretty = new StringBuilder(Css);
		pretty.Append(HtmlHelper.ToH3IfNotEmpty(Res.GetString("764F77E1-86B8-4948-B54E-7463C2C33E18", "NCTS Message (Phase 5)")));

		var sharedFields = new NCTSPrettierSharedFields();
		FillSharedFields(sharedFields);
		pretty.Append(GetSharedTableDisplay(sharedFields));

		foreach (var additionalBlock in AdditionalBlocks)
		{
			pretty.Append(additionalBlock);
		}
		return pretty.ToString();
	}

	public string MakeInboundPrettyForInterpretation(NctsHeader header) => header.IsPhase5 ? MakeInboundPrettyForPhase5Interpretation() : message.HumanReadableMessage.ToString();

	protected virtual string MakeInboundPrettyForPhase5Interpretation() => ZString.Empty;

	protected virtual void FillSharedFields(NCTSPrettierSharedFields sharedFields) => dataProvider.FillSharedFields(sharedFields);

	protected virtual IReadOnlyCollection<INCTSPrettierAdditionalBlock> AdditionalBlocks => dataProvider.AdditionalBlocks;

	protected virtual string GetSharedTableDisplay(IReadOnlyCollection<(string Key, string Value)> fields)
	{
		if (fields.Count == 0)
		{
			return string.Empty;
		}

		var tableCreator = HtmlHelper.GetHtmlTableCreator();
		tableCreator.WriteRowWithFormatting(new CellWithFormatting[] {
			new (Res.GetString("241A8ECC-CE07-4676-8240-6E549FDB18D1", "Name"), (NoResString)"width", "25%"),
			new (Res.GetString("68112D9B-0841-4D49-B139-109138189919", "Value"), (NoResString)"width", "75%") });
		foreach (var (key, value) in fields.Where(x => !string.IsNullOrEmpty(x.Value)))
		{
			tableCreator.WriteRow(key, value);
		}

		return tableCreator.ToHtml();
	}
}
