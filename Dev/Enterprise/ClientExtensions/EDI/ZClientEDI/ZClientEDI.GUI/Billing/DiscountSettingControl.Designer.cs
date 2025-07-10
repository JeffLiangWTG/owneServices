namespace Enterprise.Client.EDI.Billing.GUI
{
	partial class DiscountSettingControl
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
			this.discountNameDropEdit = new Enterprise.Client.EDI.Billing.GUI.CategoryDropEdit();
			this.discountPercentBox = new Enterprise.ZArchitecture.ZCalcEdit();
			this.isActiveCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.isManualOverrideCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.discountNameDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Client.EDI.Billing.Business.DiscountLicenceSetting);
			// 
			// discountNameDropEdit
			// 
			this.discountNameDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.discountNameDropEdit, "LS9_Name");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.EDI.Billing.Business.DiscountLicenceSetting)(null)).LS9_Name)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.EDI.Billing.Business.DiscountLicenceSetting)(null)).Lookups.DiscountNamesWithCategories)));
			this.discountNameDropEdit.BindToList = "Lookups.DiscountNamesWithCategories";
			this.discountNameDropEdit.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("d6340497-eb7d-445f-8262-2deeb2bef65a", "Name");
			this.discountNameDropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.discountNameDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(63, 5, true);
			this.discountNameDropEdit.MaxItemsToShowInDropDown = 25;
			this.discountNameDropEdit.Name = "discountNameDropEdit";
			this.discountNameDropEdit.ShowDescriptionBox = false;
			this.discountNameDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 17, true);
			this.discountNameDropEdit.TabIndex = 0;
			// 
			// discountPercentBox
			// 
			this.BindingSource.SetBindingMember(this.discountPercentBox, "LS9_Percent");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Client.EDI.Billing.Business.DiscountLicenceSetting)(null)).LS9_Percent)));
			this.discountPercentBox.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("1b5379e0-cbfc-4281-b453-3b71d4af987b", "Percent");
			this.discountPercentBox.DecimalPlaces = 2;
			this.discountPercentBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(63, 31, true);
			this.discountPercentBox.Name = "discountPercentBox";
			this.discountPercentBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(67, 17, true);
			this.discountPercentBox.TabIndex = 1;
			this.discountPercentBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// isActiveCheckBox
			// 
			this.isActiveCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.isActiveCheckBox, "LS9_IsActive");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Client.EDI.Billing.Business.DiscountLicenceSetting)(null)).LS9_IsActive)));
			this.isActiveCheckBox.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("4374f000-dd88-4d80-b92d-f908dd180410", "Active?");
			this.isActiveCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.isActiveCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(63, 56, true);
			this.isActiveCheckBox.Name = "isActiveCheckBox";
			this.isActiveCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(58, 16, true);
			this.isActiveCheckBox.TabIndex = 2;
			this.isActiveCheckBox.UseVisualStyleBackColor = true;
			// 
			// isManualOverride
			// 
			this.isManualOverrideCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.isManualOverrideCheckBox, "LS9_IsManualOverride");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Client.EDI.Billing.Business.DiscountLicenceSetting)(null)).LS9_IsManualOverride)));
			this.isManualOverrideCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.isManualOverrideCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(63, 109, true);
			this.isManualOverrideCheckBox.Name = "isManualOverrideCheckBox";
			this.isManualOverrideCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(106, 16, true);
			this.isManualOverrideCheckBox.TabIndex = 4;
			this.isManualOverrideCheckBox.Text = "Manual Override?";
			this.isManualOverrideCheckBox.UseVisualStyleBackColor = true;
			// 
			// DiscountSettingControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.isManualOverrideCheckBox);
			this.Controls.Add(this.isActiveCheckBox);
			this.Controls.Add(this.discountPercentBox);
			this.Controls.Add(this.discountNameDropEdit);
			this.Name = "DiscountSettingControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(275, 154, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.discountNameDropEdit.ResumeLayout(true);
			this.discountNameDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private CategoryDropEdit discountNameDropEdit;
		private ZArchitecture.ZCalcEdit discountPercentBox;
		private ZArchitecture.GUI.ZCheckBox isActiveCheckBox;
		private ZArchitecture.GUI.ZCheckBox isManualOverrideCheckBox;
	}
}
