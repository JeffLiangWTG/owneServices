namespace Enterprise.Customs.DE.NCTS.GUI
{
	partial class GoodsItemDetailsItemNoPlusMainPackUserControl
	{
		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.ItemNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.IsMainPackCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.DE.NCTS.Business.NctsDepartureCargoDesc);
			// 
			// ItemNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.ItemNumberTextBox, "BY_LineNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZShort)(((Enterprise.Customs.DE.NCTS.Business.NctsDepartureCargoDesc)(null)).BY_LineNo)));
			this.ItemNumberTextBox.CaptionResourceString = null;
			this.ItemNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ItemNumberTextBox.Name = "ItemNumberTextBox";
			this.ItemNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(79, 20, true);
			this.ItemNumberTextBox.TabIndex = 0;
			// 
			// IsMainPackCheckBox
			// 
			this.IsMainPackCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.IsMainPackCheckBox, "BY_IsMainPack");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.DE.NCTS.Business.NctsDepartureCargoDesc)(null)).BY_IsMainPack)));
			this.IsMainPackCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.IsMainPackCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(180, 2, true);
			this.IsMainPackCheckBox.Name = "IsMainPackCheckBox";
			this.IsMainPackCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.IsMainPackCheckBox.TabIndex = 8;
			this.IsMainPackCheckBox.UseVisualStyleBackColor = true;
			// 
			// GoodsItemDetailsItemNoPlusMainPackUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ItemNumberTextBox);
			this.Controls.Add(this.IsMainPackCheckBox);
			this.Name = "GoodsItemDetailsItemNoPlusMainPackUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 23, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		internal Enterprise.ZArchitecture.ZTextBox ItemNumberTextBox;
		internal Enterprise.ZArchitecture.GUI.ZCheckBox IsMainPackCheckBox;
	}
}
