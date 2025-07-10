using System;
using System.Windows.Forms;
using Enterprise.Accounting.GUI;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module
{
	public partial class UnapprovedIntercompanyTransactionModule : UnapprovedTransactionModule
	{
		#region Overriden methods

		public override bool AllowNew
		{
			get { return false; }
		}

		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.UnapprovedIntercompanyTransaction; }
		}

		protected override void OnSetupAndGetGrid(ZDisplayGrid grid)
		{
			base.OnSetupAndGetGrid(grid);

			if (previousGrid != null && previousGrid.ContextMenu != null)
			{
				previousGrid.ContextMenu.Popup -= ContextMenu_Popup;
			}

			if (grid.ContextMenu != null)
			{
				grid.ContextMenu.Popup += ContextMenu_Popup;
			}

			previousGrid = grid;
		}

		protected override ZController GetNewControllerFromCreator(AccTransactionHeader selectedHeader)
		{
			return AccountingControllerCreator.GetNewController(selectedHeader, ID);
		}

		ZDisplayGrid previousGrid;
		#endregion

		#region EventHandler

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded constant")]
		void ContextMenu_Popup(object sender, EventArgs e)
		{
			foreach (MenuItem menuItem in Grid.ContextMenu.MenuItems)
			{
				if (menuItem.Text == "Documents")
				{
					Grid.ContextMenu.MenuItems.Remove(menuItem);
					break;
				}
			}
		}

		#endregion
	}
}
