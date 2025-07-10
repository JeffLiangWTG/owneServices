using System;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI
{
	[SuppressControlRequiresTextBasher]
	public partial class SupplementaryCode1AndGDMUserControl : ZUserControl, IExtendedControl
	{
		public SupplementaryCode1AndGDMUserControl()
		{
			InitializeComponent();
			Extensions = new DefaultControlExtensionCollection(this);
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);
			Extensions.SetDataBinding(dataSource, dataMember);
		}

		public Control Host => this;

		public IControlExtensionCollection Extensions { get; }

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				Extensions.Dispose();
			}
			base.Dispose(disposing);
		}

		protected virtual Control GetInvoiceLineUserControl() => Parent?.Parent?.Parent?.Parent?.Parent?.Parent;

		#region GDM Link

		void GDMLink_Clicked(object sender, EventArgs e)
		{
			if (GetInvoiceLineUserControl() is EUInvoiceLineUserControl invoiceLineUserControl)
			{
				var invoiceLine = invoiceLineUserControl.CurrentInvoiceLine;
				if (invoiceLine != null)
				{
					ZFormModaliser.ShowDialogAndDispose(GetGuidedDecisionMakingForm(invoiceLine));
				}
			}
		}

		protected GuidedDecisionMakingForm GetGuidedDecisionMakingForm(JobComInvoiceLine invoiceLine)
		{
			var gDMBasic = invoiceLine.GetGuidedDecisionMakingBasic();
			var target = invoiceLine.GetGuidedDecisionMakingSingleInvoiceLineTarget();
			return GetGuidedDecisionMakingFormCore(gDMBasic, target);
		}

		protected virtual GuidedDecisionMakingForm GetGuidedDecisionMakingFormCore(GuidedDecisionMakingBasic gDMBasic, IGuidedDecisionMakingTarget target) => new GuidedDecisionMakingForm(gDMBasic, target);

		#endregion

	}
}
