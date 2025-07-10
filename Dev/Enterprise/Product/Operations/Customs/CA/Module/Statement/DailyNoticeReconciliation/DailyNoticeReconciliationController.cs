using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.CA.Module
{
	public class DailyNoticeReconciliationController : StatementController
	{
		public override ModuleIdentifier ModuleID => ModuleIDs.Customs.CA.CADailyNoticeReconciliation;
		public override ControllerID ID => ControllerIDs.Customs.CA.CADailyNoticeReconciliation;
		protected override SecurityCheckpoint CheckPointForView => Env.Security.CADailyNoticeReconciliationView;
	}
}
