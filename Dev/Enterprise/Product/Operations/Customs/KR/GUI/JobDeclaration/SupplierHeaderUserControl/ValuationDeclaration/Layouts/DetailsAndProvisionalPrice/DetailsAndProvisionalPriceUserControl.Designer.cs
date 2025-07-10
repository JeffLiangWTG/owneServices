using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public partial class DetailsAndProvisionalPriceUserControl
	{

		private void InitializeComponent()
		{
            this.InvoiceNoTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.InvoiceDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
            this.PurchaseOrderNoTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.PurchaseOrderDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
            this.ContractNoTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.ContractDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
            this.TotalCustomsValueCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
            this.ProvisionalPricingDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.ProvisionalAdditionRateCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
            this.ProvisionalAdditionAmountCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
            this.EstimatedDateOfFinalPriceDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
            this.ContractExpirationDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.InvoiceDateEdit.SuspendLayout();
            this.PurchaseOrderDateEdit.SuspendLayout();
            this.ContractDateEdit.SuspendLayout();
            this.ProvisionalPricingDropEdit.SuspendLayout();
            this.EstimatedDateOfFinalPriceDateEdit.SuspendLayout();
            this.ContractExpirationDateEdit.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.KR.Business.JobComInvoiceHeader);
            // 
            // InvoiceNoTextBox
            // 
            this.BindingSource.SetBindingMember(this.InvoiceNoTextBox, "JZ_InvoiceNumber");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.JobComInvoiceHeader)(null)).JZ_InvoiceNumber)));
            this.InvoiceNoTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(160, 39, true);
            this.InvoiceNoTextBox.Name = "InvoiceNoTextBox";
			this.InvoiceNoTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(205, 15, true);
            this.InvoiceNoTextBox.TabIndex = 0;
            // 
            // InvoiceDateEdit
            // 
            this.InvoiceDateEdit.AllowDrop = true;
            this.InvoiceDateEdit.AutoCompleteMonthThreshold = 1;
            this.BindingSource.SetBindingMember(this.InvoiceDateEdit, "JZ_InvoiceDate");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.KR.Business.JobComInvoiceHeader)(null)).JZ_InvoiceDate)));
            this.InvoiceDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(605, 39, true);
            this.InvoiceDateEdit.Name = "InvoiceDateEdit";
            this.InvoiceDateEdit.TabIndex = 1;
            // 
            // PurchaseOrderNoTextBox
            // 
            this.BindingSource.SetBindingMember(this.PurchaseOrderNoTextBox, "PurchaseOrderNumber");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.JobComInvoiceHeader)(null)).PurchaseOrderNumber)));
            this.PurchaseOrderNoTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.PurchaseOrderNoTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(160, 65, true);
            this.PurchaseOrderNoTextBox.Name = "PurchaseOrderNoTextBox";
			this.PurchaseOrderNoTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(205, 15, true);
            this.PurchaseOrderNoTextBox.TabIndex = 2;
            // 
            // PurchaseOrderDateEdit
            // 
            this.PurchaseOrderDateEdit.AllowDrop = true;
            this.PurchaseOrderDateEdit.AutoCompleteMonthThreshold = 1;
            this.BindingSource.SetBindingMember(this.PurchaseOrderDateEdit, "PurchaseOrderDate");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.KR.Business.JobComInvoiceHeader)(null)).PurchaseOrderDate)));
            this.PurchaseOrderDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(605, 65, true);
            this.PurchaseOrderDateEdit.Name = "PurchaseOrderDateEdit";
            this.PurchaseOrderDateEdit.TabIndex = 3;
            // 
            // ContractNoTextBox
            // 
            this.BindingSource.SetBindingMember(this.ContractNoTextBox, "ContractNumber");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.JobComInvoiceHeader)(null)).ContractNumber)));
            this.ContractNoTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.ContractNoTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(160, 91, true);
            this.ContractNoTextBox.Name = "ContractNoTextBox";
			this.ContractNoTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(205, 15, true);
            this.ContractNoTextBox.TabIndex = 4;
            // 
            // ContractDateEdit
            // 
            this.ContractDateEdit.AllowDrop = true;
            this.ContractDateEdit.AutoCompleteMonthThreshold = 1;
            this.BindingSource.SetBindingMember(this.ContractDateEdit, "ContractDate");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.KR.Business.JobComInvoiceHeader)(null)).ContractDate)));
            this.ContractDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(605, 91, true);
            this.ContractDateEdit.Name = "ContractDateEdit";
            this.ContractDateEdit.TabIndex = 5;
            // 
            // TotalCustomsValueCalcEdit
            // 
            this.BindingSource.SetBindingMember(this.TotalCustomsValueCalcEdit, "CustomsValueKRW");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.JobComInvoiceHeader)(null)).CustomsValueKRW)));
            this.TotalCustomsValueCalcEdit.DecimalPlaces = 2;
            this.TotalCustomsValueCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(160, 117, true);
            this.TotalCustomsValueCalcEdit.Name = "TotalCustomsValueCalcEdit";
			this.TotalCustomsValueCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(205, 15, true);
            this.TotalCustomsValueCalcEdit.TabIndex = 6;
            this.TotalCustomsValueCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.TotalCustomsValueCalcEdit.TrackDisposedAccess = true;
            // 
            // ProvisionalPricingDropEdit
            // 
            this.ProvisionalPricingDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.ProvisionalPricingDropEdit, "JZ_ProvPricingYN");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.KR.Business.JobComInvoiceHeader)(null)).JZ_ProvPricingYN)));
            this.ProvisionalPricingDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(160, 220, true);
            this.ProvisionalPricingDropEdit.Name = "ProvisionalPricingDropEdit";
			this.ProvisionalPricingDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(205, 15, true);
            this.ProvisionalPricingDropEdit.TabIndex = 7;
            // 
            // ProvisionalAdditionRateCalcEdit
            // 
            this.BindingSource.SetBindingMember(this.ProvisionalAdditionRateCalcEdit, "JZ_ProvAdditionalRate");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.JobComInvoiceHeader)(null)).JZ_ProvAdditionalRate)));
            this.ProvisionalAdditionRateCalcEdit.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("9ceff359-994f-4c5d-be74-e80f076a1001", "Provisional Addition Rate / Amount", "Provisional Addition Rate");
            this.ProvisionalAdditionRateCalcEdit.DecimalPlaces = 2;
            this.ProvisionalAdditionRateCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(604, 220, true);
            this.ProvisionalAdditionRateCalcEdit.Name = "ProvisionalAdditionRateCalcEdit";
			this.ProvisionalAdditionRateCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(85, 15, true);
            this.ProvisionalAdditionRateCalcEdit.TabIndex = 8;
            this.ProvisionalAdditionRateCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.ProvisionalAdditionRateCalcEdit.TrackDisposedAccess = true;
            // 
            // ProvisionalAdditionAmountCalcEdit
            // 
            this.BindingSource.SetBindingMember(this.ProvisionalAdditionAmountCalcEdit, "JZ_ProvAdditionalAmount");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.KR.Business.JobComInvoiceHeader)(null)).JZ_ProvAdditionalAmount)));
            this.ProvisionalAdditionAmountCalcEdit.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("12359273-2ee5-4056-8750-7a57766bb605", "/", "Provisional Additional Amount");
            this.ProvisionalAdditionAmountCalcEdit.DecimalPlaces = 0;
            this.ProvisionalAdditionAmountCalcEdit.Decimals = 0;
            this.ProvisionalAdditionAmountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(705, 220, true);
            this.ProvisionalAdditionAmountCalcEdit.Name = "ProvisionalAdditionAmountCalcEdit";
			this.ProvisionalAdditionAmountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(138, 15, true);
            this.ProvisionalAdditionAmountCalcEdit.TabIndex = 9;
            this.ProvisionalAdditionAmountCalcEdit.Text = "0";
            this.ProvisionalAdditionAmountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.ProvisionalAdditionAmountCalcEdit.TrackDisposedAccess = true;
            // 
            // EstimatedDateOfFinalPriceDateEdit
            // 
            this.EstimatedDateOfFinalPriceDateEdit.AllowDrop = true;
            this.EstimatedDateOfFinalPriceDateEdit.AutoCompleteMonthThreshold = 1;
            this.BindingSource.SetBindingMember(this.EstimatedDateOfFinalPriceDateEdit, "JZ_EstimatedDateOfFinalPrice");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.KR.Business.JobComInvoiceHeader)(null)).JZ_EstimatedDateOfFinalPrice)));
            this.EstimatedDateOfFinalPriceDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(160, 246, true);
            this.EstimatedDateOfFinalPriceDateEdit.Name = "EstimatedDateOfFinalPriceDateEdit";
            this.EstimatedDateOfFinalPriceDateEdit.TabIndex = 10;
            // 
            // ContractExpirationDateEdit
            // 
            this.ContractExpirationDateEdit.AllowDrop = true;
            this.ContractExpirationDateEdit.AutoCompleteMonthThreshold = 1;
            this.BindingSource.SetBindingMember(this.ContractExpirationDateEdit, "JZ_ImpContractExpiryDate");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.KR.Business.JobComInvoiceHeader)(null)).JZ_ImpContractExpiryDate)));
            this.ContractExpirationDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(604, 246, true);
            this.ContractExpirationDateEdit.Name = "ContractExpirationDateEdit";
            this.ContractExpirationDateEdit.TabIndex = 11;
            // 
            // DetailsAndProvisionalPriceUserControl
            // 
            this.CaptionRenderingEnabled = true;
            this.Controls.Add(this.ContractExpirationDateEdit);
            this.Controls.Add(this.EstimatedDateOfFinalPriceDateEdit);
            this.Controls.Add(this.ProvisionalAdditionAmountCalcEdit);
            this.Controls.Add(this.ProvisionalAdditionRateCalcEdit);
            this.Controls.Add(this.ProvisionalPricingDropEdit);
            this.Controls.Add(this.TotalCustomsValueCalcEdit);
            this.Controls.Add(this.ContractDateEdit);
            this.Controls.Add(this.ContractNoTextBox);
            this.Controls.Add(this.PurchaseOrderDateEdit);
            this.Controls.Add(this.PurchaseOrderNoTextBox);
            this.Controls.Add(this.InvoiceDateEdit);
            this.Controls.Add(this.InvoiceNoTextBox);
            this.Name = "DetailsAndProvisionalPriceUserControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(888, 293, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.InvoiceDateEdit.ResumeLayout(true);
            this.InvoiceDateEdit.PerformLayout();
            this.PurchaseOrderDateEdit.ResumeLayout(true);
            this.PurchaseOrderDateEdit.PerformLayout();
            this.ContractDateEdit.ResumeLayout(true);
            this.ContractDateEdit.PerformLayout();
            this.ProvisionalPricingDropEdit.ResumeLayout(true);
            this.ProvisionalPricingDropEdit.PerformLayout();
            this.EstimatedDateOfFinalPriceDateEdit.ResumeLayout(true);
            this.EstimatedDateOfFinalPriceDateEdit.PerformLayout();
            this.ContractExpirationDateEdit.ResumeLayout(true);
            this.ContractExpirationDateEdit.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}
		internal ZDateEdit InvoiceDateEdit;
		internal ZArchitecture.ZTextBox PurchaseOrderNoTextBox;
		internal ZDateEdit PurchaseOrderDateEdit;
		internal ZArchitecture.ZTextBox ContractNoTextBox;
		internal ZDateEdit ContractDateEdit;
		internal ZArchitecture.ZCalcEdit TotalCustomsValueCalcEdit;
		internal ZDropEdit ProvisionalPricingDropEdit;
		internal ZArchitecture.ZCalcEdit ProvisionalAdditionRateCalcEdit;
		internal ZArchitecture.ZCalcEdit ProvisionalAdditionAmountCalcEdit;
		internal ZDateEdit EstimatedDateOfFinalPriceDateEdit;
		internal ZDateEdit ContractExpirationDateEdit;
		internal ZArchitecture.ZTextBox InvoiceNoTextBox;
	}
}
