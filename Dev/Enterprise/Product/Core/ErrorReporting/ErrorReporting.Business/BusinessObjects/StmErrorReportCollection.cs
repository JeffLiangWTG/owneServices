using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ErrorReporting.Business
{
	[ModuleID(ModuleId.ErrorReporting)]
	public class StmErrorReportCollection : ActiveBusinessObjectCollection<StmErrorReport>
	{
		public StmErrorReportCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public StmErrorReportCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
		{
		}
	}
}
