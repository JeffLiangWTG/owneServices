using System;
using Enterprise.Accounting.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.JobInvoicing
{
	public partial class PaymentBasesForm : ZChildForm, ICaptionRenderingSupport
	{
		public PaymentBasesForm(JobPaymentBasisViewCollection jobPaymentBases, bool showRevenue)
			: base()
		{
			InitializeComponent();

			if (showRevenue)
			{
				this.Text = Res.GetString("6f201c6d-a844-4fcc-813c-81811a8160fc", "Revenue Calculation");
			}
			else
			{
				this.Text = Res.GetString("58eb89c8-d9cd-47c5-8623-5a5b40a9084a", "Cost Calculation");
			}
			this.paymentBasesGrid.SetDataBinding(jobPaymentBases, string.Empty);
		}

		void closeButton_Click(object sender, EventArgs e)
		{
			Close();
		}

		#region ICaptionRenderingSupport

		bool? ICaptionRenderingSupport.CaptionRenderingEnabled => true;

		event EventHandler ICaptionRenderingSupport.CaptionRenderingEnabledChanged
		{
			add { }
			remove { }
		}

		#endregion
	}
}

