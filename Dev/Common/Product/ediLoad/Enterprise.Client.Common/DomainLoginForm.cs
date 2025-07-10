using System.Windows.Forms;

namespace Enterprise.Client.Common
{
	public partial class DomainLoginForm : Form
	{
		public DomainLoginForm(string errorMessage)
		{
			InitializeComponent();
			this.labelError.Text = errorMessage;
		}

		public string Username
		{
			get { return textBoxUsername.Text; }
		}

		public string Password
		{
			get { return textBoxPassword.Text; }
		}

		void buttonLogin_Click(object sender, System.EventArgs e)
		{
			DialogResult = DialogResult.OK;
			Close();
		}
	}
}