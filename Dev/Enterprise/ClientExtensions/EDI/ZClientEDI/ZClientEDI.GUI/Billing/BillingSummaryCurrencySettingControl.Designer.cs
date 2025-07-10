using CargoWise.Types;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.EDI.Billing.GUI
{
	partial class BillingSummaryCurrencySettingControl
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
			this.CurrencyBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.CurrencyBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Client.EDI.Billing.Business.BillingSummaryCurrencyLicenceSetting);
			// 
			// CurrencyBox
			// 
			this.CurrencyBox.AllowDrop = true;
			this.CurrencyBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.CurrencyBox, "LS9_RX_NKPriceCurrency");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.Billing.Business.BillingSummaryCurrencyLicenceSetting)(null)).LS9_RX_NKPriceCurrency)));
			this.CurrencyBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(20, 26, true);
			this.CurrencyBox.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(233, 0, true);
			this.CurrencyBox.Name = "CurrencyBox";
			this.CurrencyBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.CurrencyBox.ParentType = null;
			this.CurrencyBox.PreBoundMaxLength = 3;
			this.CurrencyBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.CurrencyBox.TabIndex = 2;
			// 
			// BillingSummaryCurrencySettingControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.CurrencyBox);
			this.Name = "BillingSummaryCurrencySettingControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(274, 73, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.CurrencyBox.ResumeLayout(true);
			this.CurrencyBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZCodeFindBox CurrencyBox;
	}
}
