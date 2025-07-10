using System;
using Enterprise.Customs.DE.Business;
using Enterprise.Customs.DE.NCTS.Business;
using Enterprise.Customs.GUI;
using Enterprise.Customs.GUI.Guarantees;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.DE.NCTS.GUI
{
	public class DEGuaranteeForm : GuaranteeForm
	{
		public DEGuaranteeForm(CusGuaranteeHeader guaranteeHeader, GuaranteeTransactionFilterStripBusinessObject filterStripBusinessObject = null)
			: base(guaranteeHeader, filterStripBusinessObject)
		{
			ControllerID = ControllerIDs.Customs.Guarantees;
			ZFormMenuStrategy.AddActionsMenuItem(this, ResString.GetMultilingualString("b4e1e282-9647-4499-b91c-987f1e372f1c", "Send Access Code to Customs"), SendAccessCodesMenuItemClick);
		}

		void SendAccessCodesMenuItemClick(object sender, EventArgs e)
		{
			var viewModel = new SendAccessCodeViewModel(BusinessEntity as CusGuaranteeHeader);
			var form = new SendAccessCodeForm(viewModel);
			ZFormModaliser.ShowDialogAndDispose(form, this);
		}
	}
}
