using Enterprise.Customs.CA.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CA.GUI
{
	partial class ConsumerProductUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.UnitVolumeDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.QuantityDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.GrossWeightDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.NetWeightCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.SuspendLayout();
			this.UnitVolumeDropEdit.SuspendLayout();
			this.QuantityDropEdit.SuspendLayout();
			this.GrossWeightDropEdit.SuspendLayout();
			this.NetWeightCalcDropEdit.SuspendLayout();
			this.GTINNumberTextBox.TabIndex = 5;
			this.BrandNameTextBox.TabIndex = 6;
			this.BatchLotNumberTextBox.TabIndex = 7;
			this.TradeNameTextBox.TabIndex = 8;
			this.DetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1070, 132, true);
			this.DetailsGroupBox.Controls.Add(this.NetWeightCalcDropEdit);
			this.DetailsGroupBox.Controls.Add(this.UnitVolumeDropEdit);
			this.DetailsGroupBox.Controls.Add(this.QuantityDropEdit);
			this.DetailsGroupBox.Controls.Add(this.GrossWeightDropEdit);
			// 
			// IntendedUseCodeDropEdit
			// 
			this.BindingSource.SetBindingMember(this.IntendedUseCodeDropEdit, "CA_IntendedUseCodeCPR");
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CA.Business.HCPGAHeader)(null)).CA_IntendedUseCodeCPR)));
			// 
			// CategoryDropEdit
			// 
			this.BindingSource.SetBindingMember(this.CategoryDropEdit, "CA_CategoryCPR");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CA.Business.HCPGAHeader)(null)).CA_CategoryCPR)));
			//
			// NetWeightCalcDropEdit
			// 
			this.BindingSource.SetBindingMember(this.NetWeightCalcDropEdit, ".");
			this.NetWeightCalcDropEdit.BindToAmount = "InvoiceLine.JI_NetWeight";
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDecimal)(((Enterprise.Customs.CA.Business.HCPGAHeader)(null)).InvoiceLine.JI_NetWeight)));
			this.NetWeightCalcDropEdit.BindToUnit = "InvoiceLine.JI_NetWeightUQ";
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.HCPGAHeader)(null)).InvoiceLine.JI_NetWeightUQ)));
			this.NetWeightCalcDropEdit.AllowDrop = true;
			this.NetWeightCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(876, 66, true);
			this.NetWeightCalcDropEdit.Name = "NetWeightCalcDropEdit";
			this.NetWeightCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 20, true);
			this.NetWeightCalcDropEdit.TabIndex = 11;
			// 
			// UnitVolumeDropEdit
			// 
			this.BindingSource.SetBindingMember(this.UnitVolumeDropEdit, ".");
			this.UnitVolumeDropEdit.BindToAmount = "InvoiceLine.JI_Volume";
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDecimal)(((Enterprise.Customs.CA.Business.HCPGAHeader)(null)).InvoiceLine.JI_Volume)));
			this.UnitVolumeDropEdit.BindToUnit = "InvoiceLine.JI_VolumeUQ";
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.HCPGAHeader)(null)).InvoiceLine.JI_VolumeUQ)));
			this.UnitVolumeDropEdit.AllowDrop = true;
			this.UnitVolumeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(876, 92, true);
			this.UnitVolumeDropEdit.Name = "UnitVolumeDropEdit";
			this.UnitVolumeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 20, true);
			this.UnitVolumeDropEdit.TabIndex = 12;
			// 
			// QuantityDropEdit
			// 
			this.BindingSource.SetBindingMember(this.QuantityDropEdit, ".");
			this.QuantityDropEdit.BindToAmount = "InvoiceLine.JI_InvoiceQuantity";
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDecimal)(((Enterprise.Customs.CA.Business.HCPGAHeader)(null)).InvoiceLine.JI_InvoiceQuantity)));
			this.QuantityDropEdit.BindToUnit = "InvoiceLine.JI_InvoiceUQ";
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.HCPGAHeader)(null)).InvoiceLine.JI_InvoiceUQ)));
			this.QuantityDropEdit.AllowDrop = true;
			this.QuantityDropEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("E3814BC6-C67F-40C7-8FF0-F7BF1091AC12", "Quantity");
			this.QuantityDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(876, 14, true);
			this.QuantityDropEdit.Name = "QuantityDropEdit";
			this.QuantityDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 20, true);
			this.QuantityDropEdit.TabIndex = 9;
			// 
			// GrossWeightDropEdit
			// 
			this.BindingSource.SetBindingMember(this.GrossWeightDropEdit, ".");
			this.GrossWeightDropEdit.BindToAmount = "InvoiceLine.JI_Weight";
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDecimal)(((Enterprise.Customs.CA.Business.HCPGAHeader)(null)).InvoiceLine.JI_Weight)));
			this.GrossWeightDropEdit.BindToUnit = "InvoiceLine.JI_WeightUQ";
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.HCPGAHeader)(null)).InvoiceLine.JI_WeightUQ)));
			this.GrossWeightDropEdit.AllowDrop = true;
			this.GrossWeightDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(876, 40, true);
			this.GrossWeightDropEdit.Name = "GrossWeightDropEdit";
			this.GrossWeightDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 20, true);
			this.GrossWeightDropEdit.TabIndex = 10;
			// 
			// ConsumerProductUserControl
			// 
			this.Name = "ConsumerProductUserControl";
			this.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.UnitVolumeDropEdit.ResumeLayout(true);
			this.UnitVolumeDropEdit.PerformLayout();
			this.QuantityDropEdit.ResumeLayout(true);
			this.QuantityDropEdit.PerformLayout();
			this.GrossWeightDropEdit.ResumeLayout(true);
			this.GrossWeightDropEdit.PerformLayout();
			this.NetWeightCalcDropEdit.ResumeLayout(true);
			this.NetWeightCalcDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion
		internal ZCalcDropEdit NetWeightCalcDropEdit;
		internal ZCalcDropEdit UnitVolumeDropEdit;
		internal ZCalcDropEdit QuantityDropEdit;
		internal ZCalcDropEdit GrossWeightDropEdit;
	}
}
