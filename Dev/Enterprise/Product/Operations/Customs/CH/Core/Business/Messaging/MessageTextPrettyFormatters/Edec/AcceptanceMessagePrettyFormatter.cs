using System;
using System.Collections.Specialized;
using System.Globalization;
using CargoWise.Common;
using CargoWise.Customs.CH.MessageContracts.MessageProviders.Edec.GoodsDeclarations;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.CH.Business;

public class AcceptanceMessagePrettyFormatter : IMessagePrettyFormatter
{
	public AcceptanceMessagePrettyFormatter(BusinessObjectFactory factory, IAcceptanceResponseDetail responseDetail)
	{
		this.factory = Argument.NotNull(factory, nameof(factory));
		this.responseDetail = responseDetail;
	}

	readonly BusinessObjectFactory factory;
	readonly IAcceptanceResponseDetail responseDetail;

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1103:DoNotUseStringLiteralsForDateFormats", Justification = "Baseline")]
	public ZString GetFormattedText()
	{
		if (responseDetail is null)
		{
			return FormattableString.Invariant($"<h2>{Res.GetString("E954E4F3-7BC8-4796-B3AA-3F7B6F026E8C", "Message is empty")}</h2>");
		}

		var acceptanceTable = new HtmlTableCreator(new NameValueCollection { { (NoResString)"border", "0" } });

		var description = ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(factory, responseDetail.Status, Core.Constants.CountryCodes.Switzerland, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, ZDateTime.Today)?.ZZD_Description ?? ZString.Empty;

		var detailValues = new string[]
		{
					responseDetail.CustomsDeclarationNumber,
					responseDetail.CustomsDeclarationVersion.ToString(),
					responseDetail.IssueDate.ToString("dd-MMM-yy"),
					responseDetail.DeclarantNumber,
					responseDetail.Duty?.ToString(CultureInfo.InvariantCulture) ?? string.Empty,
					responseDetail.VAT?.ToString(CultureInfo.InvariantCulture) ?? string.Empty,
					responseDetail.AccessCode,
					responseDetail.Status + " - " + description
		};

		for (var i = 0; i < detailValues.Length; i++)
		{
			acceptanceTable.WriteRow(detailKeyList[i] + ":", detailValues[i]);
		}

		return acceptanceTable.ToHtml();
	}

	readonly string[] detailKeyList = {
				Res.GetString("C874E1C0-52D7-4113-9495-0C2E421C8371", "Customs Declaration Number") ,
				Res.GetString("4C4DDCAA-69AA-4DA1-82C5-8FF21C173B49", "Customs Declaration Version"),
				Res.GetString("32F08E81-3B88-44F0-8C8C-A7CF19B318EF", "Acceptance Date"),
				Res.GetString("3C05A216-15F9-4D55-9C7A-A211A52258DF", "Declarant Number"),
				Res.GetString("7C669A49-35D4-4A1D-8C04-028DED5FE440", "Duty Total Amount"),
				Res.GetString("AAB9B314-5BC8-4435-B75A-39FB51003864", "VAT Total Amount"),
				Res.GetString("FFB2E5E3-BF16-433D-999F-337EA9FAAC99", "Access Code"),
				Res.GetString("394ADA6A-A19C-4C7A-87DD-39146B75F3FB", "Entry Status")
		};
}
