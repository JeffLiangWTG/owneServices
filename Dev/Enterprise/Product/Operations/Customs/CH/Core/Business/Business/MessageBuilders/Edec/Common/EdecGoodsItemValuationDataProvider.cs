using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.CH.MessageContracts.Edec.GoodsDeclarations;

namespace Enterprise.Customs.CH.Business;

public class EdecGoodsItemValuationDataProvider : IEdecGoodsItemValuation
{
	public static EdecGoodsItemValuationDataProvider New(CusEntryLine entryLine) => entryLine == null ? null : new EdecGoodsItemValuationDataProvider(entryLine);

	EdecGoodsItemValuationDataProvider(CusEntryLine entryLine)
	{
		this.entryLine = Argument.NotNull(entryLine, nameof(entryLine));
		invoiceLine = this.entryLine.RandomLine;
	}
	readonly CusEntryLine entryLine;
	readonly JobComInvoiceLine invoiceLine;

	public bool NetDuty => entryLine.InvoiceLines.Cast<JobComInvoiceLine>().Any(x => x.JI_WeightIncludingInnerPackage > 0);

	public decimal? TareSupplement => invoiceLine.JI_TareSupplementPercentage.IsEmpty ? null : invoiceLine.JI_TareSupplementConfirmation ? (decimal?)invoiceLine.JI_TareSupplementPercentage : null;

	public bool? TareSupplementConfirmation => NetDuty ? invoiceLine.JI_TareSupplementConfirmation : null;

	public string CustomsFavourCode => invoiceLine.CustomsFavourCode.IsEmpty ? null : (string)invoiceLine.CustomsFavourCode;

	public decimal VATValue => entryLine.CL_CustomsValue;

	public bool VATValueConfirmation => invoiceLine.JI_VATValueConfirmation;

	public string VATCode => invoiceLine.JI_ZZF_NKTaxType;

	public bool VATCodeConfirmation => invoiceLine.JI_VATCodeConfirmation;

	public decimal? Rate => invoiceLine.JI_RateOverride ? invoiceLine.OverriddenRateXML : invoiceLine.DutyRateAdditionalCode.IsEmpty || invoiceLine.DutyRateFormulaNumber.IsEmpty ? null : decimal.Parse(invoiceLine.DutyRateFormulaNumber, CultureInfo.InvariantCulture);

	public bool RateConfirmation => invoiceLine.JI_RateOverride;
}
