using CargoWise.EntityFramework;
using Enterprise.Customs.CA.Business;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.CA.Module
{
	public class DailyNoticeReconciliationModule : StatementModule
	{
		public override ModuleIdentifier ID => ModuleIDs.Customs.CA.CADailyNoticeReconciliation;
		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.CADailyNoticeReconciliation;

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.Customs.CA.CADailyNoticeReconciliation);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new DailyNoticeReconciliationModuleCollection(Factory);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new DailyNoticeReconciliationFilterStripBusinessObject();
		}

		public override string WorkflowType => DailyNoticeWorkflowDescriptor.Constants.Code;
	}
}
