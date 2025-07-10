namespace Enterprise.Customs.BR.GUI
{
	partial class WarehouseAreaIDsForm
	{
		#region Component Designer generated code

		internal Enterprise.ZArchitecture.ZGrid WarehouseAreaIDGrid;
		internal Enterprise.ZArchitecture.GUI.ZButton CloseButton;
		internal Enterprise.ZArchitecture.GUI.ZButton OKButton;

		new void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.WarehouseAreaIDGrid = new Enterprise.ZArchitecture.ZGrid();
			this.CloseButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.OKButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.WarehouseAreaIDGrid)).BeginInit();
			this.WarehouseAreaIDGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 235, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(286, 23, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.BR.Business.JobDeclaration);
			// 
			// warehouseAreaIDGrid
			// 
			this.WarehouseAreaIDGrid.AllowNavigation = false;
			this.WarehouseAreaIDGrid.AllowSorting = false;
			this.WarehouseAreaIDGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.WarehouseAreaIDGrid, "WarehouseAreas");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.BR.Business.JobDeclaration)(null)).WarehouseAreas)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.CusCodeData)(((System.Collections.IList)(((Enterprise.Customs.BR.Business.JobDeclaration)(null)).WarehouseAreas)).SyncRoot)).CY_Code)));
			this.WarehouseAreaIDGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.ColumnName = "CY_Code";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			this.WarehouseAreaIDGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.WarehouseAreaIDGrid.GridId = "DD772FE2-B925-49D9-A132-8D28395D4FB9";
			this.WarehouseAreaIDGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.WarehouseAreaIDGrid.LayoutKey = "WarehouseAreaIDGrid";
			this.WarehouseAreaIDGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.WarehouseAreaIDGrid.Name = "WarehouseAreaIDGrid";
			this.WarehouseAreaIDGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(286, 206, true);
			this.WarehouseAreaIDGrid.TabIndex = 1;
			// 
			// CloseButton
			// 
			this.CloseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CloseButton.CaptionResourceString = Enterprise.Customs.BR.GUI.Res.GetData("WarehouseAreaIDForm|5D0F3D26-8946-4FC3-95C0-DDDB11A4C17B", "Cancel");
			this.CloseButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(160, 211, true);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 21, true);
			this.CloseButton.TabIndex = 3;
			this.CloseButton.ToolTipCaption = null;
			this.CloseButton.Click += new System.EventHandler(this.OnCloseButton_Click);
			// 
			// OKButton
			// 
			this.OKButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.OKButton.CaptionResourceString = Enterprise.Customs.BR.GUI.Res.GetData("WarehouseAreaIDForm|30C3627B-6B2A-485F-A276-C0064F4A972F", "OK");
			this.OKButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.OKButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 211, true);
			this.OKButton.Name = "OKButton";
			this.OKButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 21, true);
			this.OKButton.TabIndex = 2;
			this.OKButton.ToolTipCaption = null;
			this.OKButton.Click += new System.EventHandler(this.OnOKButton_Click);
			// 
			// WarehouseAreasForm
			// 
			this.AcceptButton = this.OKButton;
			this.CancelButton = this.CloseButton;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Customs.BR.GUI.Res.GetData("WarehouseAreaIDForm|359AC22F-F679-4977-B6B8-7BBE262C1782", "Warehouse Area ID");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(286, 258, true);
			this.Controls.Add(this.OKButton);
			this.Controls.Add(this.CloseButton);
			this.Controls.Add(this.WarehouseAreaIDGrid);
			this.DataSourceAssemblyName = "Enterprise.Customs.BR.Business";
			this.DataSourceType = typeof(Enterprise.Customs.BR.Business.JobDeclaration);
			this.DataSourceTypeName = "Enterprise.Customs.BR.Business.JobDeclaration";
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 296, true);
			this.Name = "WarehouseAreasForm";
			this.Controls.SetChildIndex(this.WarehouseAreaIDGrid, 0);
			this.Controls.SetChildIndex(this.CloseButton, 0);
			this.Controls.SetChildIndex(this.OKButton, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.WarehouseAreaIDGrid)).EndInit();
			this.WarehouseAreaIDGrid.ResumeLayout(false);
			this.WarehouseAreaIDGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
	}
}
