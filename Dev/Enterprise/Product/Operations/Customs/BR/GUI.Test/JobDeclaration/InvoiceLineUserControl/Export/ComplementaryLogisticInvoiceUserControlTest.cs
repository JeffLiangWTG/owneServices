using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.BR.Business;

namespace Enterprise.Customs.BR.GUI.Testing
{
	class ComplementaryLogisticInvoiceUserControlTest : TestCaseWithFactory
	{
		public void TestDataSourceType()
		{
			using (var control = new ComplementaryLogisticInvoiceUserControl())
			{
				AssertEquals("DataSourceType", typeof(JobDeclaration), control.DataSourceType);
			}
		}
	}
}
