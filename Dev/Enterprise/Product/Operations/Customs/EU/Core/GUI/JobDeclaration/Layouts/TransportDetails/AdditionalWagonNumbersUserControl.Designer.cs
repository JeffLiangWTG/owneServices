namespace Enterprise.Customs.EU.GUI
{
	partial class AdditionalWagonNumbersUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.AdditionalWagonNumbersButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.Business.Declaration.JobDeclaration);
			// 
			// AdditionalWagonNumbersButton
			// 
			this.AdditionalWagonNumbersButton.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("A0B5E3BC-52EB-4DB0-9FF0-6CC34F3EE70C", "More..");
			this.AdditionalWagonNumbersButton.Dock = System.Windows.Forms.DockStyle.Left;
			this.AdditionalWagonNumbersButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.AdditionalWagonNumbersButton.Name = "AdditionalWagonNumbersButton";
			this.AdditionalWagonNumbersButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(60, 20, true);
			this.AdditionalWagonNumbersButton.TabIndex = 1;
			this.AdditionalWagonNumbersButton.ToolTipCaption = null;
			this.AdditionalWagonNumbersButton.Click += new System.EventHandler(this.AdditionalWagonNumbersButton_Click);
			// 
			// AdditionalWagonNumbersUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.AdditionalWagonNumbersButton);
			this.Name = "AdditionalWagonNumbersUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(219, 20, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal Enterprise.ZArchitecture.GUI.ZButton AdditionalWagonNumbersButton;

	}
}
