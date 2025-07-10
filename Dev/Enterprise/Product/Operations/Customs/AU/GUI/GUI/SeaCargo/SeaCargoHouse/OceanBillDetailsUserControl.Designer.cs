namespace Enterprise.Customs.AU.SeaCargo.GUI
{
	partial class OceanBillDetailsUserControl
	{
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				CB_RV_NKVessel.PopupSelected -= CB_RV_NKVessel_PopupSelected;
			}
			base.Dispose(disposing);
		}

		private void InitializeComponent()
		{
			this.GroupBoxOceanBill = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.DepartureDateLabell = new Enterprise.ZArchitecture.ZLabel();
			this.ArrivalDateLabel = new Enterprise.ZArchitecture.ZLabel();
			this.FirstArrivalDateLabel = new Enterprise.ZArchitecture.ZLabel();
			this.FirstArrivalDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.ArrivalDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.DepartureDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.ResponsiblePartyIDTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.PrincipalIDTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CB_ApplicationCodeBoundDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.zLabel3 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel2 = new Enterprise.ZArchitecture.ZLabel();
			this.LloydsDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.zLabel1 = new Enterprise.ZArchitecture.ZLabel();
			this.CusSCAOceanBillParentBillTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CusSCAOceanBillParentBillLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ShippingLineGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.ShippingLineLabel = new Enterprise.ZArchitecture.ZLabel();
			this.CB_RV_NKVessel = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.CB_RL_NKPortOfFirstArrivalCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.CB_RL_NKPortOfDischargeBoundCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.ArrivalLabel = new Enterprise.ZArchitecture.ZLabel();
			this.CB_RL_NKPortOfLoadingBoundCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.DischargeLabel = new Enterprise.ZArchitecture.ZLabel();
			this.LoadingPortLabel = new Enterprise.ZArchitecture.ZLabel();
			this.VoyageLabel = new Enterprise.ZArchitecture.ZLabel();
			this.CB_VoyageBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.VesselLabel = new Enterprise.ZArchitecture.ZLabel();
			this.OceanBillLabel = new Enterprise.ZArchitecture.ZLabel();
			this.CB_OceanBillBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.MessagingModeLabel = new Enterprise.ZArchitecture.ZLabel();
			this.branchLabel = new Enterprise.ZArchitecture.ZLabel();
			this.BranchGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.overrideFreightDefaultsCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.GroupBoxOceanBill.SuspendLayout();
			this.FirstArrivalDateEdit.SuspendLayout();
			this.ArrivalDateEdit.SuspendLayout();
			this.DepartureDateEdit.SuspendLayout();
			this.CB_ApplicationCodeBoundDropEdit.SuspendLayout();
			this.LloydsDropEdit.SuspendLayout();
			this.ShippingLineGuidFindBox.SuspendLayout();
			this.CB_RV_NKVessel.SuspendLayout();
			this.CB_RL_NKPortOfFirstArrivalCodeFindBox.SuspendLayout();
			this.CB_RL_NKPortOfDischargeBoundCodeFindBox.SuspendLayout();
			this.CB_RL_NKPortOfLoadingBoundCodeFindBox.SuspendLayout();
			this.BranchGuidFindBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.AU.Declaration.Business.CusSCAOceanBill);
			// 
			// GroupBoxOceanBill
			// 
			this.GroupBoxOceanBill.Controls.Add(this.DepartureDateLabell);
			this.GroupBoxOceanBill.Controls.Add(this.ArrivalDateLabel);
			this.GroupBoxOceanBill.Controls.Add(this.FirstArrivalDateLabel);
			this.GroupBoxOceanBill.Controls.Add(this.FirstArrivalDateEdit);
			this.GroupBoxOceanBill.Controls.Add(this.ArrivalDateEdit);
			this.GroupBoxOceanBill.Controls.Add(this.DepartureDateEdit);
			this.GroupBoxOceanBill.Controls.Add(this.ResponsiblePartyIDTextBox);
			this.GroupBoxOceanBill.Controls.Add(this.PrincipalIDTextBox);
			this.GroupBoxOceanBill.Controls.Add(this.CB_ApplicationCodeBoundDropEdit);
			this.GroupBoxOceanBill.Controls.Add(this.zLabel3);
			this.GroupBoxOceanBill.Controls.Add(this.zLabel2);
			this.GroupBoxOceanBill.Controls.Add(this.LloydsDropEdit);
			this.GroupBoxOceanBill.Controls.Add(this.zLabel1);
			this.GroupBoxOceanBill.Controls.Add(this.CusSCAOceanBillParentBillTextBox);
			this.GroupBoxOceanBill.Controls.Add(this.CusSCAOceanBillParentBillLabel);
			this.GroupBoxOceanBill.Controls.Add(this.ShippingLineGuidFindBox);
			this.GroupBoxOceanBill.Controls.Add(this.ShippingLineLabel);
			this.GroupBoxOceanBill.Controls.Add(this.CB_RV_NKVessel);
			this.GroupBoxOceanBill.Controls.Add(this.CB_RL_NKPortOfFirstArrivalCodeFindBox);
			this.GroupBoxOceanBill.Controls.Add(this.CB_RL_NKPortOfDischargeBoundCodeFindBox);
			this.GroupBoxOceanBill.Controls.Add(this.ArrivalLabel);
			this.GroupBoxOceanBill.Controls.Add(this.CB_RL_NKPortOfLoadingBoundCodeFindBox);
			this.GroupBoxOceanBill.Controls.Add(this.DischargeLabel);
			this.GroupBoxOceanBill.Controls.Add(this.LoadingPortLabel);
			this.GroupBoxOceanBill.Controls.Add(this.VoyageLabel);
			this.GroupBoxOceanBill.Controls.Add(this.CB_VoyageBoundTextBox);
			this.GroupBoxOceanBill.Controls.Add(this.VesselLabel);
			this.GroupBoxOceanBill.Controls.Add(this.OceanBillLabel);
			this.GroupBoxOceanBill.Controls.Add(this.CB_OceanBillBoundTextBox);
			this.GroupBoxOceanBill.Controls.Add(this.MessagingModeLabel);
			this.GroupBoxOceanBill.Controls.Add(this.branchLabel);
			this.GroupBoxOceanBill.Controls.Add(this.BranchGuidFindBox);
			this.GroupBoxOceanBill.Controls.Add(this.overrideFreightDefaultsCheckBox);
			this.GroupBoxOceanBill.Dock = System.Windows.Forms.DockStyle.Fill;
			this.GroupBoxOceanBill.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.GroupBoxOceanBill.Name = "GroupBoxOceanBill";
			this.GroupBoxOceanBill.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(967, 107, true);
			this.GroupBoxOceanBill.TabIndex = 0;
			this.GroupBoxOceanBill.TabStop = false;
			this.GroupBoxOceanBill.Text = "Ocean Bill";
			// 
			// DepartureDateLabell
			// 
			this.DepartureDateLabell.AutoSize = true;
			this.DepartureDateLabell.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.DepartureDateLabell.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(783, 16, true);
			this.DepartureDateLabell.Name = "DepartureDateLabell";
			this.DepartureDateLabell.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(57, 13, true);
			this.DepartureDateLabell.TabIndex = 24;
			this.DepartureDateLabell.Text = "Departure:";
			// 
			// ArrivalDateLabel
			// 
			this.ArrivalDateLabel.AutoSize = true;
			this.ArrivalDateLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.ArrivalDateLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(802, 40, true);
			this.ArrivalDateLabel.Name = "ArrivalDateLabel";
			this.ArrivalDateLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(39, 13, true);
			this.ArrivalDateLabel.TabIndex = 26;
			this.ArrivalDateLabel.Text = "Arrival:";
			// 
			// FirstArrivalDateLabel
			// 
			this.FirstArrivalDateLabel.AutoSize = true;
			this.FirstArrivalDateLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.FirstArrivalDateLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(778, 64, true);
			this.FirstArrivalDateLabel.Name = "FirstArrivalDateLabel";
			this.FirstArrivalDateLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(61, 13, true);
			this.FirstArrivalDateLabel.TabIndex = 28;
			this.FirstArrivalDateLabel.Text = "First Arrival:";
			// 
			// FirstArrivalDateEdit
			// 
			this.FirstArrivalDateEdit.AllowDrop = true;
			this.FirstArrivalDateEdit.AutoCompleteMonthThreshold = 1;
			this.FirstArrivalDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.FirstArrivalDateEdit, "CB_DateOfFirstArrival");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAOceanBill)(null)).CB_DateOfFirstArrival)));
			this.FirstArrivalDateEdit.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("532C13ED-1916-4B87-9347-9CDC85F51277", "First Arrival:");
			this.FirstArrivalDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(847, 61, true);
			this.FirstArrivalDateEdit.Name = "FirstArrivalDateEdit";
			this.FirstArrivalDateEdit.TabIndex = 29;
			// 
			// ArrivalDateEdit
			// 
			this.ArrivalDateEdit.AllowDrop = true;
			this.ArrivalDateEdit.AutoCompleteMonthThreshold = 1;
			this.ArrivalDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.ArrivalDateEdit, "CB_DateOfArrival");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAOceanBill)(null)).CB_DateOfArrival)));
			this.ArrivalDateEdit.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("745A6A78-0C02-4FFB-B0DC-F38F60A75238", "Arrival:");
			this.ArrivalDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(847, 37, true);
			this.ArrivalDateEdit.Name = "ArrivalDateEdit";
			this.ArrivalDateEdit.TabIndex = 27;
			// 
			// DepartureDateEdit
			// 
			this.DepartureDateEdit.AllowDrop = true;
			this.DepartureDateEdit.AutoCompleteMonthThreshold = 1;
			this.DepartureDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.DepartureDateEdit, "CB_DateOfDeparture");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAOceanBill)(null)).CB_DateOfDeparture)));
			this.DepartureDateEdit.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("D4A814AB-5CB0-4093-AAC2-13D53B409E25", "Departure:");
			this.DepartureDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(847, 13, true);
			this.DepartureDateEdit.Name = "DepartureDateEdit";
			this.DepartureDateEdit.TabIndex = 25;
			// 
			// ResponsiblePartyIDTextBox
			// 
			this.BindingSource.SetBindingMember(this.ResponsiblePartyIDTextBox, "CB_ResponsiblePartyID");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusSCAOceanBill)(null)).CB_ResponsiblePartyID)));
			this.ResponsiblePartyIDTextBox.CaptionResourceString = null;
			this.ResponsiblePartyIDTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(392, 61, true);
			this.ResponsiblePartyIDTextBox.Name = "ResponsiblePartyIDTextBox";
			this.ResponsiblePartyIDTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(106, 20, true);
			this.ResponsiblePartyIDTextBox.TabIndex = 13;
			// 
			// PrincipalIDTextBox
			// 
			this.BindingSource.SetBindingMember(this.PrincipalIDTextBox, "CB_PrincipalID");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusSCAOceanBill)(null)).CB_PrincipalID)));
			this.PrincipalIDTextBox.CaptionResourceString = null;
			this.PrincipalIDTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(392, 37, true);
			this.PrincipalIDTextBox.Name = "PrincipalIDTextBox";
			this.PrincipalIDTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(106, 20, true);
			this.PrincipalIDTextBox.TabIndex = 11;
			// 
			// CB_ApplicationCodeBoundDropEdit
			// 
			this.CB_ApplicationCodeBoundDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CB_ApplicationCodeBoundDropEdit, "CB_ApplicationCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAOceanBill)(null)).CB_ApplicationCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusSCAOceanBill)(null)).Lookups.ApplicationCodeList)));
			this.CB_ApplicationCodeBoundDropEdit.BindToList = "Lookups+ApplicationCodeList";
			this.CB_ApplicationCodeBoundDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(847, 83, true);
			this.CB_ApplicationCodeBoundDropEdit.Name = "CB_ApplicationCodeBoundDropEdit";
			this.CB_ApplicationCodeBoundDropEdit.PreBoundMaxLength = 3;
			this.CB_ApplicationCodeBoundDropEdit.ShouldResizeByMaxLength = true;
			this.CB_ApplicationCodeBoundDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(116, 20, true);
			this.CB_ApplicationCodeBoundDropEdit.TabIndex = 31;
			// 
			// zLabel3
			// 
			this.zLabel3.AutoSize = true;
			this.zLabel3.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabel3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(289, 64, true);
			this.zLabel3.Name = "zLabel3";
			this.zLabel3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(95, 13, true);
			this.zLabel3.TabIndex = 12;
			this.zLabel3.Text = "Responsible Party:";
			// 
			// zLabel2
			// 
			this.zLabel2.AutoSize = true;
			this.zLabel2.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabel2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(334, 40, true);
			this.zLabel2.Name = "zLabel2";
			this.zLabel2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 13, true);
			this.zLabel2.TabIndex = 10;
			this.zLabel2.Text = "Principal:";
			// 
			// LloydsDropEdit
			// 
			this.LloydsDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.LloydsDropEdit, "CB_LloydsIMO");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAOceanBill)(null)).CB_LloydsIMO)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusSCAOceanBill)(null)).Lookups.LloydsIMOList)));
			this.LloydsDropEdit.BindToList = "Lookups+LloydsIMOList";
			this.LloydsDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(52, 37, true);
			this.LloydsDropEdit.Name = "LloydsDropEdit";
			this.LloydsDropEdit.ShouldResizeByMaxLength = true;
			this.LloydsDropEdit.ShowDescriptionBox = false;
			this.LloydsDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.LloydsDropEdit.TabIndex = 3;
			// 
			// zLabel1
			// 
			this.zLabel1.AutoSize = true;
			this.zLabel1.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 40, true);
			this.zLabel1.Name = "zLabel1";
			this.zLabel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 13, true);
			this.zLabel1.TabIndex = 2;
			this.zLabel1.Text = "Lloyds:";
			// 
			// CusSCAOceanBillParentBillTextBox
			// 
			this.BindingSource.SetBindingMember(this.CusSCAOceanBillParentBillTextBox, "CB_MasterHouseBill");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusSCAOceanBill)(null)).CB_MasterHouseBill)));
			this.CusSCAOceanBillParentBillTextBox.CaptionResourceString = null;
			this.CusSCAOceanBillParentBillTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(679, 84, true);
			this.CusSCAOceanBillParentBillTextBox.Name = "CusSCAOceanBillParentBillTextBox";
			this.CusSCAOceanBillParentBillTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			this.CusSCAOceanBillParentBillTextBox.TabIndex = 23;
			// 
			// CusSCAOceanBillParentBillLabel
			// 
			this.CusSCAOceanBillParentBillLabel.AutoSize = true;
			this.CusSCAOceanBillParentBillLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.CusSCAOceanBillParentBillLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(614, 86, true);
			this.CusSCAOceanBillParentBillLabel.Name = "CusSCAOceanBillParentBillLabel";
			this.CusSCAOceanBillParentBillLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(57, 13, true);
			this.CusSCAOceanBillParentBillLabel.TabIndex = 22;
			this.CusSCAOceanBillParentBillLabel.Text = "Parent Bill:";
			// 
			// ShippingLineGuidFindBox
			// 
			this.ShippingLineGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ShippingLineGuidFindBox, "CB_OH_ShippingLine");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.AU.Declaration.Business.CusSCAOceanBill)(null)).CB_OH_ShippingLine)));
			this.ShippingLineGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(392, 13, true);
			this.ShippingLineGuidFindBox.Name = "ShippingLineGuidFindBox";
			this.ShippingLineGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(194, 20, true);
			this.ShippingLineGuidFindBox.TabIndex = 9;
			// 
			// ShippingLineLabel
			// 
			this.ShippingLineLabel.AutoSize = true;
			this.ShippingLineLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.ShippingLineLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(310, 16, true);
			this.ShippingLineLabel.Name = "ShippingLineLabel";
			this.ShippingLineLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(74, 13, true);
			this.ShippingLineLabel.TabIndex = 8;
			this.ShippingLineLabel.Text = "Shipping Line:";
			// 
			// CB_RV_NKVessel
			// 
			this.CB_RV_NKVessel.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CB_RV_NKVessel, "CB_VesselName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusSCAOceanBill)(null)).CB_VesselName)));
			this.CB_RV_NKVessel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(52, 13, true);
			this.CB_RV_NKVessel.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefVessel;
			this.CB_RV_NKVessel.Name = "CB_RV_NKVessel";
			this.CB_RV_NKVessel.PreBoundMaxLength = 35;
			this.CB_RV_NKVessel.ShowDescriptionBox = false;
			this.CB_RV_NKVessel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(234, 20, true);
			this.CB_RV_NKVessel.TabIndex = 1;
			// 
			// CB_RL_NKPortOfFirstArrivalCodeFindBox
			// 
			this.CB_RL_NKPortOfFirstArrivalCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CB_RL_NKPortOfFirstArrivalCodeFindBox, "CB_RL_NKPortOfFirstArrival");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusSCAOceanBill)(null)).CB_RL_NKPortOfFirstArrival)));
			this.CB_RL_NKPortOfFirstArrivalCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(679, 61, true);
			this.CB_RL_NKPortOfFirstArrivalCodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefUNLOCO;
			this.CB_RL_NKPortOfFirstArrivalCodeFindBox.Name = "CB_RL_NKPortOfFirstArrivalCodeFindBox";
			this.CB_RL_NKPortOfFirstArrivalCodeFindBox.ShowDescriptionBox = false;
			this.CB_RL_NKPortOfFirstArrivalCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			this.CB_RL_NKPortOfFirstArrivalCodeFindBox.TabIndex = 21;
			// 
			// CB_RL_NKPortOfDischargeBoundCodeFindBox
			// 
			this.CB_RL_NKPortOfDischargeBoundCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CB_RL_NKPortOfDischargeBoundCodeFindBox, "CB_RL_NKPortOfDischarge");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusSCAOceanBill)(null)).CB_RL_NKPortOfDischarge)));
			this.CB_RL_NKPortOfDischargeBoundCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(679, 37, true);
			this.CB_RL_NKPortOfDischargeBoundCodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefUNLOCO;
			this.CB_RL_NKPortOfDischargeBoundCodeFindBox.Name = "CB_RL_NKPortOfDischargeBoundCodeFindBox";
			this.CB_RL_NKPortOfDischargeBoundCodeFindBox.ShowDescriptionBox = false;
			this.CB_RL_NKPortOfDischargeBoundCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			this.CB_RL_NKPortOfDischargeBoundCodeFindBox.TabIndex = 19;
			// 
			// ArrivalLabel
			// 
			this.ArrivalLabel.AutoSize = true;
			this.ArrivalLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.ArrivalLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(575, 64, true);
			this.ArrivalLabel.Name = "ArrivalLabel";
			this.ArrivalLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(95, 13, true);
			this.ArrivalLabel.TabIndex = 20;
			this.ArrivalLabel.Text = "Port of First Arrival:";
			// 
			// CB_RL_NKPortOfLoadingBoundCodeFindBox
			// 
			this.CB_RL_NKPortOfLoadingBoundCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CB_RL_NKPortOfLoadingBoundCodeFindBox, "CB_RL_NKPortOfLoading");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusSCAOceanBill)(null)).CB_RL_NKPortOfLoading)));
			this.CB_RL_NKPortOfLoadingBoundCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(679, 13, true);
			this.CB_RL_NKPortOfLoadingBoundCodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefUNLOCO;
			this.CB_RL_NKPortOfLoadingBoundCodeFindBox.Name = "CB_RL_NKPortOfLoadingBoundCodeFindBox";
			this.CB_RL_NKPortOfLoadingBoundCodeFindBox.ShowDescriptionBox = false;
			this.CB_RL_NKPortOfLoadingBoundCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			this.CB_RL_NKPortOfLoadingBoundCodeFindBox.TabIndex = 17;
			// 
			// DischargeLabel
			// 
			this.DischargeLabel.AutoSize = true;
			this.DischargeLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.DischargeLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(592, 40, true);
			this.DischargeLabel.Name = "DischargeLabel";
			this.DischargeLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 13, true);
			this.DischargeLabel.TabIndex = 18;
			this.DischargeLabel.Text = "Discharge Port:";
			// 
			// LoadingPortLabel
			// 
			this.LoadingPortLabel.AutoSize = true;
			this.LoadingPortLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.LoadingPortLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(602, 16, true);
			this.LoadingPortLabel.Name = "LoadingPortLabel";
			this.LoadingPortLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(70, 13, true);
			this.LoadingPortLabel.TabIndex = 16;
			this.LoadingPortLabel.Text = "Loading Port:";
			// 
			// VoyageLabel
			// 
			this.VoyageLabel.AutoSize = true;
			this.VoyageLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.VoyageLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(154, 40, true);
			this.VoyageLabel.Name = "VoyageLabel";
			this.VoyageLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(46, 13, true);
			this.VoyageLabel.TabIndex = 4;
			this.VoyageLabel.Text = "Voyage:";
			// 
			// CB_VoyageBoundTextBox
			// 
			this.BindingSource.SetBindingMember(this.CB_VoyageBoundTextBox, "CB_Voyage");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusSCAOceanBill)(null)).CB_Voyage)));
			this.CB_VoyageBoundTextBox.CaptionResourceString = null;
			this.CB_VoyageBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(203, 37, true);
			this.CB_VoyageBoundTextBox.Name = "CB_VoyageBoundTextBox";
			this.CB_VoyageBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(82, 20, true);
			this.CB_VoyageBoundTextBox.TabIndex = 5;
			// 
			// VesselLabel
			// 
			this.VesselLabel.AutoSize = true;
			this.VesselLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.VesselLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 16, true);
			this.VesselLabel.Name = "VesselLabel";
			this.VesselLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(41, 13, true);
			this.VesselLabel.TabIndex = 0;
			this.VesselLabel.Text = "Vessel:";
			// 
			// OceanBillLabel
			// 
			this.OceanBillLabel.AutoSize = true;
			this.OceanBillLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.OceanBillLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(326, 86, true);
			this.OceanBillLabel.Name = "OceanBillLabel";
			this.OceanBillLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(58, 13, true);
			this.OceanBillLabel.TabIndex = 14;
			this.OceanBillLabel.Text = "Ocean Bill:";
			// 
			// CB_OceanBillBoundTextBox
			// 
			this.BindingSource.SetBindingMember(this.CB_OceanBillBoundTextBox, "CB_OceanBill");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusSCAOceanBill)(null)).CB_OceanBill)));
			this.CB_OceanBillBoundTextBox.CaptionResourceString = null;
			this.CB_OceanBillBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(392, 84, true);
			this.CB_OceanBillBoundTextBox.Name = "CB_OceanBillBoundTextBox";
			this.CB_OceanBillBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(106, 20, true);
			this.CB_OceanBillBoundTextBox.TabIndex = 15;
			// 
			// MessagingModeLabel
			// 
			this.MessagingModeLabel.AutoSize = true;
			this.MessagingModeLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.MessagingModeLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(779, 86, true);
			this.MessagingModeLabel.Name = "MessagingModeLabel";
			this.MessagingModeLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(61, 13, true);
			this.MessagingModeLabel.TabIndex = 30;
			this.MessagingModeLabel.Text = "Messaging:";
			// 
			// branchLabel
			// 
			this.branchLabel.AutoSize = true;
			this.branchLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.branchLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 64, true);
			this.branchLabel.Name = "branchLabel";
			this.branchLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(44, 13, true);
			this.branchLabel.TabIndex = 6;
			this.branchLabel.Text = "Branch:";
			// 
			// BranchGuidFindBox
			// 
			this.BranchGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.BranchGuidFindBox, "CB_GB");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.AU.Declaration.Business.CusSCAOceanBill)(null)).CB_GB)));
			this.BranchGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(52, 61, true);
			this.BranchGuidFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.GlbBranch;
			this.BranchGuidFindBox.Name = "BranchGuidFindBox";
			this.BranchGuidFindBox.PreBoundMaxLength = 3;
			this.BranchGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(234, 20, true);
			this.BranchGuidFindBox.TabIndex = 7;
			// 
			// overrideFreightDefaultsCheckBox
			// 
			this.overrideFreightDefaultsCheckBox.BindTo = "OverrideFreightDefaults";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.AU.Declaration.Business.CusSCAOceanBill)(null)).OverrideFreightDefaults)));
			this.overrideFreightDefaultsCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(52, 85, true);
			this.overrideFreightDefaultsCheckBox.Name = "overrideFreightDefaultsCheckBox";
			this.overrideFreightDefaultsCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(234, 20, true);
			this.overrideFreightDefaultsCheckBox.TabIndex = 8;
			this.overrideFreightDefaultsCheckBox.Text = "Override Default Values from Consol";
			// 
			// OceanBillDetailsUserControl
			// 
			this.Controls.Add(this.GroupBoxOceanBill);
			this.Name = "OceanBillDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(967, 107, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.GroupBoxOceanBill.ResumeLayout(false);
			this.GroupBoxOceanBill.PerformLayout();
			this.FirstArrivalDateEdit.ResumeLayout(true);
			this.FirstArrivalDateEdit.PerformLayout();
			this.ArrivalDateEdit.ResumeLayout(true);
			this.ArrivalDateEdit.PerformLayout();
			this.DepartureDateEdit.ResumeLayout(true);
			this.DepartureDateEdit.PerformLayout();
			this.CB_ApplicationCodeBoundDropEdit.ResumeLayout(true);
			this.CB_ApplicationCodeBoundDropEdit.PerformLayout();
			this.LloydsDropEdit.ResumeLayout(true);
			this.LloydsDropEdit.PerformLayout();
			this.ShippingLineGuidFindBox.ResumeLayout(true);
			this.ShippingLineGuidFindBox.PerformLayout();
			this.CB_RV_NKVessel.ResumeLayout(true);
			this.CB_RV_NKVessel.PerformLayout();
			this.CB_RL_NKPortOfFirstArrivalCodeFindBox.ResumeLayout(true);
			this.CB_RL_NKPortOfFirstArrivalCodeFindBox.PerformLayout();
			this.CB_RL_NKPortOfDischargeBoundCodeFindBox.ResumeLayout(true);
			this.CB_RL_NKPortOfDischargeBoundCodeFindBox.PerformLayout();
			this.CB_RL_NKPortOfLoadingBoundCodeFindBox.ResumeLayout(true);
			this.CB_RL_NKPortOfLoadingBoundCodeFindBox.PerformLayout();
			this.BranchGuidFindBox.ResumeLayout(true);
			this.BranchGuidFindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		Enterprise.ZArchitecture.GUI.ZGroupBox GroupBoxOceanBill;
		ZArchitecture.ZLabel DepartureDateLabell;
		ZArchitecture.ZLabel ArrivalDateLabel;
		ZArchitecture.ZLabel FirstArrivalDateLabel;
		ZArchitecture.GUI.ZDateEdit FirstArrivalDateEdit;
		ZArchitecture.GUI.ZDateEdit ArrivalDateEdit;
		ZArchitecture.GUI.ZDateEdit DepartureDateEdit;
		Enterprise.ZArchitecture.ZTextBox ResponsiblePartyIDTextBox;
		Enterprise.ZArchitecture.ZTextBox PrincipalIDTextBox;
		Enterprise.ZArchitecture.GUI.ZDropEdit CB_ApplicationCodeBoundDropEdit;
		Enterprise.ZArchitecture.ZLabel zLabel3;
		Enterprise.ZArchitecture.ZLabel zLabel2;
		Enterprise.ZArchitecture.GUI.ZDropEdit LloydsDropEdit;
		Enterprise.ZArchitecture.ZLabel zLabel1;
		Enterprise.ZArchitecture.ZTextBox CusSCAOceanBillParentBillTextBox;
		Enterprise.ZArchitecture.ZLabel CusSCAOceanBillParentBillLabel;
		Enterprise.ZArchitecture.GUI.ZGuidFindBox ShippingLineGuidFindBox;
		Enterprise.ZArchitecture.ZLabel ShippingLineLabel;
		Enterprise.ZArchitecture.GUI.ZCodeFindBox CB_RV_NKVessel;
		ZArchitecture.GUI.ZCodeFindBox CB_RL_NKPortOfFirstArrivalCodeFindBox;
		Enterprise.ZArchitecture.GUI.ZCodeFindBox CB_RL_NKPortOfDischargeBoundCodeFindBox;
		ZArchitecture.ZLabel ArrivalLabel;
		Enterprise.ZArchitecture.GUI.ZCodeFindBox CB_RL_NKPortOfLoadingBoundCodeFindBox;
		Enterprise.ZArchitecture.ZLabel DischargeLabel;
		Enterprise.ZArchitecture.ZLabel LoadingPortLabel;
		Enterprise.ZArchitecture.ZLabel VoyageLabel;
		Enterprise.ZArchitecture.ZTextBox CB_VoyageBoundTextBox;
		Enterprise.ZArchitecture.ZLabel VesselLabel;
		Enterprise.ZArchitecture.ZLabel OceanBillLabel;
		Enterprise.ZArchitecture.ZTextBox CB_OceanBillBoundTextBox;
		Enterprise.ZArchitecture.ZLabel MessagingModeLabel;
		ZArchitecture.ZLabel branchLabel;
		Enterprise.ZArchitecture.GUI.ZGuidFindBox BranchGuidFindBox;
		Enterprise.ZArchitecture.GUI.ZCheckBox overrideFreightDefaultsCheckBox;
	}
}
