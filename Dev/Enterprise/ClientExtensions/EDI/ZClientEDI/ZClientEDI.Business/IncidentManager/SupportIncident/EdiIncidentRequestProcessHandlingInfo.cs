using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Business.Business.EventManagement;
using Enterprise.ZArchitecture.Business.EventManagement;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public class EdiIncidentRequestProcessHandlingInfo : ProcessHandlingInfo
	{
		public EdiIncidentRequestProcessHandlingInfo(EdiIncidentRequest request) : base(request)
		{
		}

		EdiIncidentRequest Request
		{
			get { return (EdiIncidentRequest)LogParent; }
		}

		protected override IEnumerable<CascadingLink> PopulateCascadingTargets(IStmALog logBeingAdded)
		{
			var incident = Request.RelatedSupportIncident;
			if (incident != null)
			{
				var matchedTasks = incident.WorkflowItems.Where(x => x.P9_RespondToCascadedEvents && x.P9_SE_NKMilestoneEvent == logBeingAdded.SL_SE_NKEvent);
				if (matchedTasks.Any())
				{
					yield return new CascadingLink()
					{
						Parent = incident,
						Triggers = matchedTasks.ToArray()
					};
				}
			}
		}
	}
}
