using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.BufferManagement.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.BufferManagement.GUI
{
	public partial class WorkflowTransferDiagnosisForm : ZChildForm
	{
		public static void Show(ProcessHeader workflow)
		{
			if (!workflow.IsInDatabase)
			{
				Globals.Message.ShowError(Res.GetString("f93e342a-edb5-44f8-b5d9-0bfa6bd5de72", "Please save the form first."));
			}
			else
			{
				var factory = new BusinessObjectFactory { NameForDebugging = "WorkflowTransferFailureForm" };
				var loadedWorkflow = factory.Load<ProcessHeader>(workflow.PK);

				if (workflow.IsDeleted || loadedWorkflow == null)
				{
					Globals.Message.ShowError(Res.GetString("feaad20f-5d26-45bd-a208-fd8eddc4d82b", "This workflow has been deleted."));
				}
				else
				{
					var viewModel = new WorkflowTransferDiagnosisViewModel(loadedWorkflow);
					ZFormModaliser.ShowDialogAndDispose(new WorkflowTransferDiagnosisForm(viewModel));
				}
			}
		}

		public WorkflowTransferDiagnosisForm(WorkflowTransferDiagnosisViewModel viewModel)
			: base(viewModel)
		{
			InitializeComponent();
		}

		public new WorkflowTransferDiagnosisViewModel DataSource => (WorkflowTransferDiagnosisViewModel)base.DataSource;

		#region ZForm Overrides

		public override string FormCaption
		{
			get { return Res.GetString("c46a0963-90ee-4d48-a279-efd819a83983", "Transfer Diagnosis: {0}", DataSource.Workflow.Description); }
		}

		public override string FormVerb
		{
			get { return string.Empty; }
		}

		#endregion

		#region For Test
#if DEBUG

		public ZGrid ToComponentGrid_ForTest
		{
			get { return (ZGrid)Controls.Find("TransferFailureGrid", searchAllChildren: true)[0]; }
		}

		public int StripControlsCount_ForTest
		{
			get { return TransferDiagnosisControl.FindAll<ZFilterStrip>().Count(); }
		}

#endif
		#endregion
	}
}
