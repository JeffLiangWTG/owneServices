using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.JP.MessageDefinitions;
using CargoWise.Types;

namespace Enterprise.Customs.JP.Common;

public sealed class ExportClearancePermitItemDocumentWrapper(ExportClearancePermitItemProvider item) : DocumentEngineCore.DocWrappers.DocumentWrapper
{
	readonly ExportClearancePermitItemProvider item = Argument.NotNull(item, nameof(item));

	#region Item Fields

	public ZString I_105 => item.ColumnNumber;

	public ZString I_106 => item.OriginalColumnNumber;

	public ZString I_107 => item.PriceReconfirmationType;

	public ZString I_108 => item.GoodsDescription;

	public ZString I_109 => item.TariffCode;

	public ZString I_110 => item.NACCSCode;

	public ZString I_111 => item.CustomsValue.FormatNumberInDocument();

	public ZString I_112 => item.Quantity1.FormatNumberInDocument();

	public ZString I_113 => item.Quantity1Unit;

	public ZString I_114 => item.CustomsValueSummary.FormatNumberInDocument();

	public ZString I_115 => item.Quantity2.FormatNumberInDocument();

	public ZString I_116 => item.Quantity2Unit;

	public ZString I_117 => item.BasicPriceApportionmentCoefficient.FormatNumberInDocument();

	public ZString I_118 => item.BasicPriceCurrency;

	public ZString I_119 => item.BasicPrice.FormatNumberInDocument();

	public ZString I_120_1 => OtherLawsAndRegulationsCode[0];

	public ZString I_120_2 => OtherLawsAndRegulationsCode[1];

	public ZString I_120_3 => OtherLawsAndRegulationsCode[2];

	public ZString I_120_4 => OtherLawsAndRegulationsCode[3];

	public ZString I_120_5 => OtherLawsAndRegulationsCode[4];

	public ZString I_121 => item.ExportControlOrdinanceAppendixCode;

	public ZString I_122 => item.ForeignExchangeAndForeignTradeActArticle48Code;

	public ZString I_123 => item.DutyExemptionReductionRefundCode;

	public ZString I_124 => item.DutyReductionExemptionClauseLaw;

	public ZString I_125 => item.DutyReductionExemptionClauseLawArticleNumber;

	public ZString I_126 => item.DutyReductionExemptionClauseOrderArticleNumber;

	public ZString I_127 => item.DomesticConsumptionTaxExemptionCode;

	public ZString I_128 => item.DomesticConsumptionTaxExemptionType;

	public ZString I_129 => item.ExemptedDomesticConsumptionTaxName;

	#endregion

	string[] OtherLawsAndRegulationsCode
	{
		get
		{
			if (otherLawsAndRegulationsCode == null)
			{
				otherLawsAndRegulationsCode = new string[5];
				var resultFromProvider = item.OtherLawsAndRegulationsCode?.ToArray();
				if (resultFromProvider != null)
				{
					for (var i = 0; i < resultFromProvider.Length; i++)
					{
						otherLawsAndRegulationsCode[i] = resultFromProvider[i];
					}
				}
			}
			return otherLawsAndRegulationsCode;
		}
	}
	string[] otherLawsAndRegulationsCode;
}
