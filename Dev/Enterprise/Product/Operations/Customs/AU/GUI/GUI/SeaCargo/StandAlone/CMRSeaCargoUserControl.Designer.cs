namespace Enterprise.Customs.AU.SeaCargo.GUI
{
	public partial class CMRSeaCargoUserControl
	{
		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		#region Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		///

		OceanBillDetailsUserControl OceanBillDetailsUserControl;
		HouseBillDetailsUserControl HouseBillDetailsUserControl;
		HouseBillPartiesUserControl HouseBillPartiesUserControl;
		protected internal Enterprise.ZArchitecture.GUI.ZTemplateTabControl OceanBillSpecificsTabControl;
		Enterprise.ZArchitecture.GUI.ZTabPage CustomFieldsTabPage;
		SeaCargoCustomFieldsUserControl CustomFieldsControl;
		Enterprise.ZArchitecture.GUI.ZTabPage ContainersTabPage;
		Enterprise.ZArchitecture.GUI.ZPanel ContainerGridPanel;
		internal Enterprise.ZArchitecture.ZGrid CusSCAContainersBoundGrid;
		protected internal Enterprise.ZArchitecture.GUI.ZTabPage HouseBillsTabPage;
		protected internal Enterprise.ZArchitecture.ZGrid HouseBillsGrid;
		CargoWise.Windows.UI.KSplitter SeaCargoSplitter;
		protected internal Enterprise.ZArchitecture.GUI.ZTemplateTabControl HouseBillTabControl;
		Enterprise.ZArchitecture.GUI.ZTabPage HouseBillCustomFieldsTabPage;
		SeaCargoHouseBillCustomFieldsUserControl HouseBillCustomFieldsControl;
		Enterprise.ZArchitecture.GUI.ZTabPage DetailsTabPage;
		protected internal Enterprise.ZArchitecture.GUI.ZTabPage PackingTabPage;
		Enterprise.ZArchitecture.GUI.ZTabPage MessagesTabPage;
		protected internal Enterprise.Messaging.GUI.EDIMessageUserControl MessageUserControl;
		CargoWise.Windows.UI.KSplitter ContainerVsPackingSplitter;
		protected internal Enterprise.ZArchitecture.ZLabel zLabel4;
		protected internal Enterprise.ZArchitecture.ZLabel zLabel5;
		ZArchitecture.GUI.ZPanel filterPanel;
		ZArchitecture.GUI.ZButton clearButton;
		ZArchitecture.GUI.ZButton filterButton;
		ZArchitecture.GUI.ZDropEdit customsCargoStatusDropEdit;
		ZArchitecture.ZLabel customsCargoStatusLabel;
		ZArchitecture.GUI.ZDropEdit customsMessageStatusDropEdit;
		ZArchitecture.ZLabel customsMessageStatusLabel;
		ZArchitecture.GUI.ZCheckBox housesWithNotificationsCheckBox;
		protected internal SeaCargoUnderbondAndPackingForContainersUserControl PackingForContainersControl;
		SeaCargoUnderbondAndPackingStandAloneUserControl UnderbondAndPackingUserControlForCMR;
		System.ComponentModel.IContainer components = null;
		Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo ConsigneeAddressDropEditColumnStyleInfo;
		Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo ConsignorAddressDropEditColumnStyleInfo;

		void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo zOrganisationFindBoxColumnStyleInfo1 = new Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo10 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo11 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo12 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo13 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo14 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo15 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo16 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo zOrganisationFindBoxColumnStyleInfo2 = new Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo17 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo18 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo19 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo20 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo21 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo zOrganisationFindBoxColumnStyleInfo3 = new Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo22 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo23 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo24 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo25 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo26 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo27 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo28 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo29 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo30 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo31 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo32 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo33 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo zGuidDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo zGuidDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo();
			Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo zOrganisationFindBoxColumnStyleInfo4 = new Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo();
			Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo zOrganisationFindBoxColumnStyleInfo5 = new Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo();
			this.OceanBillDetailsUserControl = new Enterprise.Customs.AU.SeaCargo.GUI.OceanBillDetailsUserControl();
			this.HouseBillDetailsUserControl = new Enterprise.Customs.AU.SeaCargo.GUI.HouseBillDetailsUserControl();
			this.HouseBillPartiesUserControl = new Enterprise.Customs.AU.SeaCargo.GUI.HouseBillPartiesUserControl();
			this.OceanBillSpecificsTabControl = new Enterprise.ZArchitecture.GUI.ZTemplateTabControl();
			this.ContainersTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.ContainerGridPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.CusSCAContainersBoundGrid = new Enterprise.ZArchitecture.ZGrid();
			this.ContainerVsPackingSplitter = new CargoWise.Windows.UI.KSplitter();
			this.PackingForContainersControl = new Enterprise.Customs.AU.SeaCargo.GUI.SeaCargoUnderbondAndPackingForContainersUserControl();
			this.HouseBillsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.HouseBillsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.SeaCargoSplitter = new CargoWise.Windows.UI.KSplitter();
			this.HouseBillTabControl = new Enterprise.ZArchitecture.GUI.ZTemplateTabControl();
			this.DetailsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.PackingTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.UnderbondAndPackingUserControlForCMR = new Enterprise.Customs.AU.SeaCargo.GUI.SeaCargoUnderbondAndPackingStandAloneUserControl();
			this.HouseBillCustomFieldsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.HouseBillCustomFieldsControl = new Enterprise.Customs.AU.SeaCargo.GUI.SeaCargoHouseBillCustomFieldsUserControl();
			this.MessagesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.MessageUserControl = new Enterprise.Messaging.GUI.EDIMessageUserControl();
			this.filterPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.housesWithNotificationsCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.customsMessageStatusDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.customsMessageStatusLabel = new Enterprise.ZArchitecture.ZLabel();
			this.clearButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.filterButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.customsCargoStatusDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.customsCargoStatusLabel = new Enterprise.ZArchitecture.ZLabel();
			this.CustomFieldsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.CustomFieldsControl = new Enterprise.Customs.AU.SeaCargo.GUI.SeaCargoCustomFieldsUserControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.OceanBillDetailsUserControl.SuspendLayout();
			this.HouseBillDetailsUserControl.SuspendLayout();
			this.HouseBillPartiesUserControl.SuspendLayout();
			this.OceanBillSpecificsTabControl.SuspendLayout();
			this.ContainersTabPage.SuspendLayout();
			this.ContainerGridPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.CusSCAContainersBoundGrid)).BeginInit();
			this.CusSCAContainersBoundGrid.SuspendLayout();
			this.PackingForContainersControl.SuspendLayout();
			this.HouseBillsTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.HouseBillsGrid)).BeginInit();
			this.HouseBillsGrid.SuspendLayout();
			this.HouseBillTabControl.SuspendLayout();
			this.DetailsTabPage.SuspendLayout();
			this.PackingTabPage.SuspendLayout();
			this.UnderbondAndPackingUserControlForCMR.SuspendLayout();
			this.HouseBillCustomFieldsTabPage.SuspendLayout();
			this.HouseBillCustomFieldsControl.SuspendLayout();
			this.MessagesTabPage.SuspendLayout();
			this.MessageUserControl.SuspendLayout();
			this.filterPanel.SuspendLayout();
			this.customsMessageStatusDropEdit.SuspendLayout();
			this.customsCargoStatusDropEdit.SuspendLayout();
			this.CustomFieldsTabPage.SuspendLayout();
			this.CustomFieldsControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.AU.Declaration.Business.CusSCAOceanBill);
			// 
			// OceanBillDetailsUserControl
			// 
			this.OceanBillDetailsUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OceanBillDetailsUserControl, ".");
			this.OceanBillDetailsUserControl.Dock = System.Windows.Forms.DockStyle.Top;
			this.OceanBillDetailsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.OceanBillDetailsUserControl.Name = "OceanBillDetailsUserControl";
			this.OceanBillDetailsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(992, 108, true);
			this.OceanBillDetailsUserControl.TabIndex = 0;
			// 
			// HouseBillDetailsUserControl
			// 
			this.HouseBillDetailsUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.HouseBillDetailsUserControl, "FilteredHouseBills");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusSCAOceanBill)(null)).FilteredHouseBills)).SyncRoot)))));
			this.HouseBillDetailsUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.HouseBillDetailsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.HouseBillDetailsUserControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(500, 255, true);
			this.HouseBillDetailsUserControl.Name = "HouseBillDetailsUserControl";
			this.HouseBillDetailsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(603, 291, true);
			this.HouseBillDetailsUserControl.TabIndex = 0;
			this.HouseBillDetailsUserControl.TabStop = false;
			// 
			// HouseBillPartiesUserControl
			// 
			this.HouseBillPartiesUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.HouseBillPartiesUserControl, "FilteredHouseBills");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusSCAOceanBill)(null)).FilteredHouseBills)).SyncRoot)))));
			this.HouseBillPartiesUserControl.Dock = System.Windows.Forms.DockStyle.Right;
			this.HouseBillPartiesUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(603, 0, true);
			this.HouseBillPartiesUserControl.Name = "HouseBillPartiesUserControl";
			this.HouseBillPartiesUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(379, 291, true);
			this.HouseBillPartiesUserControl.TabIndex = 1;
			// 
			// OceanBillSpecificsTabControl
			// 
			this.OceanBillSpecificsTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.OceanBillSpecificsTabControl.Controls.Add(this.ContainersTabPage);
			this.OceanBillSpecificsTabControl.Controls.Add(this.HouseBillsTabPage);
			this.OceanBillSpecificsTabControl.Controls.Add(this.CustomFieldsTabPage);
			this.OceanBillSpecificsTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.OceanBillSpecificsTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 108, true);
			this.OceanBillSpecificsTabControl.Name = "OceanBillSpecificsTabControl";
			this.OceanBillSpecificsTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(992, 588, true);
			this.OceanBillSpecificsTabControl.TabIndex = 1;
			// 
			// ContainersTabPage
			// 
			this.ContainersTabPage.Controls.Add(this.ContainerGridPanel);
			this.ContainersTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.ContainersTabPage.Name = "ContainersTabPage";
			this.ContainersTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(987, 566, true);
			this.ContainersTabPage.TabIndex = 1;
			this.ContainersTabPage.Text = "Containers";
			// 
			// ContainerGridPanel
			// 
			this.ContainerGridPanel.Controls.Add(this.CusSCAContainersBoundGrid);
			this.ContainerGridPanel.Controls.Add(this.ContainerVsPackingSplitter);
			this.ContainerGridPanel.Controls.Add(this.PackingForContainersControl);
			this.ContainerGridPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ContainerGridPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ContainerGridPanel.Name = "ContainerGridPanel";
			this.ContainerGridPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(987, 566, true);
			this.ContainerGridPanel.TabIndex = 6;
			// 
			// CusSCAContainersBoundGrid
			// 
			this.CusSCAContainersBoundGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.CusSCAContainersBoundGrid, "Containers");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusSCAOceanBill)(null)).Containers)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusSCAContainer)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusSCAOceanBill)(null)).Containers)).SyncRoot)).CN_ContainerNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusSCAContainer)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusSCAOceanBill)(null)).Containers)).SyncRoot)).CN_SealNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusSCAContainer)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusSCAOceanBill)(null)).Containers)).SyncRoot)).CN_ContainerMode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusSCAContainer)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusSCAOceanBill)(null)).Containers)).SyncRoot)).CN_ContainerMode_List)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusSCAContainer)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusSCAOceanBill)(null)).Containers)).SyncRoot)).CN_RC_NKContainerType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusSCAContainer)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusSCAOceanBill)(null)).Containers)).SyncRoot)).CN_ContainerType_List)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.AU.Declaration.Business.CusSCAContainer)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusSCAOceanBill)(null)).Containers)).SyncRoot)).CN_ShipperOwnedContainer)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusSCAContainer)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusSCAOceanBill)(null)).Containers)).SyncRoot)).CN_TypeOfContainer)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusSCAContainer)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusSCAOceanBill)(null)).Containers)).SyncRoot)).Lookups.TypesOfContainer)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusSCAContainer)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusSCAOceanBill)(null)).Containers)).SyncRoot)).CN_ContainerSizeOrISOCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusSCAContainer)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusSCAOceanBill)(null)).Containers)).SyncRoot)).Lookups.ContainerSizes)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusSCAContainer)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusSCAOceanBill)(null)).Containers)).SyncRoot)).CN_ContainerStatus)));
			this.CusSCAContainersBoundGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.Caption = "Container Number";
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "CN_ContainerNumber";
			zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo1.ToolTip = "Container Number";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo2.Caption = "Seal Number";
			zTextBoxColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo2.ColumnName = "CN_SealNumber";
			zTextBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo2.ToolTip = "Container Seal Number";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDropEditColumnStyleInfo1.BindToList = "CN_ContainerMode_List";
			zDropEditColumnStyleInfo1.Caption = "Mode";
			zDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo1.ColumnName = "CN_ContainerMode";
			zDropEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo1.ToolTip = "Mode of the Container";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zCodeFindBoxColumnStyleInfo1.BindToList = "CN_ContainerType_List";
			zCodeFindBoxColumnStyleInfo1.Caption = "Type";
			zCodeFindBoxColumnStyleInfo1.ColumnName = "CN_RC_NKContainerType";
			zCodeFindBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zCodeFindBoxColumnStyleInfo1.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefContainer;
			zCodeFindBoxColumnStyleInfo1.ToolTip = "Container Type";
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zCheckBoxColumnStyleInfo1.Caption = "Shipper Owned";
			zCheckBoxColumnStyleInfo1.ColumnName = "CN_ShipperOwnedContainer";
			zCheckBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zCheckBoxColumnStyleInfo1.ToolTip = "Is Shipper Owned Container";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo2.BindToList = "Lookups.TypesOfContainer";
			zDropEditColumnStyleInfo2.Caption = "Container Type";
			zDropEditColumnStyleInfo2.ColumnName = "CN_TypeOfContainer";
			zDropEditColumnStyleInfo2.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo2.ToolTip = "Type of Container";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo3.BindToList = "Lookups.ContainerSizes";
			zDropEditColumnStyleInfo3.Caption = "Container Size";
			zDropEditColumnStyleInfo3.ColumnName = "CN_ContainerSizeOrISOCode";
			zDropEditColumnStyleInfo3.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo3.ToolTip = "ISO Code for container";
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo3.Caption = "Container Status";
			zTextBoxColumnStyleInfo3.ColumnName = "CN_ContainerStatus";
			zTextBoxColumnStyleInfo3.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo3.IsVisible = false;
			zTextBoxColumnStyleInfo3.ToolTip = "Container Status";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.CusSCAContainersBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.CusSCAContainersBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.CusSCAContainersBoundGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.CusSCAContainersBoundGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.CusSCAContainersBoundGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.CusSCAContainersBoundGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.CusSCAContainersBoundGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.CusSCAContainersBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.CusSCAContainersBoundGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CusSCAContainersBoundGrid.GridId = "ebf3b3b7-ab75-4556-967a-4ff1f64f569e";
			this.CusSCAContainersBoundGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.CusSCAContainersBoundGrid.LayoutKey = "zGrid1";
			this.CusSCAContainersBoundGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.CusSCAContainersBoundGrid.Name = "CusSCAContainersBoundGrid";
			this.CusSCAContainersBoundGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(987, 230, true);
			this.CusSCAContainersBoundGrid.TabIndex = 0;
			// 
			// ContainerVsPackingSplitter
			// 
			this.ContainerVsPackingSplitter.Cursor = System.Windows.Forms.Cursors.HSplit;
			this.ContainerVsPackingSplitter.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.ContainerVsPackingSplitter.DoNotSaveSplitterLayout = false;
			this.ContainerVsPackingSplitter.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 230, true);
			this.ContainerVsPackingSplitter.Name = "ContainerVsPackingSplitter";
			this.ContainerVsPackingSplitter.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(987, 8, true);
			this.ContainerVsPackingSplitter.TabIndex = 1;
			this.ContainerVsPackingSplitter.TabStop = false;
			// 
			// PackingForContainersControl
			// 
			this.PackingForContainersControl.AllowDrop = true;
			this.PackingForContainersControl.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.PackingForContainersControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 238, true);
			this.PackingForContainersControl.Name = "PackingForContainersControl";
			this.PackingForContainersControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(987, 328, true);
			this.PackingForContainersControl.TabIndex = 2;
			// 
			// HouseBillsTabPage
			// 
			this.HouseBillsTabPage.Controls.Add(this.HouseBillsGrid);
			this.HouseBillsTabPage.Controls.Add(this.SeaCargoSplitter);
			this.HouseBillsTabPage.Controls.Add(this.HouseBillTabControl);
			this.HouseBillsTabPage.Controls.Add(this.filterPanel);
			this.HouseBillsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.HouseBillsTabPage.Name = "HouseBillsTabPage";
			this.HouseBillsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(987, 566, true);
			this.HouseBillsTabPage.TabIndex = 0;
			this.HouseBillsTabPage.Text = "House Bills";
			// 
			// HouseBillsGrid
			// 
			this.HouseBillsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.HouseBillsGrid, "FilteredHouseBills");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusSCAOceanBill)(null)).FilteredHouseBills)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusSCAOceanBill)(null)).FilteredHouseBills)).SyncRoot)).CA_HouseBill)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusSCAOceanBill)(null)).FilteredHouseBills)).SyncRoot)).CA_RL_NK_PortOfOrigin)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusSCAOceanBill)(null)).FilteredHouseBills)).SyncRoot)).PortOfOriginList)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusSCAOceanBill)(null)).FilteredHouseBills)).SyncRoot)).CA_RL_NK_PortOfDestination)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusSCAOceanBill)(null)).FilteredHouseBills)).SyncRoot)).PortOfDestinationList)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusSCAOceanBill)(null)).FilteredHouseBills)).SyncRoot)).CA_RN_NKGoodsOrigin)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusSCAOceanBill)(null)).FilteredHouseBills)).SyncRoot)).CountryOfOriginList)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusSCAOceanBill)(null)).FilteredHouseBills)).SyncRoot)).CA_PrepaidCollectOther)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusSCAOceanBill)(null)).FilteredHouseBills)).SyncRoot)).Lookups.MethodsOfPayment)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusSCAOceanBill)(null)).FilteredHouseBills)).SyncRoot)).CA_ShipmentStatus)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusSCAOceanBill)(null)).FilteredHouseBills)).SyncRoot)).ShipmentStatus)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusSCAOceanBill)(null)).FilteredHouseBills)).SyncRoot)).CA_MessageStatus)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusSCAOceanBill)(null)).FilteredHouseBills)).SyncRoot)).MessageStatus)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusSCAOceanBill)(null)).FilteredHouseBills)).SyncRoot)).CA_IsMasterHouse)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusSCAOceanBill)(null)).FilteredHouseBills)).SyncRoot)).CA_MasterHouseBill)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusSCAOceanBill)(null)).FilteredHouseBills)).SyncRoot)).CA_OH_Consignee)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusSCAOceanBill)(null)).FilteredHouseBills)).SyncRoot)).CA_OH_Consignee_List)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusSCAOceanBill)(null)).FilteredHouseBills)).SyncRoot)).CA_ConsigneeName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusSCAOceanBill)(null)).FilteredHouseBills)).SyncRoot)).CA_ConsigneeAddress1)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusSCAOceanBill)(null)).FilteredHouseBills)).SyncRoot)).CA_ConsigneeAddress2)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusSCAOceanBill)(null)).FilteredHouseBills)).SyncRoot)).CA_ConsigneeSuburb)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusSCAOceanBill)(null)).FilteredHouseBills)).SyncRoot)).CA_ConsigneePostcode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusSCAOceanBill)(null)).FilteredHouseBills)).SyncRoot)).CA_ConsigneePhone)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusSCAOceanBill)(null)).FilteredHouseBills)).SyncRoot)).CA_ConsigneeFax)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusSCAOceanBill)(null)).FilteredHouseBills)).SyncRoot)).CA_OH_Consignor)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusSCAOceanBill)(null)).FilteredHouseBills)).SyncRoot)).CA_OH_Consignor_List)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusSCAOceanBill)(null)).FilteredHouseBills)).SyncRoot)).CA_ConsignorName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusSCAOceanBill)(null)).FilteredHouseBills)).SyncRoot)).CA_ConsignorAddress1)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusSCAOceanBill)(null)).FilteredHouseBills)).SyncRoot)).CA_ConsignorAddress2)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusSCAOceanBill)(null)).FilteredHouseBills)).SyncRoot)).CA_ConsignorSuburb)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusSCAOceanBill)(null)).FilteredHouseBills)).SyncRoot)).CA_ConsignorPostcode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusSCAOceanBill)(null)).FilteredHouseBills)).SyncRoot)).CA_OH_Notify)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusSCAOceanBill)(null)).FilteredHouseBills)).SyncRoot)).CA_OH_Notify_List)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusSCAOceanBill)(null)).FilteredHouseBills)).SyncRoot)).CA_NotifyName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusSCAOceanBill)(null)).FilteredHouseBills)).SyncRoot)).CA_NotifyAddress1)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusSCAOceanBill)(null)).FilteredHouseBills)).SyncRoot)).CA_NotifyAddress2)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusSCAOceanBill)(null)).FilteredHouseBills)).SyncRoot)).CA_NotifySuburb)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusSCAOceanBill)(null)).FilteredHouseBills)).SyncRoot)).CA_NotifyPostcode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusSCAOceanBill)(null)).FilteredHouseBills)).SyncRoot)).CA_NotifyPhone)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusSCAOceanBill)(null)).FilteredHouseBills)).SyncRoot)).CA_NotifyFax)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusSCAOceanBill)(null)).FilteredHouseBills)).SyncRoot)).CA_ResponsiblePartyID)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusSCAOceanBill)(null)).FilteredHouseBills)).SyncRoot)).CA_ConsigneeBusinessNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusSCAOceanBill)(null)).FilteredHouseBills)).SyncRoot)).CA_ConsigneeIdentifier)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusSCAOceanBill)(null)).FilteredHouseBills)).SyncRoot)).CA_VendorIdentifier)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusSCAOceanBill)(null)).FilteredHouseBills)).SyncRoot)).CA_ConsignorIdentifier)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusSCAOceanBill)(null)).FilteredHouseBills)).SyncRoot)).CA_OA_ConsigneeAddress)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusSCAOceanBill)(null)).FilteredHouseBills)).SyncRoot)).CA_OA_ConsignorAddress)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusSCAOceanBill)(null)).FilteredHouseBills)).SyncRoot)).ConsigneeOrgPK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusSCAOceanBill)(null)).FilteredHouseBills)).SyncRoot)).ConsignorOrgPK)));
			this.HouseBillsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo4.Caption = "House Bill";
			zTextBoxColumnStyleInfo4.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo4.ColumnName = "CA_HouseBill";
			zTextBoxColumnStyleInfo4.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo4.ToolTip = "House Bill";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCodeFindBoxColumnStyleInfo2.BindToList = "PortOfOriginList";
			zCodeFindBoxColumnStyleInfo2.Caption = "Origin";
			zCodeFindBoxColumnStyleInfo2.ColumnName = "CA_RL_NK_PortOfOrigin";
			zCodeFindBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
			zCodeFindBoxColumnStyleInfo2.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefUNLOCO;
			zCodeFindBoxColumnStyleInfo2.ToolTip = "Port of Origin";
			zCodeFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCodeFindBoxColumnStyleInfo3.BindToList = "PortOfDestinationList";
			zCodeFindBoxColumnStyleInfo3.Caption = "Dest.";
			zCodeFindBoxColumnStyleInfo3.ColumnName = "CA_RL_NK_PortOfDestination";
			zCodeFindBoxColumnStyleInfo3.DefaultCollectionIndex = 0;
			zCodeFindBoxColumnStyleInfo3.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefUNLOCO;
			zCodeFindBoxColumnStyleInfo3.ToolTip = "Port of Destination";
			zCodeFindBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCodeFindBoxColumnStyleInfo4.BindToList = "CountryOfOriginList";
			zCodeFindBoxColumnStyleInfo4.Caption = "Goods Origin";
			zCodeFindBoxColumnStyleInfo4.ColumnName = "CA_RN_NKGoodsOrigin";
			zCodeFindBoxColumnStyleInfo4.DefaultCollectionIndex = 0;
			zCodeFindBoxColumnStyleInfo4.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefCountry;
			zCodeFindBoxColumnStyleInfo4.ToolTip = "Goods\' Ctry/Rgn. of Origin";
			zCodeFindBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo4.BindToList = "Lookups+MethodsOfPayment";
			zDropEditColumnStyleInfo4.Caption = "Payment Type";
			zDropEditColumnStyleInfo4.ColumnName = "CA_PrepaidCollectOther";
			zDropEditColumnStyleInfo4.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo4.ToolTip = "Method of Payment ";
			zDropEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo5.Caption = "Shipment Status";
			zTextBoxColumnStyleInfo5.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo5.ColumnName = "CA_ShipmentStatus";
			zTextBoxColumnStyleInfo5.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo5.ToolTip = "Shipment Status";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zTextBoxColumnStyleInfo6.Caption = "Shipment Status";
			zTextBoxColumnStyleInfo6.ColumnName = "ShipmentStatus";
			zTextBoxColumnStyleInfo6.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo6.IsReadOnly = true;
			zTextBoxColumnStyleInfo6.IsVisible = false;
			zTextBoxColumnStyleInfo6.ToolTip = "Full Customs Shipment Status";
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zTextBoxColumnStyleInfo7.Caption = "Message Status";
			zTextBoxColumnStyleInfo7.ColumnName = "CA_MessageStatus";
			zTextBoxColumnStyleInfo7.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo7.IsReadOnly = true;
			zTextBoxColumnStyleInfo7.ToolTip = "Status of messaging";
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zTextBoxColumnStyleInfo8.Caption = "Message Status";
			zTextBoxColumnStyleInfo8.ColumnName = "MessageStatus";
			zTextBoxColumnStyleInfo8.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo8.IsReadOnly = true;
			zTextBoxColumnStyleInfo8.IsVisible = false;
			zTextBoxColumnStyleInfo8.ToolTip = "Full Message Status Description";
			zTextBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zCheckBoxColumnStyleInfo2.Caption = "FF Ind";
			zCheckBoxColumnStyleInfo2.ColumnName = "CA_IsMasterHouse";
			zCheckBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
			zCheckBoxColumnStyleInfo2.ToolTip = "Is a co load master bill";
			zCheckBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo9.Caption = "Parent Bill";
			zTextBoxColumnStyleInfo9.ColumnName = "CA_MasterHouseBill";
			zTextBoxColumnStyleInfo9.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo9.ToolTip = "Master House Bill Number";
			zTextBoxColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zOrganisationFindBoxColumnStyleInfo1.BindToList = "CA_OH_Consignee_List";
			zOrganisationFindBoxColumnStyleInfo1.Caption = "Consignee";
			zOrganisationFindBoxColumnStyleInfo1.ColumnName = "CA_OH_Consignee";
			zOrganisationFindBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zOrganisationFindBoxColumnStyleInfo1.IsVisible = false;
			zOrganisationFindBoxColumnStyleInfo1.ToolTip = "Consignee Code";
			zOrganisationFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo10.Caption = "Consignee Name";
			zTextBoxColumnStyleInfo10.ColumnName = "CA_ConsigneeName";
			zTextBoxColumnStyleInfo10.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo10.IsVisible = false;
			zTextBoxColumnStyleInfo10.ToolTip = "Consignee Name";
			zTextBoxColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo11.Caption = "Consignee Address 1";
			zTextBoxColumnStyleInfo11.ColumnName = "CA_ConsigneeAddress1";
			zTextBoxColumnStyleInfo11.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo11.IsVisible = false;
			zTextBoxColumnStyleInfo11.ToolTip = "Consignee Address 1";
			zTextBoxColumnStyleInfo11.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo12.Caption = "Consignee Address 2";
			zTextBoxColumnStyleInfo12.ColumnName = "CA_ConsigneeAddress2";
			zTextBoxColumnStyleInfo12.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo12.IsVisible = false;
			zTextBoxColumnStyleInfo12.ToolTip = "Consignee Address 2";
			zTextBoxColumnStyleInfo12.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo13.Caption = "Consignee Suburb";
			zTextBoxColumnStyleInfo13.ColumnName = "CA_ConsigneeSuburb";
			zTextBoxColumnStyleInfo13.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo13.IsVisible = false;
			zTextBoxColumnStyleInfo13.ToolTip = "Consignee Suburb";
			zTextBoxColumnStyleInfo13.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo14.Caption = "Consignee Postcode";
			zTextBoxColumnStyleInfo14.ColumnName = "CA_ConsigneePostcode";
			zTextBoxColumnStyleInfo14.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo14.IsVisible = false;
			zTextBoxColumnStyleInfo14.ToolTip = "Consignee Postcode";
			zTextBoxColumnStyleInfo14.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo15.Caption = "Consignee Phone";
			zTextBoxColumnStyleInfo15.ColumnName = "CA_ConsigneePhone";
			zTextBoxColumnStyleInfo15.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo15.IsVisible = false;
			zTextBoxColumnStyleInfo15.ToolTip = "Consignee Phone";
			zTextBoxColumnStyleInfo15.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo16.Caption = "Consignee Fax";
			zTextBoxColumnStyleInfo16.ColumnName = "CA_ConsigneeFax";
			zTextBoxColumnStyleInfo16.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo16.IsVisible = false;
			zTextBoxColumnStyleInfo16.ToolTip = "Consignee Fax";
			zTextBoxColumnStyleInfo16.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zOrganisationFindBoxColumnStyleInfo2.BindToList = "CA_OH_Consignor_List";
			zOrganisationFindBoxColumnStyleInfo2.Caption = "Consignor";
			zOrganisationFindBoxColumnStyleInfo2.ColumnName = "CA_OH_Consignor";
			zOrganisationFindBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
			zOrganisationFindBoxColumnStyleInfo2.IsVisible = false;
			zOrganisationFindBoxColumnStyleInfo2.ToolTip = "Consignor Code";
			zOrganisationFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo17.Caption = "Consignor Name";
			zTextBoxColumnStyleInfo17.ColumnName = "CA_ConsignorName";
			zTextBoxColumnStyleInfo17.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo17.IsVisible = false;
			zTextBoxColumnStyleInfo17.ToolTip = "Consignor Name";
			zTextBoxColumnStyleInfo17.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo18.Caption = "Consignor Address 1";
			zTextBoxColumnStyleInfo18.ColumnName = "CA_ConsignorAddress1";
			zTextBoxColumnStyleInfo18.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo18.IsVisible = false;
			zTextBoxColumnStyleInfo18.ToolTip = "Consignor Address 1";
			zTextBoxColumnStyleInfo18.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo19.Caption = "Consignor Address 2";
			zTextBoxColumnStyleInfo19.ColumnName = "CA_ConsignorAddress2";
			zTextBoxColumnStyleInfo19.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo19.IsVisible = false;
			zTextBoxColumnStyleInfo19.ToolTip = "Consignor Address 2";
			zTextBoxColumnStyleInfo19.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo20.Caption = "Consignor Suburb";
			zTextBoxColumnStyleInfo20.ColumnName = "CA_ConsignorSuburb";
			zTextBoxColumnStyleInfo20.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo20.IsVisible = false;
			zTextBoxColumnStyleInfo20.ToolTip = "Consginor Suburb";
			zTextBoxColumnStyleInfo20.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo21.Caption = "Consignor Postcode";
			zTextBoxColumnStyleInfo21.ColumnName = "CA_ConsignorPostcode";
			zTextBoxColumnStyleInfo21.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo21.IsVisible = false;
			zTextBoxColumnStyleInfo21.ToolTip = "Consignor Postcode";
			zTextBoxColumnStyleInfo21.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zOrganisationFindBoxColumnStyleInfo3.BindToList = "CA_OH_Notify_List";
			zOrganisationFindBoxColumnStyleInfo3.Caption = "Notify Party";
			zOrganisationFindBoxColumnStyleInfo3.ColumnName = "CA_OH_Notify";
			zOrganisationFindBoxColumnStyleInfo3.DefaultCollectionIndex = 0;
			zOrganisationFindBoxColumnStyleInfo3.IsVisible = false;
			zOrganisationFindBoxColumnStyleInfo3.ToolTip = "Notify Code";
			zOrganisationFindBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo22.Caption = "Notify Name";
			zTextBoxColumnStyleInfo22.ColumnName = "CA_NotifyName";
			zTextBoxColumnStyleInfo22.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo22.IsVisible = false;
			zTextBoxColumnStyleInfo22.ToolTip = "Notify Name";
			zTextBoxColumnStyleInfo22.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo23.Caption = "Notify Address 1";
			zTextBoxColumnStyleInfo23.ColumnName = "CA_NotifyAddress1";
			zTextBoxColumnStyleInfo23.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo23.IsVisible = false;
			zTextBoxColumnStyleInfo23.ToolTip = "Notify Address 1";
			zTextBoxColumnStyleInfo23.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo24.Caption = "Notify Address 2";
			zTextBoxColumnStyleInfo24.ColumnName = "CA_NotifyAddress2";
			zTextBoxColumnStyleInfo24.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo24.IsVisible = false;
			zTextBoxColumnStyleInfo24.ToolTip = "Notify Address 2";
			zTextBoxColumnStyleInfo24.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo25.Caption = "Notify Suburb";
			zTextBoxColumnStyleInfo25.ColumnName = "CA_NotifySuburb";
			zTextBoxColumnStyleInfo25.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo25.IsVisible = false;
			zTextBoxColumnStyleInfo25.ToolTip = "Notify Suburb";
			zTextBoxColumnStyleInfo25.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo26.Caption = "Notify Postcode";
			zTextBoxColumnStyleInfo26.ColumnName = "CA_NotifyPostcode";
			zTextBoxColumnStyleInfo26.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo26.IsVisible = false;
			zTextBoxColumnStyleInfo26.ToolTip = "Notify Postcode";
			zTextBoxColumnStyleInfo26.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo27.Caption = "Notify Phone";
			zTextBoxColumnStyleInfo27.ColumnName = "CA_NotifyPhone";
			zTextBoxColumnStyleInfo27.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo27.IsVisible = false;
			zTextBoxColumnStyleInfo27.ToolTip = "Notify Phone";
			zTextBoxColumnStyleInfo27.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo28.Caption = "Notify Fax";
			zTextBoxColumnStyleInfo28.ColumnName = "CA_NotifyFax";
			zTextBoxColumnStyleInfo28.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo28.IsVisible = false;
			zTextBoxColumnStyleInfo28.ToolTip = "Notify Fax";
			zTextBoxColumnStyleInfo28.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo29.Caption = "Responsible Party I D";
			zTextBoxColumnStyleInfo29.ColumnName = "CA_ResponsiblePartyID";
			zTextBoxColumnStyleInfo29.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo29.ToolTip = "Line level Responsible Party Override";
			zTextBoxColumnStyleInfo29.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo30.Caption = "Consignee ABN";
			zTextBoxColumnStyleInfo30.ColumnName = "CA_ConsigneeBusinessNumber";
			zTextBoxColumnStyleInfo30.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo30.IsVisible = false;
			zTextBoxColumnStyleInfo30.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo31.Caption = "Consignee Identifier";
			zTextBoxColumnStyleInfo31.ColumnName = "CA_ConsigneeIdentifier";
			zTextBoxColumnStyleInfo31.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo31.IsVisible = false;
			zTextBoxColumnStyleInfo31.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo32.Caption = "Consignor Vendor";
			zTextBoxColumnStyleInfo32.ColumnName = "CA_VendorIdentifier";
			zTextBoxColumnStyleInfo32.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo32.IsVisible = false;
			zTextBoxColumnStyleInfo32.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo33.Caption = "Supplier ID";
			zTextBoxColumnStyleInfo33.ColumnName = "CA_ConsignorIdentifier";
			zTextBoxColumnStyleInfo33.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo33.IsVisible = false;
			zTextBoxColumnStyleInfo33.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("b24e368a-8c67-4123-bdc5-d17a39f060e8", "Address", "Consignee Address", "");
			zGuidDropEditColumnStyleInfo1.ColumnName = "CA_OA_ConsigneeAddress";
			zGuidDropEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zGuidDropEditColumnStyleInfo1.GroupName = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("8e9865fc-56b8-49a6-a03d-651c8112ae62", "Consignee");
			zGuidDropEditColumnStyleInfo1.IsVisible = false;
			zGuidDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("53f54162-2f3c-49dd-a164-e37a0747fd10", "Address", "Consignor Address", "");
			zGuidDropEditColumnStyleInfo2.ColumnName = "CA_OA_ConsignorAddress";
			zGuidDropEditColumnStyleInfo2.DefaultCollectionIndex = 0;
			zGuidDropEditColumnStyleInfo2.GroupName = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("ca785cb2-c874-4b33-82e4-7c9b4d7ce89a", "Consignor");
			zGuidDropEditColumnStyleInfo2.IsVisible = false;
			zGuidDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zOrganisationFindBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("53C29EFB-C483-4FEC-B4EF-083E6EB29509", "Consignee");
			zOrganisationFindBoxColumnStyleInfo4.ColumnName = "ConsigneeOrgPK";
			zOrganisationFindBoxColumnStyleInfo4.DefaultCollectionIndex = 0;
			zOrganisationFindBoxColumnStyleInfo4.GroupName = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("8e9865fc-56b8-49a6-a03d-651c8112ae62", "Consignee");
			zOrganisationFindBoxColumnStyleInfo4.IsVisible = false;
			zOrganisationFindBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zOrganisationFindBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("EB7E42F8-32E4-425B-870C-9BBC319D6126", "Consignor");
			zOrganisationFindBoxColumnStyleInfo5.ColumnName = "ConsignorOrgPK";
			zOrganisationFindBoxColumnStyleInfo5.DefaultCollectionIndex = 0;
			zOrganisationFindBoxColumnStyleInfo5.GroupName = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("ca785cb2-c874-4b33-82e4-7c9b4d7ce89a", "Consignor");
			zOrganisationFindBoxColumnStyleInfo5.IsVisible = false;
			zOrganisationFindBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.HouseBillsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.HouseBillsGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo2);
			this.HouseBillsGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo3);
			this.HouseBillsGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo4);
			this.HouseBillsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo4);
			this.HouseBillsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.HouseBillsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.HouseBillsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.HouseBillsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.HouseBillsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.HouseBillsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
			this.HouseBillsGrid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo1);
			this.HouseBillsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo10);
			this.HouseBillsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo11);
			this.HouseBillsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo12);
			this.HouseBillsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo13);
			this.HouseBillsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo14);
			this.HouseBillsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo15);
			this.HouseBillsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo16);
			this.HouseBillsGrid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo2);
			this.HouseBillsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo17);
			this.HouseBillsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo18);
			this.HouseBillsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo19);
			this.HouseBillsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo20);
			this.HouseBillsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo21);
			this.HouseBillsGrid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo3);
			this.HouseBillsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo22);
			this.HouseBillsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo23);
			this.HouseBillsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo24);
			this.HouseBillsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo25);
			this.HouseBillsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo26);
			this.HouseBillsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo27);
			this.HouseBillsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo28);
			this.HouseBillsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo29);
			this.HouseBillsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo30);
			this.HouseBillsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo31);
			this.HouseBillsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo32);
			this.HouseBillsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo33);
			this.HouseBillsGrid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo4);
			this.HouseBillsGrid.ColumnStyles.Add(zGuidDropEditColumnStyleInfo1);
			this.HouseBillsGrid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo5);
			this.HouseBillsGrid.ColumnStyles.Add(zGuidDropEditColumnStyleInfo2);           
			this.HouseBillsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.HouseBillsGrid.GridId = "ea7d565d-11e6-4bf0-832f-2e80b00e5431";
			this.HouseBillsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.HouseBillsGrid.LayoutKey = "zGrid1";
			this.HouseBillsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 30, true);
			this.HouseBillsGrid.Name = "HouseBillsGrid";
			this.HouseBillsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(987, 219, true);
			this.HouseBillsGrid.TabIndex = 1;
			// 
			// SeaCargoSplitter
			// 
			this.SeaCargoSplitter.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.SeaCargoSplitter.DoNotSaveSplitterLayout = false;
			this.SeaCargoSplitter.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 249, true);
			this.SeaCargoSplitter.Name = "SeaCargoSplitter";
			this.SeaCargoSplitter.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(987, 5, true);
			this.SeaCargoSplitter.TabIndex = 2;
			this.SeaCargoSplitter.TabStop = false;
			// 
			// HouseBillTabControl
			// 
			this.HouseBillTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.HouseBillTabControl.Controls.Add(this.DetailsTabPage);
			this.HouseBillTabControl.Controls.Add(this.PackingTabPage);
			this.HouseBillTabControl.Controls.Add(this.HouseBillCustomFieldsTabPage);
			this.HouseBillTabControl.Controls.Add(this.MessagesTabPage);
			this.HouseBillTabControl.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.HouseBillTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 253, true);
			this.HouseBillTabControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(944, 300, true);
			this.HouseBillTabControl.Name = "HouseBillTabControl";
			this.HouseBillTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(987, 300, true);
			this.HouseBillTabControl.TabIndex = 3;
			// 
			// DetailsTabPage
			// 
			this.DetailsTabPage.Controls.Add(this.HouseBillDetailsUserControl);
			this.DetailsTabPage.Controls.Add(this.HouseBillPartiesUserControl);
			this.DetailsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.DetailsTabPage.Name = "DetailsTabPage";
			this.DetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(981, 291, true);
			this.DetailsTabPage.TabIndex = 0;
			this.DetailsTabPage.Text = "Details";
			// 
			// PackingTabPage
			// 
			this.PackingTabPage.Controls.Add(this.UnderbondAndPackingUserControlForCMR);
			this.PackingTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.PackingTabPage.Name = "PackingTabPage";
			this.PackingTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(979, 291, true);
			this.PackingTabPage.TabIndex = 1;
			this.PackingTabPage.Text = "Packing";
			// 
			// UnderbondAndPackingUserControlForCMR
			// 
			this.UnderbondAndPackingUserControlForCMR.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.UnderbondAndPackingUserControlForCMR, "FilteredHouseBills");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusSCAOceanBill)(null)).FilteredHouseBills)).SyncRoot)))));
			this.UnderbondAndPackingUserControlForCMR.Dock = System.Windows.Forms.DockStyle.Fill;
			this.UnderbondAndPackingUserControlForCMR.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.UnderbondAndPackingUserControlForCMR.Name = "UnderbondAndPackingUserControlForCMR";
			this.UnderbondAndPackingUserControlForCMR.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(979, 291, true);
			this.UnderbondAndPackingUserControlForCMR.TabIndex = 4;
			// 
			// HouseBillCustomFieldsTabPage
			// 
			this.HouseBillCustomFieldsTabPage.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("C26B9CD3-BD1C-48D9-9BDE-9D3B5A6C6FE2", "Custom Fields");
			this.HouseBillCustomFieldsTabPage.Controls.Add(this.HouseBillCustomFieldsControl);
			this.HouseBillCustomFieldsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.HouseBillCustomFieldsTabPage.Name = "HouseBillCustomFieldsTabPage";
			this.HouseBillCustomFieldsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(979, 291, true);
			this.HouseBillCustomFieldsTabPage.TabIndex = 2;
			this.HouseBillCustomFieldsTabPage.Text = "Custom Fields";
			// 
			// HouseBillCustomFieldsControl
			// 
			this.HouseBillCustomFieldsControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.HouseBillCustomFieldsControl, "FilteredHouseBills");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusSCAOceanBill)(null)).FilteredHouseBills)).SyncRoot)))));
			this.HouseBillCustomFieldsControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.HouseBillCustomFieldsControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.HouseBillCustomFieldsControl.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.HouseBillCustomFieldsControl.Name = "HouseBillCustomFieldsControl";
			this.HouseBillCustomFieldsControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(979, 291, true);
			this.HouseBillCustomFieldsControl.TabIndex = 1;
			// 
			// MessagesTabPage
			// 
			this.MessagesTabPage.Controls.Add(this.MessageUserControl);
			this.MessagesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.MessagesTabPage.Name = "MessagesTabPage";
			this.MessagesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(979, 291, true);
			this.MessagesTabPage.TabIndex = 3;
			this.MessagesTabPage.Text = "Messages";
			// 
			// MessageUserControl
			// 
			this.MessageUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.MessageUserControl, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Messaging.Business.IEDIMessageCollectionProvider)(((Enterprise.Customs.AU.Declaration.Business.CusSCAOceanBill)(null)))));
			this.MessageUserControl.BindPrepend = "";
			this.MessageUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MessageUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MessageUserControl.Name = "MessageUserControl";
			this.MessageUserControl.ShowChangingBlueMessageHeading = false;
			this.MessageUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(979, 291, true);
			this.MessageUserControl.TabIndex = 0;
			// 
			// filterPanel
			// 
			this.filterPanel.Controls.Add(this.housesWithNotificationsCheckBox);
			this.filterPanel.Controls.Add(this.customsMessageStatusDropEdit);
			this.filterPanel.Controls.Add(this.customsMessageStatusLabel);
			this.filterPanel.Controls.Add(this.clearButton);
			this.filterPanel.Controls.Add(this.filterButton);
			this.filterPanel.Controls.Add(this.customsCargoStatusDropEdit);
			this.filterPanel.Controls.Add(this.customsCargoStatusLabel);
			this.filterPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.filterPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.filterPanel.Name = "filterPanel";
			this.filterPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(987, 30, true);
			this.filterPanel.TabIndex = 0;
			// 
			// housesWithNotificationsCheckBox
			// 
			this.housesWithNotificationsCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.housesWithNotificationsCheckBox, "InvalidHouseBillsOnlyFilter");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.AU.Declaration.Business.CusSCAOceanBill)(null)).InvalidHouseBillsOnlyFilter)));
			this.housesWithNotificationsCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(410, 7, true);
			this.housesWithNotificationsCheckBox.Name = "housesWithNotificationsCheckBox";
			this.housesWithNotificationsCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(171, 16, true);
			this.housesWithNotificationsCheckBox.TabIndex = 4;
			this.housesWithNotificationsCheckBox.Text = "Show Houses with Notifications";
			this.housesWithNotificationsCheckBox.UseVisualStyleBackColor = false;
			// 
			// customsMessageStatusDropEdit
			// 
			this.customsMessageStatusDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.customsMessageStatusDropEdit, "CustomsMessageStatusFilter");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAOceanBill)(null)).CustomsMessageStatusFilter)));
			this.customsMessageStatusDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(331, 5, true);
			this.customsMessageStatusDropEdit.Name = "customsMessageStatusDropEdit";
			this.customsMessageStatusDropEdit.PreBoundMaxLength = 3;
			this.customsMessageStatusDropEdit.ShowDescriptionBox = false;
			this.customsMessageStatusDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(69, 17, true);
			this.customsMessageStatusDropEdit.TabIndex = 3;
			// 
			// customsMessageStatusLabel
			// 
			this.customsMessageStatusLabel.AutoSize = true;
			this.customsMessageStatusLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.customsMessageStatusLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(199, 8, true);
			this.customsMessageStatusLabel.Name = "customsMessageStatusLabel";
			this.customsMessageStatusLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(128, 13, true);
			this.customsMessageStatusLabel.TabIndex = 2;
			this.customsMessageStatusLabel.Text = "Customs Message Status";
			this.customsMessageStatusLabel.UseMnemonic = false;
			// 
			// clearButton
			// 
			this.clearButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.clearButton.IsCaptionOverridden = true;
			this.clearButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(909, 3, true);
			this.clearButton.Name = "clearButton";
			this.clearButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.clearButton.TabIndex = 6;
			this.clearButton.Text = "Clear";
			this.clearButton.ToolTipCaption = null;
			this.clearButton.UseVisualStyleBackColor = false;
			this.clearButton.Click += new System.EventHandler(this.OnClearButtonClick);
			// 
			// filterButton
			// 
			this.filterButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.filterButton.IsCaptionOverridden = true;
			this.filterButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(827, 3, true);
			this.filterButton.Name = "filterButton";
			this.filterButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.filterButton.TabIndex = 5;
			this.filterButton.Text = "Filter";
			this.filterButton.ToolTipCaption = null;
			this.filterButton.UseVisualStyleBackColor = false;
			this.filterButton.Click += new System.EventHandler(this.OnFilterButtonClick);
			// 
			// customsCargoStatusDropEdit
			// 
			this.customsCargoStatusDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.customsCargoStatusDropEdit, "CustomsShipmentStatusFilter");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAOceanBill)(null)).CustomsShipmentStatusFilter)));
			this.customsCargoStatusDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 5, true);
			this.customsCargoStatusDropEdit.Name = "customsCargoStatusDropEdit";
			this.customsCargoStatusDropEdit.PreBoundMaxLength = 3;
			this.customsCargoStatusDropEdit.ShowDescriptionBox = false;
			this.customsCargoStatusDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(69, 17, true);
			this.customsCargoStatusDropEdit.TabIndex = 1;
			// 
			// customsCargoStatusLabel
			// 
			this.customsCargoStatusLabel.AutoSize = true;
			this.customsCargoStatusLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.customsCargoStatusLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 8, true);
			this.customsCargoStatusLabel.Name = "customsCargoStatusLabel";
			this.customsCargoStatusLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(113, 13, true);
			this.customsCargoStatusLabel.TabIndex = 0;
			this.customsCargoStatusLabel.Text = "Customs Cargo Status";
			this.customsCargoStatusLabel.UseMnemonic = false;
			// 
			// CustomFieldsTabPage
			// 
			this.CustomFieldsTabPage.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("C702D26C-861F-4F11-97A1-F9BB98F3E4EA", "Custom Fields");
			this.CustomFieldsTabPage.Controls.Add(this.CustomFieldsControl);
			this.CustomFieldsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.CustomFieldsTabPage.Name = "CustomFieldsTabPage";
			this.CustomFieldsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(987, 566, true);
			this.CustomFieldsTabPage.TabIndex = 2;
			this.CustomFieldsTabPage.Text = "Custom Fields";
			// 
			// CustomFieldsControl
			// 
			this.CustomFieldsControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CustomFieldsControl, "OceanBill");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.AU.Declaration.Business.CusSCAOceanBill)(((Enterprise.Customs.AU.Declaration.Business.CusSCAOceanBill)(null)).OceanBill)));
			this.CustomFieldsControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CustomFieldsControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.CustomFieldsControl.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.CustomFieldsControl.Name = "CustomFieldsControl";
			this.CustomFieldsControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(987, 566, true);
			this.CustomFieldsControl.TabIndex = 1;
			// 
			// CMRSeaCargoUserControl
			// 
			this.CaptionRenderingEnabled = false;
			this.Controls.Add(this.OceanBillSpecificsTabControl);
			this.Controls.Add(this.OceanBillDetailsUserControl);
			this.Name = "CMRSeaCargoUserControl";
			this.ShouldSerializeTabPageMethods = false;
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(992, 696, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.OceanBillDetailsUserControl.ResumeLayout(true);
			this.OceanBillDetailsUserControl.PerformLayout();
			this.HouseBillDetailsUserControl.ResumeLayout(true);
			this.HouseBillDetailsUserControl.PerformLayout();
			this.HouseBillPartiesUserControl.ResumeLayout(true);
			this.HouseBillPartiesUserControl.PerformLayout();
			this.OceanBillSpecificsTabControl.ResumeLayout(false);
			this.OceanBillSpecificsTabControl.PerformLayout();
			this.ContainersTabPage.ResumeLayout(false);
			this.ContainersTabPage.PerformLayout();
			this.ContainerGridPanel.ResumeLayout(false);
			this.ContainerGridPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.CusSCAContainersBoundGrid)).EndInit();
			this.CusSCAContainersBoundGrid.ResumeLayout(false);
			this.CusSCAContainersBoundGrid.PerformLayout();
			this.PackingForContainersControl.ResumeLayout(true);
			this.PackingForContainersControl.PerformLayout();
			this.HouseBillsTabPage.ResumeLayout(false);
			this.HouseBillsTabPage.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.HouseBillsGrid)).EndInit();
			this.HouseBillsGrid.ResumeLayout(false);
			this.HouseBillsGrid.PerformLayout();
			this.HouseBillTabControl.ResumeLayout(false);
			this.HouseBillTabControl.PerformLayout();
			this.DetailsTabPage.ResumeLayout(false);
			this.DetailsTabPage.PerformLayout();
			this.PackingTabPage.ResumeLayout(false);
			this.PackingTabPage.PerformLayout();
			this.UnderbondAndPackingUserControlForCMR.ResumeLayout(true);
			this.UnderbondAndPackingUserControlForCMR.PerformLayout();
			this.HouseBillCustomFieldsTabPage.ResumeLayout(false);
			this.HouseBillCustomFieldsTabPage.PerformLayout();
			this.HouseBillCustomFieldsControl.ResumeLayout(true);
			this.HouseBillCustomFieldsControl.PerformLayout();
			this.MessagesTabPage.ResumeLayout(false);
			this.MessagesTabPage.PerformLayout();
			this.MessageUserControl.ResumeLayout(true);
			this.MessageUserControl.PerformLayout();
			this.filterPanel.ResumeLayout(false);
			this.filterPanel.PerformLayout();
			this.customsMessageStatusDropEdit.ResumeLayout(true);
			this.customsMessageStatusDropEdit.PerformLayout();
			this.customsCargoStatusDropEdit.ResumeLayout(true);
			this.customsCargoStatusDropEdit.PerformLayout();
			this.CustomFieldsTabPage.ResumeLayout(false);
			this.CustomFieldsTabPage.PerformLayout();
			this.CustomFieldsControl.ResumeLayout(true);
			this.CustomFieldsControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
	}
}
