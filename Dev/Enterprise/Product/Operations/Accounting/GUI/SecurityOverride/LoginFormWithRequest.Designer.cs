namespace Enterprise.Accounting.GUI
{
	partial class LoginFormWithRequest
	{
		System.ComponentModel.IContainer components = null;

		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		void InitializeComponent()
		{
			this.ApprovalRequestButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// ApprovalRequestButton
			// 
			this.ApprovalRequestButton.DialogResult = System.Windows.Forms.DialogResult.Ignore;
			this.ApprovalRequestButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(209, 234, true);
			this.ApprovalRequestButton.Name = "ApprovalRequestButton";
			this.ApprovalRequestButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 24, true);
			this.ApprovalRequestButton.TabIndex = 3;
			this.ApprovalRequestButton.Text = Enterprise.Accounting.GUI.Res.GetString("6846CAEE-BFD6-42ED-8A4F-AF3298CF5FED", "Approval Request");
			// 
			// LoginFormWithRequest
			//
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(555, 264, true);
			this.Controls.Add(this.ApprovalRequestButton);
			this.Name = "LoginFormWithRequest";
			this.Controls.SetChildIndex(this.ApprovalRequestButton, 0);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		protected ZArchitecture.GUI.ZButton ApprovalRequestButton;
	}
}
