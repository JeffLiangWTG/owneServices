using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI
{
	public sealed class InvoiceLineDetailsControlBag : ControlBag
	{
		public static InvoiceLineDetailsControlBag Instance => invoiceLineDetailsControlBag.Value;

		InvoiceLineDetailsControlBag()
		{
			NationalAdditionalCode1DropEdit = RegisterControl(nameof(InvoiceLineDetailsUserControl.NationalAdditionalCode1DropEdit));
			NationalAdditionalCode2DropEdit = RegisterControl(nameof(InvoiceLineDetailsUserControl.NationalAdditionalCode2DropEdit));
			NationalAdditionalCodesUserControl = RegisterControl(nameof(InvoiceLineDetailsUserControl.NationalAdditionalCodesUserControl));
			FormattedProcedureCodeFindBox = RegisterControl(nameof(InvoiceLineDetailsUserControl.FormattedProcedureCodeFindBox));
			UnformattedProcedureCodeFindBox = RegisterControl(nameof(InvoiceLineDetailsUserControl.UnformattedProcedureCodeFindBox));
			DestinationCodeFindBox = RegisterControl(nameof(InvoiceLineDetailsUserControl.DestinationCodeFindBox));
			DispatchCodeFindBox = RegisterControl(nameof(InvoiceLineDetailsUserControl.DispatchCodeFindBox));
			DestinationUsingZZRefCusCodeListCodeFindBox = RegisterControl(nameof(InvoiceLineDetailsUserControl.DestinationUsingZZRefCusCodeListCodeFindBox));
			SupplementaryCode1DropEdit = RegisterControl(nameof(InvoiceLineDetailsUserControl.SupplementaryCode1DropEdit));
			SupplementaryCode2DropEdit = RegisterControl(nameof(InvoiceLineDetailsUserControl.SupplementaryCode2DropEdit));
			AdditionalSupplementaryCodesUserControl = RegisterControl(nameof(InvoiceLineDetailsUserControl.AdditionalSupplementaryCodesUserControl));
			CountryOfSupplyCodeFindBox = RegisterControl(nameof(InvoiceLineDetailsUserControl.CountryOfSupplyCodeFindBox));
			AdditionalProcedureCodesUserControl = RegisterControl(nameof(InvoiceLineDetailsUserControl.AdditionalProcedureCodesUserControl));
			PreferenceCodeDropEdit = RegisterControl(nameof(InvoiceLineDetailsUserControl.PreferenceCodeDropEdit));
			QuotaDropEdit = RegisterControl(nameof(InvoiceLineDetailsUserControl.QuotaDropEdit));
			SecondQuotaDropEdit = RegisterControl(nameof(InvoiceLineDetailsUserControl.SecondQuotaDropEdit));
			AdditionalSupplementaryCodesAndGDMUserControl = RegisterControl(nameof(InvoiceLineDetailsUserControl.AdditionalSupplementaryCodesAndGDMUserControl));
			CusNumberCodeFindBox = RegisterControl(nameof(InvoiceLineDetailsUserControl.CusNumberCodeFindBox));
			QuotaWithCheckLinkUserControl = RegisterControl(nameof(InvoiceLineDetailsUserControl.QuotaWithCheckLinkUserControl));
			ValuationAdjustmentPercentageCalcEdit = RegisterControl(nameof(InvoiceLineDetailsUserControl.ValuationAdjustmentPercentageCalcEdit));
			TransactionNatureDropEdit = RegisterControl(nameof(InvoiceLineDetailsUserControl.TransactionNatureDropEdit));
			UsedGoodsCodeDropEdit = RegisterControl(nameof(InvoiceLineDetailsUserControl.UsedGoodsCodeDropEdit));
			ReturnToOriginCheckBox = RegisterControl(nameof(InvoiceLineDetailsUserControl.ReturnToOriginCheckBox));
			SecondaryTreatedProductCheckBox = RegisterControl(nameof(InvoiceLineDetailsUserControl.SecondaryTreatedProductCheckBox));
			ReturningGoodsReasonCodeDropEdit = RegisterControl(nameof(InvoiceLineDetailsUserControl.ReturningGoodsReasonCodeDropEdit));
			ReturningGoodsReasonDetailTextBox = RegisterControl(nameof(InvoiceLineDetailsUserControl.ReturningGoodsReasonDetailTextBox));
			ExportUnionPackCodeFindBox = RegisterControl(nameof(InvoiceLineDetailsUserControl.ExportUnionPackCodeFindBox));
			ExportUnionThreadCodeFindBox = RegisterControl(nameof(InvoiceLineDetailsUserControl.ExportUnionThreadCodeFindBox));
			ExportUnionProductionYearCalcEdit = RegisterControl(nameof(InvoiceLineDetailsUserControl.ExportUnionProductionYearCalcEdit));
			ExportUnionDeferredInstallmentTextBox = RegisterControl(nameof(InvoiceLineDetailsUserControl.ExportUnionDeferredInstallmentTextBox));
			ExportUnionEcologicalCheckBox = RegisterControl(nameof(InvoiceLineDetailsUserControl.ExportUnionEcologicalCheckBox));
			InwardProcessingLicenseLineNumberTextBox = RegisterControl(nameof(InvoiceLineDetailsUserControl.InwardProcessingLicenseLineNumberTextBox));
			ProcessingDescriptionLongTextControl = RegisterControl(nameof(InvoiceLineDetailsUserControl.ProcessingDescriptionLongTextControl));
			SupplementaryCode1AndGDMUserControl = RegisterControl(nameof(InvoiceLineDetailsUserControl.SupplementaryCode1AndGDMUserControl));
			EntryExitPurposeCodeDropEdit = RegisterControl(nameof(InvoiceLineDetailsUserControl.EntryExitPurposeCodeDropEdit));
			EntryExitPurposeDetailTextBox = RegisterControl(nameof(InvoiceLineDetailsUserControl.EntryExitPurposeDetailTextBox));
			BorderTradeStateCodeFindBox = RegisterControl(nameof(InvoiceLineDetailsUserControl.BorderTradeStateCodeFindBox));
			ExportUnionAdditionalTariffCodeFindBox = RegisterControl(nameof(InvoiceLineDetailsUserControl.ExportUnionAdditionalTariffCodeFindBox));
			ExcessStockCheckBox = RegisterControl(nameof(InvoiceLineDetailsUserControl.ExcessStockCheckBox));
			CommercialPaymentCodeDropEdit = RegisterControl(nameof(InvoiceLineDetailsUserControl.CommercialPaymentCodeDropEdit));
			ValuationCodeFindBox = RegisterControl(nameof(InvoiceLineDetailsUserControl.ValuationCodeFindBox));
			RegionOfDestinationDropEdit = RegisterControl(nameof(InvoiceLineDetailsUserControl.RegionOfDestinationDropEdit));
			PriceTypeDropEdit = RegisterControl(nameof(InvoiceLineDetailsUserControl.PriceTypeDropEdit));
		}

		public ControlReference NationalAdditionalCode1DropEdit { get; }
		public ControlReference NationalAdditionalCode2DropEdit { get; }
		public ControlReference NationalAdditionalCodesUserControl { get; }
		public ControlReference FormattedProcedureCodeFindBox { get; }
		public ControlReference UnformattedProcedureCodeFindBox { get; }
		public ControlReference DestinationCodeFindBox { get; }
		public ControlReference DispatchCodeFindBox { get; }
		public ControlReference DestinationUsingZZRefCusCodeListCodeFindBox { get; }
		public ControlReference SupplementaryCode1DropEdit { get; }
		public ControlReference SupplementaryCode2DropEdit { get; }
		public ControlReference AdditionalSupplementaryCodesUserControl { get; }
		public ControlReference CountryOfSupplyCodeFindBox { get; }
		public ControlReference AdditionalProcedureCodesUserControl { get; }
		public ControlReference PreferenceCodeDropEdit { get; }
		public ControlReference QuotaDropEdit { get; }
		public ControlReference SecondQuotaDropEdit { get; }
		public ControlReference AdditionalSupplementaryCodesAndGDMUserControl { get; }
		public ControlReference CusNumberCodeFindBox { get; }
		public ControlReference QuotaWithCheckLinkUserControl { get; }
		public ControlReference ValuationAdjustmentPercentageCalcEdit { get; }
		public ControlReference TransactionNatureDropEdit { get; }
		public ControlReference UsedGoodsCodeDropEdit { get; }
		public ControlReference ReturnToOriginCheckBox { get; }
		public ControlReference SecondaryTreatedProductCheckBox { get; }
		public ControlReference ReturningGoodsReasonCodeDropEdit { get; }
		public ControlReference ReturningGoodsReasonDetailTextBox { get; }
		public ControlReference ExportUnionPackCodeFindBox { get; }
		public ControlReference ExportUnionThreadCodeFindBox { get; }
		public ControlReference ExportUnionProductionYearCalcEdit { get; }
		public ControlReference ExportUnionDeferredInstallmentTextBox { get; }
		public ControlReference ExportUnionEcologicalCheckBox { get; }
		public ControlReference InwardProcessingLicenseLineNumberTextBox { get; }
		public ControlReference EntryExitPurposeCodeDropEdit { get; }
		public ControlReference EntryExitPurposeDetailTextBox { get; }
		public ControlReference ProcessingDescriptionLongTextControl { get; }
		public ControlReference SupplementaryCode1AndGDMUserControl { get; }
		public ControlReference BorderTradeStateCodeFindBox { get; }
		public ControlReference ExportUnionAdditionalTariffCodeFindBox { get; }
		public ControlReference ExcessStockCheckBox { get; }
		public ControlReference CommercialPaymentCodeDropEdit { get; }
		public ControlReference ValuationCodeFindBox { get; }
		public ControlReference RegionOfDestinationDropEdit { get; }
		public ControlReference PriceTypeDropEdit { get; }

		protected override Control CreateTemplate() => new InvoiceLineDetailsUserControl();

		[WTG.StaticAnalysis.Annotation.ThreadSafe]
		static readonly Lazy<InvoiceLineDetailsControlBag> invoiceLineDetailsControlBag = new Lazy<InvoiceLineDetailsControlBag>(() => new InvoiceLineDetailsControlBag());
	}
}
