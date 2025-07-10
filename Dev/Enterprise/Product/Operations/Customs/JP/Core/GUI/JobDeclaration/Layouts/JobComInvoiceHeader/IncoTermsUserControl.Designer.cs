using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.JP.GUI
{
	partial class IncoTermsUserControl
	{
		private void InitializeComponent()
		{
			this.CommercialInvoiceDetailsIncoTermsUserControl = new Enterprise.Customs.GUI.CommercialInvoiceDetailsIncoTermsUserControl();
			this.IncoTermPlaceTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.CommercialInvoiceDetailsIncoTermsUserControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.JP.Business.JobComInvoiceHeader);
			// 
			// CommercialInvoiceDetailsIncoTermsUserControl
			// 
			this.CommercialInvoiceDetailsIncoTermsUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CommercialInvoiceDetailsIncoTermsUserControl, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.Business.BaseJobComInvoiceHeader)(((Enterprise.Customs.JP.Business.JobComInvoiceHeader)(null)))));
			this.CommercialInvoiceDetailsIncoTermsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(105, 0, true);
			this.CommercialInvoiceDetailsIncoTermsUserControl.Name = "CommercialInvoiceDetailsIncoTermsUserControl";
			this.CommercialInvoiceDetailsIncoTermsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(90, 20, true);
			this.CommercialInvoiceDetailsIncoTermsUserControl.TabIndex = 1;
			// 
			// IncoTermPlaceTextBox
			// 
			this.BindingSource.SetBindingMember(this.IncoTermPlaceTextBox, "JZ_IncoTermPlace");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.Business.JobComInvoiceHeader)(null)).JZ_IncoTermPlace)));
			this.IncoTermPlaceTextBox.CaptionResourceString = Enterprise.Customs.JP.GUI.Res.GetData("84704D28-2520-47E5-8919-5296C7E997FF", "Agreed Place");
			this.IncoTermPlaceTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.IncoTermPlaceTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(276, 0, true);
			this.IncoTermPlaceTextBox.Name = "IncoTermPlaceTextBox";
			this.IncoTermPlaceTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(60, 20, true);
			this.IncoTermPlaceTextBox.TabIndex = 2;
			// 
			// IncoTermsUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.CommercialInvoiceDetailsIncoTermsUserControl);
			this.Controls.Add(this.IncoTermPlaceTextBox);
			this.Name = "IncoTermsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(342, 22, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.CommercialInvoiceDetailsIncoTermsUserControl.ResumeLayout(true);
			this.CommercialInvoiceDetailsIncoTermsUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		internal CommercialInvoiceDetailsIncoTermsUserControl CommercialInvoiceDetailsIncoTermsUserControl;
		internal ZTextBox IncoTermPlaceTextBox;
	}
}
