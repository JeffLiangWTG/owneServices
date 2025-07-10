using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.Module
{
	public partial class ChequeTransactionFilterStripControl : ZFilterStripControl
	{
		public ChequeTransactionFilterStripControl()
		{
			InitializeComponent();
		}

		public ChequeTransactionFilterStripControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}
	}
}
