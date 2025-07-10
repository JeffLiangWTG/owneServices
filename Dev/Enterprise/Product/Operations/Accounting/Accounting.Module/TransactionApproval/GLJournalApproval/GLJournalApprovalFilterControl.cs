using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.Module.TransactionApproval
{
	public partial class GLJournalApprovalFilterControl : ZFilterStripControl
	{
		public GLJournalApprovalFilterControl()
		{
			InitializeComponent();
		}

		public GLJournalApprovalFilterControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}
	}
}

