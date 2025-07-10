using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.GUI.PlugIn
{
	partial class EntryManualReleaseForm : ZChildForm
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
		new void InitializeComponent()
		{
			this.ReleaseCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ReleaseDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.OkButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.AbortButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ReleaseDateEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 95, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(350, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.IT.Business.Declaration.EntryManualReleaseHandler);
			// 
			// ReleaseCodeTextBox
			// 
			this.BindingSource.SetBindingMember(this.ReleaseCodeTextBox, "ReleaseCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IT.Business.Declaration.EntryManualReleaseHandler)(null)).ReleaseCode)));
			this.ReleaseCodeTextBox.CaptionResourceString = Enterprise.Customs.IT.GUI.Res.GetData("d622839b-177f-4892-8065-1c6ec129d86c", "Release code");
			this.ReleaseCodeTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ReleaseCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(79, 21, true);
			this.ReleaseCodeTextBox.Name = "ReleaseCodeTextBox";
			this.ReleaseCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(85, 15, true);
			this.ReleaseCodeTextBox.TabIndex = 1;
			// 
			// ReleaseDateEdit
			// 
			this.ReleaseDateEdit.AllowDrop = true;
			this.ReleaseDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.ReleaseDateEdit, "ReleaseDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.IT.Business.Declaration.EntryManualReleaseHandler)(null)).ReleaseDate)));
			this.ReleaseDateEdit.CaptionResourceString = Enterprise.Customs.IT.GUI.Res.GetData("0F5844AC-54EE-48D8-ABA6-94F748B9EF93", "Release date");
			this.ReleaseDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(246, 21, true);
			this.ReleaseDateEdit.Name = "ReleaseDateEdit";
			this.ReleaseDateEdit.TabIndex = 2;
			// 
			// OkButton
			// 
			this.OkButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.OkButton.CaptionResourceString = Enterprise.Customs.IT.GUI.Res.GetData("BBF0789E-A048-4D51-BAA5-6DD45DA18B96", "OK");
			this.OkButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(89, 61, true);
			this.OkButton.Name = "OkButton";
			this.OkButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(79, 23, true);
			this.OkButton.TabIndex = 3;
			this.OkButton.ToolTipCaption = null;
			this.OkButton.Click += new System.EventHandler(this.OkButton_Click);
			// 
			// AbortButton
			// 
			this.AbortButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.AbortButton.CaptionResourceString = Enterprise.Customs.IT.GUI.Res.GetData("FA00500D-7895-47DA-AF8F-C10D2D748D42", "Cancel");
			this.AbortButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.AbortButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(178, 61, true);
			this.AbortButton.Name = "AbortButton";
			this.AbortButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(79, 23, true);
			this.AbortButton.TabIndex = 4;
			this.AbortButton.ToolTipCaption = null;
			this.AbortButton.Click += new System.EventHandler(this.AbortButton_Click);
			// 
			// EntryManualReleaseForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CancelButton = this.AbortButton;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(350, 119, true);
			this.Controls.Add(this.AbortButton);
			this.Controls.Add(this.OkButton);
			this.Controls.Add(this.ReleaseCodeTextBox);
			this.Controls.Add(this.ReleaseDateEdit);
			this.DataSourceType = typeof(Enterprise.Customs.IT.Business.Declaration.EntryManualReleaseHandler);
			this.Name = "EntryManualReleaseForm";
			this.Controls.SetChildIndex(this.ReleaseDateEdit, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.ReleaseCodeTextBox, 0);
			this.Controls.SetChildIndex(this.OkButton, 0);
			this.Controls.SetChildIndex(this.AbortButton, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ReleaseDateEdit.ResumeLayout(true);
			this.ReleaseDateEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.ZTextBox ReleaseCodeTextBox;
		internal Enterprise.ZArchitecture.GUI.ZDateEdit ReleaseDateEdit;
		internal ZArchitecture.GUI.ZButton OkButton;
		internal ZArchitecture.GUI.ZButton AbortButton;
	}
}
