using System;

namespace Enterprise.Customs.FR.GUI.NCTS
{
	public partial class FRUnloadingRemarksUserControl : EU.NCTS.GUI.UnloadingRemarksUserControl
	{
		public FRUnloadingRemarksUserControl()
		{
			InitializeComponent();
		}

		protected override Type UnloadingItemDifferencesDynamicUserControlType()
		{
			return typeof(FRUnloadingItemDifferencesTabUserControl);
		}
	}
}
