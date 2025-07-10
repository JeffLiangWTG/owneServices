using System;
using System.Windows.Forms;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.EDI.IncidentManager.GUI
{
	public partial class BaseIncidentPopupForm : ZChildForm
	{
		public BaseIncidentPopupForm(SupportIncidentAction action)
			: base(action)
		{
		}

		public BaseIncidentPopupForm()
		{
		}

		public override string FormVerb
		{
			get { return ""; }
		}

		#region Close

		void CloseButton_Click(object sender, EventArgs e)
		{
			CloseButtonClickCore(sender, e);
		}

		protected virtual void CloseButtonClickCore(object sender, EventArgs e)
		{
			bool success = ((SupportIncidentAction)BusinessEntity).SynchroniseToIncident();
			if (success)
			{
				DialogResult = DialogResult.OK;
				OnSuccessfulClose();
			}
			else
			{
				DialogResult = DialogResult.None;
				ShowErrorsDialog();
			}
		}

		protected virtual void OnSuccessfulClose()
		{
		}

		void CancelButtonX_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
			OnCancelButtonClick();
		}

		protected virtual void OnCancelButtonClick()
		{
		}

		#endregion

		protected override void InitialiseForm()
		{
			base.InitializeComponent();
			InitializeComponent();
		}
	}
}
