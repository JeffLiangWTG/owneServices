using CargoWise.Types;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.EDI.Billing.GUI
{
	partial class DiscountSuspensionPolicySettingControl
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
			this.PolicyCodeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.PolicyCodeDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Client.EDI.Billing.Business.DiscountSuspensionPolicyLicenceSetting);
			// 
			// PolicyCodeDropEdit
			// 
			this.PolicyCodeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PolicyCodeDropEdit, "PolicyCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.EDI.Billing.Business.DiscountSuspensionPolicyLicenceSetting)(null)).PolicyCode)));
			this.PolicyCodeDropEdit.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("8c8c04d0-d2c2-490c-ac89-aa636df1728b", "Discount Policy");
			this.PolicyCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(94, 19, true);
			this.PolicyCodeDropEdit.Name = "PolicyCodeDropEdit";
			this.PolicyCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(284, 20, true);
			this.PolicyCodeDropEdit.TabIndex = 1;
			// 
			// DiscountSuspensionPolicySettingControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.PolicyCodeDropEdit);
			this.Name = "DiscountSuspensionPolicySettingControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(397, 58, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.PolicyCodeDropEdit.ResumeLayout(true);
			this.PolicyCodeDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZDropEdit PolicyCodeDropEdit;
	}
}
