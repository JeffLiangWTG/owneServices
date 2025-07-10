namespace Enterprise.DocumentEngine.GUI.DocumentDelivery
{
	partial class PrintTaskDeliveryForm
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
			this.printTaskSettingsControl = new Enterprise.DocumentEngine.GUI.DocumentDelivery.PrintTaskDeliveryControl();
			this.printTaskDeliveryButtonsControl = new Enterprise.DocumentEngine.GUI.DocumentDelivery.PrintTaskDeliveryButtonsControl();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 387, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(663, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.DocumentEngine.PrintTaskSettings);
			// 
			// printTaskSettingsControl
			// 
			this.BindingSource.SetBindingMember(this.printTaskSettingsControl, ".");
			this.printTaskSettingsControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.printTaskSettingsControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.printTaskSettingsControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(610, 349, true);
			this.printTaskSettingsControl.Name = "printTaskSettingsControl";
			this.printTaskSettingsControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(663, 387, true);
			this.printTaskSettingsControl.TabIndex = 3;
			// 
			// printTaskDeliveryButtonsControl
			// 
			this.printTaskDeliveryButtonsControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.printTaskDeliveryButtonsControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(367, 356, true);
			this.printTaskDeliveryButtonsControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(290, 25, true);
			this.printTaskDeliveryButtonsControl.Name = "printTaskDeliveryButtonsControl";
			this.printTaskDeliveryButtonsControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(290, 25, true);
			this.printTaskDeliveryButtonsControl.TabIndex = 4;
			// 
			// PrintTaskDeliveryForm
			// 
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(663, 411, true);
			this.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("PrintTaskDeliveryForm|4b1c90ef-b220-4086-a6ac-ef2ae0892b59", "Deliver Documents");
			this.Controls.Add(this.printTaskDeliveryButtonsControl);
			this.Controls.Add(this.printTaskSettingsControl);
			this.DataSourceAssemblyName = "Enterprise.DocumentEngine";
			this.DataSourceType = typeof(Enterprise.DocumentEngine.PrintTaskSettings);
			this.DataSourceTypeName = "Enterprise.DocumentEngine.PrintTaskSettings";
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(679, 406, true);
			this.Name = "PrintTaskDeliveryForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.printTaskSettingsControl, 0);
			this.Controls.SetChildIndex(this.printTaskDeliveryButtonsControl, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private PrintTaskDeliveryControl printTaskSettingsControl;
		private PrintTaskDeliveryButtonsControl printTaskDeliveryButtonsControl;

	}
}
