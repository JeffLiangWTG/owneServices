using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ProcessManagement.Integration;
using Res = ZClientEDI.Business.Res;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public class NewWorkItemValidation : ProcessManagement.Business.WorkItemActualValidation
	{
		public NewWorkItemValidation(NewWorkItem parent)
			: base(parent)
		{
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateHasDuplicateDatSubmissions();
		}

		protected override void CheckWKI_ActivitySubtype()
		{
			base.CheckWKI_ActivitySubtype();

			if ((!Parent.IsInDatabase || Parent.WKI_ActivitySubtypeInfo.HasChanges) && !Parent.WKI_ActivitySubtypeInfo.HasErrors())
			{
				CheckCapitalizedTypes(EDIDataRegistry.Instance.ActivitySubtypeAssignments, Parent.WKI_ActivitySubtypeInfo);
			}

			if (Parent.WKI_ActivitySubtype == NewWorkItemLookups.WorkItemTypeConstants.IssueFix && (Parent.RelatedItems == null || Parent.RelatedItems.All(e => ((IWorkTaskRelatedItem)e).Type != EDIWorkTaskRelatedItemTypes.Issue)))
			{
				Parent.WKI_ActivitySubtypeInfo.AddError(Res.GetString("338e8748-1079-44dd-b376-f8267451ac64", "This field cannot be set to issue type without a related Issue."));
			}
		}

		protected override void CheckWKI_Details()
		{
			base.CheckWKI_Details();
			CheckDetailsForPersonalSharePointLink();
		}

		#region HasMultipleShelfSetTasksAssigned

		void ValidateHasDuplicateDatSubmissions()
		{
			ValidateCalculatedProperty(Parent.HasDuplicateDatSubmissionsInfo);
		}

		protected void CheckHasDuplicateDatSubmissions()
		{
			if (Parent.HasDuplicateDatSubmissions)
			{
				Parent.HasDuplicateDatSubmissionsInfo.AddError(Res.GetString("0c6207af-43f8-47ef-ab1c-75667bea4102", "Multiple Check-In/Shelf Test tasks can be queued simultaneously. However each Git pull request must be unique. Check the task notes of any Check-In or Shelf Test tasks in this work item with Status set to '{0}'. This can occur when a task or workflow is cloned but the task notes have not been cleared.", ProcessTaskStatusCodeList.Codes.Assigned));
			}
		}

		#endregion

		void CheckDetailsForPersonalSharePointLink()
		{
			var parent = Parent;
			var itemDetailsInfo = Parent.WKI_DetailsInfo;
			var detailsValue = parent.WKI_Details.ToUTF8();
			if (HasPersonalSharePointLinkInDetails(detailsValue))
			{
				var personalSharePointLinkWarningMessage = Res.GetString("600ca529-2213-4269-ae38-4ee557d22d73",
					englishText: "Description may contain a link to a personal SharePoint / OneDrive space. Please ensure all links point to official {0} SharePoint sites.", EDIConstants.ClientDisplayName);
				itemDetailsInfo.AddWarning(personalSharePointLinkWarningMessage);
			}
		}

		bool HasPersonalSharePointLinkInDetails(ZString detailsValue) => detailsValue.Contains("https://wisetechglobal-my.sharepoint.com/", StringComparison.OrdinalIgnoreCase);

		void CheckCapitalizedTypes(ActivitySubtypeAssignmentsRegistryItem registryItem, ZPropertyInfo info)
		{
			var assignmentCollection = registryItem.Value;
			if (!info.Value.IsEmpty && assignmentCollection.Count > 0 && assignmentCollection.FirstOrDefault(a => ((ActivitySubtypeAssignment)a).ActivitySubtype == info.Value.ToString()) == null)
			{
				info.AddError(Res.GetString("b6535969-048a-4b56-b7de-d4a1cbde7974", "Code must be entered in the Capitalized Development Change Types registry"));
			}
		}

		new NewWorkItem Parent => (NewWorkItem)base.Parent;
	}
}

