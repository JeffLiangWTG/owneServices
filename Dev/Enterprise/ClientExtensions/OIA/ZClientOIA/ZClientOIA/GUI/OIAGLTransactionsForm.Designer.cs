using System;
using Enterprise.Accounting.DataTransfer.GLJournals;
using Enterprise.Accounting.GUI.GeneralLedger.GLJournals;
using Enterprise.Client.OIA.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.OIA.GUI
{
	internal partial class OIAGLTransactionsForm : CsvExportGLTransactionForm
	{
		new void InitializeComponent()
		{
			base.InitializeComponent();

			// 
			// OIAGLTransactionsForm
			// 
			this.DataSourceAssemblyName = "ZClientOIA";
			this.DataSourceType = typeof(Enterprise.Client.OIA.Business.OIAGLTransactionBusinessObject);
			this.DataSourceTypeName = "Enterprise.Client.OIA.Business.OIAGLTransactionBusinessObject";
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Name = "OIAGLTransactionsForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Export GL Transactions in OIA CSV Format";
		}
	}
}
