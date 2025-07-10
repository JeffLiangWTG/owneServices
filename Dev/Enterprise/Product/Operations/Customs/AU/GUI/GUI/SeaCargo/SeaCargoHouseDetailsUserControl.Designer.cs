namespace Enterprise.Customs.AU.SeaCargo.GUI
{
	public partial class SeaCargoHouseDetailsUserControl
	{
		internal Enterprise.ZArchitecture.ZLabel ParentBillLabel;
		internal Enterprise.ZArchitecture.ZTextBox ParentBillTextBox;
		internal Enterprise.ZArchitecture.ZLabel PrincipalIDLabel;
		internal Enterprise.ZArchitecture.ZTextBox CB_PrincipalIDBoundTextBox;
		internal Enterprise.ZArchitecture.ZLabel ShippingLineLabel;
		internal Enterprise.ZArchitecture.GUI.ZGuidFindBox ShippingLineGuidBoundFindBox;
		Enterprise.ZArchitecture.GUI.ZGroupBox GroupBoxHouseBill;
		Enterprise.ZArchitecture.GUI.ZCodeFindBox CA_RN_NKGoodsOriginBoundCodeFindBox;
		Enterprise.ZArchitecture.GUI.ZCodeFindBox CA_RL_NK_PortOfDestinationBoundCodeFindBox;
		Enterprise.ZArchitecture.GUI.ZCodeFindBox CA_RL_NK_PortOfOriginBoundCodeFindBox;
		Enterprise.ZArchitecture.GUI.ZDropEdit CA_PrepaidCollectOtherBoundDropEdit;
		Enterprise.ZArchitecture.ZLabel zLabel6;
		Enterprise.ZArchitecture.ZLabel zLabel5;
		Enterprise.ZArchitecture.ZLabel zLabel4;
		Enterprise.ZArchitecture.ZLabel zLabel3;
		Enterprise.ZArchitecture.ZLabel zLabel1;
		Enterprise.ZArchitecture.ZTextBox CA_HouseBillBoundTextBox;
		HouseBillPartiesUserControl HouseBillPartiesUserControl;
		Enterprise.ZArchitecture.GUI.ZTemplateTabControl BillDetailsTabControl;
		Enterprise.ZArchitecture.GUI.ZTabPage HouseBillDetailsTabPage;
		Enterprise.ZArchitecture.GUI.ZTabPage OceanBillDetailsTabPage;
		Enterprise.ZArchitecture.GUI.ZCodeFindBox CB_RL_NKPortOfDischargeBoundCodeFindBox;
		Enterprise.ZArchitecture.GUI.ZCodeFindBox CB_RL_NKPortOfLoadingBoundCodeFindBox;
		Enterprise.ZArchitecture.ZLabel zLabel31;
		Enterprise.ZArchitecture.ZLabel zLabel22;
		Enterprise.ZArchitecture.ZLabel zLabel23;
		Enterprise.ZArchitecture.ZTextBox CB_VoyageBoundTextBox;
		Enterprise.ZArchitecture.ZLabel zLabel24;
		Enterprise.ZArchitecture.ZLabel zLabel25;
		Enterprise.ZArchitecture.ZTextBox CB_OceanBillBoundTextBox;
		Enterprise.ZArchitecture.GUI.ZCodeFindBox CB_RV_NKVesselBoundFindBox;
		Enterprise.ZArchitecture.ZLabel zLabel37;
		Enterprise.ZArchitecture.ZLabel zLabel38;
		Enterprise.ZArchitecture.ZLabel zLabel32;
		Enterprise.ZArchitecture.GUI.ZDropEdit CB_ApplicationCodeBoundDropEdit;
		Enterprise.ZArchitecture.ZTextBox ResponsiblePartyIDBoundTextBox;
		Enterprise.ZArchitecture.ZTextBox PrincipalIDBoundTextBox;
		Enterprise.ZArchitecture.GUI.ZCheckBox FreightForwarderIndicatorCheckBox;
		Enterprise.ZArchitecture.ZTextBox zTextBox1;
		Enterprise.ZArchitecture.ZTextBox zTextBox2;
		Enterprise.ZArchitecture.ZTextBox zTextBox3;
		Enterprise.ZArchitecture.ZLabel zLabel2;
		Enterprise.ZArchitecture.ZLabel zLabel39;
		System.ComponentModel.IContainer components;
		Enterprise.ZArchitecture.ZLabel zLabel41;
		Enterprise.ZArchitecture.ZTextBox responsiblePartyTextBox;
		Enterprise.ZArchitecture.ZTextBox CA_ShipmentStatusBoundTextBox;
		Enterprise.ZArchitecture.GUI.ZCheckBox overrideFreightDefaultsCheckBox;

		void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.GroupBoxHouseBill = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.zLabel41 = new Enterprise.ZArchitecture.ZLabel();
			this.responsiblePartyTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.zLabel39 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel2 = new Enterprise.ZArchitecture.ZLabel();
			this.zTextBox3 = new Enterprise.ZArchitecture.ZTextBox();
			this.zTextBox2 = new Enterprise.ZArchitecture.ZTextBox();
			this.zTextBox1 = new Enterprise.ZArchitecture.ZTextBox();
			this.FreightForwarderIndicatorCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.zLabel32 = new Enterprise.ZArchitecture.ZLabel();
			this.CB_ApplicationCodeBoundDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ParentBillLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ParentBillTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CA_RN_NKGoodsOriginBoundCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.CA_RL_NK_PortOfDestinationBoundCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.CA_RL_NK_PortOfOriginBoundCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.CA_PrepaidCollectOtherBoundDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.zLabel6 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel5 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel4 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel3 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel1 = new Enterprise.ZArchitecture.ZLabel();
			this.CA_HouseBillBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CA_ShipmentStatusBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.HouseBillPartiesUserControl = new Enterprise.Customs.AU.SeaCargo.GUI.HouseBillPartiesUserControl();
			this.BillDetailsTabControl = new Enterprise.ZArchitecture.GUI.ZTemplateTabControl();
			this.HouseBillDetailsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.OceanBillDetailsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.zLabel38 = new Enterprise.ZArchitecture.ZLabel();
			this.PrincipalIDBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.zLabel37 = new Enterprise.ZArchitecture.ZLabel();
			this.ResponsiblePartyIDBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ShippingLineGuidBoundFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.ShippingLineLabel = new Enterprise.ZArchitecture.ZLabel();
			this.CB_RV_NKVesselBoundFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.CB_RL_NKPortOfDischargeBoundCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.CB_RL_NKPortOfLoadingBoundCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.PrincipalIDLabel = new Enterprise.ZArchitecture.ZLabel();
			this.CB_PrincipalIDBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.zLabel31 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel22 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel23 = new Enterprise.ZArchitecture.ZLabel();
			this.CB_VoyageBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.zLabel24 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel25 = new Enterprise.ZArchitecture.ZLabel();
			this.CB_OceanBillBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.overrideFreightDefaultsCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.GroupBoxHouseBill.SuspendLayout();
			this.CB_ApplicationCodeBoundDropEdit.SuspendLayout();
			this.CA_RN_NKGoodsOriginBoundCodeFindBox.SuspendLayout();
			this.CA_RL_NK_PortOfDestinationBoundCodeFindBox.SuspendLayout();
			this.CA_RL_NK_PortOfOriginBoundCodeFindBox.SuspendLayout();
			this.CA_PrepaidCollectOtherBoundDropEdit.SuspendLayout();
			this.HouseBillPartiesUserControl.SuspendLayout();
			this.BillDetailsTabControl.SuspendLayout();
			this.HouseBillDetailsTabPage.SuspendLayout();
			this.OceanBillDetailsTabPage.SuspendLayout();
			this.ShippingLineGuidBoundFindBox.SuspendLayout();
			this.CB_RV_NKVesselBoundFindBox.SuspendLayout();
			this.CB_RL_NKPortOfDischargeBoundCodeFindBox.SuspendLayout();
			this.CB_RL_NKPortOfLoadingBoundCodeFindBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.AU.Declaration.Business.CusSCAHouse);
			// 
			// GroupBoxHouseBill
			// 
			this.GroupBoxHouseBill.Controls.Add(this.zLabel41);
			this.GroupBoxHouseBill.Controls.Add(this.responsiblePartyTextBox);
			this.GroupBoxHouseBill.Controls.Add(this.zLabel39);
			this.GroupBoxHouseBill.Controls.Add(this.zLabel2);
			this.GroupBoxHouseBill.Controls.Add(this.zTextBox3);
			this.GroupBoxHouseBill.Controls.Add(this.zTextBox2);
			this.GroupBoxHouseBill.Controls.Add(this.zTextBox1);
			this.GroupBoxHouseBill.Controls.Add(this.FreightForwarderIndicatorCheckBox);
			this.GroupBoxHouseBill.Controls.Add(this.zLabel32);
			this.GroupBoxHouseBill.Controls.Add(this.CB_ApplicationCodeBoundDropEdit);
			this.GroupBoxHouseBill.Controls.Add(this.ParentBillLabel);
			this.GroupBoxHouseBill.Controls.Add(this.ParentBillTextBox);
			this.GroupBoxHouseBill.Controls.Add(this.CA_RN_NKGoodsOriginBoundCodeFindBox);
			this.GroupBoxHouseBill.Controls.Add(this.CA_RL_NK_PortOfDestinationBoundCodeFindBox);
			this.GroupBoxHouseBill.Controls.Add(this.CA_RL_NK_PortOfOriginBoundCodeFindBox);
			this.GroupBoxHouseBill.Controls.Add(this.CA_PrepaidCollectOtherBoundDropEdit);
			this.GroupBoxHouseBill.Controls.Add(this.zLabel6);
			this.GroupBoxHouseBill.Controls.Add(this.zLabel5);
			this.GroupBoxHouseBill.Controls.Add(this.zLabel4);
			this.GroupBoxHouseBill.Controls.Add(this.zLabel3);
			this.GroupBoxHouseBill.Controls.Add(this.zLabel1);
			this.GroupBoxHouseBill.Controls.Add(this.CA_HouseBillBoundTextBox);
			this.GroupBoxHouseBill.Controls.Add(this.CA_ShipmentStatusBoundTextBox);
			this.GroupBoxHouseBill.Dock = System.Windows.Forms.DockStyle.Fill;
			this.GroupBoxHouseBill.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.GroupBoxHouseBill.Name = "GroupBoxHouseBill";
			this.GroupBoxHouseBill.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(560, 236, true);
			this.GroupBoxHouseBill.TabIndex = 0;
			this.GroupBoxHouseBill.TabStop = false;
			this.GroupBoxHouseBill.Text = "House Bill";
			// 
			// zLabel41
			// 
			this.zLabel41.AutoSize = true;
			this.zLabel41.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabel41.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(270, 146, true);
			this.zLabel41.Name = "zLabel41";
			this.zLabel41.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(109, 13, true);
			this.zLabel41.TabIndex = 19;
			this.zLabel41.Text = "Responsible Party ID:";
			this.zLabel41.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.zLabel41.UseMnemonic = false;
			// 
			// responsiblePartyTextBox
			// 
			this.BindingSource.SetBindingMember(this.responsiblePartyTextBox, "CA_ResponsiblePartyID");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).CA_ResponsiblePartyID)));
			this.responsiblePartyTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(389, 143, true);
			this.responsiblePartyTextBox.Name = "responsiblePartyTextBox";
			this.responsiblePartyTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			this.responsiblePartyTextBox.TabIndex = 20;
			// 
			// zLabel39
			// 
			this.zLabel39.AutoSize = true;
			this.zLabel39.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabel39.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 43, true);
			this.zLabel39.Name = "zLabel39";
			this.zLabel39.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(86, 13, true);
			this.zLabel39.TabIndex = 3;
			this.zLabel39.Text = "Message Status:";
			this.zLabel39.UseMnemonic = false;
			// 
			// zLabel2
			// 
			this.zLabel2.AutoSize = true;
			this.zLabel2.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabel2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 17, true);
			this.zLabel2.Name = "zLabel2";
			this.zLabel2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(83, 13, true);
			this.zLabel2.TabIndex = 0;
			this.zLabel2.Text = "Customs Status:";
			this.zLabel2.UseMnemonic = false;
			// 
			// zTextBox3
			// 
			this.zTextBox3.BackColor = System.Drawing.SystemColors.Control;
			this.BindingSource.SetBindingMember(this.zTextBox3, "CA_ShipmentStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).CA_ShipmentStatus)));
			this.zTextBox3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(98, 13, true);
			this.zTextBox3.Multiline = true;
			this.zTextBox3.Name = "zTextBox3";
			this.zTextBox3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(48, 20, true);
			this.zTextBox3.TabIndex = 1;
			// 
			// zTextBox2
			// 
			this.zTextBox2.BackColor = System.Drawing.SystemColors.Control;
			this.BindingSource.SetBindingMember(this.zTextBox2, "CA_MessageStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).CA_MessageStatus)));
			this.zTextBox2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(98, 39, true);
			this.zTextBox2.Multiline = true;
			this.zTextBox2.Name = "zTextBox2";
			this.zTextBox2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(48, 20, true);
			this.zTextBox2.TabIndex = 4;
			// 
			// zTextBox1
			// 
			this.zTextBox1.BackColor = System.Drawing.SystemColors.Control;
			this.BindingSource.SetBindingMember(this.zTextBox1, "MessageStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).MessageStatus)));
			this.zTextBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(152, 39, true);
			this.zTextBox1.Multiline = true;
			this.zTextBox1.Name = "zTextBox1";
			this.zTextBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(389, 20, true);
			this.zTextBox1.TabIndex = 5;
			// 
			// FreightForwarderIndicatorCheckBox
			// 
			this.BindingSource.SetBindingMember(this.FreightForwarderIndicatorCheckBox, "CA_IsMasterHouse");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).CA_IsMasterHouse)));
			this.FreightForwarderIndicatorCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.FreightForwarderIndicatorCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(269, 90, true);
			this.FreightForwarderIndicatorCheckBox.Name = "FreightForwarderIndicatorCheckBox";
			this.FreightForwarderIndicatorCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(134, 25, true);
			this.FreightForwarderIndicatorCheckBox.TabIndex = 16;
			this.FreightForwarderIndicatorCheckBox.Text = "Consolidation (FF Ind)";
			// 
			// zLabel32
			// 
			this.zLabel32.AutoSize = true;
			this.zLabel32.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabel32.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(270, 173, true);
			this.zLabel32.Name = "zLabel32";
			this.zLabel32.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(91, 13, true);
			this.zLabel32.TabIndex = 21;
			this.zLabel32.Text = "Messaging Mode:";
			this.zLabel32.UseMnemonic = false;
			// 
			// CB_ApplicationCodeBoundDropEdit
			// 
			this.CB_ApplicationCodeBoundDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CB_ApplicationCodeBoundDropEdit, "OceanBill+CB_ApplicationCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).OceanBill.CB_ApplicationCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).OceanBill.Lookups.ApplicationCodeList)));
			this.CB_ApplicationCodeBoundDropEdit.BindToList = "OceanBill+Lookups+ApplicationCodeList";
			this.CB_ApplicationCodeBoundDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(389, 170, true);
			this.CB_ApplicationCodeBoundDropEdit.Name = "CB_ApplicationCodeBoundDropEdit";
			this.CB_ApplicationCodeBoundDropEdit.PreBoundMaxLength = 3;
			this.CB_ApplicationCodeBoundDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(152, 20, true);
			this.CB_ApplicationCodeBoundDropEdit.TabIndex = 22;
			// 
			// ParentBillLabel
			// 
			this.ParentBillLabel.AutoSize = true;
			this.ParentBillLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.ParentBillLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(270, 68, true);
			this.ParentBillLabel.Name = "ParentBillLabel";
			this.ParentBillLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(57, 13, true);
			this.ParentBillLabel.TabIndex = 14;
			this.ParentBillLabel.Text = "Parent Bill:";
			this.ParentBillLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.ParentBillLabel.UseMnemonic = false;
			// 
			// ParentBillTextBox
			// 
			this.BindingSource.SetBindingMember(this.ParentBillTextBox, "CA_MasterHouseBill");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).CA_MasterHouseBill)));
			this.ParentBillTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(389, 65, true);
			this.ParentBillTextBox.Name = "ParentBillTextBox";
			this.ParentBillTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(152, 20, true);
			this.ParentBillTextBox.TabIndex = 15;
			// 
			// CA_RN_NKGoodsOriginBoundCodeFindBox
			// 
			this.CA_RN_NKGoodsOriginBoundCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CA_RN_NKGoodsOriginBoundCodeFindBox, "CA_RN_NKGoodsOrigin");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).CA_RN_NKGoodsOrigin)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).CountryOfOriginList)));
			this.CA_RN_NKGoodsOriginBoundCodeFindBox.BindToList = "CountryOfOriginList";
			this.CA_RN_NKGoodsOriginBoundCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(98, 143, true);
			this.CA_RN_NKGoodsOriginBoundCodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefCountry;
			this.CA_RN_NKGoodsOriginBoundCodeFindBox.Name = "CA_RN_NKGoodsOriginBoundCodeFindBox";
			this.CA_RN_NKGoodsOriginBoundCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.CA_RN_NKGoodsOriginBoundCodeFindBox.ParentType = null;
			this.CA_RN_NKGoodsOriginBoundCodeFindBox.ShowDescriptionBox = false;
			this.CA_RN_NKGoodsOriginBoundCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			this.CA_RN_NKGoodsOriginBoundCodeFindBox.TabIndex = 13;
			// 
			// CA_RL_NK_PortOfDestinationBoundCodeFindBox
			// 
			this.CA_RL_NK_PortOfDestinationBoundCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CA_RL_NK_PortOfDestinationBoundCodeFindBox, "CA_RL_NK_PortOfDestination");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).CA_RL_NK_PortOfDestination)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).PortOfDestinationList)));
			this.CA_RL_NK_PortOfDestinationBoundCodeFindBox.BindToList = "PortOfDestinationList";
			this.CA_RL_NK_PortOfDestinationBoundCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(98, 117, true);
			this.CA_RL_NK_PortOfDestinationBoundCodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefUNLOCO;
			this.CA_RL_NK_PortOfDestinationBoundCodeFindBox.Name = "CA_RL_NK_PortOfDestinationBoundCodeFindBox";
			this.CA_RL_NK_PortOfDestinationBoundCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.CA_RL_NK_PortOfDestinationBoundCodeFindBox.ParentType = null;
			this.CA_RL_NK_PortOfDestinationBoundCodeFindBox.ShowDescriptionBox = false;
			this.CA_RL_NK_PortOfDestinationBoundCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			this.CA_RL_NK_PortOfDestinationBoundCodeFindBox.TabIndex = 11;
			// 
			// CA_RL_NK_PortOfOriginBoundCodeFindBox
			// 
			this.CA_RL_NK_PortOfOriginBoundCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CA_RL_NK_PortOfOriginBoundCodeFindBox, "CA_RL_NK_PortOfOrigin");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).CA_RL_NK_PortOfOrigin)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).PortOfOriginList)));
			this.CA_RL_NK_PortOfOriginBoundCodeFindBox.BindToList = "PortOfOriginList";
			this.CA_RL_NK_PortOfOriginBoundCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(98, 91, true);
			this.CA_RL_NK_PortOfOriginBoundCodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefUNLOCO;
			this.CA_RL_NK_PortOfOriginBoundCodeFindBox.Name = "CA_RL_NK_PortOfOriginBoundCodeFindBox";
			this.CA_RL_NK_PortOfOriginBoundCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.CA_RL_NK_PortOfOriginBoundCodeFindBox.ParentType = null;
			this.CA_RL_NK_PortOfOriginBoundCodeFindBox.ShowDescriptionBox = false;
			this.CA_RL_NK_PortOfOriginBoundCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			this.CA_RL_NK_PortOfOriginBoundCodeFindBox.TabIndex = 9;
			// 
			// CA_PrepaidCollectOtherBoundDropEdit
			// 
			this.CA_PrepaidCollectOtherBoundDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CA_PrepaidCollectOtherBoundDropEdit, "CA_PrepaidCollectOther");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).CA_PrepaidCollectOther)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).Lookups.MethodsOfPayment)));
			this.CA_PrepaidCollectOtherBoundDropEdit.BindToList = "Lookups+MethodsOfPayment";
			this.CA_PrepaidCollectOtherBoundDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(389, 117, true);
			this.CA_PrepaidCollectOtherBoundDropEdit.Name = "CA_PrepaidCollectOtherBoundDropEdit";
			this.CA_PrepaidCollectOtherBoundDropEdit.PreBoundMaxLength = 3;
			this.CA_PrepaidCollectOtherBoundDropEdit.ShowDescriptionBox = false;
			this.CA_PrepaidCollectOtherBoundDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
			this.CA_PrepaidCollectOtherBoundDropEdit.TabIndex = 18;
			// 
			// zLabel6
			// 
			this.zLabel6.AutoSize = true;
			this.zLabel6.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabel6.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(270, 120, true);
			this.zLabel6.Name = "zLabel6";
			this.zLabel6.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(78, 13, true);
			this.zLabel6.TabIndex = 17;
			this.zLabel6.Text = "Payment Type:";
			this.zLabel6.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.zLabel6.UseMnemonic = false;
			// 
			// zLabel5
			// 
			this.zLabel5.AutoSize = true;
			this.zLabel5.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabel5.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 147, true);
			this.zLabel5.Name = "zLabel5";
			this.zLabel5.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(71, 13, true);
			this.zLabel5.TabIndex = 12;
			this.zLabel5.Text = "Goods Origin:";
			this.zLabel5.UseMnemonic = false;
			// 
			// zLabel4
			// 
			this.zLabel4.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabel4.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 119, true);
			this.zLabel4.Name = "zLabel4";
			this.zLabel4.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(68, 17, true);
			this.zLabel4.TabIndex = 10;
			this.zLabel4.Text = "Destination:";
			this.zLabel4.UseMnemonic = false;
			// 
			// zLabel3
			// 
			this.zLabel3.AutoSize = true;
			this.zLabel3.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabel3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 95, true);
			this.zLabel3.Name = "zLabel3";
			this.zLabel3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(37, 13, true);
			this.zLabel3.TabIndex = 8;
			this.zLabel3.Text = "Origin:";
			this.zLabel3.UseMnemonic = false;
			// 
			// zLabel1
			// 
			this.zLabel1.AutoSize = true;
			this.zLabel1.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 69, true);
			this.zLabel1.Name = "zLabel1";
			this.zLabel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(57, 13, true);
			this.zLabel1.TabIndex = 6;
			this.zLabel1.Text = "House Bill:";
			this.zLabel1.UseMnemonic = false;
			// 
			// CA_HouseBillBoundTextBox
			// 
			this.BindingSource.SetBindingMember(this.CA_HouseBillBoundTextBox, "CA_HouseBill");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).CA_HouseBill)));
			this.CA_HouseBillBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(98, 65, true);
			this.CA_HouseBillBoundTextBox.Name = "CA_HouseBillBoundTextBox";
			this.CA_HouseBillBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(152, 20, true);
			this.CA_HouseBillBoundTextBox.TabIndex = 7;
			// 
			// CA_ShipmentStatusBoundTextBox
			// 
			this.CA_ShipmentStatusBoundTextBox.BackColor = System.Drawing.SystemColors.Control;
			this.BindingSource.SetBindingMember(this.CA_ShipmentStatusBoundTextBox, "ShipmentStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).ShipmentStatus)));
			this.CA_ShipmentStatusBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(152, 13, true);
			this.CA_ShipmentStatusBoundTextBox.Multiline = true;
			this.CA_ShipmentStatusBoundTextBox.Name = "CA_ShipmentStatusBoundTextBox";
			this.CA_ShipmentStatusBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(389, 20, true);
			this.CA_ShipmentStatusBoundTextBox.TabIndex = 2;
			// 
			// HouseBillPartiesUserControl
			// 
			this.HouseBillPartiesUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.HouseBillPartiesUserControl, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)))));
			this.HouseBillPartiesUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(569, 0, true);
			this.HouseBillPartiesUserControl.Name = "HouseBillPartiesUserControl";
			this.HouseBillPartiesUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(376, 263, true);
			this.HouseBillPartiesUserControl.TabIndex = 1;
			// 
			// BillDetailsTabControl
			// 
			this.BillDetailsTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.BillDetailsTabControl.Controls.Add(this.HouseBillDetailsTabPage);
			this.BillDetailsTabControl.Controls.Add(this.OceanBillDetailsTabPage);
			this.BillDetailsTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.BillDetailsTabControl.Name = "BillDetailsTabControl";
			this.BillDetailsTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(568, 263, true);
			this.BillDetailsTabControl.TabIndex = 0;
			// 
			// HouseBillDetailsTabPage
			// 
			this.HouseBillDetailsTabPage.Controls.Add(this.GroupBoxHouseBill);
			this.HouseBillDetailsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.HouseBillDetailsTabPage.Name = "HouseBillDetailsTabPage";
			this.HouseBillDetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(560, 236, true);
			this.HouseBillDetailsTabPage.TabIndex = 0;
			this.HouseBillDetailsTabPage.Text = "House Bill";
			// 
			// OceanBillDetailsTabPage
			// 
			this.OceanBillDetailsTabPage.Controls.Add(this.zLabel38);
			this.OceanBillDetailsTabPage.Controls.Add(this.PrincipalIDBoundTextBox);
			this.OceanBillDetailsTabPage.Controls.Add(this.zLabel37);
			this.OceanBillDetailsTabPage.Controls.Add(this.ResponsiblePartyIDBoundTextBox);
			this.OceanBillDetailsTabPage.Controls.Add(this.ShippingLineGuidBoundFindBox);
			this.OceanBillDetailsTabPage.Controls.Add(this.ShippingLineLabel);
			this.OceanBillDetailsTabPage.Controls.Add(this.CB_RV_NKVesselBoundFindBox);
			this.OceanBillDetailsTabPage.Controls.Add(this.CB_RL_NKPortOfDischargeBoundCodeFindBox);
			this.OceanBillDetailsTabPage.Controls.Add(this.CB_RL_NKPortOfLoadingBoundCodeFindBox);
			this.OceanBillDetailsTabPage.Controls.Add(this.PrincipalIDLabel);
			this.OceanBillDetailsTabPage.Controls.Add(this.CB_PrincipalIDBoundTextBox);
			this.OceanBillDetailsTabPage.Controls.Add(this.zLabel31);
			this.OceanBillDetailsTabPage.Controls.Add(this.zLabel22);
			this.OceanBillDetailsTabPage.Controls.Add(this.zLabel23);
			this.OceanBillDetailsTabPage.Controls.Add(this.CB_VoyageBoundTextBox);
			this.OceanBillDetailsTabPage.Controls.Add(this.zLabel24);
			this.OceanBillDetailsTabPage.Controls.Add(this.zLabel25);
			this.OceanBillDetailsTabPage.Controls.Add(this.CB_OceanBillBoundTextBox);
			this.OceanBillDetailsTabPage.Controls.Add(this.overrideFreightDefaultsCheckBox);
			this.OceanBillDetailsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.OceanBillDetailsTabPage.Name = "OceanBillDetailsTabPage";
			this.OceanBillDetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(560, 236, true);
			this.OceanBillDetailsTabPage.TabIndex = 1;
			this.OceanBillDetailsTabPage.Text = "Ocean Bill";
			// 
			// zLabel38
			// 
			this.zLabel38.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabel38.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(376, 32, true);
			this.zLabel38.Name = "zLabel38";
			this.zLabel38.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 23, true);
			this.zLabel38.TabIndex = 14;
			this.zLabel38.Text = "Lloyds:";
			this.zLabel38.UseMnemonic = false;
			// 
			// PrincipalIDBoundTextBox
			// 
			this.BindingSource.SetBindingMember(this.PrincipalIDBoundTextBox, "OceanBill+CB_LloydsIMO");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).OceanBill.CB_LloydsIMO)));
			this.PrincipalIDBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(432, 32, true);
			this.PrincipalIDBoundTextBox.Name = "PrincipalIDBoundTextBox";
			this.PrincipalIDBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			this.PrincipalIDBoundTextBox.TabIndex = 15;
			// 
			// zLabel37
			// 
			this.zLabel37.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabel37.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(304, 56, true);
			this.zLabel37.Name = "zLabel37";
			this.zLabel37.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(112, 23, true);
			this.zLabel37.TabIndex = 16;
			this.zLabel37.Text = "Responsible Party ID:";
			this.zLabel37.UseMnemonic = false;
			// 
			// ResponsiblePartyIDBoundTextBox
			// 
			this.BindingSource.SetBindingMember(this.ResponsiblePartyIDBoundTextBox, "OceanBill+CB_ResponsiblePartyID");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).OceanBill.CB_ResponsiblePartyID)));
			this.ResponsiblePartyIDBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(432, 56, true);
			this.ResponsiblePartyIDBoundTextBox.Name = "ResponsiblePartyIDBoundTextBox";
			this.ResponsiblePartyIDBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			this.ResponsiblePartyIDBoundTextBox.TabIndex = 17;
			// 
			// ShippingLineGuidBoundFindBox
			// 
			this.ShippingLineGuidBoundFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ShippingLineGuidBoundFindBox, "OceanBill+CB_OH_ShippingLine");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).OceanBill.CB_OH_ShippingLine)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).OceanBill.Lookups.ShippingLines)));
			this.ShippingLineGuidBoundFindBox.BindToList = "OceanBill+Lookups+ShippingLines";
			this.ShippingLineGuidBoundFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 128, true);
			this.ShippingLineGuidBoundFindBox.Name = "ShippingLineGuidBoundFindBox";
			this.ShippingLineGuidBoundFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.ShippingLineGuidBoundFindBox.ParentType = null;
			this.ShippingLineGuidBoundFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.ShippingLineGuidBoundFindBox.TabIndex = 11;
			// 
			// ShippingLineLabel
			// 
			this.ShippingLineLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.ShippingLineLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 128, true);
			this.ShippingLineLabel.Name = "ShippingLineLabel";
			this.ShippingLineLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(77, 23, true);
			this.ShippingLineLabel.TabIndex = 10;
			this.ShippingLineLabel.Text = "Shipping Line:";
			this.ShippingLineLabel.UseMnemonic = false;
			// 
			// CB_RV_NKVesselBoundFindBox
			// 
			this.CB_RV_NKVesselBoundFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CB_RV_NKVesselBoundFindBox, "OceanBill.CB_VesselName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).OceanBill.CB_VesselName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).OceanBill.Lookups.VesselNames)));
			this.CB_RV_NKVesselBoundFindBox.BindToList = "OceanBill+Lookups+VesselNames";
			this.CB_RV_NKVesselBoundFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 32, true);
			this.CB_RV_NKVesselBoundFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefVessel;
			this.CB_RV_NKVesselBoundFindBox.Name = "CB_RV_NKVesselBoundFindBox";
			this.CB_RV_NKVesselBoundFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.CB_RV_NKVesselBoundFindBox.ParentType = null;
			this.CB_RV_NKVesselBoundFindBox.PreBoundMaxLength = 35;
			this.CB_RV_NKVesselBoundFindBox.ShowDescriptionBox = false;
			this.CB_RV_NKVesselBoundFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(234, 20, true);
			this.CB_RV_NKVesselBoundFindBox.TabIndex = 3;
			// 
			// CB_RL_NKPortOfDischargeBoundCodeFindBox
			// 
			this.CB_RL_NKPortOfDischargeBoundCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CB_RL_NKPortOfDischargeBoundCodeFindBox, "OceanBill.CB_RL_NKPortOfDischarge");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).OceanBill.CB_RL_NKPortOfDischarge)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).PortOfDestinationList)));
			this.CB_RL_NKPortOfDischargeBoundCodeFindBox.BindToList = "PortOfDestinationList";
			this.CB_RL_NKPortOfDischargeBoundCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 80, true);
			this.CB_RL_NKPortOfDischargeBoundCodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefUNLOCO;
			this.CB_RL_NKPortOfDischargeBoundCodeFindBox.Name = "CB_RL_NKPortOfDischargeBoundCodeFindBox";
			this.CB_RL_NKPortOfDischargeBoundCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.CB_RL_NKPortOfDischargeBoundCodeFindBox.ParentType = null;
			this.CB_RL_NKPortOfDischargeBoundCodeFindBox.ShowDescriptionBox = false;
			this.CB_RL_NKPortOfDischargeBoundCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			this.CB_RL_NKPortOfDischargeBoundCodeFindBox.TabIndex = 7;
			// 
			// CB_RL_NKPortOfLoadingBoundCodeFindBox
			// 
			this.CB_RL_NKPortOfLoadingBoundCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CB_RL_NKPortOfLoadingBoundCodeFindBox, "OceanBill.CB_RL_NKPortOfLoading");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).OceanBill.CB_RL_NKPortOfLoading)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).OceanBill.PortOfLoadingList)));
			this.CB_RL_NKPortOfLoadingBoundCodeFindBox.BindToList = "OceanBill.PortOfLoadingList";
			this.CB_RL_NKPortOfLoadingBoundCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 56, true);
			this.CB_RL_NKPortOfLoadingBoundCodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefUNLOCO;
			this.CB_RL_NKPortOfLoadingBoundCodeFindBox.Name = "CB_RL_NKPortOfLoadingBoundCodeFindBox";
			this.CB_RL_NKPortOfLoadingBoundCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.CB_RL_NKPortOfLoadingBoundCodeFindBox.ParentType = null;
			this.CB_RL_NKPortOfLoadingBoundCodeFindBox.ShowDescriptionBox = false;
			this.CB_RL_NKPortOfLoadingBoundCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			this.CB_RL_NKPortOfLoadingBoundCodeFindBox.TabIndex = 5;
			// 
			// PrincipalIDLabel
			// 
			this.PrincipalIDLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.PrincipalIDLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 152, true);
			this.PrincipalIDLabel.Name = "PrincipalIDLabel";
			this.PrincipalIDLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 23, true);
			this.PrincipalIDLabel.TabIndex = 12;
			this.PrincipalIDLabel.Text = "Principal ID:";
			this.PrincipalIDLabel.UseMnemonic = false;
			// 
			// CB_PrincipalIDBoundTextBox
			// 
			this.BindingSource.SetBindingMember(this.CB_PrincipalIDBoundTextBox, "OceanBill.CB_PrincipalID");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).OceanBill.CB_PrincipalID)));
			this.CB_PrincipalIDBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 152, true);
			this.CB_PrincipalIDBoundTextBox.Name = "CB_PrincipalIDBoundTextBox";
			this.CB_PrincipalIDBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			this.CB_PrincipalIDBoundTextBox.TabIndex = 13;
			// 
			// zLabel31
			// 
			this.zLabel31.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabel31.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 80, true);
			this.zLabel31.Name = "zLabel31";
			this.zLabel31.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 23, true);
			this.zLabel31.TabIndex = 6;
			this.zLabel31.Text = "Discharge Port:";
			this.zLabel31.UseMnemonic = false;
			// 
			// zLabel22
			// 
			this.zLabel22.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabel22.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 56, true);
			this.zLabel22.Name = "zLabel22";
			this.zLabel22.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 23, true);
			this.zLabel22.TabIndex = 4;
			this.zLabel22.Text = "Loading Port:";
			this.zLabel22.UseMnemonic = false;
			// 
			// zLabel23
			// 
			this.zLabel23.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabel23.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 104, true);
			this.zLabel23.Name = "zLabel23";
			this.zLabel23.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(48, 23, true);
			this.zLabel23.TabIndex = 8;
			this.zLabel23.Text = "Voyage:";
			this.zLabel23.UseMnemonic = false;
			// 
			// CB_VoyageBoundTextBox
			// 
			this.BindingSource.SetBindingMember(this.CB_VoyageBoundTextBox, "OceanBill.CB_Voyage");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).OceanBill.CB_Voyage)));
			this.CB_VoyageBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 104, true);
			this.CB_VoyageBoundTextBox.Name = "CB_VoyageBoundTextBox";
			this.CB_VoyageBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			this.CB_VoyageBoundTextBox.TabIndex = 9;
			// 
			// zLabel24
			// 
			this.zLabel24.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabel24.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 32, true);
			this.zLabel24.Name = "zLabel24";
			this.zLabel24.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 23, true);
			this.zLabel24.TabIndex = 2;
			this.zLabel24.Text = "Vessel:";
			this.zLabel24.UseMnemonic = false;
			// 
			// zLabel25
			// 
			this.zLabel25.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabel25.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 8, true);
			this.zLabel25.Name = "zLabel25";
			this.zLabel25.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 23, true);
			this.zLabel25.TabIndex = 0;
			this.zLabel25.Text = "Ocean Bill:";
			this.zLabel25.UseMnemonic = false;
			// 
			// CB_OceanBillBoundTextBox
			// 
			this.BindingSource.SetBindingMember(this.CB_OceanBillBoundTextBox, "OceanBill.CB_OceanBill");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).OceanBill.CB_OceanBill)));
			this.CB_OceanBillBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 8, true);
			this.CB_OceanBillBoundTextBox.Name = "CB_OceanBillBoundTextBox";
			this.CB_OceanBillBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(280, 20, true);
			this.CB_OceanBillBoundTextBox.TabIndex = 1;
			// 
			// overrideFreightDefaultsCheckBox
			// 
			this.BindingSource.SetBindingMember(this.overrideFreightDefaultsCheckBox, "OceanBill.OverrideFreightDefaults");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).OceanBill.OverrideFreightDefaults)));
			this.overrideFreightDefaultsCheckBox.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("de72ad2d-236a-4a19-92bc-62f85cc7ec98", "Override Default Values from Consol", "Override Default Values from Consol is settable on the Consol Sea Cargo");
			this.overrideFreightDefaultsCheckBox.ForeColor = System.Drawing.SystemColors.GrayText;
			this.overrideFreightDefaultsCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 178, true);
			this.overrideFreightDefaultsCheckBox.Name = "overrideFreightDefaultsCheckBox";
			this.overrideFreightDefaultsCheckBox.ReadOnly = true;
			this.overrideFreightDefaultsCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.overrideFreightDefaultsCheckBox.TabIndex = 18;
			this.overrideFreightDefaultsCheckBox.Text = "Override Default Values from Consol";
			this.overrideFreightDefaultsCheckBox.Visible = false;
			// 
			// SeaCargoHouseDetailsUserControl
			// 
			this.Controls.Add(this.BillDetailsTabControl);
			this.Controls.Add(this.HouseBillPartiesUserControl);
			this.Name = "SeaCargoHouseDetailsUserControl";
			this.ShouldSerializeTabPageMethods = false;
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(947, 265, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.GroupBoxHouseBill.ResumeLayout(false);
			this.GroupBoxHouseBill.PerformLayout();
			this.CB_ApplicationCodeBoundDropEdit.ResumeLayout(true);
			this.CB_ApplicationCodeBoundDropEdit.PerformLayout();
			this.CA_RN_NKGoodsOriginBoundCodeFindBox.ResumeLayout(true);
			this.CA_RN_NKGoodsOriginBoundCodeFindBox.PerformLayout();
			this.CA_RL_NK_PortOfDestinationBoundCodeFindBox.ResumeLayout(true);
			this.CA_RL_NK_PortOfDestinationBoundCodeFindBox.PerformLayout();
			this.CA_RL_NK_PortOfOriginBoundCodeFindBox.ResumeLayout(true);
			this.CA_RL_NK_PortOfOriginBoundCodeFindBox.PerformLayout();
			this.CA_PrepaidCollectOtherBoundDropEdit.ResumeLayout(true);
			this.CA_PrepaidCollectOtherBoundDropEdit.PerformLayout();
			this.HouseBillPartiesUserControl.ResumeLayout(true);
			this.HouseBillPartiesUserControl.PerformLayout();
			this.BillDetailsTabControl.ResumeLayout(false);
			this.BillDetailsTabControl.PerformLayout();
			this.HouseBillDetailsTabPage.ResumeLayout(false);
			this.HouseBillDetailsTabPage.PerformLayout();
			this.OceanBillDetailsTabPage.ResumeLayout(false);
			this.OceanBillDetailsTabPage.PerformLayout();
			this.ShippingLineGuidBoundFindBox.ResumeLayout(true);
			this.ShippingLineGuidBoundFindBox.PerformLayout();
			this.CB_RV_NKVesselBoundFindBox.ResumeLayout(true);
			this.CB_RV_NKVesselBoundFindBox.PerformLayout();
			this.CB_RL_NKPortOfDischargeBoundCodeFindBox.ResumeLayout(true);
			this.CB_RL_NKPortOfDischargeBoundCodeFindBox.PerformLayout();
			this.CB_RL_NKPortOfLoadingBoundCodeFindBox.ResumeLayout(true);
			this.CB_RL_NKPortOfLoadingBoundCodeFindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}
	}
}
