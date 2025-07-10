namespace Enterprise.Client.EDI.Billing.GUI
{
	partial class ConversionCreditSettingControl
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
			this.revenueEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.creditDepartmentBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.suspenseDepartmentBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.currencyBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.chargeCodeBox = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.creditDepartmentBox.SuspendLayout();
			this.suspenseDepartmentBox.SuspendLayout();
			this.currencyBox.SuspendLayout();
			this.chargeCodeBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Client.EDI.Billing.Business.ConversionCreditLicenceSetting);
			// 
			// revenueEdit
			// 
			this.BindingSource.SetBindingMember(this.revenueEdit, "LS9_Price");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Client.EDI.Billing.Business.ConversionCreditLicenceSetting)(null)).LS9_Price)));
			this.revenueEdit.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("3f21ad8a-283a-4a5d-bced-1471da59e7e2", "Revenue Discount & Currency (Per Month)");
			this.revenueEdit.DecimalPlaces = 2;
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.revenueEdit, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			this.revenueEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(15, 23, true);
			this.revenueEdit.Name = "revenueEdit";
			this.revenueEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(91, 17, true);
			this.revenueEdit.TabIndex = 0;
			this.revenueEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// creditDepartmentBox
			// 
			this.creditDepartmentBox.AllowDrop = true;
			this.creditDepartmentBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.creditDepartmentBox, "LS9_GE_Department1");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Client.EDI.Billing.Business.ConversionCreditLicenceSetting)(null)).LS9_GE_Department1)));
			this.creditDepartmentBox.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("7ab410a8-7423-418f-a94f-f5b3247ce4eb", "Credit Use Department");
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.creditDepartmentBox, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			this.creditDepartmentBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(15, 104, true);
			this.creditDepartmentBox.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(333, 0, true);
			this.creditDepartmentBox.Name = "creditDepartmentBox";
			this.creditDepartmentBox.PreBoundMaxLength = 3;
			this.creditDepartmentBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(255, 17, true);
			this.creditDepartmentBox.TabIndex = 3;
			// 
			// suspenseDepartmentBox
			// 
			this.suspenseDepartmentBox.AllowDrop = true;
			this.suspenseDepartmentBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.suspenseDepartmentBox, "LS9_GE_Department2");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Client.EDI.Billing.Business.ConversionCreditLicenceSetting)(null)).LS9_GE_Department2)));
			this.suspenseDepartmentBox.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("a7716fe6-746f-4573-aacb-c114b1b95c78", "Suspense Discount Department");
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.suspenseDepartmentBox, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			this.suspenseDepartmentBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(15, 145, true);
			this.suspenseDepartmentBox.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(333, 0, true);
			this.suspenseDepartmentBox.Name = "suspenseDepartmentBox";
			this.suspenseDepartmentBox.PreBoundMaxLength = 3;
			this.suspenseDepartmentBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(255, 17, true);
			this.suspenseDepartmentBox.TabIndex = 4;
			// 
			// CurrencyBox
			// 
			this.currencyBox.AllowDrop = true;
			this.currencyBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.currencyBox, "LS9_RX_NKPriceCurrency");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.Billing.Business.ConversionCreditLicenceSetting)(null)).LS9_RX_NKPriceCurrency)));
			this.currencyBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(116, 23, true);
			this.currencyBox.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(233, 0, true);
			this.currencyBox.Name = "CurrencyBox";
			this.currencyBox.PreBoundMaxLength = 3;
			this.currencyBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(153, 17, true);
			this.currencyBox.TabIndex = 1;
			// 
			// chargeCodeBox
			// 
			this.chargeCodeBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.chargeCodeBox, "CreditChargeCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.EDI.Billing.Business.ConversionCreditLicenceSetting)(null)).CreditChargeCode)));
			this.chargeCodeBox.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("c67134aa-bd15-4d0e-b0a3-98293ab7d3c7", "Credit Charge Code");
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.chargeCodeBox, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			this.chargeCodeBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(15, 63, true);
			this.chargeCodeBox.Name = "chargeCodeBox";
			this.chargeCodeBox.PreBoundMaxLength = 25;
			this.chargeCodeBox.ShowDescriptionBox = false;
			this.chargeCodeBox.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			this.chargeCodeBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(170, 17, true);
			this.chargeCodeBox.TabIndex = 2;
			// 
			// ConversionCreditSettingControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.chargeCodeBox);
			this.Controls.Add(this.currencyBox);
			this.Controls.Add(this.suspenseDepartmentBox);
			this.Controls.Add(this.creditDepartmentBox);
			this.Controls.Add(this.revenueEdit);
			this.Name = "ConversionCreditSettingControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(284, 169, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.creditDepartmentBox.ResumeLayout(true);
			this.creditDepartmentBox.PerformLayout();
			this.suspenseDepartmentBox.ResumeLayout(true);
			this.suspenseDepartmentBox.PerformLayout();
			this.currencyBox.ResumeLayout(true);
			this.currencyBox.PerformLayout();
			this.chargeCodeBox.ResumeLayout(true);
			this.chargeCodeBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZCalcEdit revenueEdit;
		private ZArchitecture.GUI.ZGuidFindBox creditDepartmentBox;
		private ZArchitecture.GUI.ZGuidFindBox suspenseDepartmentBox;
		private ZArchitecture.GUI.ZCodeFindBox currencyBox;
		private ZArchitecture.GUI.ZDropEdit chargeCodeBox;
	}
}
