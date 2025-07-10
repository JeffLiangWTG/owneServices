using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.CA.Business
{
	[ModuleID(ModuleId.CACFIAEndUseCodes)]
	public class CACFIAEndUseCodesCollection : BusinessObjectCollection<CACFIAEndUseCodes>
	{
		public CACFIAEndUseCodesCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}
	}
}
