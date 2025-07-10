using CargoWise.EntityFramework;

namespace Enterprise.Accounting.Module
{
	public partial class APAccQueryClaimFilterControl : AccQueryClaimFilterControl
	{
		public APAccQueryClaimFilterControl()
		{
			InitializeComponent();
		}

		public APAccQueryClaimFilterControl(IBusinessObjectCollection gridCollection, APAccQueryClaimFilterBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
		}
	}
}
