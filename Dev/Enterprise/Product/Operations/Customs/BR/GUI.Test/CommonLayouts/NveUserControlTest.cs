using Enterprise.Customs.BR.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.BR.GUI.Testing
{
	class NveUserControlTest : TestCase
	{
		public void TestDataSourceType()
		{
			using (var control = new NveUserControl())
			{
				AssertEquals("DataSourceType", typeof(NveCusCodeDataCollection), control.DataSourceType);
			}
		}

		public void TestGroupBoxCaption()
		{
			using (var control = new NveUserControl())
			{
				AssertEquals("NveGroupBox caption must be NVE", "NVE", control.NveGroupBox.CaptionResourceString.Caption);
			}
		}

		public void TestNveGridType()
		{
			using (var control = new NveUserControl())
			{
				AssertNotNull(control.NveGrid);
				CombineAssertions(() =>
				{
					AssertType<ZGrid>("NveGrid must be ZGrid", control.NveGrid);
					AssertType<ZGroupBox>("NveGrid must be ZGroupBox", control.NveGroupBox);
				});
			}
		}
	}
}
