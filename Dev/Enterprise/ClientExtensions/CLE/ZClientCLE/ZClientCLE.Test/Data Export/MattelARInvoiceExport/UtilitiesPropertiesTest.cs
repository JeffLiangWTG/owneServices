using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Client.CLE.MattelARInvoiceExport.Testing
{
	public class UtilitiesPropertiesTest : TransactionedTestCase
	{
		[TestDate(2005, 11, 7, 9, 55, 23)]
		public void TestGetFileName()
		{
			Utilities utilities = new Utilities();
			ZString expectedFileName = "INV_200511070955_S10101010.EDI";
			AssertEquals("File name should be " + expectedFileName, expectedFileName, utilities.GetFileName("S10101010"));
		}
	}
}
