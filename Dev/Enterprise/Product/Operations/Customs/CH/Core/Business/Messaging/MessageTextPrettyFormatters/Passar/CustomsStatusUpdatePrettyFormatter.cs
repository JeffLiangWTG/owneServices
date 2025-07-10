using System.Collections.Generic;
using System.Linq;
using System.Net;
using CargoWise.Customs.CH.MessageContracts.MessageProviders.Passar;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.CH.Business;

public abstract class CustomsStatusUpdatePrettyFormatter<TResponseDetail> : BasePassarResponsePrettyFormatter<TResponseDetail>, IMessagePrettyFormatter
{
	protected CustomsStatusUpdatePrettyFormatter(BusinessObjectFactory factory, TResponseDetail responseDetail) : base(factory, responseDetail) { }

	public ZString GetFormattedText()
	{
		var htmlBuilder = new ZStringBuilder();

		htmlBuilder
			.Append("<h2>")
			.Append(WebUtility.HtmlEncode(Title))
			.Append("</h2>");

		if (ResponseDetail is IPassarResponseWithMRN responseWithMRN && !string.IsNullOrEmpty(responseWithMRN.MRN))
		{
			AppendDetail(htmlBuilder, MRNLabel, $"{responseWithMRN.MRN}.{responseWithMRN.MRNVersion}");
		}

		if (ResponseDetail is IPassarResponseWithGDRN responseWithGDRN && !string.IsNullOrEmpty(responseWithGDRN.GDRN))
		{
			AppendDetail(htmlBuilder, GDRNLabel, $"{responseWithGDRN.GDRN}.{responseWithGDRN.GDRNVersion}");
		}

		foreach (var (label, text) in GetDetails())
		{
			AppendDetail(htmlBuilder, label, text);
		}

		AppendFooter(htmlBuilder);

		return htmlBuilder.ToString();
	}

	void AppendDetail(ZStringBuilder htmlBuilder, string label, string text)
	{
		htmlBuilder
			.Append((NoResString)"<p><b>")
			.Append(WebUtility.HtmlEncode(label))
			.Append((NoResString)":</b> ")
			.Append(WebUtility.HtmlEncode(text ?? string.Empty))
			.Append((NoResString)"</p>");
	}

	protected virtual IEnumerable<(string label, string text)> GetDetails() => Enumerable.Empty<(string label, string text)>();

	protected virtual void AppendFooter(ZStringBuilder htmlBuilder) { }

	protected abstract string Title { get; }

	static string MRNLabel => Res.GetString("129C8372-0787-47D7-84A6-8180F30738DA", "MRN");

	static string GDRNLabel => Res.GetString("2D7E6059-BD1C-4FCB-80F9-CE2277D54E7A", "Goods Declaration Reference Number");
}
