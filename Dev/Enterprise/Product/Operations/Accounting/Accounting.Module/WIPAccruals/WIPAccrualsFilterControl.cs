using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.Module
{
	/// <summary>
	/// Filter control for WIPAccruals.
	/// </summary>
	public partial class WIPAccrualsFilterControl : ZFilterStripControl
	{
		protected override ZFilterStrip NewZFilterStrip()
		{
			return new WIPAccrualsFilterStrip();
		}

		public WIPAccrualsFilterControl()
		{
			InitializeComponent();
		}

		public WIPAccrualsFilterControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}
	}
}
