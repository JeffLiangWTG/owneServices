using CargoWiseOne.ResourceStrings;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.Workflow.Integration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.EDI.MasterFiles.GUI
{
	public class CompetencyRequirementsTaskStatusChangeResponder : IStatusChangeResponder
	{
		public StatusChangeResponder Responder => StatusChangeResponder.CompetencyRequirements;

		bool IStatusChangeResponder.RespondsToStatusChange(IProcessTask task, string newStatus)
		{
			if (task is WorkItemProcessTask)
			{
				switch (newStatus)
				{
					case ProcessTaskStatusCodeList.Codes.Working:
					case ProcessTaskStatusCodeList.Codes.Closed:
						return true;
				}
			}
			return false;
		}

		StatusChangeResult IStatusChangeResponder.RespondToChange(IProcessTask task, string newStatus)
		{
			if (!CanChangeStatus(task, newStatus).result)
			{
				var concreteTask = (WorkItemProcessTask)task;
				ShowTaskSkillsForm(concreteTask);
				if (concreteTask.HasAssignedStaffNotCompletedAnyAssessRequirements())
				{
					return StatusChangeResult.ChangeNotHandled;
				}
			}

			return StatusChangeResult.ChangeHandled;
		}

		static void ShowTaskSkillsForm(WorkItemProcessTask task)
		{
			using (var form = new TaskSkillsForm(task))
			{
				form.CreateSkillLearningTaskFunction = WorkItemProcessTask.CreateSkillLearningTask;
				ZFormModaliser.ShowDialogWithoutDispose(form);
			}
		}

		public (bool result, string reason) CanChangeStatus(IProcessTask task, string newStatus)
		{
			var concreteTask = (WorkItemProcessTask)task;
			return !concreteTask.P9_GS_NKAssignedStaffMember.IsEmpty && concreteTask.IsReviewTask && concreteTask.HasAssignedStaffNotCompletedAnyAssessRequirements()
				? (false, Res.GetString("4B051B60-68B7-4067-8E1A-EC4BF7C4C387", "Staff don't have all skills."))
				: (true, null);
		}
	}
}
