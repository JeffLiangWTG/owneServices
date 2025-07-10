using System;
using Enterprise.Client.UPE.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.UPE.GUI
{
	public partial class PartPaymentForm : ZChildForm
	{
		public PartPaymentForm(CalloutPartPayment calloutPartPayment)
			: base(calloutPartPayment)
		{
		}

		protected override void InitialiseForm()
		{
			InitializeComponent();
			base.InitializeComponent();
		}
		CalloutPartPayment CalloutPartPayment
		{
			get { return (CalloutPartPayment)BusinessEntity; }
		}

		public override string FormHeading
		{
			get { return "Enter Part Payment Details"; }
		}

		void OKButton_Click(object sender, EventArgs e)
		{
			CalloutPartPayment.RunPreSaveValidation();
			if (CalloutPartPayment.HasErrors)
			{
				CalloutPartPayment.MustAddNote = false;
				Globals.Message.ShowInformation("All errors must be corrected before you can proceed.");
			}
			else
			{
				CalloutPartPayment.MustAddNote = true;
				Close();
			}
		}

		void cancelButton_Click(object sender, EventArgs e)
		{
			CalloutPartPayment.MustAddNote = false;
			Close();
		}
	}
}
