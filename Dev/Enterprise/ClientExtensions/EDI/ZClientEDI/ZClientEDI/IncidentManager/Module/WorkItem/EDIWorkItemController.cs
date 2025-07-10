using System;
using CargoWise.EntityFramework;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.IncidentManager.GUI;
using Enterprise.MasterFiles.Business;
using Enterprise.ProcessManagement.Module;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.IncidentManager.Module
{
	public class EDIWorkItemController : WorkItemController
	{
		protected override IZForm GetForm(IBusiness businessEntity) => new NewWorkItemForm((NewWorkItem)businessEntity);

		public override bool MakeUrlsOnlyOpenableForCurrentCompany => false;

		protected override IBusiness GetNewBusinessEntityInLocalFactory()
		{
			var workItem = (NewWorkItem)base.GetNewBusinessEntityInLocalFactory();
			var lastAddedWorkItem = GetLastAddedWorkItemByCurrentUser();
			if (lastAddedWorkItem != null)
			{
				workItem.WKI_WorkItemType = lastAddedWorkItem.WKI_WorkItemType;
				workItem.WKI_WorkItemArea = lastAddedWorkItem.WKI_WorkItemArea;
				workItem.WKI_ActivityType = lastAddedWorkItem.WKI_ActivityType;
			}

			return workItem;
		}

		NewWorkItem GetLastAddedWorkItemByCurrentUser()
		{
			var filter = new ZQuery(WorkItemSchema.WKI_SystemCreateUser, GlbStaff.CurrentUser.GS_Code);
			filter.OrderBy = WorkItemSchema.Constants.WKI_SystemCreateTimeUtc + OrderByClause.Descending;

			return Factory.LoadTop1<NewWorkItem>(filter);
		}

		internal IBusiness GetNewAndPopulateFromSource(IWorkItemSource workItemSource)
		{
			var workItem = (NewWorkItem)GetNewBusinessEntityInFactory(workItemSource.Factory);
			workItemSource.PopulateWorkItem(workItem);

			return workItem;
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(NewWorkItem); }
		}

		#region Delete Forms

		public override IZForm ShowDeleteForm(BusinessObject sourceEntity)
		{
			IZForm result = null;

			if (sourceEntity.CanDelete)
			{
				result = base.ShowDeleteForm(sourceEntity);
			}
			else
			{
				Globals.Message.ShowError(sourceEntity.ReasonForNotAbleToDelete, "Cannot Delete");
			}

			return result;
		}

		#endregion
	}
}
