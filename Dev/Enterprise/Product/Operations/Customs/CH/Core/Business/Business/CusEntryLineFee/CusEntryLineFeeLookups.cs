using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.CH.Business;

public class CusEntryLineFeeLookups : Customs.Business.CusEntryLineFeeLookups
{
	public CusEntryLineFeeLookups(CusEntryLineFee parent)
		: base(parent)
	{
	}

	public CodeDescriptionPairList ChargeTypeList
	{
		get
		{
			return Factory.GetCachedValue("CusEntryLineFee.Lookups.CF_ChargeType", () =>
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
