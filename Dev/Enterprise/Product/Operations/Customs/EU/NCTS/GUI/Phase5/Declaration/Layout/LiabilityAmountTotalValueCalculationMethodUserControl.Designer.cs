namespace Enterprise.Customs.EU.NCTS.GUI
{
	partial class LiabilityAmountTotalValueCalculationMethodUserControl
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
			this.UseDutiesAndTaxesCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.UseMonetaryValueCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.NCTS.Business.CalculateLiabilityBizObj);
			// 
			// UseDutiesAndTaxesCheckBox
			// 
			this.UseDutiesAndTaxesCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.UseDutiesAndTaxesCheckBox, "UseDutiesAndTaxes");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.EU.NCTS.Business.CalculateLiabilityBizObj)(null)).UseDutiesAndTaxes)));
			this.UseDutiesAndTaxesCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 4, true);
			this.UseDutiesAndTaxesCheckBox.Name = "UseDutiesAndTaxesCheckBox";
			this.UseDutiesAndTaxesCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.UseDutiesAndTaxesCheckBox.TabIndex = 0;
			this.UseDutiesAndTaxesCheckBox.UseVisualStyleBackColor = true;
			// 
			// UseMonetaryValueCheckBox
			// 
			this.UseMonetaryValueCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.UseMonetaryValueCheckBox, "UseMonetaryValue");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.EU.NCTS.Business.CalculateLiabilityBizObj)(null)).UseMonetaryValue)));
			this.UseMonetaryValueCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 4, true);
			this.UseMonetaryValueCheckBox.Name = "UseMonetaryValueCheckBox";
			this.UseMonetaryValueCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.UseMonetaryValueCheckBox.TabIndex = 1;
			this.UseMonetaryValueCheckBox.UseVisualStyleBackColor = true;
			// 
			// LiabilityAmountTotalValueCalculationMethodUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.UseMonetaryValueCheckBox);
			this.Controls.Add(this.UseDutiesAndTaxesCheckBox);
			this.Name = "LiabilityAmountTotalValueCalculationMethodUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(305, 33, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.GUI.ZCheckBox UseDutiesAndTaxesCheckBox;
		internal ZArchitecture.GUI.ZCheckBox UseMonetaryValueCheckBox;
	}
}
