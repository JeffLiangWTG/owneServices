using CargoWiseOne.ResourceStrings;

namespace Enterprise.Customs.IT.TemporaryStorage.GUI
{
	partial class UCC6TemporaryStorageBillDetailControl
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
			this.LrnTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.GoodsDescTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.MrnTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.GrossWeightCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.NetWeightCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.SuppQtyCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.IT.TemporaryStorage.Business.TemporaryStorageHeader);
			// 
			// LrnTextBox
			// 
			this.BindingSource.SetBindingMember(this.LrnTextBox, "Bills.Lrn");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IT.TemporaryStorage.Business.TemporaryStorageBill)(((System.Collections.IList)(((Enterprise.Customs.IT.TemporaryStorage.Business.TemporaryStorageHeader)(null)).Bills)).SyncRoot)).Lrn)));
			this.LrnTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(74, 212, true);
			this.LrnTextBox.Name = "LrnTextBox";
			this.LrnTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(188, 20, true);
			this.LrnTextBox.TabIndex = 1;
			// 
			// MrnTextBox
			// 
			this.BindingSource.SetBindingMember(this.MrnTextBox, "Bills.Mrn");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IT.TemporaryStorage.Business.TemporaryStorageBill)(((System.Collections.IList)(((Enterprise.Customs.IT.TemporaryStorage.Business.TemporaryStorageHeader)(null)).Bills)).SyncRoot)).Mrn)));
			this.MrnTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(74, 242, true);
			this.MrnTextBox.Name = "MrnTextBox";
			this.MrnTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(188, 20, true);
			this.MrnTextBox.TabIndex = 2;
			// 
			// GrossWeightCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.GrossWeightCalcEdit, "Bills.GrossWeightInKG");
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDecimal)(((Enterprise.Customs.IT.TemporaryStorage.Business.TemporaryStorageBill)(((System.Collections.IList)(((Enterprise.Customs.IT.TemporaryStorage.Business.TemporaryStorageHeader)(null)).Bills)).SyncRoot)).GrossWeightInKG)));
			this.GrossWeightCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(74, 332, true);
			this.GrossWeightCalcEdit.Name = "GrossWeightCalcEdit";
			this.GrossWeightCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(188, 20, true);
			this.GrossWeightCalcEdit.TabIndex = 5;
			// 
			// NetWeightCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.NetWeightCalcEdit, "Bills.NetWeightInKG");
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDecimal)(((Enterprise.Customs.IT.TemporaryStorage.Business.TemporaryStorageBill)(((System.Collections.IList)(((Enterprise.Customs.IT.TemporaryStorage.Business.TemporaryStorageHeader)(null)).Bills)).SyncRoot)).NetWeightInKG)));
			this.NetWeightCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(74, 362, true);
			this.NetWeightCalcEdit.Name = "NetWeightCalcEdit";
			this.NetWeightCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(188, 20, true);
			this.NetWeightCalcEdit.TabIndex = 6;
			// 
			// SupplyQuantityCalcDropEdit
			// 
			this.BindingSource.SetBindingMember(this.SuppQtyCalcEdit, "Bills.SuppQuantity");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.IT.TemporaryStorage.Business.TemporaryStorageBill)(((System.Collections.IList)(((Enterprise.Customs.IT.TemporaryStorage.Business.TemporaryStorageHeader)(null)).Bills)).SyncRoot)).SuppQuantity)));
			this.SuppQtyCalcEdit.Decimals = 6;
			this.SuppQtyCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(74, 392, true);
			this.SuppQtyCalcEdit.Name = "SuppQtyCalcEdit";
			this.SuppQtyCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(188, 20, true);
			this.SuppQtyCalcEdit.TabIndex = 7;
			// 
			// GoodsDescTextBox
			//
			// this.GoodsDescTextBox = new ZArchitecture.ZTextBox();
			this.BindingSource.SetBindingMember(this.GoodsDescTextBox, "Bills.ABL_GoodsDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IT.TemporaryStorage.Business.TemporaryStorageBill)(((System.Collections.IList)(((Enterprise.Customs.IT.TemporaryStorage.Business.TemporaryStorageHeader)(null)).Bills)).SyncRoot)).ABL_GoodsDescription)));
			this.GoodsDescTextBox.CaptionResourceString = Res.GetData("A2D14BF6-C365-4D9D-83A2-FBC96E7DB8E3", "Goods Description");
			this.GoodsDescTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(74, 422, true);
			this.GoodsDescTextBox.Name = "GoodsDescTextBox";
			this.GoodsDescTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(188, 20, true);
			this.GoodsDescTextBox.TabIndex = 8;
			// 
			// UCC6TemporaryStorageBillDetailControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.LrnTextBox);
			this.Controls.Add(this.GoodsDescTextBox);
			this.Controls.Add(this.MrnTextBox);
			this.Controls.Add(this.GrossWeightCalcEdit);
			this.Controls.Add(this.NetWeightCalcEdit);
			this.Controls.Add(this.SuppQtyCalcEdit);
			this.Name = "UCC6TemporaryStorageBillDetailControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(534, 426, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.ZTextBox LrnTextBox;
		internal ZArchitecture.ZTextBox GoodsDescTextBox;
		internal ZArchitecture.ZTextBox MrnTextBox;
		internal ZArchitecture.ZCalcEdit GrossWeightCalcEdit;
		internal ZArchitecture.ZCalcEdit NetWeightCalcEdit;
		internal ZArchitecture.ZCalcEdit SuppQtyCalcEdit;
	}
}
