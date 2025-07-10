namespace Enterprise.Client.EDI.Billing.GUI
{
	partial class BorderWisePurchasedLicenceControl
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
			this.licenceCountEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.discountNameDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.discountNameDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Client.EDI.Billing.Business.BorderWisePurchasedLicenceSetting);
			// 
			// licenceCountEdit
			// 
			this.BindingSource.SetBindingMember(this.licenceCountEdit, "LicenceCount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Client.EDI.Billing.Business.BorderWisePurchasedLicenceSetting)(null)).LicenceCount)));
			this.licenceCountEdit.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("8471b63d-d460-4c0d-ad98-c27fb3e5fcaf", "Licenses");
			this.licenceCountEdit.DecimalPlaces = 0;
			this.licenceCountEdit.Decimals = 0;
			this.licenceCountEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(62, 32, true);
			this.licenceCountEdit.Name = "licenceCountEdit";
			this.licenceCountEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(67, 17, true);
			this.licenceCountEdit.TabIndex = 0;
			this.licenceCountEdit.Text = "0";
			this.licenceCountEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// discountNameDropEdit
			// 
			this.discountNameDropEdit.AllowDrop = true;
			this.discountNameDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.discountNameDropEdit, "PriceCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.EDI.Billing.Business.BorderWisePurchasedLicenceSetting)(null)).PriceCode)));
			this.discountNameDropEdit.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("6169b462-b051-43bc-9ad6-daf9bf15ae7a", "Feature");
			this.discountNameDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(62, 7, true);
			this.discountNameDropEdit.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(333, 0, true);
			this.discountNameDropEdit.MaxItemsToShowInDropDown = 40;
			this.discountNameDropEdit.Name = "discountNameDropEdit";
			this.discountNameDropEdit.PreBoundMaxLength = 4;
			this.discountNameDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 17, true);
			this.discountNameDropEdit.TabIndex = 3;
			// 
			// BorderWiseLegacyLicenceControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.discountNameDropEdit);
			this.Controls.Add(this.licenceCountEdit);
			this.Name = "BorderWiseLegacyLicenceControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(305, 145, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.discountNameDropEdit.ResumeLayout(true);
			this.discountNameDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZCalcEdit licenceCountEdit;
		private ZArchitecture.GUI.ZDropEdit discountNameDropEdit;
	}
}
