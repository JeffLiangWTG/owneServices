namespace Enterprise.Customs.JP.AFR.GUI
{
	partial class AFRReporterIDControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			this.ReporterIDTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.PasswordTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ViewButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.JP.AFR.Business.AFRReporterID);
			// 
			// ReporterIDTextBox
			// 
			this.ReporterIDTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ReporterIDTextBox, "ReporterID");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.AFR.Business.AFRReporterID)(null)).ReporterID)));
			this.ReporterIDTextBox.CaptionResourceString = Enterprise.Customs.JP.AFR.GUI.Res.GetData("3d22652a-c5df-4682-a346-71f1dab129b6", "Reporter ID");
			this.ReporterIDTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 20, true);
			this.ReporterIDTextBox.Name = "ReporterIDTextBox";
			this.ReporterIDTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 20, true);
			this.ReporterIDTextBox.TabIndex = 2;
			// 
			// PasswordTextBox
			// 
			this.PasswordTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.PasswordTextBox, "Password");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.AFR.Business.AFRReporterID)(null)).Password)));
			this.PasswordTextBox.CaptionResourceString = Enterprise.Customs.JP.AFR.GUI.Res.GetData("be68624c-e493-4360-aaad-632ce7a66123", "Password");
			this.PasswordTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 46, true);
			this.PasswordTextBox.Name = "PasswordTextBox";
			this.PasswordTextBox.PasswordChar = '*';
			this.PasswordTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 20, true);
			this.PasswordTextBox.TabIndex = 3;
			this.PasswordTextBox.UseSystemPasswordChar = true;
			// 
			// ViewButton
			// 
			this.ViewButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.ViewButton.CaptionResourceString = Enterprise.Customs.JP.AFR.GUI.Res.GetData("699e542b-ed3f-431a-8f87-190a2d02c307", "View");
			this.ViewButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(286, 43, true);
			this.ViewButton.Name = "ViewButton";
			this.ViewButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.ViewButton.TabIndex = 4;
			this.ViewButton.ToolTipCaption = null;
			this.ViewButton.Click += new System.EventHandler(this.ViewButton_Click);
			// 
			// AFRReporterIDControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ViewButton);
			this.Controls.Add(this.PasswordTextBox);
			this.Controls.Add(this.ReporterIDTextBox);
			this.Name = "AFRReporterIDControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(433, 83, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		public ZArchitecture.ZTextBox ReporterIDTextBox;
		public ZArchitecture.ZTextBox PasswordTextBox;
		private ZArchitecture.GUI.ZButton ViewButton;
	}
}
