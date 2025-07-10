using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Services.OperationalActions.Business
{
	sealed class EventPseudoApplicator : PseudoApplicator
	{
		public EventPseudoApplicator(OperationalActionRunner runner)
			: base(runner, Res.GetString("EventPseudoApplicator|Name", "Events"))
		{
			staffCode = GlbStaff.CurrentUser == null ? ZString.Empty : GlbStaff.CurrentUser.GS_Code;
			eventLogText = string.Format((NoResString)"Ran Action ({0})", Action.SU_MenuName);
		}

		protected override void ApplyCore(IOperationalActionSectionLog log, BusinessObject[] targets)
		{
			foreach (BusinessObject target in targets)
			{
				var eventType = Events.All[Action.DocumentEvent.SE_Code];
				var eventLog = target.GetLogs().AddNew(eventType, eventLogText);
				eventLog.SL_GS_NKUser = staffCode;
			}
		}

		readonly ZString staffCode;
		readonly ZString eventLogText;
	}
}
