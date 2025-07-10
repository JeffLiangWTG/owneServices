using System;
using System.ComponentModel;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Business
{
	public class WorkflowTransferDiagnosis : NonPersistentBusinessObject, IObsoleteValidation
	{
		internal WorkflowTransferDiagnosis()
		{
		}

		public WorkflowTransferDiagnosis(BMComponentLink link, ProcessHeader workflow)
		{
			Argument.NotNull(link, "link");
			Argument.NotNull(workflow, "workflow");

			this.link = link;
			this.workflow = workflow;
		}

		public BMComponentLink Link
		{
			get { return link; }
		}

		readonly BMComponentLink link;
		readonly ProcessHeader workflow;

		#region Properties

		#region ComponentToName

		[ResourceStringData("WorkflowTransferFailure.ComponentToName", Caption = "To Component")]
		public ZString ComponentToName => link.ComponentTo.FC_Name;

		[ResourceStringData("WorkflowTransferFailure.LinkName", Caption = "Link", FullDescription = "The name of the component link, which can be set in the Component Links section of the Buffer Management System form.")]
		public ZString LinkName => link.DisplayText;

		#endregion

		#region PassedFilterRules

		[ReadOnly(true)]
		[ResourceStringData("WorkflowTransferFailure.PassedFilterRules", Caption = "Workflow matches all filter strips", FullDescription = "Indicates whether this workflow matches the filter strips defined on the link to the selected component.")]
		public ZBool PassedFilterRules { get; private set; }

		#endregion

		#region DestinationType
		[ReadOnly(true)]
		[ResourceStringData("WorkflowTransferFailure.DestinationType", Caption = "Destination Type", FullDescription = "The type (bucket or buffer) of the destination component for this component link.")]
		public ZString DestinationType => link.ComponentTo.TypeDescription;

		#endregion

		#region FilterRuleMatchingStatus
		[ReadOnly(true)]
		[ResourceStringData("WorkflowTransferFailure.FilterRuleMatchingStatus", Caption = "Matches Filter Rules")]
		public ZString FilterRuleMatchingStatus
		{
			get
			{
				return PassedFilterRules ?
					Res.GetString("d64ea3fd-1c87-462a-83d3-97fbefde55e5", "Yes") :
					Res.GetString("e45c7217-b8dc-4e69-86be-d1f53ca1015d", "No");
			}
		}

		#endregion

		#region FailureReason
		[ReadOnly(true)]
		[MaxLength(Int32.MaxValue)]
		[ResourceStringData("WorkflowTransferFailure.FailureReason", Caption = "Buffer Release Outcome")]
		public ZString FailureReason
		{
			get { return failureReason; }
			set { SetNonPersistentPropertyValue(FailureReasonInfo, ref failureReason, value); }
		}
		ZString failureReason;

		public ZPropertyInfo FailureReasonInfo
		{
			get { return GetZPropertyInfo(nameof(FailureReason)); }
		}

		#endregion

		#endregion

		#region TryTransfer

		public void TryTransfer()
		{
			if (!link.FL_TransferRulesEnabled)
			{
				FailureReason = Res.GetString("9acd9f6f-1706-4c96-94e1-8e6ffc996d5b", "The selected component link is currently disabled.");
				return;
			}

			ZQuery filterQuery;
			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, link.Branch, link.Department))
			{
				filterQuery = RelatedModuleFiltersHelper.GetFilterQuerySafe(link.FilterRule);
			}
			filterQuery.AddToFilter(ProcessHeaderSchema.PK, workflow.PK);

			PassedFilterRules = workflow.Factory.Exists(typeof(ProcessHeader), filterQuery, mergeDbAndCacheResult: false);

			if (!PassedFilterRules)
			{
				FailureReason = Res.GetString("bd95c99d-d3a5-4275-9d4a-464c45ec3ca1", "This workflow cannot be released until the filter strip requirements are met.");
				return;
			}

			if (!link.ComponentTo.IsBuffer)
			{
				FailureReason = Res.GetString("76e81736-6c24-4b67-b1ea-6242e3f23068", "The selected destination component is not a buffer.");
			}
			else if (!link.FL_IsReleaseGateRuleApplied)
			{
				FailureReason = Res.GetString("f9b61966-9aa6-485d-b4e4-cdbb55f1417f", "The selected component link is not configured as a 'Release Gate' link, so does not evaluate resource capacity.");
			}
			else
			{
				var releaseFailure = workflow.GetReleaseFailureReasonForBuffer(link.ComponentTo);

				if (!string.IsNullOrEmpty(releaseFailure))
				{
					FailureReason = releaseFailure;
				}
			}
		}

		#endregion
	}
}
