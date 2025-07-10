using Enterprise.BarcodeParsing.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.BarcodeParsing.Module
{
	public partial class BarcodeRuleFilterControl : ZFilterStripControl
	{
		// for Designer
		public BarcodeRuleFilterControl()
			: this(null, null)
		{
		}

		public BarcodeRuleFilterControl(BarcodeRuleCollection collection, BarcodeRuleFilterBusinessObject filterBizO)
			: base(collection, filterBizO)
		{
			InitializeComponent();
		}
	}
}
