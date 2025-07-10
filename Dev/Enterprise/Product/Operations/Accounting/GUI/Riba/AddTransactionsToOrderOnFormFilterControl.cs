using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.Riba
{
	public partial class AddTransactionsToOrderOnFormFilterControl : ZFilterStripControl
	{
		public AddTransactionsToOrderOnFormFilterControl(BusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject)
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
	}
}

