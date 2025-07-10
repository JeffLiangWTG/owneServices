using System.Net;
using CargoWise.Common;
using CargoWise.Customs.CH.MessageContracts.MessageProviders.Passar;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.CH.NCTS.Business;

public sealed class NT140ResponsePrettyFormatter : IMessagePrettyFormatter
{
	public NT140ResponsePrettyFormatter(BusinessObjectFactory factory, INT140ResponseDetail responseDetail)
	{
		this.factory = Argument.NotNull(factory, nameof(factory));
		this.responseDetail = Argument.NotNull(responseDetail, nameof(responseDetail));
	}
	readonly BusinessObjectFactory factory;
	readonly INT140ResponseDetail responseDetail;

	public ZString GetFormattedText()
	{
		var htmlBuilder = new ZStringBuilder();

		htmlBuilder
			.Append("<h2>")
			.Append(WebUtility.HtmlEncode(Title))
			.Append("</h2>");

		AppendDetail(htmlBuilder, MRNLabel, $"{responseDetail.MRN}.{responseDetail.MRNVersion}");
		AppendDetail(htmlBuilder, RequestOnNonArrivedMovementDateLabel, $"{responseDetail.RequestOnNonArrivedMovementDate:dd - MM - yyyy}");
		AppendDetail(htmlBuilder, LimitForResponseDateLabel, $"{responseDetail.LimitForResponseDate:dd - MM - yyyy}");
		AppendDetail(htmlBuilder, CustomsOfficeOfDepartureLabel, $"{responseDetail.CustomsOfficeOfDepartureReferenceNumber} - {CustomsOfficeCodes.GetDescriptionFromCode(responseDetail.CustomsOfficeOfDepartureReferenceNumber)}");
		AppendDetail(htmlBuilder, CustomsOfficeOfEnquiryAtDepartureLabel, $"{responseDetail.CustomsOfficeOfEnquiryAtDepartureReferenceNumber} - {CustomsOfficeCodes.GetDescriptionFromCode(responseDetail.CustomsOfficeOfEnquiryAtDepartureReferenceNumber)}");

		return htmlBuilder.ToString();
	}

	void AppendDetail(ZStringBuilder htmlBuilder, string label, string text)
	{
		htmlBuilder
			.Append((NoResString)" <p><b>")
			.Append(WebUtility.HtmlEncode(label))
			.Append((NoResString)":</b> ")
			.Append(WebUtility.HtmlEncode(text ?? string.Empty))
			.Append((NoResString)"</p>");
	}

	CodeDescriptionPairList CustomsOfficeCodes => customsOfficeCodes ?? (customsOfficeCodes = RefCusCodeListTypes.GetCachedList(factory, Core.Constants.CountryCodes.Switzerland, Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, ZDateTime.Today));
	CodeDescriptionPairList customsOfficeCodes;

	static string Title => Res.GetString("16B47624-9C73-467F-AEAF-C2509F48128F", "Enquiry of Not Arrived Transit Movement");
	static string MRNLabel => Res.GetString("CD9D5ED6-501D-4F31-91B4-6CE0438A7046", "MRN");
	static string RequestOnNonArrivedMovementDateLabel => Res.GetString("5E564E80-F42F-4296-A562-0C32AFDC875A", "Enquiry Date");
	static string LimitForResponseDateLabel => Res.GetString("F94FD6C7-B879-47E5-B533-F34D97FEC7E0", "Limit for Response Date");
	static string CustomsOfficeOfDepartureLabel => Res.GetString("830F339A-2021-4DC5-A92E-C4FEF97F01F7", "Customs Office of Departure");
	static string CustomsOfficeOfEnquiryAtDepartureLabel => Res.GetString("0593CF0C-C680-44E4-85D2-0A842A4B9C4B", "Customs Office of Enquiry at Departure");
}
