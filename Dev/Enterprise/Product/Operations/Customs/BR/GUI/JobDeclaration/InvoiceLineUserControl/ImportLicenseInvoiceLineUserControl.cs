using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.BR.Business;
using Enterprise.Customs.Common.BR;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.BR.GUI
{
	public partial class ImportLicenseInvoiceLineUserControl : BaseInvoiceLineUserControl
	{
		public ImportLicenseInvoiceLineUserControl()
		{
			InitializeComponent();
			CustomsInvoiceLinesBoundGrid.ColumnLayoutContext = BRJobMessageTypeList.Codes.ImportLicense;
			CustomsInvoiceLinesBoundGrid.RemoveFromAvailableColumns(JobComInvoiceLine.Schema.JI_Volume, JobComInvoiceLine.Schema.JI_VolumeUQ, JobComInvoiceLine.Schema.JI_Calc_OrderLineNumberAndSubLine);
		}

		protected override IPanelLayoutProvider GetNewInvoiceLineDetailsPanelLayout() => new ImportLicenseInvoiceLineDetailsLayout();

		protected override ZBool DynamicLayoutApplied => true;

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			JI_Calc_DutyConvertToLocalCurrencyControl.Visible = false;
		}

		protected override void AddNewColumnForCustomsInvoiceLinesBoundGrid()
		{
			CreateNewTextBoxColumn(JobComInvoiceLine.Schema.JI_Calc_MergedLineNumber, 80);
			CreateNewDropEditColumn(JobComInvoiceLine.Schema.JI_ManufacturerIndicator, 120);
			CreateNewTextBoxColumn(JobComInvoiceLine.Schema.ManufacturerName, 120, defaultColumn: true);
			CreateManufacturerAddressColumns();
			CreateNewCodeFindBoxColumn(JobComInvoiceLine.Schema.NaladiHs, 90, Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.Universal.RefCusTariff);
			CreateNewTextBoxColumn(JobComInvoiceLine.Schema.DrawbackCANumber, 90, defaultColumn: false);
			CreateNewCalcEditColumn(JobComInvoiceLine.Schema.DrawbackItemNumber, 90, false);
			CreateNewDropEditColumn(JobComInvoiceLine.Schema.DutyTaxRegime, 120);
			CreateNewDropEditColumn(JobComInvoiceLine.Schema.DutyLegalBase, 120);
			CreateNewDropEditColumn(nameof(JobComInvoiceLine.DrawbackModality), 80);
			CreateNewDropEditColumn(JobComInvoiceLine.Schema.JI_UsedMaterialRegime, 80, defaultColumn: false);
			CreateNewDropEditColumn(JobComInvoiceLine.Schema.JI_UsedMaterialOperationType, 80, defaultColumn: false);
			CreateNewTextBoxColumn(JobComInvoiceLine.Schema.JI_BrandName, 80, defaultColumn: false);
			CreateNewTextBoxColumn(JobComInvoiceLine.Schema.JI_Model, 80, defaultColumn: false);
			CreateNewTextBoxColumn(JobComInvoiceLine.Schema.JI_UsedMaterialManufactureYear, 50, defaultColumn: false);
			CreateNewDropEditColumn(JobComInvoiceLine.Schema.JI_SecondaryPreference, 80, defaultColumn: false);
		}

		protected override void CreateManufacturerAddressColumns()
		{
			var manufacturerOrgPKOrganisationFindBox = new ZOrganisationFindBoxColumnStyleInfo();
			manufacturerOrgPKOrganisationFindBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			manufacturerOrgPKOrganisationFindBox.ColumnName = JobComInvoiceLine.Schema.ManufacturerDocOrgPK;
			manufacturerOrgPKOrganisationFindBox.GroupName = ManufacturerCaption;
			manufacturerOrgPKOrganisationFindBox.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);

			var manufacturerAddressGuidDropEdit = new ZGuidDropEditColumnStyleInfo();

			manufacturerAddressGuidDropEdit.BindToList = "ManufacturerDocAddress+Organisation+Addresses";
			manufacturerAddressGuidDropEdit.GroupName = ManufacturerCaption;
			manufacturerAddressGuidDropEdit.ColumnName = JobComInvoiceLine.Schema.ManufacturerDocAddressPK;
			manufacturerAddressGuidDropEdit.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(130);

			CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(manufacturerOrgPKOrganisationFindBox);
			CustomsInvoiceLinesBoundGrid.ColumnStyles.Add(manufacturerAddressGuidDropEdit);
		}

		protected override IEnumerable<string> GetDefaultColumnsInOrderCore() => new List<string>
		{
			JobComInvoiceLine.Schema.JI_LineNo,
			JobComInvoiceLine.Schema.JI_Calc_Invoice,
			JobComInvoiceLine.Schema.JI_PartNo,
			JobComInvoiceLine.Schema.JI_CC,
			JobComInvoiceLine.Schema.JI_Tariff,
			JobComInvoiceLine.Schema.JI_InvoiceQuantity,
			JobComInvoiceLine.Schema.JI_InvoiceUQ,
			JobComInvoiceLine.Schema.JI_CustomsQuantity,
			JobComInvoiceLine.Schema.JI_CustomsUnitQty,
			JobComInvoiceLine.Schema.JI_LinePrice,
			JobComInvoiceLine.Schema.FullGoodsDescription,
			JobComInvoiceLine.Schema.JI_RH_NKCommodity_Code,
			JobComInvoiceLine.Schema.JI_Weight,
			JobComInvoiceLine.Schema.JI_WeightUQ,
			JobComInvoiceLine.Schema.JI_NetWeight,
			JobComInvoiceLine.Schema.JI_NetWeightUQ,
			JobComInvoiceLine.Schema.JI_Volume,
			JobComInvoiceLine.Schema.JI_VolumeUQ,
			JobComInvoiceLine.Schema.JI_OrderNumber,
			JobComInvoiceLine.Schema.JI_Calc_OrderLineNumberAndSubLine,
			JobComInvoiceLine.Schema.UnitPrice,
			JobComInvoiceLine.Schema.JI_CustomAttrib1,
			JobComInvoiceLine.Schema.JI_CustomAttrib2,
			JobComInvoiceLine.Schema.JI_CustomAttrib3,
			JobComInvoiceLine.Schema.JI_CustomAttrib4,
			JobComInvoiceLine.Schema.JI_CustomAttrib5,
			JobComInvoiceLine.Schema.JI_CustomAttrib6,
			JobComInvoiceLine.Schema.JI_CustomTextBlob1,
			JobComInvoiceLine.Schema.JI_PartAttrib1,
			JobComInvoiceLine.Schema.JI_PartAttrib2,
			JobComInvoiceLine.Schema.JI_PartAttrib3,
			JobComInvoiceLine.Schema.JI_SerialNumber,
			JobComInvoiceLine.Schema.JI_CEI,
			JobComInvoiceLine.Schema.JI_Calc_MergedLineNumber,
			JobComInvoiceLine.Schema.JI_ManufacturerIndicator,
			JobComInvoiceLine.Schema.ManufacturerDocOrgPK,
			JobComInvoiceLine.Schema.ManufacturerDocAddressPK,
			JobComInvoiceLine.Schema.ManufacturerName,
			JobComInvoiceLine.Schema.JI_CountryOfOrigin,
			JobComInvoiceLine.Schema.NaladiHs,
			JobComInvoiceLine.Schema.DutyTaxRegime,
			JobComInvoiceLine.Schema.DutyLegalBase,
			JobComInvoiceLine.Schema.DrawbackModality,
		};
	}
}
