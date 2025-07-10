using Enterprise.ZArchitecture.Core;
namespace Enterprise.Accounting.GUI.ARAP
{
	partial class PaymentBatchGroupingForm
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
		private void InitializeComponent()
		{
			this.GroupByInvoicePaymentCriticalityCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.GroupByInvoiceRelatedDebtorOrganisationCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.OKButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CloseButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.GroupByUserCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// GroupByInvoicePaymentCriticalityCheckBox
			// 
			this.GroupByInvoicePaymentCriticalityCheckBox.AutoSize = true;
			this.GroupByInvoicePaymentCriticalityCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.GroupByInvoicePaymentCriticalityCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 12, true);
			this.GroupByInvoicePaymentCriticalityCheckBox.Name = "GroupByInvoicePaymentCriticalityCheckBox";
			this.GroupByInvoicePaymentCriticalityCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(195, 17, true);
			this.GroupByInvoicePaymentCriticalityCheckBox.TabIndex = 1;
			this.GroupByInvoicePaymentCriticalityCheckBox.Text = Res.GetString("9b83e3ac-3246-4933-89f0-006d890b5fdb", "Group by Invoice Payment Criticality");
			this.GroupByInvoicePaymentCriticalityCheckBox.UseVisualStyleBackColor = true;
			// 
			// GroupByInvoiceRelatedDebtorOrganisationCheckBox
			// 
			this.GroupByInvoiceRelatedDebtorOrganisationCheckBox.AutoSize = true;
			this.GroupByInvoiceRelatedDebtorOrganisationCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.GroupByInvoiceRelatedDebtorOrganisationCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 35, true);
			this.GroupByInvoiceRelatedDebtorOrganisationCheckBox.Name = "GroupByInvoiceRelatedDebtorOrganisationCheckBox";
			this.GroupByInvoiceRelatedDebtorOrganisationCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 17, true);
			this.GroupByInvoiceRelatedDebtorOrganisationCheckBox.TabIndex = 2;
			this.GroupByInvoiceRelatedDebtorOrganisationCheckBox.Text = Res.GetString("c8e5a501-8736-4cd3-9d9a-ebe393640352", "Group by Invoice Related debtor organization");
			this.GroupByInvoiceRelatedDebtorOrganisationCheckBox.UseVisualStyleBackColor = true;
			// 
			// OKButton
			// 
			this.OKButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.OKButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.OKButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(93, 97, true);
			this.OKButton.Name = "OKButton";
			this.OKButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.OKButton.TabIndex = 3;
			this.OKButton.Text = Res.GetString("a2b64dbd-dc4b-49b3-af61-7e62aea227f5", "&OK");
			this.OKButton.UseVisualStyleBackColor = true;
			// 
			// CloseButton
			// 
			this.CloseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CloseButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(174, 97, true);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.CloseButton.TabIndex = 4;
			this.CloseButton.Text = Res.GetString("fa0cf070-d3b1-4bd5-9ffd-9f6d0c4c5939", "&Cancel");
			this.CloseButton.UseVisualStyleBackColor = true;
			// 
			// GroupByUserCheckBox
			// 
			this.GroupByUserCheckBox.AutoSize = true;
			this.GroupByUserCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.GroupByUserCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 58, true);
			this.GroupByUserCheckBox.Name = "GroupByUserCheckBox";
			this.GroupByUserCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(207, 17, true);
			this.GroupByUserCheckBox.TabIndex = 5;
			this.GroupByUserCheckBox.Text = Res.GetString("73442976-1f2b-4add-9be1-42c5bf106326", "Group by User that Added the Invoice");
			this.GroupByUserCheckBox.UseVisualStyleBackColor = true;
			// 
			// PaymentBatchGroupingForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(261, 132, true);
			this.Controls.Add(this.GroupByUserCheckBox);
			this.Controls.Add(this.GroupByInvoicePaymentCriticalityCheckBox);
			this.Controls.Add(this.CloseButton);
			this.Controls.Add(this.OKButton);
			this.Controls.Add(this.GroupByInvoiceRelatedDebtorOrganisationCheckBox);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Name = "PaymentBatchGroupingForm";
			this.Text = Res.GetString("f7b7ce7d-6e73-401a-8156-47af23eac951", "Grouping");
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZCheckBox GroupByInvoicePaymentCriticalityCheckBox;
		private Enterprise.ZArchitecture.GUI.ZCheckBox GroupByInvoiceRelatedDebtorOrganisationCheckBox;
		private Enterprise.ZArchitecture.GUI.ZButton OKButton;
		private Enterprise.ZArchitecture.GUI.ZButton CloseButton;
		public Enterprise.ZArchitecture.GUI.ZCheckBox GroupByUserCheckBox;
	}
}