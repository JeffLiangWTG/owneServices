namespace Enterprise.Customs.EU.NCTS.GUI
{
	partial class HouseConsignmentGoodItemsSupportingDocumentsUserControl
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(HouseConsignmentGoodItemsSupportingDocumentsUserControl));
			this.SequenceNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.DocTypeCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.ReferenceNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ComplementInfoTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.StatusLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.DocTypeCodeFindBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.NCTS.Business.NctsSupportingDocument);
			// 
			// SequenceNumberTextBox
			// 
			this.SequenceNumberTextBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SequenceNumberTextBox, "CSI_LineNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZInt)(((Enterprise.Customs.EU.NCTS.Business.NctsSupportingDocument)(null)).CSI_LineNo)));
			this.SequenceNumberTextBox.CaptionResourceString = null;
			this.SequenceNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(76, 43, true);
			this.SequenceNumberTextBox.Name = "SequenceNumberTextBox";
			this.SequenceNumberTextBox.ReadOnly = true;
			this.SequenceNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.SequenceNumberTextBox.TabIndex = 0;
			// 
			// DocTypeCodeFindBox
			// 
			this.DocTypeCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DocTypeCodeFindBox, "CSI_Code");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.NctsSupportingDocument)(null)).CSI_Code)));
			this.DocTypeCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(76, 121, true);
			this.DocTypeCodeFindBox.Name = "DocTypeCodeFindBox";
			this.DocTypeCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.DocTypeCodeFindBox.ParentType = null;
			this.DocTypeCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.DocTypeCodeFindBox.TabIndex = 1;
			// 
			// ReferenceNumberTextBox
			// 
			this.ReferenceNumberTextBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ReferenceNumberTextBox, "CSI_ReferenceNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.NctsSupportingDocument)(null)).CSI_ReferenceNumber)));
			this.ReferenceNumberTextBox.CaptionResourceString = null;
			this.ReferenceNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(76, 69, true);
			this.ReferenceNumberTextBox.Name = "ReferenceNumberTextBox";
			this.ReferenceNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.ReferenceNumberTextBox.TabIndex = 2;
			// 
			// ComplementInfoTextBox
			// 
			this.ComplementInfoTextBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ComplementInfoTextBox, "CSI_ReferenceNumber2");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.NctsSupportingDocument)(null)).CSI_ReferenceNumber2)));
			this.ComplementInfoTextBox.CaptionResourceString = null;
			this.ComplementInfoTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(76, 95, true);
			this.ComplementInfoTextBox.Name = "ComplementInfoTextBox";
			this.ComplementInfoTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.ComplementInfoTextBox.TabIndex = 3;
			// 
			// StatusLabel
			// 
			this.StatusLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.StatusLabel, false);
			this.StatusLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(73, 14, true);
			this.StatusLabel.Name = "StatusLabel";
			this.StatusLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(260, 17, true);
			this.StatusLabel.TabIndex = 11;
			// 
			// HouseConsignmentGoodItemsSupportingDocumentsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ComplementInfoTextBox);
			this.Controls.Add(this.ReferenceNumberTextBox);
			this.Controls.Add(this.SequenceNumberTextBox);
			this.Controls.Add(this.StatusLabel);
			this.Controls.Add(this.DocTypeCodeFindBox);
			this.Name = "HouseConsignmentGoodItemsSupportingDocumentsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(386, 289, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.DocTypeCodeFindBox.ResumeLayout(true);
			this.DocTypeCodeFindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		internal ZArchitecture.ZTextBox SequenceNumberTextBox;
		internal ZArchitecture.GUI.ZCodeFindBox DocTypeCodeFindBox;
		internal ZArchitecture.ZTextBox ReferenceNumberTextBox;
		internal ZArchitecture.ZTextBox ComplementInfoTextBox;
		internal ZArchitecture.ZLabel StatusLabel;

		#endregion
	}
}
