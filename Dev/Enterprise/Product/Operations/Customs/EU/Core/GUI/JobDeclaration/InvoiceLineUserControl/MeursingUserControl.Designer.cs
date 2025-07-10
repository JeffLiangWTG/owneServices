namespace Enterprise.Customs.EU.GUI
{
	partial class MeursingUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		protected void InitializeComponent()
		{
			this.CalculateButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.StarchGlucoseUpDown = new Enterprise.ZArchitecture.GUI.ZNumericUpDown();
			this.SucroseUpDown = new Enterprise.ZArchitecture.GUI.ZNumericUpDown();
			this.MilkFatUpDown = new Enterprise.ZArchitecture.GUI.ZNumericUpDown();
			this.MilkProteinsUpDown = new Enterprise.ZArchitecture.GUI.ZNumericUpDown();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.StarchGlucoseUpDown)).BeginInit();
			this.StarchGlucoseUpDown.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.SucroseUpDown)).BeginInit();
			this.SucroseUpDown.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MilkFatUpDown)).BeginInit();
			this.MilkFatUpDown.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MilkProteinsUpDown)).BeginInit();
			this.MilkProteinsUpDown.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.Business.Meursing.MeursingTable);
			// 
			// CalculateButton
			// 
			this.CalculateButton.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("D72589AE-DB9B-47F1-AF29-1BF8E07C49C0", "Calculate");
			this.CalculateButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.CalculateButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(89, 116, true);
			this.CalculateButton.Name = "CalculateButton";
			this.CalculateButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.CalculateButton.TabIndex = 5;
			this.CalculateButton.ToolTipCaption = null;
			this.CalculateButton.UseVisualStyleBackColor = true;
			this.CalculateButton.Click += new System.EventHandler(this.CalculateButton_Click);
			// 
			// StarchGlucoseUpDown
			// 
			this.StarchGlucoseUpDown.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.StarchGlucoseUpDown, "StarchGlucose");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDecimal)(((Enterprise.Customs.EU.Business.Meursing.MeursingTable)(null)).StarchGlucose)));
			this.StarchGlucoseUpDown.BindTo = "StarchGlucose";
			this.StarchGlucoseUpDown.DecimalPlaces = 2;
			this.StarchGlucoseUpDown.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(156, 12, true);
			this.StarchGlucoseUpDown.Name = "StarchGlucoseUpDown";
			this.StarchGlucoseUpDown.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
			this.StarchGlucoseUpDown.TabIndex = 1;
			// 
			// SucroseUpDown
			// 
			this.BindingSource.SetBindingMember(this.SucroseUpDown, "Sucrose");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDecimal)(((Enterprise.Customs.EU.Business.Meursing.MeursingTable)(null)).Sucrose)));
			this.SucroseUpDown.BindTo = "Sucrose";
			this.SucroseUpDown.DecimalPlaces = 2;
			this.SucroseUpDown.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(156, 38, true);
			this.SucroseUpDown.Name = "SucroseUpDown";
			this.SucroseUpDown.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
			this.SucroseUpDown.TabIndex = 2;
			// 
			// MilkFatUpDown
			// 
			this.BindingSource.SetBindingMember(this.MilkFatUpDown, "MilkFat");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDecimal)(((Enterprise.Customs.EU.Business.Meursing.MeursingTable)(null)).MilkFat)));
			this.MilkFatUpDown.BindTo = "MilkFat";
			this.MilkFatUpDown.DecimalPlaces = 2;
			this.MilkFatUpDown.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(156, 64, true);
			this.MilkFatUpDown.Name = "MilkFatUpDown";
			this.MilkFatUpDown.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
			this.MilkFatUpDown.TabIndex = 3;
			// 
			// MilkProteinsUpDown
			// 
			this.BindingSource.SetBindingMember(this.MilkProteinsUpDown, "MilkProtein");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDecimal)(((Enterprise.Customs.EU.Business.Meursing.MeursingTable)(null)).MilkProtein)));
			this.MilkProteinsUpDown.BindTo = "MilkProtein";
			this.MilkProteinsUpDown.DecimalPlaces = 2;
			this.MilkProteinsUpDown.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(156, 90, true);
			this.MilkProteinsUpDown.Name = "MilkProteinsUpDown";
			this.MilkProteinsUpDown.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
			this.MilkProteinsUpDown.TabIndex = 4;
			// 
			// MeursingUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("F0E6B7FA-591D-4A5C-BE19-90DD8F8D6192", "Meursing calculator");
			this.Controls.Add(this.MilkProteinsUpDown);
			this.Controls.Add(this.MilkFatUpDown);
			this.Controls.Add(this.SucroseUpDown);
			this.Controls.Add(this.StarchGlucoseUpDown);
			this.Controls.Add(this.CalculateButton);
			this.Name = "MeursingUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(230, 150, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.StarchGlucoseUpDown)).EndInit();
			this.StarchGlucoseUpDown.ResumeLayout(false);
			this.StarchGlucoseUpDown.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.SucroseUpDown)).EndInit();
			this.SucroseUpDown.ResumeLayout(false);
			this.SucroseUpDown.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MilkFatUpDown)).EndInit();
			this.MilkFatUpDown.ResumeLayout(false);
			this.MilkFatUpDown.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MilkProteinsUpDown)).EndInit();
			this.MilkProteinsUpDown.ResumeLayout(false);
			this.MilkProteinsUpDown.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZButton CalculateButton;
		protected Enterprise.ZArchitecture.GUI.ZNumericUpDown StarchGlucoseUpDown;
		protected Enterprise.ZArchitecture.GUI.ZNumericUpDown SucroseUpDown;
		protected Enterprise.ZArchitecture.GUI.ZNumericUpDown MilkFatUpDown;
		protected Enterprise.ZArchitecture.GUI.ZNumericUpDown MilkProteinsUpDown;
	}
}
