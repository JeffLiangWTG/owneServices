#if DEBUG

using System;
using System.Windows.Forms;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.Accounting.Module
{
	public partial class AccHotChequeModule
	{
		public void HandlePrint_ForTestOnly(object sender, EventArgs e)
		{
			HandlePrint(sender, e);
		}

		public MenuItem[] GetNewActionMenuItems_ForTestOnly()
		{
			return GetNewActionMenuItems();
		}

		public ResourceStringData GetDeleteMenuItemText_ForTestOnly()
		{
			return GetDeleteMenuItemText();
		}
	}
}

#endif