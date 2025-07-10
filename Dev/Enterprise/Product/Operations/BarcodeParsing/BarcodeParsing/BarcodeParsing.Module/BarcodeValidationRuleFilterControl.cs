using Enterprise.BarcodeParsing.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.BarcodeParsing.Module
{
	public partial class BarcodeValidationRuleFilterControl : ZFilterStripControl
	{
		// for Designer
		public BarcodeValidationRuleFilterControl()
			: this(null, null)
		{
		}

		public BarcodeValidationRuleFilterControl(BarcodeValidationRuleCollection collection, BarcodeValidationRuleFilterBusinessObject filterBizO)
			: base(collection, filterBizO)
		{
			InitializeComponent();
		}
	}
}
