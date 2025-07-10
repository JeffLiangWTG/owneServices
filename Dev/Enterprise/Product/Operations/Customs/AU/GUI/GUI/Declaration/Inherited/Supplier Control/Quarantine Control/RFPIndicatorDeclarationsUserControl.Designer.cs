namespace Enterprise.Customs.AU.Declaration.GUI
{
	partial class RFPIndicatorDeclarationsUserControl
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
			this.ExporterDeclarationTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.TrueAndCompleteLabel = new Enterprise.ZArchitecture.ZLabel();
			this.DeclarationOfComplianceLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ImportedProductsLabel = new Enterprise.ZArchitecture.ZLabel();
			this.QH_ImportedProductFlagDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.QH_DecOfComplianceDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.QH_TrueAndCompleteDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.DeclarationofComplianceGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ImportedProductFlagGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.TrueAndCompleteIndicatorGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ExporterDeclarationGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.QH_ImportedProductFlagDropEdit.SuspendLayout();
			this.QH_DecOfComplianceDropEdit.SuspendLayout();
			this.QH_TrueAndCompleteDropEdit.SuspendLayout();
			this.DeclarationofComplianceGroupBox.SuspendLayout();
			this.ImportedProductFlagGroupBox.SuspendLayout();
			this.TrueAndCompleteIndicatorGroupBox.SuspendLayout();
			this.ExporterDeclarationGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.AU.Declaration.Business.JobComInvoiceHeader);
			// 
			// ExporterDeclarationTextBox
			// 
			this.ExporterDeclarationTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ExporterDeclarationTextBox, "QuarantineExDocHeader+QH_ExporterDeclaration");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceHeader)(null)).QuarantineExDocHeader.QH_ExporterDeclaration)));
			this.ExporterDeclarationTextBox.CaptionResourceString = null;
			this.ExporterDeclarationTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 19, true);
			this.ExporterDeclarationTextBox.Multiline = true;
			this.ExporterDeclarationTextBox.Name = "ExporterDeclarationTextBox";
			this.ExporterDeclarationTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(238, 210, true);
			this.ExporterDeclarationTextBox.TabIndex = 41;
			// 
			// TrueAndCompleteLabel
			// 
			this.TrueAndCompleteLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.TrueAndCompleteLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.TrueAndCompleteLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 47, true);
			this.TrueAndCompleteLabel.Name = "TrueAndCompleteLabel";
			this.TrueAndCompleteLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(278, 95, true);
			this.TrueAndCompleteLabel.TabIndex = 2;
			this.TrueAndCompleteLabel.TextAlign = System.Drawing.ContentAlignment.TopLeft;
			// 
			// DeclarationOfComplianceLabel
			// 
			this.DeclarationOfComplianceLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.DeclarationOfComplianceLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.DeclarationOfComplianceLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 47, true);
			this.DeclarationOfComplianceLabel.Name = "DeclarationOfComplianceLabel";
			this.DeclarationOfComplianceLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(308, 182, true);
			this.DeclarationOfComplianceLabel.TabIndex = 2;
			this.DeclarationOfComplianceLabel.TextAlign = System.Drawing.ContentAlignment.TopLeft;
			// 
			// ImportedProductsLabel
			// 
			this.ImportedProductsLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.ImportedProductsLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.ImportedProductsLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 47, true);
			this.ImportedProductsLabel.Name = "ImportedProductsLabel";
			this.ImportedProductsLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(278, 26, true);
			this.ImportedProductsLabel.TabIndex = 2;
			this.ImportedProductsLabel.Text = "Do any of the products listed in this RFP contain imported dairy ingredients, oth" +
    "er than from New Zealand?";
			this.ImportedProductsLabel.TextAlign = System.Drawing.ContentAlignment.TopLeft;
			// 
			// QH_ImportedProductFlagDropEdit
			// 
			this.QH_ImportedProductFlagDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.QH_ImportedProductFlagDropEdit, "QuarantineExDocHeader+QH_ImportedProductFlag");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceHeader)(null)).QuarantineExDocHeader.QH_ImportedProductFlag)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceHeader)(null)).QuarantineExDocHeader.Lookups.ImportedProduct)));
			this.QH_ImportedProductFlagDropEdit.BindToList = "QuarantineExDocHeader+Lookups+ImportedProduct";
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.QH_ImportedProductFlagDropEdit, false);
			this.QH_ImportedProductFlagDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 19, true);
			this.QH_ImportedProductFlagDropEdit.Name = "QH_ImportedProductFlagDropEdit";
			this.QH_ImportedProductFlagDropEdit.PreBoundMaxLength = 3;
			this.QH_ImportedProductFlagDropEdit.ShouldResizeByMaxLength = true;
			this.QH_ImportedProductFlagDropEdit.ShowDescriptionBox = false;
			this.QH_ImportedProductFlagDropEdit.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			this.QH_ImportedProductFlagDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
			this.QH_ImportedProductFlagDropEdit.TabIndex = 1;
			// 
			// QH_DecOfComplianceDropEdit
			// 
			this.QH_DecOfComplianceDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.QH_DecOfComplianceDropEdit, "QuarantineExDocHeader+QH_DecOfCompliance");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceHeader)(null)).QuarantineExDocHeader.QH_DecOfCompliance)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceHeader)(null)).QuarantineExDocHeader.Lookups.DeclarationOfCompliance)));
			this.QH_DecOfComplianceDropEdit.BindToList = "QuarantineExDocHeader+Lookups+DeclarationOfCompliance";
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.QH_DecOfComplianceDropEdit, false);
			this.QH_DecOfComplianceDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 19, true);
			this.QH_DecOfComplianceDropEdit.Name = "QH_DecOfComplianceDropEdit";
			this.QH_DecOfComplianceDropEdit.PreBoundMaxLength = 3;
			this.QH_DecOfComplianceDropEdit.ShouldResizeByMaxLength = true;
			this.QH_DecOfComplianceDropEdit.ShowDescriptionBox = false;
			this.QH_DecOfComplianceDropEdit.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			this.QH_DecOfComplianceDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
			this.QH_DecOfComplianceDropEdit.TabIndex = 1;
			// 
			// QH_TrueAndCompleteDropEdit
			// 
			this.QH_TrueAndCompleteDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.QH_TrueAndCompleteDropEdit, "QuarantineExDocHeader+QH_TrueAndCompleteIndicator");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceHeader)(null)).QuarantineExDocHeader.QH_TrueAndCompleteIndicator)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.QH_TrueAndCompleteDropEdit, false);
			this.QH_TrueAndCompleteDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 19, true);
			this.QH_TrueAndCompleteDropEdit.Name = "QH_TrueAndCompleteDropEdit";
			this.QH_TrueAndCompleteDropEdit.PreBoundMaxLength = 3;
			this.QH_TrueAndCompleteDropEdit.ShouldResizeByMaxLength = true;
			this.QH_TrueAndCompleteDropEdit.ShowDescriptionBox = false;
			this.QH_TrueAndCompleteDropEdit.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			this.QH_TrueAndCompleteDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
			this.QH_TrueAndCompleteDropEdit.TabIndex = 1;
			// 
			// DeclarationofComplianceGroupBox
			// 
			this.DeclarationofComplianceGroupBox.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("aa4f639d-86d2-47b8-9704-7667e7711d75", "Declaration of Compliance Indicator");
			this.DeclarationofComplianceGroupBox.Controls.Add(this.QH_DecOfComplianceDropEdit);
			this.DeclarationofComplianceGroupBox.Controls.Add(this.DeclarationOfComplianceLabel);
			this.DeclarationofComplianceGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 12, true);
			this.DeclarationofComplianceGroupBox.Name = "DeclarationofComplianceGroupBox";
			this.DeclarationofComplianceGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(320, 237, true);
			this.DeclarationofComplianceGroupBox.TabIndex = 1;
			this.DeclarationofComplianceGroupBox.TabStop = false;
			this.DeclarationofComplianceGroupBox.Text = "Declaration of Compliance Indicator";
			// 
			// ImportedProductFlagGroupBox
			// 
			this.ImportedProductFlagGroupBox.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("8fa325c0-48de-4876-971e-8cf05724aefe", "Imported Product Flag");
			this.ImportedProductFlagGroupBox.Controls.Add(this.QH_ImportedProductFlagDropEdit);
			this.ImportedProductFlagGroupBox.Controls.Add(this.ImportedProductsLabel);
			this.ImportedProductFlagGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(329, 12, true);
			this.ImportedProductFlagGroupBox.Name = "ImportedProductFlagGroupBox";
			this.ImportedProductFlagGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(290, 86, true);
			this.ImportedProductFlagGroupBox.TabIndex = 2;
			this.ImportedProductFlagGroupBox.TabStop = false;
			this.ImportedProductFlagGroupBox.Text = "Imported Product Flag";
			// 
			// TrueAndCompleteIndicatorGroupBox
			// 
			this.TrueAndCompleteIndicatorGroupBox.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("01106a92-214a-4fc7-a162-bc0d6adc5743", "True and Complete Indicator");
			this.TrueAndCompleteIndicatorGroupBox.Controls.Add(this.QH_TrueAndCompleteDropEdit);
			this.TrueAndCompleteIndicatorGroupBox.Controls.Add(this.TrueAndCompleteLabel);
			this.TrueAndCompleteIndicatorGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(329, 104, true);
			this.TrueAndCompleteIndicatorGroupBox.Name = "TrueAndCompleteIndicatorGroupBox";
			this.TrueAndCompleteIndicatorGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(290, 145, true);
			this.TrueAndCompleteIndicatorGroupBox.TabIndex = 3;
			this.TrueAndCompleteIndicatorGroupBox.TabStop = false;
			this.TrueAndCompleteIndicatorGroupBox.Text = "True and Complete Indicator";
			// 
			// ExporterDeclarationGroupBox
			// 
			this.ExporterDeclarationGroupBox.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("af04c7fa-5b6b-427a-a342-8c55f436e2d4", "Exporter Declaration");
			this.ExporterDeclarationGroupBox.Controls.Add(this.ExporterDeclarationTextBox);
			this.ExporterDeclarationGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(625, 12, true);
			this.ExporterDeclarationGroupBox.Name = "ExporterDeclarationGroupBox";
			this.ExporterDeclarationGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 237, true);
			this.ExporterDeclarationGroupBox.TabIndex = 4;
			this.ExporterDeclarationGroupBox.TabStop = false;
			this.ExporterDeclarationGroupBox.Text = "Exporter Declaration";
			// 
			// RFPIndicatorDeclarationsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.ExporterDeclarationGroupBox);
			this.Controls.Add(this.TrueAndCompleteIndicatorGroupBox);
			this.Controls.Add(this.ImportedProductFlagGroupBox);
			this.Controls.Add(this.DeclarationofComplianceGroupBox);
			this.Name = "RFPIndicatorDeclarationsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1124, 253, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.QH_ImportedProductFlagDropEdit.ResumeLayout(true);
			this.QH_ImportedProductFlagDropEdit.PerformLayout();
			this.QH_DecOfComplianceDropEdit.ResumeLayout(true);
			this.QH_DecOfComplianceDropEdit.PerformLayout();
			this.QH_TrueAndCompleteDropEdit.ResumeLayout(true);
			this.QH_TrueAndCompleteDropEdit.PerformLayout();
			this.DeclarationofComplianceGroupBox.ResumeLayout(false);
			this.DeclarationofComplianceGroupBox.PerformLayout();
			this.ImportedProductFlagGroupBox.ResumeLayout(false);
			this.ImportedProductFlagGroupBox.PerformLayout();
			this.TrueAndCompleteIndicatorGroupBox.ResumeLayout(false);
			this.TrueAndCompleteIndicatorGroupBox.PerformLayout();
			this.ExporterDeclarationGroupBox.ResumeLayout(false);
			this.ExporterDeclarationGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZTextBox ExporterDeclarationTextBox;
		private ZArchitecture.ZLabel TrueAndCompleteLabel;
		private ZArchitecture.ZLabel DeclarationOfComplianceLabel;
		private ZArchitecture.ZLabel ImportedProductsLabel;
		private ZArchitecture.GUI.ZDropEdit QH_ImportedProductFlagDropEdit;
		private ZArchitecture.GUI.ZDropEdit QH_DecOfComplianceDropEdit;
		private ZArchitecture.GUI.ZDropEdit QH_TrueAndCompleteDropEdit;
		private ZArchitecture.GUI.ZGroupBox DeclarationofComplianceGroupBox;
		private ZArchitecture.GUI.ZGroupBox ImportedProductFlagGroupBox;
		private ZArchitecture.GUI.ZGroupBox TrueAndCompleteIndicatorGroupBox;
		private ZArchitecture.GUI.ZGroupBox ExporterDeclarationGroupBox;
	}
}
