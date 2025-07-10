namespace Enterprise.Customs.JP.Shared.GUI
{
	partial class EditableMessageSendingForm<T>
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
		new void InitializeComponent()
		{
			this.MessageVisualObjectUserControl = new Enterprise.Customs.JP.Shared.GUI.MessageVisualObjectUserControl();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.MessageVisualObjectUserControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 580, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(825, 23, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(T);
			// 
			// MessageVisualObjectUserControl
			// 
			this.MessageVisualObjectUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.MessageVisualObjectUserControl, "VisualObjectParent");
			this.MessageVisualObjectUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MessageVisualObjectUserControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(780, 550, true);
			this.MessageVisualObjectUserControl.Name = "MessageVisualObjectUserControl";
			this.MessageVisualObjectUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MessageVisualObjectUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(780, 550, true);
			this.MessageVisualObjectUserControl.TabIndex = 0;
			// 
			// EditableMessageSendingForm
			//
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(MessageVisualObjectUserControl);
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(825, 603, true);
			this.DataSourceType = typeof(T);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(840, 640, true);
			this.Name = "EditableMessageSendingForm";
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.MessageVisualObjectUserControl.ResumeLayout(true);
			this.MessageVisualObjectUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		MessageVisualObjectUserControl MessageVisualObjectUserControl;
	}
}
