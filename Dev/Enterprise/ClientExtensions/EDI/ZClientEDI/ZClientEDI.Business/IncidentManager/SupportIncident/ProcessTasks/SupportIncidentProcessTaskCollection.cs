using System.Linq;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public class SupportIncidentProcessTaskCollection : ProcessTaskCollection
	{
		public SupportIncidentProcessTaskCollection(SupportIncident parent)
			: base(parent)
		{
		}

		public new SupportIncidentProcessTask this[int index]
		{
			get { return (SupportIncidentProcessTask)Elements[index]; }
		}

		public virtual new SupportIncidentProcessTask AddNew()
		{
			return (SupportIncidentProcessTask)base.AddNew();
		}

		public bool AnyTaskIsWorkingOrCompleted
		{
			get
			{
				return this.Cast<ProcessTask>().Any(task => task.P9_Status == ProcessTaskStatusCodeList.Codes.Working
														|| task.P9_Status == ProcessTaskStatusCodeList.Codes.Closed
														|| task.P9_Status == ProcessTaskStatusCodeList.Codes.Suspended);
			}
		}

		#region Defaults

		protected override void SetDefaultsForNewChildCore(IWorkflowProviderCollection collection, ProcessTask processTask, bool defaultAssignedStaff)
		{
			base.SetDefaultsForNewChildCore(collection, processTask, defaultAssignedStaff);
			PopulateClientAndContact(processTask);
			PopulateDefaultStaffForProjectFeatureRequestTask(processTask);
		}

		void PopulateClientAndContact(ProcessTask processTask)
		{
			processTask.P9_OA = Parent.IM_OA_BranchAddress;
			processTask.P9_OC = Parent.IM_OC_Contact;
		}

		void PopulateDefaultStaffForProjectFeatureRequestTask(ProcessTask processTask)
		{
			if (Parent != null && Parent.IM_Category == SupportIncidentCategoriesList.Codes.FeatureRequest && Parent.IsProjectRelatedIncident)
			{
				if (processTask.P9_Type == "PJM" && !Parent.RelatedProjectPK.IsEmpty && !Parent.RelatedProject.WKP_GS_NKProjectManager.IsEmpty)
				{
					processTask.P9_GS_NKAssignedStaffMember = Parent.RelatedProject.WKP_GS_NKProjectManager;
				}
				if (processTask.P9_Type == "BSC" && !Parent.IM_GS_NKSpecifiedBy.IsEmpty)
				{
					processTask.P9_GS_NKAssignedStaffMember = Parent.IM_GS_NKSpecifiedBy;
				}
			}
		}

		public new SupportIncident Parent
		{
			get { return (SupportIncident)base.Parent; }
		}

		#endregion
	}
}

