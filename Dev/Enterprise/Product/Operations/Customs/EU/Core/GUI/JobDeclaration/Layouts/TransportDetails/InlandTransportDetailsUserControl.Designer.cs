namespace Enterprise.Customs.EU.GUI
{
	partial class InlandTransportDetailsUserControl
	{
		public class ControlNames
		{
			public const string TransportIDTextBox = "TransportIDTextBox";

			public const string TransportNationalityCodeFindBox = "TransportNationalityCodeFindBox";
		}

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
			this.TransportIDTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.TransportNationalityCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.TransportNationalityCodeFindBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.Business.Declaration.JobDeclaration);
			// 
			// TransportIDTextBox
			// 
			this.BindingSource.SetBindingMember(this.TransportIDTextBox, "ZG_Box18TransportID");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).ZG_Box18TransportID)));
			this.TransportIDTextBox.CaptionResourceString = null;
			this.TransportIDTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TransportIDTextBox.Name = "TransportIDTextBox";
			this.TransportIDTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.TransportIDTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(188, 20, true);
			this.TransportIDTextBox.TabIndex = 0;
			// 
			// TransportNationalityCodeFindBox
			// 
			this.TransportNationalityCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TransportNationalityCodeFindBox, "ZG_Box18TransportNationality");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).ZG_Box18TransportNationality)));
			this.TransportNationalityCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(287, 0, true);
			this.TransportNationalityCodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefCountry;
			this.TransportNationalityCodeFindBox.Name = "TransportNationalityCodeFindBox";
			this.TransportNationalityCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.TransportNationalityCodeFindBox.ParentType = null;
			this.TransportNationalityCodeFindBox.PreBoundMaxLength = 2;
			this.TransportNationalityCodeFindBox.ShowDescriptionBox = false;
			this.TransportNationalityCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(47, 20, true);
			this.TransportNationalityCodeFindBox.TabIndex = 1;
			// 
			// TransportDetailsBox18UserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.TransportIDTextBox);
			this.Controls.Add(this.TransportNationalityCodeFindBox);
			this.Name = "InlandTransportDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(338, 23, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.TransportNationalityCodeFindBox.ResumeLayout(true);
			this.TransportNationalityCodeFindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.ZTextBox TransportIDTextBox;
		internal ZArchitecture.GUI.ZCodeFindBox TransportNationalityCodeFindBox;
	}
}
