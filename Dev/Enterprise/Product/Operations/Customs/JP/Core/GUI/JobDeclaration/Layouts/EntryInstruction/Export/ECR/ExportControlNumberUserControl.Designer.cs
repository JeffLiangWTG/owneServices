namespace Enterprise.Customs.JP.GUI
{
	partial class ExportControlNumberUserControl
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
			if (disposing)
			{
				Extensions.Dispose();
				if (components != null)
				{
					components.Dispose();
				}
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
			this.ExportControlNumberPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.ExportControlNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.DeleteButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ExportControlNumberPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.JP.Business.CusEntryInstruction);
			// 
			// ExportControlNumberPanel
			// 
			this.ExportControlNumberPanel.Controls.Add(this.ExportControlNumberTextBox);
			this.ExportControlNumberPanel.Controls.Add(this.DeleteButton);
			this.ExportControlNumberPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ExportControlNumberPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ExportControlNumberPanel.Name = "ExportControlNumberPanel";
			this.ExportControlNumberPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(311, 20, true);
			this.ExportControlNumberPanel.TabIndex = 0;
			// 
			// ExportControlNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.ExportControlNumberTextBox, "ExportControlNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.Business.CusEntryInstruction)(null)).ExportControlNumber)));
			this.ExportControlNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 2, true);
			this.ExportControlNumberTextBox.Name = "ExportControlNumberTextBox";
			this.ExportControlNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(220, 20, true);
			this.ExportControlNumberTextBox.TabIndex = 1;
			// 
			// DeleteButton
			// 
			this.DeleteButton.IsCaptionOverridden = true;
			this.DeleteButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(226, 2, true);
			this.DeleteButton.Name = "DeleteButton";
			this.DeleteButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 20, true);
			this.DeleteButton.TabIndex = 1;
			this.DeleteButton.Text = "Delete";
			this.DeleteButton.ToolTipCaption = null;
			// 
			// ExportControlNumberUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ExportControlNumberPanel);
			this.Name = "ExportControlNumberUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(311, 22, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ExportControlNumberPanel.ResumeLayout(false);
			this.ExportControlNumberPanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZPanel ExportControlNumberPanel;
		private ZArchitecture.ZTextBox ExportControlNumberTextBox;
		private ZArchitecture.GUI.ZButton DeleteButton;
	}
}
