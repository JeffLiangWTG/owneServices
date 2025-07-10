namespace Enterprise.Client.EDI.Billing.GUI
{
	partial class MinSpendSettingControl
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
			this.priceEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.priceItemCodeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.categoryCodeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.InfoLinkLabel = new Enterprise.ZArchitecture.GUI.ZLinkLabel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.priceItemCodeDropEdit.SuspendLayout();
			this.categoryCodeDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Client.EDI.Billing.Business.MinSpendLicenceSetting);
			// 
			// priceEdit
			// 
			this.BindingSource.SetBindingMember(this.priceEdit, "LS9_Price");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Client.EDI.Billing.Business.MinSpendLicenceSetting)(null)).LS9_Price)));
			this.priceEdit.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("e8421ab7-8364-479c-9bd9-584e25fea64c", "Min. Spend");
			this.priceEdit.DecimalPlaces = 2;
			this.priceEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(89, 59, true);
			this.priceEdit.Name = "priceEdit";
			this.priceEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(67, 20, true);
			this.priceEdit.TabIndex = 2;
			this.priceEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// priceItemCodeDropEdit
			// 
			this.priceItemCodeDropEdit.AllowDrop = true;
			this.priceItemCodeDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.priceItemCodeDropEdit, "PriceCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.EDI.Billing.Business.MinSpendLicenceSetting)(null)).PriceCode)));
			this.priceItemCodeDropEdit.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("b95ace57-3818-4659-a8d4-b05c31b49e60", "Feature");
			this.priceItemCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(89, 31, true);
			this.priceItemCodeDropEdit.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(333, 0, true);
			this.priceItemCodeDropEdit.MaxItemsToShowInDropDown = 40;
			this.priceItemCodeDropEdit.Name = "priceItemCodeDropEdit";
			this.priceItemCodeDropEdit.PreBoundMaxLength = 3;
			this.priceItemCodeDropEdit.ShouldResizeByMaxLength = true;
			this.priceItemCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(235, 20, true);
			this.priceItemCodeDropEdit.TabIndex = 1;
			// 
			// categoryCodeDropEdit
			// 
			this.categoryCodeDropEdit.AllowDrop = true;
			this.categoryCodeDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.categoryCodeDropEdit, "PriceCategory");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.EDI.Billing.Business.MinSpendLicenceSetting)(null)).PriceCategory)));
			this.categoryCodeDropEdit.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("86805fb5-e24c-4829-9854-684d15db3e0b", "Category");
			this.categoryCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(89, 3, true);
			this.categoryCodeDropEdit.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(333, 0, true);
			this.categoryCodeDropEdit.MaxItemsToShowInDropDown = 40;
			this.categoryCodeDropEdit.Name = "categoryCodeDropEdit";
			this.categoryCodeDropEdit.PreBoundMaxLength = 3;
			this.categoryCodeDropEdit.ShouldResizeByMaxLength = true;
			this.categoryCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(235, 20, true);
			this.categoryCodeDropEdit.TabIndex = 0;
			// 
			// InfoLinkLabel
			// 
			this.InfoLinkLabel.AutoSize = true;
			this.InfoLinkLabel.IsFontBold = false;
			this.InfoLinkLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(91, 97, true);
			this.InfoLinkLabel.Name = "InfoLinkLabel";
			this.InfoLinkLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(65, 13, true);
			this.InfoLinkLabel.TabIndex = 16;
			this.InfoLinkLabel.Text = "What\'s this?";
			this.InfoLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.InfoLinkLabel_LinkClicked);
			// 
			// MinSpendSettingControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("bfbfc77d-8b16-41f3-9e61-f695c5a28e89", "Apply Discounts");
			this.Controls.Add(this.InfoLinkLabel);
			this.Controls.Add(this.categoryCodeDropEdit);
			this.Controls.Add(this.priceItemCodeDropEdit);
			this.Controls.Add(this.priceEdit);
			this.Name = "MinSpendSettingControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(332, 160, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.priceItemCodeDropEdit.ResumeLayout(true);
			this.priceItemCodeDropEdit.PerformLayout();
			this.categoryCodeDropEdit.ResumeLayout(true);
			this.categoryCodeDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZCalcEdit priceEdit;
		private ZArchitecture.GUI.ZDropEdit priceItemCodeDropEdit;
		private ZArchitecture.GUI.ZDropEdit categoryCodeDropEdit;
		private ZArchitecture.GUI.ZLinkLabel InfoLinkLabel;
	}
}
