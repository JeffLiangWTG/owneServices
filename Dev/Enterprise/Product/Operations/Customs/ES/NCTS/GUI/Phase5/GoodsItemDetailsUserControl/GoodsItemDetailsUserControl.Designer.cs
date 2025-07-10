namespace Enterprise.Customs.ES.NCTS.GUI
{
	partial class GoodsItemDetailsUserControl
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
			this.IsVehiclesCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ExciseCodeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.PVPValueCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ExciseCodeDropEdit.SuspendLayout();
			this.PVPValueCalcDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.ES.NCTS.Business.NctsDepartureCargoDesc);
			// 
			// IsVehiclesCheckBox
			// 
			this.IsVehiclesCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.IsVehiclesCheckBox, "IsVehicles");
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.ES.NCTS.Business.NctsDepartureCargoDesc)(null)).IsVehicles)));
			this.IsVehiclesCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(118, 62, true);
			this.IsVehiclesCheckBox.Name = "IsVehiclesCheckBox";
			this.IsVehiclesCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 14, true);
			this.IsVehiclesCheckBox.TabIndex = 3;
			this.IsVehiclesCheckBox.CaptionResourceString = Enterprise.Customs.ES.NCTS.GUI.Res.GetData("E25E31FD-80B5-4000-AB74-6DE148F08206", "Vehicles");
			this.IsVehiclesCheckBox.UseVisualStyleBackColor = true;
			// 
			// ExciseCodeDropEdit
			// 
			this.ExciseCodeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ExciseCodeDropEdit, "ExciseCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ES.NCTS.Business.NctsDepartureCargoDesc)(null)).ExciseCode)));
			this.ExciseCodeDropEdit.CaptionResourceString = Enterprise.Customs.ES.NCTS.GUI.Res.GetData("714E5364-86A1-4AD6-A98B-90C83734E794", "Excise Code");
			this.ExciseCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(568, 164, true);
			this.ExciseCodeDropEdit.Name = "ExciseCodeDropEdit";
			this.ExciseCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.ExciseCodeDropEdit.TabIndex = 2;
			// 
			// PVPValueCalcDropEdit
			// 
			this.PVPValueCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PVPValueCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDecimal)(((Enterprise.Customs.ES.NCTS.Business.NctsDepartureCargoDesc)(null)).PVPValue)));
			this.PVPValueCalcDropEdit.CaptionResourceString = Enterprise.Customs.ES.NCTS.GUI.Res.GetData("0A2D0968-9A82-43CC-912F-02DDB8BBD413", "PVP");
			this.PVPValueCalcDropEdit.BindToAmount = "PVPValue";
			this.PVPValueCalcDropEdit.BindToUnit = "PVPCurrency";
			this.PVPValueCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(568, 139, true);
			this.PVPValueCalcDropEdit.Name = "PVPValueCalcDropEdit";
			this.PVPValueCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(180, 20, true);
			this.PVPValueCalcDropEdit.TabIndex = 3;
			this.PVPValueCalcDropEdit.UnitPreBoundMaxLength = 2;
			// 
			// GoodsItemDetailsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.IsVehiclesCheckBox);
			this.Controls.Add(this.ExciseCodeDropEdit);
			this.Controls.Add(this.PVPValueCalcDropEdit);
			this.Name = "GoodsItemDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(282, 211, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ExciseCodeDropEdit.ResumeLayout(true);
			this.ExciseCodeDropEdit.PerformLayout();
			this.PVPValueCalcDropEdit.ResumeLayout(true);
			this.PVPValueCalcDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.GUI.ZCheckBox IsVehiclesCheckBox;
		internal ZArchitecture.GUI.ZDropEdit ExciseCodeDropEdit;
		internal ZArchitecture.GUI.ZCalcDropEdit PVPValueCalcDropEdit;
	}
}
