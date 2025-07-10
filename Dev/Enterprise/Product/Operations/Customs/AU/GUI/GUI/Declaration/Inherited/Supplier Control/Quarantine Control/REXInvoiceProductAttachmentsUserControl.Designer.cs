namespace Enterprise.Customs.AU.Declaration.GUI
{
	partial class REXInvoiceProductAttachmentsUserControl
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
			this.REXCommonProductAttachmentsUserControl = new Enterprise.Customs.AU.Declaration.GUI.REXProductAttachmentsUserControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.REXCommonProductAttachmentsUserControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.AU.Declaration.Business.JobComInvoiceHeader);
			// 
			// REXCommonProductAttachmentsUserControl
			// 
			this.REXCommonProductAttachmentsUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.REXCommonProductAttachmentsUserControl, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.AU.Declaration.Business.ICusStorageDocPivotParent)(((Enterprise.Customs.AU.Declaration.Business.JobComInvoiceHeader)(null)))));
			this.REXCommonProductAttachmentsUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.REXCommonProductAttachmentsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.REXCommonProductAttachmentsUserControl.Name = "REXCommonProductAttachmentsUserControl";
			this.REXCommonProductAttachmentsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1720, 450, true);
			this.REXCommonProductAttachmentsUserControl.TabIndex = 3;
			// 
			// REXInvoiceProductAttachmentsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.REXCommonProductAttachmentsUserControl);
			this.Name = "REXInvoiceProductAttachmentsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1720, 450, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.REXCommonProductAttachmentsUserControl.ResumeLayout(true);
			this.REXCommonProductAttachmentsUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private REXProductAttachmentsUserControl REXCommonProductAttachmentsUserControl;
	}
}
