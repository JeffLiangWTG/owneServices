using CargoWise.EntityFramework;

namespace Enterprise.ZArchitecture.Business
{
	public class StmModuleFilterLookups : ZLookups
	{
		public StmModuleFilterLookups(BusinessObject parent)
			: base(parent)
		{
		}

		public StmModuleFilterTypes FilterTypes => Factory.GetCachedValue("StmModuleFilterLookups.FilterTypes", () => new StmModuleFilterTypes());
	}
}
