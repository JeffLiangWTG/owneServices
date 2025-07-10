using Enterprise.Messaging.GUI;

namespace Enterprise.Accounting.GUI.ARAP
{
	public partial class LinkedeNettEDIMessageStandAloneUserControl
	{
void InitializeComponent()
		{
			this.linkedeNettEDIMessageUserControl1 = new LinkedeNettEDIMessageUserControl();
			this.messageDetailsPanel.SuspendLayout();
			this.processingDetailsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// messageDetailsPanel
			// 
			this.messageDetailsPanel.Visible = false;
			// 
			// messageContentsPanel
			// 
			this.messageContentsPanel.Visible = false;
			// 
			// MessageTextTextBox
			// 
			this.MessageTextTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.MessageTextTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(536, 262, true);
			// 
			// linkedeNettEDIMessageUserControl1
			// 
			this.linkedeNettEDIMessageUserControl1.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.linkedeNettEDIMessageUserControl1, ".");
			this.linkedeNettEDIMessageUserControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.linkedeNettEDIMessageUserControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(297, 0, true);
			this.linkedeNettEDIMessageUserControl1.Name = "linkedeNettEDIMessageUserControl1";
			this.linkedeNettEDIMessageUserControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(542, 338, true);
			this.linkedeNettEDIMessageUserControl1.TabIndex = 24;
			// 
			// LinkedeNettEDIMessageStandAloneUserControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.Controls.Add(this.linkedeNettEDIMessageUserControl1);
			this.Name = "LinkedeNettEDIMessageStandAloneUserControl";
			this.Controls.SetChildIndex(this.messageDetailsPanel, 0);
			this.Controls.SetChildIndex(this.messageContentsPanel, 0);
			this.Controls.SetChildIndex(this.linkedeNettEDIMessageUserControl1, 0);
			this.messageDetailsPanel.ResumeLayout(false);
			this.processingDetailsGroupBox.ResumeLayout(false);
			this.processingDetailsGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
		}

	}
}