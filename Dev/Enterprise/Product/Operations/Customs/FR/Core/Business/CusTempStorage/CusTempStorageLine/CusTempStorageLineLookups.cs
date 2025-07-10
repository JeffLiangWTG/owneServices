using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.FR.Business.CusTempStorage
{
	public class CusTempStorageLineLookups : EU.Business.CusTempStorage.CusTempStorageLineLookups
	{
		public CusTempStorageLineLookups(CusTempStorageLine parent) : base(parent)
		{
		}

		protected new CusTempStorageLine Parent => (CusTempStorageLine)base.Parent;

		public CodeDescriptionPairList OwnerReferenceTypeList => Factory.GetCachedValue<OwnerReferenceTypeList>();

		public CodeDescriptionPairList UnionStatusList => Factory.GetCachedValue<UnionStatusList>();

		public CodeDescriptionPairList GoodsTypeList => Factory.GetCachedValue<GoodsTypeList>();

		public CodeDescriptionPairList WeightUQList => Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.Weight);

		public CodeDescriptionPairList GoodsLocations => Factory.GetCachedValue("FR.CusTempStorageLineLookups.GoodsLocations" + Parent.Dec?.StorageHeader?.SJH_OA_Presenter, () =>
		{
			if (Parent.Dec?.StorageHeader?.Presenter != null)
			{
				var country = Parent.Dec?.StorageHeader?.CountryCode ?? GlbCompany.CurrentCompany.Country.Code;
				var authorizedLocations = Parent.Dec.StorageHeader.Presenter.GetCusAuthorisationHeadersWithApplyingAddressAndType(CusAuthorizationHeaderTypeList.Codes.AuthorizedLocation, country);
				return authorizedLocations.Aggregate(new CodeDescriptionPairList(), (list, auth) =>
				{
					var desc = ZString.Join(", ", new[]
					{
						auth.AppliesTo?.EffectiveCompanyName ?? ZString.Empty,
						auth.AppliesTo?.Address1 ?? ZString.Empty,
						auth.CPH_PermitDescription
					}.Where(x => !x.IsEmpty).ToArray());
					list.AddPair(auth.CPH_Number.Left(CusTempStorageLine.Schema.TSL_LocationOfGoodsMaxLength), desc);
					return list;
				});
			}
			return new CodeDescriptionPairList();
		});

		public CodeDescriptionPairList PackageTypeList => UniversalReferenceDataHelper.GetCachedPackageTypeList(Parent.Factory);
	}
}
