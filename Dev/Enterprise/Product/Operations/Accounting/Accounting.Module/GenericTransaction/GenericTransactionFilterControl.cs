using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.Module
{
	/// <summary>
	/// Filter control for GenericTransaction.
	/// </summary>
	public partial class GenericTransactionFilterControl : ZFilterStripControl
	{
		protected override ZFilterStrip NewZFilterStrip()
		{
			return new GenericTransactionFilterStrip();
		}

		public GenericTransactionFilterControl()
		{
			InitializeComponent();
		}

		public GenericTransactionFilterControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}
	}
}
