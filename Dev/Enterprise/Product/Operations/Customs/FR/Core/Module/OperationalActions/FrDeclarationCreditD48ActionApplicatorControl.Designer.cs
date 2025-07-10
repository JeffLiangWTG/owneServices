using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.FR.Module
{
	public partial class FrDeclarationCreditD48ActionApplicatorControl : ZUserControl
	{
		ZArchitecture.ZTextBox documentCodeTextBox;
		ZArchitecture.ZTextBox referenceTextBox;

		void InitializeComponent()
		{
			this.documentCodeTextBox = new ZArchitecture.ZTextBox();
			this.referenceTextBox = new ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Business.OperationalActions.FrDeclarationCreditD48Applicator);
			// 
			// documentCodeTextBox
			this.BindingSource.SetBindingMember(this.documentCodeTextBox, "D48DocumentCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Business.OperationalActions.FrDeclarationCreditD48Applicator)(null)).D48DocumentCode);
			this.documentCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 14, true);
			this.documentCodeTextBox.Name = "documentCodeTextBox";
			//this.documentCodeTextBox.CaptionResourceString = Enterprise.Customs.FR.Module.Res.GetData("6B678BEB-C1A2-44F2-90E4-745D7591E958", "Document Code");
			this.documentCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(145, 17, true);
			this.documentCodeTextBox.TabIndex = 0;
			// 
			// referenceTextBox
			//
			this.BindingSource.SetBindingMember(this.referenceTextBox, "ReferenceNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Business.OperationalActions.FrDeclarationCreditD48Applicator)(null)).ReferenceNumber);
			this.referenceTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 49, true);
			this.referenceTextBox.Name = "referenceTextBox";
			//this.referenceTextBox.CaptionResourceString = Enterprise.Customs.FR.Module.Res.GetData("41D549E7-B60F-422D-8E1D-5BE4C3DC2049", "Reference Number");
			this.referenceTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(145, 17, true);
			this.referenceTextBox.TabIndex = 1;
			// 
			// FrDeclarationCreditD48ActionApplicatorControl
			// 
			this.Controls.Add(this.referenceTextBox);
			this.Controls.Add(this.documentCodeTextBox);
			this.Name = "FrDeclarationCreditD48ActionApplicatorControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(527, 103, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
