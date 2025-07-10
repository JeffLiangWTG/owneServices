using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CA.Module
{
	public partial class LVXFilterStripControl : ZFilterStripControl<LVXFilterStrip>
	{
		public LVXFilterStripControl()
		{
			InitializeComponent();
		}

		public LVXFilterStripControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterStripBusinessObject)
			: base(gridCollection, filterStripBusinessObject)
		{
			InitializeComponent();
		}

		#region Override

		protected override ZBool ShouldSetColorContextKeyFromParentModuleID => true;

		#endregion
	}
}
