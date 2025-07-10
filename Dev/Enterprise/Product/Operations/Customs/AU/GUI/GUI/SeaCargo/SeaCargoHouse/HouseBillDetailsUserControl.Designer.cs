namespace Enterprise.Customs.AU.SeaCargo.GUI
{
	partial class HouseBillDetailsUserControl
	{
		private void InitializeComponent()
		{
			this.GroupBoxHouseBill = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.DetailsButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.MessageStatusLabel = new Enterprise.ZArchitecture.ZLabel();
			this.CustomsStatusLabel = new Enterprise.ZArchitecture.ZLabel();
			this.MessageStatusTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.FreightForwarderIndicatorCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ParentBillLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ParentBillTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CA_RN_NKGoodsOriginBoundCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.CA_RL_NK_PortOfDestinationBoundCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.CA_RL_NK_PortOfOriginBoundCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.CA_PrepaidCollectOtherBoundDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.PaymentTypeLabel = new Enterprise.ZArchitecture.ZLabel();
			this.GoodsOriginLabel = new Enterprise.ZArchitecture.ZLabel();
			this.DestinationLabel = new Enterprise.ZArchitecture.ZLabel();
			this.OriginLabel = new Enterprise.ZArchitecture.ZLabel();
			this.CA_HouseBillBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CA_ShipmentStatusBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.HouseBillLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.GroupBoxHouseBill.SuspendLayout();
			this.CA_RN_NKGoodsOriginBoundCodeFindBox.SuspendLayout();
			this.CA_RL_NK_PortOfDestinationBoundCodeFindBox.SuspendLayout();
			this.CA_RL_NK_PortOfOriginBoundCodeFindBox.SuspendLayout();
			this.CA_PrepaidCollectOtherBoundDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.AU.Declaration.Business.CusSCAHouse);
			// 
			// GroupBoxHouseBill
			// 
			this.GroupBoxHouseBill.Controls.Add(this.DetailsButton);
			this.GroupBoxHouseBill.Controls.Add(this.MessageStatusLabel);
			this.GroupBoxHouseBill.Controls.Add(this.CustomsStatusLabel);
			this.GroupBoxHouseBill.Controls.Add(this.MessageStatusTextBox);
			this.GroupBoxHouseBill.Controls.Add(this.FreightForwarderIndicatorCheckBox);
			this.GroupBoxHouseBill.Controls.Add(this.ParentBillLabel);
			this.GroupBoxHouseBill.Controls.Add(this.ParentBillTextBox);
			this.GroupBoxHouseBill.Controls.Add(this.CA_RN_NKGoodsOriginBoundCodeFindBox);
			this.GroupBoxHouseBill.Controls.Add(this.CA_RL_NK_PortOfDestinationBoundCodeFindBox);
			this.GroupBoxHouseBill.Controls.Add(this.CA_RL_NK_PortOfOriginBoundCodeFindBox);
			this.GroupBoxHouseBill.Controls.Add(this.CA_PrepaidCollectOtherBoundDropEdit);
			this.GroupBoxHouseBill.Controls.Add(this.PaymentTypeLabel);
			this.GroupBoxHouseBill.Controls.Add(this.GoodsOriginLabel);
			this.GroupBoxHouseBill.Controls.Add(this.DestinationLabel);
			this.GroupBoxHouseBill.Controls.Add(this.OriginLabel);
			this.GroupBoxHouseBill.Controls.Add(this.CA_HouseBillBoundTextBox);
			this.GroupBoxHouseBill.Controls.Add(this.CA_ShipmentStatusBoundTextBox);
			this.GroupBoxHouseBill.Controls.Add(this.HouseBillLabel);
			this.GroupBoxHouseBill.Dock = System.Windows.Forms.DockStyle.Fill;
			this.GroupBoxHouseBill.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.GroupBoxHouseBill.Name = "GroupBoxHouseBill";
			this.GroupBoxHouseBill.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(600, 275, true);
			this.GroupBoxHouseBill.TabIndex = 0;
			this.GroupBoxHouseBill.TabStop = false;
			this.GroupBoxHouseBill.Text = "House Bill";
			// 
			// DetailsButton
			// 
			this.DetailsButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.DetailsButton.IsCaptionOverridden = true;
			this.DetailsButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(546, 16, true);
			this.DetailsButton.Name = "DetailsButton";
			this.DetailsButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.DetailsButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(48, 24, true);
			this.DetailsButton.TabIndex = 2;
			this.DetailsButton.Text = "Details";
			this.DetailsButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.DetailsButton.ToolTipCaption = null;
			this.DetailsButton.Click += new System.EventHandler(this.DetailsButton_Click);
			// 
			// MessageStatusLabel
			// 
			this.MessageStatusLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.MessageStatusLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 52, true);
			this.MessageStatusLabel.Name = "MessageStatusLabel";
			this.MessageStatusLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(88, 23, true);
			this.MessageStatusLabel.TabIndex = 3;
			this.MessageStatusLabel.Text = "Message Status:";
			// 
			// CustomsStatusLabel
			// 
			this.CustomsStatusLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.CustomsStatusLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 17, true);
			this.CustomsStatusLabel.Name = "CustomsStatusLabel";
			this.CustomsStatusLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(88, 23, true);
			this.CustomsStatusLabel.TabIndex = 0;
			this.CustomsStatusLabel.Text = "Customs Status:";
			// 
			// MessageStatusTextBox
			// 
			this.MessageStatusTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.MessageStatusTextBox.BackColor = System.Drawing.SystemColors.Control;
			this.BindingSource.SetBindingMember(this.MessageStatusTextBox, "MessageStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).MessageStatus)));
			this.MessageStatusTextBox.CaptionResourceString = null;
			this.MessageStatusTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 54, true);
			this.MessageStatusTextBox.Multiline = true;
			this.MessageStatusTextBox.Name = "MessageStatusTextBox";
			this.MessageStatusTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(428, 20, true);
			this.MessageStatusTextBox.TabIndex = 4;
			this.MessageStatusTextBox.TabStop = false;
			// 
			// FreightForwarderIndicatorCheckBox
			// 
			this.BindingSource.SetBindingMember(this.FreightForwarderIndicatorCheckBox, "CA_IsMasterHouse");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).CA_IsMasterHouse)));
			this.FreightForwarderIndicatorCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.FreightForwarderIndicatorCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(296, 102, true);
			this.FreightForwarderIndicatorCheckBox.Name = "FreightForwarderIndicatorCheckBox";
			this.FreightForwarderIndicatorCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(248, 24, true);
			this.FreightForwarderIndicatorCheckBox.TabIndex = 9;
			this.FreightForwarderIndicatorCheckBox.Text = "Consolidation (FF Ind)";
			// 
			// ParentBillLabel
			// 
			this.ParentBillLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.ParentBillLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 102, true);
			this.ParentBillLabel.Name = "ParentBillLabel";
			this.ParentBillLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(88, 23, true);
			this.ParentBillLabel.TabIndex = 7;
			this.ParentBillLabel.Text = "Parent Bill:";
			// 
			// ParentBillTextBox
			// 
			this.BindingSource.SetBindingMember(this.ParentBillTextBox, "CA_MasterHouseBill");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).CA_MasterHouseBill)));
			this.ParentBillTextBox.CaptionResourceString = null;
			this.ParentBillTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 102, true);
			this.ParentBillTextBox.Name = "ParentBillTextBox";
			this.ParentBillTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 20, true);
			this.ParentBillTextBox.TabIndex = 8;
			// 
			// CA_RN_NKGoodsOriginBoundCodeFindBox
			// 
			this.CA_RN_NKGoodsOriginBoundCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CA_RN_NKGoodsOriginBoundCodeFindBox, "CA_RN_NKGoodsOrigin");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).CA_RN_NKGoodsOrigin)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).CountryOfOriginList)));
			this.CA_RN_NKGoodsOriginBoundCodeFindBox.BindToList = "CountryOfOriginList";
			this.CA_RN_NKGoodsOriginBoundCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 174, true);
			this.CA_RN_NKGoodsOriginBoundCodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefCountry;
			this.CA_RN_NKGoodsOriginBoundCodeFindBox.Name = "CA_RN_NKGoodsOriginBoundCodeFindBox";
			this.CA_RN_NKGoodsOriginBoundCodeFindBox.ShowDescriptionBox = false;
			this.CA_RN_NKGoodsOriginBoundCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			this.CA_RN_NKGoodsOriginBoundCodeFindBox.TabIndex = 15;
			// 
			// CA_RL_NK_PortOfDestinationBoundCodeFindBox
			// 
			this.CA_RL_NK_PortOfDestinationBoundCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CA_RL_NK_PortOfDestinationBoundCodeFindBox, "CA_RL_NK_PortOfDestination");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).CA_RL_NK_PortOfDestination)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).PortOfDestinationList)));
			this.CA_RL_NK_PortOfDestinationBoundCodeFindBox.BindToList = "PortOfDestinationList";
			this.CA_RL_NK_PortOfDestinationBoundCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 150, true);
			this.CA_RL_NK_PortOfDestinationBoundCodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefUNLOCO;
			this.CA_RL_NK_PortOfDestinationBoundCodeFindBox.Name = "CA_RL_NK_PortOfDestinationBoundCodeFindBox";
			this.CA_RL_NK_PortOfDestinationBoundCodeFindBox.ShowDescriptionBox = false;
			this.CA_RL_NK_PortOfDestinationBoundCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			this.CA_RL_NK_PortOfDestinationBoundCodeFindBox.TabIndex = 13;
			// 
			// CA_RL_NK_PortOfOriginBoundCodeFindBox
			// 
			this.CA_RL_NK_PortOfOriginBoundCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CA_RL_NK_PortOfOriginBoundCodeFindBox, "CA_RL_NK_PortOfOrigin");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).CA_RL_NK_PortOfOrigin)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).PortOfOriginList)));
			this.CA_RL_NK_PortOfOriginBoundCodeFindBox.BindToList = "PortOfOriginList";
			this.CA_RL_NK_PortOfOriginBoundCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 126, true);
			this.CA_RL_NK_PortOfOriginBoundCodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefUNLOCO;
			this.CA_RL_NK_PortOfOriginBoundCodeFindBox.Name = "CA_RL_NK_PortOfOriginBoundCodeFindBox";
			this.CA_RL_NK_PortOfOriginBoundCodeFindBox.ShowDescriptionBox = false;
			this.CA_RL_NK_PortOfOriginBoundCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			this.CA_RL_NK_PortOfOriginBoundCodeFindBox.TabIndex = 11;
			// 
			// CA_PrepaidCollectOtherBoundDropEdit
			// 
			this.CA_PrepaidCollectOtherBoundDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CA_PrepaidCollectOtherBoundDropEdit, "CA_PrepaidCollectOther");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).CA_PrepaidCollectOther)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).Lookups.MethodsOfPayment)));
			this.CA_PrepaidCollectOtherBoundDropEdit.BindToList = "Lookups+MethodsOfPayment";
			this.CA_PrepaidCollectOtherBoundDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 198, true);
			this.CA_PrepaidCollectOtherBoundDropEdit.Name = "CA_PrepaidCollectOtherBoundDropEdit";
			this.CA_PrepaidCollectOtherBoundDropEdit.PreBoundMaxLength = 3;
			this.CA_PrepaidCollectOtherBoundDropEdit.ShouldResizeByMaxLength = true;
			this.CA_PrepaidCollectOtherBoundDropEdit.ShowDescriptionBox = false;
			this.CA_PrepaidCollectOtherBoundDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
			this.CA_PrepaidCollectOtherBoundDropEdit.TabIndex = 17;
			// 
			// PaymentTypeLabel
			// 
			this.PaymentTypeLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.PaymentTypeLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 198, true);
			this.PaymentTypeLabel.Name = "PaymentTypeLabel";
			this.PaymentTypeLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(88, 23, true);
			this.PaymentTypeLabel.TabIndex = 16;
			this.PaymentTypeLabel.Text = "Payment Type:";
			// 
			// GoodsOriginLabel
			// 
			this.GoodsOriginLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.GoodsOriginLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 174, true);
			this.GoodsOriginLabel.Name = "GoodsOriginLabel";
			this.GoodsOriginLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(88, 23, true);
			this.GoodsOriginLabel.TabIndex = 14;
			this.GoodsOriginLabel.Text = "Goods Origin:";
			// 
			// DestinationLabel
			// 
			this.DestinationLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.DestinationLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 150, true);
			this.DestinationLabel.Name = "DestinationLabel";
			this.DestinationLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(88, 23, true);
			this.DestinationLabel.TabIndex = 12;
			this.DestinationLabel.Text = "Destination:";
			// 
			// OriginLabel
			// 
			this.OriginLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.OriginLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 126, true);
			this.OriginLabel.Name = "OriginLabel";
			this.OriginLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(88, 23, true);
			this.OriginLabel.TabIndex = 10;
			this.OriginLabel.Text = "Origin:";
			// 
			// CA_HouseBillBoundTextBox
			// 
			this.BindingSource.SetBindingMember(this.CA_HouseBillBoundTextBox, "CA_HouseBill");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).CA_HouseBill)));
			this.CA_HouseBillBoundTextBox.CaptionResourceString = null;
			this.CA_HouseBillBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 78, true);
			this.CA_HouseBillBoundTextBox.Name = "CA_HouseBillBoundTextBox";
			this.CA_HouseBillBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 20, true);
			this.CA_HouseBillBoundTextBox.TabIndex = 6;
			// 
			// CA_ShipmentStatusBoundTextBox
			// 
			this.CA_ShipmentStatusBoundTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.CA_ShipmentStatusBoundTextBox.BackColor = System.Drawing.SystemColors.Control;
			this.BindingSource.SetBindingMember(this.CA_ShipmentStatusBoundTextBox, "ShipmentStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.CusSCAHouse)(null)).ShipmentStatus)));
			this.CA_ShipmentStatusBoundTextBox.CaptionResourceString = null;
			this.CA_ShipmentStatusBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 16, true);
			this.CA_ShipmentStatusBoundTextBox.Multiline = true;
			this.CA_ShipmentStatusBoundTextBox.Name = "CA_ShipmentStatusBoundTextBox";
			this.CA_ShipmentStatusBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(428, 32, true);
			this.CA_ShipmentStatusBoundTextBox.TabIndex = 1;
			this.CA_ShipmentStatusBoundTextBox.TabStop = false;
			// 
			// HouseBillLabel
			// 
			this.HouseBillLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.HouseBillLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 78, true);
			this.HouseBillLabel.Name = "HouseBillLabel";
			this.HouseBillLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(88, 23, true);
			this.HouseBillLabel.TabIndex = 5;
			this.HouseBillLabel.Text = "House Bill:";
			// 
			// HouseBillDetailsUserControl
			// 
			this.Controls.Add(this.GroupBoxHouseBill);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(500, 275, true);
			this.Name = "HouseBillDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(600, 275, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.GroupBoxHouseBill.ResumeLayout(false);
			this.GroupBoxHouseBill.PerformLayout();
			this.CA_RN_NKGoodsOriginBoundCodeFindBox.ResumeLayout(true);
			this.CA_RN_NKGoodsOriginBoundCodeFindBox.PerformLayout();
			this.CA_RL_NK_PortOfDestinationBoundCodeFindBox.ResumeLayout(true);
			this.CA_RL_NK_PortOfDestinationBoundCodeFindBox.PerformLayout();
			this.CA_RL_NK_PortOfOriginBoundCodeFindBox.ResumeLayout(true);
			this.CA_RL_NK_PortOfOriginBoundCodeFindBox.PerformLayout();
			this.CA_PrepaidCollectOtherBoundDropEdit.ResumeLayout(true);
			this.CA_PrepaidCollectOtherBoundDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		Enterprise.ZArchitecture.GUI.ZGroupBox GroupBoxHouseBill;
		Enterprise.ZArchitecture.GUI.ZButton DetailsButton;
		Enterprise.ZArchitecture.ZLabel MessageStatusLabel;
		Enterprise.ZArchitecture.ZLabel CustomsStatusLabel;
		Enterprise.ZArchitecture.ZTextBox MessageStatusTextBox;
		Enterprise.ZArchitecture.GUI.ZCheckBox FreightForwarderIndicatorCheckBox;
		Enterprise.ZArchitecture.ZLabel ParentBillLabel;
		Enterprise.ZArchitecture.ZTextBox ParentBillTextBox;
		Enterprise.ZArchitecture.GUI.ZCodeFindBox CA_RN_NKGoodsOriginBoundCodeFindBox;
		Enterprise.ZArchitecture.GUI.ZCodeFindBox CA_RL_NK_PortOfDestinationBoundCodeFindBox;
		Enterprise.ZArchitecture.GUI.ZCodeFindBox CA_RL_NK_PortOfOriginBoundCodeFindBox;
		Enterprise.ZArchitecture.GUI.ZDropEdit CA_PrepaidCollectOtherBoundDropEdit;
		Enterprise.ZArchitecture.ZLabel PaymentTypeLabel;
		Enterprise.ZArchitecture.ZLabel GoodsOriginLabel;
		Enterprise.ZArchitecture.ZLabel DestinationLabel;
		Enterprise.ZArchitecture.ZLabel OriginLabel;
		Enterprise.ZArchitecture.ZTextBox CA_HouseBillBoundTextBox;
		Enterprise.ZArchitecture.ZTextBox CA_ShipmentStatusBoundTextBox;
		Enterprise.ZArchitecture.ZLabel HouseBillLabel;
	}
}
