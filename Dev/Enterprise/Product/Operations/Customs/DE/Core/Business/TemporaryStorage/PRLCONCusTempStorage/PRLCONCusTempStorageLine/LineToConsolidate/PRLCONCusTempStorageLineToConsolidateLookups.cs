using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.DE.Business.CusTempStorage
{
	public class PRLCONCusTempStorageLineToConsolidateLookups : CusTempStorageLineLookups
	{
		public PRLCONCusTempStorageLineToConsolidateLookups(AutoCusTempStorageLine parent) : base(parent)
		{
		}

		public override CodeDescriptionPairList OwnerReferenceTypeList
		{
			get
			{
				return Factory.GetCachedValue("DE|PRLCONCusTempStorageLineToConsolidateLookups|OwnerReferenceTypeList", () => new CodeDescriptionPairList
				{
					new CodeDescriptionPair(Business.OwnerReferenceTypeList.Codes.AWB, Business.OwnerReferenceTypeList.Descriptions.AWB),
					new CodeDescriptionPair(Business.OwnerReferenceTypeList.Codes.ULD, Business.OwnerReferenceTypeList.Descriptions.ULD),
				});
			}
		}

		public IBusinessObjectCollection CusTempStorageRegLineCollection
		{
			get
			{
				var customsOffice = Parent?.StorageHeader?.SJH_CustomsOffice ?? ZString.Empty;
				return TemporaryStorageHelper.GetCusTempStorageRegLineCollection(customsOffice, Factory);
			}
		}

		new PRLCONCusTempStorageLine Parent => (PRLCONCusTempStorageLine)base.Parent;
	}
}
