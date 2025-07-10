namespace Enterprise.Customs.ES.TemporaryStorage.GUI
{
	partial class AssignLocationAndReferenceForm
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
		private new void InitializeComponent()
		{
			this.OKButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.Cancel_Button = new Enterprise.ZArchitecture.GUI.ZButton();
			this.LocationTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.EmptyLocationCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ReferenceTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.EmptyReferenceCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 170, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(447, 24, true);
			// 
			// OKButton
			// 
			this.OKButton.CaptionResourceString = Enterprise.Customs.ES.TemporaryStorage.GUI.Res.GetData("2F541147-27C3-4D57-B570-690B43C679EC", "&OK");
			this.OKButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.OKButton.IsCaptionOverridden = false;
			this.OKButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(163, 130, true);
			this.OKButton.Name = "OKButton";
			this.OKButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.OKButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.OKButton.TabIndex = 5;
			this.OKButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.OKButton.ToolTipCaption = null;
			this.OKButton.UseVisualStyleBackColor = true;
			// 
			// Cancel_Button
			// 
			this.Cancel_Button.CaptionResourceString = Enterprise.Customs.ES.TemporaryStorage.GUI.Res.GetData("2C7032F1-0E75-4EC1-89BD-53AD4782626A", "&Cancel");
			this.Cancel_Button.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.Cancel_Button.IsCaptionOverridden = false;
			this.Cancel_Button.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(242, 130, true);
			this.Cancel_Button.Name = "Cancel_Button";
			this.Cancel_Button.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.Cancel_Button.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(76, 23, true);
			this.Cancel_Button.TabIndex = 6;
			this.Cancel_Button.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.Cancel_Button.ToolTipCaption = null;
			this.Cancel_Button.UseVisualStyleBackColor = true;
			// 
			// LocationTextBox
			// 
			this.LocationTextBox.CaptionResourceString = Enterprise.Customs.ES.TemporaryStorage.GUI.Res.GetData("6D49B1B2-D51A-41A7-8D4E-161D979CC01D", "Location of Goods");
			this.LocationTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(117, 28, true);
			this.LocationTextBox.Name = "LocationTextBox";
			this.LocationTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.LocationTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 20, true);
			this.LocationTextBox.MaxLength = 35;
			this.LocationTextBox.TabIndex = 1;
			// 
			// EmptyLocationCheckBox
			// 
			this.EmptyLocationCheckBox.CaptionResourceString = Enterprise.Customs.ES.TemporaryStorage.GUI.Res.GetData("F39EECF1-69B7-4358-9287-069061ED2CEA", "Leave Empty");
			this.EmptyLocationCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(395, 28, true);
			this.EmptyLocationCheckBox.Name = "EmptyLocationCheckBox";
			this.EmptyLocationCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(115, 20, true);
			this.EmptyLocationCheckBox.TabIndex = 2;
			// 
			// ReferenceTextBox
			// 
			this.ReferenceTextBox.CaptionResourceString = Enterprise.Customs.ES.TemporaryStorage.GUI.Res.GetData("2A171AE1-DB6F-46D3-A7C9-86B464C4393B", "Owner Reference");
			this.ReferenceTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(117, 58, true);
			this.ReferenceTextBox.Name = "ReferenceTextBox";
			this.ReferenceTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.ReferenceTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 40, true);
			this.ReferenceTextBox.MaxLength = 50;
			this.ReferenceTextBox.Multiline = true;
			this.ReferenceTextBox.TabIndex = 3;
			// 
			// EmptyReferenceCheckBox
			//
			this.EmptyReferenceCheckBox.CaptionResourceString = Enterprise.Customs.ES.TemporaryStorage.GUI.Res.GetData("FD0F1ED3-77A9-4DCA-A3DE-029C0BCB04BA", "Leave Empty");
			this.EmptyReferenceCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(395, 58, true);
			this.EmptyReferenceCheckBox.Name = "EmptyReferenceCheckBox";
			this.EmptyReferenceCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(115, 20, true);
			this.EmptyReferenceCheckBox.TabIndex = 4;
			// 
			// AssignLocationAndReferenceForm
			// 
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(515, 190, true);
			this.Controls.Add(this.LocationTextBox);
			this.Controls.Add(this.EmptyLocationCheckBox);
			this.Controls.Add(this.ReferenceTextBox);
			this.Controls.Add(this.EmptyReferenceCheckBox);
			this.Controls.Add(this.OKButton);
			this.Controls.Add(this.Cancel_Button);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Name = "AssignLocationAndReferenceForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Assign Location + ReferenceForm";
			this.Controls.SetChildIndex(this.Cancel_Button, 0);
			this.Controls.SetChildIndex(this.OKButton, 0);
			this.Controls.SetChildIndex(this.EmptyReferenceCheckBox, 0);
			this.Controls.SetChildIndex(this.ReferenceTextBox, 0);
			this.Controls.SetChildIndex(this.EmptyLocationCheckBox, 0);
			this.Controls.SetChildIndex(this.LocationTextBox, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		public ZArchitecture.ZTextBox LocationTextBox;
		public ZArchitecture.GUI.ZCheckBox EmptyLocationCheckBox;
		public ZArchitecture.ZTextBox ReferenceTextBox;
		public ZArchitecture.GUI.ZCheckBox EmptyReferenceCheckBox;
		private ZArchitecture.GUI.ZButton OKButton;
		private ZArchitecture.GUI.ZButton Cancel_Button;
	}
}
