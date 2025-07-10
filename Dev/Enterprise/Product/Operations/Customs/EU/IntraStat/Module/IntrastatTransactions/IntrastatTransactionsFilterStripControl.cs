using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.Intrastat.Module
{
	public partial class IntrastatTransactionsFilterStripControl : ZFilterStripControl
	{
		public IntrastatTransactionsFilterStripControl()
		{
			InitializeComponent();
		}

		public IntrastatTransactionsFilterStripControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject) : base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}
	}
}
