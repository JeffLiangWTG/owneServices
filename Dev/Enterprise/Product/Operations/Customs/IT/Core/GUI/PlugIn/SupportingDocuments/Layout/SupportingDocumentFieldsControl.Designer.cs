namespace Enterprise.Customs.IT.GUI
{
	partial class SupportingDocumentFieldsControl
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
			this.YearOfIssueTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.IssuingAuthorityTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CountryCodeCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.AvailabilityDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.LineNoCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.ValueCalcFindBox = new Enterprise.ZArchitecture.GUI.ZCalcFindBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.CountryCodeCodeFindBox.SuspendLayout();
			this.AvailabilityDropEdit.SuspendLayout();
			this.ValueCalcFindBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.IT.Business.Declaration.SupportingDocument);
			// 
			// YearOfIssueTextBox
			// 
			this.BindingSource.SetBindingMember(this.YearOfIssueTextBox, "CSI_YearOfIssue");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IT.Business.Declaration.SupportingDocument)(null)).CSI_YearOfIssue)));
			this.YearOfIssueTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(47, 86, true);
			this.YearOfIssueTextBox.Name = "YearOfIssueTextBox";
			this.YearOfIssueTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.YearOfIssueTextBox.TabIndex = 12;
			// 
			// IssuingAuthorityTextBox
			// 
			this.IssuingAuthorityTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.IssuingAuthorityTextBox, "CSI_ReferenceNumber2");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IT.Business.Declaration.SupportingDocument)(null)).CSI_ReferenceNumber2)));
			this.IssuingAuthorityTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.IssuingAuthorityTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(47, 131, true);
			this.IssuingAuthorityTextBox.Name = "IssuingAuthorityTextBox";
			this.IssuingAuthorityTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(254, 20, true);
			this.IssuingAuthorityTextBox.TabIndex = 15;
			// 
			// CountryCodeCodeFindBox
			// 
			this.CountryCodeCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CountryCodeCodeFindBox, "CSI_RN_NKCountryCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IT.Business.Declaration.SupportingDocument)(null)).CSI_RN_NKCountryCode)));
			this.CountryCodeCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(47, 157, true);
			this.CountryCodeCodeFindBox.Name = "CountryCodeCodeFindBox";
			this.CountryCodeCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.CountryCodeCodeFindBox.ParentType = null;
			this.CountryCodeCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(150, 20, true);
			this.CountryCodeCodeFindBox.TabIndex = 16;
			// 
			// AvailabilityDropEdit
			// 
			this.AvailabilityDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AvailabilityDropEdit, "CSI_Status");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IT.Business.Declaration.SupportingDocument)(null)).CSI_Status)));
			this.AvailabilityDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(47, 195, true);
			this.AvailabilityDropEdit.Name = "AvailabilityDropEdit";
			this.AvailabilityDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(254, 20, true);
			this.AvailabilityDropEdit.TabIndex = 17;
			// 
			// LineNoCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.LineNoCalcEdit, "CSI_LineNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.IT.Business.Declaration.SupportingDocument)(null)).CSI_LineNo)));
			this.LineNoCalcEdit.DecimalPlaces = 0;
			this.LineNoCalcEdit.Decimals = 0;
			this.LineNoCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(47, 221, true);
			this.LineNoCalcEdit.MaxLength = 4;
			this.LineNoCalcEdit.Name = "LineNoCalcEdit";
			this.LineNoCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.LineNoCalcEdit.TabIndex = 18;
			this.LineNoCalcEdit.Text = "0";
			this.LineNoCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.LineNoCalcEdit.TrackDisposedAccess = true;
			// 
			// ValueCalcFindBox
			// 
			this.ValueCalcFindBox.AllowDrop = true;
			this.ValueCalcFindBox.BackColor = System.Drawing.SystemColors.Window;
			this.ValueCalcFindBox.BindToAmount = "CSI_Value";
			this.ValueCalcFindBox.BindToUnit = "CSI_RX_NKCurrency";
			this.ValueCalcFindBox.Decimals = 5;
			this.ValueCalcFindBox.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
			this.ValueCalcFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(47, 247, true);
			this.ValueCalcFindBox.Name = "ValueCalcFindBox";
			this.ValueCalcFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.ValueCalcFindBox.TabIndex = 19;
			// 
			// SupportingDocumentFieldsControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.AvailabilityDropEdit);
			this.Controls.Add(this.CountryCodeCodeFindBox);
			this.Controls.Add(this.IssuingAuthorityTextBox);
			this.Controls.Add(this.YearOfIssueTextBox);
			this.Controls.Add(this.LineNoCalcEdit);
			this.Controls.Add(this.ValueCalcFindBox);
			this.Name = "SupportingDocumentFieldsControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(786, 339, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.CountryCodeCodeFindBox.ResumeLayout(true);
			this.CountryCodeCodeFindBox.PerformLayout();
			this.AvailabilityDropEdit.ResumeLayout(true);
			this.AvailabilityDropEdit.PerformLayout();
			this.ValueCalcFindBox.ResumeLayout(true);
			this.ValueCalcFindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.ZTextBox YearOfIssueTextBox;
		internal ZArchitecture.ZTextBox IssuingAuthorityTextBox;
		internal ZArchitecture.GUI.ZCodeFindBox CountryCodeCodeFindBox;
		internal ZArchitecture.GUI.ZDropEdit AvailabilityDropEdit;
		internal ZArchitecture.ZCalcEdit LineNoCalcEdit;
		internal ZArchitecture.GUI.ZCalcFindBox ValueCalcFindBox;
	}
}
