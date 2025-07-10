namespace Enterprise.Customs.CA.GUI
{
	partial class GACUserControl
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
			this.DetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ClothingAndTextileDetailsGroup = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.CA_FTACodeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.CA_FibreCountryOfOriginCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.CA_FabricCountryOfOriginCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.CA_YarnCountryOfOriginCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.CommodityCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.TitleLabel = new Enterprise.ZArchitecture.ZLabel();
			this.TPLPermitLabel = new Enterprise.ZArchitecture.ZLabel();
			this.CA_ComplianceStatementCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.LPCOsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.LPCODetailUserControl = new Enterprise.Customs.CA.GUI.LPCODetailUserControl();
			this.LPCOGridUserControl = new Enterprise.Customs.CA.GUI.LPCOGridUserControl();
			this.GACSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.DetailsGroupBox.SuspendLayout();
			this.ClothingAndTextileDetailsGroup.SuspendLayout();
			this.CA_FTACodeDropEdit.SuspendLayout();
			this.CA_FibreCountryOfOriginCodeFindBox.SuspendLayout();
			this.CA_FabricCountryOfOriginCodeFindBox.SuspendLayout();
			this.CA_YarnCountryOfOriginCodeFindBox.SuspendLayout();
			this.LPCOsGroupBox.SuspendLayout();
			this.LPCODetailUserControl.SuspendLayout();
			this.LPCOGridUserControl.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.GACSplitContainer)).BeginInit();
			this.GACSplitContainer.Panel1.SuspendLayout();
			this.GACSplitContainer.Panel2.SuspendLayout();
			this.GACSplitContainer.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.CA.Business.GACPGAHeader);
			// 
			// DetailsGroupBox
			// 
			this.DetailsGroupBox.Controls.Add(this.ClothingAndTextileDetailsGroup);
			this.DetailsGroupBox.Controls.Add(this.CommodityCodeTextBox);
			this.DetailsGroupBox.Controls.Add(this.TitleLabel);
			this.DetailsGroupBox.Controls.Add(this.CA_ComplianceStatementCheckBox);
			this.DetailsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DetailsGroupBox.Name = "DetailsGroupBox";
			this.DetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1256, 130, true);
			this.DetailsGroupBox.TabIndex = 0;
			this.DetailsGroupBox.TabStop = false;
			// 
			// ClothingAndTextileDetailsGroup
			// 
			this.ClothingAndTextileDetailsGroup.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("5f66e44e-490a-4677-8231-f68968196e7d", "Clothing and Textile Details");
			this.ClothingAndTextileDetailsGroup.Controls.Add(this.CA_FTACodeDropEdit);
			this.ClothingAndTextileDetailsGroup.Controls.Add(this.CA_FibreCountryOfOriginCodeFindBox);
			this.ClothingAndTextileDetailsGroup.Controls.Add(this.CA_FabricCountryOfOriginCodeFindBox);
			this.ClothingAndTextileDetailsGroup.Controls.Add(this.CA_YarnCountryOfOriginCodeFindBox);
			this.ClothingAndTextileDetailsGroup.Controls.Add(this.TPLPermitLabel);
			this.ClothingAndTextileDetailsGroup.Dock = System.Windows.Forms.DockStyle.Right;
			this.ClothingAndTextileDetailsGroup.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(955, 12, true);
			this.ClothingAndTextileDetailsGroup.Name = "ClothingAndTextileDetailsGroup";
			this.ClothingAndTextileDetailsGroup.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 170, true);
			this.ClothingAndTextileDetailsGroup.TabIndex = 2;
			this.ClothingAndTextileDetailsGroup.TabStop = false;
			// 
			// CA_FTACodeDropEdit
			// 
			this.CA_FTACodeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CA_FTACodeDropEdit, "CA_FTACode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CA.Business.GACPGAHeader)(null)).CA_FTACode)));
			this.CA_FTACodeDropEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("89ac0537-ec9a-49bb-a09f-14e5982164b8", "FTA Processing Code");
			this.CA_FTACodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(114, 19, true);
			this.CA_FTACodeDropEdit.Name = "CA_FTACodeDropEdit";
			this.CA_FTACodeDropEdit.ShouldResizeByMaxLength = true;
			this.CA_FTACodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(174, 20, true);
			this.CA_FTACodeDropEdit.TabIndex = 0;
			// 
			// CA_FibreCountryOfOriginCodeFindBox
			// 
			this.CA_FibreCountryOfOriginCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CA_FibreCountryOfOriginCodeFindBox, "CA_FibreCountryOfOrigin");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.GACPGAHeader)(null)).CA_FibreCountryOfOrigin)));
			this.CA_FibreCountryOfOriginCodeFindBox.BindToForDescription = "FibreCODescription";
			this.CA_FibreCountryOfOriginCodeFindBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("138a0a98-edd8-4b4e-a2f5-b1f73d50fe83", "Fibre C/O");
			this.CA_FibreCountryOfOriginCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(114, 42, true);
			this.CA_FibreCountryOfOriginCodeFindBox.Name = "CA_FibreCountryOfOriginCodeFindBox";
			this.CA_FibreCountryOfOriginCodeFindBox.ShouldResize = true;
			this.CA_FibreCountryOfOriginCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(174, 20, true);
			this.CA_FibreCountryOfOriginCodeFindBox.TabIndex = 1;
			// 
			// CA_FabricCountryOfOriginCodeFindBox
			// 
			this.CA_FabricCountryOfOriginCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CA_FabricCountryOfOriginCodeFindBox, "CA_FabricCountryOfOrigin");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.GACPGAHeader)(null)).CA_FabricCountryOfOrigin)));
			this.CA_FabricCountryOfOriginCodeFindBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("e42f2f5f-05cb-42ed-b47e-8427aa3cca6f", "Fabric C/O");
			this.CA_FabricCountryOfOriginCodeFindBox.BindToForDescription = "FabricCODescription";
			this.CA_FabricCountryOfOriginCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(114, 88, true);
			this.CA_FabricCountryOfOriginCodeFindBox.Name = "CA_FabricCountryOfOriginCodeFindBox";
			this.CA_FabricCountryOfOriginCodeFindBox.ShouldResize = true;
			this.CA_FabricCountryOfOriginCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(174, 20, true);
			this.CA_FabricCountryOfOriginCodeFindBox.TabIndex = 3;
			// 
			// CA_YarnCountryOfOriginCodeFindBox
			// 
			this.CA_YarnCountryOfOriginCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CA_YarnCountryOfOriginCodeFindBox, "CA_YarnCountryOfOrigin");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.GACPGAHeader)(null)).CA_YarnCountryOfOrigin)));
			this.CA_YarnCountryOfOriginCodeFindBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("aa23c744-736d-4ce1-90c0-52b803840cb2", "Yarn C/O");
			this.CA_YarnCountryOfOriginCodeFindBox.BindToForDescription = "YarnCODescription";
			this.CA_YarnCountryOfOriginCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(114, 65, true);
			this.CA_YarnCountryOfOriginCodeFindBox.Name = "CA_YarnCountryOfOriginCodeFindBox";
			this.CA_YarnCountryOfOriginCodeFindBox.ShouldResize = true;
			this.CA_YarnCountryOfOriginCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(174, 20, true);
			this.CA_YarnCountryOfOriginCodeFindBox.TabIndex = 2;
			// 
			// TPLPermitLabel
			// 
			this.TPLPermitLabel.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("40379510-D8D4-4389-8782-2B0C0B42821B", @"Use Code ""3C"" if TPL Permit Application is for a 3rd party country/region.");
			this.TPLPermitLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.TPLPermitLabel.IsFontBold = true;
			this.TPLPermitLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 111, true);
			this.TPLPermitLabel.Name = "TPLPermitLabel";
			this.TPLPermitLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(285, 42, true);
			this.TPLPermitLabel.TextAlign = System.Drawing.ContentAlignment.TopLeft;
			// 
			// CommodityCodeTextBox
			// 
			this.BindingSource.SetBindingMember(this.CommodityCodeTextBox, "CA_CommodityCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.GACPGAHeader)(null)).CA_CommodityCode)));
			this.CommodityCodeTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("422360e3-1b48-4371-bafe-ddbd2206fd98", "Commodity Code");
			this.CommodityCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(93, 12, true);
			this.CommodityCodeTextBox.Name = "CommodityCodeTextBox";
			this.CommodityCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 20, true);
			this.CommodityCodeTextBox.TabIndex = 0;
			// 
			// TitleLabel
			// 
			this.TitleLabel.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("be7fea64-a325-42e3-baac-f98e98082872", "The importer of this commodity certifies that they are a resident of Canada, that is, either in the case of a natural person, a person who ordinarily resides in Canada and, in the case of a corporation, a corporation having its head office in Canada or operating a branch office in Canada.");
			this.TitleLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.TitleLabel.IsFontBold = true;
			this.TitleLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(299, 12, true);
			this.TitleLabel.Name = "TitleLabel";
			this.TitleLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(260, 62, true);
			this.TitleLabel.TabIndex = 3;
			this.TitleLabel.TextAlign = System.Drawing.ContentAlignment.TopLeft;
			// 
			// CA_ComplianceStatementCheckBox
			// 
			this.CA_ComplianceStatementCheckBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CA_ComplianceStatementCheckBox, "CA_ComplianceStatement");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.CA.Business.GACPGAHeader)(null)).CA_ComplianceStatement)));
			this.CA_ComplianceStatementCheckBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("c00c7865-9326-409e-bece-d8d70dfd22f9", "Certify");
			this.CA_ComplianceStatementCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.CA_ComplianceStatementCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.CA_ComplianceStatementCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(38, 38, true);
			this.CA_ComplianceStatementCheckBox.Name = "CA_ComplianceStatementCheckBox";
			this.CA_ComplianceStatementCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(69, 20, true);
			this.CA_ComplianceStatementCheckBox.TabIndex = 1;
			// 
			// LPCOsGroupBox
			// 
			this.LPCOsGroupBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("afad4491-2c98-4442-84f5-7479f5a679cd", "LPCOs");
			this.LPCOsGroupBox.Controls.Add(this.LPCODetailUserControl);
			this.LPCOsGroupBox.Controls.Add(this.LPCOGridUserControl);
			this.LPCOsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LPCOsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.LPCOsGroupBox.Name = "LPCOsGroupBox";
			this.LPCOsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1256, 361, true);
			this.LPCOsGroupBox.TabIndex = 1;
			this.LPCOsGroupBox.TabStop = false;
			// 
			// LPCODetailUserControl
			// 
			this.LPCODetailUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.LPCODetailUserControl, "LPCOViews");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.CA.Business.LPCOViewCollection)(((Enterprise.Customs.CA.Business.GACPGAHeader)(null)).LPCOViews)));
			this.LPCODetailUserControl.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.LPCODetailUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 206, true);
			this.LPCODetailUserControl.Name = "LPCODetailUserControl";
			this.LPCODetailUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1250, 152, true);
			this.LPCODetailUserControl.TabIndex = 2;
			// 
			// LPCOGridUserControl
			// 
			this.LPCOGridUserControl.AllowDrop = true;
			this.LPCOGridUserControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
			| System.Windows.Forms.AnchorStyles.Left) 
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.LPCOGridUserControl, "LPCOViews");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.CA.Business.LPCOViewCollection)(((Enterprise.Customs.CA.Business.GACPGAHeader)(null)).LPCOViews)));
			this.LPCOGridUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.LPCOGridUserControl.Name = "LPCOGridUserControl";
			this.LPCOGridUserControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1250, 54, true);
			this.LPCOGridUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1250, 184, true);
			this.LPCOGridUserControl.TabIndex = 1;
			// 
			// GACSplitContainer
			// 
			this.GACSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.GACSplitContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
			this.GACSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.GACSplitContainer.Name = "GACSplitContainer";
			this.GACSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// GACSplitContainer.Panel1
			// 
			this.GACSplitContainer.Panel1.Controls.Add(this.DetailsGroupBox);
			this.GACSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1205, 525, true);
			this.GACSplitContainer.Panel1MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(175);
			// 
			// GACSplitContainer.Panel2
			// 
			this.GACSplitContainer.Panel2.Controls.Add(this.LPCOsGroupBox);
			this.GACSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1256, 495, true);
			this.GACSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(130);
			this.GACSplitContainer.TabIndex = 2;
			// 
			// GACUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.AutoScroll = true;
			this.AutoScrollMinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1080, 525, true);
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.GACSplitContainer);
			this.Name = "GACUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1256, 495, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.DetailsGroupBox.ResumeLayout(false);
			this.DetailsGroupBox.PerformLayout();
			this.ClothingAndTextileDetailsGroup.ResumeLayout(false);
			this.ClothingAndTextileDetailsGroup.PerformLayout();
			this.CA_FTACodeDropEdit.ResumeLayout(true);
			this.CA_FTACodeDropEdit.PerformLayout();
			this.CA_FibreCountryOfOriginCodeFindBox.ResumeLayout(true);
			this.CA_FibreCountryOfOriginCodeFindBox.PerformLayout();
			this.CA_FabricCountryOfOriginCodeFindBox.ResumeLayout(true);
			this.CA_FabricCountryOfOriginCodeFindBox.PerformLayout();
			this.CA_YarnCountryOfOriginCodeFindBox.ResumeLayout(true);
			this.CA_YarnCountryOfOriginCodeFindBox.PerformLayout();
			this.LPCOsGroupBox.ResumeLayout(false);
			this.LPCOsGroupBox.PerformLayout();
			this.LPCODetailUserControl.ResumeLayout(true);
			this.LPCODetailUserControl.PerformLayout();
			this.LPCOGridUserControl.ResumeLayout(true);
			this.LPCOGridUserControl.PerformLayout();
			this.GACSplitContainer.Panel1.ResumeLayout(false);
			this.GACSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.GACSplitContainer)).EndInit();
			this.GACSplitContainer.ResumeLayout(false);
			this.GACSplitContainer.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox DetailsGroupBox;
		private ZArchitecture.GUI.ZDropEdit CA_FTACodeDropEdit;
		private ZArchitecture.GUI.ZCheckBox CA_ComplianceStatementCheckBox;
		private ZArchitecture.ZLabel TitleLabel;
		internal ZArchitecture.ZLabel TPLPermitLabel;
		private ZArchitecture.GUI.ZCodeFindBox CA_FibreCountryOfOriginCodeFindBox;
		private ZArchitecture.GUI.ZCodeFindBox CA_YarnCountryOfOriginCodeFindBox;
		private ZArchitecture.GUI.ZCodeFindBox CA_FabricCountryOfOriginCodeFindBox;
		private ZArchitecture.GUI.ZGroupBox LPCOsGroupBox;
		internal LPCOGridUserControl LPCOGridUserControl;
		private CargoWise.Windows.UI.KSplitContainer GACSplitContainer;
		private ZArchitecture.ZTextBox CommodityCodeTextBox;
		internal Enterprise.ZArchitecture.GUI.ZGroupBox ClothingAndTextileDetailsGroup;
		private LPCODetailUserControl LPCODetailUserControl;
	}
}
