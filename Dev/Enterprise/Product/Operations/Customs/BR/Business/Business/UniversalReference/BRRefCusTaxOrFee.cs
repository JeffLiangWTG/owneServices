using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;
using ECC = Enterprise.Core.Constants;

namespace Enterprise.Customs.BR.Business
{
	public static class BRRefCusTaxOrFee
	{
		public static RefCusTaxOrFee[] GetSiscomexUsageEntryFees(BusinessObjectFactory factory, ZDateTime valuationDate)
		{
			return RefCusTaxOrFee.Loader.LoadTaxOrFeeFromTypeDate(factory, ECC.CountryCodes.Brazil, ECC.Customs.Universal.RefCusTaxOrFee.Types.SiscomexUsageEntryFee, valuationDate);
		}

		public static RefCusTaxOrFee[] GetAfrmmTaxs(BusinessObjectFactory factory, ZDateTime valuationDate)
		{
			return RefCusTaxOrFee.Loader.LoadTaxOrFeeFromTypeDate(factory, ECC.CountryCodes.Brazil, ECC.Customs.Universal.RefCusTaxOrFee.Types.AfrmmTax, valuationDate);
		}

		public static ZDecimal GetAfrmmTaxRate(BusinessObjectFactory factory, ZString taxOrFeeCode, ZDateTime valuationDate)
		{
			return GetRefCusTaxOrFee(factory, ECC.Customs.Universal.RefCusTaxOrFee.Types.AfrmmTax, taxOrFeeCode, valuationDate)?.ZZF_Value ?? ZDecimal.Zero;
		}

		static RefCusTaxOrFee GetRefCusTaxOrFee(BusinessObjectFactory factory, ZString type, ZString taxOrFeeCode, ZDateTime valuationDate)
		{
			return RefCusTaxOrFee.Loader.LoadTaxOrFeeFromTypeDate(factory, ECC.CountryCodes.Brazil, type, valuationDate).FirstOrDefault(x => x.ZZF_Code == taxOrFeeCode);
		}

		public static RefCusTaxOrFee[] GetImportLicenseFees(BusinessObjectFactory factory, ZDateTime valuationDate)
		{
			return RefCusTaxOrFee.Loader.LoadTaxOrFeeFromTypeDate(factory, ECC.CountryCodes.Brazil, ECC.Customs.Universal.RefCusTaxOrFee.Types.ImportLicenseFine, valuationDate);
		}

		public static RefCusTaxOrFee GetImportLicenseFee(BusinessObjectFactory factory, ZString taxOrFeeCode, ZDateTime valuationDate)
		{
			return GetRefCusTaxOrFee(factory, ECC.Customs.Universal.RefCusTaxOrFee.Types.ImportLicenseFine, taxOrFeeCode, valuationDate);
		}

		public static CodeDescriptionPairList GetFeeTypeList(BusinessObjectFactory factory, ZDateTime effectiveDate)
		{
			return factory.GetCachedValue($"BRImportLicenseFeeTypeList_{effectiveDate}", () =>
			{
				var importLicenseFeeTypeList = new CodeDescriptionPairList();
				importLicenseFeeTypeList.AddRange(GetImportLicenseFees(factory, effectiveDate));
				return importLicenseFeeTypeList;
			});
		}
	}
}
