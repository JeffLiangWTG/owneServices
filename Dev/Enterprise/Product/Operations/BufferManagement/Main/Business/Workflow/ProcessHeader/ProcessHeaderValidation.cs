using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.WorkflowManager;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Business
{
	public class ProcessHeaderValidation : AutoProcessHeaderValidation, IProcessHeaderValidation
	{
		public ProcessHeaderValidation(AutoProcessHeader parent)
			: base(parent)
		{
		}

		new ProcessHeader Parent
		{
			get { return (ProcessHeader)base.Parent; }
		}

		#region ValidateLoops

		public void ValidateLoops()
		{
			foreach (var link in Parent.LinksFromOthersToMe_ForBinding) // Using the links collection is important so it becomes registered as child editable and therefore child notifications are present on the parent.
			{
				using (link.ForceLoopValidation())
				{
					link.Validation.ValidateFP_FH_HeaderFrom();
				}
			}
		}

		#endregion

		#region FH_CompletionStatement

		protected override void CheckFH_CompletionStatement()
		{
			base.CheckFH_CompletionStatement();
			MandatoryValidation.CheckEntered(Parent.FH_CompletionStatementInfo);
		}

		#endregion

		#region FH_FC_CurrentComponent

		protected override void CheckFH_FC_CurrentComponent()
		{
			base.CheckFH_FC_CurrentComponent();

			if (Parent.FH_ParentId.IsEmpty)
			{
				MandatoryValidation.CheckNotEntered(Parent.FH_FC_CurrentComponentInfo);
			}
			else if (Parent.FH_P0_Template.IsEmpty && (!Parent.IsInDatabase || Parent.FH_FC_CurrentComponentInfo.HasChanges))
			{
				var firstComponent = Parent.GetFirstBMComponent();
				if (firstComponent != null)
				{
					MandatoryValidation.CheckEntered(Parent.FH_FC_CurrentComponentInfo);

					var isAllowed = Parent.IsCurrentComponentSecurityValidationSuspended || Env.Security.WorkflowTasksCurrentBufferManagementComponent.IsAllowed;
					var valueIsUnchanged = Parent.IsInDatabase && Parent.FH_FC_CurrentComponent == (ZGuid)Parent.FH_FC_CurrentComponentInfo.OriginalValue;
					var isFirstComponent = firstComponent.PK == Parent.FH_FC_CurrentComponent;
					var wasAlreadyBlank = !Parent.IsInDatabase || Parent.FH_FC_CurrentComponentInfo.OriginalValue.IsEmpty;

					var isChangeValid = isAllowed || valueIsUnchanged || (wasAlreadyBlank && isFirstComponent) || IsAllowedQualityIterationComponentChange();

					if (!isChangeValid)
					{
						Parent.FH_FC_CurrentComponentInfo.AddError(Res.GetString("0d79470b-8304-46f7-af87-c672ffdff3dc", "You do not have security rights to change Current Component."));
					}
				}
			}
		}

		bool IsAllowedQualityIterationComponentChange()
		{
			if (!Parent.IsInDatabase)
			{
				var workflowParent = Parent.WorkflowParent;
				return workflowParent != null && workflowParent.FH_FC_CurrentComponent == Parent.FH_FC_CurrentComponent;
			}

			return false;
		}

		#endregion

		#region FH_DateAcceptability

		protected override void CheckFH_DateAcceptability()
		{
			base.CheckFH_DateAcceptability();

			ListValidation.ErrorIfInvalidCode(Parent.FH_DateAcceptabilityInfo);

			if (!Parent.FH_DateAcceptability.IsEmpty && !Parent.IsTemplate)
			{
				if (Parent.FH_AgreedDeliveryDate.IsEmpty)
				{
					var jobAgreedDeliveryDate = Parent.JobHeader?.FH_AgreedDeliveryDate ?? ZDateTime.Empty;

					if (!jobAgreedDeliveryDate.IsEmpty)
					{
						Parent.FH_DateAcceptabilityInfo.AddWarning(Res.GetString("5cdd5be1-47e7-4c1f-8d64-f8c44c2ba175", "This Date Acceptability will be used in conjunction with the Agreed Delivery Date of the job, which is {0}.", jobAgreedDeliveryDate.ToLongTimeString()));
					}
					else
					{
						Parent.FH_DateAcceptabilityInfo.AddWarning(Res.GetString("bed954a5-22e1-4c48-b951-a4b75d4af6b5", "Specifying a Date Acceptability value may be redundant without an Agreed Delivery Date on this workflow or the job-level workflow."));
					}
				}
			}
		}

		#endregion

		#region FH_DeadlineType

		protected override void CheckFH_DeadlineType()
		{
			base.CheckFH_DeadlineType();

			ListValidation.ErrorIfInvalidCode(Parent.FH_DeadlineTypeInfo);
		}

		#endregion

		#region FH_DoNotStartBeforeDate

		protected override void CheckFH_DoNotStartBeforeDate()
		{
			base.CheckFH_DoNotStartBeforeDate();

			if (!Parent.FH_DoNotStartBeforeDate.IsEmpty && !Parent.FH_EarliestStartDateDefaultsFrom.IsEmpty)
			{
				var description = Parent.Lookups.DatesDefaultsFromList.GetDescriptionFromCode(Parent.FH_EarliestStartDateDefaultsFrom);
				Parent.FH_DoNotStartBeforeDateInfo.AddWarning(Res.GetString("dbc57f93-f2a7-4ef6-88f1-88b6d4ccff2c", "This field is currently configured to match the {0} field. Your changes will be automatically discarded on Save. In order to ensure your changes are not discarded, please remove the 'ESD Defaults From' value from this workflow.", description));
			}
		}
		#endregion

		#region FH_AgreedDeliveryDate

		protected override void CheckFH_AgreedDeliveryDate()
		{
			base.CheckFH_AgreedDeliveryDate();

			if (!Parent.FH_AgreedDeliveryDate.IsEmpty)
			{
				if (!Parent.FH_DateAcceptability.IsEmpty && Parent.FH_AgreedDeliveryDate < ZDateTime.UtcNow)
				{
					switch (Parent.FH_DateAcceptability)
					{
						case DateAcceptabilityList.Codes.ExtendedStartSharpFinish:
						case DateAcceptabilityList.Codes.GraduatedStartSharpFinish:
						case DateAcceptabilityList.Codes.SharpStartSharpFinish:
							Parent.FH_AgreedDeliveryDateInfo.AddWarning(Res.GetString("fb898d3e-a642-4aa7-8825-9111938b7df7", "This workflow was due to be completed in the past, and the Date Acceptability indicates this is a 'hard' due date."));
							break;
					}
				}

				if (!Parent.FH_AgreedDeliveryDateDefaultsFrom.IsEmpty)
				{
					var description = Parent.Lookups.DatesDefaultsFromList.GetDescriptionFromCode(Parent.FH_AgreedDeliveryDateDefaultsFrom);
					Parent.FH_AgreedDeliveryDateInfo.AddWarning(Res.GetString("6596342e-de0d-494a-b1eb-475c1c85c283", "This field is currently configured to match the {0} field. Your changes will be automatically discarded on Save. In order to ensure your changes are not discarded, please remove the 'ADD Defaults From' value from this workflow.", description));
				}
			}
		}

		#endregion

		#region FH_GG_ReleaseGroup

		protected override void CheckFH_GG_ReleaseGroup()
		{
			base.CheckFH_GG_ReleaseGroup();

			if (!Parent.IsDeleted)
			{
				CheckIfReleaseGroupEntered();
				CheckReleaseGroupHasMembers();
				CheckReleaseGroupAllowsTaskToBeAssignedToResource();
			}
		}

		void CheckIfReleaseGroupEntered()
		{
			if (Parent.FH_ParentId.IsEmpty && Parent.FH_P0_Template.IsEmpty)
			{
				MandatoryValidation.CheckNotEntered(Parent.FH_GG_ReleaseGroupInfo);
			}
			else
			{
				var system = Parent.BMSystem;
				if (system != null && Parent.Template == null && system.ReleaseGroups.Any())
				{
					if (BMSRegistry.Instance.RequireReleaseGroup.Value)
					{
						MandatoryValidation.CheckEntered(Parent.FH_GG_ReleaseGroupInfo);
					}
					else
					{
						MandatoryValidation.WarnIfNotEntered(Parent.FH_GG_ReleaseGroupInfo);
					}

					var group = Parent.ReleaseGroup;
					if (group != null && !group.GG_IsActive && Parent.Tasks.Any(t => t.P9_Status != ProcessTaskStatusCodeList.Codes.Cancelled && t.P9_Status != ProcessTaskStatusCodeList.Codes.Closed))
					{
						Parent.FH_GG_ReleaseGroupInfo.AddError(Res.GetString("fa36720d-19e5-4c2c-9395-dc244223392d", "This Release Group is inactive."));
					}
				}
			}
		}

		protected void CheckReleaseGroupHasMembers()
		{
			if (!Parent.FH_GG_ReleaseGroup.IsEmpty)
			{
				var hasTasksRequiringThisCheck = Parent.Tasks.Any(t =>
					t.P9_GS_NKAssignedStaffMember.IsEmpty
					&& t.P9_GG_AssignedGroup.IsEmpty
					&& (t.P9_G4_RequiredCapability.IsEmpty || ProcessValidationHelper.IsTaskAssignedToCapabilityWithGroupScope(t)));

				if (hasTasksRequiringThisCheck && ProcessValidationHelper.IsGroupEmpty_WithoutLoadingGlbStaffRecords(Parent.Factory, Parent.FH_GG_ReleaseGroup))
				{
					if (WorkflowDataRegistry.Instance.RequireCapabilityTasksToBeAbleToBeAssignedToResources.Value)
					{
						Parent.FH_GG_ReleaseGroupInfo.AddError(ReleaseGroupHasNoMembersMessage);
					}
					else
					{
						Parent.FH_GG_ReleaseGroupInfo.AddWarning(ReleaseGroupHasNoMembersMessage);
					}
				}
			}
		}

		static string ReleaseGroupHasNoMembersMessage => Res.GetString("C3F2999E-77F2-4250-88D3-B8D018DEF349", "The assigned release group has no members.");

		void CheckReleaseGroupAllowsTaskToBeAssignedToResource()
		{
			if (!Parent.FH_GG_ReleaseGroup.IsEmpty
				&& Parent.Tasks.Any(t =>
					t.P9_GS_NKAssignedStaffMember.IsEmpty
					&& t.P9_GG_AssignedGroup.IsEmpty
					&& ProcessValidationHelper.DoesWorkflowReleaseGroupDisallowTaskToBeAutoAssignedToResource(t)))
			{
				if (WorkflowDataRegistry.Instance.RequireCapabilityTasksToBeAbleToBeAssignedToResources.Value)
				{
					Parent.FH_GG_ReleaseGroupInfo.AddError(ReleaseGroupDisallowsTaskToBeAssignedToResourceMessage);
				}
				else
				{
					Parent.FH_GG_ReleaseGroupInfo.AddWarning(ReleaseGroupDisallowsTaskToBeAssignedToResourceMessage);
				}
			}
		}

		static string ReleaseGroupDisallowsTaskToBeAssignedToResourceMessage => Res.GetString("91D026EF-78FE-4228-A6B9-59EDD7A044E5", "The intersection of the workflow release group and task capabilities for some of the tasks has no resources in it.");

		#endregion

		#region CurrentComponentSystemPK

		public void ValidateCurrentComponentSystemPK()
		{
			ValidateCalculatedProperty(Parent.CurrentComponentSystemPKInfo);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Needed for successful ProcessHeaderValidationTest")]
		void CheckCurrentComponentSystemPK()
		{
			MandatoryValidation.CheckEntered(Parent.CurrentComponentSystemPKInfo);
			TypeValidation.CheckValidGuid(Parent.CurrentComponentSystemPKInfo);
		}

		#endregion

		#region IterationReason

		public void ValidateIterationReason()
		{
			ValidateCalculatedProperty(Parent.IterationReasonInfo);
		}

		protected void CheckIterationReason()
		{
			if (Parent.HasValidIterationLink())
			{
				if (!Parent.IterationReason.IsEmpty)
				{
					ListValidation.ErrorIfInvalidCode(Parent.IterationReasonInfo);
				}

				if (IterationReasonValidationType != IterationReasonValidationList.Codes.None)
				{
					if (IterationReasonValidationType == IterationReasonValidationList.Codes.Warning)
					{
						MandatoryValidation.WarnIfNotEntered(Parent.IterationReasonInfo);
					}
					else
					{
						MandatoryValidation.CheckEntered(Parent.IterationReasonInfo);
					}
				}
			}
		}

		public ZString IterationReasonValidationType
		{
			get
			{
				if (iterationReasonValidation.IsEmpty)
				{
					if (Parent != null)
					{
						iterationReasonValidation = WorkflowDataRegistry.Instance.IterationReasons.Value.GetIterationReasonValidationFromWorkflowCode(Parent.Parent.WorkflowType);
					}
					else
					{
						iterationReasonValidation = IterationReasonValidationList.Codes.Error;
					}
				}
				return iterationReasonValidation;
			}
		}

		ZString iterationReasonValidation;

		#endregion

		#region FH_AgreedDeliveryDateDefaultsFrom

		protected override void CheckFH_AgreedDeliveryDateDefaultsFrom()
		{
			base.CheckFH_AgreedDeliveryDateDefaultsFrom();
			ListValidation.ErrorIfInvalidCode(Parent.FH_AgreedDeliveryDateDefaultsFromInfo);
		}

		#endregion

		#region FH_EarliestStartDateDefaultsFrom

		protected override void CheckFH_EarliestStartDateDefaultsFrom()
		{
			base.CheckFH_EarliestStartDateDefaultsFrom();
			ListValidation.ErrorIfInvalidCode(Parent.FH_EarliestStartDateDefaultsFromInfo);
		}

		#endregion

		#region FH_Category

		protected override void CheckFH_Category()
		{
			base.CheckFH_Category();
			MandatoryValidation.CheckEntered(Parent.FH_CategoryInfo);
			CheckFH_CategoryCore();
		}

		protected virtual void CheckFH_CategoryCore()
		{
			if (Parent.FH_Category != BMConstants.JobLevelWorkflowCategoryCode)
			{
				ListValidation.ErrorIfInvalidCode(Parent.FH_CategoryInfo);
			}
			else
			{
				Parent.FH_CategoryInfo.AddError(Res.GetString("d279b4f7-e24d-4d3d-aa28-7e26ee313030", "The {0} category is reserved for job level workflows only.", BMConstants.JobLevelWorkflowCategoryCode));
			}
		}

		#endregion

		#region FH_MilestoneCompletionPivotKey

		protected override void CheckFH_MilestoneCompletionPivotKey()
		{
			base.CheckFH_MilestoneCompletionPivotKey();

			if (!Parent.FH_MilestoneCompletionPivotKey.IsEmpty)
			{
				ListValidation.WarnIfInvalidCode(Parent.FH_MilestoneCompletionPivotKeyInfo, ResString.GetMultilingualString("ee55c8c7-110d-4712-aa6a-2420054474c1", "Does not match any milestone."));
			}
		}

		#endregion

		protected override bool ShouldValidateFKToCancelledRecord(ZPropertyInfo info)
		{
			return info.Name != ProcessHeaderSchema.Constants.FH_P0_Template;
		}
	}
}
