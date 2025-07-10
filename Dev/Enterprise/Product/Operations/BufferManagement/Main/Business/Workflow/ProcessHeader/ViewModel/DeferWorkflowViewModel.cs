using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.PAVE.Common.Interfaces;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.BufferManagement.Business
{
	public class DeferWorkflowViewModel : NonPersistentBusinessObject
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		public DeferWorkflowViewModel(ProcessHeader workflowToDefer, IEnumerable<ZGuid> fromComponentPKs)
			: base(workflowToDefer.Factory)
		{
			Argument.NotNull(workflowToDefer, "workflowToDefer");

			RegisterEditableChildObject(workflowToDefer);
			this.workflowToDefer = workflowToDefer;
			FromComponentPKs = fromComponentPKs;
		}

		readonly ProcessHeader workflowToDefer;

		public void Defer(string reason = "")
		{
			if (!reason.IsNullOrEmpty())
			{
				DeferralReason = reason;
			}

			var startingComponent = workflowToDefer.GetFirstBMComponent();
			if (startingComponent != null)
			{
				var workflows =
					from bizo in WorkflowsToDefer.Cast<WorkflowToDeferBusinessObject>()
					where bizo.ActionToBeTaken == WorkflowDeferalActionList.Codes.Defer
					select bizo.ProcessHeader.IsWorkflow ? new[] { bizo.ProcessHeader } : ((ProcessJobHeader)bizo.ProcessHeader).ProcessHeaders.Where(x => x.IsReleased);

				WorkflowsDeferred = workflowToDefer.IsWorkflow
					? new[] { workflowToDefer }.Concat(workflows.SelectMany(x => x)).ToArray()
					: workflows.SelectMany(x => x).ToArray();

				MaybeClearESDDefaultsFrom();

				var earliestStartDate = workflowToDefer.DoNotStartBeforeDateLocal;

				using (ApplySuspensions())
				{
					var collector = ObjectFactory.New<IPAVEUsageCollector>();
					var eventData = new List<KeyValuePair<string, string>>();

					foreach (var workflow in WorkflowsDeferred)
					{
						workflow.Defer(startingComponent, earliestStartDate, DeferralReason);

						var originalEarliestStartDate = (ZDateTime)workflow.FH_DoNotStartBeforeDateInfo.OriginalValue;
						var originalEarliestStartDateDefaultsFrom = (ZString)workflow.FH_EarliestStartDateDefaultsFromInfo.OriginalValue;

						if (workflow.FH_EarliestStartDateDefaultsFrom.IsEmpty &&
							workflow.FH_EarliestStartDateDefaultsFrom != originalEarliestStartDateDefaultsFrom &&
							earliestStartDate != originalEarliestStartDate)
						{
							eventData.Add(new KeyValuePair<string, string>("PK", workflow.PK.ToString()));
							eventData.Add(new KeyValuePair<string, string>((NoResString)"Name", workflow.FH_CompletionStatement));
							eventData.Add(new KeyValuePair<string, string>("FromVisualBoard", "YES"));
							eventData.Add(new KeyValuePair<string, string>("OriginalESDDefaultsFrom", originalEarliestStartDateDefaultsFrom));
						}
					}

					collector.ReportDeferralWithESDOrADDRemoved(eventData.ToDictionary(kv => kv.Key, kv => kv.Value));
				}

				var postreqLinksToDisconnect =
					from bizo in WorkflowsToDefer.Cast<WorkflowToDeferBusinessObject>()
					where bizo.ActionToBeTaken == WorkflowDeferalActionList.Codes.RemovePrerequisite
					select bizo.Link;

				foreach (var prereqLink in postreqLinksToDisconnect)
				{
					prereqLink.Delete();
				}
			}
		}

		void MaybeClearESDDefaultsFrom()
		{
			if (EarliestStartDateDefaultsFromFieldSpecified)
			{
				Workflow.FH_EarliestStartDateDefaultsFrom = string.Empty;
			}
		}

		public IDisposable ApplySuspensions()
		{
			var disposables = new List<IDisposable>();

			foreach (var prereq in WorkflowsDeferred)
			{
				disposables.Add(prereq.SuspendCheckingCurrentComponentSecurity());
				disposables.Add(prereq.SuspendCollectDataForDeferralWithESDOrADDRemovedReport());
			}

			return new DisposableAction(() => disposables.ForEach(d => d.Dispose()));
		}

		#region Related Business Objects

		public ProcessHeader Workflow
		{
			get { return workflowToDefer; }
		}

		public ICollection<ProcessHeader> WorkflowsDeferred
		{
			get { return workflowsDeferred ?? Array.Empty<ProcessHeader>(); }
			set { workflowsDeferred = value; }
		}

		ICollection<ProcessHeader> workflowsDeferred;

		[ChildEditable]
		public WorkflowToDeferBusinessObjectCollection WorkflowsToDefer
		{
			get
			{
				if (workflowsToDefer == null)
				{
					workflowsToDefer = Workflow.IsWorkflow
						? new WorkflowToDeferBusinessObjectCollection(Workflow)
						: new WorkflowToDeferBusinessObjectCollection((ProcessJobHeader)Workflow, FromComponentPKs);
					RegisterEditableChildObject(workflowsToDefer);
				}

				return workflowsToDefer;
			}
		}

		WorkflowToDeferBusinessObjectCollection workflowsToDefer;

		#endregion

		#region Properties

		[ResourceStringData("DeferWorkflowViewModel.DeferralReason", Caption = "Reason", FullDescription = "Nominate a reason to defer this workflow.")]
		[List("DeferralReasonsList")]
		[MaxLength(3)]
		public ZString DeferralReason
		{
			get { return deferralReason; }
			set
			{
				CheckMaximumLength(DeferralReasonInfo, value);
				deferralReason = value;

				if (!IsValidationSuspended)
				{
					Validation.ValidateDeferralReason();
				}

				DeferralReasonInfo.RefreshBinding();
			}
		}
		ZString deferralReason;

		public ZPropertyInfo DeferralReasonInfo
		{
			get { return GetZPropertyInfo(nameof(DeferralReason)); }
		}

		[ResourceStringData("DeferWorkflowViewModel.DoNotStartBeforeDate", Caption = "Defer Until", FullDescription = "Sends this workflow back to the start of the schematic until this date, which becomes the Earliest Start Date.")]
		public ZDateTime DoNotStartBeforeDate
		{
			get { return workflowToDefer.DoNotStartBeforeDateLocal; }
			set
			{
				workflowToDefer.DoNotStartBeforeDateLocal = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateDoNotStartBeforeDate();
				}

				DoNotStartBeforeDateInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo DoNotStartBeforeDateInfo
		{
			get { return GetZPropertyInfo(nameof(DoNotStartBeforeDate)); }
		}

		public ZString DoNotStartBeforeHint => Res.GetString("b62df351-1462-4976-a82e-13626e448126", "Please nominate the date and time this {0} should be deferred until:",
			Workflow.IsWorkflow ? Res.GetString("855f8e2c-1b7f-430a-87e0-554b1e9297fd", "workflow") : Res.GetString("a13d9521-c410-40b1-a206-da910c107db3", "job"));

		public ZString WorkflowsHint => Workflow.IsWorkflow
			? Res.GetString("fad2c5db-5b75-4e97-b0b2-43e949a1d033", "The workflow selected for deferral is a pre-requisite to the following workflows:")
			: Res.GetString("7eb22479-8a99-481d-a76f-5556b83157bf", "Please indicate which workflows will be deferred:");

		public ZPropertyInfo WorkflowsHintInfo => GetZPropertyInfo(nameof(WorkflowsHint), Res.GetString("b229f3b2-7fcf-4352-9bdb-4b81c46c384c", "Action"));

		IEnumerable<ZGuid> FromComponentPKs { get; }

		public bool EarliestStartDateDefaultsFromFieldSpecified => !Workflow.FH_EarliestStartDateDefaultsFrom.IsEmpty;

		#endregion

		public ReadOnlyCodeDescriptionPairList DeferralReasonsList
		{
			get { return Factory.GetCachedValue("DeferralReasonsList", () => BMSRegistry.Instance.DeferralReasons.Value); }
		}

		public DeferWorkflowViewModelValidation Validation
		{
			get { return new DeferWorkflowViewModelValidation(this); }
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();

			Validation.ValidateAll();
		}

		protected override ZString HumanReadableNameCore => Res.GetString("45a2728b-c873-49a7-987a-7b8a2340c83d", "Workflows to Defer");
	}
}
