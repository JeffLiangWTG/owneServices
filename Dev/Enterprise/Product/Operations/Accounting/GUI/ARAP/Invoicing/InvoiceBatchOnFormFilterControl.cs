using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI
{
	public partial class InvoiceBatchOnFormFilterControl : ZFilterStripControl
	{
		public InvoiceBatchOnFormFilterControl(BusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}

		protected override int MaxFilterStripPanelHeight
		{
			get
			{
				return MaxFilterStripPanelHeight_internalValue;
			}
		}

		public void SetMaxFilterStripPanelHeight(int value)
		{
			MaxFilterStripPanelHeight_internalValue = value;
		}

		internal int MaxFilterStripPanelHeight_internalValue;

		#region Dispose

		protected override void Dispose(bool isNotFinalizing)
		{
			if (isNotFinalizing && components != null)
			{
				components.Dispose();
			}

			base.Dispose(isNotFinalizing);
		}

		#endregion
	}
}

