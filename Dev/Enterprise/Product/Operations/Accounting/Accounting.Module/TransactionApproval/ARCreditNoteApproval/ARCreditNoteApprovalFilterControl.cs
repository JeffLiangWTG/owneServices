using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.Module.TransactionApproval
{
	public partial class ARCreditNoteApprovalFilterControl : ZFilterStripControl
	{
		public ARCreditNoteApprovalFilterControl()
		{
			InitializeComponent();
		}

		public ARCreditNoteApprovalFilterControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}
	}
}

