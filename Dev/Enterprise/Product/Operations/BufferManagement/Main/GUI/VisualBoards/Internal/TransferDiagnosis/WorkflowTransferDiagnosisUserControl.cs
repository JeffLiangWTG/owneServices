using System;
using CargoWise.Async;
using Enterprise.BufferManagement.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.BufferManagement.GUI
{
	public partial class WorkflowTransferDiagnosisUserControl : ZUserControl
	{
		public WorkflowTransferDiagnosisUserControl()
		{
			InitializeComponent();
			TransferFailureGrid.FontDeciding += new EventHandler<ZArchitecture.FontDecidingEventArgs>(TransferFailureGrid_FontDeciding);
		}

		public new WorkflowTransferDiagnosisViewModel DataSource => (WorkflowTransferDiagnosisViewModel)base.DataSource;

		void OpenJobButton_Click(object sender, EventArgs e)
		{
			this.TransferFailureGrid.ListManager.GetCurrent();
			MainThreadRunner.RunOnMainThread(() => WorkflowParentFormFactory.ShowFormAndNavigateToWorkflowItem(DataSource.Workflow));
		}

		void TransferFailureGrid_FontDeciding(object sender, ZArchitecture.FontDecidingEventArgs e)
		{
			var diagnosis = (WorkflowTransferDiagnosis)e.ObjectAtRow;

			if (diagnosis.PassedFilterRules)
			{
				e.Font = new System.Drawing.Font(e.OriginalFont, System.Drawing.FontStyle.Bold);
			}
		}
	}
}
