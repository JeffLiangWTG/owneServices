namespace Enterprise.Customs.EU.GUI
{
	partial class MessagesTabUserControl
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
			// BottomVerticalSplitContainer
			// 
			// 
			// MessageDetailsTabPage
			// 
			this.MessageDetailsTabPage.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("4880F089-244E-4EF4-A90D-3A897949D19C", "Interpretation");
			this.MessageDetailsTabPage.Controls.Add(this.InterpretedMessageTextWebBrowser);
			this.MessageDetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(502, 539, true);
			this.MessageDetailsTabPage.Controls.SetChildIndex(this.InterpretedMessageTextBox, 0);
			this.MessageDetailsTabPage.Controls.SetChildIndex(this.InterpretedMessageTextWebBrowser, 0);
			// 
			// InterpretedMessageTextBox
			// 
			this.InterpretedMessageTextBox.Dock = System.Windows.Forms.DockStyle.None;
			this.InterpretedMessageTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(143, 201, true);
			this.InterpretedMessageTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(10, 10, true);
			this.InterpretedMessageTextBox.TabIndex = 2;
			// 
			// MessageTextTabPage
			// 
			this.MessageTextTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(502, 539, true);
			// 
			// MessageTextTextBox
			// 
			this.MessageTextTextBox.BackColor = System.Drawing.SystemColors.Info;
			this.BindingSource.SetBindingMember(this.MessageTextTextBox, "EM_MessageTextIndentedXml");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(null)).EM_MessageTextIndentedXml)));
			this.MessageTextTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.MessageTextTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(496, 533, true);
			this.MessageTextTextBox.WordWrap = false;
			this.MessageTextTextBox.HideSelection = false;
			this.MessageTextTextBox.EnableFindDialog = true;
			// 
			// InterpretedMessageTextWebBrowser
			// 
			this.InterpretedMessageTextWebBrowser.AllowWebBrowserDrop = false;
			this.InterpretedMessageTextWebBrowser.Dock = System.Windows.Forms.DockStyle.Fill;
			this.InterpretedMessageTextWebBrowser.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.InterpretedMessageTextWebBrowser.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(20, 20, true);
			this.InterpretedMessageTextWebBrowser.Name = "InterpretedMessageTextWebBrowser";
			this.InterpretedMessageTextWebBrowser.ScriptErrorsSuppressed = true;
			this.InterpretedMessageTextWebBrowser.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(496, 533, true);
			this.InterpretedMessageTextWebBrowser.TabIndex = 1;
			// 
			// MessagesTabUserControl
			// 
			this.Name = "MessagesTabUserControl";
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

		protected Enterprise.ZArchitecture.GUI.ZWebBrowser InterpretedMessageTextWebBrowser;
	}
}
