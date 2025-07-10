using Enterprise.ZArchitecture;

namespace Enterprise.Customs.EU.ExitControl.GUI
{
	partial class ReportItemsUserControl
	{
		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo5 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo6 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.ReportItemsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ReportItemsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.BottomPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.IsPackageRelatedToConsignmentItemCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ReportItemAdditionalInfosGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.AdditionalDocumentGrid = new Enterprise.ZArchitecture.ZGrid();
			this.PackagesAndAdditionalDocumentSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.ReportItemPackagesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ReportItemPackagesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.AdditionalPanelsSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.ReportItemAuthorizationsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.AuthorizationsGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ReportItemsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ReportItemsGrid)).BeginInit();
			this.ReportItemsGrid.SuspendLayout();
			this.BottomPanel.SuspendLayout();
			this.ReportItemAdditionalInfosGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.AdditionalDocumentGrid)).BeginInit();
			this.AdditionalDocumentGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.PackagesAndAdditionalDocumentSplitContainer)).BeginInit();
			this.PackagesAndAdditionalDocumentSplitContainer.Panel1.SuspendLayout();
			this.PackagesAndAdditionalDocumentSplitContainer.Panel2.SuspendLayout();
			this.PackagesAndAdditionalDocumentSplitContainer.SuspendLayout();
			this.ReportItemPackagesGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ReportItemPackagesGrid)).BeginInit();
			this.ReportItemPackagesGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.AdditionalPanelsSplitContainer)).BeginInit();
			this.AdditionalPanelsSplitContainer.Panel1.SuspendLayout();
			this.AdditionalPanelsSplitContainer.Panel2.SuspendLayout();
			this.AdditionalPanelsSplitContainer.SuspendLayout();
			this.ReportItemAuthorizationsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.AuthorizationsGrid)).BeginInit();
			this.AuthorizationsGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.ExitControlBase.Business.ICusExitReportCollection<Enterprise.Customs.EU.ExitControl.Business.CusExitReport>);
			// 
			// ReportItemsGroupBox
			// 
			this.ReportItemsGroupBox.CaptionResourceString = Enterprise.Customs.EU.ExitControl.GUI.Res.GetData("6CB8ACA9-2408-451D-B220-84A9D244084F", "Items");
			this.ReportItemsGroupBox.Controls.Add(this.ReportItemsGrid);
			this.ReportItemsGroupBox.Dock = System.Windows.Forms.DockStyle.Left;
			this.ReportItemsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ReportItemsGroupBox.Name = "ReportItemsGroupBox";
			this.ReportItemsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(500, 630, true);
			this.ReportItemsGroupBox.TabIndex = 0;
			this.ReportItemsGroupBox.TabStop = false;
			// 
			// ReportItemsGrid
			// 
			this.ReportItemsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ReportItemsGrid, "CusExitReportItemsForBinding");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.EU.ExitControl.Business.CusExitReport)(null)).CusExitReportItemsForBinding)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.ExitControl.Business.CusExitReportItem)(((System.Collections.IList)(((Enterprise.Customs.EU.ExitControl.Business.CusExitReport)(null)).CusExitReportItemsForBinding)).SyncRoot)).ConsignmentItemLineNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.ExitControl.Business.CusExitReportItem)(((System.Collections.IList)(((Enterprise.Customs.EU.ExitControl.Business.CusExitReport)(null)).CusExitReportItemsForBinding)).SyncRoot)).ERI_GrossMass)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.ExitControl.Business.CusExitReportItem)(((System.Collections.IList)(((Enterprise.Customs.EU.ExitControl.Business.CusExitReport)(null)).CusExitReportItemsForBinding)).SyncRoot)).ERI_NetMass)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.ExitControl.Business.CusExitReportItem)(((System.Collections.IList)(((Enterprise.Customs.EU.ExitControl.Business.CusExitReport)(null)).CusExitReportItemsForBinding)).SyncRoot)).ConsignmentItemUniqueConsignmentReference)));
			this.ReportItemsGrid.CaptionVisible = false;
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "ConsignmentItemLineNumber";
			zCalcEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.ColumnName = "ERI_GrossMass";
			zCalcEditColumnStyleInfo2.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.ColumnName = "ERI_NetMass";
			zCalcEditColumnStyleInfo3.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "ConsignmentItemUniqueConsignmentReference";
			zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140);
			this.ReportItemsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.ReportItemsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.ReportItemsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.ReportItemsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ReportItemsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ReportItemsGrid.GridId = "378037C9-CC93-4985-B2EF-A0E7A4B4E92A";
			this.ReportItemsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ReportItemsGrid.LayoutKey = "ReportItemsGrid";
			this.ReportItemsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 15, true);
			this.ReportItemsGrid.Name = "ReportItemsGrid";
			this.ReportItemsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(496, 613, true);
			this.ReportItemsGrid.TabIndex = 0;
			this.ReportItemsGrid.AfterBind += new System.EventHandler(this.ReportItemsGrid_AfterBind);
			// 
			// BottomPanel
			// 
			this.BottomPanel.Controls.Add(this.IsPackageRelatedToConsignmentItemCheckBox);
			this.BottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.BottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 630, true);
			this.BottomPanel.Name = "BottomPanel";
			this.BottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1000, 50, true);
			this.BottomPanel.TabIndex = 1;
			// 
			// IsPackageRelatedToConsignmentItemCheckBox
			// 
			this.BindingSource.SetBindingMember(this.IsPackageRelatedToConsignmentItemCheckBox, "IsPackageRelatedToConsignmentItem");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.EU.ExitControl.Business.CusExitReport)(null)).IsPackageRelatedToConsignmentItem)));
			this.IsPackageRelatedToConsignmentItemCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(501, 15, true);
			this.IsPackageRelatedToConsignmentItemCheckBox.Name = "IsPackageRelatedToConsignmentItemCheckBox";
			this.IsPackageRelatedToConsignmentItemCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(286, 24, true);
			this.IsPackageRelatedToConsignmentItemCheckBox.TabIndex = 0;
			// 
			// ReportItemAdditionalInfosGroupBox
			// 
			this.ReportItemAdditionalInfosGroupBox.CaptionResourceString = Enterprise.Customs.EU.ExitControl.GUI.Res.GetData("F3342C9B-13EE-4AFA-BEA8-EFCCEC8A4859", "Additional Document");
			this.ReportItemAdditionalInfosGroupBox.Controls.Add(this.AdditionalDocumentGrid);
			this.ReportItemAdditionalInfosGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ReportItemAdditionalInfosGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ReportItemAdditionalInfosGroupBox.Name = "ReportItemAdditionalInfosGroupBox";
			this.ReportItemAdditionalInfosGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(500, 181, true);
			this.ReportItemAdditionalInfosGroupBox.TabIndex = 0;
			this.ReportItemAdditionalInfosGroupBox.TabStop = false;
			// 
			// AdditionalDocumentGrid
			// 
			this.AdditionalDocumentGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.AdditionalDocumentGrid, "CusExitReportItemsForBinding.AdditionalInfos");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.EU.ExitControl.Business.CusExitReportItem)(((System.Collections.IList)(((Enterprise.Customs.EU.ExitControl.Business.CusExitReport)(null)).CusExitReportItemsForBinding)).SyncRoot)).AdditionalInfos)));
			this.AdditionalDocumentGrid.CaptionVisible = false;
			this.AdditionalDocumentGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AdditionalDocumentGrid.GridId = "378037C9-CC93-4985-B2EF-A0E7A4B4E92A";
			this.AdditionalDocumentGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.AdditionalDocumentGrid.LayoutKey = "AdditionalDocumentGrid";
			this.AdditionalDocumentGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 15, true);
			this.AdditionalDocumentGrid.Name = "AdditionalDocumentGrid";
			this.AdditionalDocumentGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(496, 165, true);
			this.AdditionalDocumentGrid.TabIndex = 1;
			// 
			// PackagesAndAdditionalDocumentSplitContainer
			// 
			this.PackagesAndAdditionalDocumentSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PackagesAndAdditionalDocumentSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(500, 0, true);
			this.PackagesAndAdditionalDocumentSplitContainer.Name = "PackagesAndAdditionalDocumentSplitContainer";
			this.PackagesAndAdditionalDocumentSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// PackagesAndAdditionalDocumentSplitContainer.Panel1
			// 
			this.PackagesAndAdditionalDocumentSplitContainer.Panel1.Controls.Add(this.ReportItemPackagesGroupBox);
			// 
			// PackagesAndAdditionalDocumentSplitContainer.Panel2
			// 
			this.PackagesAndAdditionalDocumentSplitContainer.Panel2.Controls.Add(this.AdditionalPanelsSplitContainer);
			this.PackagesAndAdditionalDocumentSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(500, 630, true);
			this.PackagesAndAdditionalDocumentSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(219);
			this.PackagesAndAdditionalDocumentSplitContainer.SplitterWidth = 6;
			this.PackagesAndAdditionalDocumentSplitContainer.TabIndex = 2;
			// 
			// ReportItemPackagesGroupBox
			// 
			this.ReportItemPackagesGroupBox.CaptionResourceString = Enterprise.Customs.EU.ExitControl.GUI.Res.GetData("D93D60AC-7FB5-4B9D-8039-28BB643BE140", "Packages");
			this.ReportItemPackagesGroupBox.Controls.Add(this.ReportItemPackagesGrid);
			this.ReportItemPackagesGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ReportItemPackagesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ReportItemPackagesGroupBox.Name = "ReportItemPackagesGroupBox";
			this.ReportItemPackagesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(500, 219, true);
			this.ReportItemPackagesGroupBox.TabIndex = 0;
			this.ReportItemPackagesGroupBox.TabStop = false;
			// 
			// ReportItemPackagesGrid
			// 
			this.ReportItemPackagesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ReportItemPackagesGrid, "CusExitReportItemPackages");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.EU.ExitControl.Business.CusExitReport)(null)).CusExitReportItemPackages)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.ExitControl.Business.CusExitReportItem)(((System.Collections.IList)(((Enterprise.Customs.EU.ExitControl.Business.CusExitReport)(null)).CusExitReportItemPackages)).SyncRoot)).SeqNo)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.ExitControl.Business.CusExitReportItem)(((System.Collections.IList)(((Enterprise.Customs.EU.ExitControl.Business.CusExitReport)(null)).CusExitReportItemPackages)).SyncRoot)).ConsignmentItemLineNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.ExitControl.Business.CusExitReportItem)(((System.Collections.IList)(((Enterprise.Customs.EU.ExitControl.Business.CusExitReport)(null)).CusExitReportItemPackages)).SyncRoot)).ERI_Quantity)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.ExitControl.Business.CusExitReportItem)(((System.Collections.IList)(((Enterprise.Customs.EU.ExitControl.Business.CusExitReport)(null)).CusExitReportItemPackages)).SyncRoot)).PackageType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.ExitControl.Business.CusExitReportItem)(((System.Collections.IList)(((Enterprise.Customs.EU.ExitControl.Business.CusExitReport)(null)).CusExitReportItemPackages)).SyncRoot)).PackageMarksAndNumbers)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.ExitControl.Business.CusExitReportItem)(((System.Collections.IList)(((Enterprise.Customs.EU.ExitControl.Business.CusExitReport)(null)).CusExitReportItemPackages)).SyncRoot)).PackageContainerOrEquipment)));
			this.ReportItemPackagesGrid.CaptionVisible = false;
			zCalcEditColumnStyleInfo4.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo4.ColumnName = "SeqNo";
			zCalcEditColumnStyleInfo4.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo5.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo5.ColumnName = "ConsignmentItemLineNumber";
			zCalcEditColumnStyleInfo5.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo6.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo6.ColumnName = "ERI_Quantity";
			zCalcEditColumnStyleInfo6.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo2.ColumnName = "PackageType";
			zTextBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo3.ColumnName = "PackageMarksAndNumbers";
			zTextBoxColumnStyleInfo3.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140);
			zTextBoxColumnStyleInfo4.ColumnName = "PackageContainerOrEquipment";
			zTextBoxColumnStyleInfo4.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140);
			this.ReportItemPackagesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
			this.ReportItemPackagesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo5);
			this.ReportItemPackagesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo6);
			this.ReportItemPackagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.ReportItemPackagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.ReportItemPackagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.ReportItemPackagesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ReportItemPackagesGrid.GridId = "378037C9-CC93-4985-B2EF-A0E7A4B4E92A";
			this.ReportItemPackagesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ReportItemPackagesGrid.LayoutKey = "ReportItemPackagesGrid";
			this.ReportItemPackagesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 15, true);
			this.ReportItemPackagesGrid.Name = "ReportItemPackagesGrid";
			this.ReportItemPackagesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(496, 202, true);
			this.ReportItemPackagesGrid.TabIndex = 0;
			// 
			// AdditionalPanelsSplitContainer
			// 
			this.AdditionalPanelsSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AdditionalPanelsSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.AdditionalPanelsSplitContainer.Name = "AdditionalPanelsSplitContainer";
			this.AdditionalPanelsSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// AdditionalPanelsSplitContainer.Panel1
			// 
			this.AdditionalPanelsSplitContainer.Panel1.Controls.Add(this.ReportItemAuthorizationsGroupBox);
			// 
			// AdditionalPanelsSplitContainer.Panel2
			// 
			this.AdditionalPanelsSplitContainer.Panel2.Controls.Add(this.ReportItemAdditionalInfosGroupBox);
			this.AdditionalPanelsSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(500, 399, true);
			this.AdditionalPanelsSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(199);
			this.AdditionalPanelsSplitContainer.SplitterWidth = 6;
			this.AdditionalPanelsSplitContainer.TabIndex = 3;
			// 
			// AuthorizationsGroupBox
			// 
			this.ReportItemAuthorizationsGroupBox.CaptionResourceString = Enterprise.Customs.EU.ExitControl.GUI.Res.GetData("03950ec0-1fc7-4814-85e8-3e91c90d6f8b", "Authorizations");
			this.ReportItemAuthorizationsGroupBox.Controls.Add(this.AuthorizationsGrid);
			this.ReportItemAuthorizationsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ReportItemAuthorizationsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ReportItemAuthorizationsGroupBox.Name = "AuthorizationsGroupBox";
			this.ReportItemAuthorizationsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(500, 199, true);
			this.ReportItemAuthorizationsGroupBox.TabIndex = 1;
			this.ReportItemAuthorizationsGroupBox.TabStop = false;
			// 
			// AuthorizationsGrid
			// 
			this.AuthorizationsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.AuthorizationsGrid, "CusExitReportItemsForBinding.CusAuthorizationUsages");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.EU.ExitControl.Business.CusExitReportItem)(((System.Collections.IList)(((Enterprise.Customs.EU.ExitControl.Business.CusExitReport)(null)).CusExitReportItemsForBinding)).SyncRoot)).CusAuthorizationUsages)));
			this.AuthorizationsGrid.CaptionVisible = false;
			this.AuthorizationsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AuthorizationsGrid.GridId = "378037C9-CC93-4985-B2EF-A0E7A4B4E92A";
			this.AuthorizationsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.AuthorizationsGrid.LayoutKey = "AuthorizationsGrid";
			this.AuthorizationsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 15, true);
			this.AuthorizationsGrid.Name = "AuthorizationsGrid";
			this.AuthorizationsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(496, 182, true);
			this.AuthorizationsGrid.TabIndex = 1;
			// 
			// ReportItemsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.PackagesAndAdditionalDocumentSplitContainer);
			this.Controls.Add(this.ReportItemsGroupBox);
			this.Controls.Add(this.BottomPanel);
			this.Name = "ReportItemsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1000, 680, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ReportItemsGroupBox.ResumeLayout(false);
			this.ReportItemsGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ReportItemsGrid)).EndInit();
			this.ReportItemsGrid.ResumeLayout(false);
			this.ReportItemsGrid.PerformLayout();
			this.BottomPanel.ResumeLayout(false);
			this.BottomPanel.PerformLayout();
			this.ReportItemAdditionalInfosGroupBox.ResumeLayout(false);
			this.ReportItemAdditionalInfosGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.AdditionalDocumentGrid)).EndInit();
			this.AdditionalDocumentGrid.ResumeLayout(false);
			this.AdditionalDocumentGrid.PerformLayout();
			this.PackagesAndAdditionalDocumentSplitContainer.Panel1.ResumeLayout(false);
			this.PackagesAndAdditionalDocumentSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.PackagesAndAdditionalDocumentSplitContainer)).EndInit();
			this.PackagesAndAdditionalDocumentSplitContainer.ResumeLayout(false);
			this.PackagesAndAdditionalDocumentSplitContainer.PerformLayout();
			this.ReportItemPackagesGroupBox.ResumeLayout(false);
			this.ReportItemPackagesGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ReportItemPackagesGrid)).EndInit();
			this.ReportItemPackagesGrid.ResumeLayout(false);
			this.ReportItemPackagesGrid.PerformLayout();
			this.AdditionalPanelsSplitContainer.Panel1.ResumeLayout(false);
			this.AdditionalPanelsSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.AdditionalPanelsSplitContainer)).EndInit();
			this.AdditionalPanelsSplitContainer.ResumeLayout(false);
			this.AdditionalPanelsSplitContainer.PerformLayout();
			this.ReportItemAuthorizationsGroupBox.ResumeLayout(false);
			this.ReportItemAuthorizationsGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.AuthorizationsGrid)).EndInit();
			this.AuthorizationsGrid.ResumeLayout(false);
			this.AuthorizationsGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		internal ZArchitecture.GUI.ZGroupBox ReportItemsGroupBox;
		internal ZArchitecture.ZGrid ReportItemsGrid;
		internal ZArchitecture.GUI.ZGroupBox ReportItemAdditionalInfosGroupBox;
		internal ZArchitecture.GUI.ZCheckBox IsPackageRelatedToConsignmentItemCheckBox;
		internal ZArchitecture.GUI.ZPanel BottomPanel;
		internal CargoWise.Windows.UI.KSplitContainer PackagesAndAdditionalDocumentSplitContainer;
		internal ZArchitecture.ZGrid ReportItemPackagesGrid;
		internal ZArchitecture.GUI.ZGroupBox ReportItemPackagesGroupBox;
		internal ZArchitecture.ZGrid AdditionalDocumentGrid;
		internal CargoWise.Windows.UI.KSplitContainer AdditionalPanelsSplitContainer;
		internal ZArchitecture.GUI.ZGroupBox ReportItemAuthorizationsGroupBox;
		internal ZGrid AuthorizationsGrid;
	}
}
