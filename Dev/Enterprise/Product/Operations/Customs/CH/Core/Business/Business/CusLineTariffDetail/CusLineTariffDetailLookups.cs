using System.Linq;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Customs.CH.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.CH.Business;

public class CusLineTariffDetailLookups : Customs.Business.CusLineTariffDetailLookups
{
	public CusLineTariffDetailLookups(Customs.Business.CusLineTariffDetail parent)
		: base(parent)
	{
	}

	new CusLineTariffDetail Parent => (CusLineTariffDetail)base.Parent;

	public CodeDescriptionPairList TariffList
	{
		get
		{
			if (Parent.IsAdditionalFee)
			{
				return Factory.GetCachedValue("CH_AdditionalFeeTariffList", () =>
				{
					var list = new CodeDescriptionPairList();
					list.AddRange(CusRefRateCodeView.Loader.LoadByRateType(Factory, Core.Constants.CountryCodes.Switzerland, RateTypes.AdditionalFees));
					list.Sort();
					return list;
				});
			}
			else if (Parent.IsAdditionalTax)
			{
				var taxType = Parent.BZ_TaxType;
				if (!taxType.IsEmpty && Parent.InvoiceLine is JobComInvoiceLine invoiceLine)
				{
					return Factory.GetCachedValue($"CH_AdditionalTaxsTariffList_{taxType}_{invoiceLine.GetAdditionalTaxTariffsCacheKey()}", () =>
					{
						var codeDescriptionPairList = new CodeDescriptionPairList();

						foreach (var additionalTaxTariff in invoiceLine.GetApplicableAdditionalTaxTariffs().Where(t => t.ZZ1_TariffCode.StartsWith(taxType)))
						{
							codeDescriptionPairList.Add(additionalTaxTariff);
						}
						return codeDescriptionPairList;
					});
				}
			}

			return new CodeDescriptionPairList();
		}
	}
}
