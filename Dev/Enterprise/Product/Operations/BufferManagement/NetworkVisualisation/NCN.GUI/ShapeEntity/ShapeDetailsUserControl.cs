using System;
using Enterprise.BufferManagement.NetworkVisualisation.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.BufferManagement.NetworkVisualisation.GUI
{
	public partial class ShapeDetailsUserControl : ZUserControl
	{
		public ShapeDetailsUserControl()
		{
			InitializeComponent();
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			var entity = (ShapeNetworkEntity)DataSource;
			var supportsChanneling = entity.Shape.SupportsChanneling;

			ChannelsTabPage.TabVisible = supportsChanneling;
			LevelingRulesTabPage.TabVisible = supportsChanneling;
		}
	}
}
