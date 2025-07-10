using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.Declaration.GUI
{
	public partial class PopupStatusChangeForm : ZChildForm
	{
		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		public PopupStatusChangeForm()
		{
		}

		public void Show(JobDeclaration declaration, bool isHoldType)
		{
			this.isHoldType = isHoldType;
			this.declaration = declaration;
			if (isHoldType)
			{
				Text = "Set Status to 'Hold Awaiting'";
				reasonLabel.Text = "Please enter a reason for this status change";
			}
			else
			{
				Text = "Set Status to 'Declaration Work Complete'";
				reasonLabel.Text = "Please enter a reason for this status change";
			}
			Show();
		}

		protected JobDeclaration declaration;
		protected bool isHoldType;
		void OKButton_Click(object sender, System.EventArgs e)
		{
			if (isHoldType)
			{
				declaration.PlaceHold(reasonTextBox.Text);
			}
			else
			{
				declaration.PlaceDeclarationWorkComplete(reasonTextBox.Text);
			}
			Close();
		}

		void CancelButton_Click(object sender, System.EventArgs e)
		{
			Close();
		}
	}
}
