using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.GeneralLedger.GLJournals
{
	public partial class GLJournalDissectionAttributesControl : ZUserControl
	{
		public GLJournalDissectionAttributesControl() : base()
		{
			InitializeComponent();

			((System.ComponentModel.ISupportInitialize)(BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(DissectionAttributesGrid)).BeginInit();
			BindingSource.DataSourceType = typeof(Business.Base.Transaction.TransactionHeaderWithLines);
			BindingSource.SetBindingMember(this.DissectionAttributesGrid, DissectionAttributesGridBindingMemberCore);
			((System.ComponentModel.ISupportInitialize)(DissectionAttributesGrid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(BindingSource)).EndInit();
		}

		protected string DissectionAttributesGridBindingMemberCore => "GLJournalLines.AccTransactionLineDissectionAttributes";
	}
}
