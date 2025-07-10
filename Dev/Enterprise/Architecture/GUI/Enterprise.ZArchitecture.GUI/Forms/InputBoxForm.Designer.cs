using System.Drawing;
using CargoWise.Windows.UI;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.Core.Forms
{
	partial class InputBoxForm
	{
		string fReturnValue;
		Point fStartLocation;
		KButton OKButton;
		KTextBox ResultTextEdit;
		KButton CancelBtn;
		KLabel MessageText;
		ZArchitecture.GUI.ZPanel MessageAutoSizingWrapperPanel;

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.OKButton = new CargoWise.Windows.UI.KButton();
			this.ResultTextEdit = new CargoWise.Windows.UI.KTextBox();
			this.MessageText = new CargoWise.Windows.UI.KLabel();
			this.CancelBtn = new CargoWise.Windows.UI.KButton();
			this.MessageAutoSizingWrapperPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.MessageAutoSizingWrapperPanel.SuspendLayout();
			this.SuspendLayout();
			//
			// OKButton
			//
			this.OKButton.Anchor = (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right);
			this.OKButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(280, 8, true);
			this.OKButton.Name = "OKButton";
			this.OKButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.OKButton.TabIndex = 2;
			this.OKButton.Text = Res.GetString("160810fa-cc01-483c-b17e-8bd4367cee8c", "OK");
			this.OKButton.Click += new System.EventHandler(this.btnOK_Click);
			//
			// ResultTextEdit
			//
			this.ResultTextEdit.Anchor = ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right);
			this.ResultTextEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 80, true);
			this.ResultTextEdit.Name = "ResultTextEdit";
			this.ResultTextEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(344, 20, true);
			this.ResultTextEdit.TabIndex = 1;
			//
			// MessageText
			//
			this.MessageText.AutoSize = true;
			this.MessageText.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MessageText.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MessageText.Name = "MessageText";
			this.MessageText.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(49, 13, true);
			this.MessageText.TabIndex = 0;
			this.MessageText.Text = "InputBox";
			//
			// CancelBtn
			//
			this.CancelBtn.Anchor = (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right);
			this.CancelBtn.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CancelBtn.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(280, 40, true);
			this.CancelBtn.Name = "CancelBtn";
			this.CancelBtn.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.CancelBtn.TabIndex = 3;
			this.CancelBtn.Text = Res.GetString("4d9cae78-c7cd-4758-b3b9-eb8b78d24f45", "Cancel");
			this.CancelBtn.Click += new System.EventHandler(this.btnCancel_Click);
			//
			// MessageAutoSizingWrapperPanel
			//
			this.MessageAutoSizingWrapperPanel.Anchor = (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right);
			this.MessageAutoSizingWrapperPanel.Controls.Add(this.MessageText);
			this.MessageAutoSizingWrapperPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 8, true);
			this.MessageAutoSizingWrapperPanel.Name = "MessageAutoSizingWrapperPanel";
			this.MessageAutoSizingWrapperPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(266, 66, true);
			this.MessageAutoSizingWrapperPanel.TabIndex = 4;
			//
			// InputBoxForm
			//
			this.AcceptButton = this.OKButton;
			this.AutoSize = true;
			this.CancelButton = this.CancelBtn;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(362, 111, true);
			this.Controls.Add(this.MessageAutoSizingWrapperPanel);
			this.Controls.Add(this.CancelBtn);
			this.Controls.Add(this.ResultTextEdit);
			this.Controls.Add(this.OKButton);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "InputBoxForm";
			this.Text = "InputBox";
			this.Load += new System.EventHandler(this.InputBoxForm_Load);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.MessageAutoSizingWrapperPanel.ResumeLayout(false);
			this.MessageAutoSizingWrapperPanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
