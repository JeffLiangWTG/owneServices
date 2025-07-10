using Enterprise.Customs.Universal.Helper;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.CL.Manifest.Business
{
	public class AsycudaManifestHeaderLookups : ASYCUDA.Business.AsycudaManifestHeaderLookups
	{
		public AsycudaManifestHeaderLookups(AsycudaManifestHeader parent) : base(parent)
		{
		}

		public override CodeDescriptionPairList Natures => Parent.IsAir ? ShipmentTypeList.Export22AndImport23() : base.Natures;

		public CodeDescriptionPairList TSS_Types
		{
			get
			{
				return new CodeDescriptionPairList
				{
					new CodeDescriptionPair(TransshipmentTypeCodeList.Codes.TR, TransshipmentTypeCodeList.Descriptions.TR),
					new CodeDescriptionPair(TransshipmentTypeCodeList.Codes.REX, TransshipmentTypeCodeList.Descriptions.REX)
				};
			}
		}
	}
}
