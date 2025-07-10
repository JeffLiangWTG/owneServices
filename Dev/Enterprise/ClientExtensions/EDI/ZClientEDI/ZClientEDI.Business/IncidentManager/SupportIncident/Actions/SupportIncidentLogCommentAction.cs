using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public class SupportIncidentLogCommentAction : SupportIncidentAction
	{
		public SupportIncidentLogCommentAction(SupportIncident incident)
			: base(incident)
		{
		}

		protected override void PerformAction()
		{
			if (!Comment.IsEmpty)
			{
				if (!ChangeCriticalityFrom.IsEmpty)
				{
					Incident.AddStaffMessageToCustomer(Comment);
					string changedLog = string.Format(CultureInfo.CurrentCulture, IncidentConstants.CriticalityChangedMessageTemplate, ChangeCriticalityFrom, Incident.IM_Priority);
					Incident.AddSystemMessageToCustomer(changedLog);
				}
				else if (AddInternalLog)
				{
					if (NeedNotifyInternal)
					{
						Incident.NotifyInternalSubscribersForInternalLog = true;
					}

					Incident.AddInternalMessage(Comment);
				}
				else
				{
					Incident.AddStaffMessageToCustomer(Comment);
				}
			}
		}

		public ZString ChangeCriticalityFrom { get; set; }
		public ZBool AddInternalLog { get; set; }

		protected override void ValidateComment()
		{
			CommentInfo.ClearAllNotifications();
			if (!ChangeCriticalityFrom.IsEmpty)
			{
				MandatoryValidation.CheckEntered(CommentInfo);
			}
		}
	}
}

