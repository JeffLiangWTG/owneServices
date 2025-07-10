using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.CA.Business
{
	[ModuleID(ModuleId.CACSubLocation)]
	public class CACSubLocationCollection : ZZRefCusCodeListWrapperCollection<CACSubLocation>
	{
		public CACSubLocationCollection(BusinessObjectFactory factory)
			: base(factory, Core.Constants.CountryCodes.Canada, UniversalReferenceConstants.RefCusCodeListType.Codes.SubLocation, ZDateTime.Now)
		{
		}
	}
}
