using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.CH.Business;

public class CusEntryHeaderChargesLookups : Customs.Business.CusEntryHeaderChargesLookups
{
	public CusEntryHeaderChargesLookups(CusEntryHeaderCharges parent)
		: base(parent)
	{
	}

	public CodeDescriptionPairList ChargeTypeList
	{
		get
		{
			return Factory.GetCachedValue("CusEntryHeaderCharges.Lookups.C1_ChargeType", () =>
			{
				var list = new CodeDescriptionPairList();
				list.AddRange(CusRefRateCodeView.Loader.Load(Factory, Core.Constants.CountryCodes.Switzerland));
				list.AddPairIfNotExist(Core.Constants.Customs.CusEntryFeeTypes.VAT, BaseJobComInvoiceLine.ConsumptionTaxDescription);
				list.SortByDescription();
				return list;
			});
		}
	}
}
