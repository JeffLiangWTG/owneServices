using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.BufferManagement.Integration;
using Enterprise.Client.EDI.IssueManager.Business;
using Enterprise.Client.EDI.Modules;
using Enterprise.Client.EDI.ReleaseBuilds.Business;
using Enterprise.EConversation.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ProcessManagement.Business;
using Enterprise.ProcessManagement.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using WTG.DevTools.SourceControl;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public class NewWorkItem : EDIWorkItem, IWorkTaskRelatedItemProvider
	{
		public NewWorkItem(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new NewWorkItemFetchStrategy(this);
		}

		#region Saving

		public override void OnSaving()
		{
			base.OnSaving();
			OriginalIM_Status = (ZString)WKI_StatusInfo.OriginalValue;
		}
		ZString OriginalIM_Status;

		protected override INumberFountainProxy JobNumberFountain
		{
			get { return Modules.ClientNumberFountainRegistration.GetInstance().NewWorkItemNo; }
		}

		public sealed override void OnSaved(bool saveSucceeded)
		{
			base.OnSaved(saveSucceeded);

			if (saveSucceeded)
			{
				SendEmailsCore();

				if ((WKI_Status == ProcessTaskStatusCodeList.Codes.Closed && OriginalIM_Status != ProcessTaskStatusCodeList.Codes.Closed) ||
					(WKI_Status == ProcessTaskStatusCodeList.Codes.Cancelled && OriginalIM_Status != ProcessTaskStatusCodeList.Codes.Cancelled))
				{
					var conn = DbConnectionCrikey.GetAutoTesterUserTestsConnection();
					if (conn != null)
					{
						using (conn)
						using (var cmd = conn.Command(@"
							UPDATE AmnestyFailures SET AF_ExpiryDate = GETDATE() WHERE AF_IM = @workItemPk AND AF_ExpiryDate IS NULL
							UPDATE AspectPreApproval SET AP_DateInactive = GETDATE() WHERE AP_WorkItem = @workItemPk AND AP_DateInactive IS NULL
							"))
						{
							cmd.AddParameter("workItemPk", SqlDbType.UniqueIdentifier, this.PK.ToGuid());
							cmd.ExecuteNonQuery();
						}
					}
				}
			}
		}

		#endregion

		#region Lookups

		protected override WorkItemLookups GetNewLookupsCore()
		{
			return new NewWorkItemLookups(this);
		}

		public new NewWorkItemLookups Lookups
		{
			get { return (NewWorkItemLookups)base.Lookups; }
		}

		#endregion

		#region Validation

		protected override WorkItemCommonValidation GetNewValidationCore()
		{
			return new NewWorkItemValidation(this);
		}

		public new NewWorkItemValidation Validation
		{
			get { return (NewWorkItemValidation)base.Validation; }
		}

		#endregion

		#region Related Business Objects

		#region Related Items

		#region IWorkTaskRelatedItemSource

		protected override IEnumerable<WorkTaskRelatedItemModuleInfo> GetSupportedRelatedItemModules()
		{
			yield return EDIWorkTaskRelatedItemModuleInfo.EscalatedIncident(Factory, GetLinkedToIncidentAsDefectCausingWorkItemQuery());
			yield return EDIWorkTaskRelatedItemModuleInfo.Issue(Factory);
			yield return EDIWorkTaskRelatedItemModuleInfo.Quote(Factory, false);
			yield return EDIWorkTaskRelatedItemModuleInfo.EDIProject(Factory, false);
		}

		public bool IsAttachedToDefectOrIssue
		{
			get
			{
				foreach (IWorkTaskRelatedItem relatedItem in RelatedItems)
				{
					if (relatedItem.Type == EDIWorkTaskRelatedItemTypes.Defect ||
						 relatedItem.Type == EDIWorkTaskRelatedItemTypes.Issue)
					{
						return true;
					}
				}

				return false;
			}
		}

		/// <summary>
		/// Exclude from related items those defects that are already linked as the cause
		/// </summary>
		/// <returns></returns>
		ZQuery GetLinkedToIncidentAsDefectCausingWorkItemQuery()
		{
			ZQuery query = new ZQuery();

			ZDBOnlyQuery defectCausedSubQuery = new ZDBOnlyQuery(typeof(SupportIncident));
			ZDBOnlySubQuery defectCausedByWorkItem = new ZDBOnlySubQuery(typeof(GenPivot), WorkItemSchema.PK, true);
			defectCausedByWorkItem.AddToFilter(GenPivotSchema.XX_RelationType, EDIGenPivotTypes.DefectCausedByWorkItem);
			defectCausedByWorkItem.AddToFilter(GenPivotSchema.XX_Relation2TableCode, IncidentMainSchema.Constants.Prefix);
			defectCausedByWorkItem.AddToFilter(GenPivotSchema.XX_Relation1ID, PK);
			defectCausedByWorkItem.AddToFilter(GenPivotSchema.XX_Relation1TableCode, WorkItemSchema.Constants.Prefix);

			defectCausedSubQuery.AddSubQuery(IncidentMainSchema.PK, GenPivotSchema.XX_Relation2ID, defectCausedByWorkItem, JoinCondition.And);
			query.AddToFilter(defectCausedSubQuery);

			return query;
		}
		#endregion

		#endregion

		#region DefectCausedByWorkItem

		public new NewWorkItem DefectCausedByWorkItem => (NewWorkItem)base.DefectCausedByWorkItem;

		#endregion

		#endregion

		#region Properties

		#region Job Delivery Date

		[ResourceStringData("NewWorkItem|JobAgreedDeliveryDate", Caption = "Job Agreed Delivery Date", ShortCaption = "Del. Date")]
		public ZDateTime JobAgreedDeliveryDate
		{
			get { return JobWorkflow != null ? JobWorkflow.AgreedDeliveryDateLocal : ZDateTime.Empty; }
		}

		#endregion

		#region Product Area

		public ZString ProductArea
		{
			get { return WKI_WorkItemArea; }
			set { WKI_WorkItemArea = value; }
		}

		public ZPropertyInfo ProductAreaInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(ProductArea), x => WKI_WorkItemAreaInfo); }
		}

		#endregion

		#region WKI_ActivityType

		public override ZString WKI_ActivityType
		{
			get { return base.WKI_ActivityType; }
			set
			{
				if (WKI_ActivityType != value)
				{
					base.WKI_ActivityType = value;
				}
			}
		}

		#endregion

		#region WKI_ActivitySubtype

		public static string DefaultFixPatchTo
		{
			get { return EDIDataRegistry.Instance.WorkItemDefaultFixPriority.Value; }
		}

		public override ZString WKI_ActivitySubtype
		{
			get { return base.WKI_ActivitySubtype; }
			set
			{
				if (WKI_ActivitySubtype != value)
				{
					base.WKI_ActivitySubtype = value;

					if (!WKI_ActivitySubtype.IsEmpty)
					{
						if (IsDefect)
						{
							if (Lookups.ActivePriorities.ContainsCode(DefaultFixPatchTo))
							{
								WKI_Priority = DefaultFixPatchTo;
							}
						}
					}
				}
			}
		}

		public ZString WorkItemType
		{
			get { return !WKI_ActivitySubtype.IsEmpty ? WKI_ActivitySubtype + " - " + Lookups.AllActivitySubtypes.GetDescriptionFromCode(WKI_ActivitySubtype) : ""; }
		}

		public ZPropertyInfo WorkItemTypeInfo
		{
			get { return GetZPropertyInfo(nameof(WorkItemType)); }
		}

		#endregion

		#region WKI_Priority

		public override ZString WKI_Priority
		{
			get { return base.WKI_Priority; }
			set
			{
				if (base.WKI_Priority != value)
				{
					base.WKI_Priority = value;
				}
			}
		}

		public ZString PatchTo
		{
			get { return !WKI_Priority.IsEmpty ? WKI_Priority + " - " + Lookups.AllPriorities.GetDescriptionFromCode(WKI_Priority) : ""; }
		}

		public ZPropertyInfo PatchToInfo
		{
			get { return GetZPropertyInfo(nameof(PatchTo)); }
		}

		#endregion

		#region Task Properties

		public void RefreshTaskProperties()
		{
			RefreshBinding();
		}

		public bool HasShelfCheckInTasksForReleaseRing(string ringCode)
		{
			bool result = false;
			foreach (WorkItemProcessTask task in WorkflowItems)
			{
				if (task.IsCheckInTaskEvenWhenAssignedToDAT && (ReleaseRingsLookup.GetRingCodeByCheckInType(task.P9_Type) == ringCode))
				{
					result = true;
					break;
				}
			}
			return result;
		}

		public bool HasOpenedShelfCheckInTasksForReleaseRing(string ringCode)
		{
			bool result = false;
			foreach (WorkItemProcessTask task in WorkflowItems)
			{
				if (task.IsCheckInTaskEvenWhenAssignedToDAT &&
					(ReleaseRingsLookup.GetRingCodeByCheckInType(task.P9_Type) == ringCode) &&
					!(task.P9_Status == ProcessTaskStatusCodeList.Codes.Closed ||
					task.P9_Status == ProcessTaskStatusCodeList.Codes.Cancelled))
				{
					result = true;
					break;
				}
			}
			return result;
		}

		public bool HasCancelledShelfCheckInTasksForReleaseRing(string ringCode)
		{
			foreach (WorkItemProcessTask task in WorkflowItems)
			{
				if (task.IsCheckInTaskEvenWhenAssignedToDAT &&
					(ReleaseRingsLookup.GetRingCodeByCheckInType(task.P9_Type) == ringCode) &&
					task.P9_Status == ProcessTaskStatusCodeList.Codes.Cancelled)
				{
					return true;
				}
			}
			return false;
		}

		public bool HasClosedShelfCheckInTasksForReleaseRing(string ringCode)
		{
			bool result = false;
			foreach (WorkItemProcessTask task in WorkflowItems)
			{
				if (task.IsCheckInTaskEvenWhenAssignedToDAT &&
					(ReleaseRingsLookup.GetRingCodeByCheckInType(task.P9_Type) == ringCode) &&
					task.P9_Status == ProcessTaskStatusCodeList.Codes.Closed)
				{
					result = true;
					break;
				}
			}
			return result;
		}

		public WorkItemProcessTask GetClosedShelfCheckInTasksForReleaseRing(string ringCode)
		{
			WorkItemProcessTask result = null;

			foreach (WorkItemProcessTask task in WorkflowItems)
			{
				if (task.IsCheckInTaskEvenWhenAssignedToDAT &&
						(ReleaseRingsLookup.GetRingCodeByCheckInType(task.P9_Type) == ringCode) &&
						task.P9_Status == ProcessTaskStatusCodeList.Codes.Closed)
				{
					result = task;
					break;
				}
			}
			return result;
		}

		public bool HasShelfCheckInTasks
		{
			get
			{
				bool result = false;
				foreach (WorkItemProcessTask task in WorkflowItems)
				{
					if (task.IsCheckInTask)
					{
						result = true;
						break;
					}
				}
				return result;
			}
		}

		public bool HasClosedCheckInTasks
		{
			get
			{
				bool result = false;
				foreach (WorkItemProcessTask task in WorkflowItems)
				{
					if (task.IsCheckInTask && task.P9_Status == ProcessTaskStatusCodeList.Codes.Closed)
					{
						result = true;
						break;
					}
				}
				return result;
			}
		}

		public bool HasCheckInTasks
		{
			get
			{
				bool result = false;
				foreach (WorkItemProcessTask task in WorkflowItems)
				{
					if (task.IsCheckInTask || task.P9_Type == EDITaskTypes.TaskCheckin)
					{
						result = true;
						break;
					}
				}
				return result;
			}
		}

		#region Containment Auto-Enforce

		public bool AreAllCheckinPrerequisiteTasksComplete(WorkItemProcessTask checkinTask)
		{
			return GetTasksMandatoryForShelfQueue(checkinTask).All(task => task.IsClosedOrCancelled);
		}

		public bool AreCheckinPrerequisiteTasksOfAllRequiredTypes(WorkItemProcessTask checkinTask)
		{
			var tasks = GetTasksMandatoryForShelfQueue(checkinTask);
			var requiredTaskTypes = EDIDataRegistry.Instance.TasksMandatoryInShelfCreation.Value
					.Cast<ICodeDescription>().Select(cdp => cdp.Code).OrderBy(s => s).ToArray();
			return tasks.Select(task => task.P9_Type.ToString()).Distinct().SequenceEqual(requiredTaskTypes);
		}

		IEnumerable<WorkItemProcessTask> GetTasksMandatoryForShelfQueue(WorkItemProcessTask checkinTask)
		{
			var tasksInWorkflow = checkinTask != null && checkinTask.ProcessHeader != null
				? GetPrereqTasks(checkinTask.ProcessHeader).Cast<WorkItemProcessTask>()
				: WorkflowItems.Cast<WorkItemProcessTask>();
			return tasksInWorkflow.Where(task => task.IsTaskMandatoryForShelfQueue).OrderBy(t => t.P9_Type).ToArray();
		}

		static IEnumerable<ProcessTask> GetPrereqTasks(IProcessHeader workflow)
		{
			foreach (ProcessTask task in workflow.Tasks)
			{
				yield return task;
			}

			foreach (ProcessTask task in workflow.GetChildWorkflowsDownTheHierarchy().SelectMany(w => w.Tasks))
			{
				yield return task;
			}

			foreach (var header in workflow.PrerequisiteLinks.Select(l => l.HeaderFrom).Where(h => h.JobHeader == workflow.JobHeader))
			{
				foreach (var task in GetPrereqTasks(header))
				{
					yield return task;
				}
			}
		}

		public ZDateTime GetRecentReviewTaskDateTimeUTC()
		{
			var recentReviewTaskDateTime = ZDateTime.Empty;

			var recentReviewTask = WorkflowItems.Cast<WorkItemProcessTask>()
				.Where(task => task.IsTaskMandatoryForShelfQueue && task.IsClosedOrCancelled && !task.P9_ActualDate.IsEmpty)
				.OrderByDescending(task => task.P9_Sequence)
				.FirstOrDefault();

			if (recentReviewTask != null)
			{
				recentReviewTaskDateTime = recentReviewTask.P9_ActualDateForBinding.ToUtcZDateTime();
			}

			return recentReviewTaskDateTime;
		}
		#endregion

		#endregion

		#region Disposition Description

		public ZString DispositionDescription
		{
			get
			{
				var result = ZString.Empty;
				var task = CurrentOrNextTask;
				if (task == null)
				{
					if (WorkflowItems.AllTasksCancelled)
					{
						result = ProcessTaskStatusCodeList.Descriptions.Cancelled;
					}
					else if (WorkflowItems.Tasks.Count > 0)
					{
						result = ProcessTaskStatusCodeList.Descriptions.Closed;
					}
				}
				else
				{
					result = new ProcessTaskStatusCodeList().GetDescriptionFromCode(task.P9_Status) + " - " + task.P9_Description;
				}
				return result;
			}
		}

		public ZPropertyInfo DispositionDescriptionInfo
		{
			get { return GetZPropertyInfo(nameof(DispositionDescription)); }
		}

		#endregion

		#region RelatedClientCode

		public ZString RelatedClientCode
		{
			get
			{
				List<string> codes = new List<string>(RelatedItems.Count);
				foreach (IWorkItemRelatedItem item in RelatedItems)
				{
					if (!item.ClientCode.IsEmpty && !codes.Contains(item.ClientCode))
					{
						codes.Add(item.ClientCode);
					}
				}

				return string.Join(", ", codes.ToArray());
			}
		}

		public ZPropertyInfo RelatedClientCodeInfo
		{
			get { return GetZPropertyInfo(nameof(RelatedClientCode)); }
		}

		#endregion

		public ZString ProjectIDs
		{
			get { return ConcatenateRelatedProjectsProperty(p => p.WKP_ProjectNumber, ", "); }
		}

		public ZPropertyInfo ProjectIDsInfo
		{
			get { return GetZPropertyInfo(nameof(ProjectIDs)); }
		}

		public ZString ProjectManagerNames
		{
			get { return ConcatenateRelatedProjectsProperty(p => (p.ProjectManager != null) ? p.ProjectManager.GS_FullName : ZString.Empty, ", "); }
		}

		public ZPropertyInfo ProjectManagerNamesInfo
		{
			get { return GetZPropertyInfo(nameof(ProjectManagerNames)); }
		}

		public ZString ProjectManagerEmailAddresses
		{
			get { return ConcatenateRelatedProjectsProperty(p => (p.ProjectManager != null) ? p.ProjectManager.GS_EmailAddress : ZString.Empty, ";"); }
		}

		string ConcatenateRelatedProjectsProperty(GetRelatedProjectPropertyValue getValue, string separator)
		{
			List<string> list = new List<string>();
			foreach (EDIProject relatedProject in RelatedProjects)
			{
				string value = getValue(relatedProject);
				if (!string.IsNullOrEmpty(value) && !list.Contains(value))
				{
					list.Add(value);
				}
			}
			return string.Join(separator, list.ToArray());
		}

		delegate string GetRelatedProjectPropertyValue(EDIProject relatedProject);

		IEnumerable<EDIProject> RelatedProjects
		{
			get
			{
				foreach (IWorkTaskRelatedItem relatedItem in RelatedItems)
				{
					SupportIncident featureRequest = relatedItem as SupportIncident;
					if (featureRequest != null && featureRequest.RelatedProject != null)
					{
						yield return featureRequest.RelatedProject;
					}
					EDIProject project = relatedItem as EDIProject;
					if (project != null)
					{
						yield return project;
					}
				}
			}
		}

		#endregion

		#region Actions

		public override void Cancel()
		{
			var initialStatus = WKI_Status;
			base.Cancel();
			if (initialStatus != WKI_Status)
			{
				foreach (IWorkItemRelatedItem item in RelatedItems)
				{
					item.OnRelatedWorkItemClosed(this);
				}
			}
		}

		public ZDateTime JobCloseDateUtc
		{
			get
			{
				ZDateTime result = ZDateTime.Empty;

				if (IsClosedOrCancelled && JobWorkflow != null)
				{
					ZQuery query = new ZQuery(StmALogSchema.SL_Parent, JobWorkflow.PK);
					query.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.JobClose.Code);
					query.OrderBy = StmALogSchema.Constants.SL_PostedTimeUtc + OrderByClause.Descending;
					StmALog log = Factory.LoadTop1<StmALog>(query);
					if (log != null)
					{
						result = log.SL_PostedTimeUtc;
					}
				}

				return result;
			}
		}

		#endregion

		#region Email

		protected void SendEmailsCore()
		{
			SendClosedEmailIfAttachedProfessionalServicesQuote();
		}

		void SendClosedEmailIfAttachedProfessionalServicesQuote()
		{
			if (WKI_Status == ProcessTaskStatusCodeList.Codes.Closed && OriginalIM_Status != ProcessTaskStatusCodeList.Codes.Closed)
			{
				foreach (ProfessionalServicesQuote quote in RelatedItems.GetElements<ProfessionalServicesQuote>())
				{
					if (quote.CustServiceContact != null && !quote.CustServiceContact.GS_EmailAddress.IsEmpty)
					{
						HtmlEmailDef email = new HtmlEmailDef();
						email.FromDisplayName = SupportIncident.MailFromName;
						email.FromAddress = SupportIncident.MailFromAddress;
						email.ReplyTo = SupportIncident.MailFromAddress;
						email.AddRecipientForUserCommunication(quote.CustServiceContact.GS_EmailAddress);
						email.Subject = JobNumber + " completed (PS Quote " + quote.IM_IncidentNumber + ")";

						string workItemUrl = ObjectFactory.Get<IShowEditFormUrlCreator>().Create(ControllerIDs.WorkItem, PK.ToGuid());
						string quoteUrl = ObjectFactory.Get<IShowEditFormUrlCreator>().Create(ClientControllerRegistration.ProfessionalServicesQuote, quote.PK.ToGuid());
						email.Body = " <a href=\"" + workItemUrl + "\">" + WebUtility.HtmlEncode("Work Item " + WKI_WorkItemNumber + " (" + WKI_Summary + ")") + "</a>" + " has been completed and is linked to <a href=\"" + quoteUrl + "\">" + WebUtility.HtmlEncode(quote.HumanReadableName + " (" + quote.IM_Description + ")") + "</a>";

						Env.OutgoingMailManager.CreateAndSave(email);
					}
				}
			}
		}

		#endregion

		#region Workflow

		[ChildEditable(true)]
		public new WorkItemProcessTaskCollection WorkflowItems
		{
			get { return (WorkItemProcessTaskCollection)base.WorkflowItems; }
		}

		protected override ProcessManagement.Business.WorkItemProcessTaskCollection CreateWorkItemProcessTaskCollection()
		{
			return new WorkItemProcessTaskCollection(this);
		}

		#endregion

		#region Create Incidents

		public List<SupportIncident> CreateIncidentsFromRelatedIssues()
		{
			List<SupportIncident> result = new List<SupportIncident>();

			foreach (var item in RelatedItems.ToArray())
			{
				var log = item as EdiHelpErrorLog;
				if (log != null)
				{
					result.AddRange(log.CreateIncidents(false));
				}
			}

			return result;
		}

		#endregion

		#region Shelf-CheckIn staff

		public ZString CriticalityBasedOnRelatedIncidents
		{
			get
			{
				string result = string.Empty;

				if (RelatedItems.Count > 0)
				{
					var min = 0;
					foreach (IWorkItemRelatedItem item in RelatedItems)
					{
						int itemCriticality;
						if (!int.TryParse(item.Criticality.Right(1), out itemCriticality))
						{
							itemCriticality = 0;
						}

						if (itemCriticality > 0 && (itemCriticality < min || min == 0))
						{
							result = item.Criticality;
							min = itemCriticality;
						}
					}
				}

				return result;
			}
		}

		internal static ZString GetActionByShelfsetTaskType(ZString taskType)
		{
			switch (taskType)
			{
				case WorkItemProcessTask.ShelfsetTestTask:
				case WorkItemProcessTask.ExperimentalPullRequestTask:
					return EDIShelvesetInfo.ActionTypes.ShelfsetTest;

				case WorkItemProcessTask.UATBuildShelfTask:
					return EDIShelvesetInfo.ActionTypes.UATBuild;

				case WorkItemProcessTask.AspectOnlyBuildShelfTask:
					return EDIShelvesetInfo.ActionTypes.AspectOnlyBuild;

				default:
					return EDIShelvesetInfo.ActionTypes.ShelfCheckin;
			}
		}

		#endregion

		public ZInt NumberOfRelatedIncidents
		{
			get
			{
				return RelatedItems.GetElements<SupportIncident>().Count();
			}
		}

		#region IDocManagerSupport Members

		public override DocManagerInfo DocManagerInfo
		{
			get { return docManagerInfo ?? (docManagerInfo = new NewWorkItemDocManagerInfo(this, Enterprise.Core.Constants.DocManagerCodes.WorkItem)); }
		}

		NewWorkItemDocManagerInfo docManagerInfo;

		#endregion

		#region ICanDelete

		public override bool CanDelete
		{
			get { return false; }
		}

		public override MultilingualString ReasonForNotAbleToDelete
		{
			get { return (NoResString)"Work Items should be cancelled with a reason rather than deleted directly."; }
		}

		#endregion

		#region IWorkTaskRelatedItemProvider Members

		IBusinessObjectCollection IWorkTaskRelatedItemProvider.RelatedItems => RelatedItems;

		#endregion

		#region Shelf-sets

		public ZBool HasDuplicateDatSubmissions
		{
			get
			{
				var shelfsetOrCheckinTasksBeingProcessedByDat = WorkflowItems.Cast<WorkItemProcessTask>().Where(task => (task.IsShelfsetTestTask || task.IsCheckInTask) && task.P9_Status == ProcessTaskStatusCodeList.Codes.Assigned);
				var gitSubmissions = shelfsetOrCheckinTasksBeingProcessedByDat.SelectMany(s => PullRequestUrl.ParseMany(s.P9_NotesAsString)).Select(s => s.ToString());

				var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
				var foundDuplicate = gitSubmissions.Any(x => !set.Add(x));
				return foundDuplicate;
			}
		}

		public ZPropertyInfo HasDuplicateDatSubmissionsInfo
		{
			get { return GetZPropertyInfo(nameof(HasDuplicateDatSubmissions)); }
		}

		#endregion

		#region eConversation

		public bool HasEConversationBroadcastMessages()
		{
			var list = GetAllEConversationBroadcastMessages();
			return list != null && list.Any();
		}

		public List<JobConversationMessage> GetAllEConversationBroadcastMessages() => Conversation?.Messages.Where(m => m.JCM_IsBroadcast).ToList();

		#endregion
	}

	#region Task Type Constants

	// These task types have hard-coded logic. They should also be in the registry (or the logic will never be used)
	// The registry can contain additional task types, but they will have no special logic.
	public static class EDITaskTypes
	{
		public const string TaskCheckin = "CHK";
		public const string TaskActiveCheckin = "CH0";
		public const string TaskShelfTest = "SHV";
		public const string TaskActiveShelfTest = "SH0";
		public const string TaskUATBuild = "UAT";
		public const string TaskActiveUATBuild = "UA0";
		public const string TaskAspectOnlyBuild = "ASP";
		public const string TaskActiveAspectOnlyBuild = "AS0";
		public const string TaskExperimentalPullRequest = "JPR";
		public const string TaskActiveExperimentalPullRequest = "JP0";
	}

	public static class EDITaskOutcomes
	{
		public const string Success = "PSS"; // Passed
		public const string Failed = "FAI";
	}

	#endregion
}
