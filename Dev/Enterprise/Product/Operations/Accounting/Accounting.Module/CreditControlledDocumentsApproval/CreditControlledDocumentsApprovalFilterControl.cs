using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.Module
{
	public partial class CreditControlledDocumentsApprovalFilterControl : ZFilterStripControl
	{
		public CreditControlledDocumentsApprovalFilterControl()
		{
			InitializeComponent();
		}

		public CreditControlledDocumentsApprovalFilterControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}
	}
}
