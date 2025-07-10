using System;
using System.ComponentModel;
using System.Windows.Forms;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.ZArchitecture.GUI
{
	[TypeDescriptionProvider(typeof(ZControlTypeDescriptionProvider))]
	public partial class TermsAcknowledgementForm : ZChildForm
	{
		public TermsAcknowledgementForm(BaseTermsAgreement term)
		{
			BindingSource.DataSource = term;
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		protected async void ContinueButton_Click(object sender, EventArgs e)
		{
			ContinueButton.Enabled = false;
			using (var progressForm = new TermsProgressForm())
			{
				progressForm.ShowCancelButton = false;
				progressForm.ShowProgressBar = false;
				progressForm.UpdateStatus(
					Res.GetString("3864715D-5301-462A-8605-F4564DEB8D07",
						"Processing response..."), 0);
				progressForm.ShowModalTo(this);
				await ((BaseTermsAgreement)DataSource).TryPostAcknowledgement();
			}

			DialogResult = DialogResult.OK;
			Close();
		}

		void ForbiddenButton_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
			Close();
		}

		void TermsAcknowledgementForm_Activated(object sender, EventArgs e)
		{
			ForbiddenButton.Focus();
		}
	}
}
