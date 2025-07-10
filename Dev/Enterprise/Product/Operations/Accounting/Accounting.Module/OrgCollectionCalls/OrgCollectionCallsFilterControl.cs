using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.Module
{
	/// <summary>
	/// Filter control for OrgCollectionCall.
	/// </summary>

	public partial class OrgCollectionCallsFilterControl : ZFilterStripControl
	{
		public OrgCollectionCallsFilterControl()
		{
			InitializeComponent();
		}

		public OrgCollectionCallsFilterControl(IBusinessObjectCollection gridCollection, OrgCollectionCallsFilterBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}

		protected override ZFilterStrip NewZFilterStrip()
		{
			return new OrgCollectionCallsModuleFilterStrip();
		}
	}
}

