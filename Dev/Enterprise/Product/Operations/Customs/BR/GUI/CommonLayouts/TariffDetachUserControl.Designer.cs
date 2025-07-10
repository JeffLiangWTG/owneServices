namespace Enterprise.Customs.BR.GUI
{
	partial class TariffDetachUserControl
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

			this.TariffDetachGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.TariffDetachButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.TariffDetachTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.TariffDetachGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.BR.Business.TariffDetachCollection);
			// 
			// TariffDetachGroupBox
			// 
			this.TariffDetachGroupBox.CaptionResourceString = Enterprise.Customs.BR.GUI.Res.GetData("111D23F3-EAD4-4A41-AE75-AAD233D823FA", "Tariff Detach");
			this.TariffDetachGroupBox.Controls.Add(this.TariffDetachButton);
			this.TariffDetachGroupBox.Controls.Add(this.TariffDetachTextBox);
			this.TariffDetachGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.TariffDetachGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TariffDetachGroupBox.Name = "TariffDetachGroupBox";
			this.TariffDetachGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(983, 50, true);
			this.TariffDetachGroupBox.TabIndex = 2;
			this.TariffDetachGroupBox.TabStop = false;
			// 
			// TariffDetachButton
			// 
			this.TariffDetachButton.CaptionResourceString = Enterprise.Customs.BR.GUI.Res.GetData("2426061c-607c-450a-8a42-3a26ac96dc64", "More...");
			this.TariffDetachButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(380, 19, true);
			this.TariffDetachButton.Name = "TariffDetachButton";
			this.TariffDetachButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(58, 20, true);
			this.TariffDetachButton.TabIndex = 1;
			this.TariffDetachButton.ToolTipCaption = null;
			this.TariffDetachButton.Click += new System.EventHandler(this.TariffDetach_Button_Click);
			// 
			// TariffDetachTextBox
			// 
			this.BindingSource.SetBindingMember(this.TariffDetachTextBox, "TariffDetachConcatenated");
			this.TariffDetachTextBox.CaptionResourceString = Enterprise.Customs.BR.GUI.Res.GetData("7d0c8270-787e-4542-ac72-0439856c237d", "Tariff Detach");
			this.TariffDetachTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(98, 19, true);
			this.TariffDetachTextBox.Name = "TariffDetachTextBox";
			this.TariffDetachTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(279, 20, true);
			this.TariffDetachTextBox.TabIndex = 0;
			// 
			// TariffDetachUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.TariffDetachGroupBox);
			this.Name = "TariffDetachUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(983, 50, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.TariffDetachGroupBox.ResumeLayout(false);
			this.TariffDetachGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.GUI.ZGroupBox TariffDetachGroupBox;
		internal ZArchitecture.ZTextBox TariffDetachTextBox;
		internal ZArchitecture.GUI.ZButton TariffDetachButton;
	}
}
