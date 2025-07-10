namespace Enterprise.Customs.EU.NCTS.GUI
{
	partial class AdditionalWagonNumbersForm
	{
		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
            this.AdditionalWagonNumbersGrid = new Enterprise.ZArchitecture.ZGrid();
            this.CloseButton = new Enterprise.ZArchitecture.GUI.ZButton();
            this.OKButton = new Enterprise.ZArchitecture.GUI.ZButton();
            this.ButtonsPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
            this.TopPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
            ((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.AdditionalWagonNumbersGrid)).BeginInit();
            this.AdditionalWagonNumbersGrid.SuspendLayout();
            this.ButtonsPanel.SuspendLayout();
            this.TopPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // MainStatusBar
            // 
            this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 411, true);
            this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(665, 26, true);
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.NCTS.Business.Interfaces.IDepartureTransportMeansProvider);
            // 
            // AdditionalWagonNumbersGrid
            // 
            this.AdditionalWagonNumbersGrid.AllowNavigation = false;
            this.AdditionalWagonNumbersGrid.AllowSorting = false;
            this.BindingSource.SetBindingMember(this.AdditionalWagonNumbersGrid, "AdditionalWagons");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.Interfaces.IDepartureTransportMeansProvider)(null)).AdditionalWagons)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.Interfaces.IAdditionalWagonProvider)(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.Interfaces.IDepartureTransportMeansProvider)(null)).AdditionalWagons)).SyncRoot)).WagonNumber)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.Interfaces.IAdditionalWagonProvider)(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.Interfaces.IDepartureTransportMeansProvider)(null)).AdditionalWagons)).SyncRoot)).WagonNationality)));
            this.AdditionalWagonNumbersGrid.CaptionVisible = false;
            zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("E3DDE52D-0629-4EB3-A673-F5D1ABCDA597", "Wagon No.", "Wagon Number", "");
            zTextBoxColumnStyleInfo1.ColumnName = "WagonNumber";
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zCodeFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("8f84df95-03c8-4e76-bd7d-c095b7c0dea6", "Nationality");
            zCodeFindBoxColumnStyleInfo1.ColumnName = "WagonNationality";
			zCodeFindBoxColumnStyleInfo1.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.Universal.ZZRefCusCodeList;
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCodeFindBoxColumnStyleInfo1.IsMandatory= true;
            this.AdditionalWagonNumbersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
            this.AdditionalWagonNumbersGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
            this.AdditionalWagonNumbersGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.AdditionalWagonNumbersGrid.GridId = "28D3A19D-4884-449F-A5DE-4F3B5FCAC143";
            this.AdditionalWagonNumbersGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.AdditionalWagonNumbersGrid.LayoutKey = "InlandTransportsGrid";
            this.AdditionalWagonNumbersGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
            this.AdditionalWagonNumbersGrid.Name = "AdditionalWagonNumbersGrid";
            this.AdditionalWagonNumbersGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(658, 390, true);
            this.AdditionalWagonNumbersGrid.TabIndex = 1;
            // 
            // CloseButton
            // 
            this.CloseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.CloseButton.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("7EB332AA-C81E-44F6-8C4E-57BF2D05B2AC", "Cancel");
            this.CloseButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(586, 18, true);
            this.CloseButton.Name = "CloseButton";
            this.CloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(55, 21, true);
            this.CloseButton.TabIndex = 3;
            this.CloseButton.ToolTipCaption = null;
            this.CloseButton.Click += new System.EventHandler(this.OnCloseButton_Click);
            // 
            // OKButton
            // 
            this.OKButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.OKButton.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("97B47B88-AF09-4874-8A61-DD2F789D1C2A", "OK");
            this.OKButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.OKButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(502, 18, true);
            this.OKButton.Name = "OKButton";
            this.OKButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(55, 21, true);
            this.OKButton.TabIndex = 2;
            this.OKButton.ToolTipCaption = null;
            this.OKButton.Click += new System.EventHandler(this.OnOKButton_Click);
            // 
            // ButtonsPanel
            // 
            this.ButtonsPanel.AutoScroll = true;
            this.ButtonsPanel.Controls.Add(this.OKButton);
            this.ButtonsPanel.Controls.Add(this.CloseButton);
            this.ButtonsPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.ButtonsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 394, true);
            this.ButtonsPanel.Name = "ButtonsPanel";
            this.ButtonsPanel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
            this.ButtonsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(658, 40, true);
            this.ButtonsPanel.TabIndex = 2;
            // 
            // TopPanel
            // 
            this.TopPanel.AutoScroll = true;
            this.TopPanel.Controls.Add(this.AdditionalWagonNumbersGrid);
            this.TopPanel.Controls.Add(this.ButtonsPanel);
            this.TopPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.TopPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.TopPanel.Name = "TopPanel";
            this.TopPanel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
            this.TopPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(665, 437, true);
            this.TopPanel.TabIndex = 2;
            // 
            // AdditionalWagonNumbersForm
            // 
            this.CaptionRenderingEnabled = true;
            this.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("B594065E-7629-4DEC-BE17-066AC790423F", "Additional Wagon Numbers");
            this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(665, 437, true);
            this.Controls.Add(this.TopPanel);
            this.DataSourceAssemblyName = "Enterprise.Customs.EU.NCTS.Business.Interfaces";
            this.DataSourceType = typeof(Enterprise.Customs.EU.NCTS.Business.Interfaces.IDepartureTransportMeansProvider);
            this.DataSourceTypeName = "Enterprise.Customs.EU.NCTS.Business.Interfaces.IDepartureTransportMeansProvider";
            this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(500, 400, true);
            this.Name = "AdditionalWagonNumbersForm";
            this.Controls.SetChildIndex(this.TopPanel, 0);
            this.Controls.SetChildIndex(this.MainStatusBar, 0);
            ((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.AdditionalWagonNumbersGrid)).EndInit();
            this.AdditionalWagonNumbersGrid.ResumeLayout(false);
            this.AdditionalWagonNumbersGrid.PerformLayout();
            this.ButtonsPanel.ResumeLayout(false);
            this.ButtonsPanel.PerformLayout();
            this.TopPanel.ResumeLayout(false);
            this.TopPanel.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZPanel TopPanel;
		private Enterprise.ZArchitecture.GUI.ZPanel ButtonsPanel;
		internal Enterprise.ZArchitecture.ZGrid AdditionalWagonNumbersGrid;
		internal Enterprise.ZArchitecture.GUI.ZButton CloseButton;
		internal Enterprise.ZArchitecture.GUI.ZButton OKButton;
	}
}
