namespace Enterprise.Customs.DE.GUI
{
	partial class CHGBaseDeclarationUserControl
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
			this.components = new System.ComponentModel.Container();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZMultiControlColumnStyleInfo zMultiControlColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZMultiControlColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo zOrganisationFindBoxColumnStyleInfo1 = new Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo zGuidDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo zOrganisationFindBoxColumnStyleInfo2 = new Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo zGuidDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo5 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.declarationAndLinesSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.DecsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.LinesTabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.LinesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.LinesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.MessagesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.MessagesUserControl = new Enterprise.Customs.DE.GUI.MessagesUserControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.declarationAndLinesSplitContainer)).BeginInit();
			this.declarationAndLinesSplitContainer.Panel1.SuspendLayout();
			this.declarationAndLinesSplitContainer.Panel2.SuspendLayout();
			this.declarationAndLinesSplitContainer.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.DecsGrid)).BeginInit();
			this.DecsGrid.SuspendLayout();
			this.LinesTabControl.SuspendLayout();
			this.LinesTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.LinesGrid)).BeginInit();
			this.LinesGrid.SuspendLayout();
			this.MessagesTabPage.SuspendLayout();
			this.MessagesUserControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageJobHeader);
			// 
			// declarationAndLinesSplitContainer
			// 
			this.declarationAndLinesSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.declarationAndLinesSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.declarationAndLinesSplitContainer.Name = "declarationAndLinesSplitContainer";
			this.declarationAndLinesSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// declarationAndLinesSplitContainer.Panel1
			// 
			this.declarationAndLinesSplitContainer.Panel1.Controls.Add(this.DecsGrid);
			// 
			// declarationAndLinesSplitContainer.Panel2
			// 
			this.declarationAndLinesSplitContainer.Panel2.Controls.Add(this.LinesTabControl);
			this.declarationAndLinesSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1253, 649, true);
			this.declarationAndLinesSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(199);
			this.declarationAndLinesSplitContainer.TabIndex = 1;
			// 
			// DecsGrid
			// 
			this.DecsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.DecsGrid, "CusTempStorageDecs");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageJobHeader)(null)).CusTempStorageDecs)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageDec)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageJobHeader)(null)).CusTempStorageDecs)).SyncRoot)).STH_IdentificationIndicator)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageDec)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageJobHeader)(null)).CusTempStorageDecs)).SyncRoot)).STH_SystemCreateTimeUtc)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageDec)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageJobHeader)(null)).CusTempStorageDecs)).SyncRoot)).STH_MessageStatus)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageDec)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageJobHeader)(null)).CusTempStorageDecs)).SyncRoot)).FormattedOwnerReferenceNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageDec)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageJobHeader)(null)).CusTempStorageDecs)).SyncRoot)).ReferenceNumberColumnFieldType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageDec)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageJobHeader)(null)).CusTempStorageDecs)).SyncRoot)).Lookups.CusTempStorageRegLineCollection)));
			this.DecsGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo1.ColumnName = "STH_IdentificationIndicator";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(62);
			zDateEditColumnStyleInfo1.ColumnName = "STH_SystemCreateTimeUtc";
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(95);
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "STH_MessageStatus";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zMultiControlColumnStyleInfo1.BindToDecimalPlaces = null;
			zMultiControlColumnStyleInfo1.BindToList = "Lookups.CusTempStorageRegLineCollection";
			zMultiControlColumnStyleInfo1.ColumnName = "FormattedOwnerReferenceNumber";
			zMultiControlColumnStyleInfo1.FieldTypeColumnName = "ReferenceNumberColumnFieldType";
			zMultiControlColumnStyleInfo1.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.EU.DE.ImportFromSumARegister;
			zMultiControlColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(187);
			this.DecsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.DecsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.DecsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.DecsGrid.ColumnStyles.Add(zMultiControlColumnStyleInfo1);
			this.DecsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DecsGrid.GridId = "eb6e1998-dfc3-42df-96af-0026f0102ae3";
			this.DecsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.DecsGrid.LayoutKey = "DecsGrid";
			this.DecsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DecsGrid.Name = "DecsGrid";
			this.DecsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1253, 199, true);
			this.DecsGrid.TabIndex = 0;
			this.DecsGrid.AfterBind += new System.EventHandler(this.DecsGrid_AfterBind);
			// 
			// LinesTabControl
			// 
			this.LinesTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.LinesTabControl.Controls.Add(this.LinesTabPage);
			this.LinesTabControl.Controls.Add(this.MessagesTabPage);
			this.LinesTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LinesTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.LinesTabControl.Name = "LinesTabControl";
			this.LinesTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1253, 446, true);
			this.LinesTabControl.TabIndex = 1;
			// 
			// LinesTabPage
			// 
			this.LinesTabPage.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("ec157e47-ff73-42e2-87ae-2979cd909cff", "Lines");
			this.LinesTabPage.Controls.Add(this.LinesGrid);
			this.LinesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.LinesTabPage.Name = "LinesTabPage";
			this.LinesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1245, 419, true);
			this.LinesTabPage.TabIndex = 0;
			// 
			// LinesGrid
			// 
			this.LinesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.LinesGrid, "CusTempStorageDecs.CusTempStorageLines");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageDec)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageJobHeader)(null)).CusTempStorageDecs)).SyncRoot)).CusTempStorageLines)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.Business.CusTempStorage.CusTempStorageLine)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageDec)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageJobHeader)(null)).CusTempStorageDecs)).SyncRoot)).CusTempStorageLines)).SyncRoot)).TSL_LineNo)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.CusTempStorage.CusTempStorageLine)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageDec)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageJobHeader)(null)).CusTempStorageDecs)).SyncRoot)).CusTempStorageLines)).SyncRoot)).TSL_OwnerReferenceType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.EU.Business.CusTempStorage.CusTempStorageLine)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageDec)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageJobHeader)(null)).CusTempStorageDecs)).SyncRoot)).CusTempStorageLines)).SyncRoot)).CustodianOrgPK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusTempStorage.CusTempStorageLine)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageDec)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageJobHeader)(null)).CusTempStorageDecs)).SyncRoot)).CusTempStorageLines)).SyncRoot)).Lookups.OrganizationsFindBoxList)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.EU.Business.CusTempStorage.CusTempStorageLine)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageDec)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageJobHeader)(null)).CusTempStorageDecs)).SyncRoot)).CusTempStorageLines)).SyncRoot)).TSL_OA_Custodian)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusTempStorage.CusTempStorageLine)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageDec)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageJobHeader)(null)).CusTempStorageDecs)).SyncRoot)).CusTempStorageLines)).SyncRoot)).TSL_OA_Custodian_ZAddress.OrgAddress_List)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.CusTempStorage.CusTempStorageLine)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageDec)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageJobHeader)(null)).CusTempStorageDecs)).SyncRoot)).CusTempStorageLines)).SyncRoot)).TSL_CustodianIdentifier)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.CusTempStorage.CusTempStorageLine)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageDec)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageJobHeader)(null)).CusTempStorageDecs)).SyncRoot)).CusTempStorageLines)).SyncRoot)).TSL_CustodianIdentifierBranchNo)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.EU.Business.CusTempStorage.CusTempStorageLine)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageDec)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageJobHeader)(null)).CusTempStorageDecs)).SyncRoot)).CusTempStorageLines)).SyncRoot)).GoodsOwnerOrgPK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusTempStorage.CusTempStorageLine)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageDec)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageJobHeader)(null)).CusTempStorageDecs)).SyncRoot)).CusTempStorageLines)).SyncRoot)).Lookups.OrganizationsFindBoxList)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.EU.Business.CusTempStorage.CusTempStorageLine)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageDec)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageJobHeader)(null)).CusTempStorageDecs)).SyncRoot)).CusTempStorageLines)).SyncRoot)).TSL_OA_GoodsOwner)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.EU.Business.CusTempStorage.CusTempStorageLine)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageDec)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageJobHeader)(null)).CusTempStorageDecs)).SyncRoot)).CusTempStorageLines)).SyncRoot)).TSL_OA_GoodsOwner_ZAddress.OrgAddress_List)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.CusTempStorage.CusTempStorageLine)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageDec)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageJobHeader)(null)).CusTempStorageDecs)).SyncRoot)).CusTempStorageLines)).SyncRoot)).TSL_GoodsOwnerIdentifier)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.CusTempStorage.CusTempStorageLine)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageDec)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageJobHeader)(null)).CusTempStorageDecs)).SyncRoot)).CusTempStorageLines)).SyncRoot)).TSL_GoodsOwnerIdentifierBranchNo)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.CusTempStorage.CusTempStorageLine)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageDec)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageJobHeader)(null)).CusTempStorageDecs)).SyncRoot)).CusTempStorageLines)).SyncRoot)).TSL_LocationOfGoods)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.CusTempStorage.CusTempStorageLine)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageDec)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageJobHeader)(null)).CusTempStorageDecs)).SyncRoot)).CusTempStorageLines)).SyncRoot)).TSL_CustomsStatus)));
			this.LinesGrid.CaptionVisible = false;
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("5de510fe-1a8e-42eb-b7dd-9a8847b16682", "Reference Line No.");
			zCalcEditColumnStyleInfo1.ColumnName = "TSL_LineNo";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(105);
			zDropEditColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo2.ColumnName = "TSL_OwnerReferenceType";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(103);
			zOrganisationFindBoxColumnStyleInfo1.BindToList = "Lookups.OrganizationsFindBoxList";
			zOrganisationFindBoxColumnStyleInfo1.ColumnName = "CustodianOrgPK";
			zOrganisationFindBoxColumnStyleInfo1.GroupName = Enterprise.Customs.DE.GUI.Res.GetData("d6246497-f1f5-4116-82d3-d6c97df1f5f7", "Custodian");
			zOrganisationFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidDropEditColumnStyleInfo1.BindToList = "TSL_OA_Custodian_ZAddress.OrgAddress_List";
			zGuidDropEditColumnStyleInfo1.Caption = "";
			zGuidDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zGuidDropEditColumnStyleInfo1.ColumnName = "TSL_OA_Custodian";
			zGuidDropEditColumnStyleInfo1.GroupName = Enterprise.Customs.DE.GUI.Res.GetData("d6246497-f1f5-4116-82d3-d6c97df1f5f7", "Custodian");
			zGuidDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(155);
			zTextBoxColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo2.ColumnName = "TSL_CustodianIdentifier";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(146);
			zDropEditColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo3.ColumnName = "TSL_CustodianIdentifierBranchNo";
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(155);
			zOrganisationFindBoxColumnStyleInfo2.BindToList = "Lookups.OrganizationsFindBoxList";
			zOrganisationFindBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("ef5dd0f4-001b-4ff3-a27c-1369afadaba5", "New Entitled Trader");
			zOrganisationFindBoxColumnStyleInfo2.ColumnName = "GoodsOwnerOrgPK";
			zOrganisationFindBoxColumnStyleInfo2.GroupName = Enterprise.Customs.DE.GUI.Res.GetData("5329c21e-455e-4d77-8472-f7fdaa6a9c74", "Disposal Entitled Trader");
			zOrganisationFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(137);
			zGuidDropEditColumnStyleInfo2.BindToList = "TSL_OA_GoodsOwner_ZAddress.OrgAddress_List";
			zGuidDropEditColumnStyleInfo2.Caption = "";
			zGuidDropEditColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zGuidDropEditColumnStyleInfo2.ColumnName = "TSL_OA_GoodsOwner";
			zGuidDropEditColumnStyleInfo2.GroupName = Enterprise.Customs.DE.GUI.Res.GetData("5329c21e-455e-4d77-8472-f7fdaa6a9c74", "Disposal Entitled Trader");
			zGuidDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(234);
			zTextBoxColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo3.ColumnName = "TSL_GoodsOwnerIdentifier";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(146);
			zDropEditColumnStyleInfo4.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo4.ColumnName = "TSL_GoodsOwnerIdentifierBranchNo";
			zDropEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(155);
			zDropEditColumnStyleInfo5.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo5.ColumnName = "TSL_LocationOfGoods";
			zDropEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(122);
			zTextBoxColumnStyleInfo4.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo4.ColumnName = "TSL_CustomsStatus";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(98);
			this.LinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.LinesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.LinesGrid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo1);
			this.LinesGrid.ColumnStyles.Add(zGuidDropEditColumnStyleInfo1);
			this.LinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.LinesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.LinesGrid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo2);
			this.LinesGrid.ColumnStyles.Add(zGuidDropEditColumnStyleInfo2);
			this.LinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.LinesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo4);
			this.LinesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo5);
			this.LinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.LinesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LinesGrid.GridId = "83e50ed8-a54b-46fb-b2dd-0a33d186b95d";
			this.LinesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.LinesGrid.LayoutKey = "LinesGrid";
			this.LinesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.LinesGrid.Name = "LinesGrid";
			this.LinesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1245, 419, true);
			this.LinesGrid.TabIndex = 0;
			this.LinesGrid.AfterBind += new System.EventHandler(this.AfterBind_LinesGrid);
			// 
			// MessagesTabPage
			// 
			this.MessagesTabPage.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("594381c6-9c31-4c52-af98-b4ae2c288432", "Messages");
			this.MessagesTabPage.Controls.Add(this.MessagesUserControl);
			this.MessagesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.MessagesTabPage.Name = "MessagesTabPage";
			this.MessagesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1245, 419, true);
			this.MessagesTabPage.TabIndex = 1;
			// 
			// MessagesUserControl
			// 
			this.MessagesUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.MessagesUserControl, "CusTempStorageDecs.Messages");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Messaging.Business.EDIMessageCollection)(((Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageDec)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageJobHeader)(null)).CusTempStorageDecs)).SyncRoot)).Messages)));
			this.MessagesUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MessagesUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MessagesUserControl.Name = "MessagesUserControl";
			this.MessagesUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1245, 419, true);
			this.MessagesUserControl.TabIndex = 0;
			// 
			// CHGBaseDeclarationUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.declarationAndLinesSplitContainer);
			this.Name = "CHGBaseDeclarationUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1253, 649, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.declarationAndLinesSplitContainer.Panel1.ResumeLayout(false);
			this.declarationAndLinesSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.declarationAndLinesSplitContainer)).EndInit();
			this.declarationAndLinesSplitContainer.ResumeLayout(false);
			this.declarationAndLinesSplitContainer.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.DecsGrid)).EndInit();
			this.DecsGrid.ResumeLayout(false);
			this.DecsGrid.PerformLayout();
			this.LinesTabControl.ResumeLayout(false);
			this.LinesTabControl.PerformLayout();
			this.LinesTabPage.ResumeLayout(false);
			this.LinesTabPage.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.LinesGrid)).EndInit();
			this.LinesGrid.ResumeLayout(false);
			this.LinesGrid.PerformLayout();
			this.MessagesTabPage.ResumeLayout(false);
			this.MessagesTabPage.PerformLayout();
			this.MessagesUserControl.ResumeLayout(true);
			this.MessagesUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.ZGrid DecsGrid;
		protected internal ZArchitecture.ZGrid LinesGrid;
		protected CargoWise.Windows.UI.KSplitContainer declarationAndLinesSplitContainer;
		protected Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo DefaultOwnerReferenceNumberColumnStyle;
		protected ZArchitecture.GUI.ZTabControl LinesTabControl;
		protected internal ZArchitecture.GUI.ZTabPage MessagesTabPage;
		protected internal ZArchitecture.GUI.ZTabPage LinesTabPage;
		protected MessagesUserControl MessagesUserControl;
	}
}
