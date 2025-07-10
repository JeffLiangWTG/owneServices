using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.ARAP.Invoicing
{
	public partial class SubAccountsControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		System.ComponentModel.IContainer components = null;

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
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new ZDropEditColumnStyleInfo();
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new ZGuidFindBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			this.SubAccountsGrid = new ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.SubAccountsGrid)).BeginInit();
			this.SubAccountsGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// SubAccountsGrid
			// 
			this.SubAccountsGrid.AllowNavigation = false;
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Business.Base.Transaction.DependentTransactionLine)(((System.Collections.IList)(((Business.Base.Transaction.TransactionHeaderWithLines)(null)).Lines)).SyncRoot)).SubAccounts)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.Base.Transaction.TransactionLineSubAccount)(((System.Collections.IList)(((Business.Base.Transaction.DependentTransactionLine)(((System.Collections.IList)(((Business.Base.Transaction.TransactionHeaderWithLines)(null)).Lines)).SyncRoot)).SubAccounts)).SyncRoot)).AL1_Calc_SubClassParent)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Business.Base.Transaction.TransactionLineSubAccount)(((System.Collections.IList)(((Business.Base.Transaction.DependentTransactionLine)(((System.Collections.IList)(((Business.Base.Transaction.TransactionHeaderWithLines)(null)).Lines)).SyncRoot)).SubAccounts)).SyncRoot)).AL1_SubClassParentId)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.Base.Transaction.TransactionLineSubAccount)(((System.Collections.IList)(((Business.Base.Transaction.DependentTransactionLine)(((System.Collections.IList)(((Business.Base.Transaction.TransactionHeaderWithLines)(null)).Lines)).SyncRoot)).SubAccounts)).SyncRoot)).AL1_Calc_SubAccountDescription)));
			this.SubAccountsGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("6c03d7aa-d7f9-4276-ad99-3e01beae6df3", "Type");
			zDropEditColumnStyleInfo1.ColumnName = "AL1_Calc_SubClassParent";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zGuidFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ac973e3b-fa1d-4b62-8d6e-06d065935498", "Sub Account");
			zGuidFindBoxColumnStyleInfo1.ColumnName = "AL1_SubClassParentId";
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("798b98d5-3939-4594-9b9b-187fe7fd2b32", "Sub Account Description");
			zTextBoxColumnStyleInfo1.ColumnName = "AL1_Calc_SubAccountDescription";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180);
			this.SubAccountsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.SubAccountsGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.SubAccountsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.SubAccountsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SubAccountsGrid.GridId = "bfabf151-dc11-4497-8603-d95f0717aecb";
			this.SubAccountsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.SubAccountsGrid.LayoutKey = "SubAccountsGrid";
			this.SubAccountsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SubAccountsGrid.Name = "SubAccountsGrid";
			this.SubAccountsGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.SubAccountsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(501, 150, true);
			this.SubAccountsGrid.TabIndex = 1;
			// 
			// SubAccountsControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.SubAccountsGrid);
			this.Name = "SubAccountsControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(501, 150, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.SubAccountsGrid)).EndInit();
			this.SubAccountsGrid.ResumeLayout(false);
			this.SubAccountsGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

	}
}
