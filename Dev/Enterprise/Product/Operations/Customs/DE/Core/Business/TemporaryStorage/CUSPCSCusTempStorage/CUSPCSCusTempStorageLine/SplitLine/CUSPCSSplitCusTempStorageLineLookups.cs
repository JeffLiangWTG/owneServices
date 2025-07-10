using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.DE.Business.CusTempStorage
{
	public class CUSPCSSplitCusTempStorageLineLookups : CusTempStorageLineLookups
	{
		public CUSPCSSplitCusTempStorageLineLookups(AutoCusTempStorageLine parent) : base(parent)
		{
		}

		public override CodeDescriptionPairList OwnerReferenceTypeList
		{
			get
			{
				return Factory.GetCachedValue("DE|CUSPCSSplitCusTempStorageLineLookups|OwnerReferenceTypeList", () => new CodeDescriptionPairList
				{
					new CodeDescriptionPair(Business.OwnerReferenceTypeList.Codes.AWB, Business.OwnerReferenceTypeList.Descriptions.AWB),
					new CodeDescriptionPair(Business.OwnerReferenceTypeList.Codes.ULD, Business.OwnerReferenceTypeList.Descriptions.ULD),
					new CodeDescriptionPair(Business.OwnerReferenceTypeList.Codes.ZZZ, Business.OwnerReferenceTypeList.Descriptions.ZZZ)
				});
			}
		}
	}
}
