using System;
using System.Net;
using CargoWise.Customs.CH.MessageContracts.MessageProviders.Edec.ECom;
using CargoWise.Types;

namespace Enterprise.Customs.CH.Business;

public class EComXMLSchemaErrorPrettyFormatter : IMessagePrettyFormatter
{
	public EComXMLSchemaErrorPrettyFormatter(IEComXMLSchemaErrorsResponseDetail responseDetail)
	{
		this.responseDetail = responseDetail;
	}

	readonly IEComXMLSchemaErrorsResponseDetail responseDetail;

	public ZString GetFormattedText()
	{
		if (responseDetail is null)
		{
			return FormattableString.Invariant($"<h2>{MessagePrettyFormatterHelper.MessageIsEmpty}</h2>");
		}

		var htmlBuilder = new ZStringBuilder();
		htmlBuilder.Append($"<h2>{RequestRejectedTitle}</h2>");
		htmlBuilder.Append($"<p>{WebUtility.HtmlEncode(MessagePrettyFormatterHelper.RejectionDateTimeLabel)}: {responseDetail.RejectionDateTime:yyyy-MM-dd HH:mm:ss}</p>");
		htmlBuilder.Append($"<h3>{MessagePrettyFormatterHelper.SchemaErrorsTitle}</h3>");
		htmlBuilder.Append($"<p>{responseDetail.ErrorMessage}</p>");
		return htmlBuilder.ToString();
	}

	#region Labels

	static string RequestRejectedTitle => Res.GetString("539FBDD1-1B2E-49BF-B84B-88A21C8EEC11", "eCom Request has been rejected");

	#endregion
}
