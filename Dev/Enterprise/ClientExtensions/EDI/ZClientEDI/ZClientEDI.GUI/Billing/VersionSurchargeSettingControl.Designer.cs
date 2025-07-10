namespace Enterprise.Client.EDI.Billing.GUI
{
	partial class VersionSurchargeSettingControl
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
			this.percentBox = new Enterprise.ZArchitecture.ZCalcEdit();
			this.zCalcEdit1 = new Enterprise.ZArchitecture.ZCalcEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Client.EDI.Billing.Business.VersionSurchargeLicenceSetting);
			// 
			// percentBox
			// 
			this.BindingSource.SetBindingMember(this.percentBox, "LS9_Percent");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Client.EDI.Billing.Business.VersionSurchargeLicenceSetting)(null)).LS9_Percent)));
			this.percentBox.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("1b5379e0-cbfc-4281-b453-3b71d4af987b", "Surcharge % for utilising an old Version");
			this.percentBox.DecimalPlaces = 2;
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.percentBox, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			this.percentBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 22, true);
			this.percentBox.Name = "percentBox";
			this.percentBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(67, 20, true);
			this.percentBox.TabIndex = 1;
			this.percentBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// zCalcEdit1
			// 
			this.BindingSource.SetBindingMember(this.zCalcEdit1, "AdditionalPercent");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Client.EDI.Billing.Business.VersionSurchargeLicenceSetting)(null)).AdditionalPercent)));
			this.zCalcEdit1.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("e88ea329-e1fb-4e67-924e-085c319e5eef", "Addtional % for every Version released after the old Version");
			this.zCalcEdit1.DecimalPlaces = 2;
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.zCalcEdit1, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			this.zCalcEdit1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 66, true);
			this.zCalcEdit1.Name = "zCalcEdit1";
			this.zCalcEdit1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(67, 20, true);
			this.zCalcEdit1.TabIndex = 2;
			this.zCalcEdit1.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// VersionSurchargeSettingControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.zCalcEdit1);
			this.Controls.Add(this.percentBox);
			this.Name = "VersionSurchargeSettingControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(275, 154, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private ZArchitecture.ZCalcEdit percentBox;
		private ZArchitecture.ZCalcEdit zCalcEdit1;
	}
}
