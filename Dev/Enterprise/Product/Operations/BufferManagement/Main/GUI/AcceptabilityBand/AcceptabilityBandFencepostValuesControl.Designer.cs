namespace Enterprise.BufferManagement.GUI
{
	partial class AcceptabilityBandFencepostValuesControl
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
			this.BoundaryValuesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.BoundaryHintLabel = new Enterprise.ZArchitecture.ZLabel();
			this.MinCautionCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.MaxCautionCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.MinGoodCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.MaxGoodCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.MinExcellentCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.MaxExcellentCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.BoundaryValuesGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.BufferManagement.Business.BMComponentAcceptabilityBand);
			// 
			// BoundaryValuesGroupBox
			// 
			this.BoundaryValuesGroupBox.CaptionResourceString = Enterprise.BufferManagement.GUI.Res.GetData("92d701c0-f382-48a4-95c4-4838b7795c4d", "Boundary Values");
			this.BoundaryValuesGroupBox.Controls.Add(this.BoundaryHintLabel);
			this.BoundaryValuesGroupBox.Controls.Add(this.MinCautionCalcEdit);
			this.BoundaryValuesGroupBox.Controls.Add(this.MaxCautionCalcEdit);
			this.BoundaryValuesGroupBox.Controls.Add(this.MinGoodCalcEdit);
			this.BoundaryValuesGroupBox.Controls.Add(this.MaxGoodCalcEdit);
			this.BoundaryValuesGroupBox.Controls.Add(this.MinExcellentCalcEdit);
			this.BoundaryValuesGroupBox.Controls.Add(this.MaxExcellentCalcEdit);
			this.BoundaryValuesGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.BoundaryValuesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.BoundaryValuesGroupBox.Name = "BoundaryValuesGroupBox";
			this.BoundaryValuesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(720, 81, true);
			this.BoundaryValuesGroupBox.TabIndex = 15;
			this.BoundaryValuesGroupBox.TabStop = false;
			// 
			// BoundaryHintLabel
			// 
			this.BoundaryHintLabel.AutoSize = true;
			this.BindingSource.SetBindingMember(this.BoundaryHintLabel, "BoundaryValuesHintLabel");
			this.BoundaryHintLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 19, true);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.BufferManagement.Business.BMComponentAcceptabilityBand)(null)).BoundaryValuesHintLabel)));
			this.BoundaryHintLabel.Name = "BoundaryHintLabel";
			this.BoundaryHintLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(355, 13, true);
			this.BoundaryHintLabel.TabIndex = 14;
			// 
			// MinCautionCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.MinCautionCalcEdit, "BAB_CautionLowerBound");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.BufferManagement.Business.BMComponentAcceptabilityBand)(null)).BAB_CautionLowerBound)));
			this.MinCautionCalcEdit.DecimalPlaces = 2;
			this.MinCautionCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(77, 41, true);
			this.MinCautionCalcEdit.Name = "MinCautionCalcEdit";
			this.MinCautionCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 20, true);
			this.MinCautionCalcEdit.TabIndex = 8;
			this.MinCautionCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// MaxCautionCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.MaxCautionCalcEdit, "BAB_CautionUpperBound");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.BufferManagement.Business.BMComponentAcceptabilityBand)(null)).BAB_CautionUpperBound)));
			this.MaxCautionCalcEdit.DecimalPlaces = 2;
			this.MaxCautionCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(652, 41, true);
			this.MaxCautionCalcEdit.Name = "MaxCautionCalcEdit";
			this.MaxCautionCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 20, true);
			this.MaxCautionCalcEdit.TabIndex = 13;
			this.MaxCautionCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// MinGoodCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.MinGoodCalcEdit, "BAB_GoodLowerBound");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.BufferManagement.Business.BMComponentAcceptabilityBand)(null)).BAB_GoodLowerBound)));
			this.MinGoodCalcEdit.DecimalPlaces = 2;
			this.MinGoodCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(181, 41, true);
			this.MinGoodCalcEdit.Name = "MinGoodCalcEdit";
			this.MinGoodCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 20, true);
			this.MinGoodCalcEdit.TabIndex = 9;
			this.MinGoodCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// MaxGoodCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.MaxGoodCalcEdit, "BAB_GoodUpperBound");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.BufferManagement.Business.BMComponentAcceptabilityBand)(null)).BAB_GoodUpperBound)));
			this.MaxGoodCalcEdit.DecimalPlaces = 2;
			this.MaxGoodCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(535, 41, true);
			this.MaxGoodCalcEdit.Name = "MaxGoodCalcEdit";
			this.MaxGoodCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 20, true);
			this.MaxGoodCalcEdit.TabIndex = 12;
			this.MaxGoodCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// MinExcellentCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.MinExcellentCalcEdit, "BAB_ExcellentLowerBound");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.BufferManagement.Business.BMComponentAcceptabilityBand)(null)).BAB_ExcellentLowerBound)));
			this.MinExcellentCalcEdit.DecimalPlaces = 2;
			this.MinExcellentCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(302, 41, true);
			this.MinExcellentCalcEdit.Name = "MinExcellentCalcEdit";
			this.MinExcellentCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 20, true);
			this.MinExcellentCalcEdit.TabIndex = 10;
			this.MinExcellentCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// MaxExcellentCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.MaxExcellentCalcEdit, "BAB_ExcellentUpperBound");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.BufferManagement.Business.BMComponentAcceptabilityBand)(null)).BAB_ExcellentUpperBound)));
			this.MaxExcellentCalcEdit.DecimalPlaces = 2;
			this.MaxExcellentCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(427, 41, true);
			this.MaxExcellentCalcEdit.Name = "MaxExcellentCalcEdit";
			this.MaxExcellentCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 20, true);
			this.MaxExcellentCalcEdit.TabIndex = 11;
			this.MaxExcellentCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// AcceptabilityBandFencepostValuesControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.BoundaryValuesGroupBox);
			this.Name = "AcceptabilityBandFencepostValuesControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(720, 81, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.BoundaryValuesGroupBox.ResumeLayout(false);
			this.BoundaryValuesGroupBox.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox BoundaryValuesGroupBox;
		private ZArchitecture.ZLabel BoundaryHintLabel;
		private ZArchitecture.ZCalcEdit MinCautionCalcEdit;
		private ZArchitecture.ZCalcEdit MaxCautionCalcEdit;
		private ZArchitecture.ZCalcEdit MinGoodCalcEdit;
		private ZArchitecture.ZCalcEdit MaxGoodCalcEdit;
		private ZArchitecture.ZCalcEdit MinExcellentCalcEdit;
		private ZArchitecture.ZCalcEdit MaxExcellentCalcEdit;
	}
}
