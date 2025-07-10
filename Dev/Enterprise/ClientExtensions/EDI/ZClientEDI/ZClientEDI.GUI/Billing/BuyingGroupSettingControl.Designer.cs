namespace Enterprise.Client.EDI.Billing.GUI
{
	partial class BuyingGroupSettingControl
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
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.discountNameDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Client.EDI.Billing.Business.BuyingGroupLicenceSetting);
			// 
			// discountNameDropEdit
			// 
			this.discountNameDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.discountNameDropEdit, "LS9_Name");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.EDI.Billing.Business.BuyingGroupLicenceSetting)(null)).LS9_Name)));
			this.discountNameDropEdit.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("e5b8e2a3-0515-4d90-b03b-66f1f66ba224", "Name");
			this.discountNameDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(63, 5, true);
			this.discountNameDropEdit.MaxItemsToShowInDropDown = 25;
			this.discountNameDropEdit.Name = "discountNameDropEdit";
			this.discountNameDropEdit.ShowDescriptionBox = false;
			this.discountNameDropEdit.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			this.discountNameDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 17, true);
			this.discountNameDropEdit.TabIndex = 1;
			// 
			// BuyingGroupSettingControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.discountNameDropEdit);
			this.Name = "BuyingGroupSettingControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(316, 144, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.discountNameDropEdit.ResumeLayout(true);
			this.discountNameDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZDropEdit discountNameDropEdit;
	}
}
