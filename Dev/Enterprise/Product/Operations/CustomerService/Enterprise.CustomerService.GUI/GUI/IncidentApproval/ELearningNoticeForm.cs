using System;
using System.ComponentModel;
using System.Windows.Forms;
using Enterprise.Registry.Business;
using Enterprise.UserPortal;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.CustomerService.GUI
{
	public partial class ELearningNoticeForm : ZChildForm
	{
		public ELearningNoticeForm()
		{
			InitializeComponent();
		}

		void raiseTrainingIncidentLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			Close();
		}

		void eLearningPortalButton_Click(object sender, EventArgs e)
		{
			OpenELearningPortal();
		}

		void wiseLearningButton_Click(object sender, EventArgs e)
		{
			OpenELearningPortal();
		}

		void RaiseTrainingIncidentCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			raiseTrainingIncidentLinkLabel.Enabled = raiseTrainingIncidentCheckBox.Checked;
		}

		void OpenELearningPortal()
		{
			UserPortalLauncher.LaunchUserPortal(Res.GetString("a2b9afe9-5e59-4126-af77-2c72442b0102", "Wise Learning"), SystemDataRegistry.Instance.ELearningUrl.Value);
		}

		protected override void ZForm_Closing(object sender, CancelEventArgs e)
		{
			if (!raiseTrainingIncidentCheckBox.Checked)
			{
				Globals.Message.ShowInformation(Res.GetString("107E16D6-55CC-4F1F-98A9-F445F34801E7", "Please tick the box 'I searched the WiseLearning portal and did not find any relevant content' if you want to proceed with the incident."));
				e.Cancel = true;
			}
		}

		UserPortalLauncher userPortalLauncher;
		protected virtual UserPortalLauncher UserPortalLauncher
		{
			get { return userPortalLauncher ?? (userPortalLauncher = new UserPortalLauncher()); }
		}

		void HowToDocumentERequestIncidentLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			UserPortalLauncher.LaunchUserPortal(Res.GetString("1C6889F4-FACE-440E-BDAB-4B1C681F4073", "How to Document eRequest Incident"), SystemDataRegistry.Instance.HowToDocumentERequestIncidentUrl.Value);
		}
	}
}
