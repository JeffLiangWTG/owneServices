using NUnit.Framework;

namespace Enterprise.Client.OSP.Data_Import.Testing
{
	public class IFTMIN5MContainersOnGroupageDataRowTest : TestCase
	{
		public void TestProperties()
		{
			string testString = "CNG001HC40  INKU26361640029539             32325";
			IFTMIN5MContainersOnGroupageDataRow dataRow = new IFTMIN5MContainersOnGroupageDataRow(testString);
			AssertEquals(1, dataRow.ContainerProgr);
			AssertEquals("HC40", dataRow.TypeContainer);
			AssertEquals("INKU", dataRow.PlateContainer);
			AssertEquals(263616, dataRow.NumberContainer);
			AssertEquals(4, dataRow.CheckDigitContainer);
			AssertEquals("0029539", dataRow.Seal);
			AssertEquals("32325", dataRow.TruckPlateRrIDNumber);
		}
	}
}
