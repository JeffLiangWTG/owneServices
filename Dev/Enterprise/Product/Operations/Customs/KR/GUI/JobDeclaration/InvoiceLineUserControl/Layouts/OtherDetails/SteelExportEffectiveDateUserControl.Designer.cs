namespace Enterprise.Customs.KR.GUI
{
	partial class SteelExportEffectiveDateUserControl
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
            this.EffectiveDateFromDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
            this.EffectiveDateToDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
            this.zLabel1 = new Enterprise.ZArchitecture.ZLabel();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.EffectiveDateFromDateEdit.SuspendLayout();
            this.EffectiveDateToDateEdit.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.KR.Business.JobDeclaration);
            // 
            // EffectiveDateFromDateEdit
            // 
            this.EffectiveDateFromDateEdit.AllowDrop = true;
            this.EffectiveDateFromDateEdit.AutoCompleteMonthThreshold = 1;
            this.BindingSource.SetBindingMember(this.EffectiveDateFromDateEdit, "FilteredInvoiceLines.PRA_DateOfIssue");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.KR.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).PRA_DateOfIssue)));
            this.EffectiveDateFromDateEdit.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("75ed0a10-0d7f-4c5c-95f4-99d63a2f8863", "Effective Date");
            this.EffectiveDateFromDateEdit.Dock = System.Windows.Forms.DockStyle.Left;
            this.EffectiveDateFromDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.EffectiveDateFromDateEdit.Name = "EffectiveDateFromDateEdit";
            this.EffectiveDateFromDateEdit.TabIndex = 1;
            // 
            // EffectiveDateToDateEdit
            // 
            this.EffectiveDateToDateEdit.AllowDrop = true;
            this.EffectiveDateToDateEdit.AutoCompleteMonthThreshold = 1;
            this.BindingSource.SetBindingMember(this.EffectiveDateToDateEdit, "FilteredInvoiceLines.PRA_DateOfExpiry");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.KR.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).PRA_DateOfExpiry)));
            this.EffectiveDateToDateEdit.Dock = System.Windows.Forms.DockStyle.Right;
            this.EffectiveDateToDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(165, 0, true);
            this.EffectiveDateToDateEdit.Name = "EffectiveDateToDateEdit";
            this.EffectiveDateToDateEdit.TabIndex = 2;
            // 
            // zLabel1
            // 
            this.zLabel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.zLabel1.FontType = Enterprise.ZArchitecture.Core.OFontTypes.Larger;
            this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.zLabel1, false);
            this.zLabel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(116, 0, true);
            this.zLabel1.Name = "zLabel1";
            this.zLabel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(49, 27, true);
            this.zLabel1.TabIndex = 3;
            this.zLabel1.Text = "~";
            this.zLabel1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // SteelExportEffectiveDateUserControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CaptionRenderingEnabled = true;
            this.Controls.Add(this.zLabel1);
            this.Controls.Add(this.EffectiveDateToDateEdit);
            this.Controls.Add(this.EffectiveDateFromDateEdit);
            this.Name = "SteelExportEffectiveDateUserControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(281, 27, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.EffectiveDateFromDateEdit.ResumeLayout(true);
            this.EffectiveDateFromDateEdit.PerformLayout();
            this.EffectiveDateToDateEdit.ResumeLayout(true);
            this.EffectiveDateToDateEdit.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion
		public ZArchitecture.GUI.ZDateEdit EffectiveDateFromDateEdit;
		public ZArchitecture.GUI.ZDateEdit EffectiveDateToDateEdit;
		private ZArchitecture.ZLabel zLabel1;
	}
}
