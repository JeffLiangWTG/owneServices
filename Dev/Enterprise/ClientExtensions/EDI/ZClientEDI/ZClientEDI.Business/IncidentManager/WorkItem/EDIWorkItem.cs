using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Net;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.Mail.Business;
using Enterprise.Client.EDI.Modules;
using Enterprise.MasterFiles.Business;
using Enterprise.ProcessManagement.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public class EDIWorkItem : WorkItem, IWorkTaskTreeNode
	{
		public EDIWorkItem(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		internal GenPivotCollection RelatedItemsPivot
		{
			get
			{
				if (relatedItemsPivot == null)
				{
					relatedItemsPivot = new GenPivotCollection(this, Core.Constants.GenPivotTypes.ProcessManagement);
				}

				return relatedItemsPivot;
			}
		}
		GenPivotCollection relatedItemsPivot;

		protected override ZArchitecture.Environment.INumberFountainProxy JobNumberFountain
		{
			get { return Modules.ClientNumberFountainRegistration.GetInstance().NewWorkItemNo; }
		}

		public const string EngineeringTypeCode = "ENG";

		#region OnSaving / OnSaved

		public override void OnSaved(bool saveSucceeded)
		{
			base.OnSaved(saveSucceeded);

			if (saveSucceeded)
			{
				SendNotificationEmailToClient();
			}
		}

		public override void OnSaving()
		{
			base.OnSaving();

			if (HasBeenReassigned)
			{
				var originalProductInfo = (ZString)WKI_WorkItemTypeInfo.OriginalValue;
				var originalProductAreaInfo = (ZString)WKI_WorkItemAreaInfo.OriginalValue;
				var originalModuleInfo = (ZString)WKI_ActivityTypeInfo.OriginalValue;
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				Logs.AddNew(Events.EditedARecord, ZString.Format("Reassigned from {0}/{1}/{2} to {3}/{4}/{5}", originalProductInfo, originalProductAreaInfo, originalModuleInfo, WKI_WorkItemType, WKI_WorkItemArea, WKI_ActivityType));
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			}
		}

		bool HasBeenReassigned
		{
			get
			{
				return (ZString)WKI_WorkItemTypeInfo.OriginalValue != WKI_WorkItemType
						|| (ZString)WKI_WorkItemAreaInfo.OriginalValue != WKI_WorkItemArea
						|| (ZString)WKI_ActivityTypeInfo.OriginalValue != WKI_ActivityType;
			}
		}

		#endregion

		#region Notification Email

		public CustomerServiceEmail[] DequeueClientEmails()
		{
			CustomerServiceEmail[] result;
			if (queuedClientEmails == null)
			{
				result = System.Array.Empty<CustomerServiceEmail>();
			}
			else
			{
				result = queuedClientEmails.ToArray();
				queuedClientEmails.Clear();
			}
			return result;
		}

		void SendNotificationEmailToClient()
		{
			if (WKI_WorkItemType == EngineeringTypeCode &&
				OverallTaskStatusCode == ProcessTaskStatusCodeList.Codes.Closed)
			{
				var createUser = Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, WKI_SystemCreateUser);
				CustomerServiceEmail email = new CustomerServiceEmail(this);
				email.UseCurrentUsersNameAndTitle = true;
				email.UseCurrentUsersEmailAddress = true;
				email.ToDisplayName = createUser.GS_FullName;
				email.ToEmailAddress = createUser.GS_EmailAddress;
				email.Subject = string.Format(CultureInfo.CurrentCulture, "Engineering Task {0} completed - {1}", WKI_WorkItemNumber, WKI_Summary);
				string bodyTemplate =
@"Engineering work is complete for your task <a href=""{0}"">{1}</a>

Description: 
{2}

Details:
{3}";
				string url = ObjectFactory.Get<IShowEditFormUrlCreator>().Create(ClientControllerRegistration.EngineeringTask, this.PK.ToGuid());
				var detailsAsText = ORtfTextUtil.RtfToText(WKI_Details);
				email.Body = string.Format(CultureInfo.InvariantCulture, bodyTemplate, url, WKI_WorkItemNumber, WebUtility.HtmlEncode(WKI_Summary), WebUtility.HtmlEncode(detailsAsText));

				QueuedClientEmails.Add(email);
			}
		}

		List<CustomerServiceEmail> QueuedClientEmails
		{
			get { return queuedClientEmails ?? (queuedClientEmails = new List<CustomerServiceEmail>()); }
		}
		List<CustomerServiceEmail> queuedClientEmails;

		#endregion

		#region ValidateTasks 

		protected override void ValidateTasks()
		{
			if (!IsValidationSuspended)
			{
				foreach (WorkItemProcessTask checkinTask in WorkflowItems.Tasks.Cast<WorkItemProcessTask>().Where(x => x.IsCheckInTask))
				{
					checkinTask.Validation.ValidateP9_Type();
				}
			}
		}

		#endregion

		#region DefectCausedByTask

		public override ZGuid WKI_P9_DefectCausedByTask
		{
			get { return base.WKI_P9_DefectCausedByTask; }
			set
			{
				base.WKI_P9_DefectCausedByTask = value;
				ValidateTasks();
			}
		}

		#endregion

		#region DefectFirstMissedInTask
		public override ZGuid WKI_P9_DefectFirstMissedInTask
		{
			get { return base.WKI_P9_DefectFirstMissedInTask; }
			set
			{
				base.WKI_P9_DefectFirstMissedInTask = value;
				ValidateTasks();
			}
		}
		#endregion

		#region IWorkTaskTreeNode Members

		public BusinessObjectCollection ChildrenOnlyRelatedItems
		{
			get
			{
				if (childrenOnlyRelatedItems == null)
				{
					childrenOnlyRelatedItems = new WorkItemRelatedItemCollection(this, RelatedLinkType.MasterAlwaysParent);
					childrenOnlyRelatedItems.Load();
				}
				return childrenOnlyRelatedItems;
			}
		}
		BusinessObjectCollection childrenOnlyRelatedItems;

		public BusinessObjectCollection ParentsOnlyRelatedItems
		{
			get
			{
				if (parentOnlyRelatedItems == null)
				{
					parentOnlyRelatedItems = new WorkItemRelatedItemCollection(this, RelatedLinkType.MasterAlwaysChild);
					parentOnlyRelatedItems.Load();
				}
				return parentOnlyRelatedItems;
			}
		}
		BusinessObjectCollection parentOnlyRelatedItems;

		public ZDateTime AgreedDeliveryDate => JobWorkflow?.AgreedDeliveryDateLocal ?? ZDateTime.Empty;

		public ZString CurrentTaskDescription => CurrentTask?.P9_Description ?? ZString.Empty;

		public ZString CurrentTaskCapabilityCodeDescription
		{
			get
			{
				var result = ZString.Empty;
				var task = CurrentTask;
				if (task != null && task.RequiredCapability != null)
				{
					if (!string.IsNullOrEmpty(task.RequiredCapability.G4_Code))
					{
						result = task.RequiredCapability.G4_Code + " - ";
					}

					result += task.RequiredCapability.G4_Description;
				}

				return result;
			}
		}

		public ZString CurrentTaskAssigned => AssignedToCode;

		public ZString SelectionCriterion1Code => WKI_WorkItemType;

		public ZString SelectionCriterion2Code => WKI_WorkItemArea;

		public ZString SelectionCriterion3Code => WKI_ActivityType;

		public ZString SelectionCriterion4Code => WKI_ActivitySubtype;

		public ZString SelectionCriterion5Code => WKI_Priority;

		public void AddFetchHintsForOrgAddressIfRequired()
		{
		}

		public void AddFetchHintsForOrgHeaderIfRequired()
		{
		}

		#endregion
	}
}
