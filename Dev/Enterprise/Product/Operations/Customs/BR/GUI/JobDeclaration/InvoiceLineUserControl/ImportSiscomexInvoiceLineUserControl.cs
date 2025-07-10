using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.BR.Business;
using Enterprise.Customs.Common.BR;
using Enterprise.ZArchitecture.GUI;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.BR.GUI
{
	public partial class ImportSiscomexInvoiceLineUserControl : BaseInvoiceLineUserControl
	{
		public ImportSiscomexInvoiceLineUserControl()
		{
			InitializeComponent();
			CustomsInvoiceLinesBoundGrid.ColumnLayoutContext = BRJobMessageTypeList.Codes.ImportSiscomex;
		}

		public static class Schema
		{
			public const string ImportLicenseReference = "AttachedImportLicenseLine+Declaration+JE_DeclarationReference";
			public const string ImportLicenseLineNumber = "AttachedImportLicenseLine+JI_LineNo";
		}

		protected override IPanelLayoutProvider GetNewInvoiceLineDetailsPanelLayout() => new ImportInvoiceLineDetailsLayout();

		protected override ZBool DynamicLayoutApplied => true;

		protected override void AddNewColumnForCustomsInvoiceLinesBoundGrid()
		{
			CreateNewTextBoxColumn(JobComInvoiceLine.Schema.JI_Calc_MergedLineNumber, 80);
			CreateNewCodeFindBoxColumn(JobComInvoiceLine.Schema.NaladiNcca, 90, Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.Universal.RefCusTariff);
			CreateNewCodeFindBoxColumn(JobComInvoiceLine.Schema.NaladiHs, 90, Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.Universal.RefCusTariff);
			CreateNewTextBoxColumn(JobComInvoiceLine.Schema.ImportLicenseNumber, 120);
			CreateNewCheckBoxColumn(JobComInvoiceLine.Schema.JI_RequiresImportLicense, 80, defaultColumn: false);
			CreateNewDropEditColumn(JobComInvoiceLine.Schema.JI_ManufacturerIndicator, 120);
			CreateNewTextBoxColumn(JobComInvoiceLine.Schema.ManufacturerName, 120, defaultColumn: true);
			CreateNewDropEditColumn(JobComInvoiceLine.Schema.JI_GoodsApplication, 120);
			CreateManufacturerAddressColumns();
			CreateNewTextBoxColumn(Schema.ImportLicenseReference, 120, Res.GetData("7ecf4032-4596-404a-9176-a4bf0c424e2e", "Import License Ref.", "Import License Reference"), readOnly: true);
			CreateNewTextBoxColumn(Schema.ImportLicenseLineNumber, 120, Res.GetData("13f5718c-1e67-462b-ade1-f8dd10be38e0", "Import License Line No."), readOnly: true);
			CreateNewDropEditColumn(JobComInvoiceLine.Schema.DutyTaxRegime, 120);
			CreateNewDropEditColumn(JobComInvoiceLine.Schema.DutyLegalBase, 120);
			CreateNewDropEditColumn(JobComInvoiceLine.Schema.IPITaxRegime, 120);
			CreateNewDropEditColumn(JobComInvoiceLine.Schema.JI_ComplementaryNote, 150);
			CreateNewDropEditColumn(JobComInvoiceLine.Schema.PisCofinsTaxRegime, 150);
			CreateNewDropEditColumn(JobComInvoiceLine.Schema.PisCofinsLegalBase, 150);
			CreateNewDropEditColumn(JobComInvoiceLine.Schema.ICMSTaxRegime, 120);
			CreateNewDropEditColumn(JobComInvoiceLine.Schema.ICMSLegalBase, 120);
			CreateNewCalcEditColumn(JobComInvoiceLine.Schema.JI_ICMSRate, 90);
			CreateNewDropEditColumn(JobComInvoiceLine.Schema.JI_GoodsCondition, 120);

			var groupFMMBenefit = Res.GetData("0f55d2c8-2521-425c-b2d5-76243f615eda", "FMM Benefit");
			CreateNewDropEditColumn(JobComInvoiceLine.Schema.FMMBenefit, 80, groupName: groupFMMBenefit);
			CreateNewTextBoxColumn(JobComInvoiceLine.Schema.FMMBenefitDescription, 130, groupName: groupFMMBenefit).CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;

			CreateNewDropEditColumn(JobComInvoiceLine.Schema.MercosulForeignDeclarationType, 90, defaultColumn: false);
			CreateNewCalcEditColumn(JobComInvoiceLine.Schema.JI_ICMSBaseValueReductionPercentage, 120, defaultColumn: false);
			CreateNewCalcEditColumn(JobComInvoiceLine.Schema.JI_ICMSTotalAmountReductionPercentage, 120, defaultColumn: false);
			CreateNewDropEditColumn(JobComInvoiceLine.Schema.JI_ICMSFormula, 120, defaultColumn: false);

			var groupIPILegalBasis = Res.GetData("81557e30-5c63-44b3-9377-ae6b4ce11960", "IPI Legal Basis");
			CreateNewDropEditColumn(JobComInvoiceLine.Schema.IPITaxBenefitLegalActType, 120, defaultColumn: false, groupName: groupIPILegalBasis);
			CreateNewDropEditColumn(JobComInvoiceLine.Schema.IPITaxBenefitLegalActIssuingBody, 120, defaultColumn: false, groupName: groupIPILegalBasis);
			CreateNewTextBoxColumn(JobComInvoiceLine.Schema.IPITaxBenefitLegalActNumber, 80, defaultColumn: false, groupName: groupIPILegalBasis);
			CreateNewTextBoxColumn(JobComInvoiceLine.Schema.IPITaxBenefitLegalActYear, 80, defaultColumn: false, groupName: groupIPILegalBasis);

			var groupImportLicense = Res.GetData("6F9A1D11-4BE1-4FA0-A04E-AC90F171BE19", "Import License Fine");
			CreateNewDropEditColumn(JobComInvoiceLine.Schema.ImportLicenseType, 150, defaultColumn: false, groupName: groupImportLicense);
			CreateNewDateEditColumn(JobComInvoiceLine.Schema.ImportLicenseAuthorizationDate, 180, defaultColumn: false, groupName: groupImportLicense);
			CreateNewDropEditColumn(JobComInvoiceLine.Schema.ImportLicenseFeeType, 150, defaultColumn: false, groupName: groupImportLicense);

			CreateNewCalcEditColumn(JobComInvoiceLine.Schema.ICMSFCPRateValue, 90, defaultColumn: false);
		}

		protected override IEnumerable<string> GetDefaultColumnsInOrderCore() => new List<string>
		{
			JobComInvoiceLine.Schema.JI_LineNo,
			JobComInvoiceLine.Schema.JI_Calc_Invoice,
			JobComInvoiceLine.Schema.JI_PartNo,
			JobComInvoiceLine.Schema.JI_CC,
			JobComInvoiceLine.Schema.JI_Tariff,
			JobComInvoiceLine.Schema.FMMBenefit,
			JobComInvoiceLine.Schema.FMMBenefitDescription,
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
			JobComInvoiceLine.Schema.ManufacturerOrgPK,
			JobComInvoiceLine.Schema.JI_OA_ManufacturerAddress,
			JobComInvoiceLine.Schema.ManufacturerName,
			JobComInvoiceLine.Schema.JI_CountryOfOrigin,
			Schema.ImportLicenseReference,
			Schema.ImportLicenseLineNumber,
			JobComInvoiceLine.Schema.ImportLicenseNumber,
			JobComInvoiceLine.Schema.NaladiHs,
			JobComInvoiceLine.Schema.NaladiNcca,
			JobComInvoiceLine.Schema.JI_GoodsApplication,
			JobComInvoiceLine.Schema.JI_GoodsCondition,
			JobComInvoiceLine.Schema.DutyTaxRegime,
			JobComInvoiceLine.Schema.DutyLegalBase,
			JobComInvoiceLine.Schema.IPITaxRegime,
			JobComInvoiceLine.Schema.JI_ComplementaryNote,
			JobComInvoiceLine.Schema.PisCofinsTaxRegime,
			JobComInvoiceLine.Schema.PisCofinsLegalBase,
			JobComInvoiceLine.Schema.ICMSTaxRegime,
			JobComInvoiceLine.Schema.ICMSLegalBase,
			JobComInvoiceLine.Schema.JI_ICMSRate
		};
	}
}
