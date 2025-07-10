namespace Enterprise.Registry.GUI
{
	partial class LoginPasswordListEditControlForRegistry
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			this.panel1 = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.buttonShowPasswords = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.CodeDescriptionGrid)).BeginInit();
			this.CodeDescriptionGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.panel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// CodeDescriptionGrid
			// 
			this.CodeDescriptionGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(296, 255, true);
			// 
			// panel1
			// 
			this.panel1.Controls.Add(this.buttonShowPasswords);
			this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.panel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 255, true);
			this.panel1.Name = "panel1";
			this.panel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(296, 33, true);
			this.panel1.TabIndex = 1;
			// 
			// buttonShowPasswords
			// 
			this.buttonShowPasswords.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 6, true);
			this.buttonShowPasswords.Name = "buttonShowPasswords";
			this.buttonShowPasswords.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.buttonShowPasswords.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(147, 23, true);
			this.buttonShowPasswords.TabIndex = 0;
			this.buttonShowPasswords.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.buttonShowPasswords.ToolTipCaption = null;
			this.buttonShowPasswords.UseVisualStyleBackColor = true;
			this.buttonShowPasswords.Click += new System.EventHandler(this.buttonShowPasswords_Click);
			// 
			// LoginPasswordListEditControlForRegistry
			// 
			this.Controls.Add(this.panel1);
			this.Name = "LoginPasswordListEditControlForRegistry";
			this.Controls.SetChildIndex(this.panel1, 0);
			this.Controls.SetChildIndex(this.CodeDescriptionGrid, 0);
			((System.ComponentModel.ISupportInitialize)(this.CodeDescriptionGrid)).EndInit();
			this.CodeDescriptionGrid.ResumeLayout(false);
			this.CodeDescriptionGrid.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.panel1.ResumeLayout(false);
			this.panel1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZPanel panel1;
		private ZArchitecture.GUI.ZButton buttonShowPasswords;
	}
}
