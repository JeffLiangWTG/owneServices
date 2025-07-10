using System.Collections.Generic;
using System.Linq;
using System.Net;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.CH.Business;

public static class MessagePrettyFormatterHelper
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Constant strings")]
	public const string StyleSheet = @"
		*
		{
			font-family: Verdana, Arial, Helvetica, sans-serif;
		}

		body,
		table,
		th,
		td {
			font-size: 9pt;
			border-collapse: collapse;
			padding: 3px 5px 3px 5px;
			vertical-align: top;
			text-align: left;
		}";

	public static string GetTextByLanguageCode(IEnumerable<(string Language, string Text)> texts)
	{
		var userLanguage = TranslationHelper.GetLanguageCode(GlbStaff.CurrentUser.Language);
		return texts.FirstOrDefault(i => userLanguage.EqualsIgnoringCase(i.Language)).Text
			?? texts.FirstOrDefault(i => defaultLanguage.EqualsIgnoringCase(i.Language)).Text
			?? texts.FirstOrDefault().Text;
	}

	static readonly ZString defaultLanguage = (NoResString)"de";

	public static string MessageIsEmpty => WebUtility.HtmlEncode(Res.GetString("837FAE4E-BAC6-468C-918C-A046F4CDBDC8", "Message is empty"));
	public static string SchemaErrorsTitle => WebUtility.HtmlEncode(Res.GetString("461DC16E-F9CD-4C35-9326-F547D1BA6065", "Schema Errors"));
	public static string RejectionDateTimeLabel => WebUtility.HtmlEncode(Res.GetString("D61221A8-8F1A-48E9-9EE8-2B16E02889F8", "Rejection Date/Time"));
	public static string ErrorsTitle => WebUtility.HtmlEncode(Res.GetString("2DF0D1D2-E1C1-40A8-A4EF-35AD987853B5", "Errors"));
	public static string YesLabel => Res.GetString("12EDF763-CCFA-4B2E-9939-D8D8CD24E207", "Yes");
	public static string NoLabel => Res.GetString("ED813A2D-9C19-4AFC-A616-3B0F1D30CD39", "No");
}
