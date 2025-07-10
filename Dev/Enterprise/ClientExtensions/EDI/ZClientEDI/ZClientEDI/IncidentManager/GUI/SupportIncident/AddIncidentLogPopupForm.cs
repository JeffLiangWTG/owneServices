using System;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.ZArchitecture.GUI.Tools;

namespace Enterprise.Client.EDI.IncidentManager.GUI
{
	public partial class AddIncidentLogPopupForm : BaseIncidentPopupForm
	{
		public AddIncidentLogPopupForm(SupportIncidentLogCommentAction action, SupportIncidentForm relatedSupportIncidentForm = null)
			: base(action)
		{
			this.SupportIncidentLogCommentAction = action;
			action.AddInternalLog = true;
			if (relatedSupportIncidentForm != null)
			{
				relatedSupportIncidentForm.LogPopupFormCount++;
			}
			RelatedSupportIncidentForm = relatedSupportIncidentForm;
		}

		protected override void InitialiseForm()
		{
			base.InitializeComponent();
			InitializeComponent();
			SpellChecker.InitialiseSpellcheck(zTextBox1, "AddIncidentLogPopupForm_ZTextBox1");
		}

		protected override void OnCancelButtonClick()
		{
			base.OnCancelButtonClick();
			this.Close();
		}

		protected override void OnSuccessfulClose()
		{
			base.OnSuccessfulClose();
			this.Close();
		}

		protected override void OnClosed(EventArgs e)
		{
			if (RelatedSupportIncidentForm != null)
			{
				RelatedSupportIncidentForm.LogPopupFormCount--;
			}
			base.OnClosed(e);
		}
	}
}
