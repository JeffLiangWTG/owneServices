using CargoWise.EntityFramework;

namespace Enterprise.Accounting.Module
{
	public partial class ARAccQueryClaimFilterControl : AccQueryClaimFilterControl
	{
		public ARAccQueryClaimFilterControl()
		{
			InitializeComponent();
		}

		public ARAccQueryClaimFilterControl(IBusinessObjectCollection gridCollection, ARAccQueryClaimFilterBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
		}
	}
}
