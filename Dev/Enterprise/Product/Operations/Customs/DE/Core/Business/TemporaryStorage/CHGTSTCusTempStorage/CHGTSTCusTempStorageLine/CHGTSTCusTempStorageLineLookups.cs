using CargoWise.Types;
using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.DE.Business.CusTempStorage
{
	public class CHGTSTCusTempStorageLineLookups : CusTempStorageLineLookups
	{
		public CHGTSTCusTempStorageLineLookups(AutoCusTempStorageLine parent) : base(parent)
		{
		}

		public new CHGTSTCusTempStorageLine Parent => (CHGTSTCusTempStorageLine)base.Parent;

		public override CodeDescriptionPairList OwnerReferenceTypeList
		{
			get
			{
				CodeDescriptionPairList result = null;
				if (Parent.IsAWBDeclaration)
				{
					result = Factory.GetCachedValue("DE|CHGTSTCusTempStorageLineLookups|OwnerReferenceTypeList|AWB", () => // Cache Key
						new CodeDescriptionPairList()
						{
							{ new CodeDescriptionPair(Business.OwnerReferenceTypeList.Codes.AWB, Business.OwnerReferenceTypeList.Descriptions.AWB) },
							{ new CodeDescriptionPair(Business.OwnerReferenceTypeList.Codes.ULD, Business.OwnerReferenceTypeList.Descriptions.ULD) }
						});
				}
				return result ?? new CodeDescriptionPairList();
			}
		}

		public override CodeDescriptionPairList LocationOfGoodsList
		{
			get
			{
				var orgProxy = GlbBranch.CurrentBranch.OrgProxy ?? GlbCompany.CurrentCompany.OrgProxy;
				var customsOffice = Parent.StorageHeader?.SJH_CustomsOffice ?? ZString.Empty;
				return orgProxy.GetLocationOfGoodsListForTemporaryStorage(customsOffice);
			}
		}
	}
}
