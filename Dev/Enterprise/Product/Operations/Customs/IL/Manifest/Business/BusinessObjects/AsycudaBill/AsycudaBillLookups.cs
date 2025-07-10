using CargoWise.Types;
using Enterprise.Customs.IL.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IL.Manifest.Business
{
	public class AsycudaBillLookups : ASYCUDA.Business.AsycudaBillLookups
	{
		public AsycudaBillLookups(AsycudaBill parent) : base(parent)
		{
		}

		public CodeDescriptionPairList BillKindList
		{
			get
			{
				return Factory.GetCachedValue<AsycudaBillKindList>();
			}
		}

		public CodeDescriptionPairList ConditionList => Factory.GetCachedValue<ILBillConditionList>();

		protected override CodeDescriptionPairList PackageTypeListCore
		{
			get
			{
				var list = Universal.RefCusCodeListTypes.GetCachedList(Factory, Core.Constants.CountryCodes.Israel, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ILManifestPackageTypes, ZDateTime.Today);
				list.SortByDescription();
				return list;
			}
		}
	}
}
