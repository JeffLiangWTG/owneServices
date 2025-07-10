using System.Collections.Generic;
using System.Linq;
using System.Net;
using CargoWise.Customs.CH.MessageContracts.MessageProviders.Passar;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.CH.Business;

public sealed class NC909ResponsePrettyFormatter : BasePassarResponsePrettyFormatter<INC909ResponseDetail>, IMessagePrettyFormatter
{
	public NC909ResponsePrettyFormatter(BusinessObjectFactory factory, INC909ResponseDetail responseDetail) : base(factory, responseDetail)
	{
	}

	public ZString GetFormattedText()
	{
		var htmlBuilder = new ZStringBuilder();

		htmlBuilder
			.Append("<h2>")
			.Append(WebUtility.HtmlEncode(InvalidMessageTitle))
			.Append("</h2><h3>")
			.Append(WebUtility.HtmlEncode(ErrorsSubTitle))
			.Append("</h3>");

		var htmlTable = new HtmlTableCreator(GetColumnTitles(ResponseDetail.Errors.FirstOrDefault()));

		foreach (var error in ResponseDetail.Errors)
		{
			htmlTable.WriteRow(new[]
			{
					error.ErrorType,
					error.ErrorCode,
					ErrorCodes.GetDescriptionFromCode(error.ErrorCode),
					error.ErrorMessage,
					error.ErrorLineNumber ?? error.ErrorPointer,
				});
		}
		htmlBuilder.Append(htmlTable.ToHtml());

		return htmlBuilder.ToString();
	}

	CodeDescriptionPairList ErrorCodes => RefCusCodeListTypes.GetCachedList(Factory, Core.Constants.CountryCodes.Switzerland, CH.Business.UniversalReferenceConstants.RefCusCodeList.PassarTypes.N2000, ZDateTime.Today);

	static string InvalidMessageTitle => Res.GetString("544AB943-101A-463A-A801-A166FB325779", "The sent message is invalid");
	static string ErrorsSubTitle => Res.GetString("24221021-BF4C-4B57-AF5E-1576079B6526", "Errors:");

	IEnumerable<string> GetColumnTitles(INC909Error error)
	{
		yield return Res.GetString("0AF71B2C-4F6F-437F-B18C-2915940ABDD0", "Type");
		yield return Res.GetString("A8A8ADDC-4308-4672-A5D9-55658FF9FD59", "Code");
		yield return Res.GetString("11A905C4-C195-4A6A-AF15-CB1408281C21", "Description");
		yield return Res.GetString("390F09A4-8D91-4F98-96E4-9C9F423FBB92", "Message");
		yield return string.IsNullOrEmpty(error?.ErrorPointer) ? Res.GetString("9CCC8909-E796-4D28-9F38-FF30A7032C23", "Line No.") : Res.GetString("079AFCAC-C86E-4F5A-818E-7897B1E0973C", "Pointer");
	}
}
