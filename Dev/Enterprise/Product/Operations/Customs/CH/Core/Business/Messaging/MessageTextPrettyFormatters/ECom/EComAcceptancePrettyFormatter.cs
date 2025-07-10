using System;
using System.Net;
using CargoWise.Customs.CH.MessageContracts.MessageProviders.Edec.ECom;
using CargoWise.Types;

namespace Enterprise.Customs.CH.Business;

public class EComAcceptancePrettyFormatter : IMessagePrettyFormatter
{
	public EComAcceptancePrettyFormatter(IEComAcceptanceResponseDetail responseDetail)
	{
		this.responseDetail = responseDetail;
	}

	readonly IEComAcceptanceResponseDetail responseDetail;

	public ZString GetFormattedText()
	{
		if (responseDetail is null)
		{
			return FormattableString.Invariant($"<h2>{MessagePrettyFormatterHelper.MessageIsEmpty}</h2>");
		}

		var htmlBuilder = new ZStringBuilder();

		htmlBuilder.Append($"<h2>{WebUtility.HtmlEncode(AcceptanceTitle)}</h2>");
		htmlBuilder.Append($"<p>{WebUtility.HtmlEncode(AcceptanceDateTimeLabel)}: {responseDetail.AcceptanceDateTime:yyyy-MM-dd HH:mm:ss}</p>");
		htmlBuilder.Append($"<p>{WebUtility.HtmlEncode(RequestNumberLabel)}: {WebUtility.HtmlEncode(responseDetail.RequestNumber)}</p>");
		return htmlBuilder.ToString();
	}

	static string AcceptanceTitle => Res.GetString("FEA4CA4A-BE1D-4B27-B72E-3311D9B6E9F4", "eCom Request has been accepted");
	static string AcceptanceDateTimeLabel => Res.GetString("82481B3D-9CF3-4002-9C9F-06E65A535E63", "Acceptance Date/Time");
	static string RequestNumberLabel => Res.GetString("37EA4EE8-A7AB-40AD-B643-5EB1F754F744", "Request Number");
}
