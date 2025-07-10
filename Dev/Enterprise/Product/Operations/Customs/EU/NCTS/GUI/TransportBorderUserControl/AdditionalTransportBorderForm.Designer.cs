namespace Enterprise.Customs.EU.NCTS.GUI
{
	partial class AdditionalTransportBorderForm
	{
		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.AdditionalTransportBorderGrid = new Enterprise.ZArchitecture.ZGrid();
			this.CloseButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.OKButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ButtonsPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.TopPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.AdditionalTransportBorderGrid)).BeginInit();
			this.AdditionalTransportBorderGrid.SuspendLayout();
			this.ButtonsPanel.SuspendLayout();
			this.TopPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 215, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(665, 26, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.NCTS.Business.IDepartureCusTransportMeansCollection<Enterprise.Customs.EU.NCTS.Business.DepartureCusTransportMeans>);
			// 
			// AdditionalTransportBorderGrid
			// 
			this.AdditionalTransportBorderGrid.AllowNavigation = false;
			this.AdditionalTransportBorderGrid.AllowSorting = false;
			this.BindingSource.SetBindingMember(this.AdditionalTransportBorderGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.DepartureCusTransportMeans)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.DepartureCusTransportMeans)(null)).TPM_TypeOfIdentification)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.DepartureCusTransportMeans)(null)).TPM_IdentificationNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.DepartureCusTransportMeans)(null)).TPM_RN_NKTransportNationality)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.DepartureCusTransportMeans)(null)).TPM_ReferenceNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.DepartureCusTransportMeans)(null)).TPM_CustomsOffice)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.DepartureCusTransportMeans)(null)).CustomsOfficeDescription)));
			this.AdditionalTransportBorderGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.ColumnName = "TPM_TypeOfIdentification";
			zDropEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo1.ColumnName = "TPM_IdentificationNumber";
			zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zCodeFindBoxColumnStyleInfo1.ColumnName = "TPM_RN_NKTransportNationality";
			zCodeFindBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo2.ColumnName = "TPM_ReferenceNumber";
			zTextBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zDropEditColumnStyleInfo2.ColumnName = "TPM_CustomsOffice";
			zDropEditColumnStyleInfo2.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo3.ColumnName = "CustomsOfficeDescription";
			zTextBoxColumnStyleInfo3.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			this.AdditionalTransportBorderGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.AdditionalTransportBorderGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.AdditionalTransportBorderGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.AdditionalTransportBorderGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.AdditionalTransportBorderGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.AdditionalTransportBorderGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.AdditionalTransportBorderGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AdditionalTransportBorderGrid.GridId = "3B523A3B-2314-46EA-80CA-248B2A625089";
			this.AdditionalTransportBorderGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.AdditionalTransportBorderGrid.LayoutKey = "AdditionalTransportBorderGrid";
			this.AdditionalTransportBorderGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.AdditionalTransportBorderGrid.Name = "AdditionalTransportBorderGrid";
			this.AdditionalTransportBorderGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(659, 195, true);
			this.AdditionalTransportBorderGrid.TabIndex = 1;
			// 
			// CloseButton
			// 
			this.CloseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CloseButton.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("7EB332AA-C81E-44F6-8C4E-57BF2D05B2AC", "Cancel");
			this.CloseButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(587, 18, true);
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
			this.OKButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(503, 18, true);
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
			this.ButtonsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 198, true);
			this.ButtonsPanel.Name = "ButtonsPanel";
			this.ButtonsPanel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.ButtonsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(659, 40, true);
			this.ButtonsPanel.TabIndex = 2;
			// 
			// TopPanel
			// 
			this.TopPanel.AutoScroll = true;
			this.TopPanel.Controls.Add(this.AdditionalTransportBorderGrid);
			this.TopPanel.Controls.Add(this.ButtonsPanel);
			this.TopPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TopPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TopPanel.Name = "TopPanel";
			this.TopPanel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.TopPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(665, 241, true);
			this.TopPanel.TabIndex = 2;
			// 
			// AdditionalTransportBorderForm
			// 
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("8604B33E-FDB7-4356-AB63-F68A010109D2", "Transport Border");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(665, 241, true);
			this.Controls.Add(this.TopPanel);
			this.DataSourceAssemblyName = "Enterprise.Customs.EU.NCTS.Business";
			this.DataSourceType = typeof(Enterprise.Customs.EU.NCTS.Business.IDepartureCusTransportMeansCollection<Enterprise.Customs.EU.NCTS.Business.DepartureCusTransportMeans>);
			this.DataSourceTypeName = "Enterprise.Customs.EU.NCTS.Business.CusTransportMeansCollection";
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(640, 280, true);
			this.Name = "AdditionalTransportBorderForm";
			this.Controls.SetChildIndex(this.TopPanel, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.AdditionalTransportBorderGrid)).EndInit();
			this.AdditionalTransportBorderGrid.ResumeLayout(false);
			this.AdditionalTransportBorderGrid.PerformLayout();
			this.ButtonsPanel.ResumeLayout(false);
			this.ButtonsPanel.PerformLayout();
			this.TopPanel.ResumeLayout(false);
			this.TopPanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		Enterprise.ZArchitecture.GUI.ZPanel TopPanel;
		Enterprise.ZArchitecture.GUI.ZPanel ButtonsPanel;
		internal Enterprise.ZArchitecture.ZGrid AdditionalTransportBorderGrid;
		internal Enterprise.ZArchitecture.GUI.ZButton CloseButton;
		internal Enterprise.ZArchitecture.GUI.ZButton OKButton;
	}
}
