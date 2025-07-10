using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.CH.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CH.GUI.Testing;

class InvoiceLineDetailsUserControlTest : TestCaseWithFactory
{
	public void TestBindingSourceDataSourceType()
	{
		using (var control = new InvoiceLineDetailsUserControl())
		{
			AssertEquals(typeof(JobComInvoiceLine), control.BindingSource.DataSourceType);
		}
	}

	public void TestControls()
	{
		using (var control = new InvoiceLineDetailsUserControl())
		{
			CombineAssertions(() =>
			{
				AssertType<ZCheckBox>(nameof(control.NetDutyCheckBox), control.NetDutyCheckBox);
				AssertType<ZCalcDropEdit>(nameof(control.CustomNetWeightCalcDropEdit), control.CustomNetWeightCalcDropEdit);
				AssertType<TareSupplementUserControl>(nameof(control.TareSupplementUserControl), control.TareSupplementUserControl);
				AssertType<ZCalcDropEdit>(nameof(control.CalculatedGrossMassCalcDropEdit), control.CalculatedGrossMassCalcDropEdit);
				AssertType<VATCodeUserControl>(nameof(control.VATCodeUserControl), control.VATCodeUserControl);
				AssertType<ZDropEdit>(nameof(control.PermitObligationDropEdit), control.PermitObligationDropEdit);
				AssertType<ZDropEdit>(nameof(control.NonCustomsLawObligationDropEdit), control.NonCustomsLawObligationDropEdit);
				AssertType<ZDropEdit>(nameof(control.StorageTypeDropEdit), control.StorageTypeDropEdit);
				AssertType<ZCheckBox>(nameof(control.GrossMassConfirmationCheckBox), control.GrossMassConfirmationCheckBox);
				AssertType<ZCheckBox>(nameof(control.NetMassConfirmationCheckBox), control.NetMassConfirmationCheckBox);
				AssertType<ZCheckBox>(nameof(control.AdditionalUnitConfirmationCheckBox), control.AdditionalUnitConfirmationCheckBox);
				AssertType<ZCheckBox>(nameof(control.StatisticalValueConfirmationCheckBox), control.StatisticalValueConfirmationCheckBox);
				AssertType<ZCheckBox>(nameof(control.VATValueConfirmationCheckBox), control.VATValueConfirmationCheckBox);
				AssertType<SeparatorUserControl>(nameof(control.ConfirmationCodesSeparatorUserControl), control.ConfirmationCodesSeparatorUserControl);
				AssertType<ZCheckBox>(nameof(control.NonCommercialGoodsCheckBox), control.NonCommercialGoodsCheckBox);
				AssertType<ZCheckBox>(nameof(control.GoodsReturnedCheckBox), control.GoodsReturnedCheckBox);
				AssertType<ZCodeFindBox>(nameof(control.CusCodeFindBox), control.CusCodeFindBox);
				AssertType<ZTextBox>(nameof(control.RateFormulaDescriptionTextBox), control.RateFormulaDescriptionTextBox);
				AssertType<ZCheckBox>(nameof(control.RateOverrideCheckBox), control.RateOverrideCheckBox);
				AssertType<ZCalcEdit>(nameof(control.OverriddenRateCalcEdit), control.OverriddenRateCalcEdit);
				AssertType<UNDGCodesUserControl>(nameof(control.UNDGCodesUserControl), control.UNDGCodesUserControl);
			});
		}
	}
}
