using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.DE.Business.CusTempStorage
{
	public class REXDISCusTempStorageDecLookups : CusTempStorageDecLookups
	{
		public REXDISCusTempStorageDecLookups(AutoCusTempStorageDec parent) : base(parent)
		{
		}
		protected new REXDISCusTempStorageDec Parent => (REXDISCusTempStorageDec)base.Parent;

		public CodeDescriptionPairList ProcedureTypeList => Factory.GetCachedValue<TemporaryStorageProcedureTypeList>();

		public override CodeDescriptionPairList IdentificationIndicatorList => Parent.GetIdentificationIndicatorListIncludingSIN();
	}
}
