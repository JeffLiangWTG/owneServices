using CargoWise.Types;
using Enterprise.Customs.Universal;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.BR.Business
{
	public class CusEntryLineFeeLookups : Customs.Business.CusEntryLineFeeLookups
	{
		public CusEntryLineFeeLookups(CusEntryLineFee parent)
			: base(parent)
		{
		}

		public CusEntryLineFee EntryLineFee
		{
			get { return Parent; }
		}

		protected new CusEntryLineFee Parent
		{
			get { return (CusEntryLineFee)base.Parent; }
		}

		public CodeDescriptionPairList ChargeTypeList
		{
			get
			{
				var effectiveDate = EntryLineFee?.EntryLine?.RandomLine?.EffectiveAssessmentDate ?? ZDateTime.Today;

				return Factory.GetCachedValue($"BR.CusEntryLineFeeLookups.ChargeTypeList_{effectiveDate}", () =>
				{
					var list = new CodeDescriptionPairList();
					list.AddRange(RefCusRateType.Loader.GetRateTypesByDataGrouping(Factory, Core.Constants.CountryCodes.Brazil));
					list.AddPairIfNotExist(Core.Constants.Customs.Universal.RefCusTaxOrFee.Types.SiscomexUsageEntryFee, Res.GetString("BRCusEntryLineFeeLookups|ChargeTypeList|SUF", "SISCOMEX Usage Entry Fee"));
					list.AddPairIfNotExist(Core.Constants.Customs.Universal.RefCusTaxOrFee.Types.AfrmmTax, Res.GetString("BRCusEntryLineFeeLookups|ChargeTypeList|FMM", "Freight Surcharge for Renewal of the Merchant Marine (AFRMM)"));
					list.AddPairIfNotExist(Core.Constants.Customs.Universal.RefCusTaxOrFee.Types.ICMSTax, Res.GetString("BRCusEntryLineFeeLookups|ChargeTypeList|ICM", "Tax on the Movement of Goods and Services"));
					list.AddPairIfNotExist(Constants.RateTypes.OtherExpensesICMS, Res.GetString("BRCusEntryLineFeeLookups|ChargeTypeList|EIC", "Other Expenses to ICMS"));
					list.AddPairIfNotExist(Core.Constants.Customs.Universal.RefCusTaxOrFee.Types.ICMSFCPTax, Res.GetString("BRCusEntryLineFeeLookups|ChargeTypeList|FCP", "Tax on the Fund to Combat Poverty"));
					list.AddRange(BRRefCusTaxOrFee.GetImportLicenseFees(Factory, effectiveDate));
					return list;
				});
			}
		}
	}
}
