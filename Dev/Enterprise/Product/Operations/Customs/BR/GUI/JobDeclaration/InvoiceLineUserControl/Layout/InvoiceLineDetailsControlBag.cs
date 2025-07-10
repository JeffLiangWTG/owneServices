using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.BR.GUI
{
	public sealed class InvoiceLineDetailsControlBag : ControlBag
	{
		InvoiceLineDetailsControlBag()
		{
			CPCGroupBox = RegisterControl(nameof(InvoiceLineDetailsUserControl.CPCGroupBox));
			ComplementaryDescriptionTextBox = RegisterControl(nameof(InvoiceLineDetailsUserControl.ComplementaryDescriptionTextBox));
			BRNFEItemNumberTextBox = RegisterControl(nameof(InvoiceLineDetailsUserControl.BRNFEItemNumberTextBox));
			BRNFENumberTextBox = RegisterControl(nameof(InvoiceLineDetailsUserControl.BRNFENumberTextBox));
			CargoPriorityDropEdit = RegisterControl(nameof(InvoiceLineDetailsUserControl.CargoPriorityDropEdit));
			CountryDestinationCodeFindBox = RegisterControl(nameof(InvoiceLineDetailsUserControl.CountryDestinationCodeFindBox));
			NFeLinePriceCalcEdit = RegisterControl(nameof(InvoiceLineDetailsUserControl.NFeLinePriceCalcEdit));
			NaladiNccaCodeFindBox = RegisterControl(nameof(InvoiceLineDetailsUserControl.NaladiNccaCodeFindBox));
			NaladiHsCodeFindBox = RegisterControl(nameof(InvoiceLineDetailsUserControl.NaladiHsCodeFindBox));
			ImportLicenseNumberTextBox = RegisterControl(nameof(InvoiceLineDetailsUserControl.ImportLicenseNumberTextBox));
			RequiresImportLicenseCheckBox = RegisterControl(nameof(InvoiceLineDetailsUserControl.RequiresImportLicenseCheckBox));
			GoodsConditionSeparatorUserControl = RegisterControl(nameof(InvoiceLineDetailsUserControl.GoodsConditionSeparatorUserControl));
			GoodsConditionOperationTypeDropEdit = RegisterControl(nameof(InvoiceLineDetailsUserControl.GoodsConditionOperationTypeDropEdit));
			UsedMaterialRegimeDropEdit = RegisterControl(nameof(InvoiceLineDetailsUserControl.UsedMaterialRegimeDropEdit));
			ManufacturerIndicatorDropEdit = RegisterControl(nameof(InvoiceLineDetailsUserControl.ManufacturerIndicatorDropEdit));
			ModelGoodsConditionTextBox = RegisterControl(nameof(InvoiceLineDetailsUserControl.ModelGoodsConditionTextBox));
			YearGoodsConditionTextBox = RegisterControl(nameof(InvoiceLineDetailsUserControl.YearGoodsConditionTextBox));
			SerialNumberGoodsConditionTextBox = RegisterControl(nameof(InvoiceLineDetailsUserControl.SerialNumberGoodsConditionTextBox));
			BrandGoodsConditionTextBox = RegisterControl(nameof(InvoiceLineDetailsUserControl.BrandGoodsConditionTextBox));
			DutyTaxRegimeSeparatorUserControl = RegisterControl(nameof(InvoiceLineDetailsUserControl.DutyTaxRegimeSeparatorUserControl));
			DutyTaxRegimeDropEdit = RegisterControl(nameof(InvoiceLineDetailsUserControl.DutyTaxRegimeDropEdit));
			DutyLegalBaseDropEdit = RegisterControl(nameof(InvoiceLineDetailsUserControl.DutyLegalBaseDropEdit));
			ImportLicenseFineSeparatorUserControl = RegisterControl(nameof(InvoiceLineDetailsUserControl.ImportLicenseFineSeparatorUserControl));
			ImportLicenseTypeDropEdit = RegisterControl(nameof(InvoiceLineDetailsUserControl.ImportLicenseTypeDropEdit));
			ImportLicenseAuthorizationDateTextBox = RegisterControl(nameof(InvoiceLineDetailsUserControl.ImportLicenseAuthorizationDateTextBox));
			ImportLicenseFeeTypeDropEdit = RegisterControl(nameof(InvoiceLineDetailsUserControl.ImportLicenseFeeTypeDropEdit));
			TariffAgreementSeparatorUserControl = RegisterControl(nameof(InvoiceLineDetailsUserControl.TariffAgreementSeparatorUserControl));
			TariffAgreementDropEdit = RegisterControl(nameof(InvoiceLineDetailsUserControl.TariffAgreementDropEdit));
			GoodsApplicationDropEdit = RegisterControl(nameof(InvoiceLineDetailsUserControl.GoodsApplicationDropEdit));
			GoodsConditionDropEdit = RegisterControl(nameof(InvoiceLineDetailsUserControl.GoodsConditionDropEdit));
			FullGoodsDescriptionTextBox = RegisterControl(nameof(InvoiceLineDetailsUserControl.FullGoodsDescriptionTextBox));
			EntryInstructionGuidDropEdit = RegisterControl(nameof(InvoiceLineDetailsUserControl.EntryInstructionGuidDropEdit));
		}

		public static InvoiceLineDetailsControlBag Instance => instance ?? (instance = new InvoiceLineDetailsControlBag());

		[ThreadStatic]
		static InvoiceLineDetailsControlBag instance;

		protected override Control CreateTemplate() => new InvoiceLineDetailsUserControl();

		public ControlReference CPCGroupBox { get; }
		public ControlReference ComplementaryDescriptionTextBox { get; }
		public ControlReference BRNFEItemNumberTextBox { get; }
		public ControlReference BRNFENumberTextBox { get; }
		public ControlReference CargoPriorityDropEdit { get; }
		public ControlReference CountryDestinationCodeFindBox { get; }
		public ControlReference NFeLinePriceCalcEdit { get; }
		public ControlReference NaladiNccaCodeFindBox { get; }
		public ControlReference NaladiHsCodeFindBox { get; }
		public ControlReference ImportLicenseNumberTextBox { get; }
		public ControlReference RequiresImportLicenseCheckBox { get; }
		public ControlReference GoodsConditionSeparatorUserControl { get; }
		public ControlReference GoodsConditionOperationTypeDropEdit { get; }
		public ControlReference UsedMaterialRegimeDropEdit { get; }
		public ControlReference ManufacturerIndicatorDropEdit { get; }
		public ControlReference ModelGoodsConditionTextBox { get; }
		public ControlReference YearGoodsConditionTextBox { get; }
		public ControlReference SerialNumberGoodsConditionTextBox { get; }
		public ControlReference BrandGoodsConditionTextBox { get; }
		public ControlReference DutyTaxRegimeSeparatorUserControl { get; }
		public ControlReference DutyTaxRegimeDropEdit { get; }
		public ControlReference DutyLegalBaseDropEdit { get; }
		public ControlReference ImportLicenseFineSeparatorUserControl { get; }
		public ControlReference ImportLicenseTypeDropEdit { get; }
		public ControlReference ImportLicenseAuthorizationDateTextBox { get; }
		public ControlReference ImportLicenseFeeTypeDropEdit { get; }
		public ControlReference TariffAgreementSeparatorUserControl { get; }
		public ControlReference TariffAgreementDropEdit { get; }
		public ControlReference GoodsApplicationDropEdit { get; }
		public ControlReference GoodsConditionDropEdit { get; }
		public ControlReference FullGoodsDescriptionTextBox { get; }
		public ControlReference EntryInstructionGuidDropEdit { get; }
	}
}
