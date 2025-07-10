using System.Collections.Immutable;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.CN.Business
{
	public partial class ProductQualificationCodeList
	{
		static readonly ImmutableArray<string> ProductQualificationCodesImport = ImmutableArray.Create(
			Codes._105, Codes._106, Codes._107, Codes._108, Codes._109,
			Codes._110, Codes._111, Codes._112, Codes._113, Codes._114,
			Codes._115, Codes._116, Codes._117, Codes._203, Codes._325,
			Codes._328, Codes._330, Codes._331, Codes._332, Codes._401,
			Codes._402, Codes._408, Codes._409, Codes._410, Codes._411,
			Codes._412, Codes._416, Codes._422, Codes._423, Codes._424,
			Codes._428, Codes._429, Codes._430, Codes._516, Codes._517,
			Codes._519, Codes._522, Codes._523, Codes._524, Codes._526,
			Codes._527, Codes._528, Codes._529, Codes._530, Codes._601,
			Codes._603, Codes._604, Codes._605, Codes._606, Codes._607,
			Codes._608, Codes._609, Codes._610, Codes._611, Codes._612,
			Codes._613, Codes._629, Codes._630, Codes._800, Codes._900
		);

		static readonly ImmutableArray<string> ProductQualificationCodesExport = ImmutableArray.Create(
			Codes._103, Codes._203, Codes._401, Codes._404, Codes._417,
			Codes._425, Codes._426, Codes._429, Codes._518,
			Codes._522, Codes._526, Codes._527, Codes._528, Codes._529,
			Codes._530, Codes._602, Codes._614, Codes._615, Codes._616,
			Codes._617, Codes._618, Codes._619, Codes._620, Codes._621,
			Codes._622, Codes._623, Codes._624, Codes._626, Codes._627,
			Codes._628, Codes._630
		);

		static readonly ImmutableArray<string> ProductQualificationCodesSupportTSD = ImmutableArray.Create(
			Codes._330, Codes._331, Codes._332, Codes._410, Codes._411,
			Codes._429, Codes._430, Codes._526, Codes._527, Codes._528,
			Codes._529, Codes._530, Codes._606, Codes._612, Codes._629
		);

		public static CodeDescriptionPairList GetCachedProductQualificationCodeList(BusinessObjectFactory factory, bool isImport)
		{
			return factory.GetCachedValue<ProductQualificationCodeList>().FilterListByCodes(isImport ? ProductQualificationCodesImport : ProductQualificationCodesExport);
		}

		public static bool IsProductQualificationCodeSupportTSD(string productQualificationCode)
		{
			return ProductQualificationCodesSupportTSD.Contains(productQualificationCode);
		}
	}
}
