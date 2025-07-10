namespace Enterprise.Customs.CA.GUI
{
	partial class ShipmentCargoReportUserControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.TopTemplateTabControl = new Enterprise.ZArchitecture.GUI.ZTemplateTabControl();
			this.DeclarationTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.packingUserControl = new Enterprise.Customs.CA.GUI.PackingPlugInUserControl();
			this.TopPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.BillDetailsTabControl = new Enterprise.ZArchitecture.GUI.ZTemplateTabControl();
			this.HouseBillDetailsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.billDetailsPlugInUserControl = new Enterprise.Customs.CA.GUI.BillDetailsPlugInUserControl();
			this.OceanBillDetailsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.billDetailsSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.OriginalCCNTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.BillOfLadingZTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.containersGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.containersGrid = new Enterprise.ZArchitecture.ZGrid();
			this.MessagesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.messagesPlugInUserControl = new Enterprise.Customs.CA.GUI.MessagesPlugInUserControl();
			this.splitter1 = new CargoWise.Windows.UI.KSplitter();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.TopTemplateTabControl.SuspendLayout();
			this.DeclarationTabPage.SuspendLayout();
			this.TopPanel.SuspendLayout();
			this.BillDetailsTabControl.SuspendLayout();
			this.HouseBillDetailsTabPage.SuspendLayout();
			this.OceanBillDetailsTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.billDetailsSplitContainer)).BeginInit();
			this.billDetailsSplitContainer.Panel1.SuspendLayout();
			this.billDetailsSplitContainer.Panel2.SuspendLayout();
			this.billDetailsSplitContainer.SuspendLayout();
			this.containersGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.containersGrid)).BeginInit();
			this.MessagesTabPage.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.CA.Business.CusSCAHouse);
			// 
			// TopTemplateTabControl
			// 
			this.TopTemplateTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.TopTemplateTabControl.Controls.Add(this.DeclarationTabPage);
			this.TopTemplateTabControl.Controls.Add(this.MessagesTabPage);
			this.TopTemplateTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TopTemplateTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TopTemplateTabControl.Name = "TopTemplateTabControl";
			this.TopTemplateTabControl.SelectedIndex = 0;
			this.TopTemplateTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1062, 608, true);
			this.TopTemplateTabControl.TabIndex = 1;
			// 
			// DeclarationTabPage
			// 
			this.DeclarationTabPage.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("ShipmentCargoReportUserControl|91cab04c-6833-4d8f-8197-f4fd589967c4", "Declaration");
			this.DeclarationTabPage.Controls.Add(this.packingUserControl);
			this.DeclarationTabPage.Controls.Add(this.TopPanel);
			this.DeclarationTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.DeclarationTabPage.Name = "DeclarationTabPage";
			this.DeclarationTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.DeclarationTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1054, 581, true);
			this.DeclarationTabPage.TabIndex = 0;
			this.DeclarationTabPage.UseVisualStyleBackColor = true;
			// 
			// packingUserControl
			// 
			this.packingUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.packingUserControl, ".");
			this.packingUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.packingUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 282, true);
			this.packingUserControl.Name = "packingUserControl";
			this.packingUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1048, 296, true);
			this.packingUserControl.TabIndex = 4;
			// 
			// TopPanel
			// 
			this.TopPanel.Controls.Add(this.BillDetailsTabControl);
			this.TopPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.TopPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.TopPanel.Name = "TopPanel";
			this.TopPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1048, 279, true);
			this.TopPanel.TabIndex = 2;
			// 
			// BillDetailsTabControl
			// 
			this.BillDetailsTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.BillDetailsTabControl.Controls.Add(this.HouseBillDetailsTabPage);
			this.BillDetailsTabControl.Controls.Add(this.OceanBillDetailsTabPage);
			this.BillDetailsTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.BillDetailsTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.BillDetailsTabControl.Name = "BillDetailsTabControl";
			this.BillDetailsTabControl.SelectedIndex = 0;
			this.BillDetailsTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1048, 279, true);
			this.BillDetailsTabControl.TabIndex = 1;
			// 
			// HouseBillDetailsTabPage
			// 
			this.HouseBillDetailsTabPage.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("ShipmentCargoReportUserControl|b2f677ba-e3b3-4444-9eb6-d70264126f2d", "House Bill");
			this.HouseBillDetailsTabPage.Controls.Add(this.billDetailsPlugInUserControl);
			this.HouseBillDetailsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.HouseBillDetailsTabPage.Name = "HouseBillDetailsTabPage";
			this.HouseBillDetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1040, 252, true);
			this.HouseBillDetailsTabPage.TabIndex = 0;
			// 
			// billDetailsPlugInUserControl
			// 
			this.billDetailsPlugInUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.billDetailsPlugInUserControl, ".");
			this.billDetailsPlugInUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.billDetailsPlugInUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.billDetailsPlugInUserControl.Name = "billDetailsPlugInUserControl";
			this.billDetailsPlugInUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1040, 252, true);
			this.billDetailsPlugInUserControl.TabIndex = 0;
			// 
			// OceanBillDetailsTabPage
			// 
			this.OceanBillDetailsTabPage.Controls.Add(this.billDetailsSplitContainer);
			this.OceanBillDetailsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.OceanBillDetailsTabPage.Name = "OceanBillDetailsTabPage";
			this.OceanBillDetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1040, 252, true);
			this.OceanBillDetailsTabPage.TabIndex = 1;
			// 
			// billDetailsSplitContainer
			// 
			this.billDetailsSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.billDetailsSplitContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
			this.billDetailsSplitContainer.IsSplitterFixed = true;
			this.billDetailsSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.billDetailsSplitContainer.Name = "billDetailsSplitContainer";
			this.billDetailsSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// billDetailsSplitContainer.Panel1
			// 
			this.billDetailsSplitContainer.Panel1.Controls.Add(this.OriginalCCNTextBox);
			this.billDetailsSplitContainer.Panel1.Controls.Add(this.BillOfLadingZTextBox);
			// 
			// billDetailsSplitContainer.Panel2
			// 
			this.billDetailsSplitContainer.Panel2.Controls.Add(this.containersGroupBox);
			this.billDetailsSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1040, 252, true);
			this.billDetailsSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			this.billDetailsSplitContainer.TabIndex = 23;
			// 
			// OriginalCCNTextBox
			// 
			this.OriginalCCNTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.OriginalCCNTextBox, "OriginalCCN");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.CusSCAHouse)(null)).OriginalCCN)));
			this.OriginalCCNTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("ShipmentCargoReportUserControl|bb3d6240-17a4-4486-9f37-3e36dc32e16a", "CCN", "Original  CCN", "Original Cargo Control Number", "The original CCN is used in the submission of a supplementary cargo report to reference the prime cargo report issued by the prime carrier.");
			this.OriginalCCNTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(82, 14, true);
			this.OriginalCCNTextBox.Name = "OriginalCCNTextBox";
			this.OriginalCCNTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(134, 20, true);
			this.OriginalCCNTextBox.TabIndex = 21;
			// 
			// BillOfLadingZTextBox
			// 
			this.BillOfLadingZTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.BillOfLadingZTextBox, "BillOfLading");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.CusSCAHouse)(null)).BillOfLading)));
			this.BillOfLadingZTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("ShipmentCargoReportUserControl|61ea58b6-36c9-4a8c-96c2-56e7721cdba3", "Master Bill");
			this.BillOfLadingZTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(82, 40, true);
			this.BillOfLadingZTextBox.Name = "BillOfLadingZTextBox";
			this.BillOfLadingZTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(134, 20, true);
			this.BillOfLadingZTextBox.TabIndex = 22;
			// 
			// containersGroupBox
			// 
			this.containersGroupBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("5cd9a198-1f63-4481-8d77-c928aa9d94fa", "Containers");
			this.containersGroupBox.Controls.Add(this.containersGrid);
			this.containersGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.containersGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.containersGroupBox.Name = "containersGroupBox";
			this.containersGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1040, 178, true);
			this.containersGroupBox.TabIndex = 0;
			this.containersGroupBox.TabStop = false;
			// 
			// containersGrid
			// 
			this.containersGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.containersGrid, "OceanBill.Containers");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.CA.Business.CusSCAHouse)(null)).OceanBill.Containers)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.CusSCAContainer)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.CusSCAHouse)(null)).OceanBill.Containers)).SyncRoot)).CN_ContainerNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.CusSCAContainer)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.CusSCAHouse)(null)).OceanBill.Containers)).SyncRoot)).CN_ContainerMode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.CusSCAContainer)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.CusSCAHouse)(null)).OceanBill.Containers)).SyncRoot)).CN_RC_NKContainerType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.CusSCAContainer)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.CusSCAHouse)(null)).OceanBill.Containers)).SyncRoot)).CN_RN_NKCountryOfRegistration)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.CusSCAContainer)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.CusSCAHouse)(null)).OceanBill.Containers)).SyncRoot)).CN_ContainerSizeOrISOCode)));
			this.containersGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "CN_ContainerNumber";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(111);
			zDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo1.ColumnName = "CN_ContainerMode";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(39);
			zCodeFindBoxColumnStyleInfo1.ColumnName = "CN_RC_NKContainerType";
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(47);
			zCodeFindBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("b29213f6-b1d0-493d-ba86-e6954580a13d", "Reg. Country/Region");
			zCodeFindBoxColumnStyleInfo2.ColumnName = "CN_RN_NKCountryOfRegistration";
			zCodeFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(106);
			zTextBoxColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo2.ColumnName = "CN_ContainerSizeOrISOCode";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(92);
			this.containersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.containersGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.containersGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.containersGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo2);
			this.containersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.containersGrid.CopySelectedRowsAllowed = true;
			this.containersGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.containersGrid.GridId = "3692e631-314e-49c8-b006-30b93777f983";
			this.containersGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.containersGrid.LayoutKey = "containersGrid";
			this.containersGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.containersGrid.Name = "containersGrid";
			this.containersGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1034, 159, true);
			this.containersGrid.TabIndex = 0;
			// 
			// MessagesTabPage
			// 
			this.MessagesTabPage.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("ShipmentCargoReportUserControl|0a9274ee-ccba-432a-ac77-0806b7c0bc61", "Messages");
			this.MessagesTabPage.Controls.Add(this.messagesPlugInUserControl);
			this.MessagesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.MessagesTabPage.Name = "MessagesTabPage";
			this.MessagesTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.MessagesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1054, 581, true);
			this.MessagesTabPage.TabIndex = 1;
			this.MessagesTabPage.UseVisualStyleBackColor = true;
			// 
			// messagesPlugInUserControl
			// 
			this.messagesPlugInUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.messagesPlugInUserControl, ".");
			this.messagesPlugInUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.messagesPlugInUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.messagesPlugInUserControl.Name = "messagesPlugInUserControl";
			this.messagesPlugInUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1048, 575, true);
			this.messagesPlugInUserControl.TabIndex = 1;
			// 
			// splitter1
			// 
			this.splitter1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.splitter1.Dock = System.Windows.Forms.DockStyle.Right;
			this.splitter1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(690, 16, true);
			this.splitter1.Name = "splitter1";
			this.splitter1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(5, 556, true);
			this.splitter1.TabIndex = 3;
			this.splitter1.TabStop = false;
			// 
			// ShipmentCargoReportUserControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.TopTemplateTabControl);
			this.Name = "ShipmentCargoReportUserControl";
			this.ShouldSerializeTabPageMethods = false;
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1062, 608, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.TopTemplateTabControl.ResumeLayout(false);
			this.DeclarationTabPage.ResumeLayout(false);
			this.TopPanel.ResumeLayout(false);
			this.BillDetailsTabControl.ResumeLayout(false);
			this.HouseBillDetailsTabPage.ResumeLayout(false);
			this.OceanBillDetailsTabPage.ResumeLayout(false);
			this.billDetailsSplitContainer.Panel1.ResumeLayout(false);
			this.billDetailsSplitContainer.Panel1.PerformLayout();
			this.billDetailsSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.billDetailsSplitContainer)).EndInit();
			this.billDetailsSplitContainer.ResumeLayout(false);
			this.containersGroupBox.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.containersGrid)).EndInit();
			this.MessagesTabPage.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZTemplateTabControl TopTemplateTabControl;
		private Enterprise.ZArchitecture.GUI.ZTabPage DeclarationTabPage;
		private CargoWise.Windows.UI.KSplitter splitter1;
		private Enterprise.ZArchitecture.GUI.ZPanel TopPanel;
		protected internal Enterprise.ZArchitecture.GUI.ZTemplateTabControl BillDetailsTabControl;
		protected internal Enterprise.ZArchitecture.GUI.ZTabPage HouseBillDetailsTabPage;
		protected internal Enterprise.ZArchitecture.GUI.ZTabPage OceanBillDetailsTabPage;
		private Enterprise.ZArchitecture.GUI.ZTabPage MessagesTabPage;
		private Enterprise.ZArchitecture.ZTextBox OriginalCCNTextBox;
		private Enterprise.ZArchitecture.ZTextBox BillOfLadingZTextBox;
		private MessagesPlugInUserControl messagesPlugInUserControl;
		private PackingPlugInUserControl packingUserControl;
		private BillDetailsPlugInUserControl billDetailsPlugInUserControl;
		private CargoWise.Windows.UI.KSplitContainer billDetailsSplitContainer;
		private ZArchitecture.GUI.ZGroupBox containersGroupBox;
		internal ZArchitecture.ZGrid containersGrid;
	}
}
