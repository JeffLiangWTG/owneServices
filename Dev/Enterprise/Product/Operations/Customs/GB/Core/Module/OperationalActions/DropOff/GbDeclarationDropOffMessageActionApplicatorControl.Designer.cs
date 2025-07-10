using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.OperationalActions;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GB.Module.OperationalActions
{
	public partial class GbDeclarationDropOffMessageActionApplicatorControl : ZUserControl
	{
		ZDateEdit etdBox;
		ZDateEdit etaBox;
		ZTextBox textBox1;

		void InitializeComponent()
		{
			this.textBox1 = new ZTextBox();
			this.etdBox = new ZDateEdit();
			this.etaBox = new ZDateEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(GbDeclarationDropOffMessageActionMethodApplicator);
			// 
			// textBox1
			// 
			this.BindingSource.SetBindingMember(this.textBox1, "Vehicle");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((GbDeclarationDropOffMessageActionMethodApplicator)(null)).Vehicle);
			this.textBox1.CaptionResourceString = Enterprise.Customs.GB.Module.Res.GetData("25541868-408d-40b4-9436-acd6ae756c9e", "Registration", "Vehicle Registration", "");
			this.textBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 2, true);
			this.textBox1.Name = "textBox1";
			this.textBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(114, 20, true);
			this.textBox1.TabIndex = 0;
			// 
			// etdBox
			// 
			this.etdBox.AllowDrop = true;
			this.etdBox.AutoCompleteMonthThreshold = 1;
			this.etdBox.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.etdBox, "ETD");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((GbDeclarationDropOffMessageActionMethodApplicator)(null)).ETD);
			this.etdBox.CaptionResourceString = Enterprise.Customs.GB.Module.Res.GetData("8cf76d2b-f629-4957-a106-9b6d0fa54a21", "ETD");
			this.etdBox.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.etdBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 28, true);
			this.etdBox.Name = "etdBox";
			this.etdBox.TabIndex = 1;
			// 
			// etaBox
			// 
			this.etaBox.AllowDrop = true;
			this.etaBox.AutoCompleteMonthThreshold = 1;
			this.etaBox.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.etaBox, "ETA");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((GbDeclarationDropOffMessageActionMethodApplicator)(null)).ETA);
			this.etaBox.CaptionResourceString = Enterprise.Customs.GB.Module.Res.GetData("c79824fc-6ad4-4d27-82e1-6c85d72351c6", "ETA");
			this.etaBox.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.etaBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 54, true);
			this.etaBox.Name = "etaBox";
			this.etaBox.TabIndex = 2;
			// 
			// GbDeclarationDropOffMessageActionApplicatorControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.etaBox);
			this.Controls.Add(this.etdBox);
			this.Controls.Add(this.textBox1);
			this.Name = "GbDeclarationDropOffMessageActionApplicatorControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(244, 84, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
		}
	}
}
