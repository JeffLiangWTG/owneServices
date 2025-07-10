using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.DE.Business.CusTempStorage
{
	public class PRLCONCusTempStorageDecLookups : CusTempStorageDecLookups
	{
		public PRLCONCusTempStorageDecLookups(AutoCusTempStorageDec parent) : base(parent)
		{
		}

		protected new PRLCONCusTempStorageDec Parent => (PRLCONCusTempStorageDec)base.Parent;

		public override CodeDescriptionPairList IdentificationIndicatorList => Parent.GetIdentificationIndicatorListExcludingSIN();
	}
}
