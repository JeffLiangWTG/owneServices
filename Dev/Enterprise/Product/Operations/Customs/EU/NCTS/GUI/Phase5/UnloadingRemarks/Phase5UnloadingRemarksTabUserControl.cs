using System;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public partial class Phase5UnloadingRemarksTabUserControl : ZUserControl
	{
		public Phase5UnloadingRemarksTabUserControl()
		{
			InitializeComponent();
		}

		protected new NctsHeader DataSource => (NctsHeader)base.DataSource;

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);
			SetDynamicLayout();
		}

		void SetDynamicLayout()
		{
			DynamicUnloadingDifferencesTabUserControl.UserControlType = LayoutProvider.UnloadingDifferencesTabUserControlType;
		}

		INctsPhase5LayoutProvider LayoutProvider => layoutProvider ?? (layoutProvider = NctsPhase5LayoutProvider.GetLayoutProvider(DataSource?.DefaultDataGroupingCode));
		INctsPhase5LayoutProvider layoutProvider;
	}
}
