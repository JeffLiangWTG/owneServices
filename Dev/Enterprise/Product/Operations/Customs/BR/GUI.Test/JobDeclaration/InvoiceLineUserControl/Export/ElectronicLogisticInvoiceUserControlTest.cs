using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.BR.Business;

namespace Enterprise.Customs.BR.GUI.Testing
{
	class ElectronicLogisticInvoiceUserControlTest : TestCaseWithFactory
	{
		public void TestDataSourceType()
		{
			using (var control = new ElectronicLogisticInvoiceUserControl())
			{
				AssertEquals("DataSourceType", typeof(JobDeclaration), control.DataSourceType);
			}
		}
	}
}
