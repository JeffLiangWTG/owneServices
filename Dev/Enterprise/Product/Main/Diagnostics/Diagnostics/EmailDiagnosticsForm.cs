using CargoWise.Types;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Diagnostics
{
	public partial class EmailDiagnosticsForm : ZChildForm
	{
		public EmailDiagnosticsForm() : base(new EmailDiagnostics())
		{
			InitializeComponent();
		}

		protected EmailDiagnostics EmailDiagnostics
		{
			get { return (EmailDiagnostics)BusinessEntity; }
		}

		public override string FormVerb
		{
			get { return null; }
		}

		#region Buttons

		void SendButton_Click(object sender, System.EventArgs e)
		{
			EmailDiagnostics.Send();
			EmailSentLabel.Text = Enterprise.Main.DiagnosticsAndTesting.Res.GetString("ab5b238c-25ce-4457-85f7-2553966bb2e8", "Test email sent ({0})", ZDateTime.Now.ToLongTimeString());
		}

		void CheckMailButton_Click(object sender, System.EventArgs e)
		{
			EmailDiagnostics.CheckMail();
		}

		#endregion
	}
}
