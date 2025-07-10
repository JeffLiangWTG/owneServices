using System;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.CommissionManagement.GUI
{
	public partial class GlobalCommissionFinalizerItemGridsControl : ZUserControl, ICommissionFinalizerItemGridsControl
	{
		public GlobalCommissionFinalizerItemGridsControl()
		{
			InitializeComponent();
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			CommissionFinalizerItemGridsStrategy.AddDetailsGridAdornments(DetailCommissionGrid);
		}

		ZFilterGrid ICommissionFinalizerItemGridsControl.TopLevelGrid
		{
			get { return EntityCommissionTotalGrid; }
		}
	}
}
