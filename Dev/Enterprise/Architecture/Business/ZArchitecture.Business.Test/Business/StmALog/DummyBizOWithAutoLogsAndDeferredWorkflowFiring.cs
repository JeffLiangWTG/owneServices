using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.ZArchitecture.Business.Testing
{
	sealed class DummyBizOWithAutoLogsAndDeferredWorkflowFiring : DummyBizOWithAutoLogs, IStmALogParent
	{
		public DummyBizOWithAutoLogsAndDeferredWorkflowFiring(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{ }

		bool IStmALogParent.DeferFiringWorkflow
		{
			get { return true; }
		}
	}
}
