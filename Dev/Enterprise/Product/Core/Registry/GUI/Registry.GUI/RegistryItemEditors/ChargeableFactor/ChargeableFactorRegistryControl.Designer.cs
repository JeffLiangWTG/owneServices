namespace Enterprise.Registry.GUI
{
	partial class ChargeableFactorRegistryControl
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
			this.zMetricFactorDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.zImperialFactorDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Registry.Business.ChargeableFactor);
			// 
			// zMetricFactorCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.zMetricFactorDropEdit, "MetricFactorForBinding.ConversionFactorString");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.ChargeableFactor)(null)).MetricFactorForBinding.ConversionFactorString)));
			this.zMetricFactorDropEdit.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("ChargeableFactorRegistryControl|76f1318d-f4fc-4853-a133-7fb41fedea4a", "Metric Factor");
			this.zMetricFactorDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(133, 4, true);
			this.zMetricFactorDropEdit.Name = "zMetricFactorDropEdit";
			this.zMetricFactorDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.zMetricFactorDropEdit.TabIndex = 0;
			this.zMetricFactorDropEdit.Text = "0";
			this.zMetricFactorDropEdit.ShowDescriptionBox = false;
			// 
			// zImperialFactorCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.zImperialFactorDropEdit, "ImperialFactorForBinding.ConversionFactorString");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.ChargeableFactor)(null)).ImperialFactorForBinding.ConversionFactorString)));
			this.zImperialFactorDropEdit.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("ChargeableFactorRegistryControl|25ca6952-c14e-4d60-8816-47cb6762fa2d", "Imperial Factor");
			this.zImperialFactorDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(133, 30, true);
			this.zImperialFactorDropEdit.Name = "zImperialFactorDropEdit";
			this.zImperialFactorDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.zImperialFactorDropEdit.TabIndex = 1;
			this.zImperialFactorDropEdit.Text = "0";
			this.zImperialFactorDropEdit.ShowDescriptionBox = false;
			// 
			// ChargeableFactorRegistryControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.zImperialFactorDropEdit);
			this.Controls.Add(this.zMetricFactorDropEdit);
			this.Name = "ChargeableFactorRegistryControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(259, 58, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal Enterprise.ZArchitecture.GUI.ZDropEdit zMetricFactorDropEdit;
		internal Enterprise.ZArchitecture.GUI.ZDropEdit zImperialFactorDropEdit;
	}
}
