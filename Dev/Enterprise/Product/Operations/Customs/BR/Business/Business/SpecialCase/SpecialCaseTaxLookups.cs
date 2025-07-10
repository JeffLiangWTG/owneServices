using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.BR.Business
{
	public class SpecialCaseTaxLookups : ZLookups
	{
		public SpecialCaseTaxLookups(SpecialCaseTax parent)
			: base(parent)
		{
		}

		public new SpecialCaseTax Parent => (SpecialCaseTax)base.Parent;

		public CodeDescriptionPairList TaxTypeList
		{
			get
			{
				return Factory.GetCachedValue($"BR_SpecialCaseTaxLookups_TaxTypeList_{Parent?.InvoiceLine?.Declaration?.JE_MessageType}_{Parent.TaxGroup}", () =>
				{
					var result = new SpecialCaseTaxTypeList();

					if (Parent.InvoiceLine.IsImportSiscomex)
					{
						result.RemoveCode(SpecialCaseTaxTypeList.Codes.TariffAgreement);
						result.RemoveCode(SpecialCaseTaxTypeList.Codes.Reduction);

						if (Parent.TaxGroup == Constants.RateCodes.Antidumping)
						{
							result.RemoveCode(SpecialCaseTaxTypeList.Codes.Reduced);
						}
						else
						{
							result.RemoveCode(SpecialCaseTaxTypeList.Codes.AdValoremRate);
						}
					}
					else
					{
						result.RemoveCode(SpecialCaseTaxTypeList.Codes.QuantityPerUnit);

						if (Parent.TaxGroup != Constants.RateCodes.ImportDuty)
						{
							result.RemoveCode(SpecialCaseTaxTypeList.Codes.TariffAgreement);
							result.RemoveCode(SpecialCaseTaxTypeList.Codes.Reduction);
						}
					}
					return result;
				});
			}
		}

		public CodeDescriptionPairList TaxGroupList => Factory.GetCachedValue($"BR_SpecialCaseTaxLookups_TaxGroupList_{Parent.InvoiceLine?.IsImportSiscomex}", () =>
		{
			var rateTypesToInclude = new List<ZString>
					{
						Constants.RateTypes.Antidumping,
						Constants.RateTypes.Cofins,
						Constants.RateTypes.PIS,
						Constants.RateTypes.IPI,
						Constants.RateTypes.ImportDuty
					};

			if (Parent.InvoiceLine?.IsImportSiscomex ?? false)
			{
				rateTypesToInclude.Remove(Constants.RateTypes.ImportDuty);
			}

			var loadCriteria = new RateCodeLoadCriteria()
			{
				RateTypesToInclude = rateTypesToInclude.ToArray(),
			};

			var taxesAndFees = CusRefRateCodeView.Loader.Load(Factory, Core.Constants.CountryCodes.Brazil, loadCriteria);
			var result = new CodeDescriptionPairList();
			foreach (var code in taxesAndFees)
			{
				result.AddPair(code.ZY1_RateCode, code.ZY1_Description);
			}
			result.Sort();
			return result;
		});
	}
}
