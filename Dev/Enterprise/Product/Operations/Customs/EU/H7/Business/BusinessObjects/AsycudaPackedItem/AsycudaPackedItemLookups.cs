
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.H7.Business
{
	public class AsycudaPackedItemLookups : ASYCUDA.Business.AsycudaPackedItemLookups
	{
		public AsycudaPackedItemLookups(ASYCUDA.Business.AsycudaPackedItem parent) : base(parent)
		{
		}

		public CodeDescriptionPairList CustomsUQListForSupplement
		{
			get { return AsycudaUniversalReference.RefCusCodeListTypes.GetCachedList(Factory, Parent.CountryCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ); }
		}

		public CodeDescriptionPairList PackTypeList
		{
			get { return RefPackTypeCollection.GetAsCodeDescriptionPairWithStandardUnits(Factory); }
		}
	}
}
