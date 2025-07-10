namespace Enterprise.Customs.ES.GUI
{
	partial class TransportInlandRailUserControl
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
			this.TrainNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.TrainNationalityCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.WagonNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.WagonNationalityCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.TrainNationalityCodeFindBox.SuspendLayout();
			this.WagonNationalityCodeFindBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.ES.Business.Declaration.JobDeclaration);
			// 
			// TrainNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.TrainNumberTextBox, "ZG_Box18TransportID");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ES.Business.Declaration.JobDeclaration)(null)).ZG_Box18TransportID)));
			this.TrainNumberTextBox.CaptionResourceString = Enterprise.Customs.ES.GUI.Res.GetData("{950F55DB-FD10-4EC9-8327-20A411F83EE8}", "Train No.");
			this.TrainNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TrainNumberTextBox.Name = "TrainNumberTextBox";
			this.TrainNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(78, 20, true);
			this.TrainNumberTextBox.TabIndex = 0;
			// 
			// TrainNationalityCodeFindBox
			// 
			this.TrainNationalityCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TrainNationalityCodeFindBox, "ZG_Box18TransportNationality");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ES.Business.Declaration.JobDeclaration)(null)).ZG_Box18TransportNationality)));
			this.TrainNationalityCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(82, 0, true);
			this.TrainNationalityCodeFindBox.Name = "TrainNationalityCodeFindBox";
			this.TrainNationalityCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.TrainNationalityCodeFindBox.ParentType = null;
			this.TrainNationalityCodeFindBox.PreBoundMaxLength = 2;
			this.TrainNationalityCodeFindBox.ShowDescriptionBox = false;
			this.TrainNationalityCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(47, 20, true);
			this.TrainNationalityCodeFindBox.TabIndex = 1;
			// 
			// WagonNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.WagonNumberTextBox, "JE_Trailer1RegNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ES.Business.Declaration.JobDeclaration)(null)).JE_Trailer1RegNo)));
			this.WagonNumberTextBox.CaptionResourceString = Enterprise.Customs.ES.GUI.Res.GetData("{BB91ED1F-8979-45B9-9A9F-3EB721369CDE}", "Wagon No.");
			this.WagonNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(205, 0, true);
			this.WagonNumberTextBox.Name = "WagonNumberTextBox";
			this.WagonNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(78, 20, true);
			this.WagonNumberTextBox.TabIndex = 2;
			// 
			// WagonNationalityCodeFindBox
			// 
			this.WagonNationalityCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.WagonNationalityCodeFindBox, "JE_RN_NKTrailer1Nationality");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ES.Business.Declaration.JobDeclaration)(null)).JE_RN_NKTrailer1Nationality)));
			this.WagonNationalityCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(287, 0, true);
			this.WagonNationalityCodeFindBox.Name = "WagonNationalityCodeFindBox";
			this.WagonNationalityCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.WagonNationalityCodeFindBox.ParentType = null;
			this.WagonNationalityCodeFindBox.PreBoundMaxLength = 2;
			this.WagonNationalityCodeFindBox.ShowDescriptionBox = false;
			this.WagonNationalityCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(47, 20, true);
			this.WagonNationalityCodeFindBox.TabIndex = 3;
			// 
			// TransportInlandRailUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.TrainNumberTextBox);
			this.Controls.Add(this.TrainNationalityCodeFindBox);
			this.Controls.Add(this.WagonNumberTextBox);
			this.Controls.Add(this.WagonNationalityCodeFindBox);
			this.Name = "TransportInlandRailUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(338, 23, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.TrainNationalityCodeFindBox.ResumeLayout(true);
			this.TrainNationalityCodeFindBox.PerformLayout();
			this.WagonNationalityCodeFindBox.ResumeLayout(true);
			this.WagonNationalityCodeFindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.ZTextBox TrainNumberTextBox;
		internal ZArchitecture.GUI.ZCodeFindBox TrainNationalityCodeFindBox;
		internal ZArchitecture.ZTextBox WagonNumberTextBox;
		internal ZArchitecture.GUI.ZCodeFindBox WagonNationalityCodeFindBox;
		internal ZArchitecture.ZTextBox Wagon2NumberTextBox;
		internal ZArchitecture.GUI.ZCodeFindBox Wagon2NationalityCodeFindBox;
	}
}
