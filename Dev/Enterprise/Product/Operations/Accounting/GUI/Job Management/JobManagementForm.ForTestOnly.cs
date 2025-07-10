#if DEBUG

using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.JobManagement
{
	public partial class JobManagementForm
	{
		public void ShowOperationsForm_ForTestOnly(Business.JobInvoicing.Job relatedJob)
		{
			ShowOperationsForm(relatedJob);
		}

		public IZForm LastShownForm_ForTestOnly
		{
			get { return LastShownForm; }
			set { LastShownForm = value; }
		}

		public void OpenButton_Click_ForTestOnly(object sender, System.EventArgs e)
		{
			OpenButton_Click(sender, e);
		}

		public ZButton CloseButton_ForTestOnly
		{
			get { return CloseButton; }
			set { CloseButton = value; }
		}

		public ZButton OpenOperationalDetailsButton_ForTestOnly
		{
			get { return OpenOperationalDetailsButton; }
			set { OpenOperationalDetailsButton = value; }
		}
	}
}

#endif
