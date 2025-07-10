namespace Enterprise.Customs.EU.NCTS.GUI
{
	partial class SupportingDocumentUserControl
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
			this.TypeCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.ComplementTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ReferenceNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ItemNumberCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.LineNoCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.StatusLabel = new Enterprise.ZArchitecture.ZLabel();
			this.CountryCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.TypeCodeFindBox.SuspendLayout();
			this.CountryCodeFindBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.SupportingDocument);
			// 
			// TypeCodeFindBox
			// 
			this.TypeCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TypeCodeFindBox, "CSI_Code");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.SupportingDocument)(null)).CSI_Code)));
			this.TypeCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(162, 39, true);
			this.TypeCodeFindBox.Name = "TypeCodeFindBox";
			this.TypeCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.TypeCodeFindBox.ParentType = null;
			this.TypeCodeFindBox.PreBoundMaxLength = 8;
			this.TypeCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(272, 20, true);
			this.TypeCodeFindBox.TabIndex = 0;
			// 
			// ComplementTextBox
			// 
			this.BindingSource.SetBindingMember(this.ComplementTextBox, "CSI_ReferenceNumber2");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.SupportingDocument)(null)).CSI_ReferenceNumber2)));
			this.ComplementTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ComplementTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(162, 155, true);
			this.ComplementTextBox.Name = "ComplementTextBox";
			this.ComplementTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(272, 20, true);
			this.ComplementTextBox.TabIndex = 3;
			// 
			// ReferenceNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.ReferenceNumberTextBox, "CSI_ReferenceNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.SupportingDocument)(null)).CSI_ReferenceNumber)));
			this.ReferenceNumberTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ReferenceNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(162, 82, true);
			this.ReferenceNumberTextBox.Name = "ReferenceNumberTextBox";
			this.ReferenceNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(272, 20, true);
			this.ReferenceNumberTextBox.TabIndex = 1;
			// 
			// ItemNumberCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.ItemNumberCalcEdit, "CSI_ItemNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.SupportingDocument)(null)).CSI_ItemNumber)));
			this.ItemNumberCalcEdit.DecimalPlaces = 0;
			this.ItemNumberCalcEdit.Decimals = 0;
			this.ItemNumberCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(162, 195, true);
			this.ItemNumberCalcEdit.Name = "ItemNumberCalcEdit";
			this.ItemNumberCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(74, 20, true);
			this.ItemNumberCalcEdit.TabIndex = 2;
			this.ItemNumberCalcEdit.Text = "0";
			this.ItemNumberCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// LineNoCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.LineNoCalcEdit, "CSI_LineNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.SupportingDocument)(null)).CSI_LineNo)));
			this.LineNoCalcEdit.DecimalPlaces = 0;
			this.LineNoCalcEdit.Decimals = 0;
			this.LineNoCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(162, 13, true);
			this.LineNoCalcEdit.Name = "LineNoCalcEdit";
			this.LineNoCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(67, 20, true);
			this.LineNoCalcEdit.TabIndex = 10;
			this.LineNoCalcEdit.Text = "0";
			this.LineNoCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// StatusLabel
			// 
			this.StatusLabel.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("c3b515e0-3703-435e-9c42-9a216105b39e", "New Value");
			this.StatusLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.StatusLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(159, 115, true);
			this.StatusLabel.Name = "StatusLabel";
			this.StatusLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 23, true);
			this.StatusLabel.TabIndex = 11;
			// 
			// CountryCodeFindBox
			// 
			this.BindingSource.SetBindingMember(this.CountryCodeFindBox, "CSI_RN_NKCountryCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.SupportingDocument)(null)).CSI_RN_NKCountryCode)));
			this.CountryCodeFindBox.AllowDrop = true;
			this.CountryCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(162, 221, true);
			this.CountryCodeFindBox.Name = "CountryCodeFindBox";
			this.CountryCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.CountryCodeFindBox.ParentType = null;
			this.CountryCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.CountryCodeFindBox.TabIndex = 12;
			// 
			// SupportingDocumentUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.LineNoCalcEdit);
			this.Controls.Add(this.TypeCodeFindBox);
			this.Controls.Add(this.ReferenceNumberTextBox);
			this.Controls.Add(this.ComplementTextBox);
			this.Controls.Add(this.ItemNumberCalcEdit);
			this.Controls.Add(this.StatusLabel);
			this.Controls.Add(this.CountryCodeFindBox);
			this.Name = "SupportingDocumentUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(515, 256, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.TypeCodeFindBox.ResumeLayout(true);
			this.TypeCodeFindBox.PerformLayout();
			this.CountryCodeFindBox.ResumeLayout(true);
			this.CountryCodeFindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal Enterprise.ZArchitecture.GUI.ZCodeFindBox TypeCodeFindBox;
		internal ZArchitecture.ZTextBox ReferenceNumberTextBox;
		internal ZArchitecture.ZTextBox ComplementTextBox;
		internal ZArchitecture.ZCalcEdit ItemNumberCalcEdit;
		internal ZArchitecture.ZCalcEdit LineNoCalcEdit;
		internal ZArchitecture.ZLabel StatusLabel;
		internal Enterprise.ZArchitecture.GUI.ZCodeFindBox CountryCodeFindBox;
	}
}
