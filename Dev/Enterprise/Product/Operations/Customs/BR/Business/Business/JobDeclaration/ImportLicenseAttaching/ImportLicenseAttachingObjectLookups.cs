using CargoWise.EntityFramework;
using CargoWise.Types;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.BR.Business
{
	public class ImportLicenseAttachingObjectLookups : ZLookups
	{
		public ImportLicenseAttachingObjectLookups(ImportLicenseAttachingObject parent)
			: base(parent)
		{
		}

		public CodeDescriptionPairList FeeTypeList
		{
			get
			{
				var effectiveDate = ZDateTime.Today;

				return Factory.GetCachedValue($"BRImportLicenseAttaching.FeeTypeList_{effectiveDate}", () =>
				{
					var importLicenseAttachingFeeTypeList = new CodeDescriptionPairList();
					importLicenseAttachingFeeTypeList.AddRange(BRRefCusTaxOrFee.GetImportLicenseFees(Factory, effectiveDate));
					return importLicenseAttachingFeeTypeList;
				});
			}
		}
	}
}
