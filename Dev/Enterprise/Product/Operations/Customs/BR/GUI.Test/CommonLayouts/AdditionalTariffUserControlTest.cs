using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.BR.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.BR.GUI.Testing
{
	class AdditionalTariffUserControlTest : TestCaseWithFactory
	{
		public void TestDataSourceType()
		{
			using (var control = new AdditionalTariffsUserControl())
			{
				AssertEquals("DataSourceType", typeof(AdditionalTariffCollection), control.DataSourceType);
			}
		}

		public void TestGroupBoxCaption()
		{
			using (var control = new AdditionalTariffsUserControl())
			{
				AssertEquals("AdditionalTariffGroupBox caption must be Additional Tariffs", "Additional Tariffs", control.AdditionalTariffsGroupBox.CaptionResourceString.Caption);
			}
		}

		public void TestAdditionalTariffGridType()
		{
			using (var control = new AdditionalTariffsUserControl())
			{
				AssertNotNull(control.AdditionalTariffsGrid);
				CombineAssertions(() =>
				{
					AssertType<ZGrid>("AdditionalTariffGrid must be ZGrid", control.AdditionalTariffsGrid);
					AssertType<ZGroupBox>("AdditionalTariffGrid must be ZGroupBox", control.AdditionalTariffsGroupBox);
				});
			}
		}
	}
}
