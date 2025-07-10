using Enterprise.Accounting.Business.Riba;

namespace Enterprise.Accounting.GUI.Riba
{
	partial class BackDatePostForm
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

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		protected new void InitializeComponent()
		{
			this.OKPostButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CancelPostButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.BackPostDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.BackInvoiceDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.BackPostDateEdit.SuspendLayout();
			this.BackInvoiceDateEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 129, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(252, 0, true);
			this.MainStatusBar.Visible = false;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Accounting.Business.Riba.BackDatePostHolder);
			// 
			// OKPostButton
			// 
			this.OKPostButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.OKPostButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("BackDatePostForm|b8ca41bf-fb82-4b61-98f1-879c0052371a", "Post", "&Post", "");
			this.OKPostButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(28, 97, true);
			this.OKPostButton.Name = "OKPostButton";
			this.OKPostButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 22, true);
			this.OKPostButton.TabIndex = 3;
			this.OKPostButton.Click += new System.EventHandler(this.OKButton_Click);
			// 
			// CancelPostButton
			// 
			this.CancelPostButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.CancelPostButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("BackDatePostForm|6b05beef-26ab-49f6-bcea-147a87298945", "Cancel", "Cancel", "");
			this.CancelPostButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(131, 97, true);
			this.CancelPostButton.Name = "CancelPostButton";
			this.CancelPostButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 22, true);
			this.CancelPostButton.TabIndex = 4;
			this.CancelPostButton.Click += new System.EventHandler(this.CancelButton_Click);
			// 
			// BackPostDateEdit
			// 
			this.BackPostDateEdit.AllowDrop = true;
			this.BackPostDateEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.BackPostDateEdit.AutoCompleteMonthThreshold = 1;
			this.BackPostDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.BackPostDateEdit, "BackPostDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Accounting.Business.Riba.BackDatePostHolder)(null)).BackPostDate)));
			this.BackPostDateEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("c6045b64-8028-48b6-b608-e577b2fb00ed", "Post Date");
			this.BackPostDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(92, 19, true);
			this.BackPostDateEdit.Name = "BackPostDateEdit";
			this.BackPostDateEdit.TabIndex = 1;
			// 
			// BackInvoiceDateEdit
			// 
			this.BackInvoiceDateEdit.AllowDrop = true;
			this.BackInvoiceDateEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.BackInvoiceDateEdit.AutoCompleteMonthThreshold = 1;
			this.BackInvoiceDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.BackInvoiceDateEdit, "BackInvoiceDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Accounting.Business.Riba.BackDatePostHolder)(null)).BackInvoiceDate)));
			this.BackInvoiceDateEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("fb386920-d73d-4d56-9a19-367d98c4fbee", "Invoice Date");
			this.BackInvoiceDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(92, 55, true);
			this.BackInvoiceDateEdit.Name = "BackInvoiceDateEdit";
			this.BackInvoiceDateEdit.TabIndex = 2;
			// 
			// BackDatePostForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("09346af5-2e45-493f-a929-2127f80075ee", "Back Date Receipt and Deposit Batch");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(252, 129, true);
			this.ControlBox = false;
			this.Controls.Add(this.BackInvoiceDateEdit);
			this.Controls.Add(this.BackPostDateEdit);
			this.Controls.Add(this.OKPostButton);
			this.Controls.Add(this.CancelPostButton);
			this.DataSourceType = typeof(Enterprise.Accounting.Business.Riba.BackDatePostHolder);
			this.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(267, 167, true);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(267, 167, true);
			this.Name = "BackDatePostForm";
			this.Text = "BackDatePostForm";
			this.Controls.SetChildIndex(this.CancelPostButton, 0);
			this.Controls.SetChildIndex(this.OKPostButton, 0);
			this.Controls.SetChildIndex(this.BackPostDateEdit, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.BackInvoiceDateEdit, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.BackPostDateEdit.ResumeLayout(true);
			this.BackPostDateEdit.PerformLayout();
			this.BackInvoiceDateEdit.ResumeLayout(true);
			this.BackInvoiceDateEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZButton OKPostButton;
		private Enterprise.ZArchitecture.GUI.ZButton CancelPostButton;
		private Enterprise.ZArchitecture.GUI.ZDateEdit BackPostDateEdit;
		private ZArchitecture.GUI.ZDateEdit BackInvoiceDateEdit;
	}
}
