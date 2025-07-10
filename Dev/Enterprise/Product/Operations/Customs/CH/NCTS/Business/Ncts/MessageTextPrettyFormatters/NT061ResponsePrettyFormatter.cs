using System.Net;
using CargoWise.Common;
using CargoWise.Customs.CH.MessageContracts.MessageProviders.Passar;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.CH.NCTS.Business;

public sealed class NT061ResponsePrettyFormatter : IMessagePrettyFormatter
{
	public NT061ResponsePrettyFormatter(BusinessObjectFactory factory, INT061ResponseDetail responseDetail)
	{
		this.factory = Argument.NotNull(factory, nameof(factory));
		this.responseDetail = Argument.NotNull(responseDetail, nameof(responseDetail));
	}
	readonly BusinessObjectFactory factory;
	readonly INT061ResponseDetail responseDetail;

	public ZString GetFormattedText()
	{
		var htmlBuilder = new ZStringBuilder();

		htmlBuilder
			.Append("<h2>")
			.Append(WebUtility.HtmlEncode(Title))
			.Append("</h2>");

		AppendDetail(htmlBuilder, ArrivalReferenceNumberLabel, responseDetail.ArrivalOperationReferenceNumber);
		if (responseDetail.CustomsOfficeOfDestinationActualReferenceNumber != null)
		{
			AppendDetail(htmlBuilder, CustomsOfficeLabel, $"{responseDetail.CustomsOfficeOfDestinationActualReferenceNumber} - {CustomsOfficeCodes.GetDescriptionFromCode(responseDetail.CustomsOfficeOfDestinationActualReferenceNumber)}");
		}
		AppendDetail(htmlBuilder, SelectionStatusLabel, responseDetail.SelectionNotificationStatus);
		AppendDetail(htmlBuilder, InspectionDecisionLabel, responseDetail.SelectionInspectionDecision);

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

	CodeDescriptionPairList CustomsOfficeCodes => customsOfficeCodes ?? (customsOfficeCodes = RefCusCodeListTypes.GetCachedList(factory, Core.Constants.CountryCodes.Switzerland, Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, ZDateTime.Today));
	CodeDescriptionPairList customsOfficeCodes;

	static string Title => Res.GetString("40B0CB9C-77FA-4E37-8030-B8C6AE1AFCA2", "Decision to Control");
	static string ArrivalReferenceNumberLabel => Res.GetString("CBBF50D7-FA76-4C35-9317-AD274033E570", "Customs Arrival Reference Number");
	static string CustomsOfficeLabel => Res.GetString("43544693-8C4C-4FB7-8B2D-8FEA09007074", "Customs Office of Destination");
	static string SelectionStatusLabel => Res.GetString("E3703E79-073B-4962-9817-EE201A67683A", "Selection Status");
	static string InspectionDecisionLabel => Res.GetString("34517CDD-408B-4967-A3BC-8C0A9BC10FA3", "Inspection Decision");
}
