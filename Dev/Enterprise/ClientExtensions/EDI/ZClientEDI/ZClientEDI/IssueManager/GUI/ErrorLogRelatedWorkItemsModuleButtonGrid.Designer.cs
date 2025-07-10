namespace Enterprise.Client.EDI.IssueManager.GUI
{
	partial class ErrorLogRelatedWorkItemsModuleButtonGrid
	{
		private void InitializeComponent()
		{
			this.createIncidentsButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			//
			// createIncidentsButton
			//
			this.createIncidentsButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.createIncidentsButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 154, true);
			this.createIncidentsButton.Name = "createIncidentsButton";
			this.createIncidentsButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(201, 23, true);
			this.createIncidentsButton.TabIndex = 5;
			this.createIncidentsButton.Text = "Auto Create Incidents For Each Client";
			this.createIncidentsButton.UseVisualStyleBackColor = true;
			this.createIncidentsButton.Click += new System.EventHandler(this.CreateIncidentsButton_Click);
			//
			// ErrorLogRelatedWorkItemsModuleButtonGrid
			//
			this.Controls.Add(this.createIncidentsButton);
			this.Name = "ErrorLogRelatedWorkItemsModuleButtonGrid";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(639, 184, true);
			this.Controls.SetChildIndex(this.createIncidentsButton, 0);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
		}

		internal Enterprise.ZArchitecture.GUI.ZButton createIncidentsButton;

	}
}
