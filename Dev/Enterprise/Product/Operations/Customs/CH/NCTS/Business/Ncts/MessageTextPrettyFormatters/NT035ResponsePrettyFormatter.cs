using System.Net;
using CargoWise.Customs.CH.MessageContracts.MessageProviders.Passar;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.CH.NCTS.Business;

public sealed class NT035ResponsePrettyFormatter : BasePassarResponsePrettyFormatter<INT035ResponseDetail>, IMessagePrettyFormatter
{
	public NT035ResponsePrettyFormatter(BusinessObjectFactory factory, INT035ResponseDetail responseDetail) : base(factory, responseDetail)
	{
	}

	public ZString GetFormattedText()
	{
		var htmlBuilder = new ZStringBuilder();

		htmlBuilder
			.Append("<h2>")
			.Append(WebUtility.HtmlEncode(MessageTitle))
		.Append("</h2>");

		htmlBuilder.Append($"<p>{MRNLabel}: {ResponseDetail.MRN}.{ResponseDetail.MRNVersion}");

		htmlBuilder.Append($"<p>{DeclarationAcceptanceDateLabel}: {ResponseDetail.DeclarationAcceptanceDate.ToString("dd.MM.yyyy")}");

		htmlBuilder.Append($"<p>{RecoveryDateLabel}: {ResponseDetail.RecoveryNotificationDate.ToString("dd.MM.yyyy")}");

		htmlBuilder.Append($"<p>{CustomsOfficeOfDepartureLabel}: {ResponseDetail.CustomsOfficeOfDepartureReferenceNumber} - {CustomsOfficeCodes.GetDescriptionFromCode(ResponseDetail.CustomsOfficeOfDepartureReferenceNumber)}");

		htmlBuilder.Append($"<p>{CustomsOfficeOfRecoveryLabel}: {ResponseDetail.CustomsOfficeOfRecoveryReferenceNumber} - {CustomsOfficeCodes.GetDescriptionFromCode(ResponseDetail.CustomsOfficeOfRecoveryReferenceNumber)}");

		htmlBuilder.Append($"<p>{ResponseDetail.RecoveryNotificationText}");

		return htmlBuilder.ToString();
	}

	CodeDescriptionPairList CustomsOfficeCodes => customsOfficeCodes ?? (customsOfficeCodes = RefCusCodeListTypes.GetCachedList(Factory, Core.Constants.CountryCodes.Switzerland, Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, ZDateTime.Today));
	CodeDescriptionPairList customsOfficeCodes;

	static string MessageTitle => Res.GetString("DEB7283F-8754-4DCF-B814-536831D24D73", "Recovery Notification");
	static string MRNLabel => Res.GetString("9C8E51B8-9BF6-4B51-B3A9-F5CFBC17D25B", "MRN");
	static string DeclarationAcceptanceDateLabel => Res.GetString("87775BB9-C150-47FE-B2DC-1001D7BF89A3", "Declaration Acceptance Date");
	static string RecoveryDateLabel => Res.GetString("405E7234-D08B-42E2-880D-3F0EAC58DD46", "Recovery Date");
	static string CustomsOfficeOfDepartureLabel => Res.GetString("C470DBFA-56E9-4CF6-87A7-1925467357F4", "Customs Office of Departure");
	static string CustomsOfficeOfRecoveryLabel => Res.GetString("44679CEB-4111-4039-8530-61356F196E4E", "Customs Office of Recovery");
}
