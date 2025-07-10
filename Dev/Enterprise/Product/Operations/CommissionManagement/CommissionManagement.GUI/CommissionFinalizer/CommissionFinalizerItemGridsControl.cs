using System;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.CommissionManagement.GUI
{
	public interface ICommissionFinalizerItemGridsControl
	{
		ZFilterGrid TopLevelGrid { get; }
	}

	public partial class CommissionFinalizerItemGridsControl : ZUserControl, ICommissionFinalizerItemGridsControl
	{
		public CommissionFinalizerItemGridsControl()
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
			get { return EntityCommissionTotalsGrid; }
		}
	}
}
