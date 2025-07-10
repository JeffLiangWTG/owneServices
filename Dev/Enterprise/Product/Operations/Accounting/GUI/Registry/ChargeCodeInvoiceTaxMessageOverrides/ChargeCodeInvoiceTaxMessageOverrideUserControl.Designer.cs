using Enterprise.Accounting.Registry.Business;
namespace Enterprise.Accounting.Registry.GUI
{
    partial class ChargeCodeInvoiceTaxMessageOverrideUserControl
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo zMultiLineTextBoxColumnInfo1 = new Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo();
			Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo zMultiLineTextBoxColumnInfo2 = new Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo();
			Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo zMultiLineTextBoxColumnInfo3 = new Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo();
			Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo zMultiLineTextBoxColumnInfo4 = new Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo();
			this.ChargeCodeInvoiceTaxMessageOverrideGrid = new Enterprise.ZArchitecture.ZGrid();
			this.zPanel1 = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.EnglishMessageOverrideGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.zTextBox1 = new Enterprise.ZArchitecture.ZTextBox();
			this.LocalMessageOverrideGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.zTextBox4 = new Enterprise.ZArchitecture.ZTextBox();
			this.CurrentEnglishMessageGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.zTextBox2 = new Enterprise.ZArchitecture.ZTextBox();
			this.CurrentLocalMessageGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.zTextBox3 = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ChargeCodeInvoiceTaxMessageOverrideGrid)).BeginInit();
			this.zPanel1.SuspendLayout();
			this.EnglishMessageOverrideGroupBox.SuspendLayout();
			this.LocalMessageOverrideGroupBox.SuspendLayout();
			this.CurrentEnglishMessageGroupBox.SuspendLayout();
			this.CurrentLocalMessageGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(ChargeCodeInvoiceTaxMessageOverrideCollection);
			// 
			// ChargeCodeInvoiceTaxMessageOverrideGrid
			// 
			this.ChargeCodeInvoiceTaxMessageOverrideGrid.AllowNavigation = false;
			this.ChargeCodeInvoiceTaxMessageOverrideGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ChargeCodeInvoiceTaxMessageOverrideGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((ChargeCodeInvoiceTaxMessageOverride)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((ChargeCodeInvoiceTaxMessageOverride)(null)).ChargeCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((ChargeCodeInvoiceTaxMessageOverride)(null)).TaxMessage)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((ChargeCodeInvoiceTaxMessageOverride)(null)).LocalOverrideMessage)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((ChargeCodeInvoiceTaxMessageOverride)(null)).EnglishOverrideMessage)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((ChargeCodeInvoiceTaxMessageOverride)(null)).EnglishMessage)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((ChargeCodeInvoiceTaxMessageOverride)(null)).LocalMessage)));
			this.ChargeCodeInvoiceTaxMessageOverrideGrid.CaptionVisible = false;
			zGuidFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ChargeCodeInvoiceTaxMessageOverrideUserControl|99901afb-8278-43f0-b51b-2d410c075fc6", "Charge Code");
			zGuidFindBoxColumnStyleInfo1.ColumnName = "ChargeCode";
			zGuidFindBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ChargeCodeInvoiceTaxMessageOverrideUserControl|4f82f2af-0b06-4205-b99a-f9c877660d57", "Tax Message");
			zGuidFindBoxColumnStyleInfo2.ColumnName = "TaxMessage";
			zMultiLineTextBoxColumnInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zMultiLineTextBoxColumnInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ChargeCodeInvoiceTaxMessageOverrideUserControl|b847ccfb-be6e-4fca-82f1-270efe9c8ea0", "Local Override Message");
			zMultiLineTextBoxColumnInfo1.ColumnName = "LocalOverrideMessage";
			zMultiLineTextBoxColumnInfo1.MinimumEditControlWidth = 300;
			zMultiLineTextBoxColumnInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zMultiLineTextBoxColumnInfo2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ChargeCodeInvoiceTaxMessageOverrideUserControl|aa9f6922-2f86-496d-9a4f-b43837110598", "English Override Message");
			zMultiLineTextBoxColumnInfo2.ColumnName = "EnglishOverrideMessage";
			zMultiLineTextBoxColumnInfo2.MinimumEditControlWidth = 300;
			zMultiLineTextBoxColumnInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zMultiLineTextBoxColumnInfo3.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ChargeCodeInvoiceTaxMessageOverrideUserControl|6c2a3955-5b10-40f0-a5ea-79e2dbc592c2", "Current English Message");
			zMultiLineTextBoxColumnInfo3.ColumnName = "EnglishMessage";
			zMultiLineTextBoxColumnInfo3.MinimumEditControlWidth = 300;
			zMultiLineTextBoxColumnInfo4.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zMultiLineTextBoxColumnInfo4.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ChargeCodeInvoiceTaxMessageOverrideUserControl|eb13d59c-4fce-4156-8bd1-4bc045c1f1ce", "Current Local Message");
			zMultiLineTextBoxColumnInfo4.ColumnName = "LocalMessage";
			zMultiLineTextBoxColumnInfo4.MinimumEditControlWidth = 300;
			this.ChargeCodeInvoiceTaxMessageOverrideGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.ChargeCodeInvoiceTaxMessageOverrideGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo2);
			this.ChargeCodeInvoiceTaxMessageOverrideGrid.ColumnStyles.Add(zMultiLineTextBoxColumnInfo1);
			this.ChargeCodeInvoiceTaxMessageOverrideGrid.ColumnStyles.Add(zMultiLineTextBoxColumnInfo2);
			this.ChargeCodeInvoiceTaxMessageOverrideGrid.ColumnStyles.Add(zMultiLineTextBoxColumnInfo3);
			this.ChargeCodeInvoiceTaxMessageOverrideGrid.ColumnStyles.Add(zMultiLineTextBoxColumnInfo4);
			this.ChargeCodeInvoiceTaxMessageOverrideGrid.GridId = "CE265471-EDF4-4B30-8784-7C17C48179F2";
			this.ChargeCodeInvoiceTaxMessageOverrideGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ChargeCodeInvoiceTaxMessageOverrideGrid.LayoutKey = "zGrid1";
			this.ChargeCodeInvoiceTaxMessageOverrideGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ChargeCodeInvoiceTaxMessageOverrideGrid.Name = "ChargeCodeInvoiceTaxMessageOverrideGrid";
			this.ChargeCodeInvoiceTaxMessageOverrideGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(690, 265, true);
			this.ChargeCodeInvoiceTaxMessageOverrideGrid.TabIndex = 0;
			// 
			// zPanel1
			// 
			this.zPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.zPanel1.Controls.Add(this.EnglishMessageOverrideGroupBox);
			this.zPanel1.Controls.Add(this.LocalMessageOverrideGroupBox);
			this.zPanel1.Controls.Add(this.CurrentEnglishMessageGroupBox);
			this.zPanel1.Controls.Add(this.CurrentLocalMessageGroupBox);
			this.zPanel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 266, true);
			this.zPanel1.Name = "zPanel1";
			this.zPanel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(690, 183, true);
			this.zPanel1.TabIndex = 2;
			// 
			// EnglishMessageOverrideGroupBox
			// 
			this.EnglishMessageOverrideGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.EnglishMessageOverrideGroupBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ChargeCodeInvoiceTaxMessageOverrideUserControl|770a5844-5036-4716-9d6c-f46933ab3a7a", "English Message Override");
			this.EnglishMessageOverrideGroupBox.Controls.Add(this.zTextBox1);
			this.EnglishMessageOverrideGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(347, 5, true);
			this.EnglishMessageOverrideGroupBox.Name = "EnglishMessageOverrideGroupBox";
			this.EnglishMessageOverrideGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(340, 84, true);
			this.EnglishMessageOverrideGroupBox.TabIndex = 0;
			this.EnglishMessageOverrideGroupBox.TabStop = false;
			// 
			// zTextBox1
			// 
			this.BindingSource.SetBindingMember(this.zTextBox1, "EnglishOverrideMessage");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((ChargeCodeInvoiceTaxMessageOverride)(null)).EnglishOverrideMessage)));
			this.zTextBox1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.zTextBox1, false);
			this.zTextBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.zTextBox1.Multiline = true;
			this.zTextBox1.Name = "zTextBox1";
			this.zTextBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(334, 65, true);
			this.zTextBox1.TabIndex = 0;
			// 
			// LocalMessageOverrideGroupBox
			// 
			this.LocalMessageOverrideGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.LocalMessageOverrideGroupBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ChargeCodeInvoiceTaxMessageOverrideUserControl|d6cba848-8ac5-4a64-9e9e-4f96e041a0b9", "Local Message Override");
			this.LocalMessageOverrideGroupBox.Controls.Add(this.zTextBox4);
			this.LocalMessageOverrideGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(347, 93, true);
			this.LocalMessageOverrideGroupBox.Name = "LocalMessageOverrideGroupBox";
			this.LocalMessageOverrideGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(340, 84, true);
			this.LocalMessageOverrideGroupBox.TabIndex = 0;
			this.LocalMessageOverrideGroupBox.TabStop = false;
			// 
			// zTextBox4
			// 
			this.BindingSource.SetBindingMember(this.zTextBox4, "LocalOverrideMessage");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((ChargeCodeInvoiceTaxMessageOverride)(null)).LocalOverrideMessage)));
			this.zTextBox4.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.zTextBox4, false);
			this.zTextBox4.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.zTextBox4.Multiline = true;
			this.zTextBox4.Name = "zTextBox4";
			this.zTextBox4.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(334, 65, true);
			this.zTextBox4.TabIndex = 0;
			// 
			// CurrentEnglishMessageGroupBox
			// 
			this.CurrentEnglishMessageGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.CurrentEnglishMessageGroupBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ChargeCodeInvoiceTaxMessageOverrideUserControl|f561f264-d50f-48d7-bcbd-765319425251", "Current English Message");
			this.CurrentEnglishMessageGroupBox.Controls.Add(this.zTextBox2);
			this.CurrentEnglishMessageGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 5, true);
			this.CurrentEnglishMessageGroupBox.Name = "CurrentEnglishMessageGroupBox";
			this.CurrentEnglishMessageGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(341, 84, true);
			this.CurrentEnglishMessageGroupBox.TabIndex = 0;
			this.CurrentEnglishMessageGroupBox.TabStop = false;
			// 
			// zTextBox2
			// 
			this.BindingSource.SetBindingMember(this.zTextBox2, "EnglishMessage");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((ChargeCodeInvoiceTaxMessageOverride)(null)).EnglishMessage)));
			this.zTextBox2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.zTextBox2, false);
			this.zTextBox2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.zTextBox2.Multiline = true;
			this.zTextBox2.Name = "zTextBox2";
			this.zTextBox2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(335, 65, true);
			this.zTextBox2.TabIndex = 0;
			// 
			// CurrentLocalMessageGroupBox
			// 
			this.CurrentLocalMessageGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.CurrentLocalMessageGroupBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ChargeCodeInvoiceTaxMessageOverrideUserControl|5023c0cf-dbd4-4f79-96e4-f9ccdc002bdf", "Current Local Message");
			this.CurrentLocalMessageGroupBox.Controls.Add(this.zTextBox3);
			this.CurrentLocalMessageGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 93, true);
			this.CurrentLocalMessageGroupBox.Name = "CurrentLocalMessageGroupBox";
			this.CurrentLocalMessageGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(341, 84, true);
			this.CurrentLocalMessageGroupBox.TabIndex = 0;
			this.CurrentLocalMessageGroupBox.TabStop = false;
			// 
			// zTextBox3
			// 
			this.BindingSource.SetBindingMember(this.zTextBox3, "LocalMessage");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((ChargeCodeInvoiceTaxMessageOverride)(null)).LocalMessage)));
			this.zTextBox3.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.zTextBox3, false);
			this.zTextBox3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.zTextBox3.Multiline = true;
			this.zTextBox3.Name = "zTextBox3";
			this.zTextBox3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(335, 65, true);
			this.zTextBox3.TabIndex = 0;
			// 
			// ChargeCodeInvoiceTaxMessageOverrideUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.zPanel1);
			this.Controls.Add(this.ChargeCodeInvoiceTaxMessageOverrideGrid);
			this.Name = "ChargeCodeInvoiceTaxMessageOverrideUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(690, 449, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ChargeCodeInvoiceTaxMessageOverrideGrid)).EndInit();
			this.zPanel1.ResumeLayout(false);
			this.EnglishMessageOverrideGroupBox.ResumeLayout(false);
			this.EnglishMessageOverrideGroupBox.PerformLayout();
			this.LocalMessageOverrideGroupBox.ResumeLayout(false);
			this.LocalMessageOverrideGroupBox.PerformLayout();
			this.CurrentEnglishMessageGroupBox.ResumeLayout(false);
			this.CurrentEnglishMessageGroupBox.PerformLayout();
			this.CurrentLocalMessageGroupBox.ResumeLayout(false);
			this.CurrentLocalMessageGroupBox.PerformLayout();
			this.ResumeLayout(false);

        }

        #endregion

        private Enterprise.ZArchitecture.ZGrid ChargeCodeInvoiceTaxMessageOverrideGrid;
        private Enterprise.ZArchitecture.GUI.ZPanel zPanel1;
        private Enterprise.ZArchitecture.GUI.ZGroupBox EnglishMessageOverrideGroupBox;
        private Enterprise.ZArchitecture.GUI.ZGroupBox LocalMessageOverrideGroupBox;
        private Enterprise.ZArchitecture.GUI.ZGroupBox CurrentEnglishMessageGroupBox;
        private Enterprise.ZArchitecture.GUI.ZGroupBox CurrentLocalMessageGroupBox;
        private Enterprise.ZArchitecture.ZTextBox zTextBox1;
        private Enterprise.ZArchitecture.ZTextBox zTextBox4;
        private Enterprise.ZArchitecture.ZTextBox zTextBox2;
        private Enterprise.ZArchitecture.ZTextBox zTextBox3;
    }
}
