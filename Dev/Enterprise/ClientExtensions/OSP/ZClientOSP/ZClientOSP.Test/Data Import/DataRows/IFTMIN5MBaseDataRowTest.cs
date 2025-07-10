using NUnit.Framework;

namespace Enterprise.Client.OSP.Data_Import.Testing
{
	public class IFTMIN5MBaseDataRowTest : TestCase
	{
		public void TestSegmentCode()
		{
			IFTMIN5MBaseDataRow dataRow = new IFTMIN5MBaseDataRow("IBMG Test String");
			AssertEquals("IBM", dataRow.SegmentCode);
		}
	}
}
