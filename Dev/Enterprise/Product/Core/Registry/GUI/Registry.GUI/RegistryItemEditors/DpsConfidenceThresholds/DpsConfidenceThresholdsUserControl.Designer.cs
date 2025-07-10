using Enterprise.ZArchitecture;

namespace Enterprise.Registry.GUI
{
	partial class DpsConfidenceThresholdsUserControl
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
			this.mediumThresholdCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.highThresholdCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Registry.Business.DpsConfidenceThresholdsBusinessObject);
			// 
			// mediumThresholdCalcEdit
			// 
			this.mediumThresholdCalcEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.mediumThresholdCalcEdit, "MediumThreshold");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Registry.Business.DpsConfidenceThresholdsBusinessObject)(null)).MediumThreshold)));
			this.mediumThresholdCalcEdit.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("1f344b06-6235-4d88-9a3f-cf7f7fc720c1", "Medium Confidence Threshold (%)");
			this.mediumThresholdCalcEdit.DecimalPlaces = 0;
			this.mediumThresholdCalcEdit.Decimals = 0;
			this.mediumThresholdCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(180, 13, true);
			this.mediumThresholdCalcEdit.Name = "mediumThresholdCalcEdit";
			this.mediumThresholdCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(65, 17, true);
			this.mediumThresholdCalcEdit.TabIndex = 0;
			this.mediumThresholdCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.mediumThresholdCalcEdit.MaxValue = 99;
			this.mediumThresholdCalcEdit.AllowNegative = false;
			// 
			// highThresholdCalcEdit
			// 
			this.highThresholdCalcEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.highThresholdCalcEdit, "HighThreshold");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Registry.Business.DpsConfidenceThresholdsBusinessObject)(null)).HighThreshold)));
			this.highThresholdCalcEdit.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("5686657c-5477-4bda-97f0-b35e19aaaa64", "High Confidence Threshold (%)");
			this.highThresholdCalcEdit.DecimalPlaces = 0;
			this.highThresholdCalcEdit.Decimals = 0;
			this.highThresholdCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(180, 35, true);
			this.highThresholdCalcEdit.Name = "highThresholdCalcEdit";
			this.highThresholdCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(65, 17, true);
			this.highThresholdCalcEdit.TabIndex = 1;
			this.highThresholdCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.highThresholdCalcEdit.MaxValue = 99;
			this.highThresholdCalcEdit.AllowNegative = false;
			// 
			// DpsConfidenceThresholdsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.mediumThresholdCalcEdit);
			this.Controls.Add(this.highThresholdCalcEdit);
			this.Name = "DpsConfidenceThresholdsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(400, 240, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		ZCalcEdit highThresholdCalcEdit;
		ZCalcEdit mediumThresholdCalcEdit;

		#endregion
	}
}
