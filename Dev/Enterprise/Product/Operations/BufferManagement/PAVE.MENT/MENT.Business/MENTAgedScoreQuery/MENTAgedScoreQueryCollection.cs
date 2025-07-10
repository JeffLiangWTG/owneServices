using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.PAVE.MENT.Business
{
	[ModuleID(ModuleId.MENTAgedScoreQuery)]
	public class MENTAgedScoreQueryCollection : ActiveBusinessObjectCollection<MENTAgedScoreQuery>
	{
		public MENTAgedScoreQueryCollection(BusinessObjectFactory factory, ZQuery query)
			: base(factory, query)
		{
		}

		public MENTAgedScoreQueryCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}
	}
}
