using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.BR.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.BR.GUI.Testing
{
	class SiscomexUsageFeeUserControlTest : TestCaseWithFactory
	{
		public void TestDataSourceType()
		{
			using (var control = new SiscomexUsageFeeUserControlForTesting())
			{
				AssertEquals("DataSourceType", typeof(SiscomexUsageFeeCollection), control.DataSourceType);
			}
		}

		public void TestComponents()
		{
			using (var userControl = new SiscomexUsageFeeUserControlForTesting())
			{
				CombineAssertions(() =>
				{
					AssertType<ZGrid>("SiscomexUsageFeeGrid should be", userControl.SiscomexUsageFeeGrid_Exposed);
					AssertType<ZGroupBox>("SiscomexUsageFeeGroupBox should be", userControl.SiscomexUsageFeeGroupBox_Exposed);
				});
			}
		}

		class SiscomexUsageFeeUserControlForTesting : SiscomexUsageFeeUserControl
		{
			public ZGrid SiscomexUsageFeeGrid_Exposed => SiscomexUsageFeeGrid;
			public ZGroupBox SiscomexUsageFeeGroupBox_Exposed => SiscomexUsageFeeGroupBox;
		}
	}
}
