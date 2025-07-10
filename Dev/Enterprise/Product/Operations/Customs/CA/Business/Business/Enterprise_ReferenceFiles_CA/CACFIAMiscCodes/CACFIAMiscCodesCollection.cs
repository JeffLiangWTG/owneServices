using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.CA.Business
{
	[ModuleID(ModuleId.CACFIAMiscCodes)]
	public class CACFIAMiscCodesCollection : ZZRefCusCodeListWrapperCollection<CACFIAMiscCodes>
	{
		public CACFIAMiscCodesCollection(BusinessObjectFactory factory)
			: base(factory, Core.Constants.CountryCodes.Canada, UniversalReferenceConstants.RefCusCodeListType.Codes.CFIAMiscCodes, ZDateTime.Now)
		{
		}
	}
}
