namespace Enterprise.Customs.EU.GUI.SingleLineEntry
{
	partial class SingleLineEntryForm
	{
		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		protected override void InitializeComponent()
		{
			this.OKButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CancelZButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.PriceCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.DetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.CheckBoxLic99 = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.zLabel2 = new Enterprise.ZArchitecture.ZLabel();
			this.NetWeightCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.zGuidFindBox1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.InvoiceNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CPCFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.zLabel1 = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.DetailsGroupBox.SuspendLayout();
			this.zGuidFindBox1.SuspendLayout();
			this.CPCFindBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 291, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(453, 24, true);
			this.MainStatusBar.TabIndex = 4;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.Business.Declaration.SingleLineEntry);
			// 
			// OKButton
			// 
			this.OKButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.OKButton.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("SingleLineEntryForm|51d7c433-463b-4a0f-83b0-6a64d3990b37", "&OK");
			this.OKButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.OKButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(285, 252, true);
			this.OKButton.Name = "OKButton";
			this.OKButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.OKButton.TabIndex = 2;
			this.OKButton.ToolTipCaption = null;
			this.OKButton.UseVisualStyleBackColor = true;
			this.OKButton.Click += new System.EventHandler(this.zButton1_Click);
			// 
			// CancelZButton
			// 
			this.CancelZButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CancelZButton.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("SingleLineEntryForm|9dbb0405-7112-4b51-9611-fbd1b08cd156", "&Cancel");
			this.CancelZButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CancelZButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(366, 252, true);
			this.CancelZButton.Name = "CancelZButton";
			this.CancelZButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.CancelZButton.TabIndex = 3;
			this.CancelZButton.ToolTipCaption = null;
			this.CancelZButton.UseVisualStyleBackColor = true;
			// 
			// PriceCalcEdit
			// 
			this.PriceCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.PriceCalcEdit, "Price");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.Business.Declaration.SingleLineEntry)(null)).Price)));
			this.PriceCalcEdit.CaptionResourceString = null;
			this.PriceCalcEdit.DecimalPlaces = 2;
			this.PriceCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(136, 88, true);
			this.PriceCalcEdit.Name = "PriceCalcEdit";
			this.PriceCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.PriceCalcEdit.TabIndex = 3;
			this.PriceCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// DetailsGroupBox
			// 
			this.DetailsGroupBox.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("SingleLineEntryForm|baf4c1ba-076a-4240-be87-9e46d44f215b", "Details");
			this.DetailsGroupBox.Controls.Add(this.CheckBoxLic99);
			this.DetailsGroupBox.Controls.Add(this.zLabel2);
			this.DetailsGroupBox.Controls.Add(this.NetWeightCalcEdit);
			this.DetailsGroupBox.Controls.Add(this.zGuidFindBox1);
			this.DetailsGroupBox.Controls.Add(this.InvoiceNumberTextBox);
			this.DetailsGroupBox.Controls.Add(this.CPCFindBox);
			this.DetailsGroupBox.Controls.Add(this.PriceCalcEdit);
			this.DetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(21, 72, true);
			this.DetailsGroupBox.Name = "DetailsGroupBox";
			this.DetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(420, 171, true);
			this.DetailsGroupBox.TabIndex = 1;
			this.DetailsGroupBox.TabStop = false;
			// 
			// CheckBoxLic99
			// 
			this.BindingSource.SetBindingMember(this.CheckBoxLic99, "CreateLIC99");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.EU.Business.Declaration.SingleLineEntry)(null)).CreateLIC99)));
			this.CheckBoxLic99.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("16E5FF7C-1C9D-49C1-A6B5-6434B48486B7", "Add LIC99 statement?");
			this.CheckBoxLic99.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(136, 140, true);
			this.CheckBoxLic99.Name = "CheckBoxLic99";
			this.CheckBoxLic99.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(140, 24, true);
			this.CheckBoxLic99.TabIndex = 7;
			this.CheckBoxLic99.UseVisualStyleBackColor = true;
			// 
			// zLabel2
			// 
			this.zLabel2.AutoSize = true;
			this.zLabel2.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("SingleLineEntryForm|895a848a-16f2-4da4-8d2f-746f09f7e5d0", "Kilograms");
			this.zLabel2.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabel2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(242, 117, true);
			this.zLabel2.Name = "zLabel2";
			this.zLabel2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(52, 13, true);
			this.zLabel2.TabIndex = 6;
			// 
			// NetWeightCalcEdit
			// 
			this.NetWeightCalcEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.NetWeightCalcEdit, "NetWeight");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.Business.Declaration.SingleLineEntry)(null)).NetWeight)));
			this.NetWeightCalcEdit.CaptionResourceString = null;
			this.NetWeightCalcEdit.DecimalPlaces = 2;
			this.NetWeightCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(136, 114, true);
			this.NetWeightCalcEdit.Name = "NetWeightCalcEdit";
			this.NetWeightCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.NetWeightCalcEdit.TabIndex = 5;
			this.NetWeightCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// zGuidFindBox1
			// 
			this.zGuidFindBox1.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.zGuidFindBox1, "Currency");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.EU.Business.Declaration.SingleLineEntry)(null)).Currency)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.zGuidFindBox1, false);
			this.zGuidFindBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(245, 88, true);
			this.zGuidFindBox1.Name = "zGuidFindBox1";
			this.zGuidFindBox1.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.zGuidFindBox1.ParentType = null;
			this.zGuidFindBox1.PreBoundMaxLength = 3;
			this.zGuidFindBox1.ShowDescriptionBox = false;
			this.zGuidFindBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(57, 20, true);
			this.zGuidFindBox1.TabIndex = 4;
			// 
			// InvoiceNumberTextBox
			// 
			this.InvoiceNumberTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.InvoiceNumberTextBox, "InvoiceNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.SingleLineEntry)(null)).InvoiceNumber)));
			this.InvoiceNumberTextBox.CaptionResourceString = null;
			this.InvoiceNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(136, 62, true);
			this.InvoiceNumberTextBox.Name = "InvoiceNumberTextBox";
			this.InvoiceNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(203, 20, true);
			this.InvoiceNumberTextBox.TabIndex = 2;
			// 
			// CPCFindBox
			// 
			this.CPCFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CPCFindBox, "CPCCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.SingleLineEntry)(null)).CPCCode)));
			this.CPCFindBox.CaptionResourceString = null;
			this.CPCFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(136, 36, true);
			this.CPCFindBox.Name = "CPCFindBox";
			this.CPCFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.CPCFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.Universal.ZZRefCusProcedure;
			this.CPCFindBox.ParentType = null;
			this.CPCFindBox.PreBoundMaxLength = 7;
			this.CPCFindBox.ShowDescriptionBox = false;
			this.CPCFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(89, 20, true);
			this.CPCFindBox.TabIndex = 1;
			// 
			// zLabel1
			// 
			this.zLabel1.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(18, 9, true);
			this.zLabel1.Name = "zLabel1";
			this.zLabel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(423, 60, true);
			this.zLabel1.TabIndex = 0;
			// 
			// SingleLineEntryForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CancelButton = this.CancelZButton;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("0CC279EB-9EDF-4CED-8323-3519FF833E5B", "Single Line Entry Wizard");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(453, 315, true);
			this.Controls.Add(this.zLabel1);
			this.Controls.Add(this.CancelZButton);
			this.Controls.Add(this.OKButton);
			this.Controls.Add(this.DetailsGroupBox);
			this.DataSourceType = typeof(Enterprise.Customs.EU.Business.Declaration.SingleLineEntry);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Name = "SingleLineEntryForm";
			this.Controls.SetChildIndex(this.DetailsGroupBox, 0);
			this.Controls.SetChildIndex(this.OKButton, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.CancelZButton, 0);
			this.Controls.SetChildIndex(this.zLabel1, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.DetailsGroupBox.ResumeLayout(false);
			this.DetailsGroupBox.PerformLayout();
			this.zGuidFindBox1.ResumeLayout(true);
			this.zGuidFindBox1.PerformLayout();
			this.CPCFindBox.ResumeLayout(true);
			this.CPCFindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		protected Enterprise.ZArchitecture.GUI.ZButton OKButton;
		protected Enterprise.ZArchitecture.GUI.ZButton CancelZButton;
		protected Enterprise.ZArchitecture.ZCalcEdit PriceCalcEdit;
		protected Enterprise.ZArchitecture.GUI.ZGroupBox DetailsGroupBox;
		protected Enterprise.ZArchitecture.ZLabel zLabel1;
		protected Enterprise.ZArchitecture.GUI.ZCodeFindBox CPCFindBox;
		protected Enterprise.ZArchitecture.ZTextBox InvoiceNumberTextBox;
		protected Enterprise.ZArchitecture.ZLabel zLabel2;
		protected Enterprise.ZArchitecture.ZCalcEdit NetWeightCalcEdit;
		protected Enterprise.ZArchitecture.GUI.ZGuidFindBox zGuidFindBox1;
		protected ZArchitecture.GUI.ZCheckBox CheckBoxLic99;
	}
}
