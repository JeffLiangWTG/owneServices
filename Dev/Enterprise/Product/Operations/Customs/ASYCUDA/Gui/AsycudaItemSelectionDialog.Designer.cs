namespace Enterprise.Customs.ASYCUDA.GUI
{
	partial class AsycudaItemSelectionDialog
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		protected new void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.SelectedItemCountLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ButtonPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.DeselectAllButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.SelectAllButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.SendButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ButtonCancel = new Enterprise.ZArchitecture.GUI.ZButton();
			this.DescPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.ItemsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ItemsGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ButtonPanel.SuspendLayout();
			this.DescPanel.SuspendLayout();
			this.ItemsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ItemsGrid)).BeginInit();
			this.ItemsGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Dock = System.Windows.Forms.DockStyle.None;
			this.MainStatusBar.Enabled = false;
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(192, 357, true);
			this.MainStatusBar.ShowPanels = false;
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(8, 7, true);
			this.MainStatusBar.Visible = false;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.ASYCUDA.Business.MessageChooser);
			// 
			// SelectedItemCountLabel
			// 
			this.SelectedItemCountLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.SelectedItemCountLabel.AutoSize = true;
			this.BindingSource.SetBindingMember(this.SelectedItemCountLabel, "SelectedDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ASYCUDA.Business.MessageChooser)(null)).SelectedDescription)));
			this.SelectedItemCountLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.SelectedItemCountLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(101, 7, true);
			this.SelectedItemCountLabel.Name = "SelectedItemCountLabel";
			this.SelectedItemCountLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 13, true);
			this.SelectedItemCountLabel.TabIndex = 0;
			// 
			// ButtonPanel
			// 
			this.ButtonPanel.Controls.Add(this.DeselectAllButton);
			this.ButtonPanel.Controls.Add(this.SelectAllButton);
			this.ButtonPanel.Controls.Add(this.SendButton);
			this.ButtonPanel.Controls.Add(this.ButtonCancel);
			this.ButtonPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.ButtonPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 205, true);
			this.ButtonPanel.Name = "ButtonPanel";
			this.ButtonPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(465, 30, true);
			this.ButtonPanel.TabIndex = 2;
			// 
			// DeselectAllButton
			// 
			this.DeselectAllButton.CaptionResourceString = Enterprise.Customs.ASYCUDA.Gui.Res.GetData("d8712463-2e87-4677-b3b6-5301da33ccf0", "Deselect All");
			this.DeselectAllButton.IsCaptionOverridden = false;
			this.DeselectAllButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 5, true);
			this.DeselectAllButton.Name = "DeselectAllButton";
			this.DeselectAllButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.DeselectAllButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 21, true);
			this.DeselectAllButton.TabIndex = 1;
			this.DeselectAllButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.DeselectAllButton.ToolTipCaption = null;
			this.DeselectAllButton.Click += new System.EventHandler(this.DeselectAllButton_Click);
			// 
			// SelectAllButton
			// 
			this.SelectAllButton.CaptionResourceString = Enterprise.Customs.ASYCUDA.Gui.Res.GetData("13662864-e73d-49c6-97d3-71efe94c16fe", "Select All");
			this.SelectAllButton.IsCaptionOverridden = false;
			this.SelectAllButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 5, true);
			this.SelectAllButton.Name = "SelectAllButton";
			this.SelectAllButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.SelectAllButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 21, true);
			this.SelectAllButton.TabIndex = 0;
			this.SelectAllButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.SelectAllButton.ToolTipCaption = null;
			this.SelectAllButton.Click += new System.EventHandler(this.SelectAllButton_Click);
			// 
			// SendButton
			// 
			this.SendButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.SendButton.CaptionResourceString = Enterprise.Customs.ASYCUDA.Gui.Res.GetData("e9342969-8d1d-410c-9340-6703c53f29bb", "&Send");
			this.SendButton.IsCaptionOverridden = false;
			this.SendButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(289, 5, true);
			this.SendButton.Name = "SendButton";
			this.SendButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.SendButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 21, true);
			this.SendButton.TabIndex = 2;
			this.SendButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.SendButton.ToolTipCaption = null;
			this.SendButton.Click += new System.EventHandler(this.SendButton_Click);
			// 
			// ButtonCancel
			// 
			this.ButtonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.ButtonCancel.CaptionResourceString = Enterprise.Customs.ASYCUDA.Gui.Res.GetData("b8ee19c2-a6a6-423c-856f-a5d8508ccd22", "&Cancel");
			this.ButtonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.ButtonCancel.IsCaptionOverridden = false;
			this.ButtonCancel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(379, 5, true);
			this.ButtonCancel.Name = "ButtonCancel";
			this.ButtonCancel.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.ButtonCancel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 21, true);
			this.ButtonCancel.TabIndex = 3;
			this.ButtonCancel.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.ButtonCancel.ToolTipCaption = null;
			this.ButtonCancel.Click += new System.EventHandler(this.CancelButton_Click);
			// 
			// DescPanel
			// 
			this.DescPanel.Controls.Add(this.SelectedItemCountLabel);
			this.DescPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.DescPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 176, true);
			this.DescPanel.Name = "DescPanel";
			this.DescPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(465, 29, true);
			this.DescPanel.TabIndex = 1;
			// 
			// ItemsGroupBox
			// 
			this.ItemsGroupBox.Controls.Add(this.ItemsGrid);
			this.ItemsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ItemsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ItemsGroupBox.Name = "ItemsGroupBox";
			this.ItemsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(465, 176, true);
			this.ItemsGroupBox.TabIndex = 0;
			this.ItemsGroupBox.TabStop = false;
			// 
			// ItemsGrid
			// 
			this.ItemsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ItemsGrid, "ChooserItems");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.ASYCUDA.Business.MessageChooser)(null)).ChooserItems)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.ASYCUDA.Business.MessageChooserItem)(((System.Collections.IList)(((Enterprise.Customs.ASYCUDA.Business.MessageChooser)(null)).ChooserItems)).SyncRoot)).Checked)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ASYCUDA.Business.MessageChooserItem)(((System.Collections.IList)(((Enterprise.Customs.ASYCUDA.Business.MessageChooser)(null)).ChooserItems)).SyncRoot)).Description)));
			this.ItemsGrid.CaptionVisible = false;
			zCheckBoxColumnStyleInfo1.ColumnName = "Checked";
			zCheckBoxColumnStyleInfo1.IsCustomColumn = false;
			zCheckBoxColumnStyleInfo1.IsMandatory = true;
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo1.ColumnName = "Description";
			zTextBoxColumnStyleInfo1.IsCustomColumn = false;
			zTextBoxColumnStyleInfo1.IsMandatory = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(350);
			this.ItemsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.ItemsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ItemsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ItemsGrid.GridId = "7e05cf27-e428-4d82-b43d-76e1eb89b749";
			this.ItemsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ItemsGrid.LayoutKey = "zGridItems";
			this.ItemsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.ItemsGrid.Name = "ItemsGrid";
			this.ItemsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(459, 157, true);
			this.ItemsGrid.TabIndex = 0;
			// 
			// AsycudaItemSelectionDialog
			// 
			this.CancelButton = this.ButtonCancel;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(465, 235, true);
			this.Controls.Add(this.ItemsGroupBox);
			this.Controls.Add(this.DescPanel);
			this.Controls.Add(this.ButtonPanel);
			this.DataSourceAssemblyName = "Enterprise.Customs.ASYCUDA.Business";
			this.DataSourceType = typeof(Enterprise.Customs.ASYCUDA.Business.MessageChooser);
			this.DataSourceTypeName = "Enterprise.Customs.ASYCUDA.Business.MessageChooser";
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(480, 272, true);
			this.Name = "AsycudaItemSelectionDialog";
			this.Controls.SetChildIndex(this.ButtonPanel, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.DescPanel, 0);
			this.Controls.SetChildIndex(this.ItemsGroupBox, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ButtonPanel.ResumeLayout(false);
			this.ButtonPanel.PerformLayout();
			this.DescPanel.ResumeLayout(false);
			this.DescPanel.PerformLayout();
			this.ItemsGroupBox.ResumeLayout(false);
			this.ItemsGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ItemsGrid)).EndInit();
			this.ItemsGrid.ResumeLayout(false);
			this.ItemsGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		public Enterprise.ZArchitecture.GUI.ZCheckedListBox MessagesCheckedListBox;
		Enterprise.ZArchitecture.GUI.ZPanel ButtonPanel;
		Enterprise.ZArchitecture.GUI.ZButton ButtonCancel;
		internal Enterprise.ZArchitecture.GUI.ZButton SendButton;
		protected Enterprise.ZArchitecture.GUI.ZButton SelectAllButton;
		protected Enterprise.ZArchitecture.GUI.ZButton DeselectAllButton;
		protected Enterprise.ZArchitecture.ZLabel SelectedItemCountLabel;
		protected Enterprise.ZArchitecture.GUI.ZPanel DescPanel;
		protected Enterprise.ZArchitecture.ZGrid ItemsGrid;
		protected Enterprise.ZArchitecture.GUI.ZGroupBox ItemsGroupBox;
	}
}
