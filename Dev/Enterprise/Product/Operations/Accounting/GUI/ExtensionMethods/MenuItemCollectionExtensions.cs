using System.Windows.Forms;

namespace Enterprise.Accounting.GUI
{
	public static class MenuItemCollectionExtensions
	{
		public static void AddIfNotNull(this Menu.MenuItemCollection collection, MenuItem item)
		{
			if (item != null)
			{
				collection.Add(item);
			}
		}
	}
}
