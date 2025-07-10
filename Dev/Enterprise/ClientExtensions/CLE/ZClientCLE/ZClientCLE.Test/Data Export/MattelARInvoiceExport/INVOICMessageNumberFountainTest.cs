using CargoWise.Data;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Client.CLE.MattelARInvoiceExport
{
	public class INVOICMessageNumberFountainTest : TestCaseWithFactory
	{
		#region TestMessageNumber
		public void TestInterchangeNumber()
		{
			Db.Connection.BeginTransaction(); // need to be in a second transaction to hit the NumberFountain.
			try
			{
				string firstNumber = TestFountain.InterchangeNumber.GetNextFormatted(Factory);
				string nextNumber = TestFountain.InterchangeNumber.GetNextFormatted(Factory);
				AssertEquals("First MessageNo", "0000001", firstNumber);
				AssertEquals("Next MessageNo", "0000002", nextNumber);
			}
			finally
			{
				Db.Connection.RollbackTransaction(); // need to be in a second transaction to hit the NumberFountain.
			}
		}

		#endregion
		readonly INVOICMessageNumberFountain TestFountain = new INVOICMessageNumberFountain();
	}
}
