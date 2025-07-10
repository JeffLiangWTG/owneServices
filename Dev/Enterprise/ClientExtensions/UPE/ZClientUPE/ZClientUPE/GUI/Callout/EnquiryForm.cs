using System;
using Enterprise.Client.UPE.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.UPE.GUI
{
	public partial class EnquiryForm : CalloutForm
	{
		public EnquiryForm(Enquiry enquiry) : base(enquiry)
		{
			if (!GlbStaff.CurrentUser.IsNotController)
			{
				ZFormMenuStrategy.AddActionsMenuItem(this, "Force Shipment into Finance", new EventHandler(ForceIntoFinanceMenuItem_Click));
			}
		}

		public override string FormCaption
		{
			get { return "Enquiry - " + BusinessEntity.CS_HAWB; }
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			PartPaymentButton.ReadOnly = true;
		}

		protected override bool ShowProcessQueuePlugIn
		{
			get { return false; }
		}

		void ForceIntoFinanceMenuItem_Click(object sender, EventArgs args)
		{
			Enquiry.ForceIntoFinanceQueue();
		}

		Enquiry Enquiry
		{
			get { return (Enquiry)BusinessEntity; }
		}
	}
}
