using System;
using Enterprise.Customs.BR.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.BR.GUI
{
	public partial class LPCOCollectionForm : ZChildForm
	{
		public LPCOCollectionForm(JobComInvoiceLine invoiceLine)
			: base(invoiceLine)
		{
		}

		public static void ShowDialog(JobComInvoiceLine invoiceLine)
		{
			ZFormModaliser.ShowDialogAndDispose(new LPCOCollectionForm(invoiceLine));
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		#region Form Caption

		public override string FormVerb
		{
			get { return ""; }
		}

		#endregion

		#region Buttons

		void OnOKButton_Click(object sender, EventArgs e)
		{
			Close();
		}

		void OnCloseButton_Click(object sender, EventArgs e)
		{
			Close();
		}

		#endregion
	}
}
