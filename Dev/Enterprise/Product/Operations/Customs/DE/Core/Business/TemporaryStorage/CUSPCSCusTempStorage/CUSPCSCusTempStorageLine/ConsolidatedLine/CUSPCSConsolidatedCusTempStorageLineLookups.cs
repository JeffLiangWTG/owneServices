using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.DE.Business.CusTempStorage
{
	public class CUSPCSConsolidatedCusTempStorageLineLookups : CusTempStorageLineLookups
	{
		public CUSPCSConsolidatedCusTempStorageLineLookups(AutoCusTempStorageLine parent) : base(parent)
		{
		}

		public override CodeDescriptionPairList OwnerReferenceTypeList
		{
			get
			{
				return Factory.GetCachedValue("CUSPCSConsolidatedCusTempStorageLineLookups.OwnerReferenceTypeList", () => new CodeDescriptionPairList
				{
					new CodeDescriptionPair(Business.OwnerReferenceTypeList.Codes.AWB, Business.OwnerReferenceTypeList.Descriptions.AWB),
					new CodeDescriptionPair(Business.OwnerReferenceTypeList.Codes.ULD, Business.OwnerReferenceTypeList.Descriptions.ULD)
				});
			}
		}

		public IBusinessObjectCollection CusTempStorageRegLineCollection
		{
			get
			{
				var customsOffice = Parent?.Dec?.StorageHeader?.SJH_CustomsOffice ?? ZString.Empty;
				return Factory.GetCachedValue("DE.CUSPCSConsolidatedCusTempStorageLineLookups.CusTempStorageRegLineCollection_" + customsOffice, () => TemporaryStorageHelper.GetCusTempStorageRegLineCollection(customsOffice, Factory));
			}
		}

		new CUSPCSConsolidatedCusTempStorageLine Parent => (CUSPCSConsolidatedCusTempStorageLine)base.Parent;
	}
}
