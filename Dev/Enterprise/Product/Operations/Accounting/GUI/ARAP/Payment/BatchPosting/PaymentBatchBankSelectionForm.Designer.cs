using System;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.ARAP
{
	public partial class PaymentBatchBankSelectionForm
	{


		#region Windows Form Designer generated code

		private ZGroupBox BankAccountGroupBox;
		private ZGuidDropEdit DefaultBankAccountsDropEdit;
		protected Core.Forms.ZPostOrCancelButton SaveButton;
		protected Core.Forms.ZPostOrCancelButton CloseButton;

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
			this.SaveButton = new Core.Forms.ZPostOrCancelButton();
			this.CloseButton = new Core.Forms.ZPostOrCancelButton();
			this.BankAccountGroupBox = new ZGroupBox();
			this.DefaultBankAccountsDropEdit = new ZGuidDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.BankAccountGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 101, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(423, 24, true);
			this.MainStatusBar.SizingGrip = false;
			this.MainStatusBar.TabIndex = 3;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(538);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(BankAccountSelectionObject);
			// 
			// SaveButton
			// 
			this.SaveButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.SaveButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PaymentBatchBankSelectionForm|4a3d19e0-311c-4f9e-8c06-e447d741e08e", "Select");
			this.SaveButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(262, 68, true);
			this.SaveButton.Name = "SaveButton";
			this.SaveButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(73, 23, true);
			this.SaveButton.TabIndex = 1;
			this.SaveButton.Click += new EventHandler(this.SaveButton_Click);
			// 
			// CloseButton
			// 
			this.CloseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CloseButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PaymentBatchBankSelectionForm|9c19ae46-a122-418b-b00f-6917af8dda7d", "Cancel");
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(341, 68, true);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.CloseButton.TabIndex = 2;
			this.CloseButton.Click += new EventHandler(this.CloseButton_Click);
			// 
			// BankAccountGroupBox
			// 
			this.BankAccountGroupBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PaymentBatchBankSelectionForm|abe3411a-dd7a-4d36-a1dd-1e4f18affabe", "Default Bank Accounts");
			this.BankAccountGroupBox.Controls.Add(this.DefaultBankAccountsDropEdit);
			this.BankAccountGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 6, true);
			this.BankAccountGroupBox.Name = "BankAccountGroupBox";
			this.BankAccountGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(409, 56, true);
			this.BankAccountGroupBox.TabIndex = 0;
			this.BankAccountGroupBox.TabStop = false;
			// 
			// DefaultBankAccountsDropEdit
			// 
			this.BindingSource.SetBindingMember(this.DefaultBankAccountsDropEdit, "SelectedBankAccount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((BankAccountSelectionObject)(null)).SelectedBankAccount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((BankAccountSelectionObject)(null)).DefaultBankAccounts)));
			this.DefaultBankAccountsDropEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PaymentBatchBankSelectionForm|174d9c66-1293-445e-b11e-59c22389f122", "Bank Account");
			this.DefaultBankAccountsDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 20, true);
			this.DefaultBankAccountsDropEdit.Name = "DefaultBankAccountsDropEdit";
			this.DefaultBankAccountsDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(307, 20, true);
			this.DefaultBankAccountsDropEdit.TabIndex = 1;
			// 
			// PaymentBatchBankSelectionForm
			// 
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(423, 125, true);
			this.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PaymentBatchBankSelectionForm|abdcc81a-b97a-49d1-aed4-2c986f27cff7", "Bank Selection Form");
			this.Controls.Add(this.CloseButton);
			this.Controls.Add(this.SaveButton);
			this.Controls.Add(this.BankAccountGroupBox);
			this.DataSourceAssemblyName = "Enterprise.Accounting.GUI";
			this.DataSourceType = typeof(BankAccountSelectionObject);
			this.DataSourceTypeName = "Enterprise.Accounting.GUI.ARAP.BankAccountSelectionObject";
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(429, 151, true);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(429, 151, true);
			this.Name = "PaymentBatchBankSelectionForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Controls.SetChildIndex(this.BankAccountGroupBox, 0);
			this.Controls.SetChildIndex(this.SaveButton, 0);
			this.Controls.SetChildIndex(this.CloseButton, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.BankAccountGroupBox.ResumeLayout(false);
			this.ResumeLayout(false);
		}
		#endregion

	}
}