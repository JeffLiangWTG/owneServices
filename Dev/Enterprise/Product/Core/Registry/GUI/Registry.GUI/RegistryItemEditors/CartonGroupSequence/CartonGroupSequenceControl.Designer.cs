using Enterprise.ZArchitecture.Core;

namespace Enterprise.Registry.GUI
{
	partial class CartonGroupSequenceControl
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
			this.SortOrderGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ProductCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.CarrierCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.ConsigneeCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.ClientCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.WarehouseCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SortOrderGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Registry.Business.Warehouse.CartonGroupSequence);
			// 
			// SortOrderGroupBox
			// 
			this.SortOrderGroupBox.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("CartonGroupSequenceControl|21c8af2f-ecfa-47b0-b596-e67af4f612d8", "Fallback Sequence");
			this.SortOrderGroupBox.Controls.Add(this.ProductCalcEdit);
			this.SortOrderGroupBox.Controls.Add(this.CarrierCalcEdit);
			this.SortOrderGroupBox.Controls.Add(this.ConsigneeCalcEdit);
			this.SortOrderGroupBox.Controls.Add(this.ClientCalcEdit);
			this.SortOrderGroupBox.Controls.Add(this.WarehouseCalcEdit);
			this.SortOrderGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 3, true);
			this.SortOrderGroupBox.Name = "SortOrderGroupBox";
			this.SortOrderGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(138, 155, true);
			this.SortOrderGroupBox.TabIndex = 1;
			this.SortOrderGroupBox.TabStop = false;
			// 
			// ProductCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.ProductCalcEdit, "Product");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Registry.Business.Warehouse.CartonGroupSequence)(null)).Product)));
			this.ProductCalcEdit.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("CartonGroupSequenceControl|d816ee49-4b23-4b4a-8fa1-a2c46e82488b", "Product");
			this.ProductCalcEdit.DecimalPlaces = 0;
			this.ProductCalcEdit.Decimals = 0;
			this.ProductCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 19, true);
			this.ProductCalcEdit.Name = "ProductCalcEdit";
			this.ProductCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(26, 20, true);
			this.ProductCalcEdit.TabIndex = 1;
			this.ProductCalcEdit.Text = "0";
			this.ProductCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// CarrierCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.CarrierCalcEdit, "Carrier");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Registry.Business.Warehouse.CartonGroupSequence)(null)).Carrier)));
			this.CarrierCalcEdit.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("CartonGroupSequenceControl|b465b2e3-afc9-4585-bd7c-34f88b9d06c8", "Carrier");
			this.CarrierCalcEdit.DecimalPlaces = 0;
			this.CarrierCalcEdit.Decimals = 0;
			this.CarrierCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 45, true);
			this.CarrierCalcEdit.Name = "CarrierCalcEdit";
			this.CarrierCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(26, 20, true);
			this.CarrierCalcEdit.TabIndex = 3;
			this.CarrierCalcEdit.Text = "0";
			this.CarrierCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// ConsigneeCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.ConsigneeCalcEdit, "Consignee");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Registry.Business.Warehouse.CartonGroupSequence)(null)).Consignee)));
			this.ConsigneeCalcEdit.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("CartonGroupSequenceControl|0b64f2f9-a88a-460a-9df0-7f1c33f5385d", "Consignee");
			this.ConsigneeCalcEdit.DecimalPlaces = 0;
			this.ConsigneeCalcEdit.Decimals = 0;
			this.ConsigneeCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 71, true);
			this.ConsigneeCalcEdit.Name = "ConsigneeCalcEdit";
			this.ConsigneeCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(26, 20, true);
			this.ConsigneeCalcEdit.TabIndex = 5;
			this.ConsigneeCalcEdit.Text = "0";
			this.ConsigneeCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// ClientCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.ClientCalcEdit, "Client");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Registry.Business.Warehouse.CartonGroupSequence)(null)).Client)));
			this.ClientCalcEdit.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("CartonGroupSequenceControl|6771bb24-d28d-4c42-ab25-be531ff133b1", "Client");
			this.ClientCalcEdit.DecimalPlaces = 0;
			this.ClientCalcEdit.Decimals = 0;
			this.ClientCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 97, true);
			this.ClientCalcEdit.Name = "ClientCalcEdit";
			this.ClientCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(26, 20, true);
			this.ClientCalcEdit.TabIndex = 7;
			this.ClientCalcEdit.Text = "0";
			this.ClientCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// WarehouseCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.WarehouseCalcEdit, "Warehouse");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Registry.Business.Warehouse.CartonGroupSequence)(null)).Warehouse)));
			this.WarehouseCalcEdit.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("CartonGroupSequenceControl|d4850917-0394-4bf7-bc0a-8ad9f6a3a872", "Warehouse");
			this.WarehouseCalcEdit.DecimalPlaces = 0;
			this.WarehouseCalcEdit.Decimals = 0;
			this.WarehouseCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 123, true);
			this.WarehouseCalcEdit.Name = "WarehouseCalcEdit";
			this.WarehouseCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(26, 20, true);
			this.WarehouseCalcEdit.TabIndex = 9;
			this.WarehouseCalcEdit.Text = "0";
			this.WarehouseCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// CartonGroupSequenceControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.SortOrderGroupBox);
			this.Name = "CartonGroupSequenceControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(165, 175, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.SortOrderGroupBox.ResumeLayout(false);
			this.SortOrderGroupBox.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZGroupBox SortOrderGroupBox;
		private Enterprise.ZArchitecture.ZCalcEdit ProductCalcEdit;
		private Enterprise.ZArchitecture.ZCalcEdit CarrierCalcEdit;
		private Enterprise.ZArchitecture.ZCalcEdit ConsigneeCalcEdit;
		private Enterprise.ZArchitecture.ZCalcEdit ClientCalcEdit;
		private Enterprise.ZArchitecture.ZCalcEdit WarehouseCalcEdit;
	}
}
