using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.ZArchitecture.GUI
{
	public class ZFilterStripToolStripDropDownButton : ZToolStripDropDownButton
	{
		public void SelectCategory(FilterOrCategory orCategory)
		{
			foreach (var item in DropDownCategoryItems)
			{
				item.IsChecked = (item.OrCategory == orCategory) | ((int)orCategory > (int)FilterOrCategory.Grey && item.OrCategory == FilterOrCategory.Others);
			}
		}

		public Color CurrentCategoryColor
		{
			get
			{
				foreach (var item in DropDownCategoryItems)
				{
					if (item.IsChecked)
					{
						return item.Color;
					}
				}
				return SystemColors.Control;
			}
		}

		IEnumerable<ZFilterStripToolStripMenuItem> DropDownCategoryItems
		{
			get
			{
				foreach (ToolStripItem item in DropDownItems)
				{
					var menuItem = item as ZFilterStripToolStripMenuItem;

					if (menuItem != null)
					{
						yield return menuItem;
					}
				}
			}
		}
	}
}
