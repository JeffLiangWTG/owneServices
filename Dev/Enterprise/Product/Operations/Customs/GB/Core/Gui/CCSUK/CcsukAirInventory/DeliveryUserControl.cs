using System;
using CargoWise.Types;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GB.GUI.Ccsuk
{
	public partial class DeliveryUserControl : ZUserControl
	{
		public DeliveryUserControl()
		{
			InitializeComponent();
			MakeDivisionContextMenu();
		}

		void MakeDivisionContextMenu()
		{
			this.zGrid1.ContextMenu.MenuItems.Add(0, new ZMenuItem("Divide", DivideMenu_Click));
		}

		void DivideMenu_Click(object s, EventArgs e)
		{
			if (zGrid1.SelectedElements.Length == 1)
			{
				var outturn = zGrid1.SelectedElements[0] as CusOutTurn;
				if (outturn != null)
				{
					ZString newPieces = Globals.Message.QueryDefaultValue("0", "Divide this receipt and create a new row with this many pieces...", "Divide receipt row", 1);
					if (!newPieces.IsEmpty)
					{
						var result = outturn.Divide(newPieces);
						if (!result.IsEmpty)
						{
							Globals.Message.ShowError(result);
						}
					}
				}
			}
			else
			{
				Globals.Message.ShowError("Select exactly one row");
			}
		}
	}
}
