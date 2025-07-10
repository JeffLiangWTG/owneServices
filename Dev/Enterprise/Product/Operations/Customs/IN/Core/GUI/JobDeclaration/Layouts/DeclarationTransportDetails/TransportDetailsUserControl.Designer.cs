namespace Enterprise.Customs.IN.GUI
{
	partial class TransportDetailsUserControl
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
			this.LoadingAndDestinationInformationSeparatorUserControl = new Enterprise.ZArchitecture.GUI.SeparatorUserControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.LoadingAndDestinationInformationSeparatorUserControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.Business.BaseJobDeclaration);
			// 
			// LoadingAndDestinationInformationSeparatorUserControl
			// 
			this.LoadingAndDestinationInformationSeparatorUserControl.AllowDrop = true;
			this.LoadingAndDestinationInformationSeparatorUserControl.CaptionResourceString = Enterprise.Customs.IN.GUI.Res.GetData("9c1971f8-66a4-4d02-85f8-b3267f6c3a4c", "Loading & Destination Information");
			this.LoadingAndDestinationInformationSeparatorUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(40, 17, true);
			this.LoadingAndDestinationInformationSeparatorUserControl.Name = "LoadingAndDestinationInformationSeparatorUserControl";
			this.LoadingAndDestinationInformationSeparatorUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(335, 15, true);
			this.LoadingAndDestinationInformationSeparatorUserControl.TabIndex = 22;
			// 
			// TransportDetailsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.LoadingAndDestinationInformationSeparatorUserControl);
			this.Name = "TransportDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(403, 49, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.LoadingAndDestinationInformationSeparatorUserControl.ResumeLayout(true);
			this.LoadingAndDestinationInformationSeparatorUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.GUI.SeparatorUserControl LoadingAndDestinationInformationSeparatorUserControl;
	}
}
