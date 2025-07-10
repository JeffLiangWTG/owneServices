using CargoWiseOne.ResourceStrings;

namespace Enterprise.Registry.GUI
{
	partial class CalculateDeliveryDateWithExceptionsOptionsRegistryItemControl
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
			this.MaximumDurationCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.UnlimitedDurationCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.zLabelLeft = new Enterprise.ZArchitecture.ZLabel();
			this.zLabelRight = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Registry.Business.CalculateDeliveryDateWithExceptionsOptions);
			// 
			// MaximumDurationCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.MaximumDurationCalcEdit, "MaximumDurationHours");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Registry.Business.CalculateDeliveryDateWithExceptionsOptions)(null)).MaximumDurationHours)));
			this.MaximumDurationCalcEdit.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("0ABC3E48-EBA1-433B-87B1-2B44A59B2011", "Maximum Combined Delay Duration");
			this.MaximumDurationCalcEdit.DecimalPlaces = 0;
			this.MaximumDurationCalcEdit.Decimals = 0;
			this.MaximumDurationCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(275, 39, true);
			this.MaximumDurationCalcEdit.Name = "MaximumDurationCalcEdit";
			this.MaximumDurationCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(30, 17, true);
			this.MaximumDurationCalcEdit.TabIndex = 2;
			this.MaximumDurationCalcEdit.Text = "0";
			this.MaximumDurationCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.MaximumDurationCalcEdit.TrackDisposedAccess = true;
			// 
			// UnlimitedDurationHoursCheckBox
			// 
			this.BindingSource.SetBindingMember(this.UnlimitedDurationCheckBox, "UnlimitedDuration");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Registry.Business.CalculateDeliveryDateWithExceptionsOptions)(null)).UnlimitedDuration)));
			this.UnlimitedDurationCheckBox.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("9403F84E-AF41-40B5-8F19-F95CFB973AF9", "Unlimited Delay Duration");
			this.UnlimitedDurationCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 9, true);
			this.UnlimitedDurationCheckBox.Name = "UnlimitedDurationCheckBox";
			this.UnlimitedDurationCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(211, 24, true);
			this.UnlimitedDurationCheckBox.TabIndex = 1;
			// 
			// zLabelLeft
			// 
			this.zLabelLeft.AutoSize = true;
			this.zLabelLeft.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("a833cb76-da63-4b8e-9472-decee81e3365", "Maximum combined delay duration per 1 calendar day: ");
			this.zLabelLeft.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabelLeft.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 39, true);
			this.zLabelLeft.Name = "zLabelLeft";
			this.zLabelLeft.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(263, 13, true);
			this.zLabelLeft.TabIndex = 1;
			this.zLabelLeft.UseMnemonic = false;
			// 
			// zLabelRight
			// 
			this.zLabelRight.AutoSize = true;
			this.zLabelRight.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("b833ca76-da63-4b8e-9441-decee81e3446", "Hours");
			this.zLabelRight.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabelRight.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(310, 39, true);
			this.zLabelRight.Name = "zLabelRight";
			this.zLabelRight.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(35, 13, true);
			this.zLabelRight.TabIndex = 3;
			this.zLabelRight.UseMnemonic = false;
			// 
			// CalculateDeliveryDateWithExceptionsOptionsRegistryItemControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.zLabelLeft);
			this.Controls.Add(this.MaximumDurationCalcEdit);
			this.Controls.Add(this.zLabelRight);
			this.Controls.Add(this.UnlimitedDurationCheckBox);
			this.Name = "CalculateDeliveryDateWithExceptionsOptionsRegistryItemControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(351, 63, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		Enterprise.ZArchitecture.ZCalcEdit MaximumDurationCalcEdit;
		ZArchitecture.GUI.ZCheckBox UnlimitedDurationCheckBox;
		private ZArchitecture.ZLabel zLabelLeft;
		private ZArchitecture.ZLabel zLabelRight;

		#endregion
	}
}
