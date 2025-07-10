using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.Business.CusTempStorage
{
	public class CusTempStorageDecLookups : AutoCusTempStorageDecLookups
	{
		public CusTempStorageDecLookups(AutoCusTempStorageDec parent)
			: base(parent)
		{
		}

		public virtual CodeDescriptionPairList DeclarationTypeList => new CodeDescriptionPairList();

		public virtual CodeDescriptionPairList IdentificationIndicatorList => new CodeDescriptionPairList();

		public virtual CodeDescriptionPairList DeclarationStatusList => Factory.GetCachedValue<TempStorageDeclarationStatusList>();
	}
}
