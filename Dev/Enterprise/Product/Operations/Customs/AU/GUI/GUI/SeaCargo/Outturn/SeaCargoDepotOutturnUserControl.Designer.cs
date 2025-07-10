using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture;
using System.ComponentModel;

namespace Enterprise.Customs.AU.SeaCargo.GUI
{
	public partial class SeaCargoDepotOutturnUserControl
	{
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo5 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo10 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo11 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo12 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo13 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo14 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo15 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo16 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo17 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo18 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo19 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo20 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo21 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo22 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo23 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.vesselCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.vesselLabel = new Enterprise.ZArchitecture.ZLabel();
			this.vesselLloydsLabel = new Enterprise.ZArchitecture.ZLabel();
			this.vesselLloydsDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.voyageNumberLabel = new Enterprise.ZArchitecture.ZLabel();
			this.voyageNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.outturningPremiseAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.outturningPremiseLabel = new Enterprise.ZArchitecture.ZLabel();
			this.outturningPremiseIDLabel = new Enterprise.ZArchitecture.ZLabel();
			this.outturningPremiseIDTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.outturnHeaderGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.nilOutturnButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.c6_ResponsiblePartyIDBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.outturnStatusTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.responsiblePartyLabel = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel1 = new Enterprise.ZArchitecture.ZLabel();
			this.outturnsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.OutturnsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.splitter1 = new CargoWise.Windows.UI.KSplitter();
			this.outturnTabControl = new Enterprise.ZArchitecture.GUI.ZTemplateTabControl();
			this.outturnDetailsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.seaCargoOutturnDetailUserControl1 = new Enterprise.Customs.AU.SeaCargo.GUI.SeaCargoOutturnDetailUserControl();
			this.outturnMessagesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.outturnMessagesControl = new Enterprise.Messaging.GUI.EDIMessageUserControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.vesselCodeFindBox.SuspendLayout();
			this.vesselLloydsDropEdit.SuspendLayout();
			this.outturningPremiseAddressControl.SuspendLayout();
			this.outturnHeaderGroupBox.SuspendLayout();
			this.outturnsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.OutturnsGrid)).BeginInit();
			this.OutturnsGrid.SuspendLayout();
			this.outturnTabControl.SuspendLayout();
			this.outturnDetailsTabPage.SuspendLayout();
			this.seaCargoOutturnDetailUserControl1.SuspendLayout();
			this.outturnMessagesTabPage.SuspendLayout();
			this.outturnMessagesControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.AU.Declaration.Business.CusOutturnHeader);
			// 
			// VesselCodeFindBox
			// 
			this.vesselCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.vesselCodeFindBox, "C6_VesselName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusOutturnHeader)(null)).C6_VesselName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusOutturnHeader)(null)).Lookups.VesselNames)));
			this.vesselCodeFindBox.BindToList = "Lookups+VesselNames";
			this.vesselCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 13, true);
			this.vesselCodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefVessel;
			this.vesselCodeFindBox.Name = "VesselCodeFindBox";
			this.vesselCodeFindBox.PreBoundMaxLength = 35;
			this.vesselCodeFindBox.ShouldResize = true;
			this.vesselCodeFindBox.ShowDescriptionBox = false;
			this.vesselCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(234, 20, true);
			this.vesselCodeFindBox.TabIndex = 0;
			// 
			// VesselLabel
			// 
			this.vesselLabel.AutoSize = true;
			this.vesselLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.vesselLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 16, true);
			this.vesselLabel.Name = "VesselLabel";
			this.vesselLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(41, 13, true);
			this.vesselLabel.TabIndex = 2;
			this.vesselLabel.Text = "Vessel:";
			// 
			// VesselLloydsLabel
			// 
			this.vesselLloydsLabel.AutoSize = true;
			this.vesselLloydsLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.vesselLloydsLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 43, true);
			this.vesselLloydsLabel.Name = "VesselLloydsLabel";
			this.vesselLloydsLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(99, 13, true);
			this.vesselLloydsLabel.TabIndex = 4;
			this.vesselLloydsLabel.Text = "Vessel Lloyds/IMO:";
			// 
			// VesselLloydsDropEdit
			// 
			this.vesselLloydsDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.vesselLloydsDropEdit, "C6_LloydsIMO");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.AU.Declaration.Business.CusOutturnHeader)(null)).C6_LloydsIMO)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusOutturnHeader)(null)).Lookups.LloydsIMOList)));
			this.vesselLloydsDropEdit.BindToList = "Lookups+LloydsIMOList";
			this.vesselLloydsDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(111, 40, true);
			this.vesselLloydsDropEdit.Name = "VesselLloydsDropEdit";
			this.vesselLloydsDropEdit.ShouldResizeByMaxLength = true;
			this.vesselLloydsDropEdit.ShowDescriptionBox = false;
			this.vesselLloydsDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.vesselLloydsDropEdit.TabIndex = 1;
			// 
			// VoyageNumberLabel
			// 
			this.voyageNumberLabel.AutoSize = true;
			this.voyageNumberLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.voyageNumberLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(756, 16, true);
			this.voyageNumberLabel.Name = "VoyageNumberLabel";
			this.voyageNumberLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(86, 13, true);
			this.voyageNumberLabel.TabIndex = 0;
			this.voyageNumberLabel.Text = "Voyage Number:";
			// 
			// VoyageNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.voyageNumberTextBox, "C6_VoyageNum");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusOutturnHeader)(null)).C6_VoyageNum)));
			this.voyageNumberTextBox.CaptionResourceString = null;
			this.voyageNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(859, 13, true);
			this.voyageNumberTextBox.Name = "VoyageNumberTextBox";
			this.voyageNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(110, 20, true);
			this.voyageNumberTextBox.TabIndex = 5;
			// 
			// OutturningPremiseAddressControl
			// 
			this.outturningPremiseAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.outturningPremiseAddressControl, "C6_OA_OutturningPremise");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.AU.Declaration.Business.CusOutturnHeader)(null)).C6_OA_OutturningPremise)));
			this.outturningPremiseAddressControl.BindToOrgList = "Lookups+CTOAddressOrgs";
			this.outturningPremiseAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(434, 13, true);
			this.outturningPremiseAddressControl.Name = "OutturningPremiseAddressControl";
			this.outturningPremiseAddressControl.PopupCaption = "";
			this.outturningPremiseAddressControl.ReadOnly = false;
			this.outturningPremiseAddressControl.ShowAddress = false;
			this.outturningPremiseAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 20, true);
			this.outturningPremiseAddressControl.TabIndex = 3;
			// 
			// OutturningPremiseLabel
			// 
			this.outturningPremiseLabel.AutoSize = true;
			this.outturningPremiseLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.outturningPremiseLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(366, 16, true);
			this.outturningPremiseLabel.Name = "OutturningPremiseLabel";
			this.outturningPremiseLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(47, 13, true);
			this.outturningPremiseLabel.TabIndex = 6;
			this.outturningPremiseLabel.Text = "Premise:";
			// 
			// OutturningPremiseIDLabel
			// 
			this.outturningPremiseIDLabel.AutoSize = true;
			this.outturningPremiseIDLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.outturningPremiseIDLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(366, 43, true);
			this.outturningPremiseIDLabel.Name = "OutturningPremiseIDLabel";
			this.outturningPremiseIDLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(61, 13, true);
			this.outturningPremiseIDLabel.TabIndex = 8;
			this.outturningPremiseIDLabel.Text = "Premise ID:";
			// 
			// OutturningPremiseIDTextBox
			// 
			this.BindingSource.SetBindingMember(this.outturningPremiseIDTextBox, "C6_OutturningPremiseID");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusOutturnHeader)(null)).C6_OutturningPremiseID)));
			this.outturningPremiseIDTextBox.CaptionResourceString = null;
			this.outturningPremiseIDTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(434, 40, true);
			this.outturningPremiseIDTextBox.Name = "OutturningPremiseIDTextBox";
			this.outturningPremiseIDTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.outturningPremiseIDTextBox.TabIndex = 4;
			// 
			// OutturnHeaderGroupBox
			// 
			this.outturnHeaderGroupBox.BackColor = System.Drawing.SystemColors.Control;
			this.outturnHeaderGroupBox.Controls.Add(this.nilOutturnButton);
			this.outturnHeaderGroupBox.Controls.Add(this.c6_ResponsiblePartyIDBoundTextBox);
			this.outturnHeaderGroupBox.Controls.Add(this.voyageNumberTextBox);
			this.outturnHeaderGroupBox.Controls.Add(this.outturningPremiseIDTextBox);
			this.outturnHeaderGroupBox.Controls.Add(this.outturningPremiseAddressControl);
			this.outturnHeaderGroupBox.Controls.Add(this.outturnStatusTextBox);
			this.outturnHeaderGroupBox.Controls.Add(this.vesselLloydsDropEdit);
			this.outturnHeaderGroupBox.Controls.Add(this.vesselCodeFindBox);
			this.outturnHeaderGroupBox.Controls.Add(this.responsiblePartyLabel);
			this.outturnHeaderGroupBox.Controls.Add(this.vesselLabel);
			this.outturnHeaderGroupBox.Controls.Add(this.outturningPremiseIDLabel);
			this.outturnHeaderGroupBox.Controls.Add(this.vesselLloydsLabel);
			this.outturnHeaderGroupBox.Controls.Add(this.voyageNumberLabel);
			this.outturnHeaderGroupBox.Controls.Add(this.zLabel1);
			this.outturnHeaderGroupBox.Controls.Add(this.outturningPremiseLabel);
			this.outturnHeaderGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.outturnHeaderGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.outturnHeaderGroupBox.Name = "OutturnHeaderGroupBox";
			this.outturnHeaderGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1000, 104, true);
			this.outturnHeaderGroupBox.TabIndex = 0;
			this.outturnHeaderGroupBox.TabStop = false;
			this.outturnHeaderGroupBox.Text = "Outturn";
			// 
			// NilOutturnButton
			// 
			this.nilOutturnButton.IsCaptionOverridden = true;
			this.nilOutturnButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(859, 65, true);
			this.nilOutturnButton.Name = "NilOutturnButton";
			this.nilOutturnButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.nilOutturnButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.nilOutturnButton.TabIndex = 22;
			this.nilOutturnButton.Text = "Nil Outturn";
			this.nilOutturnButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.nilOutturnButton.ToolTipCaption = null;
			this.nilOutturnButton.UseVisualStyleBackColor = true;
			this.nilOutturnButton.Click += new System.EventHandler(this.NilOutturnButton_Click);
			// 
			// C6_ResponsiblePartyIDBoundTextBox
			// 
			this.BindingSource.SetBindingMember(this.c6_ResponsiblePartyIDBoundTextBox, "C6_ResponsiblePartyID");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusOutturnHeader)(null)).C6_ResponsiblePartyID)));
			this.c6_ResponsiblePartyIDBoundTextBox.CaptionResourceString = null;
			this.c6_ResponsiblePartyIDBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(859, 39, true);
			this.c6_ResponsiblePartyIDBoundTextBox.Name = "C6_ResponsiblePartyIDBoundTextBox";
			this.c6_ResponsiblePartyIDBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(110, 20, true);
			this.c6_ResponsiblePartyIDBoundTextBox.TabIndex = 6;
			// 
			// OutturnStatusTextBox
			// 
			this.BindingSource.SetBindingMember(this.outturnStatusTextBox, "OutturnStatus+Description");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusOutturnHeader)(null)).OutturnStatus.Description)));
			this.outturnStatusTextBox.CaptionResourceString = null;
			this.outturnStatusTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 66, true);
			this.outturnStatusTextBox.Name = "OutturnStatusTextBox";
			this.outturnStatusTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(210, 20, true);
			this.outturnStatusTextBox.TabIndex = 2;
			// 
			// ResponsiblePartyLabel
			// 
			this.responsiblePartyLabel.AutoSize = true;
			this.responsiblePartyLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.responsiblePartyLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(756, 43, true);
			this.responsiblePartyLabel.Name = "ResponsiblePartyLabel";
			this.responsiblePartyLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(95, 13, true);
			this.responsiblePartyLabel.TabIndex = 21;
			this.responsiblePartyLabel.Text = "Responsible Party:";
			// 
			// zLabel1
			// 
			this.zLabel1.AutoSize = true;
			this.zLabel1.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 69, true);
			this.zLabel1.Name = "zLabel1";
			this.zLabel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(86, 13, true);
			this.zLabel1.TabIndex = 11;
			this.zLabel1.Text = "Message Status:";
			// 
			// OutturnsGroupBox
			// 
			this.outturnsGroupBox.Controls.Add(this.OutturnsGrid);
			this.outturnsGroupBox.Controls.Add(this.splitter1);
			this.outturnsGroupBox.Controls.Add(this.outturnTabControl);
			this.outturnsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.outturnsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 104, true);
			this.outturnsGroupBox.Name = "OutturnsGroupBox";
			this.outturnsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1000, 480, true);
			this.outturnsGroupBox.TabIndex = 1;
			this.outturnsGroupBox.TabStop = false;
			this.outturnsGroupBox.Text = "Outturn Bills";
			// 
			// OutturnsGrid
			// 
			this.OutturnsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.OutturnsGrid, "Outturns");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusOutturnHeader)(null)).Outturns)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.DepotCusOutturn)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusOutturnHeader)(null)).Outturns)).SyncRoot)).C5_CargoType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.DepotCusOutturn)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusOutturnHeader)(null)).Outturns)).SyncRoot)).Lookups.CargoTypes)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.DepotCusOutturn)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusOutturnHeader)(null)).Outturns)).SyncRoot)).C5_ContainerNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.DepotCusOutturn)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusOutturnHeader)(null)).Outturns)).SyncRoot)).C5_HouseBill)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.DepotCusOutturn)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusOutturnHeader)(null)).Outturns)).SyncRoot)).C5_MasterBill)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.AU.Declaration.Business.DepotCusOutturn)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusOutturnHeader)(null)).Outturns)).SyncRoot)).C5_CargoReceiptDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.AU.Declaration.Business.DepotCusOutturn)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusOutturnHeader)(null)).Outturns)).SyncRoot)).C5_CargoUnpackDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.DepotCusOutturn)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusOutturnHeader)(null)).Outturns)).SyncRoot)).OutturnStatus)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.AU.Declaration.Business.DepotCusOutturn)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusOutturnHeader)(null)).Outturns)).SyncRoot)).C5_OuterPacks)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.DepotCusOutturn)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusOutturnHeader)(null)).Outturns)).SyncRoot)).C5_OuterPackUnits)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.DepotCusOutturn)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusOutturnHeader)(null)).Outturns)).SyncRoot)).Lookups.PackageTypes)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.AU.Declaration.Business.DepotCusOutturn)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusOutturnHeader)(null)).Outturns)).SyncRoot)).C5_PackagesOutturned)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.DepotCusOutturn)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusOutturnHeader)(null)).Outturns)).SyncRoot)).C5_PackagesUnits)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.DepotCusOutturn)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusOutturnHeader)(null)).Outturns)).SyncRoot)).Lookups.PackageTypes)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.DepotCusOutturn)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusOutturnHeader)(null)).Outturns)).SyncRoot)).C5_OutturnResultType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.DepotCusOutturn)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusOutturnHeader)(null)).Outturns)).SyncRoot)).Lookups.OutturnResultTypeList)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.AU.Declaration.Business.DepotCusOutturn)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusOutturnHeader)(null)).Outturns)).SyncRoot)).C5_DamageIndicator)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.AU.Declaration.Business.DepotCusOutturn)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusOutturnHeader)(null)).Outturns)).SyncRoot)).C5_PillageIndicator)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.AU.Declaration.Business.DepotCusOutturn)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusOutturnHeader)(null)).Outturns)).SyncRoot)).C5_SealIntactIndicator)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.DepotCusOutturn)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusOutturnHeader)(null)).Outturns)).SyncRoot)).C5_CommercialStatus)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.DepotCusOutturn)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusOutturnHeader)(null)).Outturns)).SyncRoot)).Lookups.CommercialStatusList)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.DepotCusOutturn)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusOutturnHeader)(null)).Outturns)).SyncRoot)).C5_CustomsStatus)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.DepotCusOutturn)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusOutturnHeader)(null)).Outturns)).SyncRoot)).C5_MessageStatus)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.DepotCusOutturn)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusOutturnHeader)(null)).Outturns)).SyncRoot)).C5_GoodsDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.DepotCusOutturn)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusOutturnHeader)(null)).Outturns)).SyncRoot)).C5_ContainerSeal)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.DepotCusOutturn)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusOutturnHeader)(null)).Outturns)).SyncRoot)).C5_MarksAndNumbers)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.DepotCusOutturn)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusOutturnHeader)(null)).Outturns)).SyncRoot)).LoadList.JK_UniqueConsignRef)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.DepotCusOutturn)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusOutturnHeader)(null)).Outturns)).SyncRoot)).ShipmentOrContainerNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.AU.Declaration.Business.DepotCusOutturn)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusOutturnHeader)(null)).Outturns)).SyncRoot)).SEIMatched)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.AU.Declaration.Business.DepotCusOutturn)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusOutturnHeader)(null)).Outturns)).SyncRoot)).SEIProcessingDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.DepotCusOutturn)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusOutturnHeader)(null)).Outturns)).SyncRoot)).FreightForwarderIndicator)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.DepotCusOutturn)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusOutturnHeader)(null)).Outturns)).SyncRoot)).UBMRequestReason)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.DepotCusOutturn)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusOutturnHeader)(null)).Outturns)).SyncRoot)).InlandMovementMode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.DepotCusOutturn)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusOutturnHeader)(null)).Outturns)).SyncRoot)).UBMResponsibleID)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.DepotCusOutturn)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusOutturnHeader)(null)).Outturns)).SyncRoot)).UBMResponsibleIDName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.DepotCusOutturn)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusOutturnHeader)(null)).Outturns)).SyncRoot)).RecipientSiteID)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.DepotCusOutturn)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusOutturnHeader)(null)).Outturns)).SyncRoot)).ConsigneeName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.DepotCusOutturn)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusOutturnHeader)(null)).Outturns)).SyncRoot)).GrossWeight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.DepotCusOutturn)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusOutturnHeader)(null)).Outturns)).SyncRoot)).NetWeight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.DepotCusOutturn)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusOutturnHeader)(null)).Outturns)).SyncRoot)).Volume)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.DepotCusOutturn)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusOutturnHeader)(null)).Outturns)).SyncRoot)).UBMOriginID)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.DepotCusOutturn)(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusOutturnHeader)(null)).Outturns)).SyncRoot)).UBMDestinationID)));
			this.OutturnsGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.BindToList = "Lookups+CargoTypes";
			zDropEditColumnStyleInfo1.Caption = "Cargo Type";
			zDropEditColumnStyleInfo1.ColumnName = "C5_CargoType";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo1.Caption = "Container Number";
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "C5_ContainerNumber";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.Caption = "House Bill";
			zTextBoxColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo2.ColumnName = "C5_HouseBill";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo3.Caption = "Ocean Bill";
			zTextBoxColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo3.ColumnName = "C5_MasterBill";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo1.Caption = "Cargo Receipt Date";
			zDateEditColumnStyleInfo1.ColumnName = "C5_CargoReceiptDate";
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo2.Caption = "Cargo Unpack Date";
			zDateEditColumnStyleInfo2.ColumnName = "C5_CargoUnpackDate";
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo4.Caption = "Outturn Status";
			zTextBoxColumnStyleInfo4.ColumnName = "OutturnStatus";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.Caption = "Manifested";
			zCalcEditColumnStyleInfo1.ColumnName = "C5_OuterPacks";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zDropEditColumnStyleInfo2.BindToList = "Lookups+PackageTypes";
			zDropEditColumnStyleInfo2.Caption = "Manifested Units";
			zDropEditColumnStyleInfo2.ColumnName = "C5_OuterPackUnits";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.Caption = "Outturned";
			zCalcEditColumnStyleInfo2.ColumnName = "C5_PackagesOutturned";
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zDropEditColumnStyleInfo3.BindToList = "Lookups+PackageTypes";
			zDropEditColumnStyleInfo3.Caption = "Pack Units";
			zDropEditColumnStyleInfo3.ColumnName = "C5_PackagesUnits";
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo4.BindToList = "Lookups+OutturnResultTypeList";
			zDropEditColumnStyleInfo4.Caption = "Outturn Result";
			zDropEditColumnStyleInfo4.ColumnName = "C5_OutturnResultType";
			zDropEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo1.Caption = "Damaged";
			zCheckBoxColumnStyleInfo1.ColumnName = "C5_DamageIndicator";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zCheckBoxColumnStyleInfo2.Caption = "Pillage";
			zCheckBoxColumnStyleInfo2.ColumnName = "C5_PillageIndicator";
			zCheckBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zCheckBoxColumnStyleInfo3.Caption = "Seal Intact";
			zCheckBoxColumnStyleInfo3.ColumnName = "C5_SealIntactIndicator";
			zCheckBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zDropEditColumnStyleInfo5.BindToList = "Lookups+CommercialStatusList";
			zDropEditColumnStyleInfo5.Caption = "Commercial Status";
			zDropEditColumnStyleInfo5.ColumnName = "C5_CommercialStatus";
			zDropEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo5.Caption = "Customs Status";
			zTextBoxColumnStyleInfo5.ColumnName = "C5_CustomsStatus";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo6.Caption = "Underbond Status";
			zTextBoxColumnStyleInfo6.ColumnName = "C5_MessageStatus";
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo7.Caption = "Goods Description";
			zTextBoxColumnStyleInfo7.ColumnName = "C5_GoodsDescription";
			zTextBoxColumnStyleInfo7.IsVisible = false;
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo8.Caption = "Container Seal";
			zTextBoxColumnStyleInfo8.ColumnName = "C5_ContainerSeal";
			zTextBoxColumnStyleInfo8.IsVisible = false;
			zTextBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo9.Caption = "Marks And Numbers";
			zTextBoxColumnStyleInfo9.ColumnName = "C5_MarksAndNumbers";
			zTextBoxColumnStyleInfo9.IsVisible = false;
			zTextBoxColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo10.Caption = "Load List";
			zTextBoxColumnStyleInfo10.ColumnName = "LoadList+JK_UniqueConsignRef";
			zTextBoxColumnStyleInfo10.IsReadOnly = true;
			zTextBoxColumnStyleInfo10.IsVisible = false;
			zTextBoxColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo11.Caption = "Shipment Or Container Number";
			zTextBoxColumnStyleInfo11.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("SeaCargoDepotOutturnUserControl|91e8ebec-5378-41a8-995e-18e605cfee89", "Shipment Or Container Number");
			zTextBoxColumnStyleInfo11.ColumnName = "ShipmentOrContainerNumber";
			zTextBoxColumnStyleInfo11.IsReadOnly = true;
			zTextBoxColumnStyleInfo11.IsVisible = false;
			zTextBoxColumnStyleInfo11.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo4.Caption = "SEI";
			zCheckBoxColumnStyleInfo4.ColumnName = "SEIMatched";
			zCheckBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(10);
			zDateEditColumnStyleInfo3.Caption = "SEI Time";
			zDateEditColumnStyleInfo3.ColumnName = "SEIProcessingDate";
			zDateEditColumnStyleInfo3.IsVisible = false;
			zDateEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo12.Caption = "FF Ind";
			zTextBoxColumnStyleInfo12.ColumnName = "FreightForwarderIndicator";
			zTextBoxColumnStyleInfo12.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zTextBoxColumnStyleInfo13.Caption = "UBM Reason";
			zTextBoxColumnStyleInfo13.ColumnName = "UBMRequestReason";
			zTextBoxColumnStyleInfo13.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zTextBoxColumnStyleInfo14.Caption = "Mode";
			zTextBoxColumnStyleInfo14.ColumnName = "InlandMovementMode";
			zTextBoxColumnStyleInfo14.IsVisible = false;
			zTextBoxColumnStyleInfo14.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zTextBoxColumnStyleInfo15.Caption = "UBM Party ID";
			zTextBoxColumnStyleInfo15.ColumnName = "UBMResponsibleID";
			zTextBoxColumnStyleInfo15.IsVisible = false;
			zTextBoxColumnStyleInfo15.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo16.Caption = "UBM Party Name";
			zTextBoxColumnStyleInfo16.ColumnName = "UBMResponsibleIDName";
			zTextBoxColumnStyleInfo16.IsVisible = false;
			zTextBoxColumnStyleInfo16.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo17.Caption = "Site";
			zTextBoxColumnStyleInfo17.ColumnName = "RecipientSiteID";
			zTextBoxColumnStyleInfo17.IsVisible = false;
			zTextBoxColumnStyleInfo17.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo18.Caption = "Consignee";
			zTextBoxColumnStyleInfo18.ColumnName = "ConsigneeName";
			zTextBoxColumnStyleInfo18.IsVisible = false;
			zTextBoxColumnStyleInfo18.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo19.Caption = "Gross Wt";
			zTextBoxColumnStyleInfo19.ColumnName = "GrossWeight";
			zTextBoxColumnStyleInfo19.IsVisible = false;
			zTextBoxColumnStyleInfo19.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo20.Caption = "Net Wt";
			zTextBoxColumnStyleInfo20.ColumnName = "NetWeight";
			zTextBoxColumnStyleInfo20.IsVisible = false;
			zTextBoxColumnStyleInfo20.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo21.Caption = "Volume";
			zTextBoxColumnStyleInfo21.ColumnName = "Volume";
			zTextBoxColumnStyleInfo21.IsVisible = false;
			zTextBoxColumnStyleInfo21.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo22.Caption = "UBM Org";
			zTextBoxColumnStyleInfo22.ColumnName = "UBMOriginID";
			zTextBoxColumnStyleInfo22.IsVisible = false;
			zTextBoxColumnStyleInfo22.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo23.Caption = "UBM Dest";
			zTextBoxColumnStyleInfo23.ColumnName = "UBMDestinationID";
			zTextBoxColumnStyleInfo23.IsVisible = false;
			zTextBoxColumnStyleInfo23.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			this.OutturnsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.OutturnsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.OutturnsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.OutturnsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.OutturnsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.OutturnsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.OutturnsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.OutturnsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.OutturnsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.OutturnsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.OutturnsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.OutturnsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo4);
			this.OutturnsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.OutturnsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.OutturnsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo3);
			this.OutturnsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo5);
			this.OutturnsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.OutturnsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.OutturnsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.OutturnsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.OutturnsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
			this.OutturnsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo10);
			this.OutturnsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo11);
			this.OutturnsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo4);
			this.OutturnsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo3);
			this.OutturnsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo12);
			this.OutturnsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo13);
			this.OutturnsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo14);
			this.OutturnsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo15);
			this.OutturnsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo16);
			this.OutturnsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo17);
			this.OutturnsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo18);
			this.OutturnsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo19);
			this.OutturnsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo20);
			this.OutturnsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo21);
			this.OutturnsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo22);
			this.OutturnsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo23);
			this.OutturnsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.OutturnsGrid.GridId = "2952859c-8b79-42b2-9acc-dabc91992cc6";
			this.OutturnsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.OutturnsGrid.LayoutKey = "OutturnsGrid";
			this.OutturnsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.OutturnsGrid.Name = "OutturnsGrid";
			this.OutturnsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(994, 275, true);
			this.OutturnsGrid.TabIndex = 1;
			// 
			// splitter1
			// 
			this.splitter1.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.splitter1.DoNotSaveSplitterLayout = false;
			this.splitter1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 291, true);
			this.splitter1.Name = "splitter1";
			this.splitter1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(994, 3, true);
			this.splitter1.TabIndex = 2;
			this.splitter1.TabStop = false;
			// 
			// OutturnTabControl
			// 
			this.outturnTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.outturnTabControl.Controls.Add(this.outturnDetailsTabPage);
			this.outturnTabControl.Controls.Add(this.outturnMessagesTabPage);
			this.outturnTabControl.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.outturnTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 294, true);
			this.outturnTabControl.Name = "OutturnTabControl";
			this.outturnTabControl.SelectedIndex = 0;
			this.outturnTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(994, 183, true);
			this.outturnTabControl.TabIndex = 2;
			// 
			// OutturnDetailsTabPage
			// 
			this.outturnDetailsTabPage.Controls.Add(this.seaCargoOutturnDetailUserControl1);
			this.outturnDetailsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.outturnDetailsTabPage.Name = "OutturnDetailsTabPage";
			this.outturnDetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(986, 156, true);
			this.outturnDetailsTabPage.TabIndex = 0;
			this.outturnDetailsTabPage.Text = "Details";
			// 
			// seaCargoOutturnDetailUserControl1
			// 
			this.seaCargoOutturnDetailUserControl1.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.seaCargoOutturnDetailUserControl1, ".");
			this.seaCargoOutturnDetailUserControl1.CurrentOutturn = null;
			this.seaCargoOutturnDetailUserControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.seaCargoOutturnDetailUserControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.seaCargoOutturnDetailUserControl1.Name = "seaCargoOutturnDetailUserControl1";
			this.seaCargoOutturnDetailUserControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(986, 156, true);
			this.seaCargoOutturnDetailUserControl1.TabIndex = 1;
			// 
			// OutturnMessagesTabPage
			// 
			this.outturnMessagesTabPage.Controls.Add(this.outturnMessagesControl);
			this.outturnMessagesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.outturnMessagesTabPage.Name = "OutturnMessagesTabPage";
			this.outturnMessagesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(986, 156, true);
			this.outturnMessagesTabPage.TabIndex = 1;
			this.outturnMessagesTabPage.Text = "Messages";
			// 
			// OutturnMessagesControl
			// 
			this.outturnMessagesControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.outturnMessagesControl, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Messaging.Business.IEDIMessageCollectionProvider)(((Enterprise.Customs.AU.Declaration.Business.CusOutturnHeader)(null)))));
			this.outturnMessagesControl.BindPrepend = "";
			this.outturnMessagesControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.outturnMessagesControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.outturnMessagesControl.Name = "OutturnMessagesControl";
			this.outturnMessagesControl.ShowChangingBlueMessageHeading = false;
			this.outturnMessagesControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(986, 156, true);
			this.outturnMessagesControl.TabIndex = 13;
			// 
			// SeaCargoDepotOutturnUserControl
			// 
			this.Controls.Add(this.outturnsGroupBox);
			this.Controls.Add(this.outturnHeaderGroupBox);
			this.Name = "SeaCargoDepotOutturnUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1000, 584, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.vesselCodeFindBox.ResumeLayout(true);
			this.vesselCodeFindBox.PerformLayout();
			this.vesselLloydsDropEdit.ResumeLayout(true);
			this.vesselLloydsDropEdit.PerformLayout();
			this.outturningPremiseAddressControl.ResumeLayout(true);
			this.outturningPremiseAddressControl.PerformLayout();
			this.outturnHeaderGroupBox.ResumeLayout(false);
			this.outturnHeaderGroupBox.PerformLayout();
			this.outturnsGroupBox.ResumeLayout(false);
			this.outturnsGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.OutturnsGrid)).EndInit();
			this.OutturnsGrid.ResumeLayout(false);
			this.OutturnsGrid.PerformLayout();
			this.outturnTabControl.ResumeLayout(false);
			this.outturnTabControl.PerformLayout();
			this.outturnDetailsTabPage.ResumeLayout(false);
			this.outturnDetailsTabPage.PerformLayout();
			this.seaCargoOutturnDetailUserControl1.ResumeLayout(true);
			this.seaCargoOutturnDetailUserControl1.PerformLayout();
			this.outturnMessagesTabPage.ResumeLayout(false);
			this.outturnMessagesTabPage.PerformLayout();
			this.outturnMessagesControl.ResumeLayout(true);
			this.outturnMessagesControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		private ZLabel vesselLabel;
		private ZLabel vesselLloydsLabel;
		private ZDropEdit vesselLloydsDropEdit;
		private ZLabel voyageNumberLabel;
		private ZCodeFindBox vesselCodeFindBox;
		private ZTextBox voyageNumberTextBox;
		private ZAddressControl outturningPremiseAddressControl;
		private ZLabel outturningPremiseLabel;
		private ZLabel outturningPremiseIDLabel;
		private ZTextBox outturningPremiseIDTextBox;
		private ZGroupBox outturnHeaderGroupBox;
		private ZGroupBox outturnsGroupBox;
		internal ZGrid OutturnsGrid;
		private ZTextBox outturnStatusTextBox;
		private ZLabel zLabel1;
		private ZTextBox c6_ResponsiblePartyIDBoundTextBox;
		private CargoWise.Windows.UI.KSplitter splitter1;
		private ZTabPage outturnDetailsTabPage;
		internal ZTabPage outturnMessagesTabPage;
		internal ZTemplateTabControl outturnTabControl;
		internal Messaging.GUI.EDIMessageUserControl outturnMessagesControl;
		private ZLabel responsiblePartyLabel;
		internal ZButton nilOutturnButton;
		internal SeaCargoOutturnDetailUserControl seaCargoOutturnDetailUserControl1;
		private IContainer components;
	}
}
