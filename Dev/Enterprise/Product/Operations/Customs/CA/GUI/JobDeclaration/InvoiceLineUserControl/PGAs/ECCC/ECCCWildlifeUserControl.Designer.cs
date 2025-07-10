namespace Enterprise.Customs.CA.GUI
{
	partial class ECCCWildlifeUserControl
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
		void InitializeComponent()
		{
			this.DetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.AphiaIDTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.TitleLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ScientificNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CompStatCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.CountryCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.SourceDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.TSNTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.IntendedUseCodeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.SexDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.LifeStageDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.AgeCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.ModelTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.LPCOGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.LPCOGridUserControl = new Enterprise.Customs.CA.GUI.LPCOGridUserControl();
			this.IdentitiesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.IDUserControl = new Enterprise.Customs.CA.GUI.ComponentUserControl();
			this.BottomPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.TopPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.DetailsGroupBox.SuspendLayout();
			this.CountryCodeFindBox.SuspendLayout();
			this.SourceDropEdit.SuspendLayout();
			this.IntendedUseCodeDropEdit.SuspendLayout();
			this.SexDropEdit.SuspendLayout();
			this.LifeStageDropEdit.SuspendLayout();
			this.LPCOGroupBox.SuspendLayout();
			this.LPCOGridUserControl.SuspendLayout();
			this.IdentitiesGroupBox.SuspendLayout();
			this.IDUserControl.SuspendLayout();
			this.BottomPanel.SuspendLayout();
			this.TopPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.CA.Business.ECCCPGAHeader);
			// 
			// DetailsGroupBox
			// 
			this.DetailsGroupBox.AutoSize = true;
			this.DetailsGroupBox.Controls.Add(this.AphiaIDTextBox);
			this.DetailsGroupBox.Controls.Add(this.TitleLabel);
			this.DetailsGroupBox.Controls.Add(this.ScientificNameTextBox);
			this.DetailsGroupBox.Controls.Add(this.CompStatCheckBox);
			this.DetailsGroupBox.Controls.Add(this.CountryCodeFindBox);
			this.DetailsGroupBox.Controls.Add(this.SourceDropEdit);
			this.DetailsGroupBox.Controls.Add(this.TSNTextBox);
			this.DetailsGroupBox.Controls.Add(this.IntendedUseCodeDropEdit);
			this.DetailsGroupBox.Controls.Add(this.SexDropEdit);
			this.DetailsGroupBox.Controls.Add(this.LifeStageDropEdit);
			this.DetailsGroupBox.Controls.Add(this.AgeCalcEdit);
			this.DetailsGroupBox.Controls.Add(this.ModelTextBox);
			this.DetailsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DetailsGroupBox.Name = "DetailsGroupBox";
			this.DetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1080, 181, true);
			this.DetailsGroupBox.TabIndex = 0;
			this.DetailsGroupBox.TabStop = false;
			this.DetailsGroupBox.Text = "Details";
			// 
			// AphiaIDTextBox
			// 
			this.BindingSource.SetBindingMember(this.AphiaIDTextBox, "CA_AphiaID");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.ECCCPGAHeader)(null)).CA_AphiaID)));
			this.AphiaIDTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("0b326c85-eb0e-4944-8a86-cca3012b8cbb", "AphiaID");
			this.AphiaIDTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(114, 123, true);
			this.AphiaIDTextBox.Name = "AphiaIDTextBox";
			this.AphiaIDTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(239, 20, true);
			this.AphiaIDTextBox.TabIndex = 4;
			// 
			// TitleLabel
			// 
			this.TitleLabel.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("c8fc1806-116e-4bc3-9bad-1600d9a958c6", "These goods are subject to the Convention on International Trade in Endangered Species of Wild Fauna and Flora (CITES) as regulated in Canada under the Wild Animal and Plant Protection and Regulation of International and Interprovincial Trade Act (WAPPRIITA) or are listed in Schedule II of the Wild Animal and Plant Trade Regulations (WAPTR).");
			this.TitleLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.TitleLabel.IsFontBold = true;
			this.TitleLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(378, 95, true);
			this.TitleLabel.Name = "TitleLabel";
			this.TitleLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(591, 59, true);
			this.TitleLabel.TabIndex = 11;
			this.TitleLabel.TextAlign = System.Drawing.ContentAlignment.TopLeft;
			// 
			// ScientificNameTextBox
			// 
			this.BindingSource.SetBindingMember(this.ScientificNameTextBox, "CA_ScientificName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.ECCCPGAHeader)(null)).CA_ScientificName)));
			this.ScientificNameTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("030569cb-8af8-4d38-9ccb-432bccac1ad6", "Scientific Name");
			this.ScientificNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(114, 71, true);
			this.ScientificNameTextBox.Name = "ScientificNameTextBox";
			this.ScientificNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(239, 20, true);
			this.ScientificNameTextBox.TabIndex = 2;
			// 
			// CompStatCheckBox
			// 
			this.CompStatCheckBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CompStatCheckBox, "CA_ComplianceDeclaration");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.CA.Business.ECCCPGAHeader)(null)).CA_ComplianceDeclaration)));
			this.CompStatCheckBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("6eb1c18d-fc6d-4453-9027-cef421afe994", "ECCC Regulated");
			this.CompStatCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.CompStatCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.CompStatCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(450, 71, true);
			this.CompStatCheckBox.Name = "CompStatCheckBox";
			this.CompStatCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(239, 20, true);
			this.CompStatCheckBox.TabIndex = 7;
			// 
			// CountryCodeFindBox
			// 
			this.CountryCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CountryCodeFindBox, "InvoiceLine.CA_RN_NKSource");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.ECCCPGAHeader)(null)).InvoiceLine.CA_RN_NKSource)));
			this.CountryCodeFindBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("4e28e3a6-54f7-4ffd-8c54-7e3b0c3d4251", "Country/Region of Source");
			this.CountryCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(488, 45, true);
			this.CountryCodeFindBox.Name = "CountryCodeFindBox";
			this.CountryCodeFindBox.ShouldResize = true;
			this.CountryCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(211, 20, true);
			this.CountryCodeFindBox.TabIndex = 6;
			// 
			// SourceDropEdit
			// 
			this.SourceDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SourceDropEdit, "CA_SourceOfSpecimen");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CA.Business.ECCCPGAHeader)(null)).CA_SourceOfSpecimen)));
			this.SourceDropEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("d5d1622b-a7bb-4bf0-b42a-e22f2d8a33cc", "Source of Specimen");
			this.SourceDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(488, 19, true);
			this.SourceDropEdit.Name = "SourceDropEdit";
			this.SourceDropEdit.ShouldResizeByMaxLength = true;
			this.SourceDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(211, 20, true);
			this.SourceDropEdit.TabIndex = 5;
			// 
			// TSNTextBox
			// 
			this.BindingSource.SetBindingMember(this.TSNTextBox, "CA_TSN");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.ECCCPGAHeader)(null)).CA_TSN)));
			this.TSNTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("55b015ff-fdf5-48b9-887f-35e6db266d8c", "TSN");
			this.TSNTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(114, 97, true);
			this.TSNTextBox.Name = "TSNTextBox";
			this.TSNTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(239, 20, true);
			this.TSNTextBox.TabIndex = 3;
			// 
			// IntendedUseCodeDropEdit
			// 
			this.IntendedUseCodeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.IntendedUseCodeDropEdit, "CA_IntendedUseCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CA.Business.ECCCPGAHeader)(null)).CA_IntendedUseCode)));
			this.IntendedUseCodeDropEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("fb10bc25-5dac-4acb-95d6-5aaffd6da1e4", "Import Reason");
			this.IntendedUseCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(114, 19, true);
			this.IntendedUseCodeDropEdit.Name = "IntendedUseCodeDropEdit";
			this.IntendedUseCodeDropEdit.ShouldResizeByMaxLength = true;
			this.IntendedUseCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(239, 20, true);
			this.IntendedUseCodeDropEdit.TabIndex = 0;
			// 
			// SexDropEdit
			// 
			this.SexDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SexDropEdit, "CA_Sex");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CA.Business.ECCCPGAHeader)(null)).CA_Sex)));
			this.SexDropEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("19799f1b-7846-4bb2-a399-be7dca73334b", "Sex");
			this.SexDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(856, 45, true);
			this.SexDropEdit.Name = "SexDropEdit";
			this.SexDropEdit.ShouldResizeByMaxLength = true;
			this.SexDropEdit.ShowDescriptionBox = false;
			this.SexDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.SexDropEdit.TabIndex = 10;
			// 
			// LifeStageDropEdit
			// 
			this.LifeStageDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.LifeStageDropEdit, "CA_LifeStage");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CA.Business.ECCCPGAHeader)(null)).CA_LifeStage)));
			this.LifeStageDropEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("892b8f68-c2fd-4ed7-9d12-0a8893700296", "Life Stage");
			this.LifeStageDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(772, 19, true);
			this.LifeStageDropEdit.Name = "LifeStageDropEdit";
			this.LifeStageDropEdit.ShouldResizeByMaxLength = true;
			this.LifeStageDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(197, 20, true);
			this.LifeStageDropEdit.TabIndex = 8;
			// 
			// AgeCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.AgeCalcEdit, "CA_Age");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CA.Business.ECCCPGAHeader)(null)).CA_Age)));
			this.AgeCalcEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("ff786306-30ed-498e-85f2-6d7ff3d5cbc6", "Age");
			this.AgeCalcEdit.DecimalPlaces = 2;
			this.AgeCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(772, 45, true);
			this.AgeCalcEdit.Name = "AgeCalcEdit";
			this.AgeCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 20, true);
			this.AgeCalcEdit.TabIndex = 9;
			this.AgeCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// ModelTextBox
			// 
			this.BindingSource.SetBindingMember(this.ModelTextBox, "InvoiceLine.JI_Model");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.ECCCPGAHeader)(null)).InvoiceLine.JI_Model)));
			this.ModelTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("26aa6761-2ef9-41be-8577-3ff18fc0e65d", "Common Name");
			this.ModelTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(114, 45, true);
			this.ModelTextBox.Name = "ModelTextBox";
			this.ModelTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(239, 20, true);
			this.ModelTextBox.TabIndex = 1;
			// 
			// LPCOGroupBox
			// 
			this.LPCOGroupBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("ede6044b-7563-45a7-84f8-bd9d46a552ec", "LPCOs");
			this.LPCOGroupBox.Controls.Add(this.LPCOGridUserControl);
			this.LPCOGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LPCOGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(393, 0, true);
			this.LPCOGroupBox.Name = "LPCOGroupBox";
			this.LPCOGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(687, 219, true);
			this.LPCOGroupBox.TabIndex = 1;
			this.LPCOGroupBox.TabStop = false;
			this.LPCOGroupBox.Text = "LPCOs";
			// 
			// LPCOGridUserControl
			// 
			this.LPCOGridUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.LPCOGridUserControl, "LPCOViews");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.CA.Business.LPCOViewCollection)(((Enterprise.Customs.CA.Business.ECCCPGAHeader)(null)).LPCOViews)));
			this.LPCOGridUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LPCOGridUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.LPCOGridUserControl.Name = "LPCOGridUserControl";
			this.LPCOGridUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(681, 200, true);
			this.LPCOGridUserControl.TabIndex = 0;
			// 
			// IdentitiesGroupBox
			// 
			this.IdentitiesGroupBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("c1639d4c-f5bd-42bf-8df6-06ad5c93786e", "Identities");
			this.IdentitiesGroupBox.Controls.Add(this.IDUserControl);
			this.IdentitiesGroupBox.Dock = System.Windows.Forms.DockStyle.Left;
			this.IdentitiesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.IdentitiesGroupBox.Name = "IdentitiesGroupBox";
			this.IdentitiesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(393, 219, true);
			this.IdentitiesGroupBox.TabIndex = 0;
			this.IdentitiesGroupBox.TabStop = false;
			this.IdentitiesGroupBox.Text = "Identities";
			// 
			// IDUserControl
			// 
			this.IDUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.IDUserControl, "Components");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.CA.Business.ComponentCollection)(((Enterprise.Customs.CA.Business.ECCCPGAHeader)(null)).Components)));
			this.IDUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.IDUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.IDUserControl.Name = "IDUserControl";
			this.IDUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(387, 200, true);
			this.IDUserControl.TabIndex = 0;
			// 
			// BottomPanel
			// 
			this.BottomPanel.Controls.Add(this.LPCOGroupBox);
			this.BottomPanel.Controls.Add(this.IdentitiesGroupBox);
			this.BottomPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.BottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 181, true);
			this.BottomPanel.Name = "BottomPanel";
			this.BottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1080, 219, true);
			this.BottomPanel.TabIndex = 1;
			// 
			// TopPanel
			// 
			this.TopPanel.AutoScroll = true;
			this.TopPanel.AutoScrollMinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(978, 0, true);
			this.TopPanel.Controls.Add(this.DetailsGroupBox);
			this.TopPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.TopPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TopPanel.Name = "TopPanel";
			this.TopPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1080, 181, true);
			this.TopPanel.TabIndex = 0;
			// 
			// ECCCWildlifeUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.AutoScroll = true;
			this.AutoScrollMinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1080, 400, true);
			this.AutoSize = true;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.BottomPanel);
			this.Controls.Add(this.TopPanel);
			this.Name = "ECCCWildlifeUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(944, 253, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.DetailsGroupBox.ResumeLayout(false);
			this.DetailsGroupBox.PerformLayout();
			this.CountryCodeFindBox.ResumeLayout(true);
			this.CountryCodeFindBox.PerformLayout();
			this.SourceDropEdit.ResumeLayout(true);
			this.SourceDropEdit.PerformLayout();
			this.IntendedUseCodeDropEdit.ResumeLayout(true);
			this.IntendedUseCodeDropEdit.PerformLayout();
			this.SexDropEdit.ResumeLayout(true);
			this.SexDropEdit.PerformLayout();
			this.LifeStageDropEdit.ResumeLayout(true);
			this.LifeStageDropEdit.PerformLayout();
			this.LPCOGroupBox.ResumeLayout(false);
			this.LPCOGroupBox.PerformLayout();
			this.LPCOGridUserControl.ResumeLayout(true);
			this.LPCOGridUserControl.PerformLayout();
			this.IdentitiesGroupBox.ResumeLayout(false);
			this.IdentitiesGroupBox.PerformLayout();
			this.IDUserControl.ResumeLayout(true);
			this.IDUserControl.PerformLayout();
			this.BottomPanel.ResumeLayout(false);
			this.BottomPanel.PerformLayout();
			this.TopPanel.ResumeLayout(false);
			this.TopPanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		ZArchitecture.ZTextBox ModelTextBox;
		ZArchitecture.ZCalcEdit AgeCalcEdit;
		ZArchitecture.GUI.ZDropEdit LifeStageDropEdit;
		ZArchitecture.GUI.ZDropEdit SexDropEdit;
		ZArchitecture.GUI.ZDropEdit IntendedUseCodeDropEdit;
		ZArchitecture.ZTextBox TSNTextBox;
		ZArchitecture.GUI.ZDropEdit SourceDropEdit;
		ZArchitecture.GUI.ZCheckBox CompStatCheckBox;
		ZArchitecture.GUI.ZCodeFindBox CountryCodeFindBox;
		ZArchitecture.ZTextBox ScientificNameTextBox;
		ZArchitecture.ZLabel TitleLabel;
		ZArchitecture.GUI.ZGroupBox DetailsGroupBox;
		ZArchitecture.ZTextBox AphiaIDTextBox;
		ZArchitecture.GUI.ZGroupBox LPCOGroupBox;
		LPCOGridUserControl LPCOGridUserControl;
		ZArchitecture.GUI.ZGroupBox IdentitiesGroupBox;
		ComponentUserControl IDUserControl;
		ZArchitecture.GUI.ZPanel BottomPanel;
		ZArchitecture.GUI.ZPanel TopPanel;
	}
}
