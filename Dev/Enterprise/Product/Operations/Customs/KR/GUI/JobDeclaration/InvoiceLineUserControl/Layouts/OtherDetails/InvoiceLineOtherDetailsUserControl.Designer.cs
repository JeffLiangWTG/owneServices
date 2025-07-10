
namespace Enterprise.Customs.KR.GUI
{
	partial class InvoiceLineOtherDetailsUserControl
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
			this.SteelExportEffectiveDateUserControl = new Enterprise.Customs.KR.GUI.SteelExportEffectiveDateUserControl();
			this.ApprovalNoTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SteelExportEffectiveDateUserControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.KR.Business.JobDeclaration);
			// 
			// SteelExportEffectiveDateUserControl
			// 
			this.SteelExportEffectiveDateUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SteelExportEffectiveDateUserControl, ".");
			this.SteelExportEffectiveDateUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(43, 69, true);
			this.SteelExportEffectiveDateUserControl.Name = "SteelExportEffectiveDateUserControl";
			this.SteelExportEffectiveDateUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(192, 21, true);
			this.SteelExportEffectiveDateUserControl.TabIndex = 1;
			// 
			// ApprovalNoTextBox
			// 
			this.BindingSource.SetBindingMember(this.ApprovalNoTextBox, "FilteredInvoiceLines.PRA_ReferenceNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).PRA_ReferenceNumber)));
			this.ApprovalNoTextBox.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("JobComInvoiceLine|3F7EBE01-C8C0-4EB0-A6CB-F20875DAD95D", "Approval No.");
			this.ApprovalNoTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 46, true);
			this.ApprovalNoTextBox.Name = "ApprovalNoTextBox";
			this.ApprovalNoTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(177, 19, true);
			this.ApprovalNoTextBox.TabIndex = 0;
			// 
			// InvoiceLineOtherDetailsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.ApprovalNoTextBox);
			this.Controls.Add(this.SteelExportEffectiveDateUserControl);
			this.Name = "InvoiceLineOtherDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(266, 106, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.SteelExportEffectiveDateUserControl.ResumeLayout(true);
			this.SteelExportEffectiveDateUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		public SteelExportEffectiveDateUserControl SteelExportEffectiveDateUserControl;
		public ZArchitecture.ZTextBox ApprovalNoTextBox;
	}
}
