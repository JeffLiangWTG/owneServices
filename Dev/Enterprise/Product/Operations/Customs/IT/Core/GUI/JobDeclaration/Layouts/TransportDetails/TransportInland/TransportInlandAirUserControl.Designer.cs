namespace Enterprise.Customs.IT.GUI
{
	partial class TransportInlandAirUserControl
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
			this.TransportNationalityCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.FlightTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.AircraftIDTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.Trailer1NationalityCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.TransportNationalityCodeFindBox.SuspendLayout();
			this.Trailer1NationalityCodeFindBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.IT.Business.Declaration.JobDeclaration);
			// 
			// TransportNationalityCodeFindBox
			// 
			this.TransportNationalityCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TransportNationalityCodeFindBox, "JE_RN_NKTransportNationalityInland");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IT.Business.Declaration.JobDeclaration)(null)).JE_RN_NKTransportNationalityInland)));
			this.TransportNationalityCodeFindBox.CaptionResourceString = Enterprise.Customs.IT.GUI.Res.GetData("8b811f5b-49bf-486f-86f9-b51c536006e0", "[18] Nationality");
			this.TransportNationalityCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(287, 0, true);
			this.TransportNationalityCodeFindBox.Name = "TransportNationalityCodeFindBox";
			this.TransportNationalityCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.TransportNationalityCodeFindBox.ParentType = null;
			this.TransportNationalityCodeFindBox.PreBoundMaxLength = 2;
			this.TransportNationalityCodeFindBox.ShowDescriptionBox = false;
			this.TransportNationalityCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(47, 20, true);
			this.TransportNationalityCodeFindBox.TabIndex = 1;
			// 
			// FlightTextBox
			// 
			this.BindingSource.SetBindingMember(this.FlightTextBox, "JE_TransportIDInland");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IT.Business.Declaration.JobDeclaration)(null)).JE_TransportIDInland)));
			this.FlightTextBox.CaptionResourceString = Enterprise.Customs.IT.GUI.Res.GetData("ac851a24-be03-4344-84d2-aa4eacdddebc", "Flight", "Flight No.", "Flight Number", "");
			this.FlightTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.FlightTextBox.Name = "FlightTextBox";
			this.FlightTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(152, 20, true);
			this.FlightTextBox.TabIndex = 0;
			// 
			// AircraftIDTextBox
			// 
			this.BindingSource.SetBindingMember(this.AircraftIDTextBox, "JE_AircraftRegistrationInland");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IT.Business.Declaration.JobDeclaration)(null)).JE_AircraftRegistrationInland)));
			this.AircraftIDTextBox.CaptionResourceString = null;
			this.AircraftIDTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 22, true);
			this.AircraftIDTextBox.Name = "AircraftIDTextBox";
			this.AircraftIDTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(152, 20, true);
			this.AircraftIDTextBox.TabIndex = 2;
			// 
			// Trailer1NationalityCodeFindBox
			// 
			this.Trailer1NationalityCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.Trailer1NationalityCodeFindBox, "JE_RN_NKTrailer1Nationality");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IT.Business.Declaration.JobDeclaration)(null)).JE_RN_NKTrailer1Nationality)));
			this.Trailer1NationalityCodeFindBox.CaptionResourceString = Enterprise.Customs.IT.GUI.Res.GetData("44990aa8-50f3-4f15-ae1d-ad151e939e69", "[18] Nationality");
			this.Trailer1NationalityCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(287, 22, true);
			this.Trailer1NationalityCodeFindBox.Name = "Trailer1NationalityCodeFindBox";
			this.Trailer1NationalityCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.Trailer1NationalityCodeFindBox.ParentType = null;
			this.Trailer1NationalityCodeFindBox.PreBoundMaxLength = 2;
			this.Trailer1NationalityCodeFindBox.ShowDescriptionBox = false;
			this.Trailer1NationalityCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(47, 20, true);
			this.Trailer1NationalityCodeFindBox.TabIndex = 3;
			// 
			// TransportInlandAirUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.Trailer1NationalityCodeFindBox);
			this.Controls.Add(this.AircraftIDTextBox);
			this.Controls.Add(this.FlightTextBox);
			this.Controls.Add(this.TransportNationalityCodeFindBox);
			this.Name = "TransportInlandAirUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(338, 44, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.TransportNationalityCodeFindBox.ResumeLayout(true);
			this.TransportNationalityCodeFindBox.PerformLayout();
			this.Trailer1NationalityCodeFindBox.ResumeLayout(true);
			this.Trailer1NationalityCodeFindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.GUI.ZCodeFindBox TransportNationalityCodeFindBox;
		internal ZArchitecture.ZTextBox FlightTextBox;
		internal ZArchitecture.ZTextBox AircraftIDTextBox;
		internal ZArchitecture.GUI.ZCodeFindBox Trailer1NationalityCodeFindBox;
	}
}
