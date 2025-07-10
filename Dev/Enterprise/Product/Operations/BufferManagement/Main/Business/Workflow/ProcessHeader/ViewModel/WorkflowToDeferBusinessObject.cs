using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.BufferManagement.Business
{
	public class WorkflowToDeferBusinessObject : NonPersistentBusinessObject, IObsoleteValidation
	{
		public WorkflowToDeferBusinessObject(ProcessHeader processHeader, ProcessHeaderLink link, bool isDeferringJob)
			: base(processHeader.Factory)
		{
			this.processHeader = processHeader;
			this.link = link;
			this.isDeferringJob = isDeferringJob;
			ActionToBeTaken = WorkflowDeferalActionList.Codes.None;
		}

		readonly ProcessHeader processHeader;
		readonly ProcessHeaderLink link;
		readonly bool isDeferringJob;

		public ProcessHeader ProcessHeader
		{
			get { return processHeader; }
		}

		public ProcessHeaderLink Link
		{
			get { return link; }
		}

		#region Properties

		#region WorkflowName

		[ResourceStringData("PrerequisiteToDeferBusinessObject.WorkflowName", Caption = "Workflow")]
		public ZString WorkflowName
		{
			get { return ProcessHeader.Code; }
		}

		#endregion

		#region ActionToBeTaken

		[MaxLength(3)]
		[List("PrerequisiteDeferalActions")]
		[ResourceStringData("PrerequisiteToDeferBusinessObject.ActionToBeTaken", Caption = "Action")]
		public ZString ActionToBeTaken
		{
			get { return actionToBeTaken; }
			set { SetNonPersistentPropertyValue(ActionToBeTakenInfo, ref actionToBeTaken, value); }
		}
		ZString actionToBeTaken;

		public ZPropertyInfo ActionToBeTakenInfo
		{
			get { return GetZPropertyInfo(nameof(ActionToBeTaken)); }
		}

		public CodeDescriptionPairList PrerequisiteDeferalActions
		{
			get
			{
				if (isDeferringJob)
				{
					return Factory.GetCachedValue("WorkflowDeferalActionListForJob", () =>
					{
						var list = new WorkflowDeferalActionList();
						list.RemoveCode(WorkflowDeferalActionList.Codes.RemovePrerequisite);
						return list;
					});
				}

				return Factory.GetCachedValue<WorkflowDeferalActionList>();
			}
		}

		#endregion

		#region ActionDescription

		[ResourceStringData("PrerequisiteToDeferBusinessObject.ActionDescription", Caption = "Action Description")]
		public ZString ActionDescription
		{
			get { return PrerequisiteDeferalActions.GetDescriptionFromCode(ActionToBeTaken); }
		}

		#endregion

		#endregion
	}
}
