namespace Enterprise.Client.EDI.MasterFiles.GUI
{
	partial class EdiCommissionAgreementControl
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
			this.customerFiltersButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.customerCodeDropEdit = new Enterprise.Client.EDI.Billing.GUI.CategoryDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.RecipientsGrid)).BeginInit();
			this.RecipientsGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.RatesGrid)).BeginInit();
			this.RatesGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.detailsSplitContainer)).BeginInit();
			this.detailsSplitContainer.Panel2.SuspendLayout();
			this.detailsSplitContainer.SuspendLayout();
			this.CA0_CommissionBasisDropEdit.SuspendLayout();
			this.CA0_OH_CustomerGuidDropEdit.SuspendLayout();
			this.CAR_EndDateEdit.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.customerCodeDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Client.EDI.MasterFiles.Business.EdiCommissionAgreement);
			// 
			// detailsSplitContainer
			// 
			// 
			// detailsSplitContainer.Panel2
			// 
			this.detailsSplitContainer.Panel2.Controls.Add(this.customerCodeDropEdit);
			this.detailsSplitContainer.Panel2.Controls.Add(this.customerFiltersButton);
			// 
			// customerFiltersButton
			// 
			this.customerFiltersButton.CaptionResourceString = ZClientEDI.Res.GetData("54ff023d-6894-42fe-b8ad-d54e4e831645", "Add Filters");
			this.customerFiltersButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(171, 0, true);
			this.customerFiltersButton.Name = "customerFiltersButton";
			this.customerFiltersButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(76, 20, true);
			this.customerFiltersButton.TabIndex = 8;
			this.customerFiltersButton.Click += new System.EventHandler(this.CustomerFiltersButton_Click);
			// 
			// customerCodeDropEdit
			// 
			this.customerCodeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.customerCodeDropEdit, "CustomerCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.MasterFiles.Business.EdiCommissionAgreement)(null)).CustomerCode)));
			this.customerCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(61, 0, true);
			this.customerCodeDropEdit.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(2, true);
			this.customerCodeDropEdit.Name = "CustomerCodeDropEdit";
			this.customerCodeDropEdit.ShowDescriptionBox = false;
			this.customerCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 16, true);
			this.customerCodeDropEdit.TabIndex = 0;
			// 
			// EdiCommissionAgreementControl
			// 
			this.Name = "EdiCommissionAgreementControl";
			((System.ComponentModel.ISupportInitialize)(this.RecipientsGrid)).EndInit();
			this.RecipientsGrid.ResumeLayout(false);
			this.RecipientsGrid.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.RatesGrid)).EndInit();
			this.RatesGrid.ResumeLayout(false);
			this.RatesGrid.PerformLayout();
			this.detailsSplitContainer.Panel2.ResumeLayout(false);
			this.detailsSplitContainer.Panel2.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.detailsSplitContainer)).EndInit();
			this.detailsSplitContainer.ResumeLayout(false);
			this.detailsSplitContainer.PerformLayout();
			this.CA0_CommissionBasisDropEdit.ResumeLayout(true);
			this.CA0_CommissionBasisDropEdit.PerformLayout();
			this.CA0_OH_CustomerGuidDropEdit.ResumeLayout(true);
			this.CA0_OH_CustomerGuidDropEdit.PerformLayout();
			this.CAR_EndDateEdit.ResumeLayout(true);
			this.CAR_EndDateEdit.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.customerCodeDropEdit.ResumeLayout(true);
			this.customerCodeDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZButton customerFiltersButton;
		private Enterprise.Client.EDI.Billing.GUI.CategoryDropEdit customerCodeDropEdit;
	}
}
