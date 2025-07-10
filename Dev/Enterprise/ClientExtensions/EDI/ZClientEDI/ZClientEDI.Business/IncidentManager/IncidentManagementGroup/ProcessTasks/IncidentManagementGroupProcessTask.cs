using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.Modules;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public class IncidentManagementGroupProcessTask : ProcessTask
	{
		public IncidentManagementGroupProcessTask(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public override ControllerID ParentControllerID
		{
			get { return ClientControllerRegistration.IncidentManagementGroup; }
		}

		protected override Type ParentType => typeof(IncidentManagementGroup);

		public new IncidentManagementGroup Parent
		{
			get { return (IncidentManagementGroup)base.Parent; }
		}

		public override ZString P9_GS_NKAssignedStaffMember
		{
			get => base.P9_GS_NKAssignedStaffMember;
			set
			{
				base.P9_GS_NKAssignedStaffMember = value;

				if (Parent != null)
				{
					Parent.OverallAssignedToCodeInfo.RefreshBinding();
					Parent.OverallAssignedToDescriptionInfo.RefreshBinding();
					Parent.OverallAssignedToLabelTextInfo.RefreshBinding();
				}
			}
		}

		public override ZString P9_Status
		{
			get => base.P9_Status;
			set
			{
				base.P9_Status = value;

				if (Parent != null)
				{
					Parent.OverallAssignedToCodeInfo.RefreshBinding();
					Parent.OverallAssignedToDescriptionInfo.RefreshBinding();
					Parent.OverallAssignedToLabelTextInfo.RefreshBinding();
					Parent.CurrentTaskStatusInfo.RefreshBinding();
					Parent.CurrentTaskDescriptionInfo.RefreshBinding();
				}
			}
		}

		public override ZString P9_Description
		{
			get => base.P9_Description;
			set
			{
				base.P9_Description = value;

				if (Parent != null)
				{
					Parent.CurrentTaskStatusInfo.RefreshBinding();
				}
			}
		}
	}
}
