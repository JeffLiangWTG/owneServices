using System;
using System.Windows.Forms;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.JobInvoicing
{
	public class ContainerCountMenuItemManager
	{
		public ContainerCountMenuItemManager(ZGrid grid)
		{
			Grid = grid;
		}

		readonly ZGrid Grid;

		public void AddMenuItem()
		{
			Grid.ContextMenu.MenuItems.Add(0, new ZMenuItem("-"));
			MenuItem menuItem = new ZMenuItem(ResString.GetMultilingualString(AssignContainersResourceKey, "Assign Container(s)"), AssignContainers_Click);
			menuItem.Shortcut = Shortcut.Ctrl1;
			menuItem.ShowShortcut = true;
			Grid.ContextMenu.MenuItems.Add(0, menuItem);
		}

		void AssignContainers_Click(object sender, EventArgs args)
		{
			if (Grid.ListManager == null || Grid.ListManager.Position < 0 || Grid.ListManager.GetCurrent() == null)
			{
				return;
			}

			var selectedData = Grid.ListManager.GetCurrent() as ContainerCalculationData;
			if (selectedData == null)
			{
				return;
			}

			var containerSelections = selectedData.ContainerSelections;
			var copied = containerSelections.Clone();

			var result = ZFormModaliser.ShowDialogAndDispose(new ContainersSelectionForm(containerSelections), Grid.Parent as Form);
			if (result != DialogResult.OK)
			{
				containerSelections.RemoveAll();
				containerSelections.AddRange(copied);
				containerSelections.OnSelectionsChanged();
			}

			selectedData.ContainerNumbersInfo.RefreshBinding();
			selectedData.QuantitySelectedContainersInfo.RefreshBinding();
		}

		public const string AssignContainersResourceKey = "476ac4aa-f603-4bf2-8088-8b49dd311f02";
	}
}

