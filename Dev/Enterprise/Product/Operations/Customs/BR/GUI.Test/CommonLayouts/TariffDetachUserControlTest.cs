using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.BR.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.BR.GUI.Testing
{
	class TariffDetachUserControlTest : TestCaseWithFactory
	{
		public void TestDataSourceType()
		{
			using (var control = new TariffDetachUserControl())
			{
				AssertEquals("DataSourceType", typeof(TariffDetachCollection), control.DataSourceType);
			}
		}

		public void TestGroupBoxCaption()
		{
			using (var control = new TariffDetachUserControl())
			{
				AssertEquals("NveGroupBox caption must be Tariff Detach", "Tariff Detach", control.TariffDetachGroupBox.CaptionResourceString.Caption);
			}
		}

		public void TestNveGridType()
		{
			using (var control = new TariffDetachUserControl())
			{
				CombineAssertions(() =>
				{
					AssertType<ZButton>("TariffDetachButton must be ZGrid", control.TariffDetachButton);
					AssertType<ZTextBox>("TariffDetachTextBox must be ZGroupBox", control.TariffDetachTextBox);
					AssertType<ZGroupBox>("TariffDetachGroupBox must be ZGroupBox", control.TariffDetachGroupBox);
				});
			}
		}
	}
}
