using Enterprise.ZArchitecture.Core;

namespace Enterprise.Registry.GUI
{
	partial class TransportCoDefaultingRulesControl
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
		void InitializeComponent()
		{
			this.LocationSortOrderGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.LevelCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.RowCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.ColumnCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.LocationSortOrderGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Registry.Business.Warehouse.TransportCoDefaultingRules);
			// 
			// LocationSortOrderGroupBox
			// 
			this.LocationSortOrderGroupBox.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("TransportCoDefaultingRulesControl|4c3f635f-798d-43d9-abd9-25625c670f8a", "Algorithm Sequence");
			this.LocationSortOrderGroupBox.Controls.Add(this.LevelCalcEdit);
			this.LocationSortOrderGroupBox.Controls.Add(this.RowCalcEdit);
			this.LocationSortOrderGroupBox.Controls.Add(this.ColumnCalcEdit);
			this.LocationSortOrderGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 3, true);
			this.LocationSortOrderGroupBox.Name = "LocationSortOrderGroupBox";
			this.LocationSortOrderGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(138, 103, true);
			this.LocationSortOrderGroupBox.TabIndex = 1;
			this.LocationSortOrderGroupBox.TabStop = false;
			// 
			// LevelCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.LevelCalcEdit, "Warehouse");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Registry.Business.Warehouse.TransportCoDefaultingRules)(null)).Warehouse)));
			this.LevelCalcEdit.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("TransportCoDefaultingRulesControl|776d2694-ee45-4a45-b709-39222f3ae5a1", "Warehouse");
			this.LevelCalcEdit.DecimalPlaces = 0;
			this.LevelCalcEdit.Decimals = 0;
			this.LevelCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 71, true);
			this.LevelCalcEdit.Name = "LevelCalcEdit";
			this.LevelCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(26, 20, true);
			this.LevelCalcEdit.TabIndex = 5;
			this.LevelCalcEdit.Text = "0";
			this.LevelCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// RowCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.RowCalcEdit, "Consignee");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Registry.Business.Warehouse.TransportCoDefaultingRules)(null)).Consignee)));
			this.RowCalcEdit.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("TransportCoDefaultingRulesControl|f082d48c-e498-4b33-8472-85fd4767a2a0", "Consignee");
			this.RowCalcEdit.DecimalPlaces = 0;
			this.RowCalcEdit.Decimals = 0;
			this.RowCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 19, true);
			this.RowCalcEdit.Name = "RowCalcEdit";
			this.RowCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(26, 20, true);
			this.RowCalcEdit.TabIndex = 1;
			this.RowCalcEdit.Text = "0";
			this.RowCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// ColumnCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.ColumnCalcEdit, "Client");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Registry.Business.Warehouse.TransportCoDefaultingRules)(null)).Client)));
			this.ColumnCalcEdit.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("TransportCoDefaultingRulesControl|05263bf2-923d-4fdc-917b-2caee1cc3c82", "Client");
			this.ColumnCalcEdit.DecimalPlaces = 0;
			this.ColumnCalcEdit.Decimals = 0;
			this.ColumnCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 45, true);
			this.ColumnCalcEdit.Name = "ColumnCalcEdit";
			this.ColumnCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(26, 20, true);
			this.ColumnCalcEdit.TabIndex = 3;
			this.ColumnCalcEdit.Text = "0";
			this.ColumnCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// TransportCoDefaultingRulesControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.LocationSortOrderGroupBox);
			this.Name = "TransportCoDefaultingRulesControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(165, 123, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.LocationSortOrderGroupBox.ResumeLayout(false);
			this.LocationSortOrderGroupBox.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZGroupBox LocationSortOrderGroupBox;
		private Enterprise.ZArchitecture.ZCalcEdit LevelCalcEdit;
		private Enterprise.ZArchitecture.ZCalcEdit RowCalcEdit;
		private Enterprise.ZArchitecture.ZCalcEdit ColumnCalcEdit;
	}
}
