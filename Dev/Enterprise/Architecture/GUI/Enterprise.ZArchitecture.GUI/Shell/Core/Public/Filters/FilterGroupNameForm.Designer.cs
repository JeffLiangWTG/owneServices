namespace Enterprise.ZArchitecture.GUI
{
	public partial class FilterGroupNameForm
	{
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private new void InitializeComponent()
		{
			this.SaveFiltersTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CloseButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.SaveButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 82, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(431, 24, true);
			this.MainStatusBar.Visible = false;
			// 
			// SaveFiltersTextBox
			// 
			this.SaveFiltersTextBox.AcceptsReturn = true;
			this.SaveFiltersTextBox.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("FilterGroupNameForm|5905BAE3-DB04-4A5C-A01A-00695284E687", "Add Filter Group Name");
			this.SaveFiltersTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.SaveFiltersTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(162, 29, true);
			this.SaveFiltersTextBox.Name = "SaveFiltersTextBox";
			this.SaveFiltersTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(246, 20, true);
			this.SaveFiltersTextBox.TabIndex = 1;
			// 
			// CloseButton
			// 
			this.CloseButton.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("FilterGroupNameForm|6102497a-9757-42a4-969b-1dc8e0758ee1", "&Cancel");
			this.CloseButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(333, 71, true);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.CloseButton.TabIndex = 6;
			this.CloseButton.UseVisualStyleBackColor = true;
			// 
			// SaveButton
			// 
			this.SaveButton.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("FilterGroupNameForm|651e6eb5-a199-4928-8515-62ace89b9759", "&OK");
			this.SaveButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.SaveButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(252, 71, true);
			this.SaveButton.Name = "SaveButton";
			this.SaveButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.SaveButton.TabIndex = 5;
			this.SaveButton.UseVisualStyleBackColor = true;
			// 
			// FilterGroupNameForm
			// 
			this.AcceptButton = this.SaveButton;
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CancelButton = this.CloseButton;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("12205c44-e711-4955-864e-3d7180f0624b", "Save Filters Group Name");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(431, 106, true);
			this.Controls.Add(this.CloseButton);
			this.Controls.Add(this.SaveButton);
			this.Controls.Add(this.SaveFiltersTextBox);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Name = "FilterGroupNameForm";
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "FilterGroupNameForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.SaveFiltersTextBox, 0);
			this.Controls.SetChildIndex(this.SaveButton, 0);
			this.Controls.SetChildIndex(this.CloseButton, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
