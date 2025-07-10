using System;

namespace Enterprise.Customs.ES.NCTS.GUI
{
	public partial class UnloadingRemarksUserControl : EU.NCTS.GUI.UnloadingRemarksUserControl
	{
		public UnloadingRemarksUserControl()
		{
			InitializeComponent();
		}

		protected override Type UnloadingHeaderDifferencesUserControlType() => typeof(UnloadingHeaderDifferencesTabUserControl);

		protected override Type UnloadingItemDifferencesDynamicUserControlType() => typeof(UnloadingItemDifferencesTabUserControl);
	}
}
