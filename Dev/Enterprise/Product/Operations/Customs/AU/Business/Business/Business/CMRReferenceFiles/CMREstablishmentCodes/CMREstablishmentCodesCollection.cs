using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.ComponentModel;

namespace Enterprise.Customs.AU.Declaration.Business
{
	[ModuleID(Enterprise.ZArchitecture.Modules.ModuleId.CMREstablishmentCodes)]
	public class CMREstablishmentCodesCollection : ActiveBusinessObjectCollection<CMREstablishmentCodes>
	{
		public CMREstablishmentCodesCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}
	}
}
