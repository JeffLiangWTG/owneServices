using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.Module
{
	public partial class ChequeFilterControl : ZFilterStripControl
	{
		public ChequeFilterControl()
		{
			InitializeComponent();
		}

		public ChequeFilterControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}
	}
}
