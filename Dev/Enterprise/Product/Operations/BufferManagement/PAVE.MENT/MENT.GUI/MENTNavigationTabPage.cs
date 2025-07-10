using System;
using Enterprise.PAVE.MENT.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.PAVE.MENT.GUI
{
	public partial class MENTNavigationTabPage : ZTabPage
	{
		public virtual VisualisationConfigurationControl VisualisationUserControl
		{
			get
			{
				if (visualisationUserControl == null)
				{
					visualisationUserControl = new VisualisationConfigurationControl();
				}
				return visualisationUserControl;
			}
			set
			{
				visualisationUserControl = value;
			}
		}
		VisualisationConfigurationControl visualisationUserControl;

		public MENTNavigationTabPage()
		{
			VisualisationUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			Controls.Add(VisualisationUserControl);
		}

#if DEBUG
		public void Initialize_ForTest(MENTAgedScoreQuery query)
		{
			VisualisationUserControl.SetDataBinding(query, string.Empty);
		}
#endif

		public void NavigateToItem(MENTAgedScoreExtraction item)
		{
			NavigateToItem(() => VisualisationUserControl.NavigateToTabItem(item));
		}

		public void NavigateToItem(Action navigationAction)
		{
			var zTabControl = Parent as ZTabControl;

			if (zTabControl != null)
			{
				zTabControl.SelectedTab = this;

				// SelectedTab setter may have been cancelled
				if (zTabControl.SelectedTab == this)
				{
					navigationAction();
				}
			}
		}
	}
}
