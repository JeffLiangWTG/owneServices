
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI
{
	partial class EntryDetailsUserControl
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
			this.DynamicEntryLineDetailsPanel = new Enterprise.ZArchitecture.GUI.DynamicLayoutPanel();
			// 
			// DynamicEntryLineDetailsPanel
			// 
			this.DynamicEntryLineDetailsPanel.AllowDrop = true;
			this.DynamicEntryLineDetailsPanel.AutoScroll = true;
			this.DynamicEntryLineDetailsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DynamicEntryLineDetailsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DynamicEntryLineDetailsPanel.Name = "DynamicEntryLineDetailsPanel";
			this.DynamicEntryLineDetailsPanel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.DynamicEntryLineDetailsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1163, 281, true);
			this.DynamicEntryLineDetailsPanel.TabIndex = 1;
			this.DynamicEntryLineDetailsPanel.CaptionRenderingEnabled = true;
			// 
			// EntryDetailsUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(DynamicEntryLineDetailsPanel);

		}
		DynamicLayoutPanel DynamicEntryLineDetailsPanel;

		#endregion
	}
}
