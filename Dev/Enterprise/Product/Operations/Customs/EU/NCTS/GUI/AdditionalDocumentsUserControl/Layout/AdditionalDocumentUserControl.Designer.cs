namespace Enterprise.Customs.EU.NCTS.GUI
{
	partial class AdditionalDocumentUserControl
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
			this.KindDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.TypeCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.ReferenceNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.DescriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.DescriptionMultilineTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.LineNoCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.StatusLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.KindDropEdit.SuspendLayout();
			this.TypeCodeFindBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.NCTS.Business.NctsAdditionalInfo);
			// 
			// KindDropEdit
			// 
			this.KindDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.KindDropEdit, "CSI_SubType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.NCTS.Business.NctsAdditionalInfo)(null)).CSI_SubType)));
			this.KindDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(159, 41, true);
			this.KindDropEdit.Name = "KindDropEdit";
			this.KindDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 18, true);
			this.KindDropEdit.TabIndex = 0;
			// 
			// TypeCodeFindBox
			// 
			this.TypeCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TypeCodeFindBox, "CSI_Code");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.NctsAdditionalInfo)(null)).CSI_Code)));
			this.TypeCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(159, 67, true);
			this.TypeCodeFindBox.Name = "TypeCodeFindBox";
			this.TypeCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.TypeCodeFindBox.ParentType = null;
			this.TypeCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 18, true);
			this.TypeCodeFindBox.TabIndex = 1;
			// 
			// ReferenceNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.ReferenceNumberTextBox, "CSI_ReferenceNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.NctsAdditionalInfo)(null)).CSI_ReferenceNumber)));
			this.ReferenceNumberTextBox.CaptionResourceString = null;
			this.ReferenceNumberTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ReferenceNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(159, 91, true);
			this.ReferenceNumberTextBox.Name = "ReferenceNumberTextBox";
			this.ReferenceNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 18, true);
			this.ReferenceNumberTextBox.TabIndex = 2;
			// 
			// DescriptionTextBox
			// 
			this.BindingSource.SetBindingMember(this.DescriptionTextBox, "CSI_Description");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.NctsAdditionalInfo)(null)).CSI_Description)));
			this.DescriptionTextBox.CaptionResourceString = null;
			this.DescriptionTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.DescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(159, 117, true);
			this.DescriptionTextBox.Name = "DescriptionTextBox";
			this.DescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 18, true);
			this.DescriptionTextBox.TabIndex = 3;
			// 
			// DescriptionMultilineTextBox
			// 
			this.BindingSource.SetBindingMember(this.DescriptionMultilineTextBox, "CSI_Description");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.NctsAdditionalInfo)(null)).CSI_Description)));
			this.DescriptionMultilineTextBox.CaptionResourceString = null;
			this.DescriptionMultilineTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LabelCaptionRenderProvider.SetLabelTop(this.DescriptionMultilineTextBox, 2);
			this.DescriptionMultilineTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(159, 143, true);
			this.DescriptionMultilineTextBox.Multiline = true;
			this.DescriptionMultilineTextBox.Name = "DescriptionMultilineTextBox";
			this.DescriptionMultilineTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 100, true);
			this.DescriptionMultilineTextBox.TabIndex = 3;
			// 
			// LineNoCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.LineNoCalcEdit, "CSI_LineNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.NCTS.Business.NctsAdditionalInfo)(null)).CSI_LineNo)));
			this.LineNoCalcEdit.CaptionResourceString = null;
			this.LineNoCalcEdit.DecimalPlaces = 0;
			this.LineNoCalcEdit.Decimals = 0;
			this.LineNoCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(159, 0, true);
			this.LineNoCalcEdit.Name = "LineNoCalcEdit";
			this.LineNoCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(67, 18, true);
			this.LineNoCalcEdit.TabIndex = 10;
			this.LineNoCalcEdit.Text = "0";
			this.LineNoCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// StatusLabel
			// 
			this.StatusLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.StatusLabel, false);
			this.StatusLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(156, 21, true);
			this.StatusLabel.Name = "StatusLabel";
			this.StatusLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(260, 17, true);
			this.StatusLabel.TabIndex = 11;
			// 
			// AdditionalDocumentUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.StatusLabel);
			this.Controls.Add(this.LineNoCalcEdit);
			this.Controls.Add(this.KindDropEdit);
			this.Controls.Add(this.TypeCodeFindBox);
			this.Controls.Add(this.ReferenceNumberTextBox);
			this.Controls.Add(this.DescriptionTextBox);
			this.Controls.Add(this.DescriptionMultilineTextBox);
			this.Name = "AdditionalDocumentUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(444, 253, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.KindDropEdit.ResumeLayout(true);
			this.KindDropEdit.PerformLayout();
			this.TypeCodeFindBox.ResumeLayout(true);
			this.TypeCodeFindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal Enterprise.ZArchitecture.GUI.ZDropEdit KindDropEdit;
		internal Enterprise.ZArchitecture.GUI.ZCodeFindBox TypeCodeFindBox;
		internal ZArchitecture.ZTextBox ReferenceNumberTextBox;
		internal ZArchitecture.ZTextBox DescriptionTextBox;
		internal ZArchitecture.ZTextBox DescriptionMultilineTextBox;
		internal ZArchitecture.ZCalcEdit LineNoCalcEdit;
		internal ZArchitecture.ZLabel StatusLabel;
    }
}
