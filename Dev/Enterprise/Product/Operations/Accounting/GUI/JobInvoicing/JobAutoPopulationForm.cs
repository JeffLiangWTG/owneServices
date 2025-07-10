using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.JobInvoicing
{
	public partial class JobAutoPopulationForm : ZForm
	{
		public JobAutoPopulationForm(Job job) : base(job)
		{
			ZFormMenuStrategy.SetMenuItemVisible(this, ZFormMenuStrategy.MakeInactiveName, false);
			ZFormMenuStrategy.SetMenuItemVisible(this, ZFormMenuStrategy.MakeActiveName, false);
			PostingButtonsUserControl.SaveButton.Visible = false;
			ZFormPostingButtonsStrategy.SetupPosting(this, PostingButtonsUserControl, true);
			ParentJob = job;
		}

		ZTextBox SupplierCostReferenceTextBox;
		ZDateEdit DocReceivedDateEdit;
		internal readonly Job ParentJob;

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		protected override ContinueWithSave ValidateAndSave()
		{
			this.ValidateAll(ValidationType.Full);
			if (ParentJob.HasErrors)
			{
				ShowErrorsDialog();
				return ContinueWithSave.No;
			}
			else
			{
				Cursor.Current = Cursors.WaitCursor;
				ParentJob.PostAutoPopulation();
				Cursor.Current = Cursors.Arrow;
				return ContinueWithSave.Yes;
			}
		}

		#region IDisposable Members

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		#endregion
	}
}

