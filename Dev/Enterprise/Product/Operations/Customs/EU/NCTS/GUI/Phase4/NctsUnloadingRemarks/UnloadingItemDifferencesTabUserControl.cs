using System;
using System.Windows.Forms;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	public partial class UnloadingItemDifferencesTabUserControl : ZUserControl
	{
		public UnloadingItemDifferencesTabUserControl()
		{
			InitializeComponent();
			InitializeUnloadedItemUserControl();
		}

		void InitializeUnloadedItemUserControl()
		{
			this.UnloadedItemNewUserControl = GetUnloadedItemNewUserControl();
			this.UnloadedItemDetailsUserControl = GetUnloadedItemDetailsUserControl();
			this.UnloadedItemNewUserControl.SuspendLayout();
			this.UnloadedItemDetailsUserControl.SuspendLayout();

			this.UnloadingItemDifferencesSplitContainer.Panel2.Controls.Add(this.UnloadedItemNewUserControl);
			this.UnloadingItemDifferencesSplitContainer.Panel2.Controls.Add(this.UnloadedItemDetailsUserControl);
			// 
			// UnloadedItemNewUserControl
			// 
			this.UnloadedItemNewUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.UnloadedItemNewUserControl, ".");
			this.UnloadedItemNewUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.UnloadedItemNewUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.UnloadedItemNewUserControl.Name = "UnloadedItemNewUserControl";
			this.UnloadedItemNewUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1297, 253, true);
			this.UnloadedItemNewUserControl.TabIndex = 0;
			// 
			// UnloadedItemDetailsUserControl
			// 
			this.UnloadedItemDetailsUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.UnloadedItemDetailsUserControl, ".");
			this.UnloadedItemDetailsUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.UnloadedItemDetailsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.UnloadedItemDetailsUserControl.Name = "UnloadedItemDetailsUserControl";
			this.UnloadedItemDetailsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1297, 253, true);
			this.UnloadedItemDetailsUserControl.TabIndex = 1;

			this.UnloadedItemNewUserControl.ResumeLayout(true);
			this.UnloadedItemNewUserControl.PerformLayout();
			this.UnloadedItemDetailsUserControl.ResumeLayout(true);
			this.UnloadedItemDetailsUserControl.PerformLayout();

			UnloadedItemDetailsUserControl.Visible = true;
			UnloadedItemNewUserControl.Visible = false;
		}

		protected virtual UnloadedItemNewUserControl GetUnloadedItemNewUserControl()
		{
			return new UnloadedItemNewUserControl();
		}

		protected virtual UnloadedItemDetailsUserControl GetUnloadedItemDetailsUserControl()
		{
			return new UnloadedItemDetailsUserControl();
		}

		void GoodsItemsGrid_SelectedRowsChangedInMouseDown(object sender, EventArgs e)
		{
			var rowIndexOfGoodsItemsGrid = GoodsItemsGrid.CurrentRowIndex;
			if (rowIndexOfGoodsItemsGrid >= 0)
			{
				UnloadedGoodsItemsGrid.CurrentRowIndex = rowIndexOfGoodsItemsGrid;
			}
		}

		void UnloadedGoodsItemsGrid_CurrentCellChanged(object sender, EventArgs e)
		{
			var nctsMovement = (NctsHeader)BindingSource.DataSource;
			if (UnloadedGoodsItemsGrid.CurrentRowIndex <= nctsMovement.ArrivalMovementHeader.GoodsItems.Count - 1)
			{
				UnloadedItemDetailsUserControl.Visible = true;
				UnloadedItemNewUserControl.Visible = false;
				GoodsItemsGrid.CurrentRowIndex = UnloadedGoodsItemsGrid.CurrentRowIndex;
			}
			else
			{
				UnloadedItemNewUserControl.Visible = true;
				UnloadedItemDetailsUserControl.Visible = false;
			}
		}

		void UnloadedGoodsItemsGrid_RowsDeleting(object sender, ZArchitecture.RowsDeletingEventArgs e)
		{
			var nctsMovement = (NctsHeader)BindingSource.DataSource;
			if (UnloadedGoodsItemsGrid.CurrentRowIndex < nctsMovement.ArrivalMovementHeader.GoodsItems.Count)
			{
				Globals.Message.ShowWarning(Res.GetString("47ED3B51-20FC-4297-8FD7-29CDB9EC51CA", "You cannot delete this item. Mark it as ‘Missing’ instead."));
				e.Cancel = true;
			}
			else
			{
				if (Globals.Message.ShowConfirmation(Res.GetString("2C7D4E84-CC88-433E-BACF-14711BEE90A1", "Are you sure you want to delete this item?"),
					Res.GetString("4377A856-E70F-433A-B907-37CB8E7E105B", "Delete Unloaded Item"),
					Res.GetString("F16AD969-4232-4789-8724-8A2D4C6C52FF", "yes"),
					MessageBoxIcon.Question) == DialogResult.OK)
				{
					e.Cancel = false;
				}
				else
				{
					e.Cancel = true;
				}
			}
		}
	}
}
