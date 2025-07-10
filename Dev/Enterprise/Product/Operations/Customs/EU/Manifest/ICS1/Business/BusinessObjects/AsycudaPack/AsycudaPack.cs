using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Customs.EU.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.EU.Manifest.Business
{
	public class AsycudaPack : ASYCUDA.Business.AsycudaPack, Integration.Customs.ASYCUDA.EUManifest.IAsycudaPack
	{
		public AsycudaPack(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new AsycudaBill Bill => (AsycudaBill)base.Bill;

		public new AsycudaPackValidation Validation => (AsycudaPackValidation)base.Validation;

		protected override ManifestBase.AsycudaPackValidation GetNewValidation()
		{
			AsycudaPackValidation result = null;
			if (Bill?.Header?.IsICSManifest ?? false)
			{
				result = new ICSAsycudaPackValidation(this);
			}
			else
			{
				result = new AsycudaPackValidation(this);
			}
			return result;
		}

		public bool IsBulk => IsBulkCore;

		protected virtual bool IsBulkCore => BulkPackageUnitTypeList.ContainsCode(APA_PackUQ);

		public bool IsUnpacked => IsUnpackedCore;

		protected virtual bool IsUnpackedCore => UnpackedPackageUnitTypeList.ContainsCode(APA_PackUQ);

		CodeDescriptionPairList BulkPackageUnitTypeList =>
			Universal.RefCusCodeListTypes.GetCachedListMatchSingleAttributeValues(Factory,
					Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations,
					Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
					UNPackTypeStartDate,
					false,
					UniversalReferenceConstants.PackageUnitAttributes.Bulk);

		CodeDescriptionPairList UnpackedPackageUnitTypeList =>
			Universal.RefCusCodeListTypes.GetCachedListMatchSingleAttributeValues(Factory,
					Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations,
					Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
					UNPackTypeStartDate,
					false,
					UniversalReferenceConstants.PackageUnitAttributes.BreakBulk);
	}
}
