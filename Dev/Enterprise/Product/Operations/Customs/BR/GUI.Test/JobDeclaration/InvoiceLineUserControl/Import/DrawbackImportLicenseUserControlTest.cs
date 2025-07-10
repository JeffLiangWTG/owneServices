using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.BR.Business;

namespace Enterprise.Customs.BR.GUI.Testing
{
	public class DrawbackImportLicenseUserControlTest : TestCaseWithFactory
	{
		public void TestDataSourceType()
		{
			using (var control = new DrawbackImportLicenseUserControl())
			{
				AssertEquals("DataSourceType", typeof(JobComInvoiceLine), control.DataSourceType);
			}
		}
	}
}
