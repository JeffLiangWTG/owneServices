using System;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.Declaration.GUI
{
	public partial class AUStatesForm : ZChildForm
	{
		public AUStatesForm(JobComInvoiceLine invoiceLine)
		: base(invoiceLine)
		{
		}

		public override string FormCaption
		{
			get { return "AUState Form"; }
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		public new JobComInvoiceLine BusinessEntity
		{
			get { return base.BusinessEntity as JobComInvoiceLine; }
		}

		void OKBtn_Click(object sender, EventArgs e)
		{
			BusinessEntity.AUStateCodeCollection.RunPreSaveValidation();
			if (BusinessEntity.AUStateCodeCollection.HasNotifications())
			{
				Globals.Message.ShowError("There are errors that need to be fixed");
			}
			else
			{
				BusinessEntity.AUStateCodeCollection.ReBuildAUState();
				Close();
			}
		}
	}
}
