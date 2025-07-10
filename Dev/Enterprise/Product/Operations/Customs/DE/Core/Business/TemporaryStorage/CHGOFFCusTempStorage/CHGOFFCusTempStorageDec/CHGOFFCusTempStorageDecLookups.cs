using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.DE.Business.CusTempStorage
{
	public class CHGOFFCusTempStorageDecLookups : CusTempStorageDecLookups
	{
		public CHGOFFCusTempStorageDecLookups(AutoCusTempStorageDec parent) : base(parent)
		{
		}

		public new CHGOFFCusTempStorageDec Parent => (CHGOFFCusTempStorageDec)base.Parent;

		public override CodeDescriptionPairList IdentificationIndicatorList => Parent.GetIdentificationIndicatorListExcludingSIN();
	}
}
