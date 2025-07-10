using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.BR.Business;

namespace Enterprise.Customs.BR.GUI.Testing
{
	public class AdditionalDetailsUserControlTest : TestCaseWithFactory
	{
		public void TestDataSourceType()
		{
			using (var control = new AdditionalDetailsUserControl())
			{
				AssertEquals("DataSourceType", typeof(JobComInvoiceLine), control.DataSourceType);
			}
		}
	}
}
