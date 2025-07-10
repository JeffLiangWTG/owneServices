using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.DE.Business.CusTempStorage
{
	public class CHGOFFCusTempStorageLineLookups : CusTempStorageLineLookups
	{
		public CHGOFFCusTempStorageLineLookups(AutoCusTempStorageLine parent) : base(parent)
		{
		}

		public new CHGOFFCusTempStorageLine Parent => (CHGOFFCusTempStorageLine)base.Parent;

		public override CodeDescriptionPairList OwnerReferenceTypeList
		{
			get
			{
				CodeDescriptionPairList result = null;
				if (Parent.IsAWBDeclaration)
				{
					result = Factory.GetCachedValue("DE|CHGOFFCusTempStorageLineLookups|OwnerReferenceTypeList|AWB", () =>
						new CodeDescriptionPairList()
						{
							{ new CodeDescriptionPair(Business.OwnerReferenceTypeList.Codes.AWB, Business.OwnerReferenceTypeList.Descriptions.AWB) },
							{ new CodeDescriptionPair(Business.OwnerReferenceTypeList.Codes.ULD, Business.OwnerReferenceTypeList.Descriptions.ULD) }
						});
				}
				return result ?? new CodeDescriptionPairList();
			}
		}
	}
}
