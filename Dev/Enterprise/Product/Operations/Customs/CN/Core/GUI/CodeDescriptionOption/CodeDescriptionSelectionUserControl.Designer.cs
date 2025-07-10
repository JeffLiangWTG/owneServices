namespace Enterprise.Customs.CN.GUI
{
	partial class CodeDescriptionSelectionUserControl
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
			this.TextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.EditButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// TextBox
			// 
			this.TextBox.AllowDrop = true;
			this.TextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TextBox.Name = "TextBox";
			this.TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(306, 20, true);
			this.TextBox.TabIndex = 0;
			// 
			// EditButton
			// 
			this.EditButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.EditButton.CaptionResourceString = Enterprise.Customs.CN.GUI.Res.GetData("cae35f4e-c482-4ebb-95f2-b985a7beec1a", "...");
			this.EditButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(309, 0, true);
			this.EditButton.Name = "EditButton";
			this.EditButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(24, 20, true);
			this.EditButton.TabIndex = 1;
			this.EditButton.ToolTipCaption = null;
			// 
			// TextBoxWithButtonUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.TextBox);
			this.Controls.Add(this.EditButton);
			this.Name = "TextBoxWithButtonUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(333, 20, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.ZTextBox TextBox;
		internal ZArchitecture.GUI.ZButton EditButton;
	}
}
