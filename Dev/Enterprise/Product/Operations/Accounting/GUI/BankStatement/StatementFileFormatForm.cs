using System.Windows.Forms;
using CargoWise.ComponentModel;
using Enterprise.Accounting.Business.Base.AccStatement;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.BankStatement
{
	public partial class StatementFileFormatForm : ZChildForm
	{
		public StatementFileFormatForm(BankStatementFormat businessEntity)
			: base(businessEntity)
		{
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		void StatementFileFormatForm_FormClosing(object sender, FormClosingEventArgs e)
		{
			e.Cancel = DialogResult == DialogResult.OK && BusinessEntity.Notifications.HasErrors();
		}
	}
}
