using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.DE.Business.CusTempStorage
{
	public class CHGSPOCusTempStorageDecLookups : CusTempStorageDecLookups
	{
		public CHGSPOCusTempStorageDecLookups(AutoCusTempStorageDec parent) : base(parent)
		{
		}

		public override CodeDescriptionPairList IdentificationIndicatorList => Factory.GetCachedValue("CHGSPOCusTempStorageDec|IdentificationIndicatorList", () => new CodeDescriptionPairList()
		{
			new CodeDescriptionPair(Messaging.TemporaryStorageIdentificationIndicatorList.Codes.REG, Messaging.TemporaryStorageIdentificationIndicatorList.Descriptions.REG)
		});
	}
}
