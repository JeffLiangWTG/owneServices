using System.Linq;
using System.Net;
using CargoWise.Customs.CH.MessageContracts.MessageProviders.Ebd;
using CargoWise.Types;

namespace Enterprise.Customs.CH.Business;

public class EbdResponsePrettyFormatter : IMessagePrettyFormatter
{
	public EbdResponsePrettyFormatter(IDocumentImportResponseDetail responseDetail)
	{
		this.responseDetail = responseDetail;
	}

	readonly IDocumentImportResponseDetail responseDetail;

	public ZString GetFormattedText()
	{
		var htmlBuilder = new ZStringBuilder();

		htmlBuilder.Append($"<h2>{WebUtility.HtmlEncode(responseDetail.IsAcceptance ? AcceptanceTitle : RejectionTitle)}</h2>");

		foreach (var accompanyingDocument in responseDetail.AccompanyingDocuments)
		{
			htmlBuilder.Append($"<p><b>{WebUtility.HtmlEncode(accompanyingDocument.Filename)}</b></p>");

			if (responseDetail.IsRejection)
			{
				var text = MessagePrettyFormatterHelper.GetTextByLanguageCode(accompanyingDocument.Informations.Select(x => (x.Language, x.Text)));
				htmlBuilder.Append($"<p>{WebUtility.HtmlEncode(text)}</p>");
			}
		}

		return htmlBuilder.ToString();
	}

	static string AcceptanceTitle => Res.GetString("D718AE79-4A89-4C3D-93FA-9B2BEC173932", "Document successfully uploaded");
	static string RejectionTitle => Res.GetString("F761F992-2A19-47A1-BA63-87D28B2E9716", "Document upload failed");
}
