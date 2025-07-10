using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.BR.GUI
{
	partial class MultiCodesSelectForm
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

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
			this.OKButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CancelButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CheckedListGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.CheckedListBox = new Enterprise.ZArchitecture.GUI.ZCheckedListBox();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.CheckedListGroupBox.SuspendLayout();
			this.SuspendLayout();
			//
			// MainStatusBar
			//
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 165, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 24, true);
			//
			// BindingSource
			//
			this.BindingSource.DataSourceType = typeof(Enterprise.ZArchitecture.Core.CodeDescriptionPairList);
			//
			// OKButton
			//
			this.OKButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.OKButton.CaptionResourceString = Enterprise.Customs.BR.GUI.Res.GetData("MultiCodesSelectForm|08AC65D2-24FF-4469-BD74-F41D15C4A731", "OK");
			this.OKButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(132, 138, true);
			this.OKButton.Name = "OKButton";
			this.OKButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.OKButton.TabIndex = 2;
			this.OKButton.ToolTipCaption = null;
			this.OKButton.UseVisualStyleBackColor = true;
			this.OKButton.Click += new System.EventHandler(this.OKButton_Click);
			//
			// CancelButton
			//
			this.CancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CancelButton.CaptionResourceString = Enterprise.Customs.BR.GUI.Res.GetData("MultiCodesSelectForm|76B32E0E-D782-41C9-82E2-F1256D7B671B", "Cancel");
			this.CancelButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(213, 138, true);
			this.CancelButton.Name = "CancelButton";
			this.CancelButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.CancelButton.TabIndex = 3;
			this.CancelButton.ToolTipCaption = null;
			this.CancelButton.UseVisualStyleBackColor = true;
			this.CancelButton.Click += new System.EventHandler(this.CancelButton_Click);
			//
			// CheckedListGroupBox
			//
			this.CheckedListGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
			this.CheckedListGroupBox.CaptionResourceString = Enterprise.Customs.BR.GUI.Res.GetData("MultiCodesSelectForm|137805F0-2E33-450C-ABED-404A34FC8110", "Select Values");
			this.CheckedListGroupBox.Controls.Add(this.CheckedListBox);
			this.CheckedListGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.CheckedListGroupBox.Name = "CheckedListGroupBox";
			this.CheckedListGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 135, true);
			this.CheckedListGroupBox.TabIndex = 0;
			this.CheckedListGroupBox.TabStop = false;
			//
			// CheckedListBox
			//
			this.CheckedListBox.BindingItems = null;
			this.BindingSource.SetBindingMember(this.CheckedListBox, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.ZArchitecture.Business.ZBoolDescriptionPairList)(((CargoWise.Integration.ICodeDescription)(null)))));
			this.CheckedListBox.CheckOnClick = true;
			this.CheckedListBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CheckedListBox.FormattingEnabled = true;
			this.CheckedListBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.CheckedListBox.Name = "CheckedListBox";
			this.CheckedListBox.ScrollAlwaysVisible = true;
			this.CheckedListBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(294, 116, true);
			this.CheckedListBox.TabIndex = 1;
			//
			// MultiCodesSelectForm
			//
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 189, true);
			this.Controls.Add(this.OKButton);
			this.Controls.Add(this.CancelButton);
			this.Controls.Add(this.CheckedListGroupBox);
			this.DataSourceType = typeof(Enterprise.ZArchitecture.Core.CodeDescriptionPairList);
			this.Name = "MultiCodesSelectForm";
			this.Controls.SetChildIndex(this.CheckedListGroupBox, 0);
			this.Controls.SetChildIndex(this.CancelButton, 0);
			this.Controls.SetChildIndex(this.OKButton, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.CheckedListGroupBox.ResumeLayout(false);
			this.CheckedListGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		internal ZArchitecture.GUI.ZButton OKButton;
		internal new ZArchitecture.GUI.ZButton CancelButton;
		internal ZGroupBox CheckedListGroupBox;
		internal ZCheckedListBox CheckedListBox;
	}
}
