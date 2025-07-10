using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.H7.GUI
{
	partial class EUH7ItemPacksUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.asycudaPackPackedItemLinkGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.asycudaPackPackedItemLinkGrid)).BeginInit();
			this.asycudaPackPackedItemLinkGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// CusTempPacksGrid
			// 
			this.asycudaPackPackedItemLinkGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.asycudaPackPackedItemLinkGrid, "AsycudaPackPackedItemLinks");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			this.asycudaPackPackedItemLinkGrid.CaptionVisible = false;
			zCheckBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zCheckBoxColumnStyleInfo1.ColumnName = "IsLinked";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo1.ColumnName = "PackQty";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo2.ColumnName = "PackageNumber";
			this.asycudaPackPackedItemLinkGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.asycudaPackPackedItemLinkGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.asycudaPackPackedItemLinkGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.asycudaPackPackedItemLinkGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.asycudaPackPackedItemLinkGrid.GridId = "3f350399-d6ac-440e-bec2-acb4695ead44";
			this.asycudaPackPackedItemLinkGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.asycudaPackPackedItemLinkGrid.LayoutKey = "CusTempPacksGrid";
			this.asycudaPackPackedItemLinkGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.asycudaPackPackedItemLinkGrid.Name = "CusTempPacksGrid";
			this.asycudaPackPackedItemLinkGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(433, 150, true);
			this.asycudaPackPackedItemLinkGrid.TabIndex = 0;
			// 
			// EUH7CusTempPacksUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.asycudaPackPackedItemLinkGrid);
			this.Name = "EUH7CusTempPacksUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(433, 150, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.asycudaPackPackedItemLinkGrid)).EndInit();
			this.asycudaPackPackedItemLinkGrid.ResumeLayout(false);
			this.asycudaPackPackedItemLinkGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		ZArchitecture.ZGrid asycudaPackPackedItemLinkGrid;
	}
}
