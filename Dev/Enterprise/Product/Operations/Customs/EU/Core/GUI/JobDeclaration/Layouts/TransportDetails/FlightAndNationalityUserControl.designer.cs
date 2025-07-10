namespace Enterprise.Customs.EU.GUI
{
	partial class FlightAndNationalityUserControl
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
			this.TransportNationalityFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.FlightNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.TransportNationalityFindBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.Business.BaseJobDeclaration);
			// 
			// TransportNationalityFindBox
			// 
			this.TransportNationalityFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TransportNationalityFindBox, "JE_RN_NKTransportNationality");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).JE_RN_NKTransportNationality)));
			this.TransportNationalityFindBox.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("DE8E3895-D5BB-4C9C-960B-CE3D9259007F", "Nat.", "Nationality", "[UCC 7/15] Nationality");
			this.TransportNationalityFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(287, 0, true);
			this.TransportNationalityFindBox.Name = "TransportNationalityFindBox";
			this.TransportNationalityFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.TransportNationalityFindBox.ParentType = null;
			this.TransportNationalityFindBox.PreBoundMaxLength = 2;
			this.TransportNationalityFindBox.ShowDescriptionBox = false;
			this.TransportNationalityFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(47, 20, true);
			this.TransportNationalityFindBox.TabIndex = 1;
			// 
			// FlightNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.FlightNumberTextBox, "JE_VoyageFlightNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseJobDeclaration)(null)).JE_VoyageFlightNo)));
			this.FlightNumberTextBox.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("441C0CCA-B276-4559-B30A-9279A3AE3486", "Flight", "[UCC 7/9] Flight No.", "");
			this.FlightNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.FlightNumberTextBox.Name = "FlightNumberTextBox";
			this.FlightNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(188, 20, true);
			this.FlightNumberTextBox.TabIndex = 0;
			// 
			// FlightAndNationalityUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.FlightNumberTextBox);
			this.Controls.Add(this.TransportNationalityFindBox);
			this.Name = "FlightAndNationalityUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(338, 23, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.TransportNationalityFindBox.ResumeLayout(true);
			this.TransportNationalityFindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal Enterprise.ZArchitecture.ZTextBox FlightNumberTextBox;
		internal Enterprise.ZArchitecture.GUI.ZCodeFindBox TransportNationalityFindBox;
	}
}
