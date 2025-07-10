namespace Enterprise.Customs.JP.Manifest.GUI
{
	partial class ConsolidatorUserControl
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
			this.ConsolidatorNACCSUserCodeTextBox = new ZArchitecture.ZTextBox();
			this.ConsolidatorAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.ConsolidatorGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ConsolidatorAddressControl.SuspendLayout();
			this.ConsolidatorGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.JP.Manifest.Business.AsycudaManifestHeader);
			// 
			// ConsolidatorAddressControl
			// 
			this.ConsolidatorAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ConsolidatorAddressControl, "AMA_OA_Consolidator");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.JP.Manifest.Business.AsycudaManifestHeader)(null)).AMA_OA_Consolidator)));
			this.ConsolidatorAddressControl.BindToOrgList = "Lookups+Organisations";
			this.ConsolidatorAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 15, true);
			this.ConsolidatorAddressControl.CaptionResourceString = Enterprise.Customs.JP.Manifest.GUI.Res.GetData("C2106848-8237-4E38-ABCC-A7CBE10605A1", "Address");
			this.ConsolidatorAddressControl.Name = "ConsolidatorAddressControl";
			this.ConsolidatorAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(210, 20, true);
			this.ConsolidatorAddressControl.TabIndex = 1;
			this.ConsolidatorAddressControl.ShowAddress = false;
			// 
			// ConsolidatorNACCSUserCodeTextBox
			// 
			this.BindingSource.SetBindingMember(this.ConsolidatorNACCSUserCodeTextBox, "ConsolidatorNACCSUserCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.Manifest.Business.AsycudaManifestHeader)(null)).ConsolidatorNACCSUserCode)));
			this.ConsolidatorNACCSUserCodeTextBox.CaptionResourceString = Enterprise.Customs.JP.Manifest.GUI.Res.GetData("688B8B55-F145-4289-A658-FDBD4324DC3D", "NACCS User Code");
			this.ConsolidatorNACCSUserCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 39, true);
			this.ConsolidatorNACCSUserCodeTextBox.Name = "ConsolidatorNACCSUserCodeTextBox";
			this.ConsolidatorNACCSUserCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(210, 20, true);
			this.ConsolidatorNACCSUserCodeTextBox.TabIndex = 2;
			this.ConsolidatorNACCSUserCodeTextBox.Multiline = false;
			//
			// ConsolidatorGroupBox
			//
			this.ConsolidatorGroupBox.CaptionResourceString = Enterprise.Customs.JP.Manifest.GUI.Res.GetData("CFE653C0-8F94-4701-A762-1EDDD6A2A30D", "Consolidator");
			this.ConsolidatorGroupBox.Controls.Add(this.ConsolidatorAddressControl);
			this.ConsolidatorGroupBox.Controls.Add(this.ConsolidatorNACCSUserCodeTextBox);
			this.ConsolidatorGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.ConsolidatorGroupBox.Name = "ConsolidatorGroupBox";
			this.ConsolidatorGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(320, 70, true);
			this.ConsolidatorGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ConsolidatorGroupBox.TabIndex = 0;
			// 
			// ConsolidatorUserControl
			//
			this.Controls.Add(this.ConsolidatorGroupBox);
			this.Name = "ConsolidatorUserControl";
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(400, 70, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ConsolidatorGroupBox.ResumeLayout(false);
			this.ConsolidatorGroupBox.PerformLayout();
			this.ConsolidatorAddressControl.ResumeLayout(true);
			this.ConsolidatorAddressControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		ZArchitecture.GUI.ZGroupBox ConsolidatorGroupBox;
		ZArchitecture.ZTextBox ConsolidatorNACCSUserCodeTextBox;
		ZArchitecture.GUI.ZAddressControl ConsolidatorAddressControl;
	}
}
