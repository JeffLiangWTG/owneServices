using System;
using Enterprise.BufferManagement.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.BufferManagement.GUI
{
	public partial class ProcessHeaderLinkForm : ZTemplateForm
	{
		public ProcessHeaderLinkForm()
		{
			InitializeComponent();
		}

		public ProcessHeaderLinkForm(ProcessHeaderLink businessEntity)
			: base(businessEntity)
		{
			InitializeComponent();
		}

		protected new ProcessHeaderLink BusinessEntity => (ProcessHeaderLink)base.BusinessEntity;

		#region ZForm Overrides

		protected override bool SupportsEDocs => false;
		protected override bool ShowNotesTab => false;
		protected override bool ShowAuditTab => true;

		public override string FormCaption => BusinessEntity?.HumanReadableName ?? base.FormCaption;

		#endregion

		#region Implementation

		void FromWorkflowSelectorButton_Click(object sender, EventArgs e)
		{
			var workflow = PickOneWorkflow(BusinessEntity.HeaderFrom?.JobHeader);

			if (workflow != null)
			{
				BusinessEntity.FP_FH_HeaderFrom = workflow.PK;
			}
		}

		void ToWorkflowSelectorButton_Click(object sender, EventArgs e)
		{
			var workflow = PickOneWorkflow(BusinessEntity.HeaderTo?.JobHeader);

			if (workflow != null)
			{
				BusinessEntity.FP_FH_HeaderTo = workflow.PK;
			}
		}

		ProcessHeader PickOneWorkflow(ProcessJobHeader jobHeader)
		{
			var collection = new ProcessHeaderCollection(BusinessEntity.Factory);

			if (jobHeader != null)
			{
				ProcessHeaderLookups.AddFilterDefaults(jobHeader, collection);
			}

			return BusinessObjectModulePicker.PickOneRecordFromModuleScreen<ProcessHeader>(collection, ModuleIDs.ProcessHeader);
		}

		void OpenPrereqButton_Click(object sender, EventArgs e)
		{
			OpenWorkflow(BusinessEntity.HeaderFrom);
		}

		void OpenDependentButton_Click(object sender, EventArgs e)
		{
			OpenWorkflow(BusinessEntity.HeaderTo);
		}

		void OpenWorkflow(ProcessHeader workflow)
		{
			if (workflow != null)
			{
				WorkflowParentFormFactory.ShowFormAndNavigateToWorkflowItem(workflow);
			}
			else
			{
				Globals.Message.Show(Res.GetString("97e918bc-48d2-45e6-ae06-c4a386fbac8b", "Please select a workflow."));
			}
		}

		#endregion
	}
}
