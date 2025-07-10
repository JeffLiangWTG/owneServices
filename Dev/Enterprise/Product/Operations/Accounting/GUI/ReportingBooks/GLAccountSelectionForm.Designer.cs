using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.GUI
{
	partial class GLAccountSelectionForm
	{
		readonly AccGLHeaderCollection gridCollection;
		public AccGLHeaderCollection GridCollection
		{
			get
			{
				gridCollection.SetReadOnlyIncludingChildren(true);
				return gridCollection;
			}
		}

		protected override void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo10 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo11 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo12 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo13 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo14 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.MessageLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ButtonPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.panelOkCancelButtons = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.OK_Button = new Enterprise.ZArchitecture.GUI.ZButton();
			this.Cancel_Button = new Enterprise.ZArchitecture.GUI.ZButton();
			this.FilterControlPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.Grid = new Enterprise.ZArchitecture.ZGrid();
			// 
			// Grid
			//
			this.BindingSource.SetBindingMember(Grid, "GridCollection");
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("GLAccountSelectionForm|6f78263b-28f3-4e9b-89a5-9ab378dc6f67", "GL Account");
			zTextBoxColumnStyleInfo1.ColumnName = "AG_AccountNum";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("GLAccountSelectionForm|1c306b0e-3d55-48b4-9a8b-4f0178b4bb7b", "Account Name");
			zTextBoxColumnStyleInfo2.ColumnName = "AG_DescriptionMultilingual";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("GLAccountSelectionForm|53e0dca3-d8aa-4f25-985e-8f0f4291c06d", "Type");
			zTextBoxColumnStyleInfo3.ColumnName = "AG_AccountType";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo13.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("GLAccountSelectionForm|789cb305-c838-48f0-b9db-1152690430d4", "Cash Flow Type");
			zTextBoxColumnStyleInfo13.ColumnName = "AG_CashFlowType";
			zTextBoxColumnStyleInfo13.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("GLAccountSelectionForm|932a2070-469d-430b-b5b2-eb6d8c0d50a6", "PS");
			zCalcEditColumnStyleInfo1.ColumnName = "AG_PrintSequence";
			zCalcEditColumnStyleInfo1.Decimals = 0;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(30);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("GLAccountSelectionForm|85e504ef-9481-4033-b43d-02f27ea23342", "TL");
			zCalcEditColumnStyleInfo2.ColumnName = "AG_TotalLevel";
			zCalcEditColumnStyleInfo2.Decimals = 0;
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(30);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("GLAccountSelectionForm|91f9abd1-552d-47e9-a66f-2673bb150bde", "DR/CR");
			zTextBoxColumnStyleInfo4.ColumnName = "AG_DebitCredit";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("GLAccountSelectionForm|30573404-9d72-46c7-85c1-201d4416b535", "Percent of");
			zGuidFindBoxColumnStyleInfo1.ColumnName = "AG_AG_PercentNum";
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("GLAccountSelectionForm|6c862cf1-52ef-42c1-8e54-1042a72fba7b", "Consolidate");
			zGuidFindBoxColumnStyleInfo2.ColumnName = "AG_AG_ConsolidationNum";
			zGuidFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("GLAccountSelectionForm|3f8e0e3c-6208-4aa5-8e46-cc23d040e4bb", "Alternate");
			zGuidFindBoxColumnStyleInfo3.ColumnName = "AG_AG_AlternateNum";
			zGuidFindBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("GLAccountSelectionForm|a37562df-22be-4b51-a0a5-c51088029d04", "Control");
			zCheckBoxColumnStyleInfo1.ColumnName = "AG_ControlAccount";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo2.ColumnName = "AG_IsActive";
			zCheckBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo5.ColumnName = "AG_Notes";
			zTextBoxColumnStyleInfo5.IsVisible = false;
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo4.ColumnName = "AG_AG_HeaderDependsOnTotal";
			zGuidFindBoxColumnStyleInfo4.IsVisible = false;
			zGuidFindBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo3.ColumnName = "AG_DisallowDirectPosting";
			zCheckBoxColumnStyleInfo3.IsVisible = false;
			zCheckBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo6.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("GLAccountSelectionForm|d76c24bb-6870-4b41-8ab5-301caa505823", "Current Format");
			zTextBoxColumnStyleInfo6.ColumnName = "CurrentFormat";
			zTextBoxColumnStyleInfo6.IsVisible = false;
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo7.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("GLAccountSelectionForm|71b18b07-1853-42c8-ac3d-787e46152ef5", "Section");
			zTextBoxColumnStyleInfo7.ColumnName = "AG_Column";
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("GLAccountSelectionForm|e78bfc78-83cf-4336-a0ee-936c75771661", "Section Prefix");
			zCalcEditColumnStyleInfo3.ColumnName = "SectionPrefix";
			zCalcEditColumnStyleInfo3.Decimals = 0;
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo8.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("GLAccountSelectionForm|c2e9bce6-4f33-424e-a507-80e33db0caa5", "Account For Totals");
			zTextBoxColumnStyleInfo8.ColumnName = "AG_Calc_AccountNumberWithPrefix";
			zTextBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo10.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("GLAccountSelectionForm|1f4a00db-f83f-4dec-9e8f-32506c8bc291", "Company Filters");
			zTextBoxColumnStyleInfo10.ColumnName = "CompanyFiltersAsString";
			zTextBoxColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zTextBoxColumnStyleInfo11.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("GLAccountSelectionForm|a1a2be94-2677-4fe1-a1bf-fbec29cc9cae", "Local Account Code");
			zTextBoxColumnStyleInfo11.ColumnName = "LocalAccountNumber";
			zTextBoxColumnStyleInfo11.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo12.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("GLAccountSelectionForm|de30bbf1-7bcc-4a97-8899-35106a7f4eba", "Local Account Description");
			zTextBoxColumnStyleInfo12.ColumnName = "LocalAccountDescription";
			zTextBoxColumnStyleInfo12.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zTextBoxColumnStyleInfo14.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("GLAccountSelectionForm|E0492614-4541-41F7-AD5F-EB56961EF17E", "Sub Account Types");
			zTextBoxColumnStyleInfo14.ColumnName = "AG_Calc_SubAccountTypes";
			zTextBoxColumnStyleInfo14.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.Grid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.Grid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.Grid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.Grid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.Grid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.Grid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.Grid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.Grid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo2);
			this.Grid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo3);
			this.Grid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.Grid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.Grid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.Grid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo4);
			this.Grid.ColumnStyles.Add(zCheckBoxColumnStyleInfo3);
			this.Grid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.Grid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.Grid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.Grid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.Grid.ColumnStyles.Add(zTextBoxColumnStyleInfo10);
			this.Grid.ColumnStyles.Add(zTextBoxColumnStyleInfo11);
			this.Grid.ColumnStyles.Add(zTextBoxColumnStyleInfo12);
			this.Grid.ColumnStyles.Add(zTextBoxColumnStyleInfo13);
			this.Grid.ColumnStyles.Add(zTextBoxColumnStyleInfo14);
			this.Grid.AutoSize = true;
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ButtonPanel.SuspendLayout();
			this.panelOkCancelButtons.SuspendLayout();
			this.FilterControlPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.Grid)).BeginInit();
			this.Grid.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 396, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(777, 24, true);
			this.MainStatusBar.TabIndex = 3;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.AccGLHeader);
			// 
			// MessageLabel
			// 
			this.MessageLabel.AutoSize = true;
			this.MessageLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.MessageLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(15, 15, true);
			this.MessageLabel.Name = "MessageLabel";
			this.MessageLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(211, 14, true);
			this.MessageLabel.TabIndex = 4;
			this.MessageLabel.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("GLAccountSelectionForm|7B28F0F9-F4A8-4E43-87F5-DD9702864768", "Please select a Parent Account from the list");
			// 
			// ButtonPanel
			// 
			this.ButtonPanel.Controls.Add(this.panelOkCancelButtons);
			this.ButtonPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.ButtonPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 366, true);
			this.ButtonPanel.Name = "ButtonPanel";
			this.ButtonPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(777, 30, true);
			this.ButtonPanel.TabIndex = 2;
			// 
			// panelOkCancelButtons
			// 
			this.panelOkCancelButtons.Controls.Add(this.OK_Button);
			this.panelOkCancelButtons.Controls.Add(this.Cancel_Button);
			this.panelOkCancelButtons.Dock = System.Windows.Forms.DockStyle.Right;
			this.panelOkCancelButtons.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(578, 0, true);
			this.panelOkCancelButtons.Name = "panelOkCancelButtons";
			this.panelOkCancelButtons.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(199, 30, true);
			this.panelOkCancelButtons.TabIndex = 1;
			// 
			// OK_Button
			// 
			this.OK_Button.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("GLAccountSelectionForm|dbc92c13-3561-4b8a-9c24-53fe08072017", "OK");
			this.OK_Button.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(66, 3, true);
			this.OK_Button.Name = "OK_Button";
			this.OK_Button.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(58, 23, true);
			this.OK_Button.TabIndex = 0;
			this.OK_Button.ToolTipCaption = null;
			this.OK_Button.Click += new System.EventHandler(this.OK_Button_Click);
			// 
			// Cancel_Button
			// 
			this.Cancel_Button.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("GLAccountSelectionForm|f5505cdd-eed0-4099-93b4-5edf5e83bb54", "Cancel");
			this.Cancel_Button.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(134, 3, true);
			this.Cancel_Button.Name = "Cancel_Button";
			this.Cancel_Button.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(58, 23, true);
			this.Cancel_Button.TabIndex = 1;
			this.Cancel_Button.ToolTipCaption = null;
			this.Cancel_Button.Click += new System.EventHandler(this.Cancel_Button_Click);
			// 
			// FilterControlPanel
			// 
			this.FilterControlPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.FilterControlPanel.Controls.Add(this.Grid);
			this.FilterControlPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 34, true);
			this.FilterControlPanel.Name = "FilterControlPanel";
			this.FilterControlPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(764, 327, true);
			this.FilterControlPanel.TabIndex = 1;
			// 
			// Grid
			// 
			this.Grid.AllowNavigation = false;
			this.Grid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.Grid, "GridCollection");
			this.Grid.CaptionVisible = false;
			this.Grid.GridId = "1a6fb8d4-8513-49d7-b158-c02005886626";
			this.Grid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.Grid.LayoutKey = "Grid";
			this.Grid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 5, true);
			this.Grid.Name = "Grid";
			this.Grid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(755, 320, true);
			this.Grid.TabIndex = 4;
			// 
			// GLAccountSelectionForm
			// 
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(777, 420, true);
			this.Controls.Add(this.FilterControlPanel);
			this.Controls.Add(this.MessageLabel);
			this.Controls.Add(this.ButtonPanel);
			this.DataSourceType = typeof(Enterprise.MasterFiles.Business.AccGLHeader);
			this.Name = "GLAccountSelectionForm";
			this.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("GLAccountSelectionForm|DC9AC685-2E4E-4520-B68A-DADF05A6462A", "GL Accounts");
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.ButtonPanel, 0);
			this.Controls.SetChildIndex(this.MessageLabel, 0);
			this.Controls.SetChildIndex(this.FilterControlPanel, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ButtonPanel.ResumeLayout(false);
			this.ButtonPanel.PerformLayout();
			this.panelOkCancelButtons.ResumeLayout(false);
			this.panelOkCancelButtons.PerformLayout();
			this.FilterControlPanel.ResumeLayout(false);
			this.FilterControlPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.Grid)).EndInit();
			this.Grid.ResumeLayout(false);
			this.Grid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
