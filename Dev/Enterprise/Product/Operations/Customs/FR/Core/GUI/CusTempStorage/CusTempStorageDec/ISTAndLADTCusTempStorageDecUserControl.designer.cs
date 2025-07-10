using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.FR.Business.CusTempStorage;

namespace Enterprise.Customs.FR.GUI.CusTempStorage
{
	partial class ISTAndLADTCusTempStorageDecUserControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		#region Disposing
		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}
		#endregion

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
            Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
            Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
            Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
            Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
            Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
            this.ContainersTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
            this.CusTempStorageContainerGrid = new Enterprise.ZArchitecture.ZGrid();
            this.SupportingDocumentsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
            this.SupportingDocumentsUserControl = new Enterprise.ZArchitecture.GUI.ZDynamicControlCreationUserControl();
            this.SJH_CustomsProfileDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEditWithFixedWidth();
			this.DDTNumberTextBox = new ZArchitecture.ZTextBox();
			this.GoodsLocationDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.FurtherDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.LineItemsGrid = new Enterprise.ZArchitecture.ZGrid();
            this.DeclarationStatusDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.HeaderPanel.SuspendLayout();
            this.DetailsTabControl.SuspendLayout();
            this.LinesTabPage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.LinesGrid)).BeginInit();
            this.LinesGrid.SuspendLayout();
            this.LinesBottomPanel.SuspendLayout();
            this.ClassificationGroupBox.SuspendLayout();
            this.ItemDetailsGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.ContainersTabPage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.CusTempStorageContainerGrid)).BeginInit();
            this.CusTempStorageContainerGrid.SuspendLayout();
            this.SupportingDocumentsTabPage.SuspendLayout();
            this.SJH_CustomsProfileDropEdit.SuspendLayout();
            this.DDTNumberTextBox.SuspendLayout();
            this.FurtherDetailsGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.LineItemsGrid)).BeginInit();
            this.LineItemsGrid.SuspendLayout();
            this.DeclarationStatusDropEdit.SuspendLayout();
            this.SuspendLayout();
            // 
            // HeaderPanel
            // 
            this.HeaderPanel.Controls.Add(this.DeclarationStatusDropEdit);
            this.HeaderPanel.Controls.Add(this.SJH_CustomsProfileDropEdit);
			this.HeaderPanel.Controls.Add(this.DDTNumberTextBox);
			this.HeaderPanel.Controls.SetChildIndex(this.SJH_CustomsProfileDropEdit, 0);
            this.HeaderPanel.Controls.SetChildIndex(this.DeclarationStatusDropEdit, 0);
			this.HeaderPanel.Controls.SetChildIndex(this.DDTNumberTextBox, 0);
			// 
			// DetailsTabControl
			// 
			this.DetailsTabControl.Controls.Add(this.ContainersTabPage);
            this.DetailsTabControl.Controls.Add(this.SupportingDocumentsTabPage);
            this.DetailsTabControl.Controls.SetChildIndex(this.LinesTabPage, 0);
            this.DetailsTabControl.Controls.SetChildIndex(this.SupportingDocumentsTabPage, 0);
            this.DetailsTabControl.Controls.SetChildIndex(this.ContainersTabPage, 0);
            // 
            // LinesTabPage
            // 
            this.LinesTabPage.Controls.Add(this.FurtherDetailsGroupBox);
            this.LinesTabPage.Controls.SetChildIndex(this.LinesGrid, 0);
            this.LinesTabPage.Controls.SetChildIndex(this.LinesBottomPanel, 0);
            this.LinesTabPage.Controls.SetChildIndex(this.FurtherDetailsGroupBox, 0);
            // 
            // LinesGrid
            // 
            this.LinesGrid.Dock = System.Windows.Forms.DockStyle.Top;
            this.LinesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1245, 170, true);
            // 
            // LinesBottomPanel
            // 
            this.LinesBottomPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.LinesBottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 170, true);
            // 
            // ContainersTabPage
            // 
            this.ContainersTabPage.CaptionResourceString = Enterprise.Customs.FR.GUI.Res.GetData("6C937B2E-8660-473A-B154-9B0A7FFC33BC", "Containers");
            this.ContainersTabPage.Controls.Add(this.CusTempStorageContainerGrid);
            this.ContainersTabPage.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ContainersTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
            this.ContainersTabPage.Name = "ContainersTabPage";
			this.ContainersTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1245, 560, true);
            this.ContainersTabPage.TabIndex = 0;
            // 
            // CusTempStorageContainerGrid
            // 
            this.CusTempStorageContainerGrid.AllowNavigation = false;
            this.BindingSource.SetBindingMember(this.CusTempStorageContainerGrid, "CusTempStorageDec.CusTempStorageContainers");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.FR.Business.CusTempStorage.CusTempStorageJobHeader)(null)).CusTempStorageDec.CusTempStorageContainers)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((CusTempStorageContainer)(((System.Collections.IList)(((Enterprise.Customs.FR.Business.CusTempStorage.CusTempStorageJobHeader)(null)).CusTempStorageDec.CusTempStorageContainers)).SyncRoot)).CY_Data)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((CusTempStorageContainer)(((System.Collections.IList)(((Enterprise.Customs.FR.Business.CusTempStorage.CusTempStorageJobHeader)(null)).CusTempStorageDec.CusTempStorageContainers)).SyncRoot)).CY_Code)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((CusTempStorageContainer)(((System.Collections.IList)(((Enterprise.Customs.FR.Business.CusTempStorage.CusTempStorageJobHeader)(null)).CusTempStorageDec.CusTempStorageContainers)).SyncRoot)).Description)));
            this.CusTempStorageContainerGrid.CaptionVisible = false;
            zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.FR.GUI.Res.GetData("63bb456c-1f9b-4393-a040-f5780a0fd05d", "Container Number");
            zTextBoxColumnStyleInfo1.ColumnName = "CY_Data";
			zTextBoxColumnStyleInfo1.IsCustomColumn = false;
            zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
            zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.FR.GUI.Res.GetData("2b7fc246-9253-41f5-b89b-2a8068b85abc", "Type");
            zDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            zDropEditColumnStyleInfo1.ColumnName = "CY_Code";
			zDropEditColumnStyleInfo1.IsCustomColumn = false;
            zDropEditColumnStyleInfo1.IsMandatory = true;
            zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
            zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.FR.GUI.Res.GetData("D8BAEDDF-A64A-44E2-958B-CC56F46A6B4E", "Description");
            zTextBoxColumnStyleInfo2.ColumnName = "Description";
			zTextBoxColumnStyleInfo2.IsCustomColumn = false;
            zTextBoxColumnStyleInfo2.IsReadOnly = true;
            zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
            this.CusTempStorageContainerGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
            this.CusTempStorageContainerGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
            this.CusTempStorageContainerGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
            this.CusTempStorageContainerGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.CusTempStorageContainerGrid.GridId = "EFDE6F42-D43C-4A62-B912-06C502427786";
            this.CusTempStorageContainerGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.CusTempStorageContainerGrid.LayoutKey = "CDDCFD6B-F4A5-4055-8563-207ACB8FC747";
            this.CusTempStorageContainerGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.CusTempStorageContainerGrid.Name = "CusTempStorageContainerGrid";
			this.CusTempStorageContainerGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1245, 560, true);
            this.CusTempStorageContainerGrid.TabIndex = 0;
            // 
            // SupportingDocumentsTabPage
            // 
            this.SupportingDocumentsTabPage.CaptionResourceString = Enterprise.Customs.FR.GUI.Res.GetData("4379C19F-5082-4F5C-BEE5-51D64C69826E", "Supporting Documents");
            this.SupportingDocumentsTabPage.Controls.Add(this.SupportingDocumentsUserControl);
            this.SupportingDocumentsTabPage.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SupportingDocumentsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
            this.SupportingDocumentsTabPage.Name = "SupportingDocumentsTabPage";
			this.SupportingDocumentsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1245, 560, true);
            this.SupportingDocumentsTabPage.TabIndex = 0;
            // 
            // SupportingDocumentsUserControl
            // 
            this.SupportingDocumentsUserControl.AllowDrop = true;
            this.SupportingDocumentsUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.SupportingDocumentsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.SupportingDocumentsUserControl.Name = "SupportingDocumentsUserControl";
			this.SupportingDocumentsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1245, 560, true);
            this.SupportingDocumentsUserControl.TabIndex = 0;
            // 
            // SJH_CustomsProfileDropEdit
            // 
            this.SJH_CustomsProfileDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.SJH_CustomsProfileDropEdit, "SJH_CustomsProfile");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.FR.Business.CusTempStorage.CusTempStorageJobHeader)(null)).SJH_CustomsProfile)));
            this.SJH_CustomsProfileDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(589, 63, true);
            this.SJH_CustomsProfileDropEdit.Name = "SJH_CustomsProfileDropEdit";
            this.SJH_CustomsProfileDropEdit.ShouldResizeByMaxLength = false;
            this.SJH_CustomsProfileDropEdit.ShowDescriptionBox = false;
            this.SJH_CustomsProfileDropEdit.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			this.SJH_CustomsProfileDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.SJH_CustomsProfileDropEdit.TabIndex = 7;
			// 
			// DDTNumberTextBox
			// 
			this.DDTNumberTextBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DDTNumberTextBox, "DDTNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.FR.Business.CusTempStorage.CusTempStorageJobHeader)(null)).DDTNumber)));
			this.DDTNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(589, 35, true);
			this.DDTNumberTextBox.Name = "DDTNumberTextBox";
			this.DDTNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.DDTNumberTextBox.CaptionResourceString = Res.GetData("224A1B98-B0B7-42B3-A308-B0C682CB11D5", "DDT Number");
			this.DDTNumberTextBox.ReadOnly = true;
			this.DDTNumberTextBox.Visible = false;
			this.DDTNumberTextBox.TabIndex = 7;
			// 
			// ItemDetailsGroupBox
			//
			this.ItemDetailsGroupBox.Controls.Remove(this.GoodsLocationTextBox);
			this.ItemDetailsGroupBox.Controls.Add(this.GoodsLocationDropEdit);
			//
			// GoodsLocationDropEdit
			//
			this.GoodsLocationDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.GoodsLocationDropEdit, "CusTempStorageDec.CusTempStorageLines.TSL_LocationOfGoods");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.CusTempStorage.CusTempStorageLine)(((System.Collections.IList)(((Enterprise.Customs.FR.Business.CusTempStorage.CusTempStorageJobHeader)(null)).CusTempStorageDec.CusTempStorageLines)).SyncRoot)).TSL_LocationOfGoods)));
			this.GoodsLocationDropEdit.CaptionResourceString = Enterprise.Customs.FR.GUI.Res.GetData("155B816F-FF04-4AF6-86A0-1A99C4471012", "Goods Location");
			this.GoodsLocationDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(109, 44, true);
			this.GoodsLocationDropEdit.Name = "GoodsLocationDropEdit";
			this.GoodsLocationDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(397, 20, true);
			this.GoodsLocationDropEdit.ShowDescriptionBox = true;
			this.GoodsLocationDropEdit.TabIndex = 1;
			// 
			// FurtherDetailsGroupBox
			// 
			this.FurtherDetailsGroupBox.CaptionResourceString = Enterprise.Customs.FR.GUI.Res.GetData("E1A86A5D-3988-46D0-BB91-703DDF8CE7D2", "Further Details");
            this.FurtherDetailsGroupBox.Controls.Add(this.LineItemsGrid);
            this.FurtherDetailsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.FurtherDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 166, true);
            this.FurtherDetailsGroupBox.Name = "FurtherDetailsGroupBox";
			this.FurtherDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1245, 394, true);
            this.FurtherDetailsGroupBox.TabIndex = 3;
            this.FurtherDetailsGroupBox.TabStop = false;
            // 
            // LineItemsGrid
            // 
            this.LineItemsGrid.AllowNavigation = false;
            this.BindingSource.SetBindingMember(this.LineItemsGrid, "CusTempStorageDec.CusTempStorageLines.CusTempStorageLineItems");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusTempStorage.CusTempStorageLine)(((System.Collections.IList)(((Enterprise.Customs.FR.Business.CusTempStorage.CusTempStorageJobHeader)(null)).CusTempStorageDec.CusTempStorageLines)).SyncRoot)).CusTempStorageLineItems)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.CusTempStorage.CusTempStorageLineItem)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusTempStorage.CusTempStorageLine)(((System.Collections.IList)(((Enterprise.Customs.FR.Business.CusTempStorage.CusTempStorageJobHeader)(null)).CusTempStorageDec.CusTempStorageLines)).SyncRoot)).CusTempStorageLineItems)).SyncRoot)).TSI_CommodityCode)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.CusTempStorage.CusTempStorageLineItem)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusTempStorage.CusTempStorageLine)(((System.Collections.IList)(((Enterprise.Customs.FR.Business.CusTempStorage.CusTempStorageJobHeader)(null)).CusTempStorageDec.CusTempStorageLines)).SyncRoot)).CusTempStorageLineItems)).SyncRoot)).TSI_GoodsOrigin)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.Business.CusTempStorage.CusTempStorageLineItem)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusTempStorage.CusTempStorageLine)(((System.Collections.IList)(((Enterprise.Customs.FR.Business.CusTempStorage.CusTempStorageJobHeader)(null)).CusTempStorageDec.CusTempStorageLines)).SyncRoot)).CusTempStorageLineItems)).SyncRoot)).TSI_NetWeight)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.CusTempStorage.CusTempStorageLineItem)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusTempStorage.CusTempStorageLine)(((System.Collections.IList)(((Enterprise.Customs.FR.Business.CusTempStorage.CusTempStorageJobHeader)(null)).CusTempStorageDec.CusTempStorageLines)).SyncRoot)).CusTempStorageLineItems)).SyncRoot)).TSI_NetWeightUQ)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.Business.CusTempStorage.CusTempStorageLineItem)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusTempStorage.CusTempStorageLine)(((System.Collections.IList)(((Enterprise.Customs.FR.Business.CusTempStorage.CusTempStorageJobHeader)(null)).CusTempStorageDec.CusTempStorageLines)).SyncRoot)).CusTempStorageLineItems)).SyncRoot)).TSI_GoodsValue)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.Business.CusTempStorage.CusTempStorageLineItem)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusTempStorage.CusTempStorageLine)(((System.Collections.IList)(((Enterprise.Customs.FR.Business.CusTempStorage.CusTempStorageJobHeader)(null)).CusTempStorageDec.CusTempStorageLines)).SyncRoot)).CusTempStorageLineItems)).SyncRoot)).TSI_GuaranteedValue)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.CusTempStorage.CusTempStorageLineItem)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusTempStorage.CusTempStorageLine)(((System.Collections.IList)(((Enterprise.Customs.FR.Business.CusTempStorage.CusTempStorageJobHeader)(null)).CusTempStorageDec.CusTempStorageLines)).SyncRoot)).CusTempStorageLineItems)).SyncRoot)).TSI_RX_NKCurrency)));
            this.LineItemsGrid.CaptionVisible = false;
            zTextBoxColumnStyleInfo3.ColumnName = "TSI_CommodityCode";
			zTextBoxColumnStyleInfo3.IsCustomColumn = false;
            zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(226);
            zDropEditColumnStyleInfo2.ColumnName = "TSI_GoodsOrigin";
            zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo2.IsCustomColumn = false;
            zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
            zCalcEditColumnStyleInfo1.ColumnName = "TSI_NetWeight";
            zCalcEditColumnStyleInfo1.GroupName = Enterprise.Customs.FR.GUI.Res.GetData("6D4D2D2C-8270-4835-9595-F9A60C57C62A", "Net Mass");
			zCalcEditColumnStyleInfo1.IsCustomColumn = false;
            zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(146);
            zDropEditColumnStyleInfo3.ColumnName = "TSI_NetWeightUQ";
            zDropEditColumnStyleInfo3.GroupName = Enterprise.Customs.FR.GUI.Res.GetData("6D4D2D2C-8270-4835-9595-F9A60C57C62A", "Net Mass");
			zDropEditColumnStyleInfo2.IsCustomColumn = false;
            zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(91);
            zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
            zCalcEditColumnStyleInfo2.ColumnName = "TSI_GoodsValue";
            zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(152);
            zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
            zCalcEditColumnStyleInfo3.ColumnName = "TSI_GuaranteedValue";
            zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(152);
            zCodeFindBoxColumnStyleInfo1.ColumnName = "TSI_RX_NKCurrency";
            zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(68);
            this.LineItemsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
            this.LineItemsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
            this.LineItemsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
            this.LineItemsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
            this.LineItemsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
            this.LineItemsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
            this.LineItemsGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
            this.LineItemsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.LineItemsGrid.GridId = "D03269E4-76ED-41ED-A914-C904237ABB53";
            this.LineItemsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.LineItemsGrid.LayoutKey = "LineItemsGrid";
			this.LineItemsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
            this.LineItemsGrid.Name = "LineItemsGrid";
			this.LineItemsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1239, 375, true);
            this.LineItemsGrid.TabIndex = 2;
            // 
            // DeclarationStatusDropEdit
            // 
            this.DeclarationStatusDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.DeclarationStatusDropEdit, "CusTempStorageDec.STH_DeclarationStatus");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.FR.Business.CusTempStorage.CusTempStorageJobHeader)(null)).CusTempStorageDec.STH_DeclarationStatus)));
            this.DeclarationStatusDropEdit.CaptionResourceString = Enterprise.Customs.FR.GUI.Res.GetData("aaefc498-d635-49b1-bebb-daaf1fa4f649", "Declaration Status");
            this.DeclarationStatusDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(324, 7, true);
            this.DeclarationStatusDropEdit.Name = "DeclarationStatusDropEdit";
            this.DeclarationStatusDropEdit.ShouldResizeByMaxLength = true;
            this.DeclarationStatusDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(130, 15, true);
            this.DeclarationStatusDropEdit.TabIndex = 0;
            this.DeclarationStatusDropEdit.TabStop = false;
            // 
            // ISTAndLADTCusTempStorageDecUserControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.Name = "ISTAndLADTCusTempStorageDecUserControl";
            this.HeaderPanel.ResumeLayout(false);
            this.HeaderPanel.PerformLayout();
            this.DetailsTabControl.ResumeLayout(false);
            this.DetailsTabControl.PerformLayout();
            this.LinesTabPage.ResumeLayout(false);
            this.LinesTabPage.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.LinesGrid)).EndInit();
            this.LinesGrid.ResumeLayout(false);
            this.LinesGrid.PerformLayout();
            this.LinesBottomPanel.ResumeLayout(false);
            this.LinesBottomPanel.PerformLayout();
            this.ClassificationGroupBox.ResumeLayout(false);
            this.ClassificationGroupBox.PerformLayout();
            this.ItemDetailsGroupBox.ResumeLayout(false);
            this.ItemDetailsGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.ContainersTabPage.ResumeLayout(false);
            this.ContainersTabPage.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.CusTempStorageContainerGrid)).EndInit();
            this.CusTempStorageContainerGrid.ResumeLayout(false);
            this.CusTempStorageContainerGrid.PerformLayout();
            this.SupportingDocumentsTabPage.ResumeLayout(false);
            this.SupportingDocumentsTabPage.PerformLayout();
            this.SJH_CustomsProfileDropEdit.ResumeLayout(true);
            this.SJH_CustomsProfileDropEdit.PerformLayout();
            this.FurtherDetailsGroupBox.ResumeLayout(false);
            this.FurtherDetailsGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.LineItemsGrid)).EndInit();
            this.LineItemsGrid.ResumeLayout(false);
            this.LineItemsGrid.PerformLayout();
            this.DeclarationStatusDropEdit.ResumeLayout(true);
            this.DeclarationStatusDropEdit.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZTabPage ContainersTabPage;
		private ZArchitecture.GUI.ZTabPage SupportingDocumentsTabPage;
		private Enterprise.ZArchitecture.ZGrid CusTempStorageContainerGrid;
		private Enterprise.ZArchitecture.GUI.ZDynamicControlCreationUserControl SupportingDocumentsUserControl;
		private ZArchitecture.GUI.ZDropEditWithFixedWidth SJH_CustomsProfileDropEdit;
		private ZArchitecture.GUI.ZDropEdit GoodsLocationDropEdit;
		private ZArchitecture.GUI.ZGroupBox FurtherDetailsGroupBox;
		private ZArchitecture.ZGrid LineItemsGrid;
		private ZArchitecture.ZTextBox DDTNumberTextBox;
		private ZArchitecture.GUI.ZDropEdit DeclarationStatusDropEdit;
	}
}

