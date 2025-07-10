using System.Collections.Generic;
using System.Linq;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Business.EventManagement;
using Enterprise.ZArchitecture.Business.EventManagement;

namespace Enterprise.Customs.EU.ExitControl.Business
{
	public class CusExitReportProcessHandlingInfo : ProcessHandlingInfo
	{
		public CusExitReportProcessHandlingInfo(CusExitReport report)
			: base(report)
		{
		}

		protected override IEnumerable<CascadingLink> PopulateCascadingTargets(IStmALog logBeingAdded)
		{
			return Enumerable.Empty<CascadingLink>();
		}

		protected override IEnumerable<IBaseTrigger> PopulateParentTriggers(IStmALog logBeingAdded)
		{
			var exitHeader = report.Header;
			var relatedDeclaration = exitHeader?.Declaration;
			var relatedShipment = exitHeader?.Shipment;
			var logParent = (IStmALogParent)relatedDeclaration ?? (IStmALogParent)relatedShipment ?? exitHeader;
			return logParent != null
					? TriggerProvider.LoadAllMilestonesAndLineTriggersForEvent(logParent, logBeingAdded, TriggerLineTypes.Codes.CusExitReport).Select(p => p.trigger)
					: Enumerable.Empty<IBaseTrigger>();
		}

		CusExitReport report => (CusExitReport)LogParent;
	}
}
