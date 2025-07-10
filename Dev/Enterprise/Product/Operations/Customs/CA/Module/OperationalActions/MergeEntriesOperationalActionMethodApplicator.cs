using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.CA.Business.OperationalAction;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Customs.CA.Module.OperationalActions
{
	public class MergeEntriesOperationalActionMethodApplicator : OperationalActionMethodApplicator
	{
		public MergeEntriesOperationalActionMethodApplicator(BusinessObjectFactory factory)
			: base(Res.GetString("C61891F4-0F1E-48DD-87DD-9AA0528F8158", "Merge Entries operational action"), factory)
		{
		}

		protected override void ApplyCore(IOperationalActionSectionLog log, BusinessObject[] targets)
		{
			var declarations = from target in targets select target as JobDeclaration;
			var logNotificationWrapper = new OperationalActionLogAndUserNotificationWrapper(new GUI.MessageInstructionUserNotification(), log, false, false);
			var runner = new MergeEntriesOperationalActionRunner(logNotificationWrapper);
			runner.PerformFunctionOperationalAction(declarations.ToArray());
		}
	}
}
