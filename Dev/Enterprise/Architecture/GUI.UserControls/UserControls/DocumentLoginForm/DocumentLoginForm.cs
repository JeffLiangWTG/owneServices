using System.Windows.Forms;
using Enterprise.ZArchitecture.Business;
using Res = Enterprise.ZArchitecture.GUI.UserControls.Res;

namespace Enterprise.ZArchitecture.GUI
{
	public partial class DocumentLoginForm : ZChildForm
	{
		public DocumentLoginForm()
		{
			InitializeComponent();
		}

		public DocumentLoginForm(SecurityLogin businessObject)
			: base(businessObject)
		{
			InitializeComponent();
			if (businessObject.HideApprovalRequestButton)
			{
				ApprovalRequestButton.Enabled = false;
				ApprovalRequestButton.Visible = false;
			}
		}

		#region Dispose

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

		#region Print

		public override string FormHeading
		{
			get { return Res.GetString("5fa08226-813e-4327-84ca-eb4cd61666a5", "Security Override"); }
		}

		void PrintButton_Click(object sender, System.EventArgs e)
		{
			var documentLoginBisObject = (SecurityLogin)BusinessEntity;

			if (documentLoginBisObject.CheckIfValidLoginForDocumentPrinting())
			{
				DialogResult = DialogResult.Yes;
			}
			else
			{
				documentLoginBisObject.MessageToShowWhenNotPrinting = Res.GetString("cb4a54c6-2195-4589-beac-ec74595e51d1", "This action cannot be performed because either the login name and password were incorrect, password is expired or this person does not have sufficient security rights to perform it.");
				DialogResult = DialogResult.No;
			}
		}

		#endregion
	}
}
