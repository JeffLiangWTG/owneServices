using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.DE.Business.CusTempStorage
{
	public class CUSPCSCusTempStorageDecLookups : CusTempStorageDecLookups
	{
		public CUSPCSCusTempStorageDecLookups(AutoCusTempStorageDec parent) : base(parent)
		{
		}

		public new CUSPCSCusTempStorageDec Parent => (CUSPCSCusTempStorageDec)base.Parent;

		public override CodeDescriptionPairList IdentificationIndicatorList => Parent.GetIdentificationIndicatorListExcludingSIN();
	}
}
