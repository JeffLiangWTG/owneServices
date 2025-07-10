namespace Enterprise.Client.EDI.Billing.GUI
{
	partial class WiseCloudDiscountControl
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
			this.expiryMonthsEditbox = new Enterprise.ZArchitecture.ZCalcEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Client.EDI.Billing.Business.WiseCloudDiscount);
			// 
			// expiryMonthsEditbox
			// 
			this.BindingSource.SetBindingMember(this.expiryMonthsEditbox, "ExpiryMonthsFromAgreedGoLive");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Client.EDI.Billing.Business.WiseCloudDiscount)(null)).ExpiryMonthsFromAgreedGoLive)));
			this.expiryMonthsEditbox.DecimalPlaces = 2;
			this.expiryMonthsEditbox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(203, 18, true);
			this.expiryMonthsEditbox.MaxValue = new decimal(new int[] {
            9999,
            0,
            0,
            0});
			this.expiryMonthsEditbox.Name = "expiryMonthsEditbox";
			this.expiryMonthsEditbox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(43, 20, true);
			this.expiryMonthsEditbox.TabIndex = 3;
			this.expiryMonthsEditbox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.expiryMonthsEditbox.DecimalPlaces = 0;
			this.expiryMonthsEditbox.Decimals = 0;
			// 
			// WiseCloudDiscountControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.expiryMonthsEditbox);
			this.Name = "WiseCloudDiscountControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(251, 56, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Enterprise.ZArchitecture.ZCalcEdit expiryMonthsEditbox;
	}
}
