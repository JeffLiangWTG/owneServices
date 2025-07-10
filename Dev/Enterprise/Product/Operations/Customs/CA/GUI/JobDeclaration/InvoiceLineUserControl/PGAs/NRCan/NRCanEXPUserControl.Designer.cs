namespace Enterprise.Customs.CA.GUI
{
	partial class NRCanEXPUserControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

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

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.detailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.netWeightCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.grossWeightCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.quantityCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.authorizedParty = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.UNDGGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.tradeNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.countryOfOriginFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.authorizedIDTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.intendedUseCodeCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.LpcoGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.LPCOGridUserControl = new Enterprise.Customs.CA.GUI.LPCOGridUserControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.detailsGroupBox.SuspendLayout();
			this.netWeightCalcDropEdit.SuspendLayout();
			this.grossWeightCalcDropEdit.SuspendLayout();
			this.quantityCalcDropEdit.SuspendLayout();
			this.authorizedParty.SuspendLayout();
			this.UNDGGuidFindBox.SuspendLayout();
			this.countryOfOriginFindBox.SuspendLayout();
			this.LpcoGroupBox.SuspendLayout();
			this.LPCOGridUserControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.CA.Business.NRCanPGAHeader);
			// 
			// detailsGroupBox
			// 
			this.detailsGroupBox.AutoSize = true;
			this.detailsGroupBox.Controls.Add(this.netWeightCalcDropEdit);
			this.detailsGroupBox.Controls.Add(this.grossWeightCalcDropEdit);
			this.detailsGroupBox.Controls.Add(this.quantityCalcDropEdit);
			this.detailsGroupBox.Controls.Add(this.authorizedParty);
			this.detailsGroupBox.Controls.Add(this.UNDGGuidFindBox);
			this.detailsGroupBox.Controls.Add(this.tradeNameTextBox);
			this.detailsGroupBox.Controls.Add(this.countryOfOriginFindBox);
			this.detailsGroupBox.Controls.Add(this.authorizedIDTextBox);
			this.detailsGroupBox.Controls.Add(this.intendedUseCodeCheckBox);
			this.detailsGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.detailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.detailsGroupBox.Name = "detailsGroupBox";
			this.detailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1935, 102, true);
			this.detailsGroupBox.TabIndex = 24;
			this.detailsGroupBox.TabStop = false;
			// 
			// netWeightCalcDropEdit
			// 
			this.netWeightCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.netWeightCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CA.Business.NRCanPGAHeader)(null)).InvoiceLine.JI_NetWeight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.NRCanPGAHeader)(null)).InvoiceLine.JI_NetWeightUQ)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.CA.Business.NRCanPGAHeader)(null)).InvoiceLine.Lookups.WeightUQList)));
			this.netWeightCalcDropEdit.BindToAmount = "InvoiceLine.JI_NetWeight";
			this.netWeightCalcDropEdit.BindToList = "InvoiceLine.Lookups.WeightUQList";
			this.netWeightCalcDropEdit.BindToUnit = "InvoiceLine.JI_NetWeightUQ";
			this.netWeightCalcDropEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("D03B6B6C-A7DF-4D45-8298-58BAC216F1EB", "Net Weight");
			this.netWeightCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(875, 14, true);
			this.netWeightCalcDropEdit.Name = "netWeightCalcDropEdit";
			this.netWeightCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(143, 18, true);
			this.netWeightCalcDropEdit.TabIndex = 6;
			// 
			// grossWeightCalcDropEdit
			// 
			this.grossWeightCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.grossWeightCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CA.Business.NRCanPGAHeader)(null)).InvoiceLine.JI_Weight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.NRCanPGAHeader)(null)).InvoiceLine.JI_WeightUQ)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.CA.Business.NRCanPGAHeader)(null)).InvoiceLine.Lookups.WeightUQList)));
			this.grossWeightCalcDropEdit.BindToAmount = "InvoiceLine.JI_Weight";
			this.grossWeightCalcDropEdit.BindToList = "InvoiceLine.Lookups.WeightUQList";
			this.grossWeightCalcDropEdit.BindToUnit = "InvoiceLine.JI_WeightUQ";
			this.grossWeightCalcDropEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("C3C64D7D-9AD4-436C-9B2D-200A0DC30CD9", "Gross Weight");
			this.grossWeightCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(875, 39, true);
			this.grossWeightCalcDropEdit.Name = "grossWeightCalcDropEdit";
			this.grossWeightCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(143, 18, true);
			this.grossWeightCalcDropEdit.TabIndex = 7;
			// 
			// quantityCalcDropEdit
			// 
			this.quantityCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.quantityCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CA.Business.NRCanPGAHeader)(null)).InvoiceLine.JI_InvoiceQuantity)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.NRCanPGAHeader)(null)).InvoiceLine.JI_InvoiceUQ)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.CA.Business.NRCanPGAHeader)(null)).InvoiceLine.Lookups.InvoiceUQList)));
			this.quantityCalcDropEdit.BindToAmount = "InvoiceLine.JI_InvoiceQuantity";
			this.quantityCalcDropEdit.BindToList = "InvoiceLine.Lookups.InvoiceUQList";
			this.quantityCalcDropEdit.BindToUnit = "InvoiceLine.JI_InvoiceUQ";
			this.quantityCalcDropEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("3FCD7B66-7F6A-42FE-8D5B-7B0649010D22", "Invoice Qty");
			this.quantityCalcDropEdit.Decimals = 3;
			this.quantityCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(875, 67, true);
			this.quantityCalcDropEdit.Name = "quantityCalcDropEdit";
			this.quantityCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(143, 18, true);
			this.quantityCalcDropEdit.TabIndex = 8;
			// 
			// authorizedParty
			// 
			this.authorizedParty.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.authorizedParty, "CA_AuthorizedParty");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CA.Business.NRCanPGAHeader)(null)).CA_AuthorizedParty)));
			this.authorizedParty.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("7BE53E5F-732C-4095-994F-D6467B5982F6", "MFG/Authorized Party");
			this.authorizedParty.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(483, 41, true);
			this.authorizedParty.Name = "authorizedParty";
			this.authorizedParty.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(292, 18, true);
			this.authorizedParty.TabIndex = 4;
			// 
			// UNDGGuidFindBox
			// 
			this.UNDGGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.UNDGGuidFindBox, "DangerousGoodsDGSubs");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.CA.Business.NRCanPGAHeader)(null)).DangerousGoodsDGSubs)));
			this.UNDGGuidFindBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("EDC5A97C-438F-47D9-93FA-A6777AF5D247", "UNDG");
			this.UNDGGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(483, 67, true);
			this.UNDGGuidFindBox.Name = "UNDGGuidFindBox";
			this.UNDGGuidFindBox.ShouldResize = true;
			this.UNDGGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(292, 18, true);
			this.UNDGGuidFindBox.TabIndex = 5;
			// 
			// tradeNameTextBox
			// 
			this.BindingSource.SetBindingMember(this.tradeNameTextBox, "InvoiceLine.CA_TradeName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.NRCanPGAHeader)(null)).InvoiceLine.CA_TradeName)));
			this.tradeNameTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("BFBBE054-96BC-4487-852F-2CF3D4C24119", "Trade Name");
			this.tradeNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(122, 40, true);
			this.tradeNameTextBox.Name = "tradeNameTextBox";
			this.tradeNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(230, 18, true);
			this.tradeNameTextBox.TabIndex = 1;
			// 
			// countryOfOriginFindBox
			// 
			this.countryOfOriginFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.countryOfOriginFindBox, "RN_NKCountryOfOrigin");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.NRCanPGAHeader)(null)).RN_NKCountryOfOrigin)));
			this.countryOfOriginFindBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("059F8005-9DF5-4357-A4E2-FA0D41675036", "Country/Region of Origin");
			this.countryOfOriginFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(483, 14, true);
			this.countryOfOriginFindBox.Name = "countryOfOriginFindBox";
			this.countryOfOriginFindBox.PreBoundMaxLength = 3;
			this.countryOfOriginFindBox.ShouldResize = true;
			this.countryOfOriginFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(292, 18, true);
			this.countryOfOriginFindBox.TabIndex = 3;
			// 
			// authorizedIDTextBox
			// 
			this.BindingSource.SetBindingMember(this.authorizedIDTextBox, "CA_AuthorizedProductID");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.NRCanPGAHeader)(null)).CA_AuthorizedProductID)));
			this.authorizedIDTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("FB9F11CD-330F-42FE-B97F-9095A5EEDDA9", "Authorized Product ID");
			this.authorizedIDTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(122, 67, true);
			this.authorizedIDTextBox.Name = "authorizedIDTextBox";
			this.authorizedIDTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(230, 18, true);
			this.authorizedIDTextBox.TabIndex = 2;
			// 
			// intendedUseCodeCheckBox
			// 
			this.intendedUseCodeCheckBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.intendedUseCodeCheckBox, "CA_IsNotRegulatedByExplosives");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.CA.Business.NRCanPGAHeader)(null)).CA_IsNotRegulatedByExplosives)));
			this.intendedUseCodeCheckBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("9252015C-AFEF-4689-806E-CE694A6BF542", "Excluded from Regulation");
			this.intendedUseCodeCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.intendedUseCodeCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(122, 16, true);
			this.intendedUseCodeCheckBox.Name = "intendedUseCodeCheckBox";
			this.intendedUseCodeCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(230, 17, true);
			this.intendedUseCodeCheckBox.TabIndex = 0;
			// 
			// LpcoGroupBox
			// 
			this.LpcoGroupBox.Controls.Add(this.LPCOGridUserControl);
			this.LpcoGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LpcoGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 102, true);
			this.LpcoGroupBox.Name = "LpcoGroupBox";
			this.LpcoGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1935, 810, true);
			this.LpcoGroupBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("653F101A-3E3A-4BEB-920D-99B44FDC71A1", "LPCOs");
			this.LpcoGroupBox.TabIndex = 25;
			this.LpcoGroupBox.TabStop = false;
			// 
			// LPCOGridUserControl
			// 
			this.LPCOGridUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.LPCOGridUserControl, "LPCOViews");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.CA.Business.LPCOViewCollection)(((Enterprise.Customs.CA.Business.NRCanPGAHeader)(null)).LPCOViews)));
			this.LPCOGridUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LPCOGridUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 14, true);
			this.LPCOGridUserControl.Name = "LPCOGridUserControl";
			this.LPCOGridUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1930, 793, true);
			this.LPCOGridUserControl.TabIndex = 1;
			// 
			// NRCanEXPUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.AutoSize = true;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.LpcoGroupBox);
			this.Controls.Add(this.detailsGroupBox);
			this.Name = "NRCanEXPUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1935, 911, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.detailsGroupBox.ResumeLayout(false);
			this.detailsGroupBox.PerformLayout();
			this.netWeightCalcDropEdit.ResumeLayout(true);
			this.netWeightCalcDropEdit.PerformLayout();
			this.grossWeightCalcDropEdit.ResumeLayout(true);
			this.grossWeightCalcDropEdit.PerformLayout();
			this.quantityCalcDropEdit.ResumeLayout(true);
			this.quantityCalcDropEdit.PerformLayout();
			this.authorizedParty.ResumeLayout(true);
			this.authorizedParty.PerformLayout();
			this.UNDGGuidFindBox.ResumeLayout(true);
			this.UNDGGuidFindBox.PerformLayout();
			this.countryOfOriginFindBox.ResumeLayout(true);
			this.countryOfOriginFindBox.PerformLayout();
			this.LpcoGroupBox.ResumeLayout(false);
			this.LpcoGroupBox.PerformLayout();
			this.LPCOGridUserControl.ResumeLayout(true);
			this.LPCOGridUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZTextBox tradeNameTextBox;
		private ZArchitecture.GUI.ZCodeFindBox countryOfOriginFindBox;
		protected ZArchitecture.ZTextBox authorizedIDTextBox;
		private ZArchitecture.GUI.ZGuidFindBox UNDGGuidFindBox;
		private ZArchitecture.GUI.ZCalcDropEdit netWeightCalcDropEdit;
		private ZArchitecture.GUI.ZCalcDropEdit grossWeightCalcDropEdit;
		private ZArchitecture.GUI.ZCalcDropEdit quantityCalcDropEdit;
		private ZArchitecture.GUI.ZDropEdit authorizedParty;
		private ZArchitecture.GUI.ZGroupBox detailsGroupBox;
		private ZArchitecture.GUI.ZCheckBox intendedUseCodeCheckBox;
		private ZArchitecture.GUI.ZGroupBox LpcoGroupBox;
		internal LPCOGridUserControl LPCOGridUserControl;
	}
}
