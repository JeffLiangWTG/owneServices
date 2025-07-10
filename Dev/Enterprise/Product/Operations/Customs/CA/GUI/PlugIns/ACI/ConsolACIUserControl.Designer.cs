namespace Enterprise.Customs.CA.GUI
{
	partial class ConsolACIUserControl
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
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo10 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo11 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo12 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo13 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			this.packingUserControl = new Enterprise.Customs.CA.GUI.PackingPlugInUserControl();
			this.messagesPlugInUserControl = new Enterprise.Customs.CA.GUI.MessagesPlugInUserControl();
			this.billDetailsPlugInUserControl = new Enterprise.Customs.CA.GUI.BillDetailsPlugInUserControl();
			this.splitter1 = new CargoWise.Windows.UI.KSplitter();
			this.CommonGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.OriginalCCNTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.BillOfLadingZTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.BillOfLadingTabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.DetailsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.PackingTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.MessagesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.HouseBillsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.HouseBillsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.containersGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.containersGrid = new Enterprise.ZArchitecture.ZGrid();
			this.housesAndContainersSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.consolACISplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.CommonGroupBox.SuspendLayout();
			this.BillOfLadingTabControl.SuspendLayout();
			this.DetailsTabPage.SuspendLayout();
			this.PackingTabPage.SuspendLayout();
			this.MessagesTabPage.SuspendLayout();
			this.HouseBillsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.HouseBillsGrid)).BeginInit();
			this.containersGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.containersGrid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.housesAndContainersSplitContainer)).BeginInit();
			this.housesAndContainersSplitContainer.Panel1.SuspendLayout();
			this.housesAndContainersSplitContainer.Panel2.SuspendLayout();
			this.housesAndContainersSplitContainer.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.consolACISplitContainer)).BeginInit();
			this.consolACISplitContainer.Panel1.SuspendLayout();
			this.consolACISplitContainer.Panel2.SuspendLayout();
			this.consolACISplitContainer.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.CA.Business.CusSCAOceanBill);
			// 
			// packingUserControl
			// 
			this.packingUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.packingUserControl, "HouseBills");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.CA.Business.CusSCAHouse)(((Enterprise.Customs.CA.Business.CusSCAHouse)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.CusSCAOceanBill)(null)).HouseBills)).SyncRoot)))));
			this.packingUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.packingUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.packingUserControl.Name = "packingUserControl";
			this.packingUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(994, 289, true);
			this.packingUserControl.TabIndex = 0;
			// 
			// messagesPlugInUserControl
			// 
			this.messagesPlugInUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.messagesPlugInUserControl, "HouseBills");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.CA.Business.CusSCAHouse)(((Enterprise.Customs.CA.Business.CusSCAHouse)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.CusSCAOceanBill)(null)).HouseBills)).SyncRoot)))));
			this.messagesPlugInUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.messagesPlugInUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.messagesPlugInUserControl.Name = "messagesPlugInUserControl";
			this.messagesPlugInUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1000, 295, true);
			this.messagesPlugInUserControl.TabIndex = 0;
			// 
			// billDetailsPlugInUserControl
			// 
			this.billDetailsPlugInUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.billDetailsPlugInUserControl, "HouseBills");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.CA.Business.CusSCAHouse)(((Enterprise.Customs.CA.Business.CusSCAHouse)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.CusSCAOceanBill)(null)).HouseBills)).SyncRoot)))));
			this.billDetailsPlugInUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.billDetailsPlugInUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.billDetailsPlugInUserControl.Name = "billDetailsPlugInUserControl";
			this.billDetailsPlugInUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(994, 289, true);
			this.billDetailsPlugInUserControl.TabIndex = 0;
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
			// CommonGroupBox
			// 
			this.CommonGroupBox.Controls.Add(this.OriginalCCNTextBox);
			this.CommonGroupBox.Controls.Add(this.BillOfLadingZTextBox);
			this.CommonGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.CommonGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.CommonGroupBox.Name = "CommonGroupBox";
			this.CommonGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1008, 42, true);
			this.CommonGroupBox.TabIndex = 0;
			this.CommonGroupBox.TabStop = false;
			// 
			// OriginalCCNTextBox
			// 
			this.OriginalCCNTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.OriginalCCNTextBox, "OriginalCCN");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.CusSCAOceanBill)(null)).OriginalCCN)));
			this.OriginalCCNTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("ConsolACIUserControl|c33d128e-00cf-463d-9f9d-02f9b041864e", "CCN", "Original  CCN", "Original Cargo Control Number", "The original CCN is used in the submission of a supplementary cargo report to reference the prime cargo report issued by the prime carrier.");
			this.OriginalCCNTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(259, 15, true);
			this.OriginalCCNTextBox.Name = "OriginalCCNTextBox";
			this.OriginalCCNTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(134, 20, true);
			this.OriginalCCNTextBox.TabIndex = 0;
			// 
			// BillOfLadingZTextBox
			// 
			this.BillOfLadingZTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.BillOfLadingZTextBox, "CB_OceanBill");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.CusSCAOceanBill)(null)).CB_OceanBill)));
			this.BillOfLadingZTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(518, 15, true);
			this.BillOfLadingZTextBox.Name = "BillOfLadingZTextBox";
			this.BillOfLadingZTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(134, 20, true);
			this.BillOfLadingZTextBox.TabIndex = 1;
			// 
			// BillOfLadingTabControl
			// 
			this.BillOfLadingTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.BillOfLadingTabControl.Controls.Add(this.DetailsTabPage);
			this.BillOfLadingTabControl.Controls.Add(this.PackingTabPage);
			this.BillOfLadingTabControl.Controls.Add(this.MessagesTabPage);
			this.BillOfLadingTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.BillOfLadingTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.BillOfLadingTabControl.Name = "BillOfLadingTabControl";
			this.BillOfLadingTabControl.SelectedIndex = 0;
			this.BillOfLadingTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1008, 322, true);
			this.BillOfLadingTabControl.TabIndex = 2;
			// 
			// DetailsTabPage
			// 
			this.DetailsTabPage.Controls.Add(this.billDetailsPlugInUserControl);
			this.DetailsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.DetailsTabPage.Name = "DetailsTabPage";
			this.DetailsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.DetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1000, 295, true);
			this.DetailsTabPage.TabIndex = 0;
			this.DetailsTabPage.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("cdfed084-cbf9-46c3-afea-1af42a0f7f6d", "Details");
			// 
			// PackingTabPage
			// 
			this.PackingTabPage.Controls.Add(this.packingUserControl);
			this.PackingTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.PackingTabPage.Name = "PackingTabPage";
			this.PackingTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.PackingTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1000, 295, true);
			this.PackingTabPage.TabIndex = 1;
			this.PackingTabPage.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("1af9b7ee-7fe3-4ae3-bf27-a353ef467338", "Packing");
			// 
			// MessagesTabPage
			// 
			this.MessagesTabPage.Controls.Add(this.messagesPlugInUserControl);
			this.MessagesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.MessagesTabPage.Name = "MessagesTabPage";
			this.MessagesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1000, 295, true);
			this.MessagesTabPage.TabIndex = 2;
			this.MessagesTabPage.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("7c4d8b6b-d759-4ced-ad9d-b205c799f61a", "Messages");
			// 
			// HouseBillsGroupBox
			// 
			this.HouseBillsGroupBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("8ffb3b23-5e23-452e-808b-5301c89fa2f2", "House Bills");
			this.HouseBillsGroupBox.Controls.Add(this.HouseBillsGrid);
			this.HouseBillsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.HouseBillsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.HouseBillsGroupBox.Name = "HouseBillsGroupBox";
			this.HouseBillsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1008, 152, true);
			this.HouseBillsGroupBox.TabIndex = 1;
			this.HouseBillsGroupBox.TabStop = false;
			// 
			// HouseBillsGrid
			// 
			this.HouseBillsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.HouseBillsGrid, "HouseBills");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.CA.Business.CusSCAOceanBill)(null)).HouseBills)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.CusSCAHouse)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.CusSCAOceanBill)(null)).HouseBills)).SyncRoot)).CA_HouseBill)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.CusSCAHouse)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.CusSCAOceanBill)(null)).HouseBills)).SyncRoot)).SupplementaryReferenceNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.CusSCAHouse)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.CusSCAOceanBill)(null)).HouseBills)).SyncRoot)).CA_ShipmentStatus)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.CusSCAHouse)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.CusSCAOceanBill)(null)).HouseBills)).SyncRoot)).ShipmentStatusDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.CusSCAHouse)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.CusSCAOceanBill)(null)).HouseBills)).SyncRoot)).CA_MessageStatus)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.CusSCAHouse)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.CusSCAOceanBill)(null)).HouseBills)).SyncRoot)).MessageStatusDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.CusSCAHouse)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.CusSCAOceanBill)(null)).HouseBills)).SyncRoot)).CA_BGMReference)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.CusSCAHouse)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.CusSCAOceanBill)(null)).HouseBills)).SyncRoot)).CA_ConsigneeName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.CusSCAHouse)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.CusSCAOceanBill)(null)).HouseBills)).SyncRoot)).CA_ConsignorName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.CusSCAHouse)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.CusSCAOceanBill)(null)).HouseBills)).SyncRoot)).CA_RL_NK_PortOfOrigin)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.CusSCAHouse)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.CusSCAOceanBill)(null)).HouseBills)).SyncRoot)).CA_RL_NK_PortOfDestination)));
			this.HouseBillsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.Caption = "Bill Number";
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "CA_HouseBill";
			zTextBoxColumnStyleInfo1.IsMandatory = true;
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.Caption = "Sup Reference Number";
			zTextBoxColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo2.ColumnName = "SupplementaryReferenceNumber";
			zTextBoxColumnStyleInfo2.IsMandatory = true;
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo3.Caption = "Shipment Status";
			zTextBoxColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo3.ColumnName = "CA_ShipmentStatus";
			zTextBoxColumnStyleInfo3.IsMandatory = true;
			zTextBoxColumnStyleInfo3.IsReadOnly = true;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo4.Caption = "Shipment Status Description";
			zTextBoxColumnStyleInfo4.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo4.ColumnName = "ShipmentStatusDescription";
			zTextBoxColumnStyleInfo4.IsReadOnly = true;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(145);
			zTextBoxColumnStyleInfo5.Caption = "Message Status";
			zTextBoxColumnStyleInfo5.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo5.ColumnName = "CA_MessageStatus";
			zTextBoxColumnStyleInfo5.IsMandatory = true;
			zTextBoxColumnStyleInfo5.IsReadOnly = true;
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zTextBoxColumnStyleInfo6.Caption = "Message Status Description";
			zTextBoxColumnStyleInfo6.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo6.ColumnName = "MessageStatusDescription";
			zTextBoxColumnStyleInfo6.IsReadOnly = true;
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140);
			zTextBoxColumnStyleInfo7.Caption = "Job Number";
			zTextBoxColumnStyleInfo7.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo7.ColumnName = "CA_BGMReference";
			zTextBoxColumnStyleInfo7.IsMandatory = true;
			zTextBoxColumnStyleInfo7.IsReadOnly = true;
			zTextBoxColumnStyleInfo8.Caption = "Consignee";
			zTextBoxColumnStyleInfo8.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo8.ColumnName = "CA_ConsigneeName";
			zTextBoxColumnStyleInfo8.IsMandatory = true;
			zTextBoxColumnStyleInfo8.IsReadOnly = true;
			zTextBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140);
			zTextBoxColumnStyleInfo9.Caption = "Shipper";
			zTextBoxColumnStyleInfo9.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo9.ColumnName = "CA_ConsignorName";
			zTextBoxColumnStyleInfo9.IsMandatory = true;
			zTextBoxColumnStyleInfo9.IsReadOnly = true;
			zTextBoxColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140);
			zTextBoxColumnStyleInfo10.Caption = "Origin";
			zTextBoxColumnStyleInfo10.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo10.ColumnName = "CA_RL_NK_PortOfOrigin";
			zTextBoxColumnStyleInfo10.IsMandatory = true;
			zTextBoxColumnStyleInfo10.IsReadOnly = true;
			zTextBoxColumnStyleInfo11.Caption = "Destination";
			zTextBoxColumnStyleInfo11.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo11.ColumnName = "CA_RL_NK_PortOfDestination";
			zTextBoxColumnStyleInfo11.IsMandatory = true;
			zTextBoxColumnStyleInfo11.IsReadOnly = true;
			this.HouseBillsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.HouseBillsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.HouseBillsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.HouseBillsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.HouseBillsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.HouseBillsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.HouseBillsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.HouseBillsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.HouseBillsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
			this.HouseBillsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo10);
			this.HouseBillsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo11);
			this.HouseBillsGrid.CopySelectedRowsAllowed = true;
			this.HouseBillsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.HouseBillsGrid.GridId = "9fdaa7fa-8551-49a5-8c57-8899985df5c3";
			this.HouseBillsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.HouseBillsGrid.LayoutKey = "HouseBillsGrid";
			this.HouseBillsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.HouseBillsGrid.Name = "HouseBillsGrid";
			this.HouseBillsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1002, 133, true);
			this.HouseBillsGrid.TabIndex = 0;
			// 
			// containersGroupBox
			// 
			this.containersGroupBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("7f41f70b-c95f-4d03-86de-1e2198c6f2ec", "Containers");
			this.containersGroupBox.Controls.Add(this.containersGrid);
			this.containersGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.containersGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.containersGroupBox.Name = "containersGroupBox";
			this.containersGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1008, 84, true);
			this.containersGroupBox.TabIndex = 3;
			this.containersGroupBox.TabStop = false;
			this.containersGroupBox.Text = "Containers";
			// 
			// containersGrid
			// 
			this.containersGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.containersGrid, "Containers");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.CA.Business.CusSCAOceanBill)(null)).Containers)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.CusSCAContainer)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.CusSCAOceanBill)(null)).Containers)).SyncRoot)).CN_ContainerNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.CusSCAContainer)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.CusSCAOceanBill)(null)).Containers)).SyncRoot)).CN_ContainerMode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.CusSCAContainer)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.CusSCAOceanBill)(null)).Containers)).SyncRoot)).CN_RC_NKContainerType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.CusSCAContainer)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.CusSCAOceanBill)(null)).Containers)).SyncRoot)).CN_ContainerSizeOrISOCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.CusSCAContainer)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.CusSCAOceanBill)(null)).Containers)).SyncRoot)).CN_RN_NKCountryOfRegistration)));
			this.containersGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo12.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo12.ColumnName = "CN_ContainerNumber";
			zTextBoxColumnStyleInfo12.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(111);
			zDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo1.ColumnName = "CN_ContainerMode";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(49);
			zCodeFindBoxColumnStyleInfo1.ColumnName = "CN_RC_NKContainerType";
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(47);
			zTextBoxColumnStyleInfo13.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo13.ColumnName = "CN_ContainerSizeOrISOCode";
			zTextBoxColumnStyleInfo13.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(92);
			zCodeFindBoxColumnStyleInfo2.ColumnName = "CN_RN_NKCountryOfRegistration";
			zCodeFindBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("B4BF4D0F-61BF-4A4C-A3FA-96409D356DCE", "Reg. Country/Region");
			zCodeFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(106);
			this.containersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo12);
			this.containersGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.containersGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.containersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo13);
			this.containersGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo2);
			this.containersGrid.CopySelectedRowsAllowed = true;
			this.containersGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.containersGrid.GridId = "7a13bd96-355d-44e9-b108-ae2b499e464b";
			this.containersGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.containersGrid.LayoutKey = "containersGrid";
			this.containersGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.containersGrid.Name = "containersGrid";
			this.containersGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1002, 65, true);
			this.containersGrid.TabIndex = 0;
			// 
			// housesAndContainersSplitContainer
			// 
			this.housesAndContainersSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.housesAndContainersSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.housesAndContainersSplitContainer.Name = "housesAndContainersSplitContainer";
			this.housesAndContainersSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// housesAndContainersSplitContainer.Panel1
			// 
			this.housesAndContainersSplitContainer.Panel1.Controls.Add(this.containersGroupBox);
			// 
			// housesAndContainersSplitContainer.Panel2
			// 
			this.housesAndContainersSplitContainer.Panel2.Controls.Add(this.HouseBillsGroupBox);
			this.housesAndContainersSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1008, 240, true);
			this.housesAndContainersSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(84);
			this.housesAndContainersSplitContainer.TabIndex = 1;
			// 
			// consolACISplitContainer
			// 
			this.consolACISplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.consolACISplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 42, true);
			this.consolACISplitContainer.Name = "consolACISplitContainer";
			this.consolACISplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// consolACISplitContainer.Panel1
			// 
			this.consolACISplitContainer.Panel1.Controls.Add(this.housesAndContainersSplitContainer);
			// 
			// consolACISplitContainer.Panel2
			// 
			this.consolACISplitContainer.Panel2.Controls.Add(this.BillOfLadingTabControl);
			this.consolACISplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1008, 566, true);
			this.consolACISplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(240);
			this.consolACISplitContainer.TabIndex = 3;
			// 
			// ConsolACIUserControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.consolACISplitContainer);
			this.Controls.Add(this.CommonGroupBox);
			this.Name = "ConsolACIUserControl";
			this.ShouldSerializeTabPageMethods = false;
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1008, 608, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.CommonGroupBox.ResumeLayout(false);
			this.CommonGroupBox.PerformLayout();
			this.BillOfLadingTabControl.ResumeLayout(false);
			this.DetailsTabPage.ResumeLayout(false);
			this.PackingTabPage.ResumeLayout(false);
			this.MessagesTabPage.ResumeLayout(false);
			this.HouseBillsGroupBox.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.HouseBillsGrid)).EndInit();
			this.containersGroupBox.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.containersGrid)).EndInit();
			this.housesAndContainersSplitContainer.Panel1.ResumeLayout(false);
			this.housesAndContainersSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.housesAndContainersSplitContainer)).EndInit();
			this.housesAndContainersSplitContainer.ResumeLayout(false);
			this.consolACISplitContainer.Panel1.ResumeLayout(false);
			this.consolACISplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.consolACISplitContainer)).EndInit();
			this.consolACISplitContainer.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private CargoWise.Windows.UI.KSplitter splitter1;
		public ZArchitecture.GUI.ZGroupBox CommonGroupBox;
		private ZArchitecture.ZTextBox OriginalCCNTextBox;
		private ZArchitecture.ZTextBox BillOfLadingZTextBox;
		private ZArchitecture.GUI.ZTabControl BillOfLadingTabControl;
		private ZArchitecture.GUI.ZTabPage DetailsTabPage;
		private ZArchitecture.GUI.ZTabPage PackingTabPage;
		private ZArchitecture.GUI.ZTabPage MessagesTabPage;
		public ZArchitecture.GUI.ZGroupBox HouseBillsGroupBox;
		public ZArchitecture.ZGrid HouseBillsGrid;
		PackingPlugInUserControl packingUserControl;
		MessagesPlugInUserControl messagesPlugInUserControl;
		BillDetailsPlugInUserControl billDetailsPlugInUserControl;
		private ZArchitecture.GUI.ZGroupBox containersGroupBox;
		internal ZArchitecture.ZGrid containersGrid;
		private CargoWise.Windows.UI.KSplitContainer housesAndContainersSplitContainer;
		private CargoWise.Windows.UI.KSplitContainer consolACISplitContainer;
	}
}
