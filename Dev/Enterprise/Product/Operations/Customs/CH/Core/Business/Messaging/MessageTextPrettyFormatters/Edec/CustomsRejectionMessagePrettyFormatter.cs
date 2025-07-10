using System;
using System.Net;
using CargoWise.Customs.CH.MessageContracts.MessageProviders.Edec.GoodsDeclarations;
using CargoWise.Types;

namespace Enterprise.Customs.CH.Business;

public class CustomsRejectionMessagePrettyFormatter : IMessagePrettyFormatter
{
	public CustomsRejectionMessagePrettyFormatter(ICustomsRejectionResponseDetail responseDetail)
	{
		this.responseDetail = responseDetail;
	}

	readonly ICustomsRejectionResponseDetail responseDetail;

	public ZString GetFormattedText()
	{
		if (responseDetail is null)
		{
			return FormattableString.Invariant($"<h2>{EmptyMessage}</h2>");
		}

		var content = $"<h2>{WebUtility.HtmlEncode(HeaderText)}</h2>";
		if (responseDetail.IsCorrectionRejection)
		{
			content += $"<p>{WebUtility.HtmlEncode(ContentCorrectionRejectionText)}";
		}
		else if (responseDetail.IsCancellationRejection)
		{
			content += $"<p>{WebUtility.HtmlEncode(ContentCancellationRejectionText)}";
		}
		return content;
	}

	static string EmptyMessage => Res.GetString("E954E4F3-7BC8-4796-B3AA-3F7B6F026E8C", "Message is empty");
	public static string HeaderText => Res.GetString("F06D0C20-086B-4906-8FC5-F6493A34E429", "Customs Rejection");
	public static string ContentCorrectionRejectionText => Res.GetString("6B0C7758-EEEF-4354-81C4-EC716D022C25", "Correction rejected");
	public static string ContentCancellationRejectionText => Res.GetString("F19DA047-D9B8-42F1-9762-47A12CC09DB4", "Cancellation rejected");
}
