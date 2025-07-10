using Enterprise.ZArchitecture.Core;

namespace Enterprise.Registry.GUI
{
	partial class DecimalEffectiveDateControl
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
			this.PreviousValueCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.NewValueCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.EffectiveDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Registry.Business.DecimalEffectiveDate);
			// 
			// PreviousValueCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.PreviousValueCalcEdit, "PreviousValue");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Registry.Business.DecimalEffectiveDate)(null)).PreviousValue)));
			this.PreviousValueCalcEdit.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("DecimalEffectiveDateControl|f273667d-9a00-4352-836d-48bbf9d36ebb", "Previous Value");
			this.PreviousValueCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(101, 15, true);
			this.PreviousValueCalcEdit.Name = "PreviousValueCalcEdit";
			this.PreviousValueCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.PreviousValueCalcEdit.TabIndex = 1;
			this.PreviousValueCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// NewValueCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.NewValueCalcEdit, "NewValue");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Registry.Business.DecimalEffectiveDate)(null)).NewValue)));
			this.NewValueCalcEdit.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("DecimalEffectiveDateControl|59a00206-57f9-4ae1-8dff-e0e888be662b", "New Value");
			this.NewValueCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(101, 45, true);
			this.NewValueCalcEdit.Name = "NewValueCalcEdit";
			this.NewValueCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.NewValueCalcEdit.TabIndex = 2;
			this.NewValueCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// EffectiveDateEdit
			// 
			this.EffectiveDateEdit.AutoCompleteMonthThreshold = 1;
			this.EffectiveDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.EffectiveDateEdit, "EffectiveDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Registry.Business.DecimalEffectiveDate)(null)).EffectiveDate)));
			this.EffectiveDateEdit.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("DecimalEffectiveDateControl|8b47ef26-ff36-4fd4-841e-05082626b3b9", "Effective Date");
			this.EffectiveDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(297, 45, true);
			this.EffectiveDateEdit.Name = "EffectiveDateEdit";
			this.EffectiveDateEdit.TabIndex = 3;
			// 
			// DecimalEffectiveDateControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.EffectiveDateEdit);
			this.Controls.Add(this.NewValueCalcEdit);
			this.Controls.Add(this.PreviousValueCalcEdit);
			this.Name = "DecimalEffectiveDateControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(428, 83, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Enterprise.ZArchitecture.ZCalcEdit PreviousValueCalcEdit;
		private Enterprise.ZArchitecture.ZCalcEdit NewValueCalcEdit;
		protected internal Enterprise.ZArchitecture.GUI.ZDateEdit EffectiveDateEdit;
	}
}
