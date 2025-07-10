namespace Enterprise.Customs.ES.TemporaryStorage.GUI
{
	partial class TempStorageRegisterPremisesUserControl
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
            this.LocationDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.PremisesCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.PremisesDescriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.LocationDropEdit.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.TemporaryStorage.Business.CusTempStorageRegPremises);
            // 
            // LocationDropEdit
            // 
            this.LocationDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.LocationDropEdit, "SRP_CustomsLocation");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.TemporaryStorage.Business.CusTempStorageRegPremises)(null)).SRP_CustomsLocation)));
            this.LocationDropEdit.CaptionResourceString = Enterprise.Customs.ES.TemporaryStorage.GUI.Res.GetData("2BCE649E-8C53-4B0E-8CC7-8C56AC1323F4", "Customs Location");
            this.LocationDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(586, 19, true);
            this.LocationDropEdit.Name = "LocationDropEdit";
            this.LocationDropEdit.ShouldResizeByMaxLength = false;
            this.LocationDropEdit.ShowDescriptionBox = false;
            this.LocationDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 15, true);
            this.LocationDropEdit.TabIndex = 1;
            this.LocationDropEdit.UseFullWidthForCodeBox = true;
            // 
            // PremisesCodeTextBox
            // 
            this.BindingSource.SetBindingMember(this.PremisesCodeTextBox, "SRP_Code");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.TemporaryStorage.Business.CusTempStorageRegPremises)(null)).SRP_Code)));
            this.PremisesCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 19, true);
            this.PremisesCodeTextBox.Name = "PremisesCodeTextBox";
            this.PremisesCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 15, true);
            this.PremisesCodeTextBox.TabIndex = 2;
            // 
            // PremisesDescriptionTextBox
            // 
            this.BindingSource.SetBindingMember(this.PremisesDescriptionTextBox, "SRP_Description");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.TemporaryStorage.Business.CusTempStorageRegPremises)(null)).SRP_Description)));
            this.PremisesDescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(302, 19, true);
            this.PremisesDescriptionTextBox.Name = "PremisesDescriptionTextBox";
            this.PremisesDescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 15, true);
            this.PremisesDescriptionTextBox.TabIndex = 3;
            // 
            // TempStorageRegisterPremisesUserControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.AutoSize = true;
            this.Controls.Add(this.PremisesCodeTextBox);
            this.Controls.Add(this.PremisesDescriptionTextBox);
            this.Controls.Add(this.LocationDropEdit);
            this.Name = "TempStorageRegisterPremisesUserControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(839, 186, true);
            this.Tag = "";
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.LocationDropEdit.ResumeLayout(true);
            this.LocationDropEdit.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		internal ZArchitecture.GUI.ZDropEdit LocationDropEdit;
		internal ZArchitecture.ZTextBox PremisesCodeTextBox;
		internal ZArchitecture.ZTextBox PremisesDescriptionTextBox;

		#endregion

	}
}
