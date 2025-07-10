using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.ARAP.Journal
{
	public partial class BankFeeJournalForm
	{
new void InitializeComponent()
		{
			((ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BankFeeJournalForm
			// 
			this.IsPostOnly = true;
			this.Name = "BankFeeJournalForm";
			((ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
		}

	}
}