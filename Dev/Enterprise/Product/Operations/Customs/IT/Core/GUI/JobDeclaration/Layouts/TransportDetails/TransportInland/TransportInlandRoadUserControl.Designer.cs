namespace Enterprise.Customs.IT.GUI
{
	partial class TransportInlandRoadUserControl
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
			this.Trailer1NationalityCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.TransportIDTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.TransportNationalityCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.Trailer1IDTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.Trailer2IDTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.Trailer2NationalityCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.Trailer1NationalityCodeFindBox.SuspendLayout();
			this.TransportNationalityCodeFindBox.SuspendLayout();
			this.Trailer2NationalityCodeFindBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.IT.Business.Declaration.JobDeclaration);
			// 
			// Trailer1NationalityCodeFindBox
			// 
			this.Trailer1NationalityCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.Trailer1NationalityCodeFindBox, "JE_RN_NKTrailer1Nationality");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IT.Business.Declaration.JobDeclaration)(null)).JE_RN_NKTrailer1Nationality)));
			this.Trailer1NationalityCodeFindBox.CaptionResourceString = Enterprise.Customs.IT.GUI.Res.GetData("9290c45f-4fd0-4ecf-ba24-05277c9ed436", "[18] Nationality");
			this.Trailer1NationalityCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(287, 22, true);
			this.Trailer1NationalityCodeFindBox.Name = "Trailer1NationalityCodeFindBox";
			this.Trailer1NationalityCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.Trailer1NationalityCodeFindBox.ParentType = null;
			this.Trailer1NationalityCodeFindBox.PreBoundMaxLength = 2;
			this.Trailer1NationalityCodeFindBox.ShowDescriptionBox = false;
			this.Trailer1NationalityCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(47, 20, true);
			this.Trailer1NationalityCodeFindBox.TabIndex = 3;
			// 
			// TransportIDTextBox
			// 
			this.BindingSource.SetBindingMember(this.TransportIDTextBox, "JE_TransportIDInland");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IT.Business.Declaration.JobDeclaration)(null)).JE_TransportIDInland)));
			this.TransportIDTextBox.CaptionResourceString = Enterprise.Customs.IT.GUI.Res.GetData("3b85ab87-2d67-495e-b349-3dea94122f2a", "Trans. ID (Inland)");
			this.TransportIDTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TransportIDTextBox.Name = "TransportIDTextBox";
			this.TransportIDTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(152, 20, true);
			this.TransportIDTextBox.TabIndex = 0;
			// 
			// TransportNationalityCodeFindBox
			// 
			this.TransportNationalityCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TransportNationalityCodeFindBox, "JE_RN_NKTransportNationalityInland");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IT.Business.Declaration.JobDeclaration)(null)).JE_RN_NKTransportNationalityInland)));
			this.TransportNationalityCodeFindBox.CaptionResourceString = Enterprise.Customs.IT.GUI.Res.GetData("ee4c23d0-69ae-4f90-87dd-c9445d167018", "[18] Nationality");
			this.TransportNationalityCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(287, 0, true);
			this.TransportNationalityCodeFindBox.Name = "TransportNationalityCodeFindBox";
			this.TransportNationalityCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.TransportNationalityCodeFindBox.ParentType = null;
			this.TransportNationalityCodeFindBox.PreBoundMaxLength = 2;
			this.TransportNationalityCodeFindBox.ShowDescriptionBox = false;
			this.TransportNationalityCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(47, 20, true);
			this.TransportNationalityCodeFindBox.TabIndex = 1;
			// 
			// Trailer1IDTextBox
			// 
			this.BindingSource.SetBindingMember(this.Trailer1IDTextBox, "JE_Trailer1RegNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IT.Business.Declaration.JobDeclaration)(null)).JE_Trailer1RegNo)));
			this.Trailer1IDTextBox.CaptionResourceString = Enterprise.Customs.IT.GUI.Res.GetData("9ace3bbc-e8c2-4b65-bfd0-7bc0eec93228", "Trailer 1", "Trailer 1 ID", "");
			this.Trailer1IDTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 22, true);
			this.Trailer1IDTextBox.Name = "Trailer1IDTextBox";
			this.Trailer1IDTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(152, 20, true);
			this.Trailer1IDTextBox.TabIndex = 2;
			// 
			// Trailer2IDTextBox
			// 
			this.BindingSource.SetBindingMember(this.Trailer2IDTextBox, "JE_Trailer2RegNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IT.Business.Declaration.JobDeclaration)(null)).JE_Trailer2RegNo)));
			this.Trailer2IDTextBox.CaptionResourceString = Enterprise.Customs.IT.GUI.Res.GetData("8c963272-6ee6-4e74-ae96-9de9aff08700", "Trailer 2", "Trailer 2 ID", "");
			this.Trailer2IDTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 44, true);
			this.Trailer2IDTextBox.Name = "Trailer2IDTextBox";
			this.Trailer2IDTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(152, 20, true);
			this.Trailer2IDTextBox.TabIndex = 4;
			// 
			// Trailer2NationalityCodeFindBox
			// 
			this.Trailer2NationalityCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.Trailer2NationalityCodeFindBox, "JE_RN_NKTrailer2Nationality");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IT.Business.Declaration.JobDeclaration)(null)).JE_RN_NKTrailer2Nationality)));
			this.Trailer2NationalityCodeFindBox.CaptionResourceString = Enterprise.Customs.IT.GUI.Res.GetData("b91f9ae6-550c-486b-b77b-82a4f8543582", "[18] Nationality");
			this.Trailer2NationalityCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(287, 44, true);
			this.Trailer2NationalityCodeFindBox.Name = "Trailer2NationalityCodeFindBox";
			this.Trailer2NationalityCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.Trailer2NationalityCodeFindBox.ParentType = null;
			this.Trailer2NationalityCodeFindBox.PreBoundMaxLength = 2;
			this.Trailer2NationalityCodeFindBox.ShowDescriptionBox = false;
			this.Trailer2NationalityCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(47, 20, true);
			this.Trailer2NationalityCodeFindBox.TabIndex = 5;
			// 
			// TransportInlandRoadUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.Trailer2IDTextBox);
			this.Controls.Add(this.Trailer2NationalityCodeFindBox);
			this.Controls.Add(this.Trailer1IDTextBox);
			this.Controls.Add(this.TransportNationalityCodeFindBox);
			this.Controls.Add(this.TransportIDTextBox);
			this.Controls.Add(this.Trailer1NationalityCodeFindBox);
			this.Name = "TransportInlandRoadUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(338, 64, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.Trailer1NationalityCodeFindBox.ResumeLayout(true);
			this.Trailer1NationalityCodeFindBox.PerformLayout();
			this.TransportNationalityCodeFindBox.ResumeLayout(true);
			this.TransportNationalityCodeFindBox.PerformLayout();
			this.Trailer2NationalityCodeFindBox.ResumeLayout(true);
			this.Trailer2NationalityCodeFindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal Enterprise.ZArchitecture.ZTextBox TransportIDTextBox;
		internal Enterprise.ZArchitecture.GUI.ZCodeFindBox Trailer1NationalityCodeFindBox;
		internal ZArchitecture.GUI.ZCodeFindBox TransportNationalityCodeFindBox;
		internal ZArchitecture.ZTextBox Trailer1IDTextBox;
		internal ZArchitecture.ZTextBox Trailer2IDTextBox;
		internal ZArchitecture.GUI.ZCodeFindBox Trailer2NationalityCodeFindBox;
	}
}
