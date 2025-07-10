using System;
using Enterprise.PAVE.MENT.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.PAVE.MENT.GUI
{
	public partial class VisualisationConfigurationControl : ZUserControl
	{
#if DEBUG
		public ZGrid ExtractionsGrid_ForTest => extractionsGrid;
#endif

		public VisualisationConfigurationControl()
		{
			InitializeComponent();

			extractionsGrid.AfterBind += (s, e) => { extractionsGrid.ListManager.CurrentChanged += OnExtractionChanged; };
			extractionsGrid.ContextMenu.Popup += ContextMenu_Popup;
		}

		void ContextMenu_Popup(object sender, EventArgs e)
		{
			if (cloneMenuItem == null)
			{
				var collection = (MENTAgedScoreExtractionCollection)extractionsGrid.ListManager.List;
				cloneMenuItem = new CloneMenuItem(extractionsGrid, () => collection);
				cloneMenuItem.Click += OnExtractionChanged;
				extractionsGrid.ContextMenu.MenuItems.Add(cloneMenuItem);
			}
		}

		CloneMenuItem cloneMenuItem;

		void OnExtractionChanged(object sender, EventArgs e)
		{
			UpdateSectionControls();
		}

		void UpdateSectionControls()
		{
			selectedItem = GetSelectedExtraction();

			if (selectedItem != null)
			{
				if (seriesConfigurationControl != null)
				{
					seriesConfigurationControl.UpdateBinding(selectedItem.SeriesFilter);
				}
			}
		}

		MENTAgedScoreExtraction GetSelectedExtraction()
		{
			return extractionsGrid.ListManager != null ? extractionsGrid.ListManager.GetCurrent() as MENTAgedScoreExtraction : null;
		}

		MENTAgedScoreExtraction selectedItem;

		public virtual void NavigateToTabItem(MENTAgedScoreExtraction item)
		{
			if (extractionsGrid != null && extractionsGrid.ListManager != null)
			{
				for (int j = 0; j < extractionsGrid.ListManager.Count; j++)
				{
					var currentExtraction = (MENTAgedScoreExtraction)extractionsGrid.List[j];
					if (item.PK == currentExtraction.PK)
					{
						extractionsGrid.ListManager.Position = j;
						extractionsGrid.Select(j);
						break;
					}
				}
			}
		}
	}
}
