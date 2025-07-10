using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Customs.DE.Messaging.MessageSchema;

namespace Enterprise.Customs.DE.Business.CusTempStorage
{
	public class CusTempStorageLineLookups : EU.Business.CusTempStorage.CusTempStorageLineLookups
	{
		public CusTempStorageLineLookups(AutoCusTempStorageLine parent) : base(parent)
		{
		}

		new CusTempStorageLine Parent => (CusTempStorageLine)base.Parent;

		public virtual CodeDescriptionPairList CustodianIdentifierBranchNoList
		{
			get
			{
				var eoriCode = Parent.TSL_CustodianIdentifier;
				return Factory.GetCachedValue(ZString.Format("DE|BranchNoList|{0}", eoriCode), () => GetEoriBranchNumbers(eoriCode));// Cache Key
			}
		}

		public virtual CodeDescriptionPairList GoodsOwnerIdentifierBranchNoList
		{
			get
			{
				var eoriCode = Parent.TSL_GoodsOwnerIdentifier;
				return Factory.GetCachedValue(ZString.Format("DE|BranchNoList|{0}", eoriCode), () => GetEoriBranchNumbers(eoriCode));// Cache Key
			}
		}

		public CodeDescriptionPairList GoodsTypeList => Factory.GetCachedValue<DEGoodsTypeList>();

		public CodeDescriptionPairList PackageTypeList =>
			Universal.RefCusCodeListTypes.GetCachedList(Parent.Factory,
				Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations,
				Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
				ZDateTime.Today);

		public virtual CodeDescriptionPairList LocationOfGoodsList
		{
			get
			{
				var custodianHeader = Parent.Custodian?.Header;
				var customsOffice = Parent.StorageHeader?.SJH_CustomsOffice ?? ZString.Empty;
				return custodianHeader.GetLocationOfGoodsListForTemporaryStorage(customsOffice);
			}
		}

		public RefCountryCollection Countries => new RefCountryCollection(Factory);

		public OrgHeaderCollection CustodiansAndTraders => new OrganisationsFindBoxCollection(Factory);

		public virtual CodeDescriptionPairList OwnerReferenceTypeList => Factory.GetCachedValue<OwnerReferenceTypeList>();

		public virtual CodeDescriptionPairList UnionStatusList => Factory.GetCachedValue<DEUnionStatusList>();

		CodeDescriptionPairList GetEoriBranchNumbers(ZString eoriCode)
		{
			var result = new CodeDescriptionPairList();
			if (!eoriCode.IsEmpty && EORIHelper.ValidEoriLength(eoriCode))
			{
				var organisation = EORIHelper.GetOrgHeaderFromEoriCode(Factory, eoriCode);
				if (organisation != null)
				{
					var eoriBranches = organisation.CustomsCodes.GetOrgCusCodesForCodeAndCountry(GermanyOrgCusCodeInfo.OrgCusCodes.EoriBranchSuffix, Core.Constants.CountryCodes.Germany).Where(x => x.OK_CustomsRegNo.Length <= ATLASMessageSchema.EoriBranchCodeMaxLength);
					foreach (var eoriBranch in eoriBranches)
					{
						result.AddPair(eoriBranch.OK_CustomsRegNo, eoriBranch.PremisesAddress?.OA_Code ?? ZString.Empty);
					}
				}
			}
			return result;
		}
	}
}
