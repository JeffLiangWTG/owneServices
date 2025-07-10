namespace Enterprise.Client.EDI.Billing.GUI
{
	partial class SingleCountryDiscountControl
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
			this.countryFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.countryFindBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Client.EDI.Billing.Business.SingleCountryDiscount);
			// 
			// countryFindBox
			// 
			this.countryFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.countryFindBox, "Country");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.Billing.Business.SingleCountryDiscount)(null)).Country)));
			this.countryFindBox.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("03d74114-4677-488c-80ca-998dc2b18b66", "Country/Region");
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.countryFindBox, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			this.countryFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(14, 23, true);
			this.countryFindBox.Name = "countryFindBox";
			this.countryFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.countryFindBox.ParentType = null;
			this.countryFindBox.PreBoundMaxLength = 2;
			this.countryFindBox.ShouldAddFetchHints = false;
			this.countryFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(234, 20, true);
			this.countryFindBox.TabIndex = 2;
			// 
			// SingleCountryDiscountControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.countryFindBox);
			this.Name = "SingleCountryDiscountControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(251, 56, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.countryFindBox.ResumeLayout(true);
			this.countryFindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZCodeFindBox countryFindBox;
	}
}
