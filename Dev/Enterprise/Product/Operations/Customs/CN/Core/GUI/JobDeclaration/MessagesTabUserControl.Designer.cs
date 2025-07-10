namespace Enterprise.Customs.CN.GUI
{
	public partial class MessagesTabUserControl
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
			this.components = new System.ComponentModel.Container();
			this.InterpretedMessageTextWebBrowser = new Enterprise.ZArchitecture.GUI.ZWebBrowser();
			((System.ComponentModel.ISupportInitialize)(this.MessagesGrid)).BeginInit();
			this.MessagesGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BottomVerticalSplitContainer)).BeginInit();
			this.BottomVerticalSplitContainer.Panel1.SuspendLayout();
			this.BottomVerticalSplitContainer.Panel2.SuspendLayout();
			this.BottomVerticalSplitContainer.SuspendLayout();
			this.MessageTabControl.SuspendLayout();
			this.MessageDetailsTabPage.SuspendLayout();
			this.MessageTextTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MessagesGrid
			// 
			this.MessagesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(660, 634, true);
			// 
			// BottomVerticalSplitContainer
			// 
			this.BottomVerticalSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1147, 634, true);
			this.BottomVerticalSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(660);
			// 
			// MessageTabControl
			// 
			this.MessageTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(484, 634, true);
			// 
			// MessageDetailsTabPage
			// 
			this.MessageDetailsTabPage.Controls.Add(this.InterpretedMessageTextWebBrowser);
			this.MessageDetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(476, 607, true);
			this.MessageDetailsTabPage.Controls.SetChildIndex(this.InterpretedMessageTextBox, 0);
			this.MessageDetailsTabPage.Controls.SetChildIndex(this.InterpretedMessageTextWebBrowser, 0);
			// 
			// InterpretedMessageTextBox
			// 
			this.InterpretedMessageTextBox.Dock = System.Windows.Forms.DockStyle.None;
			this.InterpretedMessageTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(10, 10, true);
			this.InterpretedMessageTextBox.TabIndex = 2;
			// 
			// MessageTextTabPage
			// 
			this.MessageTextTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(502, 539, true);
			// 
			// MessageTextTextBox
			// 
			this.MessageTextTextBox.HideSelection = false;
			this.MessageTextTextBox.EnableFindDialog = true;
			this.MessageTextTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(496, 533, true);
			// 
			// InterpretedMessageTextWebBrowser
			// 
			this.InterpretedMessageTextWebBrowser.AllowWebBrowserDrop = false;
			this.InterpretedMessageTextWebBrowser.Dock = System.Windows.Forms.DockStyle.Fill;
			this.InterpretedMessageTextWebBrowser.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.InterpretedMessageTextWebBrowser.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(20, 20, true);
			this.InterpretedMessageTextWebBrowser.Name = "InterpretedMessageTextWebBrowser";
			this.InterpretedMessageTextWebBrowser.ScriptErrorsSuppressed = true;
			this.InterpretedMessageTextWebBrowser.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(470, 601, true);
			this.InterpretedMessageTextWebBrowser.TabIndex = 1;
			// 
			// MessagesTabUserControl
			// 
			this.Name = "MessagesTabUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1147, 634, true);
			((System.ComponentModel.ISupportInitialize)(this.MessagesGrid)).EndInit();
			this.MessagesGrid.ResumeLayout(false);
			this.MessagesGrid.PerformLayout();
			this.BottomVerticalSplitContainer.Panel1.ResumeLayout(false);
			this.BottomVerticalSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.BottomVerticalSplitContainer)).EndInit();
			this.BottomVerticalSplitContainer.ResumeLayout(false);
			this.BottomVerticalSplitContainer.PerformLayout();
			this.MessageTabControl.ResumeLayout(false);
			this.MessageTabControl.PerformLayout();
			this.MessageDetailsTabPage.ResumeLayout(false);
			this.MessageDetailsTabPage.PerformLayout();
			this.MessageTextTabPage.ResumeLayout(false);
			this.MessageTextTabPage.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal Enterprise.ZArchitecture.GUI.ZWebBrowser InterpretedMessageTextWebBrowser;
	}
}
