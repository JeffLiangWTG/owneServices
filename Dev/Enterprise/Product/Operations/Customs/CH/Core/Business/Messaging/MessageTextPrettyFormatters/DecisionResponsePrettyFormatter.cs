using System.Net;
using CargoWise.Common;
using CargoWise.Customs.CH.MessageContracts.MessageProviders.Passar;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.CH.Business;

public abstract class DecisionResponsePrettyFormatter<T> : BasePassarResponsePrettyFormatter<T> where T : IDecision
{
	protected DecisionResponsePrettyFormatter(BusinessObjectFactory factory, T responseDetail) : base(factory, responseDetail)
	{
	}

	public ZString GetFormattedText()
	{
		var htmlBuilder = new ZStringBuilder();

		AppendHtmlEncodedTag(htmlBuilder, "h2", ResponseTitle);
		AppendFormatterSpecificText(htmlBuilder);
		AppendHtmlEncodedTag(htmlBuilder, "h3", $"{DecisionSubTitle}: {GetDecisionText()}");
		AppendReasons(htmlBuilder);
		return htmlBuilder.ToString();

		string GetDecisionText() => ResponseDetail.IsAccepted ? DecisionAccepted : ResponseDetail.IsRejected ? DecisionRejected : ResponseDetail.IsReceived ? DecisionReceived : string.Empty;
	}

	protected abstract ZString ResponseTitle { get; }

	protected virtual void AppendFormatterSpecificText(ZStringBuilder htmlBuilder) { }

	void AppendReasons(ZStringBuilder htmlBuilder)
	{
		if (!ResponseDetail.DecisionReasons.IsNullOrEmpty())
		{
			AppendHtmlEncodedTag(htmlBuilder, "h3", $"{ReasonsSubTitle}:");

			var decisionReasonsTable = new HtmlTableCreator(ColumnTitles);
			foreach (var decisionReason in ResponseDetail.DecisionReasons)
			{
				decisionReasonsTable.WriteRow(new[]
				{
						decisionReason.Code,
						DecisionReasonCodes.GetDescriptionFromCode(decisionReason.Code),
						decisionReason.Message,
						decisionReason.Pointer,
					});
			}
			htmlBuilder.Append(decisionReasonsTable.ToHtml());
		}
	}

	protected void AppendHtmlEncodedTag(ZStringBuilder htmlBuilder, string tag, string content)
	{
		if (!string.IsNullOrEmpty(content))
		{
			htmlBuilder.Append($"<{tag}>{WebUtility.HtmlEncode(content)}</{tag}>");
		}
	}

	static string ReasonsSubTitle => Res.GetString("022CCA9E-6E91-4A24-8F58-8FA162177BF0", "Reasons");

	static string DecisionSubTitle => Res.GetString("D9C70021-A55A-4B52-B863-911CA0A0D07F", "Decision");

	static string DecisionAccepted => Res.GetString("4EDD0084-48F4-496C-A306-D903797FF3B5", "ACCEPTED");

	static string DecisionRejected => Res.GetString("0AF13E11-E763-4EFC-AA33-F736E6A879D8", "REJECTED");

	static string DecisionReceived => Res.GetString("CA47A0ED-965A-4278-85D7-1E5B2BEDC14D", "RECEIVED");

	static string[] ColumnTitles => new[]
	{
			Res.GetString("868F21A6-0743-40BE-862A-89069F02E07B", "Code"),
			Res.GetString("68E037EA-CFCB-447F-B137-3133B4DDD3B1", "Description"),
			Res.GetString("E415B12C-57B4-42DB-9E95-CD03688F8EBE", "Message"),
			Res.GetString("DF17C548-51F0-4B43-B30F-CB917C120AB8", "Pointer"),
		};

	CodeDescriptionPairList DecisionReasonCodes => decisionReasonCodes ??= RefCusCodeListTypes.GetCachedList(Factory, Core.Constants.CountryCodes.Switzerland, UniversalReferenceConstants.RefCusCodeList.PassarTypes.N3000, ZDateTime.Today);
	CodeDescriptionPairList decisionReasonCodes;
}
