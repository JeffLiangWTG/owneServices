namespace Enterprise.Customs.JP.AFR.GUI
{
	partial class JPAFRBLLFunctionForm
	{
		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		protected new void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.BottomPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.cancelButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.SendButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.MainControlPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.selectedGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.selectedGrid = new Enterprise.ZArchitecture.ZGrid();
			this.registeredGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.availableGrid = new Enterprise.ZArchitecture.ZGrid();
			this.changeResonDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.billNumberDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.removeButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.addButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.BottomPanel.SuspendLayout();
			this.MainControlPanel.SuspendLayout();
			this.selectedGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.selectedGrid)).BeginInit();
			this.selectedGrid.SuspendLayout();
			this.registeredGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.availableGrid)).BeginInit();
			this.availableGrid.SuspendLayout();
			this.changeResonDropEdit.SuspendLayout();
			this.billNumberDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 487, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(670, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.JP.AFR.Business.BLLFunction);
			// 
			// BottomPanel
			// 
			this.BottomPanel.Controls.Add(this.cancelButton);
			this.BottomPanel.Controls.Add(this.SendButton);
			this.BottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.BottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 450, true);
			this.BottomPanel.Name = "BottomPanel";
			this.BottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(670, 37, true);
			this.BottomPanel.TabIndex = 6;
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.CaptionResourceString = Enterprise.Customs.JP.AFR.GUI.Res.GetData("6567013e-fe9c-43e8-ad8f-6fdbf0d5cf11", "&Cancel");
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(584, 6, true);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.cancelButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.cancelButton.TabIndex = 16;
			this.cancelButton.ToolTipCaption = null;
			this.cancelButton.UseVisualStyleBackColor = true;
			this.cancelButton.Click += new System.EventHandler(this.CancelButton_Click);
			// 
			// SendButton
			// 
			this.SendButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.SendButton.CaptionResourceString = Enterprise.Customs.JP.AFR.GUI.Res.GetData("10095b3b-d4cb-4eed-a1ea-2a1a9d529112", "&Send");
			this.SendButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(468, 6, true);
			this.SendButton.Name = "SendButton";
			this.SendButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.SendButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(109, 23, true);
			this.SendButton.TabIndex = 15;
			this.SendButton.ToolTipCaption = null;
			this.SendButton.UseVisualStyleBackColor = true;
			this.SendButton.Click += new System.EventHandler(this.SendButton_Click);
			// 
			// MainControlPanel
			// 
			this.MainControlPanel.Controls.Add(this.selectedGroupBox);
			this.MainControlPanel.Controls.Add(this.registeredGroupBox);
			this.MainControlPanel.Controls.Add(this.changeResonDropEdit);
			this.MainControlPanel.Controls.Add(this.billNumberDropEdit);
			this.MainControlPanel.Controls.Add(this.removeButton);
			this.MainControlPanel.Controls.Add(this.addButton);
			this.MainControlPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MainControlPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MainControlPanel.Name = "MainControlPanel";
			this.MainControlPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(670, 450, true);
			this.MainControlPanel.TabIndex = 1;
			// 
			// selectedGroupBox
			// 
			this.selectedGroupBox.Controls.Add(this.selectedGrid);
			this.selectedGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(419, 98, true);
			this.selectedGroupBox.Name = "selectedGroupBox";
			this.selectedGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(230, 330, true);
			this.selectedGroupBox.TabIndex = 25;
			this.selectedGroupBox.TabStop = false;
			// 
			// selectedGrid
			// 
			this.selectedGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.selectedGrid, "SelectedBills");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.JP.AFR.Business.BLLFunction)(null)).SelectedBills)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.AFR.Business.BLLFunctionBill)(((System.Collections.IList)(((Enterprise.Customs.JP.AFR.Business.BLLFunction)(null)).SelectedBills)).SyncRoot)).JPM_BillOfLadingNumber)));
			this.selectedGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.JP.AFR.GUI.Res.GetData("eabbf5b1-0270-4a44-b4ba-f7e13c8b1b0d", "Bill No.");
			zTextBoxColumnStyleInfo1.ColumnName = "JPM_BillOfLadingNumber";
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.IsSortable = false;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			this.selectedGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.selectedGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.selectedGrid.GridId = "a32da0e1-592e-4429-8057-1a237a3a6403";
			this.selectedGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.selectedGrid.IsCustomiseMenuVisible = false;
			this.selectedGrid.LayoutKey = "selectedGrid";
			this.selectedGrid.LimitedColumns = null;
			this.selectedGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.selectedGrid.Name = "selectedGrid";
			this.selectedGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(224, 311, true);
			this.selectedGrid.TabIndex = 6;
			// 
			// registeredGroupBox
			// 
			this.registeredGroupBox.CaptionResourceString = Enterprise.Customs.JP.AFR.GUI.Res.GetData("c21db830-d4cc-48e3-96a9-29d7ec53aa53", "Registered Bill(s)");
			this.registeredGroupBox.Controls.Add(this.availableGrid);
			this.registeredGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(28, 98, true);
			this.registeredGroupBox.Name = "registeredGroupBox";
			this.registeredGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(230, 330, true);
			this.registeredGroupBox.TabIndex = 24;
			this.registeredGroupBox.TabStop = false;
			// 
			// availableGrid
			// 
			this.availableGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.availableGrid, "AvailableBills");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.JP.AFR.Business.BLLFunction)(null)).AvailableBills)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.AFR.Business.BLLFunctionBill)(((System.Collections.IList)(((Enterprise.Customs.JP.AFR.Business.BLLFunction)(null)).AvailableBills)).SyncRoot)).JPM_BillOfLadingNumber)));
			this.availableGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.JP.AFR.GUI.Res.GetData("23af67b8-90c3-4e15-bfdb-0f8c52d7642f", "Bill No.");
			zTextBoxColumnStyleInfo2.ColumnName = "JPM_BillOfLadingNumber";
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.IsSortable = false;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			this.availableGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.availableGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.availableGrid.GridId = "732b44ac-b093-43c4-844b-e52d64956173";
			this.availableGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.availableGrid.IsCustomiseMenuVisible = false;
			this.availableGrid.LayoutKey = "availableGrid";
			this.availableGrid.LimitedColumns = null;
			this.availableGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.availableGrid.Name = "availableGrid";
			this.availableGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(224, 311, true);
			this.availableGrid.TabIndex = 3;
			// 
			// changeResonDropEdit
			// 
			this.changeResonDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.changeResonDropEdit, "JPM_ChangeReasonCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.JP.AFR.Business.BLLFunction)(null)).JPM_ChangeReasonCode)));
			this.changeResonDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(183, 57, true);
			this.changeResonDropEdit.Name = "changeResonDropEdit";
			this.changeResonDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.changeResonDropEdit.TabIndex = 2;
			// 
			// billNumberDropEdit
			// 
			this.billNumberDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.billNumberDropEdit, "JPM_BillOfLadingNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.JP.AFR.Business.BLLFunction)(null)).JPM_BillOfLadingNumber)));
			this.billNumberDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(183, 24, true);
			this.billNumberDropEdit.Name = "billNumberDropEdit";
			this.billNumberDropEdit.ShowDescriptionBox = false;
			this.billNumberDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.billNumberDropEdit.TabIndex = 1;
			// 
			// removeButton
			// 
			this.removeButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(280, 243, true);
			this.removeButton.Name = "removeButton";
			this.removeButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.removeButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(105, 23, true);
			this.removeButton.TabIndex = 5;
			this.removeButton.Text = "<-----";
			this.removeButton.ToolTipCaption = null;
			this.removeButton.UseVisualStyleBackColor = true;
			this.removeButton.Click += new System.EventHandler(this.RemoveButton_Click);
			// 
			// addButton
			// 
			this.addButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(280, 169, true);
			this.addButton.Name = "addButton";
			this.addButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.addButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(105, 23, true);
			this.addButton.TabIndex = 4;
			this.addButton.Text = "----->";
			this.addButton.ToolTipCaption = null;
			this.addButton.UseVisualStyleBackColor = true;
			this.addButton.Click += new System.EventHandler(this.AddButton_Click);
			// 
			// JPAFRBLLFunctionForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(670, 511, true);
			this.Controls.Add(this.MainControlPanel);
			this.Controls.Add(this.BottomPanel);
			this.DataSourceType = typeof(Enterprise.Customs.JP.AFR.Business.BLLFunction);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MinimizeBox = false;
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(686, 550, true);
			this.Name = "JPAFRBLLFunctionForm";
			this.Text = "JPAFRMessageSendingActionForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.BottomPanel, 0);
			this.Controls.SetChildIndex(this.MainControlPanel, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.BottomPanel.ResumeLayout(false);
			this.BottomPanel.PerformLayout();
			this.MainControlPanel.ResumeLayout(false);
			this.MainControlPanel.PerformLayout();
			this.selectedGroupBox.ResumeLayout(false);
			this.selectedGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.selectedGrid)).EndInit();
			this.selectedGrid.ResumeLayout(false);
			this.selectedGrid.PerformLayout();
			this.registeredGroupBox.ResumeLayout(false);
			this.registeredGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.availableGrid)).EndInit();
			this.availableGrid.ResumeLayout(false);
			this.availableGrid.PerformLayout();
			this.changeResonDropEdit.ResumeLayout(true);
			this.changeResonDropEdit.PerformLayout();
			this.billNumberDropEdit.ResumeLayout(true);
			this.billNumberDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private ZArchitecture.GUI.ZPanel BottomPanel;
		private ZArchitecture.GUI.ZButton cancelButton;
		private ZArchitecture.GUI.ZButton SendButton;
		private ZArchitecture.GUI.ZPanel MainControlPanel;
		private ZArchitecture.GUI.ZButton removeButton;
		private ZArchitecture.GUI.ZButton addButton;
		private ZArchitecture.ZGrid selectedGrid;
		private ZArchitecture.ZGrid availableGrid;
		private ZArchitecture.GUI.ZDropEdit changeResonDropEdit;
		private ZArchitecture.GUI.ZDropEdit billNumberDropEdit;
		private ZArchitecture.GUI.ZGroupBox selectedGroupBox;
		private ZArchitecture.GUI.ZGroupBox registeredGroupBox;
	}
}
