using System.Net;
using CargoWise.Customs.CH.MessageContracts.MessageProviders.Edec.GoodsDeclarations;
using CargoWise.Types;

namespace Enterprise.Customs.CH.Business;

public class XMLSchemaErrorPrettyFormatter : IMessagePrettyFormatter
{
	readonly IXMLSchemaErrorsResponseDetail responseDetail;

	public XMLSchemaErrorPrettyFormatter(IXMLSchemaErrorsResponseDetail responseDetail)
	{
		this.responseDetail = responseDetail;
	}

	static string SchemaErrors => Res.GetString("0BCEEF66-4924-422E-B58E-1601ABCE70FA", "Schema Errors");

	public ZString GetFormattedText()
	{
		var message = responseDetail?.ErrorMessage;
		if (message != null)
		{
			return $"<b>{WebUtility.HtmlEncode(SchemaErrors)}</b><br><br>{message}";
		}
		return ZString.Empty;
	}
}
