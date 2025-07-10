using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.Registry.GUI
{
	partial class EnforceZeroBalanceDisbursementsConfigurationControl
	{


		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
		}

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.validationType = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.maximumVarianceHelpText = new Enterprise.ZArchitecture.ZLabel();
			this.maximumVariance = new Enterprise.ZArchitecture.ZCalcEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.validationType.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Accounting.Registry.Business.EnforceZeroBalanceDisbursementsConfiguration);
			// 
			// validationType
			// 
			this.validationType.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.validationType, "EnforceZeroBalanceDisbursementsValidationType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Registry.Business.EnforceZeroBalanceDisbursementsConfiguration)(null)).EnforceZeroBalanceDisbursementsValidationType)));
			this.validationType.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("929B702F-C754-44D0-9FD8-9E2FF4E0BC84", " ");
			this.validationType.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 3, true);
			this.validationType.Name = "validationType";
			this.validationType.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(400, 20, true);
			this.validationType.TabIndex = 0;
			this.validationType.SelectedIndexChanged += new System.EventHandler(this.SelectionChanged);
			// 
			// maximumVarianceHelpText
			// 
			this.maximumVarianceHelpText.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("D81E158F-3F7C-4EDD-AC72-DD37A76F37FC", "Maximum Exchange Variance Allowed:\r\nWhen enforcing Zero Balance Disbursements using LOC, EIT and BTH, there needs to be an allowance for rounding on exchange variance. \r\nSpecify the maximum acceptable rounding variance allowed. This is a monetary value. \r\nExcept in the instance of BTH, either the OS or local values can be manually manipulated to balance to zero.\r\nIt is recommended that this value is not set above the default value. A larger allowed variance may result in unacceptable differences between local cost and local sell values.");
			this.maximumVarianceHelpText.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.maximumVarianceHelpText.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 27, true);
			this.maximumVarianceHelpText.Name = "maximumVarianceHelpText";
			this.maximumVarianceHelpText.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(450, 118, true);
			this.maximumVarianceHelpText.TabIndex = 0;
			// 
			// maximumVariance
			// 
			this.maximumVariance.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.maximumVariance, "MaximumVariance");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDecimal)(((Enterprise.Accounting.Registry.Business.EnforceZeroBalanceDisbursementsConfiguration)(null)).MaximumVariance)));
			this.maximumVariance.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("EnforceZeroBalanceDisbursementsConfiguration|14DFC92B-85A4-424E-BEF3-31F4BCA04A08", "Maximum Exchange Variance Allowed");
			this.maximumVariance.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(188, 148, true);
			this.maximumVariance.Name = "maximumVariance";
			this.maximumVariance.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 20, true);
			this.maximumVariance.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.maximumVariance.TabIndex = 0;
			// 
			// EnforceZeroBalanceDisbursementsConfigurationControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.validationType);
			this.Controls.Add(this.maximumVarianceHelpText);
			this.Controls.Add(this.maximumVariance);
			this.Name = "EnforceZeroBalanceDisbursementsConfigurationControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(477, 208, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.validationType.ResumeLayout(true);
			this.validationType.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		/// <summary> 
		/// Required designer variable.
		/// </summary>

		private ZDropEdit validationType;
		private ZCalcEdit maximumVariance;
		private ZLabel maximumVarianceHelpText;
	}
}