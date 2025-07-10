
namespace Enterprise.Customs.CA.GUI
{
	partial class B3XMessagesUserControl
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
			this.MessageInterpretationTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.MessageInterpretationUserControl = new Enterprise.Messaging.GUI.EDIMessageInterpretationUserControl();
			this.MainPanel.SuspendLayout();
			this.MessagesTabControl.SuspendLayout();
			this.MessageTextTabPage.SuspendLayout();
			this.HistoryGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessagesGrid)).BeginInit();
			this.MessagesGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.MessageInterpretationTabPage.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainPanel
			// 
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(846, 497, true);
			// 
			// MessagesTabControl
			// 
			this.MessagesTabControl.Controls.Add(this.MessageInterpretationTabPage);
			this.MessagesTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 161, true);
			this.MessagesTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(843, 336, true);
			this.MessagesTabControl.Controls.SetChildIndex(this.MessageInterpretationTabPage, 0);
			this.MessagesTabControl.Controls.SetChildIndex(this.MessageTextTabPage, 0);
			// 
			// MessageTextTabPage
			// 
			this.MessageTextTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(835, 309, true);
			// 
			// MessageTextTextBox
			// 
			this.MessageTextTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(829, 303, true);
			// 
			// HistoryGroupBox
			// 
			this.HistoryGroupBox.Dock = System.Windows.Forms.DockStyle.Left;
			this.HistoryGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(846, 161, true);
			// 
			// MessagesGrid
			// 
			this.MessagesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(840, 142, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.CA.Business.JobDeclaration);
			// 
			// MessageInterpretation
			// 
			this.MessageInterpretationTabPage.Controls.Add(this.MessageInterpretationUserControl);
			this.MessageInterpretationTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.MessageInterpretationTabPage.Name = "MessageInterpretationTabPage";
			this.MessageInterpretationTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.MessageInterpretationTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(835, 309, true);
			this.MessageInterpretationTabPage.TabIndex = 1;
			this.MessageInterpretationTabPage.Text = "Interpretation";
			// 
			// MessageInterpretationUserControl
			// 
			this.BindingSource.SetBindingMember(this.MessageInterpretationUserControl, "Messages");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			this.MessageInterpretationUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MessageInterpretationUserControl.Font = new System.Drawing.Font("Courier New", 8F);
			this.MessageInterpretationUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.MessageInterpretationUserControl.Name = "MessageInterpretationUserControl";
			this.MessageInterpretationUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(829, 303, true);
			this.MessageInterpretationUserControl.TabIndex = 2;
			// 
			// StatementMessagesUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(507, 0, true);
			this.Name = "StatementMessagesUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(846, 497, true);
			this.MainPanel.ResumeLayout(false);
			this.MainPanel.PerformLayout();
			this.MessagesTabControl.ResumeLayout(false);
			this.MessagesTabControl.PerformLayout();
			this.MessageTextTabPage.ResumeLayout(false);
			this.MessageTextTabPage.PerformLayout();
			this.HistoryGroupBox.ResumeLayout(false);
			this.HistoryGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessagesGrid)).EndInit();
			this.MessagesGrid.ResumeLayout(false);
			this.MessagesGrid.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.MessageInterpretationTabPage.ResumeLayout(false);
			this.MessageInterpretationTabPage.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZTabPage MessageInterpretationTabPage;
		private Enterprise.Messaging.GUI.EDIMessageInterpretationUserControl MessageInterpretationUserControl;
	}
}
