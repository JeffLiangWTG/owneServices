namespace Enterprise.Client.EDI.Billing.GUI
{
	partial class HighVolumeFeatureSettingControl
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
			this.discountNameDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.categoryDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.discountNameDropEdit.SuspendLayout();
			this.categoryDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Client.EDI.Billing.Business.HighVolumeFeatureSetting);
			// 
			// discountNameDropEdit
			// 
			this.discountNameDropEdit.AllowDrop = true;
			this.discountNameDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.discountNameDropEdit, "PriceCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.EDI.Billing.Business.HighVolumeFeatureSetting)(null)).PriceCode)));
			this.discountNameDropEdit.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("d965f99c-11bb-4317-983a-687e03459991", "Feature");
			this.discountNameDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(62, 31, true);
			this.discountNameDropEdit.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(333, 0, true);
			this.discountNameDropEdit.MaxItemsToShowInDropDown = 40;
			this.discountNameDropEdit.Name = "discountNameDropEdit";
			this.discountNameDropEdit.PreBoundMaxLength = 3;
			this.discountNameDropEdit.ShouldResizeByMaxLength = true;
			this.discountNameDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 17, true);
			this.discountNameDropEdit.TabIndex = 2;
			// 
			// categoryDropEdit
			// 
			this.categoryDropEdit.AllowDrop = true;
			this.categoryDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.categoryDropEdit, "PriceCategory");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.EDI.Billing.Business.HighVolumeFeatureSetting)(null)).PriceCategory)));
			this.categoryDropEdit.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("ecc08484-79ee-4d09-aa6e-e8202dbee7c5", "Category");
			this.categoryDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(62, 3, true);
			this.categoryDropEdit.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(333, 0, true);
			this.categoryDropEdit.MaxItemsToShowInDropDown = 40;
			this.categoryDropEdit.Name = "categoryDropEdit";
			this.categoryDropEdit.PreBoundMaxLength = 3;
			this.categoryDropEdit.ShouldResizeByMaxLength = true;
			this.categoryDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 17, true);
			this.categoryDropEdit.TabIndex = 3;
			// 
			// HighVolumeFeatureSettingControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.categoryDropEdit);
			this.Controls.Add(this.discountNameDropEdit);
			this.Name = "HighVolumeFeatureSettingControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(305, 145, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.discountNameDropEdit.ResumeLayout(true);
			this.discountNameDropEdit.PerformLayout();
			this.categoryDropEdit.ResumeLayout(true);
			this.categoryDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private ZArchitecture.GUI.ZDropEdit discountNameDropEdit;
		private ZArchitecture.GUI.ZDropEdit categoryDropEdit;
	}
}
