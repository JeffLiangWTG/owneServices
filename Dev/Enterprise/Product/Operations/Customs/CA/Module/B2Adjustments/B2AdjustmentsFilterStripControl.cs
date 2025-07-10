using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CA.Module
{
	public partial class B2AdjustmentsFilterStripControl : ZFilterStripControl
	{
		public B2AdjustmentsFilterStripControl()
		{
			InitializeComponent();
		}

		public B2AdjustmentsFilterStripControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterStripBusinessObject)
			: base(gridCollection, filterStripBusinessObject)
		{
			InitializeComponent();
		}

		#region Override

		protected override ZBool ShouldSetColorContextKeyFromParentModuleID => true;

		#endregion
	}
}
