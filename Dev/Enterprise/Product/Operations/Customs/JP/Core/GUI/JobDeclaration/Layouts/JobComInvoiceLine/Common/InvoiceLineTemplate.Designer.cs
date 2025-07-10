namespace Enterprise.Customs.JP.GUI;
public partial class InvoiceLineTemplate
{
	void InitializeComponent()
	{
		this.CustomsSecondQuantityCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
		this.NACCSCodeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
		this.EntryInstructionGuidDropEdit = new Enterprise.ZArchitecture.GUI.ZGuidDropEdit();
		this.UnitPriceCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
		this.CustomsQuantityCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
		this.VolumeCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
		this.WeightCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
		this.InvoiceQuantityCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
		this.LinePriceCurrencyCalcFindBox = new Enterprise.ZArchitecture.GUI.ZCalcFindBox();
		this.CustomsSecondQuantityCalcDropEdit.SuspendLayout();
		this.NACCSCodeDropEdit.SuspendLayout();
		this.EntryInstructionGuidDropEdit.SuspendLayout();
		this.UnitPriceCalcEdit.SuspendLayout();
		this.CustomsQuantityCalcDropEdit.SuspendLayout();
		this.VolumeCalcDropEdit.SuspendLayout();
		this.WeightCalcDropEdit.SuspendLayout();
		this.InvoiceQuantityCalcDropEdit.SuspendLayout();
		this.LinePriceCurrencyCalcFindBox.SuspendLayout();
		this.SuspendLayout();
		this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		// 
		// BindingSource
		// 
		this.BindingSource.DataSourceType = typeof(Enterprise.Customs.JP.Business.JobDeclaration);
		this.ResumeLayout(false);
		this.PerformLayout();
		// 
		// NACCSCodeDropEdit
		// 
		this.NACCSCodeDropEdit.AllowDrop = true;
		this.BindingSource.SetBindingMember(this.NACCSCodeDropEdit, "JI_NACCSCode");
		// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.JP.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.JP.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_NACCSCode)));
		this.NACCSCodeDropEdit.CaptionResourceString = Enterprise.Customs.JP.GUI.Res.GetData("JPExportInvoiceLineUserControl|JI_NACCSCode", "NACCS Code");
		this.NACCSCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(523, 40, true);
		this.NACCSCodeDropEdit.Name = "NACCSCodeDropEdit";
		this.NACCSCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(162, 15, true);
		// 
		// CustomsSecondQuantityCalcDropEdit
		// 
		this.CustomsSecondQuantityCalcDropEdit.AllowDrop = true;
		this.BindingSource.SetBindingMember(this.CustomsSecondQuantityCalcDropEdit, ".");
		// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.JP.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.JP.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_CustomsSecondQuantity)));
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.JP.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_CustomsSecondUnitQty)));
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.JP.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.JP.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).Lookups.CustomsUQList)));
		this.CustomsSecondQuantityCalcDropEdit.BindToAmount = "JI_CustomsSecondQuantity";
		this.CustomsSecondQuantityCalcDropEdit.BindToList = "Lookups+CustomsUQList";
		this.CustomsSecondQuantityCalcDropEdit.BindToUnit = "JI_CustomsSecondUnitQty";
		this.CustomsSecondQuantityCalcDropEdit.CaptionResourceString = Enterprise.Customs.JP.GUI.Res.GetData("JPExportInvoiceLineUserControl|JI_CustomsSecondQuantity", "Custom Qty 2");
		this.CustomsSecondQuantityCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(430, 62, true);
		this.CustomsSecondQuantityCalcDropEdit.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 15, true);
		this.CustomsSecondQuantityCalcDropEdit.Name = "CustomsSecondQuantityCalcDropEdit";
		this.CustomsSecondQuantityCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 15, true);
		// 
		// EntryInstructionGuidDropEdit
		// 
		this.EntryInstructionGuidDropEdit.AllowDrop = true;
		this.BindingSource.SetBindingMember(this.EntryInstructionGuidDropEdit, "JI_CEI");
		// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.Business.BaseJobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.Business.IInvoicesProvider)(null)).FilteredInvoiceLines)).SyncRoot)).JI_CEI)));
		this.EntryInstructionGuidDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 15, true);
		this.EntryInstructionGuidDropEdit.Name = "EntryInstructionGuidDropEdit";
		this.EntryInstructionGuidDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 15, true);
		// 
		// UnitPriceCalcEdit
		// 
		this.BindingSource.SetBindingMember(this.UnitPriceCalcEdit, "UnitPrice");
		this.UnitPriceCalcEdit.Name = "UnitPriceCalcEdit";
		this.UnitPriceCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(140, 15, true);
		// 
		// CustomsQuantityCalcDropEdit
		// 
		this.CustomsQuantityCalcDropEdit.AllowDrop = true;
		this.BindingSource.SetBindingMember(this.CustomsQuantityCalcDropEdit, ".");
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.JP.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.JP.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_CustomsQuantity)));
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.Business.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.JP.Business.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).JI_CustomsUnitQty)));
		this.CustomsQuantityCalcDropEdit.BindToAmount = "JI_CustomsQuantity";
		this.CustomsQuantityCalcDropEdit.BindToUnit = "JI_CustomsUnitQty";
		this.CustomsQuantityCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 40, true);
		this.CustomsQuantityCalcDropEdit.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 15, true);
		this.CustomsQuantityCalcDropEdit.Name = "CustomsQuantityCalcDropEdit";
		this.CustomsQuantityCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 15, true);
		// 
		// VolumeCalcDropEdit
		// 
		this.VolumeCalcDropEdit.AllowDrop = true;
		this.BindingSource.SetBindingMember(this.VolumeCalcDropEdit, ".");
		// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.Business.BaseJobComInvoiceLine)(null)).JI_Volume)));
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseJobComInvoiceLine)(null)).JI_VolumeUQ)));
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobComInvoiceLine)(null)).Lookups.VolumeUQList)));
		this.VolumeCalcDropEdit.BindToAmount = "JI_Volume";
		this.VolumeCalcDropEdit.BindToList = "Lookups+VolumeUQList";
		this.VolumeCalcDropEdit.BindToUnit = "JI_VolumeUQ";
		this.VolumeCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(633, 255, true);
		this.VolumeCalcDropEdit.Name = "VolumeCalcDropEdit";
		this.VolumeCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(140, 17, true);
		this.VolumeCalcDropEdit.TabIndex = 15;
		this.VolumeCalcDropEdit.UnitPreBoundMaxLength = 2;
		// 
		// WeightCalcDropEdit
		// 
		this.WeightCalcDropEdit.AllowDrop = true;
		this.BindingSource.SetBindingMember(this.WeightCalcDropEdit, ".");
		// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.Business.BaseJobComInvoiceLine)(null)).JI_Weight)));
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseJobComInvoiceLine)(null)).JI_WeightUQ)));
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.Business.BaseJobComInvoiceLine)(null)).Lookups.WeightUQList)));
		this.WeightCalcDropEdit.BindToAmount = "JI_Weight";
		this.WeightCalcDropEdit.BindToList = "Lookups+WeightUQList";
		this.WeightCalcDropEdit.BindToUnit = "JI_WeightUQ";
		this.WeightCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(633, 228, true);
		this.WeightCalcDropEdit.Name = "WeightCalcDropEdit";
		this.WeightCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(140, 17, true);
		this.WeightCalcDropEdit.UnitPreBoundMaxLength = 2;
		// 
		// InvoiceQuantityCalcDropEdit
		// 
		this.InvoiceQuantityCalcDropEdit.AllowDrop = true;
		this.BindingSource.SetBindingMember(this.InvoiceQuantityCalcDropEdit, ".");
		// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.Business.BaseJobComInvoiceLine)(null)).JI_InvoiceQuantity)));
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseJobComInvoiceLine)(null)).JI_InvoiceUQ)));
		this.InvoiceQuantityCalcDropEdit.BindToAmount = "JI_InvoiceQuantity";
		this.InvoiceQuantityCalcDropEdit.BindToUnit = "JI_InvoiceUQ";
		this.InvoiceQuantityCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(633, 39, true);
		this.InvoiceQuantityCalcDropEdit.Name = "InvoiceQuantityCalcDropEdit";
		this.InvoiceQuantityCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(140, 17, true);
		this.InvoiceQuantityCalcDropEdit.UnitPreBoundMaxLength = 3;
		// 
		// LinePriceCurrencyCalcFindBox
		// 
		this.LinePriceCurrencyCalcFindBox.AllowDrop = true;
		this.LinePriceCurrencyCalcFindBox.BindToAmount = "JI_LinePrice";
		this.LinePriceCurrencyCalcFindBox.BindToList = "Lookups+CurrencyList";
		this.LinePriceCurrencyCalcFindBox.BindToUnit = "JI_RX_NKLinePriceCurr";
		this.LinePriceCurrencyCalcFindBox.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
		this.LinePriceCurrencyCalcFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(633, 174, true);
		this.LinePriceCurrencyCalcFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefCurrency;
		this.LinePriceCurrencyCalcFindBox.Name = "LinePriceCurrencyCalcFindBox";
		this.LinePriceCurrencyCalcFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(140, 20, true);
		// 
		// InvoiceLineTemplate
		//
		this.Name = "InvoiceLineTemplate";
		this.Controls.Add(this.CustomsQuantityCalcDropEdit);
		this.Controls.Add(this.CustomsSecondQuantityCalcDropEdit);
		this.Controls.Add(this.NACCSCodeDropEdit);
		this.Controls.Add(this.EntryInstructionGuidDropEdit);
		this.Controls.Add(this.UnitPriceCalcEdit);
		this.Controls.Add(this.VolumeCalcDropEdit);
		this.Controls.Add(this.WeightCalcDropEdit);
		this.Controls.Add(this.InvoiceQuantityCalcDropEdit);
		this.Controls.Add(this.LinePriceCurrencyCalcFindBox);
		((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
		this.NACCSCodeDropEdit.ResumeLayout(true);
		this.NACCSCodeDropEdit.PerformLayout();
		this.CustomsSecondQuantityCalcDropEdit.ResumeLayout(true);
		this.CustomsSecondQuantityCalcDropEdit.PerformLayout();
		this.EntryInstructionGuidDropEdit.ResumeLayout(true);
		this.EntryInstructionGuidDropEdit.PerformLayout();
		this.UnitPriceCalcEdit.ResumeLayout(true);
		this.UnitPriceCalcEdit.PerformLayout();
		this.CustomsQuantityCalcDropEdit.ResumeLayout(true);
		this.CustomsQuantityCalcDropEdit.PerformLayout();
		this.VolumeCalcDropEdit.ResumeLayout(true);
		this.VolumeCalcDropEdit.PerformLayout();
		this.WeightCalcDropEdit.ResumeLayout(true);
		this.WeightCalcDropEdit.PerformLayout();
		this.InvoiceQuantityCalcDropEdit.ResumeLayout(true);
		this.InvoiceQuantityCalcDropEdit.PerformLayout();
		this.LinePriceCurrencyCalcFindBox.ResumeLayout(true);
		this.LinePriceCurrencyCalcFindBox.PerformLayout();
		this.ResumeLayout(false);
		this.PerformLayout();
	}

	internal Enterprise.ZArchitecture.GUI.ZCalcDropEdit CustomsQuantityCalcDropEdit;
	internal Enterprise.ZArchitecture.GUI.ZCalcDropEdit CustomsSecondQuantityCalcDropEdit;
	internal Enterprise.ZArchitecture.GUI.ZDropEdit NACCSCodeDropEdit;
	internal ZArchitecture.GUI.ZGuidDropEdit EntryInstructionGuidDropEdit;
	internal ZArchitecture.ZCalcEdit UnitPriceCalcEdit;
	internal Enterprise.ZArchitecture.GUI.ZCalcDropEdit VolumeCalcDropEdit;
	internal Enterprise.ZArchitecture.GUI.ZCalcDropEdit WeightCalcDropEdit;
	internal Enterprise.ZArchitecture.GUI.ZCalcDropEdit InvoiceQuantityCalcDropEdit;
	internal Enterprise.ZArchitecture.GUI.ZCalcFindBox LinePriceCurrencyCalcFindBox;
}
