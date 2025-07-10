namespace Enterprise.Customs.EU.Manifest.ICS2.GUI
{
	partial class ReceptacleUserControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.ReceptacleIdTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ReceptacleEditButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.Manifest.ICS2.Business.AsycudaManifestHeader);
			// 
			// ReceptacleIdTextBox
			// 
			this.BindingSource.SetBindingMember(this.ReceptacleIdTextBox, "ReceptacleId");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Manifest.ICS2.Business.AsycudaManifestHeader)(null)).ReceptacleId)));
			this.ReceptacleIdTextBox.CaptionResourceString = null;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ReceptacleIdTextBox, false);
			this.ReceptacleIdTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ReceptacleIdTextBox.Name = "ReceptacleIdTextBox";
			this.ReceptacleIdTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(184, 15, true);
			this.ReceptacleIdTextBox.TabIndex = 0;
			// 
			// ReceptacleEditButton
			// 
			this.ReceptacleEditButton.CaptionResourceString = Enterprise.Customs.EU.Manifest.ICS2.GUI.Res.GetData("ReceptacleEditButton|88945801-349C-4CB1-8272-371A13D75D86", "Receptacle ID(s)");
			this.ReceptacleEditButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(188, 0, true);
			this.ReceptacleEditButton.Name = "ReceptacleEditButton";
			this.ReceptacleEditButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(105, 20, true);
			this.ReceptacleEditButton.TabIndex = 1;
			this.ReceptacleEditButton.ToolTipCaption = null;
			this.ReceptacleEditButton.Click += new System.EventHandler(this.ReceptaclesEditButton_Click);
			// 
			// ReceptacleUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ReceptacleEditButton);
			this.Controls.Add(this.ReceptacleIdTextBox);
			this.Name = "ReceptacleUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 22, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.ZTextBox ReceptacleIdTextBox;
		internal ZArchitecture.GUI.ZButton ReceptacleEditButton;
	}
}
