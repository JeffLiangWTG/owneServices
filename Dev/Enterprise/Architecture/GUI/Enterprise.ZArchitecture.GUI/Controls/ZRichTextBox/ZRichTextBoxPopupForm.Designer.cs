using System;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Windows.UI;

namespace Enterprise.ZArchitecture.GUI.RichEdit
{
	public partial class ZRichTextBoxPopupForm
	{
		ZButton ApplyButton;
		ZRichTextBox RichEdit;

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
			this.ApplyButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.RichEdit = new Enterprise.ZArchitecture.GUI.ZRichTextBox();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 526, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(672, 24, true);
			// 
			// ApplyButton
			// 
			this.ApplyButton.Anchor = (System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right);
			this.ApplyButton.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("ZRichTextBoxPopupForm|66ada5cd-3177-4f22-8e60-54ed163a05bf", "&Close");
			this.ApplyButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(596, 498, true);
			this.ApplyButton.Name = "ApplyButton";
			this.ApplyButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 22, true);
			this.ApplyButton.TabIndex = 2;
			this.ApplyButton.Click += new System.EventHandler(this.OnCloseButton_Click);
			// 
			// RichEdit
			// 
			this.RichEdit.Anchor = (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right);
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.RichEdit, false);
			this.RichEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 7, true);
			this.RichEdit.MaxLength = 10000000;
			this.RichEdit.Name = "RichEdit";
			this.RichEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(672, 485, true);
			this.RichEdit.TabIndex = 1;
			// 
			// ZRichTextBoxPopupForm
			// 

			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(672, 550, true);
			this.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("ZRichTextBoxPopupForm|9f290212-68aa-4df0-a637-b0f9d4247103", "Edit Text");
			this.Controls.Add(this.RichEdit);
			this.Controls.Add(this.ApplyButton);
			this.Name = "ZRichTextBoxPopupForm";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ZRichTextBoxPopupForm_FormClosing);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.ApplyButton, 0);
			this.Controls.SetChildIndex(this.RichEdit, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
		}
	}
}
