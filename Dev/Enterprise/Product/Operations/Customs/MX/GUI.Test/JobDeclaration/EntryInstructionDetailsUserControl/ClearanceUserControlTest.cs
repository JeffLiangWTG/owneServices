using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.MX.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.MX.GUI.Testing
{
	class ClearanceUserControlTest : TestCaseWithFactory
	{
		public void TestDataSourceType()
		{
			using (var control = new ClearanceUserControl())
			{
				AssertEquals("DataSourceType", typeof(CusEntryInstruction), control.DataSourceType);
			}
		}

		public void TestComponents()
		{
			using (var control = new ClearanceUserControl())
			{
				AssertType<ZGrid>("ClearenceGrid must be ZGrid", control.ClearenceGrid);
				AssertType<ZGroupBox>("ClearanceGroupBox must be ZGroupBox", control.ClearanceGroupBox);
			}
		}
	}
}
