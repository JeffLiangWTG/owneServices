using CargoWise.EntityFramework.Testing;

namespace Enterprise.Client.CLE.Testing
{
	public class ContainerDatesFlatFileDataRowTest : TestCaseWithFactory
	{
		public void TestProperties()
		{
			ContainerDatesFlatFileDataRow row = new ContainerDatesFlatFileDataRow("09-03-2007,,ContainerNumber,,10-03-2007,,,,CLE");
			AssertEquals("row.ContainerNumber", "ContainerNumber", row.ContainerNumber);
			AssertEquals("row.DeliveryDateString", "09-03-2007", row.DeliveryDateString);
			AssertEquals("row.DeHireDateString", "10-03-2007", row.DeHireDateString);
			AssertEquals("row.CLEReference", "CLE", row.CLEReference);
		}
	}
}
