using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IL.GUI
{
	partial class IncoTermsWithCountryCodeUserControl
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
			this.IncoTermsUserControl = new Enterprise.Customs.GUI.CommercialInvoiceDetailsIncoTermsUserControl();
			this.IncoTermsCountryCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.IncoTermsUserControl.SuspendLayout();
			this.IncoTermsCountryCodeFindBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.IL.Business.JobComInvoiceHeader);
			// 
			// IncoTermsUserControl
			// 
			this.IncoTermsUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.IncoTermsUserControl, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.Business.BaseJobComInvoiceHeader)(((Enterprise.Customs.IL.Business.JobComInvoiceHeader)(null)))));
			this.IncoTermsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.IncoTermsUserControl.Name = "IncoTermsUserControl";
			this.IncoTermsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(85, 22, true);
			this.IncoTermsUserControl.TabIndex = 0;
			// 
			// IncoTermsCountryCodeFindBox
			// 
			this.IncoTermsCountryCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.IncoTermsCountryCodeFindBox, "JZ_IncoTermPlace");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IL.Business.JobComInvoiceHeader)(null)).JZ_IncoTermPlace)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.IL.Business.JobComInvoiceHeader)(null)).Lookups.CountryList)));
			this.IncoTermsCountryCodeFindBox.BindToList = "Lookups+CountryList";
			this.IncoTermsCountryCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(145, 0, true);
			this.IncoTermsCountryCodeFindBox.Name = "IncoTermsCountryCodeFindBox";
			this.IncoTermsCountryCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefCountry;
			this.IncoTermsCountryCodeFindBox.ParentType = null;
			this.IncoTermsCountryCodeFindBox.ShouldResize = false;
			this.IncoTermsCountryCodeFindBox.ShowDescriptionBox = false;
			this.IncoTermsCountryCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(95, 22, true);
			this.IncoTermsCountryCodeFindBox.TabIndex = 1;
			// 
			// IncoTermsWithCountryCodeUserControl
			//
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.IncoTermsUserControl);
			this.Controls.Add(this.IncoTermsCountryCodeFindBox);
			this.Name = "IncoTermsWithCountryCodeUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 20, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.IncoTermsUserControl.ResumeLayout(true);
			this.IncoTermsUserControl.PerformLayout();
			this.IncoTermsCountryCodeFindBox.ResumeLayout(true);
			this.IncoTermsCountryCodeFindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		internal CommercialInvoiceDetailsIncoTermsUserControl IncoTermsUserControl;
		internal ZCodeFindBox IncoTermsCountryCodeFindBox;
		#endregion
	}
}
