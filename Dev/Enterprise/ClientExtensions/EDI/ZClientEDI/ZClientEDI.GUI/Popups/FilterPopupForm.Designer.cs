using System.ComponentModel;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.EDI.Popups;

partial class FilterPopupForm
{
	/// <summary>
	/// Required designer variable.
	/// </summary>
	private IContainer components = null;

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
	protected override void InitializeComponent()
	{
			this.ApplyBtn = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CancelBtn = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 276, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(533, 24, true);
			// 
			// ApplyBtn
			// 
			this.ApplyBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.ApplyBtn.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.ApplyBtn.IsCaptionOverridden = true;
			this.ApplyBtn.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(387, 242, true);
			this.ApplyBtn.Name = "ApplyBtn";
			this.ApplyBtn.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(61, 23, true);
			this.ApplyBtn.TabIndex = 2;
			this.ApplyBtn.Text = "Apply";
			this.ApplyBtn.ToolTipCaption = null;
			this.ApplyBtn.Click += new System.EventHandler(this.ApplyBtn_Click);
			// 
			// CancelBtn
			// 
			this.CancelBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CancelBtn.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CancelBtn.IsCaptionOverridden = true;
			this.CancelBtn.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(457, 242, true);
			this.CancelBtn.Name = "CancelBtn";
			this.CancelBtn.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(61, 23, true);
			this.CancelBtn.TabIndex = 3;
			this.CancelBtn.Text = "Cancel";
			this.CancelBtn.ToolTipCaption = null;
			// 
			// FilterPopupForm
			// 
			this.AcceptButton = this.ApplyBtn;
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CancelButton = this.CancelBtn;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(533, 300, true);
			this.Controls.Add(this.CancelBtn);
			this.Controls.Add(this.ApplyBtn);
			this.Name = "FilterPopupForm";
			this.Text = "Filters";
			this.Controls.SetChildIndex(this.ApplyBtn, 0);
			this.Controls.SetChildIndex(this.CancelBtn, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

	}

	#endregion

	private ZArchitecture.GUI.ZButton ApplyBtn;
	private ZArchitecture.GUI.ZButton CancelBtn;
}

