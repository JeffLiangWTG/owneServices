using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.Intrastat.GUI
{
	partial class TransactionDetailsUserControl
	{
		private void InitializeComponent()
		{
			this.CountryOfSupplyDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.CountryOfReceiptDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.TransactionDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.TradersReferenceTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.SupplierVATTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ConsigneeVATTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.SupplierNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ConsigneeNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.NatureOfTransactionDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ModeOfTransportDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.IncoTermDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.CountryOfSupplyDropEdit.SuspendLayout();
			this.CountryOfReceiptDropEdit.SuspendLayout();
			this.TransactionDateEdit.SuspendLayout();
			this.NatureOfTransactionDropEdit.SuspendLayout();
			this.ModeOfTransportDropEdit.SuspendLayout();
			this.IncoTermDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.Intrastat.Business.CusIntrastatHeader);
			// 
			// CountryOfSupplyDropEdit
			// 
			this.CountryOfSupplyDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CountryOfSupplyDropEdit, "CIH_CountryOfSupply");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.Intrastat.Business.CusIntrastatHeader)(null)).CIH_CountryOfSupply)));
			this.CountryOfSupplyDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(175, 72, true);
			this.CountryOfSupplyDropEdit.Name = "CountryOfSupplyDropEdit";
			this.CountryOfSupplyDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(204, 20, true);
			this.CountryOfSupplyDropEdit.TabIndex = 1;
			// 
			// CountryOfReceiptDropEdit
			// 
			this.CountryOfReceiptDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CountryOfReceiptDropEdit, "CIH_CountryOfReceipt");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.Intrastat.Business.CusIntrastatHeader)(null)).CIH_CountryOfReceipt)));
			this.CountryOfReceiptDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(175, 98, true);
			this.CountryOfReceiptDropEdit.Name = "CountryOfReceiptDropEdit";
			this.CountryOfReceiptDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(204, 20, true);
			this.CountryOfReceiptDropEdit.TabIndex = 2;
			// 
			// TransactionDateEdit
			// 
			this.TransactionDateEdit.AllowDrop = true;
			this.TransactionDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.TransactionDateEdit, "CIH_TransactionDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.Intrastat.Business.CusIntrastatHeader)(null)).CIH_TransactionDate)));
			this.TransactionDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			this.TransactionDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(175, 124, true);
			this.TransactionDateEdit.Name = "TransactionDateEdit";
			this.TransactionDateEdit.TabIndex = 3;
			// 
			// TradersReferenceTextBox
			// 
			this.BindingSource.SetBindingMember(this.TradersReferenceTextBox, "CIH_TradersReference");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Intrastat.Business.CusIntrastatHeader)(null)).CIH_TradersReference)));
			this.TradersReferenceTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.TradersReferenceTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(175, 150, true);
			this.TradersReferenceTextBox.Name = "TradersReferenceTextBox";
			this.TradersReferenceTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(244, 20, true);
			this.TradersReferenceTextBox.TabIndex = 0;
			// 
			// SupplierVATTextBox
			// 
			this.BindingSource.SetBindingMember(this.SupplierVATTextBox, "CIH_SupplierVAT");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Intrastat.Business.CusIntrastatHeader)(null)).CIH_SupplierVAT)));
			this.SupplierVATTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.SupplierVATTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(175, 46, true);
			this.SupplierVATTextBox.Name = "SupplierVATTextBox";
			this.SupplierVATTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(244, 20, true);
			this.SupplierVATTextBox.TabIndex = 0;
			// 
			// ConsigneeVATTextBox
			// 
			this.BindingSource.SetBindingMember(this.ConsigneeVATTextBox, "CIH_ConsigneeVAT");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Intrastat.Business.CusIntrastatHeader)(null)).CIH_ConsigneeVAT)));
			this.ConsigneeVATTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ConsigneeVATTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(175, 20, true);
			this.ConsigneeVATTextBox.Name = "ConsigneeVATTextBox";
			this.ConsigneeVATTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(244, 20, true);
			this.ConsigneeVATTextBox.TabIndex = 4;
			// 
			// SupplierNameTextBox
			// 
			this.BindingSource.SetBindingMember(this.SupplierNameTextBox, "CIH_SupplierName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Intrastat.Business.CusIntrastatHeader)(null)).CIH_SupplierName)));
			this.SupplierNameTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.SupplierNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(175, 254, true);
			this.SupplierNameTextBox.Name = "SupplierNameTextBox";
			this.SupplierNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(244, 20, true);
			this.SupplierNameTextBox.TabIndex = 7;
			// 
			// ConsigneeNameTextBox
			// 
			this.BindingSource.SetBindingMember(this.ConsigneeNameTextBox, "CIH_ConsigneeName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Intrastat.Business.CusIntrastatHeader)(null)).CIH_ConsigneeName)));
			this.ConsigneeNameTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ConsigneeNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(175, 280, true);
			this.ConsigneeNameTextBox.Name = "ConsigneeNameTextBox";
			this.ConsigneeNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(244, 20, true);
			this.ConsigneeNameTextBox.TabIndex = 8;
			// 
			// NatureOfTransactionDropEdit
			// 
			this.NatureOfTransactionDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.NatureOfTransactionDropEdit, "CIH_NatureOfTransaction");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.Intrastat.Business.CusIntrastatHeader)(null)).CIH_NatureOfTransaction)));
			this.NatureOfTransactionDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(175, 176, true);
			this.NatureOfTransactionDropEdit.Name = "NatureOfTransactionDropEdit";
			this.NatureOfTransactionDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(204, 20, true);
			this.NatureOfTransactionDropEdit.TabIndex = 5;
			// 
			// ModeOfTransportDropEdit
			// 
			this.ModeOfTransportDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ModeOfTransportDropEdit, "CIH_ModeOfTransport");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.Intrastat.Business.CusIntrastatHeader)(null)).CIH_ModeOfTransport)));
			this.ModeOfTransportDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(175, 202, true);
			this.ModeOfTransportDropEdit.Name = "ModeOfTransportDropEdit";
			this.ModeOfTransportDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(204, 20, true);
			this.ModeOfTransportDropEdit.TabIndex = 6;
			// 
			// IncoTermDropEdit
			// 
			this.IncoTermDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.IncoTermDropEdit, "CIH_IncoTerm");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.Intrastat.Business.CusIntrastatHeader)(null)).CIH_IncoTerm)));
			this.IncoTermDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(175, 228, true);
			this.IncoTermDropEdit.Name = "IncoTermDropEdit";
			this.IncoTermDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(204, 20, true);
			this.IncoTermDropEdit.TabIndex = 7;
			// 
			// TransactionDetailsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.AutoScroll = true;
			this.AutoSize = true;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.IncoTermDropEdit);
			this.Controls.Add(this.ModeOfTransportDropEdit);
			this.Controls.Add(this.NatureOfTransactionDropEdit);
			this.Controls.Add(this.ConsigneeVATTextBox);
			this.Controls.Add(this.CountryOfReceiptDropEdit);
			this.Controls.Add(this.CountryOfSupplyDropEdit);
			this.Controls.Add(this.TransactionDateEdit);
			this.Controls.Add(this.TradersReferenceTextBox);
			this.Controls.Add(this.SupplierVATTextBox);
			this.Controls.Add(this.SupplierNameTextBox);
			this.Controls.Add(this.ConsigneeNameTextBox);
			this.Name = "TransactionDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(608, 459, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.CountryOfSupplyDropEdit.ResumeLayout(true);
			this.CountryOfSupplyDropEdit.PerformLayout();
			this.CountryOfReceiptDropEdit.ResumeLayout(true);
			this.CountryOfReceiptDropEdit.PerformLayout();
			this.TransactionDateEdit.ResumeLayout(true);
			this.TransactionDateEdit.PerformLayout();
			this.NatureOfTransactionDropEdit.ResumeLayout(true);
			this.NatureOfTransactionDropEdit.PerformLayout();
			this.ModeOfTransportDropEdit.ResumeLayout(true);
			this.ModeOfTransportDropEdit.PerformLayout();
			this.IncoTermDropEdit.ResumeLayout(true);
			this.IncoTermDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		internal ZArchitecture.GUI.ZDateEdit TransactionDateEdit;
		internal ZArchitecture.GUI.ZDropEdit CountryOfReceiptDropEdit;
		internal ZArchitecture.GUI.ZDropEdit CountryOfSupplyDropEdit;
		internal ZArchitecture.ZTextBox TradersReferenceTextBox;
		internal ZArchitecture.ZTextBox SupplierVATTextBox;
		internal ZArchitecture.ZTextBox ConsigneeVATTextBox;
		internal ZArchitecture.ZTextBox SupplierNameTextBox;
		internal ZArchitecture.ZTextBox ConsigneeNameTextBox;
		internal ZArchitecture.GUI.ZDropEdit NatureOfTransactionDropEdit;
		internal ZArchitecture.GUI.ZDropEdit ModeOfTransportDropEdit;
		internal ZArchitecture.GUI.ZDropEdit IncoTermDropEdit;
	}
}
