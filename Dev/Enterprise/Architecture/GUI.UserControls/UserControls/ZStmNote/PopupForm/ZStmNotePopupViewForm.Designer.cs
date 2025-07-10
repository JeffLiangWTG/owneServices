using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.GUI
{
	partial class ZStmNotePopupViewForm
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
		protected sealed override void InitializeComponent()
		{
			this.NoteUserControl = new Enterprise.ZArchitecture.GUI.ZStmNotePopupUserControl();
			this.CloseButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.leadingCommentLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 353, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(476, 24, true);
			this.MainStatusBar.Visible = false;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.ZArchitecture.Business.StmNote);
			// 
			// NoteUserControl
			// 
			this.NoteUserControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.NoteUserControl, ".");
			this.NoteUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 4, true);
			this.NoteUserControl.Name = "NoteUserControl";
			this.NoteUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(458, 336, true);
			this.NoteUserControl.TabIndex = 0;
			// 
			// CloseButton
			// 
			this.CloseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CloseButton.CaptionResourceString = Enterprise.ZArchitecture.GUI.UserControls.Res.GetData("ZStmNotePopupViewForm|0ea855b9-2279-420b-9392-92e18910781f", "&Close");
			this.CloseButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(386, 344, true);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.CloseButton.TabIndex = 2;
			this.CloseButton.Click += new System.EventHandler(this.CloseButton_Click);
			// 
			// leadingCommentLabel
			// 
			this.leadingCommentLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.leadingCommentLabel, false);
			this.leadingCommentLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 3, true);
			this.leadingCommentLabel.Name = "leadingCommentLabel";
			this.leadingCommentLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(458, 0, true);
			this.leadingCommentLabel.TabIndex = 3;
			this.leadingCommentLabel.Visible = false;
			// 
			// ZStmNotePopupViewForm
			// 
			this.AcceptButton = this.CloseButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CancelButton = this.CloseButton;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(476, 377, true);
			this.Controls.Add(this.leadingCommentLabel);
			this.Controls.Add(this.CloseButton);
			this.Controls.Add(this.NoteUserControl);
			this.DataSourceType = typeof(Enterprise.ZArchitecture.Business.StmNote);
			this.MinimizeBox = false;
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(424, 248, true);
			this.Name = "ZStmNotePopupViewForm";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Note";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.NoteUserControl, 0);
			this.Controls.SetChildIndex(this.CloseButton, 0);
			this.Controls.SetChildIndex(this.leadingCommentLabel, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private ZButton CloseButton;
		internal ZStmNotePopupUserControl NoteUserControl;
		private ZLabel leadingCommentLabel;
	}
}
