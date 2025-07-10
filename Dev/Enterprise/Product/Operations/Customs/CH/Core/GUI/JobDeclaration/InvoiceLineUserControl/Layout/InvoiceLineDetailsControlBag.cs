using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CH.GUI;

public sealed class InvoiceLineDetailsControlBag : ControlBag
{
	InvoiceLineDetailsControlBag()
	{
		NetDutyCheckBox = RegisterControl(nameof(InvoiceLineDetailsUserControl.NetDutyCheckBox));
		CustomNetWeightCalcDropEdit = RegisterControl(nameof(InvoiceLineDetailsUserControl.CustomNetWeightCalcDropEdit));
		TareSupplementUserControl = RegisterControl(nameof(InvoiceLineDetailsUserControl.TareSupplementUserControl));
		CalculatedGrossMassCalcDropEdit = RegisterControl(nameof(InvoiceLineDetailsUserControl.CalculatedGrossMassCalcDropEdit));
		VATCodeUserControl = RegisterControl(nameof(InvoiceLineDetailsUserControl.VATCodeUserControl));
		VATValueConfirmationCheckBox = RegisterControl(nameof(InvoiceLineDetailsUserControl.VATValueConfirmationCheckBox));
		PermitObligationDropEdit = RegisterControl(nameof(InvoiceLineDetailsUserControl.PermitObligationDropEdit));
		NonCustomsLawObligationDropEdit = RegisterControl(nameof(InvoiceLineDetailsUserControl.NonCustomsLawObligationDropEdit));
		StorageTypeDropEdit = RegisterControl(nameof(InvoiceLineDetailsUserControl.StorageTypeDropEdit));
		GrossMassConfirmationCheckBox = RegisterControl(nameof(InvoiceLineDetailsUserControl.GrossMassConfirmationCheckBox));
		NetMassConfirmationCheckBox = RegisterControl(nameof(InvoiceLineDetailsUserControl.NetMassConfirmationCheckBox));
		AdditionalUnitConfirmationCheckBox = RegisterControl(nameof(InvoiceLineDetailsUserControl.AdditionalUnitConfirmationCheckBox));
		StatisticalValueConfirmationCheckBox = RegisterControl(nameof(InvoiceLineDetailsUserControl.StatisticalValueConfirmationCheckBox));
		ConfirmationCodesSeparatorUserControl = RegisterControl(nameof(InvoiceLineDetailsUserControl.ConfirmationCodesSeparatorUserControl));
		NonCommercialGoodsCheckBox = RegisterControl(nameof(InvoiceLineDetailsUserControl.NonCommercialGoodsCheckBox));
		DutyRateUserControl = RegisterControl(nameof(InvoiceLineDetailsUserControl.DutyRateUserControl));
		RefundTypeDropEdit = RegisterControl(nameof(InvoiceLineDetailsUserControl.RefundTypeDropEdit));
		RefundReferenceNumberTextBox = RegisterControl(nameof(InvoiceLineDetailsUserControl.RefundReferenceNumberTextBox));
		RefundGoodsItemNumberIntEdit = RegisterControl(nameof(InvoiceLineDetailsUserControl.RefundGoodsItemNumberIntEdit));
		RefundReasonTextBox = RegisterControl(nameof(InvoiceLineDetailsUserControl.RefundReasonTextBox));
		CusCodeFindBox = RegisterControl(nameof(InvoiceLineDetailsUserControl.CusCodeFindBox));
		GoodsReturnedCheckBox = RegisterControl(nameof(InvoiceLineDetailsUserControl.GoodsReturnedCheckBox));
		RateFormulaDescriptionTextBox = RegisterControl(nameof(InvoiceLineDetailsUserControl.RateFormulaDescriptionTextBox));
		RateOverrideCheckBox = RegisterControl(nameof(InvoiceLineDetailsUserControl.RateOverrideCheckBox));
		OverriddenRateCalcEdit = RegisterControl(nameof(InvoiceLineDetailsUserControl.OverriddenRateCalcEdit));
		UNDGCodesUserControl = RegisterControl(nameof(InvoiceLineDetailsUserControl.UNDGCodesUserControl));
	}

	public static InvoiceLineDetailsControlBag Instance => instance ?? (instance = new InvoiceLineDetailsControlBag());

	[ThreadStatic]
	static InvoiceLineDetailsControlBag instance;

	protected override Control CreateTemplate() => new InvoiceLineDetailsUserControl();

	public ControlReference NetDutyCheckBox { get; }

	public ControlReference CustomNetWeightCalcDropEdit { get; }

	public ControlReference TareSupplementUserControl { get; }

	public ControlReference CalculatedGrossMassCalcDropEdit { get; }

	public ControlReference VATCodeUserControl { get; }

	public ControlReference VATValueConfirmationCheckBox { get; }

	public ControlReference PermitObligationDropEdit { get; }

	public ControlReference NonCustomsLawObligationDropEdit { get; }

	public ControlReference StorageTypeDropEdit { get; }

	public ControlReference GrossMassConfirmationCheckBox { get; }

	public ControlReference NetMassConfirmationCheckBox { get; }

	public ControlReference AdditionalUnitConfirmationCheckBox { get; }

	public ControlReference StatisticalValueConfirmationCheckBox { get; }

	public ControlReference ConfirmationCodesSeparatorUserControl { get; }

	public ControlReference NonCommercialGoodsCheckBox { get; }

	public ControlReference DutyRateUserControl { get; }

	public ControlReference RefundTypeDropEdit { get; }

	public ControlReference RefundReferenceNumberTextBox { get; }

	public ControlReference RefundGoodsItemNumberIntEdit { get; }

	public ControlReference RefundReasonTextBox { get; }

	public ControlReference CusCodeFindBox { get; }

	public ControlReference GoodsReturnedCheckBox { get; }

	public ControlReference RateFormulaDescriptionTextBox { get; }

	public ControlReference RateOverrideCheckBox { get; }

	public ControlReference OverriddenRateCalcEdit { get; }

	public ControlReference UNDGCodesUserControl { get; }
}
