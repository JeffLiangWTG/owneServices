using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.BR.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.BR.GUI.Testing
{
	class TaxDetailsUserControlTest : TestCaseWithFactory
	{
		public void TestDataSourceType()
		{
			using (var control = new TaxDetailsUserControl())
			{
				AssertEquals("DataSourceType", typeof(JobComInvoiceLine), control.DataSourceType);
			}
		}

		public void TestGroupBoxCaptions()
		{
			using (var control = new TaxDetailsUserControl())
			{
				CombineAssertions(() =>
				{
					AssertEquals("DutyGroupBox caption must be Duty", "Duty", control.DutyGroupBox.CaptionResourceString.Caption);
					AssertEquals("RegimeGroupBox caption must be Regime", "Regime", control.RegimeGroupBox.CaptionResourceString.Caption);
					AssertEquals("RateGroupBox caption must be Rate", "Rate", control.RateGroupBox.CaptionResourceString.Caption);
					AssertEquals("PisCofinsGroupBox caption must be PIS/COFINS", "PIS/COFINS", control.PisCofinsGroupBox.CaptionResourceString.Caption);
					AssertEquals("IpiGroupBox caption must be IPI", "IPI", control.IPIGroupBox.CaptionResourceString.Caption);
					AssertEquals("IPILegalBasisGroupBox caption must be Legal Basis of the Taxation Regime", "Legal Basis of the Taxation Regime", control.IPILegalBasisGroupBox.CaptionResourceString.Caption);
				});
			}
		}

		public void TestFieldsInForm()
		{
			using (var control = new TaxDetailsUserControl())
			{
				CombineAssertions(() =>
				{
					AssertType<ZDropEdit>("PisCofinsTaxRegimeDropEdit must be ZDropEdit", control.PisCofinsTaxRegimeDropEdit);
					AssertType<ZCheckBox>("DutyRateIsOverriddenCheckBox must be ZCheckBox", control.DutyRateIsOverriddenCheckBox);
					AssertType<ZCheckBox>("PisRateIsOverriddenCheckBox must be ZCheckBox", control.PisRateIsOverriddenCheckBox);
					AssertType<ZCalcEdit>("PisVigentRateValueCalcEdit must be ZCalcEdit", control.PisVigentRateValueCalcEdit);
					AssertType<ZCheckBox>("CofinsRateIsOverriddenCheckBox must be ZCheckBox", control.CofinsRateIsOverriddenCheckBox);
					AssertType<ZCalcEdit>("CofinsVigentRateValueCalcEdit must be ZCalcEdit", control.CofinsVigentRateValueCalcEdit);
					AssertType<ZCheckBox>("IPIRateIsOverriddenCheckBox must be ZCheckBox", control.IPIRateIsOverriddenCheckBox);
					AssertType<ZCalcEdit>("FTAMarginRateValueCalcEdit must be ZCalcEdit", control.FTAMarginRateValueCalcEdit);
					AssertType<ZCalcEdit>("FTADutyRateCalcEdit must be ZCalcEdit", control.FTADutyRateCalcEdit);
					AssertType<ZDropEdit>("PisCofinsLegalBaseDropEdit must be ZDropEdit", control.PisCofinsLegalBaseDropEdit);
					AssertType<ZCodeFindBox>("ComplementaryNoteCodeFindBox must be ZCodeFindBox", control.ComplementaryNoteCodeFindBox);
					AssertType<ZCalcEdit>("ReductionMarginRateCalcEdit must be ZCalcEdit", control.ReductionMarginRateCalcEdit);
					AssertType<ZCalcEdit>("ReductionDutyRateCalcEdit must be ZCalcEdit", control.ReductionDutyRateCalcEdit);
					AssertType<AdditionalTariffsUserControl>("AdditionalTariffsGridLayout must be AdditionalTariffsUserControl", control.AdditionalTariffsGridLayout);
				});
			}
		}
	}
}
