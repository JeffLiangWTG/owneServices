using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Customs.CH.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.CH.Business;

public class DocEdecCusEntryLine : DocCusEntryLine
{
	public static new DocEdecCusEntryLine New(CusEntryLine entryLine, BusinessObjectFactory factoryToWrap)
		=> entryLine == null ? null : new DocEdecCusEntryLine(entryLine, factoryToWrap);

	DocEdecCusEntryLine(CusEntryLine entryLine, BusinessObjectFactory factoryToWrap) : base(entryLine, factoryToWrap)
	{
		invoiceLine = CusEntryLine.RandomLine;
	}
	readonly JobComInvoiceLine invoiceLine;

	public ZString CommodityCode => invoiceLine.JI_FormattedTariff.Left(9);

	public ZString StatisticalCode => invoiceLine.JI_Procedure == ProcedureCodesEdec.ExemptFromDuty && !invoiceLine.InAndOutwardProcessingRepair ? ZString.Empty : invoiceLine.StatisticalCode;

	public ZBool IsPreferentialTariff => invoiceLine.JI_PrimaryPreference == PrimaryPreferenceCodes.PreferentialTariff;

	public ZString OverriddenRate => invoiceLine.JI_RateOverride ? invoiceLine.JI_OverriddenRate.ToString() : ZString.Empty;

	public ZDecimal AdditionalUnit => Utilities.Round(CusEntryLine.CalcAdditionalQty, 1);

	public ZDecimal VATCode => Utilities.Round(RefCusTaxOrFee.Loader.LoadTaxOrFeeFromTypeDate(invoiceLine.Factory, Core.Constants.CountryCodes.Switzerland, Core.Constants.Customs.CusEntryFeeTypes.VAT, invoiceLine.EffectiveAssessmentDate).FirstOrDefault(i => i.ZZF_Code == invoiceLine.JI_ZZF_NKTaxType)?.ZZF_Value * 100 ?? ZDecimal.Zero, 1);
}
