using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Business.GeneralLedger.GLConsolidations
{
	[ModuleID(ModuleId.GLConsolidationGroups)]
	public class AccConsolidationGroupCollection : ActiveBusinessObjectCollection<AccConsolidationGroup>
	{
		public AccConsolidationGroupCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}
	}
}