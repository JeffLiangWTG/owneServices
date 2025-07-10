using Enterprise.ZArchitecture.Core;

namespace Enterprise.Registry.GUI
{
	partial class StringEffectiveDateControl
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
			this.PreviousValueTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.NewValueTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.EffectiveDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.EffectiveDateEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Registry.Business.StringEffectiveDate);
			// 
			// PreviousValueTextBox
			// 
			this.BindingSource.SetBindingMember(this.PreviousValueTextBox, "PreviousValue");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.StringEffectiveDate)(null)).PreviousValue)));
			this.PreviousValueTextBox.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("StringEffectiveDateControl|BCABF1BD-D62C-4EA2-B188-5CFE425BB406", "Previous Value");
			this.PreviousValueTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.PreviousValueTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(101, 15, true);
			this.PreviousValueTextBox.Name = "PreviousValueTextBox";
			this.PreviousValueTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 20, true);
			this.PreviousValueTextBox.TabIndex = 1;
			// 
			// NewValueTextBox
			// 
			this.BindingSource.SetBindingMember(this.NewValueTextBox, "NewValue");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.StringEffectiveDate)(null)).NewValue)));
			this.NewValueTextBox.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("StringEffectiveDateControl|CE5A8C50-7D24-4956-B9B9-B178E7853128", "New Value");
			this.NewValueTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.NewValueTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(101, 45, true);
			this.NewValueTextBox.Name = "NewValueTextBox";
			this.NewValueTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 20, true);
			this.NewValueTextBox.TabIndex = 2;
			// 
			// EffectiveDateEdit
			// 
			this.EffectiveDateEdit.AllowDrop = true;
			this.EffectiveDateEdit.AutoCompleteMonthThreshold = 1;
			this.EffectiveDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.EffectiveDateEdit, "EffectiveDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Registry.Business.StringEffectiveDate)(null)).EffectiveDate)));
			this.EffectiveDateEdit.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("StringEffectiveDateControl|5E74686D-1E62-473A-9246-AF360AFF9380", "Effective Date");
			this.EffectiveDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(101, 70, true);
			this.EffectiveDateEdit.Name = "EffectiveDateEdit";
			this.EffectiveDateEdit.TabIndex = 3;
			// 
			// StringEffectiveDateControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.EffectiveDateEdit);
			this.Controls.Add(this.NewValueTextBox);
			this.Controls.Add(this.PreviousValueTextBox);
			this.Name = "StringEffectiveDateControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(428, 93, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.EffectiveDateEdit.ResumeLayout(true);
			this.EffectiveDateEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Enterprise.ZArchitecture.ZTextBox PreviousValueTextBox;
		private Enterprise.ZArchitecture.ZTextBox NewValueTextBox;
		protected internal Enterprise.ZArchitecture.GUI.ZDateEdit EffectiveDateEdit;
	}
}
