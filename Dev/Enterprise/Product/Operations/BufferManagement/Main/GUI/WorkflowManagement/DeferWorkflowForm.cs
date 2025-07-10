using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.Async;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.BufferManagement.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.BufferManagement.GUI
{
	public partial class DeferWorkflowForm : ZChildForm
	{
		public static DeferWorkflowResult ShowDeferForm(ProcessHeader workflow, IEnumerable<ZGuid> fromComponentPKs)
		{
			var result = DialogResult.None;

			if (!Env.Security.BMSystemDeferWorkflow.IsAllowed)
			{
				Env.Security.BMSystemDeferWorkflow.ShowError();
			}
			else if (workflow != null)
			{
				if (workflow.IsDeleted)
				{
					Globals.Message.ShowError(Res.GetString("7de4d36f-f327-415e-8bae-d2217be9910a", "Workflow has been deleted."));
				}
				else if (!workflow.IsInDatabase)
				{
					Globals.Message.ShowError(Res.GetString("72a759f5-5803-4802-b7c9-be3d51168d2b", "Workflow must be saved first."));
				}
				else
				{
					var loadedWorkflow = new BusinessObjectFactory { NameForDebugging = "DeferWorkflowForm.ShowDeferForm" }.Load<ProcessHeader>(workflow.PK);
					if (loadedWorkflow != null)
					{
						var viewModel = new DeferWorkflowViewModel(loadedWorkflow, fromComponentPKs);
						result = ZFormModaliser.ShowDialogAndDispose(new DeferWorkflowForm(viewModel));

						return new DeferWorkflowResult(result, viewModel.WorkflowsDeferred);
					}
				}
			}

			return new DeferWorkflowResult(result, Array.Empty<ProcessHeader>());
		}

#if DEBUG

		public DeferWorkflowForm()
		{
			InitializeComponent();
		}

		public
#endif
		DeferWorkflowForm(DeferWorkflowViewModel dataSource)
			: base(dataSource)
		{
			InitializeComponent();

#if DEBUG
			if (Globals.IsTest)
			{
				if (dataSource.DeferralReason.IsEmpty)
				{
					dataSource.DeferralReason = WorkflowDeferralReasonsList.Codes.PrioritiesChanged;
				}
			}
#endif

			if (dataSource.WorkflowsToDefer.Count == 0)
			{
				WorkflowsPanel.Visible = false;
				ControlDpiScalingHelper.SetHeight(this, Height - WorkflowsPanel.Height, false);
			}
			else
			{
				WorkflowsGrid.MouseDoubleClick += PrerequisitesGrid_MouseDoubleClick;

				if (!dataSource.Workflow.IsWorkflow)
				{
					CustomiseControlsForJobWithMultipleWorkflows();
				}
			}
		}

		void CustomiseControlsForJobWithMultipleWorkflows()
		{
			WorkflowHintLabel.Visible = false;
			WorkflowLabel.Visible = false;
			DependencyRemovalHintLabel.Visible = false;
			ControlDpiScalingHelper.SetHeight(ref WorkflowsGrid, WorkflowsGrid.Height + DependencyRemovalHintLabel.Height, false);

			var topForLabels = (WorkflowDetailsPanel.Height / 2) - (JobHintLabel.Height / 2);
			ControlDpiScalingHelper.SetTop(ref JobHintLabel, topForLabels, false);
			ControlDpiScalingHelper.SetTop(ref JobNumberLabel, topForLabels, false);
			ControlDpiScalingHelper.SetTop(ref JobDescriptionLabel, topForLabels, false);
		}

		public new DeferWorkflowViewModel DataSource
		{
			get { return (DeferWorkflowViewModel)base.DataSource; }
		}

		#region ZForm Overrides

		public override string FormCaption
		{
			get { return Res.GetString("da7fdf79-e280-40da-965c-9fab8fdfb0d8", "Defer {0}", DataSource.Workflow.FH_CompletionStatement); }
		}

		public override string FormVerb
		{
			get { return string.Empty; }
		}

		protected override void OnShown(EventArgs e)
		{
#if DEBUG
			if (Globals.IsTest)
			{
				Defer();
			}
#endif

			base.OnShown(e);
		}

		#endregion

		#region Event Handlers

		void CancelDeferButton_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
		}

		void DeferButton_Click(object sender, EventArgs e)
		{
			Defer();
		}

		void PrerequisitesGrid_MouseDoubleClick(object sender, MouseEventArgs e)
		{
			var row = WorkflowsGrid.GetRow(e);
			if (row >= 0 && row < WorkflowsGrid.ListManager.Count && WorkflowsGrid.ListManager.Position != -1)
			{
				var selectedElement = WorkflowsGrid.List[row] as WorkflowToDeferBusinessObject;
				if (selectedElement != null)
				{
					MainThreadRunner.RunOnMainThread(() => WorkflowParentFormFactory.ShowFormAndNavigateToWorkflowItem(selectedElement.ProcessHeader));
				}
			}
		}

		#endregion

		#region Defer

		void Defer()
		{
			ValidateAll(ValidationType.Full);

			if (!DataSource.HasErrors)
			{
				if (DataSource.EarliestStartDateDefaultsFromFieldSpecified)
				{
					var datesDefaultsFromList = DataSource.Workflow.Lookups.DatesDefaultsFromList;
					var description = datesDefaultsFromList.GetDescriptionFromCode(DataSource.Workflow.FH_EarliestStartDateDefaultsFrom);
					var message = Res.GetString("ef57d03c-ad1d-4b46-a8e9-04466cdddebd", "The workflow that you are attempting to defer is currently set to automatically update its deferral date from the {0} field. Click Yes to defer and stop future automatic updates. Click No to cancel the deferral and return to the visual board.", description);
					var caption = Res.GetString("9cbfbf9c-e3c9-468a-94a4-70d2cd89fc34", "Defer");
					var result = Globals.Message.Show(message, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Warning, DialogResult.No);

					if (result == DialogResult.Yes)
					{
						DeferCore();
					}
					else
					{
						DialogResult = DialogResult.OK;
						return;
					}
				}
				else
				{
					DeferCore();
				}
			}
			else
			{
				ShowErrorsDialog();
				DialogResult = DialogResult.None;
			}
		}

		void DeferCore()
		{
			DataSource.Defer();

			using (DataSource.ApplySuspensions())
			{
				if (this.FireSaveButton() == ContinueWithSave.Yes)
				{
					DialogResult = DialogResult.OK;
					return;
				}
			}
		}

		#endregion
	}
}
