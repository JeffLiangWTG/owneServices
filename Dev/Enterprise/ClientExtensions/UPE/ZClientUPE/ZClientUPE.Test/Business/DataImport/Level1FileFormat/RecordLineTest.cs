using NUnit.Framework;

namespace Enterprise.Client.UPE.Business.DataImport.Level1FileFormat.Testing
{
	internal class RecordLineTest : TestCase
	{
		public void TestProperties()
		{
			RecordLine line = new RecordLine("US2795AU963900062440610000001   L09576XNZGX7100000SomeOtherInformation");

			AssertEquals("ShipmentNumber", "09576XNZGX7", line.ShipmentNumber);
		}
	}
}
